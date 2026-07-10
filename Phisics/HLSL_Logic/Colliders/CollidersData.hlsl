struct VerticesData
{
     float3 vertices0;
     float3 vertices1;
     float3 vertices2;
     float3 vertices3;
     float3 vertices4;
     float3 vertices5;
     float3 vertices6;
     float3 vertices7;
     float3 vertices8;
     float3 vertices9;
     float3 vertices10;
     float3 vertices11;
     float3 vertices12;
     float3 vertices13;
     float3 vertices14;
     float3 vertices15;
     float3 vertices16;
     float3 vertices17;
     float3 vertices18;
     float3 vertices19;
     float3 vertices20;
     float3 vertices21;
     float3 vertices22;
     float3 vertices23;
     float3 vertices24;
     float3 vertices25;
     float3 vertices26;
     float3 vertices27;
     float3 vertices28;
     float3 vertices29;
     float3 vertices30;
     float3 vertices31;
     float3 vertices32;
     float3 vertices33;
     float3 vertices34;
     float3 vertices35;
};
struct TrianglesData
{
     float3 triangle0;
     float3 triangle1;
     float3 triangle2;
     float3 triangle3;
     float3 triangle4;
     float3 triangle5;
     float3 triangle6;   
     float3 triangle7;
     float3 triangle8;
     float3 triangle9;
     float3 triangle10;
     float3 triangle11;
     float3 triangle12;
     float3 triangle13;
     float3 triangle14;
     float3 triangle15;
     float3 triangle16;
     float3 triangle17;
     float3 triangle18;
     float3 triangle19;
     float3 triangle20;
     float3 triangle21;
     float3 triangle22;
     float3 triangle23;
     float3 triangle24;
     float3 triangle25;
     float3 triangle26;
     float3 triangle27;
     float3 triangle28;
     float3 triangle29;
     float3 triangle30;
     float3 triangle31;
     float3 triangle32;
     float3 triangle33;
     float3 triangle34;
     float3 triangle35;
};
struct MeshIndexerData
{
     uint colliderType;
     float freeInf;
     
     float3 centerAABB;
     float3 cubSize;
     
     uint4 trianglesBufferID0;
     uint4 trianglesBufferID1;
     uint4 trianglesBufferID2;
     uint4 trianglesBufferID3;
     uint4 trianglesBufferID4;
     uint4 trianglesBufferID5;
     uint4 trianglesBufferID6;
     uint4 trianglesBufferID7;
     
     uint4 verticeBufferID0;
     uint4 verticeBufferID1;
     uint4 verticeBufferID2;
     uint4 verticeBufferID3;
     uint4 verticeBufferID4;
     uint4 verticeBufferID5;
};

struct MeshAddIndexerData
{
     float colliderType;
     float freeInf;
     
     float3 centerAABB;
     float3 cubSize;
     
     uint startTrianglIndex;
     uint endTrianglIndex;
     uint startVerticeIndex;
     uint endVerticeIndex;
};
struct Collider_Type
{
     const int None = 0;
     const int Box = 1;
     const int Sphere = 2;
     const int Capsule = 3;
     const int Cylinder = 4;
     const int Plane = 5;
     const int Mesh = 6;
};