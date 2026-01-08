using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006 // Naming Styles
namespace Prism.Maths
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [DebuggerDisplay("X:{X}, Y:{Y}, Z:{Z}")]
    public struct int3 : IEquatable<int3>
    {
        public static readonly int3 Zero = 0;
        public static readonly int3 One = 1;

        public int X, Y, Z;

        public int3(int val) : this(val, val, val)
        {
        }

        public int3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static implicit operator int3(in VRageMath.Vector3I vec) => Unsafe.As<VRageMath.Vector3I, int3>(ref Unsafe.AsRef(in vec));
        public static implicit operator int3(int val) => new int3(val);

        public static explicit operator int3(in float3 vec) => new int3((int)vec.X, (int)vec.Y, (int)vec.Z);

        public static int3 operator +(in int3 a, in int3 b) => new int3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static int3 operator -(in int3 a, in int3 b) => new int3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static int3 operator *(in int3 a, in int3 b) => new int3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static int3 operator /(in int3 a, in int3 b) => new int3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        public static int3 operator -(in int3 vec) => new int3(-vec.X, -vec.Y, -vec.Z);
        public static int3 operator <<(int3 a, int b) => new int3(a.X << b, a.Y << b, a.Z << b);
        public static int3 operator >>(int3 a, int b) => new int3(a.X >> b, a.Y >> b, a.Z >> b);

        public readonly bool Equals(int3 other) => X == other.X && Y == other.Y && Z == other.Z;
        public static bool operator ==(in int3 a, in int3 b) => a.Equals(b);
        public static bool operator !=(in int3 a, in int3 b) => !a.Equals(b);
        public override readonly bool Equals(object obj) => obj is int3 vec && Equals(vec);
        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);

        public static int3 Min(in int3 a, in int3 b)
        {
            return new int3
            {
                X = Math.Min(a.X, b.X),
                Y = Math.Min(a.Y, b.Y),
                Z = Math.Min(a.Z, b.Z),
            };
        }

        public static int3 Max(in int3 a, in int3 b)
        {
            return new int3
            {
                X = Math.Max(a.X, b.X),
                Y = Math.Max(a.Y, b.Y),
                Z = Math.Max(a.Z, b.Z),
            };
        }

        public static int3 Clamp(in int3 val, in int3 min, in int3 max)
        {
            return new int3
            {
                X = MathHelpers.Clamp(val.X, min.X, max.X),
                Y = MathHelpers.Clamp(val.Y, min.Y, max.Y),
                Z = MathHelpers.Clamp(val.Z, min.Z, max.Z),
            };
        }
    }
}
