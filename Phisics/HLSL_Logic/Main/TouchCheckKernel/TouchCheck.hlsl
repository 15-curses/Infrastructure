void CheckCollisionInChunk(uint chunkID)
{
    Chunk mainChunk = ChunkBuffer[chunkID];
    
    for (uint i = 0; i < mainChunk.objectCount; ++i)
    {
        uint sortedObjIdx = mainChunk.firstObjectSortedIndex + i;
        uint objID = SortedSortBuffer[sortedObjIdx].value;
        Object objA = ObjectBuffer[objID];
        
        uint neighborChunkID = mainChunk.x_plus_chunkId;
        Chunk neighbor = ChunkBuffer[neighborChunkID];
        
        for (uint j = 0; j < neighbor.objectCount; ++j)
        {
            uint neighborSortedObjIdx = neighbor.firstObjectSortedIndex + j;
            uint neighborObjID = SortedSortBuffer[neighborSortedObjIdx].value;
            Object objB = ObjectBuffer[neighborObjID];
            
            ResolveCollision(objA, objB);
        }
    }
}