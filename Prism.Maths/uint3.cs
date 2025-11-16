using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006 // Naming Styles
namespace Prism.Maths
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct uint3
    {
        public static readonly uint3 Zero = 0;

        public uint X;
        public uint Y;
        public uint Z;

        public uint3(uint val) : this(val, val, val)
        {
        }

        public uint3(uint x, uint y, uint z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static implicit operator uint3(uint val) => new uint3(val);

        public static implicit operator int3(in uint3 vec) => new int3((int)vec.X, (int)vec.Y, (int)vec.Z);
        public static implicit operator float3(in uint3 vec) => new float3(vec.X, vec.Y, vec.Z);

        public static uint3 Min(uint3 a, uint3 b)
        {
            return new uint3
            {
                X = Math.Min(a.X, b.X),
                Y = Math.Min(a.Y, b.Y),
                Z = Math.Min(a.Z, b.Z),
            };
        }

        public static uint3 Max(uint3 a, uint3 b)
        {
            return new uint3
            {
                X = Math.Max(a.X, b.X),
                Y = Math.Max(a.Y, b.Y),
                Z = Math.Max(a.Z, b.Z),
            };
        }
    }
}
