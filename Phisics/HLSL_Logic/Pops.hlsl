#pragma region Data

struct AABB 
{
    float3 minPos;
    float3 maxPos;
};
struct Object
{
    AABB bounds;
    uint objId_InGlobalBuffer;
};

struct GlobalObj 
{
    float4x4    transform;
    float3      massCenter; 
    float3      minAABB; 
    float3      maxAABB;
    float3      rotation;   
    float3      position;
    float3      velocity;   
    float3      angularVelocity;

    uint     colliderType;
    uint     objBufferIndex;                                       
    uint     additionalBufferIndex;                                    
            
    float    mass;                                                      
    float    staticFriction;                                      
    float    dynamicFriction;                       
    float    updateMask;
};

RWStructuredBuffer<Object> MovedObjects;
RWStructuredBuffer<uint> MovedObjectCounter; // size 1
RWStructuredBuffer<GlobalObj> GlobalObjects; // список всех обектов

#pragma endregion

void AddMovedObject(uint objId_InGlobalBuffer, AABB bounds)
{
    uint movedObjectId;
    InterlockedAdd(MovedObjectCounter[0], 1, movedObjectId);
    
    Object object;
    object.objId_InGlobalBuffer = objId_InGlobalBuffer;
    object.bounds = bounds;
    
    MovedObjects[movedObjectId] = object;
}

#pragma region Morton

struct MortonPair 
{
    uint mortonCode; // Сам код Мортона
    uint objectID;   // Исходный индекс объекта
};

cbuffer SceneBounds : register(b0)
{
    float3 SceneMin;
    float3 SceneInvSize; // 1.0 / (SceneMax - SceneMin)
    uint   ObjectCount_SceneBounds;
};

RWStructuredBuffer<MortonPair> Out_MortonPairs : register(u0);

// Вспомогательная функция: вставляет 2 нуля между каждым битом 10-битного числа
uint ExpandBits(uint v)
{
    v = (v * 0x00010001u) & 0xFF0000FFu;
    v = (v * 0x00000101u) & 0x0F00F00Fu;
    v = (v * 0x00000011u) & 0xC30C30C3u;
    v = (v * 0x00000005u) & 0x49249249u;
    return v;
}

// Генерация 32-битного кода Мортона из нормализованных координат [0, 1]
uint CalculateMorton3D(float3 center)
{
    // Нормализуем центр относительно границ всей сцены в диапазон [0, 1]
    float3 normalizedPos = (center - SceneMin) * SceneInvSize;
    
    // Масштабируем [0.0, 1.0] в диапазон 10-битного целого числа [0, 1023]
    uint x = min(max((uint)(normalizedPos.x * 1024.0f), 0u), 1023u);
    uint y = min(max((uint)(normalizedPos.y * 1024.0f), 0u), 1023u);
    uint z = min(max((uint)(normalizedPos.z * 1024.0f), 0u), 1023u);

    // Раздвигаем биты и переплетаем их
    return (ExpandBits(x) << 0) | (ExpandBits(y) << 1) | (ExpandBits(z) << 2);
}

[numthreads(64, 1, 1)]
void MortonKernel(uint3 dtID : SV_DispatchThreadID)
{
    uint objIdx = dtID.x;

    if (objIdx >= ObjectCount_SceneBounds) return;          // Защита от выхода за пределы массива
    
    AABB bounds = MovedObjects[objIdx].bounds;              // 1. Получаем AABB текущего объекта
    float3 center = (bounds.minPos + bounds.maxPos) * 0.5f; // 2. Вычисляем центр объекта
    uint code = CalculateMorton3D(center);                  // 3. Считаем код Мортона
    
    MortonPair pair;                                        // 4. Записываем пару для последующей сортировки
    pair.mortonCode = code;                                 //
    pair.objectID = objIdx;                                 //
                                                            //
    Out_MortonPairs[objIdx] = pair;                         //
}
#pragma endregion

#pragma region Radix Sort

// Константы конфигурации
#define THREADS_PER_BLOCK 256
#define BITS_PER_PASS 4
#define NUM_BUCKETS 16 // 2^4

RWStructuredBuffer<uint> OutputBuffer    : register(u1);

// Вспомогательные буферы для межшейдерного обмена
RWStructuredBuffer<uint> BlockHistograms : register(u2); 
RWStructuredBuffer<uint> GlobalOffsets   : register(u3);

