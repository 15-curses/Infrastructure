// Методы индексирования буферов для заполнения и удаления произвольных свободных позиций 
// (не только последовательные вставки)

#define DEFINE_ADD_REMOVE_METHODS(NAME, BUFFER) \
RWStructuredBuffer<uint> NAME##Indices; \
uint NAME##IndexCount = 0; \
AddToBuffer Add##NAME(NAME data) \
{ \
    AddToBuffer result = {}; \
    result.addStatus = GetFreeIndex(result.objectIndex, NAME##Indices, NAME##IndexCount); \
    BUFFER[result.objectIndex] = data; \
    return result; \
} \
void Delete##NAME(uint index) \
{ \
    AddFreeIndex(index, NAME##Indices, NAME##IndexCount); \
}

// Теперь эти вызовы развернутся корректно:
DEFINE_ADD_REMOVE_METHODS(Object, ObjectBuffer)
DEFINE_ADD_REMOVE_METHODS(Mesh, MeshBuffer)
DEFINE_ADD_REMOVE_METHODS(MeshVertices, MeshVerticesBuffer)
DEFINE_ADD_REMOVE_METHODS(Chunk, ChunksBuffer)