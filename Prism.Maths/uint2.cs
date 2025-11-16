using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006 // Naming Styles
namespace Prism.Maths
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct uint2
    {
        public static readonly uint2 Zero = 0;

        public uint X;
        public uint Y;

        public uint2(uint val) : this(val, val)
        {
        }

        public uint2(uint x, uint y)
        {
            X = x;
            Y = y;
        }
        
        public static implicit operator uint2(uint val) => new uint2(val);

        public static implicit operator int2(in uint2 vec) => new int2((int)vec.X, (int)vec.Y);
        public static implicit operator float2(in uint2 vec) => new float2(vec.X, vec.Y);

        public static uint2 Min(uint2 a, uint2 b)
        {
            return new uint2
            {
                X = Math.Min(a.X, b.X),
                Y = Math.Min(a.Y, b.Y),
            };
        }

        public static uint2 Max(uint2 a, uint2 b)
        {
            return new uint2
            {
                X = Math.Max(a.X, b.X),
                Y = Math.Max(a.Y, b.Y),
            };
        }
    }
}
