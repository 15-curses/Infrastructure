#ifndef PHYSICS_FORCES_HLSL
#define PHYSICS_FORCES_HLSL

#include "Structures.hlsl"
#include "Constants.hlsl"

void ApplyGravity(inout PhysicsObject obj)
{
    obj.velocity.y += Gravity.y * DeltaTime;
    obj.position.y += obj.velocity.y * DeltaTime;
    obj.position.w = 1;
    obj.velocity.w = 1;
}

#endif
