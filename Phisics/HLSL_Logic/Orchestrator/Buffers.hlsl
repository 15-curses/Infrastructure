#ifndef PHYSICS_BUFFERS
#define PHYSICS_BUFFERS

RWStructuredBuffer<IndexOrchestrator> index_orchestrator_buffer;
RWStructuredBuffer<uint> counter;

RWStructuredBuffer<PhysicsObject> PhysicsObjectBuffer;
RWStructuredBuffer<ObjectAdditionalData> PhysicsAdditionalDataBuffer;

RWStructuredBuffer<uint> back_counter_buffer;
RWStructuredBuffer<BackStruct> back_buffer;

RWStructuredBuffer<uint> chunks_add_counter;
RWStructuredBuffer<Chunk> chunks_buffer;
RWStructuredBuffer<uint> chunks_obj_counter;

RWStructuredBuffer<MeshAddIndexerData> mesh_add_indexer_data;

RWStructuredBuffer<VerticesData> vertices_buffer;
RWStructuredBuffer<TrianglesData> triangles_buffer;
RWStructuredBuffer<MeshIndexerData> mesh_buffer;
RWStructuredBuffer<uint> mesh_buffer_counter; //-1

RWStructuredBuffer<VerticesData> add_vertices_buffer;
RWStructuredBuffer<TrianglesData> add_triangles_buffer;



#endif 