cbuffer SortSettings : register(b0)
{
    uint BitShift;       // Текущий сдвиг (0, 4, 8, 12...)
    uint TotalElements;  // Размер массива чисел
    uint TotalBlocks;    // Общее число блоков (Ceil(TotalElements / 256))
};
groupshared uint s_Histogram[NUM_BUCKETS];

[numthreads(THREADS_PER_BLOCK, 1, 1)]
void CS_Upsweep(
    uint3 Gid : SV_GroupID, 
    uint3 tid : SV_GroupThreadID,
    uint3 DTid : SV_DispatchThreadID)
{
    // 1. Быстрое обнуление гистограммы блока
    if (tid.x < NUM_BUCKETS) 
    {
        s_Histogram[tid.x] = 0;
    }
    GroupMemoryBarrierWithGroupSync();

    // 2. Чтение элемента и определение его корзины
    uint globalIdx = DTid.x;
    uint bucket = 0xFFFFFFFF;
    
    if (globalIdx < TotalElements)
    {
        uint element = Out_MortonPairs[globalIdx].mortonCode;
        bucket = (element >> BitShift) & 0xF;
    }

    // 3. Оптимизация через Wave-интринсики
    // Потоки внутри одной волны (варпа) суммируют свои данные без обращений к памяти
    [unroll]
    for (uint b = 0; b < NUM_BUCKETS; b++)
    {
        bool isTargetBucket = (bucket == b);
        // Сколько потоков в текущей волне (32 или 64) выбрали эту корзину
        uint waveCount = WaveActiveCountBits(isTargetBucket);
        
        // Только первый поток из волны, у которого совпал бакет, пишет сумму в shared
        if (waveCount > 0 && WaveIsFirstLane())
        {
            InterlockedAdd(s_Histogram[b], waveCount);
        }
    }
    GroupMemoryBarrierWithGroupSync();

    // 4. Запись гистограммы этого блока в глобальную память
    if (tid.x < NUM_BUCKETS)
    {
        uint blockIdx = Gid.x;
        // Раскладка: [Корзина 0: Блок 0..TotalBlocks-1][Корзина 1: Блок 0..TotalBlocks-1]...
        BlockHistograms[tid.x * TotalBlocks + blockIdx] = s_Histogram[tid.x];
    }
}
groupshared uint s_ScanMem[THREADS_PER_BLOCK * 2];

[numthreads(THREADS_PER_BLOCK, 1, 1)]
void CS_Scan(uint3 tid : SV_GroupThreadID)
{
    // Суммарный размер таблицы гистограмм = Число блоков * 16 корзин
    uint totalHistogramEntries = TotalBlocks * NUM_BUCKETS;
    uint globalOffsetAccumulator = 0;

    // Сканируем блоками по THREADS_PER_BLOCK элементов.
    // Благодаря раскладке [Корзина][Блок] из CS_Upsweep этот обычный линейный
    // scan уже даёт правильную семантику: exclusiveSum для (корзина d, блок b) =
    // (сумма всех элементов корзин < d по всем блокам) + (сумма корзины d в блоках < b).
    for (uint offset = 0; offset < totalHistogramEntries; offset += THREADS_PER_BLOCK)
    {
        uint localIdx = tid.x;
        uint globalIdx = offset + localIdx;

        // 1. Загружаем данные в shared память
        s_ScanMem[localIdx] = (globalIdx < totalHistogramEntries) ? BlockHistograms[globalIdx] : 0;
        GroupMemoryBarrierWithGroupSync();

        // 2. Инкрементный префиксный скан внутри блока (Kogge-Stone / Hillis-Steele алгоритм)
        uint sum = s_ScanMem[localIdx];
        [unroll]
        for (uint stride = 1; stride < THREADS_PER_BLOCK; stride *= 2)
        {
            uint val = 0;
            if (localIdx >= stride)
            {
                val = s_ScanMem[localIdx - stride];
            }
            GroupMemoryBarrierWithGroupSync();
            sum += val;
            s_ScanMem[localIdx] = sum;
            GroupMemoryBarrierWithGroupSync();
        }

        // 3. Вычисляем эксклюзивный скан и добавляем смещение от прошлых итераций цикла
        if (globalIdx < totalHistogramEntries)
        {
            uint inclusiveSum = s_ScanMem[localIdx];
            uint exclusiveSum = inclusiveSum - BlockHistograms[globalIdx];
            
            // Записываем финальный адрес для этого блока и этой корзины
            GlobalOffsets[globalIdx] = exclusiveSum + globalOffsetAccumulator;
        }

        // Обновляем аккумулятор общего смещения для следующей итерации цикла
        GroupMemoryBarrierWithGroupSync();
        globalOffsetAccumulator += s_ScanMem[THREADS_PER_BLOCK - 1];
        GroupMemoryBarrierWithGroupSync();
    }
}
groupshared uint s_BlockOffsets[NUM_BUCKETS];

