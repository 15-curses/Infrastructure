// Методы индексирования буферов для заполнения и удаления произвольных свободных позиций 
// (не только последовательные вставки)

#define DEFINE_ADD_REMOVE_METHODS(TYPE, BUFFER, INDICES, COUNT) \
AddToBuffer Add##TYPE(TYPE data) \
{ \
    AddToBuffer result; \
    result.addStatus = GetFreeIndex(result.objectIndex, INDICES, COUNT); \
    BUFFER[result.objectIndex] = data; \
    return result; \
} \
void Delete##TYPE(uint index) \
{ \
    AddFreeIndex(index, INDICES, COUNT); \
}

DEFINE_ADD_REMOVE_METHODS(Object, ObjectBuffer, ObjectIndices, ObjectIndexCount)
DEFINE_ADD_REMOVE_METHODS(Mesh, MeshBuffer, ObjectIndices, ObjectIndexCount)
DEFINE_ADD_REMOVE_METHODS(MeshVertices, MeshVerticesBuffer, MeshVerticesIndices, MeshVerticesIndexCount)
DEFINE_ADD_REMOVE_METHODS(Chunk, ChunksBuffer, ChunksIndices, ChunksIndexCount)
DEFINE_ADD_REMOVE_METHODS()