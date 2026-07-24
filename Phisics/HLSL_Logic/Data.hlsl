#pragma region Общие константы и Статичные переменные:
    float3 chunksCount;
    float chunk_Size;

    int3 chunkOffset;
    uint maxChunksIDsInObject = 12;

    uint orchestrator_buffer_size;

    float DeltaTime;
    float3 Gravity;
    float3 MouseDelta;

#pragma endregion

#pragma region Структуры и константы к ним:
#pragma region Чанки:
    struct Chunk
    {
        float3 min_xyz;
        float3 max_xyz;
        
        uint x_plus_chunkId;
        uint x_min_chunkId;
        
        uint y_plus_chunkId;
        uint y_min_chunkId;
        
        uint z_plus_chunkId;
        uint z_min_chunkId;
        
        float obj[36];
    };
#pragma endregion

#pragma region Информация для CPU ReadBack:
    struct BackStruct
    {
        uint index0;
        uint index1;
        uint index2;
        uint index3;
    };
#pragma endregion

#pragma region Объект:
    #pragma region Типы коллайдеров:
    #define DECLARE_COLLIDER_TYPE(name, num) uint ColliderType_##name = num;

    DECLARE_COLLIDER_TYPE(None,     0)
    DECLARE_COLLIDER_TYPE(Box,      1)
    DECLARE_COLLIDER_TYPE(Sphere,   2)
    DECLARE_COLLIDER_TYPE(Capsule,  3)
    DECLARE_COLLIDER_TYPE(Cylinder, 4)
    DECLARE_COLLIDER_TYPE(Plane,    5)
    DECLARE_COLLIDER_TYPE(Mesh,     6)
        
#pragma endregion

    #pragma region Object
    struct Object
    {
        float4x4 transform;                     // offset 0,  size 64
        
        float3 massCenter;                      // offset 64, size 16
        float3 minAABB;                         // offset 80, size 16
        float3 maxAABB;                         // offset 96, size 16
        float3 rotation;                        // offset 112, size 16
        float3 position;                        // offset 128, size 16
        float3 velocity;                        // offset 144, size 16
        float3 angularVelocity;                 // offset 160, size 16
        
        uint chunksIDs[maxChunksIDsInObject];   // offset 176, size 48
        uint additionalObjBufferIndex[9];       // offset 224, size 36   TODO: подумать над тем что будет если обект с номером 0 будет в буф
        
        uint colliderType;                      // offset 260, size 4
        uint objBufferIndex;                    // offset 264, size 4
        uint additionalBufferIndex;             // offset 268, size 4
        
        float mass;                             // offset 272, size 4
        float staticFriction;                   // offset 276, size 4
        float dynamicFriction;                  // offset 280, size 4
        float updateMask;                       // offset 284, size 4
    };
    #pragma endregion

    #pragma region Коллайдеры
        #pragma region Mesh:
            struct Mesh { uint4 verticesBuffer[6]; uint vertexCount;}; // почитат  про размерность
            struct MeshVertices { float3 vertices[36]; };
            struct AddMeshData
            {
                float3 massCenter;
                float3 minAABB;
                float3 maxAABB;
                
                uint additionalMesh[9];
                uint startVerticeIndex;
                uint endVerticeIndex;
            };
        #pragma endregion

        #pragma region Sphere:
        #pragma endregion

    #pragma endregion

    #pragma region Структура для добавления в буффер
        struct AddToBuffer
        {
            uint objectIndex;
            bool addStatus;
        };
    #pragma endregion

#pragma endregion

#pragma region Movement:
    struct MoveAdditionalData
    {
        float4 data0;
        float4 data1;
        float4 data2;
        float4 data3;
    };
    
    #define DECLARE_MOVE_TYPE(name, num) uint MoveType_##name = num
    DECLARE_MOVE_TYPE(Orbit, 0);
    
#pragma endregion

#pragma region Оркестратор:
    struct IndexOrchestrator
    {
        uint index0;
        uint index1;
        uint index2;
        uint index3;
    };

    const uint None = 0;
    const uint Movement = 1;
    const uint AddMove = 2;
#pragma endregion

#pragma endregion