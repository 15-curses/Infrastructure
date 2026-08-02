#define GLUE_HELPER(a, b) a##b
#define EVAL_GLUE(a, b) GLUE_HELPER(a, b)

uint EVAL_GLUE(CUR_NAME, MoveBufferCounter);
uint EVAL_GLUE(CUR_NAME, MoveCount);
uint EVAL_GLUE(CUR_NAME, MoveStartID);

[numthreads(32, 1, 1)]
void EVAL_GLUE(CUR_NAME, Move)(uint3 id : SV_DispatchThreadID)
{
    uint index;
    InterlockedAdd(EVAL_GLUE(CUR_NAME, MoveBufferCounter), 1, index);
    
    if (index < EVAL_GLUE(CUR_NAME, MoveCount))
    {
        uint element = MoveBuffer[EVAL_GLUE(CUR_NAME, MoveStartID) + index];
        
        EVAL_GLUE(CUR_NAME, MoveMetod)(element);
    }
}

#undef GLUE_HELPER
#undef EVAL_GLUE
#undef CUR_NAME