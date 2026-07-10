void AddFreeIndex(uint index, RWStructuredBuffer<uint> FreeIndices,
    RWStructuredBuffer<uint>FreeIndexCount)
{
    uint count;
    InterlockedAdd(FreeIndexCount[0], 1, count);
    FreeIndices[count] = index;
}

bool GetFreeIndex(out uint index, RWStructuredBuffer<uint> FreeIndices,
    RWStructuredBuffer<uint>FreeIndexCount)
{
    uint count;
    InterlockedAdd(FreeIndexCount[0], -1, count);
    if (count > 0)
    {
        index = FreeIndices[count - 1];
        return true;
    }
    return false;
}