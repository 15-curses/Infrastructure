#pragma region Chunk буферы:
StructuredBuffer<Chunk>      ChunkBuffer       : register(t1); 
#pragma endregion

#pragma region Object-Colliders буферы:
    
    #pragma region Объект буферы:
        StructuredBuffer<Object>     ObjectBuffer      : register(t0);
        RWStructuredBuffer<AdditionalData> AdditionalDataBuffer;
    #pragma endregion

    #pragma region Mesh:
        RWStructuredBuffer<Mesh> MeshBuffer;
        RWStructuredBuffer<MeshVertices> MeshVerticesBuffer;
    #pragma endregion

#pragma endregion

#pragma region Оркестратор буферы:
    RWStructuredBuffer<uint> IndirectArgsBuffer;
    StructuredBuffer<uint> CounterBuffer;
#pragma endregion

#pragma region Move буферы:
    RWStructuredBuffer<uint> NeedTouchCheckObjBuffer;
    RWStructuredBuffer<uint> MoveBuffer;
#pragma endregion
