#ifndef PHYSICS_COLLIDERS_TYPE_HLSL
#define PHYSICS_COLLIDERS_TYPE_HLSL

#define COLLIDER_NONE       0
#define COLLIDER_BOX        1
#define COLLIDER_SPHERE     2
#define COLLIDER_CAPSULE    3
#define COLLIDER_CYLINDER   4
#define COLLIDER_PLANE      5
#define COLLIDER_MESH       6

#define CONN_STATIC         0
#define CONN_MOVE           1
#define CONN_DEFORMATION    2
#define CONN_DEFORMATION_MOVE 3


struct Sphere_Type
{
    float4 center_radius; // center.xyz + radius
};
struct Capsule_Type
{
    //TODO
};
struct Cylinder_Type
{
    //TODO
};
struct Plane_Type
{
    float4 a0_a3x;
    float4 a1_a3y;
    float4 a2_a3z;
};
struct Box_Type
{
    Plane_Type group0;
    Plane_Type group1;
};

struct Mesh_Type_Points
{
    float4 position_Index;
};
struct Mesh_Type_ConnectionOfPoints
{
    float4 pointID_pointID_next_connectionOfPointsTypes;
};

struct Mesh_Type_ObjectsToChanks
{
    
};
#endif