using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006 // Naming Styles
namespace Prism.Maths
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct ushort2
    {
        public static readonly ushort2 Zero = 0;

        public ushort X, Y;

        public ushort2(ushort val) : this(val, val) { }
        public ushort2(ushort x, ushort y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator ushort2(ushort val) => new ushort2(val);

        public uint ToUint() => Unsafe.As<ushort2, uint>(ref this);
    }
}
