#ifndef MOVEMENT_DATA
#define MOVEMENT_DATA

struct PhysicsObject
{
    float4 id;
    float4 position;
    float4 velocity;
    float4 rotation;
    float4 angularVelocity;
    float4 mass_drag_additionalIndex_updateMask;
};
struct PhysicsObject_
{
    uint colliderType;
    
    uint objBufferIndex;
    uint additionalBufferIndex;
    
    float3 centerAABB;
    float3 cubSize;
    float3 rotation;
    float3 position;
    
    float3 velocity;
    float3 angularVelocity;
    
    float mass;
    float drag;
    float updateMask;
};
struct ObjectAdditionalData
{
    float4 data0;
    float4 data1;
    float4 data2;
    float4 data3;
};

#endif