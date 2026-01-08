using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006 // Naming Styles
namespace Prism.Maths
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [DebuggerDisplay("X:{X}, Y:{Y}, Z:{Z}, W:{W}")]
    public struct int4 : IEquatable<int4>
    {
        public static readonly int4 Zero = 0;
        public static readonly int4 One = 1;

        public int X, Y, Z, W;

        public int4(int val) : this(val, val, val, val)
        {
        }

        public int4(int x, int y, int z, int w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static implicit operator int4(in VRageMath.Vector4I vec) => Unsafe.As<VRageMath.Vector4I, int4>(ref Unsafe.AsRef(in vec));
        public static implicit operator int4(int val) => new int4(val);

        public static explicit operator int4(in float4 vec) => new int4((int)vec.X, (int)vec.Y, (int)vec.Z, (int)vec.W);

        public static int4 operator +(in int4 a, in int4 b) => new int4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        public static int4 operator -(in int4 a, in int4 b) => new int4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        public static int4 operator *(in int4 a, in int4 b) => new int4(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
        public static int4 operator /(in int4 a, in int4 b) => new int4(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.W / b.W);
        public static int4 operator -(in int4 vec) => new int4(-vec.X, -vec.Y, -vec.Z, -vec.W);
        public static int4 operator <<(int4 a, int b) => new int4(a.X << b, a.Y << b, a.Z << b, a.W << b);
        public static int4 operator >>(int4 a, int b) => new int4(a.X >> b, a.Y >> b, a.Z >> b, a.W >> b);

        public readonly bool Equals(int4 other) => X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        public static bool operator ==(in int4 a, in int4 b) => a.Equals(b);
        public static bool operator !=(in int4 a, in int4 b) => !a.Equals(b);
        public override readonly bool Equals(object obj) => obj is int4 vec && Equals(vec);
        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z, W);

        public static int4 Min(in int4 a, in int4 b)
        {
            return new int4
            {
                X = Math.Min(a.X, b.X),
                Y = Math.Min(a.Y, b.Y),
                Z = Math.Min(a.Z, b.Z),
                W = Math.Min(a.W, b.W),
            };
        }

        public static int4 Max(in int4 a, in int4 b)
        {
            return new int4
            {
                X = Math.Max(a.X, b.X),
                Y = Math.Max(a.Y, b.Y),
                Z = Math.Max(a.Z, b.Z),
                W = Math.Max(a.W, b.W),
            };
        }

        public static int4 Clamp(in int4 val, in int4 min, in int4 max)
        {
            return new int4
            {
                X = MathHelpers.Clamp(val.X, min.X, max.X),
                Y = MathHelpers.Clamp(val.Y, min.Y, max.Y),
                Z = MathHelpers.Clamp(val.Z, min.Z, max.Z),
                W = MathHelpers.Clamp(val.W, min.W, max.W),
            };
        }
    }
}
