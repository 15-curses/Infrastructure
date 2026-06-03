#ifndef PHYSICS_STRUCTURES_HLSL
#define PHYSICS_STRUCTURES_HLSL

struct PhysicsObject
{
    float4 id;
    float4 position;
    float4 velocity;
    float4 rotation;
    float4 angularVelocity;
    float4 mass_drag_additionalIndex_updateMask;
};

struct ObjectAdditionalData
{
    float4 data0;
    float4 data1;
    float4 data2;
    float4 data3;
};

#endif