// В HLSL groupshared-переменные обязаны объявляться глобально, а не внутри функции.
groupshared uint s_WaveOffsets[NUM_BUCKETS][8]; // Максимум 8 волн в блоке из 256 потоков

[numthreads(THREADS_PER_BLOCK, 1, 1)]
void CS_Downsweep(
    uint3 Gid : SV_GroupID, 
    uint3 tid : SV_GroupThreadID,
    uint3 DTid : SV_DispatchThreadID)
{
    uint blockIdx = Gid.x;

    // 1. Кешируем стартовые глобальные адреса корзин для нашего блока
    // ИСПРАВЛЕНО: индексация под новую раскладку [Корзина][Блок].
    if (tid.x < NUM_BUCKETS)
    {
        s_BlockOffsets[tid.x] = GlobalOffsets[tid.x * TotalBlocks + blockIdx];
    }
    GroupMemoryBarrierWithGroupSync();

    uint globalIdx = DTid.x;
    if (globalIdx >= TotalElements) return;

    // 2. Читаем элемент
    MortonPair pairData = Out_MortonPairs[globalIdx];
    uint sortKey = pairData.mortonCode;
    uint element = pairData.objectID;
    uint bucket = (sortKey >> BitShift) & 0xF;

    // 3. Нахождение локального индекса внутри блока БЕЗ памяти и циклов
    uint localRank = 0;
    
    [unroll]
    for (uint b = 0; b < NUM_BUCKETS; b++)
    {
        bool isCurrentBucket = (bucket == b);
        
        // Интринсик возвращает количество потоков в текущей WAVE,
        // у которых индекс меньше текущего И условие true.
        if (isCurrentBucket)
        {
            localRank = WavePrefixCountBits(isCurrentBucket);
        }
    }

    // 4. Синхронизируем локальные сдвиги между волнами (если внутри блока > 1 волны)
    // Для этого временно используем shared-память, чтобы прибавить сдвиги от прошлых волн
    // (Поскольку THREADS_PER_BLOCK = 256, а Wave обычно 32 или 64)
    uint waveIdx = tid.x / WaveGetLaneCount();
    
    if (WaveIsFirstLane())
    {
        // Каждая волна считает сколько у нее элементов в каждом бакете всего
        [unroll]
        for (uint b = 0; b < NUM_BUCKETS; b++)
        {
            s_WaveOffsets[b][waveIdx] = WaveActiveCountBits(bucket == b);
        }
    }
    GroupMemoryBarrierWithGroupSync();

    // Считаем префиксную сумму сдвигов между волнами для нашего бакета
    uint intraWaveOffset = 0;
    for (uint w = 0; w < waveIdx; w++)
    {
        intraWaveOffset += s_WaveOffsets[bucket][w];
    }

    // 5. Запись в финальную отсортированную ячейку
    uint finalIndex = s_BlockOffsets[bucket] + localRank + intraWaveOffset;
    OutputBuffer[finalIndex] = element;
}


#pragma endregion

#pragma region BVH
struct BVHNode {
    AABB   bounds;
    uint   leftChild;
    uint   rightChild;
    uint   parent;
    uint   isLeaf;
};

// использовались оба под u0 одновременно в BVH_MAin - конфликт слота.
RWStructuredBuffer<BVHNode> BVHNodes           : register(u0);  // Дерево
StructuredBuffer  <uint>    In_ActiveIndices   : register(t1);   // Индексы нод на текущем шаге
RWStructuredBuffer<uint>    Out_ActiveIndices  : register(u1);  // Индексы нод для следующего шага

// Буфер со счетчиками (выделение памяти под внутренние ноды и под сборку следующего шага)
// Байт 0 - Глобальный индекс для выделения внутренних нод (стартует с N)
// Байт 4 - Счетчик активных нод для следующего шага (стартует с 0)
RWByteAddressBuffer    GlobalCounters     : register(u2); 

cbuffer PLOCSettings : register(b0) {
    uint ActiveCount;                   // Сколько нод участвует в этой итерации
    uint SearchWindow;                  // Радиус поиска соседей (в оригинале статьи обычно 16 или 32)
    uint ObjectCount_PLOCSettings;      // Общее число N
};

