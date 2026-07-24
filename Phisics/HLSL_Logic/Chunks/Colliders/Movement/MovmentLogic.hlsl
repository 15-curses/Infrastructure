#ifndef PHYSICS_MOVE_LOGIC0
#define PHYSICS_MOVE_LOGIC0

void Move(uint segmentIndex)
{
    PhysicsObject physicsObject = add_physics_object_buffer[segmentIndex];
    
    for (int i = 0; i < 4; i++)
    {
        switch (physicsObject.objBufferIndex[i])
        {
            case 0: break;
            case 1: Orbit(physicsObject); break;
            case 3: ApplyGravity(physicsObject); break;
            default: ResetUpdateMask(physicsObject); break;
        }
    }
    physics_object_buffer[segmentIndex] = physicsObject;
}

#endif