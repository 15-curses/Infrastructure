#pragma region Chunk буферы:
    #pragma region Основные:
        RWStructuredBuffer<Chunk> ChunksBuffer;
    #pragma endregion

    #pragma region Доп:
        RWStructuredBuffer<uint> ChunksIndices;
        RWStructuredBuffer<uint> ChunksIndexCount;
    #pragma endregion

    #pragma region Доп:
    #pragma endregion 
#pragma endregion

#pragma region Object-Colliders буферы:
    #pragma region Основные:
        #pragma region Объект буферы:
            #pragma region Основные:
                RWStructuredBuffer<Object> ObjectBuffer;
            #pragma endregion
                         
            #pragma region Доп:
               RWStructuredBuffer<uint> ObjectIndices;
               RWStructuredBuffer<uint> ObjectIndexCount;
            #pragma endregion 
                             
        #pragma endregion

        #pragma region Mesh:

            #pragma region Основные:
                RWStructuredBuffer<AddMeshData> AddMeshBuffer;
                RWStructuredBuffer<Mesh> MeshBuffer;
                RWStructuredBuffer<MeshVertices> MeshVerticesBuffer;
            #pragma endregion

            #pragma region Доп:
                RWStructuredBuffer<uint> MeshIndices;
                RWStructuredBuffer<uint> MeshIndexCount;
                RWStructuredBuffer<uint> MeshVerticesIndices;
                RWStructuredBuffer<uint> MeshVerticesIndexCount;
            #pragma endregion

        #pragma endregion

        #pragma region Sphere:
        #pragma endregion

    #pragma endregion

    #pragma region Доп:
    #pragma endregion

#pragma endregion