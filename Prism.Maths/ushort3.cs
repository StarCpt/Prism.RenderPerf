using System.Runtime.InteropServices;

#pragma warning disable IDE1006 // Naming Styles
namespace Prism.Maths
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct ushort3
    {
        public static readonly ushort3 Zero = 0;

        public ushort X, Y, Z;

        public ushort3(ushort val) : this(val, val, val) { }
        public ushort3(ushort x, ushort y, ushort z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static implicit operator ushort3(ushort val) => new ushort3(val);
    }
}
