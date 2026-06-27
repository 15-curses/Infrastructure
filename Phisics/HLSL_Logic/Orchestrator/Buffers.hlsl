#ifndef PHYSICS_BUFFERS
#define PHYSICS_BUFFERS

RWStructuredBuffer<IndexOrchestrator> index_orchestrator_buffer;
RWStructuredBuffer<uint> counter;

RWStructuredBuffer<PhysicsObject> PhysicsObjectBuffer;
RWStructuredBuffer<ObjectAdditionalData> PhysicsAdditionalDataBuffer;

RWStructuredBuffer<uint> back_counter_buffer;
RWStructuredBuffer<BackStruct> back_buffer;

RWStructuredBuffer<Chunks> chunks_buffer;

#endif 