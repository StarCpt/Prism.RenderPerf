using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006 // Naming Styles
namespace Prism.Maths
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [DebuggerDisplay("X:{X}, Y:{Y}")]
    public struct float2 : IEquatable<float2>
    {
        public static readonly float2 Zero = 0;
        public static readonly float2 One = 1;

        public float X, Y;

        public float2(float val) : this(val, val)
        {
        }

        public float2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator float2(in VRageMath.Vector2 vec) => Unsafe.As<VRageMath.Vector2, float2>(ref Unsafe.AsRef(in vec));
        public static implicit operator float2(in int2 vec) => new float2(vec.X, vec.Y);
        public static implicit operator float2(float val) => new float2(val);

        public static explicit operator float2(in VRageMath.Vector2D vec) => new float2((float)vec.X, (float)vec.Y);

        public static float2 operator +(in float2 a, in float2 b) => new float2(a.X + b.X, a.Y + b.Y);
        public static float2 operator -(in float2 a, in float2 b) => new float2(a.X - b.X, a.Y - b.Y);
        public static float2 operator *(in float2 a, in float2 b) => new float2(a.X * b.X, a.Y * b.Y);
        public static float2 operator /(in float2 a, in float2 b) => new float2(a.X / b.X, a.Y / b.Y);
        public static float2 operator -(in float2 vec) => new float2(-vec.X, -vec.Y);

        public readonly bool Equals(float2 other) => X == other.X && Y == other.Y;
        public static bool operator ==(in float2 a, in float2 b) => a.Equals(b);
        public static bool operator !=(in float2 a, in float2 b) => !a.Equals(b);
        public override readonly bool Equals(object obj) => obj is float2 vec && Equals(vec);
        public override readonly int GetHashCode() => HashCode.Combine(X, Y);

        public static float2 Min(in float2 a, in float2 b)
        {
            return new float2
            {
                X = Math.Min(a.X, b.X),
                Y = Math.Min(a.Y, b.Y),
            };
        }

        public static float2 Max(in float2 a, in float2 b)
        {
            return new float2
            {
                X = Math.Max(a.X, b.X),
                Y = Math.Max(a.Y, b.Y),
            };
        }

        public static float2 Clamp(in float2 vec, in float2 min, in float2 max)
        {
            return new float2
            {
                X = MathHelpers.Clamp(vec.X, min.X, max.X),
                Y = MathHelpers.Clamp(vec.Y, min.Y, max.Y),
            };
        }

        public static float2 Saturate(in float2 vec) => Clamp(vec, Zero, One);

        public static float2 Round(in float2 vec)
        {
            return new float2
            {
                X = (float)Math.Round(vec.X),
                Y = (float)Math.Round(vec.Y),
            };
        }

        public static float Dot(in float2 a, in float2 b) => (a.X * b.X) + (a.Y * b.Y);
        public static float2 Normalize(in float2 val) => val / Length(val);
        public static float Length(in float2 val) => (float)Math.Sqrt(LengthSq(val));
        public static float LengthSq(in float2 val) => Dot(val, val);
        public static float Distance(in float2 a, in float2 b) => Length(a - b);
        public static float DistanceSq(in float2 a, in float2 b) => LengthSq(a - b);

        public readonly float2 Clamp(in float2 min, in float2 max) => Clamp(this, min, max);
        public readonly float2 Saturate() => Saturate(this);
        public readonly float2 Round() => Round(this);
        public readonly float2 Normalize() => Normalize(this);
        public readonly float Length() => Length(this);
        public readonly float LengthSq() => LengthSq(this);
    }
}
