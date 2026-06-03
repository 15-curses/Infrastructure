#ifndef PHYSICS_BUFFERS_HLSL
#define PHYSICS_BUFFERS_HLSL

#include "Structures.hlsl"

RWStructuredBuffer<PhysicsObject> PhysicsObjectBuffer;
RWStructuredBuffer<ObjectAdditionalData> PhysicsAdditionalDataBuffer;
RWStructuredBuffer<int> ActiveObjectIndices;
RWStructuredBuffer<uint> TaskCounter;
RWStructuredBuffer<uint> ActiveObjectCount;

#include "CollidersType.hlsl"

RWStructuredBuffer<Sphere_Type> SphereBuffer;
RWStructuredBuffer<Cylinder_Type> CylinderBuffer;
RWStructuredBuffer<Plane_Type> PlaneBuffer;
RWStructuredBuffer<Box_Type> BoxBuffer;

RWStructuredBuffer<Capsule_Type> CapsuleBuffer;
RWStructuredBuffer<Mesh_Type> MeshBuffer;

#endif
