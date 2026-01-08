using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006 // Naming Styles
namespace Prism.Maths
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [DebuggerDisplay("X:{X}, Y:{Y}")]
    public struct int2 : IEquatable<int2>
    {
        public static readonly int2 Zero = 0;
        public static readonly int2 One = 1;

        public int X, Y;

        public int2(int val) : this(val, val)
        {
        }

        public int2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator int2(in VRageMath.Vector2I vec) => Unsafe.As<VRageMath.Vector2I, int2>(ref Unsafe.AsRef(in vec));
        public static implicit operator int2(int val) => new int2(val);

        public static explicit operator int2(in float2 vec) => new int2((int)vec.X, (int)vec.Y);

        public static int2 operator +(in int2 a, in int2 b) => new int2(a.X + b.X, a.Y + b.Y);
        public static int2 operator -(in int2 a, in int2 b) => new int2(a.X - b.X, a.Y - b.Y);
        public static int2 operator *(in int2 a, in int2 b) => new int2(a.X * b.X, a.Y * b.Y);
        public static int2 operator /(in int2 a, in int2 b) => new int2(a.X / b.X, a.Y / b.Y);
        public static int2 operator -(in int2 vec) => new int2(-vec.X, -vec.Y);
        public static int2 operator <<(int2 a, int b) => new int2(a.X << b, a.Y << b);
        public static int2 operator >>(int2 a, int b) => new int2(a.X >> b, a.Y >> b);

        public readonly bool Equals(int2 other) => X == other.X && Y == other.Y;
        public static bool operator ==(in int2 a, in int2 b) => a.Equals(b);
        public static bool operator !=(in int2 a, in int2 b) => !a.Equals(b);
        public override readonly bool Equals(object obj) => obj is int2 vec && Equals(vec);
        public override readonly int GetHashCode() => HashCode.Combine(X, Y);

        public static int2 Min(in int2 a, in int2 b)
        {
            return new int2
            {
                X = Math.Min(a.X, b.X),
                Y = Math.Min(a.Y, b.Y),
            };
        }

        public static int2 Max(in int2 a, in int2 b)
        {
            return new int2
            {
                X = Math.Max(a.X, b.X),
                Y = Math.Max(a.Y, b.Y),
            };
        }

        public static int2 Clamp(in int2 val, in int2 min, in int2 max)
        {
            return new int2
            {
                X = MathHelpers.Clamp(val.X, min.X, max.X),
                Y = MathHelpers.Clamp(val.Y, min.Y, max.Y),
            };
        }
    }
}
