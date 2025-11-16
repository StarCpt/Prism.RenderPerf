using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006 // Naming Styles
namespace Prism.Maths
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct float4 : IEquatable<float4>
    {
        public static readonly float4 Zero = 0;
        public static readonly float4 One = 1;

        public float X, Y, Z, W;

        public float4(float val) : this(val, val, val, val)
        {
        }

        public float4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static implicit operator float4(in VRageMath.Vector4 vec) => Unsafe.As<VRageMath.Vector4, float4>(ref Unsafe.AsRef(in vec));
        public static implicit operator float4(in int4 vec) => new float4(vec.X, vec.Y, vec.Z, vec.W);
        public static implicit operator float4(float val) => new float4(val);

        public static explicit operator float4(in VRageMath.Vector4D vec) => new float4((float)vec.X, (float)vec.Y, (float)vec.Z, (float)vec.W);

        public static float4 operator +(in float4 a, in float4 b) => new float4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        public static float4 operator -(in float4 a, in float4 b) => new float4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        public static float4 operator *(in float4 a, in float4 b) => new float4(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
        public static float4 operator /(in float4 a, in float4 b) => new float4(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.W / b.W);
        public static float4 operator -(in float4 vec) => new float4(-vec.X, -vec.Y, -vec.Z, -vec.W);

        public readonly bool Equals(float4 other) => X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        public static bool operator ==(in float4 a, in float4 b) => a.Equals(b);
        public static bool operator !=(in float4 a, in float4 b) => !a.Equals(b);
        public override readonly bool Equals(object obj) => obj is float4 vec && Equals(vec);
        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z, W);

        public static float4 Min(in float4 a, in float4 b)
        {
            return new float4
            {
                X = Math.Min(a.X, b.X),
                Y = Math.Min(a.Y, b.Y),
                Z = Math.Min(a.Z, b.Z),
                W = Math.Min(a.W, b.W),
            };
        }

        public static float4 Max(in float4 a, in float4 b)
        {
            return new float4
            {
                X = Math.Max(a.X, b.X),
                Y = Math.Max(a.Y, b.Y),
                Z = Math.Max(a.Z, b.Z),
                W = Math.Max(a.W, b.W),
            };
        }

        public static float4 Clamp(in float4 vec, in float4 min, in float4 max)
        {
            return new float4
            {
                X = MathHelpers.Clamp(vec.X, min.X, max.X),
                Y = MathHelpers.Clamp(vec.Y, min.Y, max.Y),
                Z = MathHelpers.Clamp(vec.Z, min.Z, max.Z),
                W = MathHelpers.Clamp(vec.W, min.W, max.W),
            };
        }

        public static float4 Saturate(in float4 vec) => Clamp(vec, Zero, One);

        public static float4 Round(in float4 vec)
        {
            return new float4
            {
                X = (float)Math.Round(vec.X),
                Y = (float)Math.Round(vec.Y),
                Z = (float)Math.Round(vec.Z),
                W = (float)Math.Round(vec.W),
            };
        }

        public static float Dot(in float4 a, in float4 b) => (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z) + (a.W * b.W);
        public static float4 Normalize(in float4 val) => val / Length(val);
        public static float Length(in float4 val) => (float)Math.Sqrt(LengthSq(val));
        public static float LengthSq(in float4 val) => Dot(val, val);
        public static float Distance(in float4 a, in float4 b) => Length(a - b);
        public static float DistanceSq(in float4 a, in float4 b) => LengthSq(a - b);

        public readonly float4 Clamp(in float4 min, in float4 max) => Clamp(this, min, max);
        public readonly float4 Saturate() => Saturate(this);
        public readonly float4 Round() => Round(this);
        public readonly float4 Normalize() => Normalize(this);
        public readonly float Length() => Length(this);
        public readonly float LengthSq() => LengthSq(this);
    }
}
