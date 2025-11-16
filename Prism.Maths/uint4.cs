using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006 // Naming Styles
namespace Prism.Maths
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct uint4
    {
        public static readonly uint4 Zero = 0;

        public uint X;
        public uint Y;
        public uint Z;
        public uint W;

        public uint4(uint val) : this(val, val, val, val)
        {
        }

        public uint4(uint x, uint y, uint z, uint w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static implicit operator uint4(uint val) => new uint4(val);

        public static implicit operator int4(in uint4 vec) => new int4((int)vec.X, (int)vec.Y, (int)vec.Z, (int)vec.W);
        public static implicit operator float4(in uint4 vec) => new float4(vec.X, vec.Y, vec.Z, vec.W);

        public static uint4 Min(uint4 a, uint4 b)
        {
            return new uint4
            {
                X = Math.Min(a.X, b.X),
                Y = Math.Min(a.Y, b.Y),
                Z = Math.Min(a.Z, b.Z),
                W = Math.Min(a.W, b.W),
            };
        }

        public static uint4 Max(uint4 a, uint4 b)
        {
            return new uint4
            {
                X = Math.Max(a.X, b.X),
                Y = Math.Max(a.Y, b.Y),
                Z = Math.Max(a.Z, b.Z),
                W = Math.Max(a.W, b.W),
            };
        }
    }
}
