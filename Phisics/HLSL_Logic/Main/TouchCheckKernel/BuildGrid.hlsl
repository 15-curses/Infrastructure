StructuredBuffer<SortKeyPair> SortedSortBuffer : register(t0);
StructuredBuffer<uint>        GlobalCount      : register(t1);
RWStructuredBuffer<Chunk>     RWChunkBuffer    : register(u0);

[numthreads(64, 1, 1)]
void CS_BuildGrid(uint3 DTid : SV_DispatchThreadID)
{
    uint sortedIdx = DTid.x;
    uint maxElements = GlobalCount[0];
    if (sortedIdx >= maxElements) return;

    uint currentChunkID = SortedSortBuffer[sortedIdx].key;
    
    if (sortedIdx == 0 || SortedSortBuffer[sortedIdx - 1].key != currentChunkID)
    {
        RWChunkBuffer[currentChunkID].firstObjectSortedIndex = sortedIdx;
        
        uint count = 1;
        while ((sortedIdx + count < maxElements) && (SortedSortBuffer[sortedIdx + count].key == currentChunkID))
        {
            count++;
        }
        RWChunkBuffer[currentChunkID].objectCount = count;
    }
}