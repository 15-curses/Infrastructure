//==============================================================================
//  COMMON
//==============================================================================
#pragma region Common

    float3  chunksCount;
    float   chunk_Size;

    int3    chunkOffset;
    uint    maxChunksIDsInObject = 12;
    uint    maxAdditionalObjBufferIndex = 9;
    uint    objInChunks = 36;

    uint    orchestrator_buffer_size;

    float   DeltaTime;
    float3  Gravity;
    float3  MouseDelta;

#pragma endregion


//==============================================================================
//  CHUNK
//==============================================================================
#pragma region Chunk

struct Chunk
{
    float3 min_xyz;
    float3 max_xyz;
    
    uint x_plus_chunkId; uint x_min_chunkId;
    uint y_plus_chunkId; uint y_min_chunkId;
    uint z_plus_chunkId; uint z_min_chunkId;
    
    uint firstObjectSortedIndex; 
    uint objectCount;            
};

#pragma endregion


//==============================================================================
//  CPU READBACK
//==============================================================================
#pragma region CPU ReadBack

    struct BackStruct
    {
        uint index0;
        uint index1;
        uint index2;
        uint index3;
    };

#pragma endregion


//==============================================================================
//  OBJECT SYSTEM
//==============================================================================
#pragma region Object System

    //--------------------------------------------------------------------------
    // Collider Types
    //--------------------------------------------------------------------------
    #pragma region Collider Types

        #define DECLARE_COLLIDER_TYPE(name, num) static const uint ColliderType_##name = num;

        DECLARE_COLLIDER_TYPE(None,     0)
        DECLARE_COLLIDER_TYPE(Box,      1)
        DECLARE_COLLIDER_TYPE(Sphere,   2)
        DECLARE_COLLIDER_TYPE(Capsule,  3)
        DECLARE_COLLIDER_TYPE(Cylinder, 4)
        DECLARE_COLLIDER_TYPE(Plane,    5)
        DECLARE_COLLIDER_TYPE(Mesh,     6)

    #pragma endregion


    //--------------------------------------------------------------------------
    // Object
    //--------------------------------------------------------------------------
    #pragma region Object

        struct Object ////////////////////////////////////////////////////////////////////////////
        {
            float4x4    transform;
            float3      massCenter; 
            float3      minAABB; 
            float3      maxAABB;
            float3      rotation;   
            float3      position;
            float3      velocity;   
            float3      angularVelocity;

            uint     colliderType;
            uint     objBufferIndex;                                       
            uint     additionalBufferIndex;                                    
            
            float    mass;                                                      
            float    staticFriction;                                      
            float    dynamicFriction;                       
            float    updateMask;                  
        };

        struct AdditionalData
        {
            float4 data0;
            float4 data1;
            float4 data2;
            float4 data3;
        };

    #pragma endregion


    //--------------------------------------------------------------------------
    // Colliders
    //--------------------------------------------------------------------------
    #pragma region Colliders

        #pragma region Mesh

            struct Mesh
            {
                uint4 verticesBuffer[6];
                uint  vertexCount;                  // TODO: проверить размерность
                uint  chunksIDs[maxChunksIDsInObject];
            };

            struct MeshVertices
            {
                float3 vertices[36];
            };

        #pragma endregion

        #pragma region Sphere
            // TODO: Sphere collider data
        #pragma endregion

    #pragma endregion


    //--------------------------------------------------------------------------
    // Helpers
    //--------------------------------------------------------------------------
    #pragma region Helpers

        struct AddToBuffer
        {
            uint objectIndex;
            bool addStatus;
        };

    #pragma endregion

#pragma endregion // Object System


//==============================================================================
//  MOVEMENT
//==============================================================================
#pragma region Movement

    struct Move
    {
        uint moveType;
        uint objectID;
    };

    #define DECLARE_MOVE_TYPE(name, num) static const uint MoveType_##name = num;

    DECLARE_MOVE_TYPE(Orbit, 0);

#pragma endregion


//==============================================================================
//  TOUCH CHECK
//==============================================================================
#pragma region TOUCH CHECK

    static const uint None        = 0;
    static const uint Movement    = 1;
    static const uint AddMovement = 2;

#pragma endregion

