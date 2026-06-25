#ifndef READ_OPERATION
#define READ_OPERATION

void UnpackBinaryWithGroup(uint packedValue, out uint indexInBuffer, out uint operationID)
{
    operationID = (packedValue >> 24) & 0xFFu;
    
    uint binaryDecimal = packedValue & 0xFFFFFFu;
    
    indexInBuffer = 0u;
    for (int i = 0; i < 24; i++)
    {
        uint digit = (binaryDecimal / pow(10, 23 - i)) % 10u;
        indexInBuffer = (indexInBuffer << 1) | digit;
    }
}

void ReadOperation(uint packedValue)
{
    uint indexInBuffer;
    uint operationID;
    
    UnpackBinaryWithGroup(packedValue, indexInBuffer, operationID);
    
    switch (operationID)
    {
        case None: break;
        case Movement: Move(indexInBuffer); break;
        case AddMove: break;
    }
}

#endif