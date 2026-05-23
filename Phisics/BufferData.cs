using Unity.Mathematics;

namespace Assets.Infrastructure.Phisics
{
    public struct MainBufferData
    {
        public float4 Id;
        public float4 Position;
        public float4 Velocity;
        public float4 Rotation;
        public float4 AngularVelocity;
        public float4 MassDragAdditionalFlags;
    }
    // Константы для битовых масок еу
    public static class UpdateMasks
    {
        public const int None = 0;
        public const int AngularVelocity = 1 << 0;  // 1
        public const int Velocity = 1 << 1;         // 2
        public const int Rotation = 1 << 2;         // 4
        public const int Position = 1 << 3;         // 8
        public const int MassData = 1 << 4;         // 16

        public const int All = Position | Rotation | Velocity | AngularVelocity | MassData; // 31
        public const int AllWithoutMass = Position | Rotation | Velocity | AngularVelocity; // 15
        public const int PositionAndVelocity = Position | Velocity; // 10
        public const int PositionAndRotation = Position | Rotation; // 12
        public const int RotationAndVelocity = Rotation | Velocity; // 6

        public static bool HasPosition(int mask) => (mask & Position) != 0;
        public static bool HasRotation(int mask) => (mask & Rotation) != 0;
        public static bool HasVelocity(int mask) => (mask & Velocity) != 0;
        public static bool HasAngularVelocity(int mask) => (mask & AngularVelocity) != 0;
        public static bool HasMassData(int mask) => (mask & MassData) != 0;
    }
    public struct AdditionalBufferData
    {
        public float4 Data0;
        public float4 Data1;
        public float4 Data2;
        public float4 Data3;
    }
}
