#define DECLARE_COUNTER(CounterName, Buffer) \
uint Increment##CounterName() \
{ \
    uint originalValue; \
    InterlockedAdd(Buffer[0], 1, originalValue); \
    return originalValue; \
} \
uint Decrement##CounterName() \
{ \
    uint originalValue; \
    InterlockedAdd(Buffer[0], -1, originalValue); \
    return originalValue; \
}
DECLARE_COUNTER(AddMove, addMoveCounterBuffer)
// -- IncrementAddMove() DecrementAddMove()

DECLARE_COUNTER(Move, moveCounterBuffer)
// -- IncrementMove() DecrementMove()