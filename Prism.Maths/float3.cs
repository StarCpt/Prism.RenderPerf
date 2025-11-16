using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006 // Naming Styles
namespace Prism.Maths
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct float3 : IEquatable<float3>
    {
        public static readonly float3 Zero = 0;
        public static readonly float3 One = 1;

        public float X, Y, Z;

        public float3(float val) : this(val, val, val)
        {
        }

        public float3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static implicit operator float3(in VRageMath.Vector3 vec) => Unsafe.As<VRageMath.Vector3, float3>(ref Unsafe.AsRef(in vec));
        public static implicit operator float3(in int3 vec) => new float3(vec.X, vec.Y, vec.Z);
        public static implicit operator float3(float val) => new float3(val);

        public static explicit operator float3(in VRageMath.Vector3D vec) => new float3((float)vec.X, (float)vec.Y, (float)vec.Z);

        public static float3 operator +(in float3 a, in float3 b) => new float3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static float3 operator -(in float3 a, in float3 b) => new float3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static float3 operator *(in float3 a, in float3 b) => new float3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static float3 operator /(in float3 a, in float3 b) => new float3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        public static float3 operator -(in float3 vec) => new float3(-vec.X, -vec.Y, -vec.Z);

        public readonly bool Equals(float3 other) => X == other.X && Y == other.Y && Z == other.Z;
        public static bool operator ==(in float3 a, in float3 b) => a.Equals(b);
        public static bool operator !=(in float3 a, in float3 b) => !a.Equals(b);
        public override readonly bool Equals(object obj) => obj is float3 vec && Equals(vec);
        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);

        public static float3 Min(in float3 a, in float3 b)
        {
            return new float3
            {
                X = Math.Min(a.X, b.X),
                Y = Math.Min(a.Y, b.Y),
                Z = Math.Min(a.Z, b.Z),
            };
        }

        public static float3 Max(in float3 a, in float3 b)
        {
            return new float3
            {
                X = Math.Max(a.X, b.X),
                Y = Math.Max(a.Y, b.Y),
                Z = Math.Max(a.Z, b.Z),
            };
        }

        public static float3 Clamp(in float3 vec, in float3 min, in float3 max)
        {
            return new float3
            {
                X = MathHelpers.Clamp(vec.X, min.X, max.X),
                Y = MathHelpers.Clamp(vec.Y, min.Y, max.Y),
                Z = MathHelpers.Clamp(vec.Z, min.Z, max.Z),
            };
        }

        public static float3 Saturate(in float3 vec) => Clamp(vec, Zero, One);

        public static float3 Round(in float3 vec)
        {
            return new float3
            {
                X = (float)Math.Round(vec.X),
                Y = (float)Math.Round(vec.Y),
                Z = (float)Math.Round(vec.Z),
            };
        }

        public static float Dot(in float3 a, in float3 b) => (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);
        public static float3 Normalize(in float3 val) => val / Length(val);
        public static float Length(in float3 val) => (float)Math.Sqrt(LengthSq(val));
        public static float LengthSq(in float3 val) => Dot(val, val);
        public static float Distance(in float3 a, in float3 b) => Length(a - b);
        public static float DistanceSq(in float3 a, in float3 b) => LengthSq(a - b);

        public readonly float3 Clamp(in float3 min, in float3 max) => Clamp(this, min, max);
        public readonly float3 Saturate() => Saturate(this);
        public readonly float3 Round() => Round(this);
        public readonly float3 Normalize() => Normalize(this);
        public readonly float Length() => Length(this);
        public readonly float LengthSq() => LengthSq(this);
    }
}
