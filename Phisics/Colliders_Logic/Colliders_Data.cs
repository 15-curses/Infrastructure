using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;

namespace Assets.Infrastructure.Phisics.Colliders_Logic
{ 
    public struct VerticesData
    {
        public float3 vertices0;
        public float3 vertices1;
        public float3 vertices2;

        public float3 vertices3;
        public float3 vertices4;
        public float3 vertices5;

        public float3 vertices6;
        public float3 vertices7;
        public float3 vertices8;

        public float3 vertices9;
        public float3 vertices10;
        public float3 vertices11;

        public float3 vertices12;
        public float3 vertices13;
        public float3 vertices14;

        public float3 vertices15;
        public float3 vertices16;
        public float3 vertices17;

        public float3 vertices18;
        public float3 vertices19;
        public float3 vertices20;

        public float3 vertices21;
        public float3 vertices22;
        public float3 vertices23;

        public float3 vertices24;
        public float3 vertices25;
        public float3 vertices26;

        public float3 vertices27;
        public float3 vertices28;
        public float3 vertices29;

        public float3 vertices30;
        public float3 vertices31;
        public float3 vertices32;

        public float3 vertices33;
        public float3 vertices34;
        public float3 vertices35;
    }
    public struct TrianglesData
    {
        public float3 triangle0;
        public float3 triangle1;
        public float3 triangle2;
        public float3 triangle3;
        public float3 triangle4;
        public float3 triangle5;
        public float3 triangle6;   
        public float3 triangle7;
        public float3 triangle8;
        public float3 triangle9;
        public float3 triangle10;
        public float3 triangle11;
        public float3 triangle12;
        public float3 triangle13;
        public float3 triangle14;
        public float3 triangle15;
        public float3 triangle16;
        public float3 triangle17;
        public float3 triangle18;
        public float3 triangle19;
        public float3 triangle20;
        public float3 triangle21;
        public float3 triangle22;
        public float3 triangle23;
        public float3 triangle24;
        public float3 triangle25;
        public float3 triangle26;
        public float3 triangle27;
        public float3 triangle28;
        public float3 triangle29;
        public float3 triangle30;
        public float3 triangle31;
        public float3 triangle32;
        public float3 triangle33;
        public float3 triangle34;
        public float3 triangle35;
    }
    public struct MeshIndexerData
    {
        public float colliderType;
        public float3 trianglesBufferID0;
        public float4 trianglesBufferID1;
        public float4 trianglesBufferID2;
        public float4 trianglesBufferID3;
        public float4 trianglesBufferID4;
        public float4 trianglesBufferID5;
        public float4 trianglesBufferID6;
        public float4 trianglesBufferID7;
    };
    public struct MeshAddIndexerData
    {
        public float startTrianglIndex;
        public float endTrianglIndex;
        public float startVerticeIndex;
        public float endVerticeIndex;
    }
    public struct Collider_Type
    {
        public const int None = 0;
        public const int Box = 1;
        public const int Sphere = 2;
        public const int Capsule = 3;
        public const int Cylinder = 4;
        public const int Plane = 5;
        public const int Mesh = 6;
    };
}
