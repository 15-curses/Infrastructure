#ifndef PHYSICS_MOVE_VARIANTS
#define PHYSICS_MOVE_VARIANTS

void ResetUpdateMask(inout Object obj) { obj.updateMask = 0; }

void ApplyGravity(inout Object obj)
{
    obj.velocity += Gravity * DeltaTime;
}

void OrbitMoveMetod(uint objId)
{
    Object obj = ObjectBuffer[objId];
    
    if (MouseDelta.z == 0) return;
    
    MouseDelta.z = 0;
    
    uint additionalDataIndex = obj.additionalBufferIndex;
    AdditionalData additionalData = AdditionalDataBuffer[additionalDataIndex];
    
    if (additionalData.data2.w == 0) return;
    
    float2 angle = additionalData.data0.zw;
    float torque = additionalData.data1.x;
    float radius = additionalData.data1.y;
    float3 center = additionalData.data2.xyz;
    
    float yaw = angle.x + MouseDelta.x * torque;
    float pitch = angle.y + MouseDelta.y * torque;
    pitch = clamp(pitch, -85.0f, 85.0f);

    additionalData.data0.zw = float2(yaw, pitch);
    
    float angleXRad = radians(yaw);
    float angleYRad = radians(pitch);
    float cosX, sinX, cosY, sinY;
    sincos(angleXRad, sinX, cosX);
    sincos(angleYRad, sinY, cosY);

    float rCosY = radius * cosY;
    float3 offset = float3(rCosY * sinX, -radius * sinY, -rCosY * cosX);
    
    obj.position = center + offset;
    
    obj.rotation.xy = CalculateLookAtAngles(obj.position, center);
    obj.rotation.z = 0.0f; 
    
    obj.updateMask = 12.0f;
    
    AdditionalDataBuffer[additionalDataIndex] = additionalData;
    ObjectBuffer[objId] = obj;
}

#endif