// Функция объединения двух AABB в один общий
AABB MergeAABB(AABB a, AABB b) {
    AABB merged;
    merged.minPos = min(a.minPos, b.minPos);
    merged.maxPos = max(a.maxPos, b.maxPos);
    return merged;
}

// Вычисление площади поверхности AABB (Метрика SAH)
float GetSurfaceArea(AABB box) {
    float3 d = box.maxPos - box.minPos;
    return 2.0f * (d.x * d.y + d.y * d.z + d.z * d.x);
}


#define GROUP_SIZE 64

groupshared uint SharedNodeIndices[GROUP_SIZE];                 // Реальные ID нод в дереве
groupshared uint SharedBestNeighbors[GROUP_SIZE];               // Индексы лучших соседей, которых выбрал каждый поток

[numthreads(64, 1, 1)]
void BVH_Main(uint3 dtID : SV_DispatchThreadID, uint  localIdx : SV_GroupIndex)
{
    uint currentActiveIdx = dtID.x;
    if (currentActiveIdx >= ActiveCount) return;

    // 1. Получаем индекс текущей ноды в дереве
    uint myNodeIdx = In_ActiveIndices[currentActiveIdx];

    if (myNodeIdx < ObjectCount_PLOCSettings)
    {
        BVHNodes[myNodeIdx].isLeaf     = 1;
        BVHNodes[myNodeIdx].leftChild  = myNodeIdx;      // objectId листа
        BVHNodes[myNodeIdx].rightChild = 0xFFFFFFFFu;
        BVHNodes[myNodeIdx].parent     = 0xFFFFFFFFu;
        BVHNodes[myNodeIdx].bounds     = MovedObjects[myNodeIdx].bounds; // читаем ТОЛЬКО когда точно лист
    }

    AABB myBox = BVHNodes[myNodeIdx].bounds;
    
    // 2. ПОИСК БЛИЖАЙШЕГО СОСЕДА (Nearest Neighbor)
    uint bestNeighborActiveIdx = 0xFFFFFFFFu;
    float minArea = 3.402823466e+38f; // FLT_MAX

    // Сканируем окно соседей вокруг текущего индекса в активном массиве
    int start = max(0, (int)currentActiveIdx - (int)SearchWindow);
    int end   = min((int)ActiveCount - 1, (int)currentActiveIdx + (int)SearchWindow);

    for (int j = start; j <= end; j++) 
    {
        if ((uint)j == currentActiveIdx) continue;

        uint neighborNodeIdx = In_ActiveIndices[j];
        AABB neighborBox = BVHNodes[neighborNodeIdx].bounds;

        // Считаем площадь объединенного бокса
        AABB combinedBox = MergeAABB(myBox, neighborBox);
        float area = GetSurfaceArea(combinedBox);

        if (area < minArea) {
            minArea = area;
            bestNeighborActiveIdx = (uint)j;
        }
    }

    // 3. ВЗАИМНОЕ СЛИЯНИЕ (Mutual Nearest Neighbor)
    SharedNodeIndices[localIdx]   = myNodeIdx;
    SharedBestNeighbors[localIdx] = bestNeighborActiveIdx;
    
    GroupMemoryBarrierWithGroupSync();  // СИНХРОНИЗАЦИЯ: Ждем, пока вся группа (64 потоков) запишет свои выборы
    
    bool isMutualMatch = false;
    uint neighborNodeIdx = 0xFFFFFFFFu;
    
    uint groupStartActiveIdx = currentActiveIdx - localIdx;
    int neighborLocalIdx = (int)bestNeighborActiveIdx - (int)groupStartActiveIdx;

    //  Если наш сосед находится внутри нашей же группы потоков
    if (neighborLocalIdx >= 0 && neighborLocalIdx < GROUP_SIZE)
    {
        // Читаем из быстрой LDS-памяти, кого выбрал этот сосед
        uint neighborsChoice = SharedBestNeighbors[neighborLocalIdx];
        
        // Взаимность подтверждена, если сосед выбрал НАС (наш глобальный индекс)
        if (neighborsChoice == currentActiveIdx)
        {
            isMutualMatch = true;
            neighborNodeIdx = SharedNodeIndices[neighborLocalIdx];
        }
    }
    
    // Если сосед оказался за пределами группы (на стыке блоков)    :(
    else if (bestNeighborActiveIdx < ActiveCount)
    {
        // 1. Достаем коробку нашего пограничного соседа напрямую из дерева
        uint globalNeighborNodeIdx = In_ActiveIndices[bestNeighborActiveIdx];
        AABB neighborBox = BVHNodes[globalNeighborNodeIdx].bounds;

        // 2. Нам нужно проверить: выберет ли этот сосед НАС взаимно?
        // Для этого мы встаем на место соседа и сканируем ЕГО окно поиска.
        uint neighborsBestChoice = 0xFFFFFFFFu;
        float minAreaForNeighbor = 3.402823466e+38f; // FLT_MAX

        int nStart = max(0, (int)bestNeighborActiveIdx - (int)SearchWindow);
        int nEnd   = min((int)ActiveCount - 1, (int)bestNeighborActiveIdx + (int)SearchWindow);

        for (int k = nStart; k <= nEnd; k++) 
        {
            if ((uint)k == bestNeighborActiveIdx) continue;

            uint candidateNodeIdx = In_ActiveIndices[k];
            AABB candidateBox = BVHNodes[candidateNodeIdx].bounds;

            // Считаем площадь объединения соседа с кандидатом 'k'
            AABB combinedBox = MergeAABB(neighborBox, candidateBox);
            float area = GetSurfaceArea(combinedBox);

            if (area < minAreaForNeighbor) 
            {
                minAreaForNeighbor = area;
                neighborsBestChoice = (uint)k;
            }
        }

        // 3. Если сосед, перебрав свое окно, выбрал именно НАШ индекс
        if (neighborsBestChoice == currentActiveIdx)
        {
            isMutualMatch = true;                       // Взаимность доказана
            neighborNodeIdx = globalNeighborNodeIdx;    // Запоминаем ID его ноды в дереве
        }
    }
    
    if (isMutualMatch)
    {
        // Взаимная пара найдена: только поток с меньшим activeIdx физически
        // создаёт родителя (чтобы не сделать это дважды для одной пары).
        if (currentActiveIdx < bestNeighborActiveIdx)
        {
            // 1. Атомарно выделяем место под новую внутреннюю ноду в дереве
            uint newParentNodeIdx;
            GlobalCounters.InterlockedAdd(0, 1, newParentNodeIdx); // Счетчик внутренних нод (начиная с N)

            // 2. Объединяем коробки детей
            uint nodeA = myNodeIdx;
            uint nodeB = neighborNodeIdx;
            
            BVHNode parentNode;
            parentNode.bounds     = MergeAABB(BVHNodes[nodeA].bounds, BVHNodes[nodeB].bounds);
            parentNode.leftChild  = nodeA;
            parentNode.rightChild = nodeB;
            parentNode.parent     = 0xFFFFFFFFu;        // Родителя у этой новой ноды пока нет
            parentNode.isLeaf     = 0;                  // Это внутренняя нода
            
            BVHNodes[newParentNodeIdx] = parentNode;    // Записываем родителя в дерево

            // 3. Прописываем детям индекс их нового родителя
            BVHNodes[nodeA].parent = newParentNodeIdx;
            BVHNodes[nodeB].parent = newParentNodeIdx;

            // 4. Добавляем НОВУЮ ноду в список активных для СЛЕДУЮЩЕЙ итерации
            uint nextActivePos;
            GlobalCounters.InterlockedAdd(4, 1, nextActivePos); // Счетчик активных нод следующего шага
            Out_ActiveIndices[nextActivePos] = newParentNodeIdx;
        }
        // else: "проигравшая" сторона пары ничего не делает - родитель уже
        // добавлен другим потоком, самой ноде в следующий список попадать не нужно.
    }
    else
    {
        uint nextActivePos;
        GlobalCounters.InterlockedAdd(4, 1, nextActivePos);
        Out_ActiveIndices[nextActivePos] = myNodeIdx;
    }
}
#pragma endregion

