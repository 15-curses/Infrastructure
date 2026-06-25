#ifndef PHYSICS_MOVE_LOGIC0
#define PHYSICS_MOVE_LOGIC0

void Move(uint segmentIndex)
{
    PhysicsObject physicsObject = PhysicsObjectBuffer[segmentIndex];

    for (int i = 0; i < 4; i++)
    {
        switch (physicsObject.id[i])
        {
            case 0: break;
            case 1: Orbit(physicsObject); break;
            case 3: ApplyGravity(physicsObject); break;
            default: ResetUpdateMask(physicsObject); break;
        }
    }
    PhysicsObjectBuffer[segmentIndex] = physicsObject;
}

#endif