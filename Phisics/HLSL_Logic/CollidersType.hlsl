#ifndef PHYSICS_COLLIDERS_TYPE_HLSL
#define PHYSICS_COLLIDERS_TYPE_HLSL

struct Collider_Type
{
    static const int None = 0;
    static const int Box = 1;
    static const int Sphere = 2;
    static const int Capsule = 3;
    static const int Cylinder = 4;
    static const int Plane = 5;
    static const int Mesh = 6;
};
struct Sphere_Type
{
    float4 center_radius; // center.xyz + radius
};
struct Capsule_Type
{
    float4 p0_radius; // точка 0 (начало): p0.xyz + radius
    float4 p1_height; // точка 1 (конец): p1.xyz + height (или просто p1)
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
struct Mesh_Type
{
    //TODO
};

#endif