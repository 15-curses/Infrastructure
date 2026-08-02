#ifndef PHYSICS_BACK_INF_TO_CPU_BACK_LOGIC_H
#define PHYSICS_BACK_INF_TO_CPU_BACK_LOGIC_H

uint PackBinaryWithGroup(uint indexInBuffer, uint operationID)
{
    uint binaryDecimal = 0;
    for(int i = 0; i < 24; i++)
        binaryDecimal = binaryDecimal * 10 + ((indexInBuffer >> (23 - i)) & 1u);
    
    uint result = (operationID << 24) | binaryDecimal;
    
    return result;
}

int AddIndex(uint _indexInBuffer, uint _operationID)
{
    uint indexInBuffer = PackBinaryWithGroup(_indexInBuffer, _operationID);
    uint indexInStorage;
    InterlockedAdd(back_counter_buffer[0], 1, indexInStorage);
    
    int indexInThisBuffer = indexInStorage / 4;
    uint slot = indexInBuffer % 4;
    
    BackStruct newBuffer = back_buffer[indexInThisBuffer];
    
    switch(slot)
    {
        case 0: newBuffer.index0 = indexInBuffer; break;
        case 1: newBuffer.index1 = indexInBuffer; break;
        case 2: newBuffer.index2 = indexInBuffer; break;
        case 3: newBuffer.index3 = indexInBuffer; break;
    }
    
    back_buffer[indexInThisBuffer] = newBuffer;
    return (int)indexInStorage;
}

#endif
