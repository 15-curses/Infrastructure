uint UpdateChunkIDsForAABB(AABB aabb, inout uint chunksIDs[maxChunksIDsInObject])
{
    uint count = 0;
    
    [unroll]
    for (uint i = 0; i < maxChunksIDsInObject; ++i) 
    {
        chunksIDs[i] = 0;
    }
    
    int3 minChunkWorld = (int3)floor(aabb.min / chunk_Size);
    int3 maxChunkWorld = (int3)floor(aabb.max / chunk_Size);
    
    int3 minChunkLocal = minChunkWorld - chunkOffset;
    int3 maxChunkLocal = maxChunkWorld - chunkOffset;
    
    int3 limit = (int3)chunksCount - int3(1, 1, 1);
    
    if (any(minChunkLocal > limit) || any(maxChunkLocal < int3(0, 0, 0)))
    {
        return 0; 
    }
    
    minChunkLocal = clamp(minChunkLocal, int3(0, 0, 0), limit);
    maxChunkLocal = clamp(maxChunkLocal, int3(0, 0, 0), limit);
    
    uint strideY = (uint)chunksCount.x;                         // TODO: на GridParams переписать
    uint strideZ = (uint)chunksCount.x * (uint)chunksCount.y;   // TODO: на GridParams переписать
    
    for (int z = minChunkLocal.z; z <= maxChunkLocal.z; ++z)
    {
        for (int y = minChunkLocal.y; y <= maxChunkLocal.y; ++y)
        {
            for (int x = minChunkLocal.x; x <= maxChunkLocal.x; ++x)
            {
                if (count < maxChunksIDsInObject)
                {
                    uint flatID = (uint)x + (uint)y * strideY + (uint)z * strideZ;
                    
                    chunksIDs[count] = flatID;
                    count++;
                }
                else
                {
                    return count;
                }
            }
        }
    }
    return count;
}