#pragma region FinalizeBVHIterationArgs

RWStructuredBuffer<uint> BVHIndirectArgs : register(u3);

// Финальный индекс корня. Читается CS_TraverseBVH напрямую как SRV — без
// единого readback на CPU за весь кадр.
RWStructuredBuffer<uint> RootNodeIndexBuffer : register(u4);

[numthreads(1, 1, 1)]
void CS_FinalizeBVHIterationArgs(uint3 DTid : SV_DispatchThreadID)
{
    uint nextActiveCount = GlobalCounters.Load(4);

    if (nextActiveCount <= 1)
    {
        // Дерево схлопнулось (или было изначально 0-1 объектов в сцене).
        // Читаем корень прямо из Out_ActiveIndices — того же буфера, куда
        // BVH_Main писал результат этой итерации. Отдельная декларация
        // для чтения не нужна: RWStructuredBuffer разрешает и запись, и
        // чтение в одном кернеле.
        if (nextActiveCount == 1)
        {
            RootNodeIndexBuffer[0] = Out_ActiveIndices[0];
        }

        // Обнуляем группы потоков — следующий DispatchIndirect(BVH_Main)
        // не выполнит ни одного потока. Останавливает бесконечное
        // самокопирование последней активной ноды (см. разбор бага в чате).
        BVHIndirectArgs[0] = 0;
    }
    else
    {
        BVHIndirectArgs[0] = (nextActiveCount + 63) / 64; // threadGroupsX
    }
    BVHIndirectArgs[1] = 1;
    BVHIndirectArgs[2] = 1;

    // Переносим "next" в "current" для следующей итерации BVH_Main и
    // обнуляем "next" для накопления в предстоящей итерации.
    GlobalCounters.Store(8, nextActiveCount); // current ActiveCount на следующий шаг
    GlobalCounters.Store(4, 0);               // next-счётчик обнулён под новую итерацию
}

