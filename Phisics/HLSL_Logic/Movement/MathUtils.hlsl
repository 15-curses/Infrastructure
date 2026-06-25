#ifndef MOVEMENT_MATH_UTILS
#define MOVEMENT_MATH_UTILS

float2 CalculateLookAtAngles(float3 from, float3 to)
{
    float3 direction = normalize(to - from);
    float pitch = degrees(atan2(-direction.y, sqrt(direction.x * direction.x + direction.z * direction.z)));
    float yaw = degrees(atan2(direction.x, direction.z));
    return float2(pitch, yaw);
}

#endif