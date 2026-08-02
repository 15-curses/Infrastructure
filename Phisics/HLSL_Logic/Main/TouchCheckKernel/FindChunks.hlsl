#define BLOCK_SIZE 64

struct SortKeyPair {
    uint key;   // Сюда пишем chunkID, в котором сейчас находится объект
    uint value; // Сюда пишем objBufferIndex (индекс объекта)
};

RWStructuredBuffer<SortKeyPair> OutSortBuffer : register(u0);
RWStructuredBuffer<uint>        OutGlobalCount : register(u1);

// Данные о размерах мира (передаются из C++)
cbuffer GridParams : register(b0) {
    float3 g_GridMin;
    float3 g_ChunkSize;
    uint3  g_GridDimensions;
    uint   g_MaxObjects;
};

[numthreads(BLOCK_SIZE, 1, 1)]
void CS_FindChunks(uint3 DTid : SV_DispatchThreadID)
{
    uint objID = DTid.x;
    if (objID >= g_MaxObjects) return;

    Object obj = ObjectBuffer[objID];

    // Вычисляем 3D индекс чанка по позиции центра масс объекта
    float3 localPos = obj.massCenter - g_GridMin;
    int3 chunkCoord = int3(localPos / g_ChunkSize);

    // Проверяем границы сетки мира
    if (chunkCoord.x >= 0 && chunkCoord.x < (int)g_GridDimensions.x &&
        chunkCoord.y >= 0 && chunkCoord.y < (int)g_GridDimensions.y &&
        chunkCoord.z >= 0 && chunkCoord.z < (int)g_GridDimensions.z)
    {
        // Переводим 3D координаты чанка в линейный chunkID
        uint chunkID = chunkCoord.x + 
                       chunkCoord.y * g_GridDimensions.x + 
                       chunkCoord.z * g_GridDimensions.x * g_GridDimensions.y;

        // Бронируем место в буфере сортировки
        uint writeIdx;
        InterlockedAdd(OutGlobalCount[0], 1, writeIdx);

        // Записываем пару для Radix Sort
        SortKeyPair pair;
        pair.key = chunkID;          // Сортируем по ID чанка
        pair.value = obj.objBufferIndex; // Запоминаем, какой объект тут лежит

        OutSortBuffer[writeIdx] = pair;
    }
}