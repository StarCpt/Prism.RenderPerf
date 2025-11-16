using System.Runtime.InteropServices;

#pragma warning disable IDE1006 // Naming Styles
namespace Prism.Maths
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct ushort4
    {
        public static readonly ushort4 Zero = 0;

        public ushort X, Y, Z, W;

        public ushort4(ushort val) : this(val, val, val, val) { }
        public ushort4(ushort x, ushort y, ushort z, ushort w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static implicit operator ushort4(ushort val) => new ushort4(val);
    }
}
