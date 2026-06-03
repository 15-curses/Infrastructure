#ifndef PHYSICS_CONTROLLERS_HLSL
#define PHYSICS_CONTROLLERS_HLSL

#include "Structures.hlsl"
#include "Buffers.hlsl"
#include "Constants.hlsl"
#include "MathUtils.hlsl"

void UpdateOrbitController(inout PhysicsObject obj)
{
    if (MouseDelta.z == 0)
    {
        obj.mass_drag_additionalIndex_updateMask.w = 0;
        return;
    }

    MouseDelta.z = 0;

    int additionalDataIndex = (int) obj.mass_drag_additionalIndex_updateMask.z;
    ObjectAdditionalData additionalData = PhysicsAdditionalDataBuffer[additionalDataIndex];

    if (additionalData.data2.w == 0)
        return;

    float2 angle = additionalData.data0.zw;
    float torque = additionalData.data1.x;
    float radius = additionalData.data1.y;
    float3 center = additionalData.data2.xyz;

    float yaw = angle.x + MouseDelta.x * torque;
    float pitch = angle.y + MouseDelta.y * torque;
    pitch = clamp(pitch, -85.0, 85.0);

    additionalData.data0.zw = float2(yaw, pitch);

    float angleXRad = radians(yaw);
    float angleYRad = radians(pitch);
    float cosX, sinX, cosY, sinY;
    sincos(angleXRad, sinX, cosX);
    sincos(angleYRad, sinY, cosY);

    float rCosY = radius * cosY;
    float3 offset = float3(rCosY * sinX, -radius * sinY, -rCosY * cosX);
    obj.position.xyz = center + offset;

    obj.rotation.xy = CalculateLookAtAngles(obj.position.xyz, center);

    obj.mass_drag_additionalIndex_updateMask.w = 12;

    PhysicsAdditionalDataBuffer[additionalDataIndex] = additionalData;
}

void ResetUpdateMask(inout PhysicsObject obj)
{
    obj.mass_drag_additionalIndex_updateMask.w = 0;
}

#endif