#pragma endregion

#pragma region BVH Traversal

struct CandidatePair
{
    uint objA; // globalNodeIdx (== objBufferIndex для листьев 0..N-1)
    uint objB;
};

StructuredBuffer<BVHNode>         In_BVHNodes           : register(t0);
RWStructuredBuffer<CandidatePair> Out_CandidatePairs    : register(u0); // буфер с парами  под gjk 
RWStructuredBuffer<uint>          Out_PairCount         : register(u1); // [0] = атомарный счётчик

cbuffer BVHTraversalSettings : register(b0)
{
    uint ObjectCount_Trav;   // N — количество листьев == количество объектов
    uint MaxPairsCapacity;   // размер Out_CandidatePairs, чтобы не улететь за границу
};

StructuredBuffer<uint> In_RootNodeIndex : register(t1);

bool OverlapAABB(AABB a, AABB b)
{
    return (a.minPos.x <= b.maxPos.x && a.maxPos.x >= b.minPos.x) &&
           (a.minPos.y <= b.maxPos.y && a.maxPos.y >= b.minPos.y) &&
           (a.minPos.z <= b.maxPos.z && a.maxPos.z >= b.minPos.z);
}

void EmitPair(uint idA, uint idB)
{
    uint writeIdx;
    InterlockedAdd(Out_PairCount[0], 1, writeIdx);
    if (writeIdx < MaxPairsCapacity)
    {
        CandidatePair p;
        p.objA = idA;
        p.objB = idB;
        Out_CandidatePairs[writeIdx] = p;
    }
}

#define STACK_SIZE 64

[numthreads(64, 1, 1)]
void CS_TraverseBVH(uint3 DTid : SV_DispatchThreadID)
{
    uint myLeafIdx = DTid.x;
    
    if (myLeafIdx >= ObjectCount_Trav) return;
    
    BVHNode node = In_BVHNodes[myLeafIdx];
    AABB myBox = node.bounds;
    uint myObjId = node.leftChild; // у листа leftChild переиспользован под objectId

    // Локальный стек индексов нод, которые предстоит посетить
    uint stack[STACK_SIZE];
    int stackPtr = 0;
    stack[stackPtr++] = In_RootNodeIndex[0];

    while (stackPtr > 0)
    {
        uint nodeIdx = stack[--stackPtr];
        BVHNode node0 = In_BVHNodes[nodeIdx];

        // Грубая отсечка: если AABB ноды (поддерева) не пересекает нас — пропускаем
        if (!OverlapAABB(myBox, node0.bounds)) continue;
        
        if (node0.isLeaf != 0)
        {
            // У листа leftChild переиспользован под objectId (см. BVH_Main).
            uint otherObjId = node0.leftChild;

            if (otherObjId != myObjId && myObjId < otherObjId)
            {
                EmitPair(myObjId, otherObjId);
            }
            continue;
        }
        
        if (stackPtr < STACK_SIZE) stack[stackPtr++] = node0.leftChild;
        if (stackPtr < STACK_SIZE) stack[stackPtr++] = node0.rightChild;
    }
}

#pragma endregion