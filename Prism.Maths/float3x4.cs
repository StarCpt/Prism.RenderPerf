using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006 // Naming Styles
namespace Prism.Maths
{
    /// <summary>
    /// A 32-bit floating point matrix with 3 rows and 4 columns.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct float3x4
    {
        public static readonly float3x4 Identity = new float3x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0);

        public float M11;
        public float M12;
        public float M13;
        public float M14;

        public float M21;
        public float M22;
        public float M23;
        public float M24;

        public float M31;
        public float M32;
        public float M33;
        public float M34;

        public float3x4(
            float m11, float m12, float m13, float m14,
            float m21, float m22, float m23, float m24,
            float m31, float m32, float m33, float m34)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M14 = m14;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M24 = m24;
            M31 = m31;
            M32 = m32;
            M33 = m33;
            M34 = m34;
        }

        public static explicit operator float3x4(in VRageMath.Matrix mat) => Unsafe.As<VRageMath.Matrix, float3x4>(ref Unsafe.AsRef(in mat));
        public static explicit operator float3x4(in VRageMath.MatrixD mat)
        {
            return new float3x4
            {
                M11 = (float)mat.M11,
                M12 = (float)mat.M12,
                M13 = (float)mat.M13,
                M14 = (float)mat.M14,

                M21 = (float)mat.M21,
                M22 = (float)mat.M22,
                M23 = (float)mat.M23,
                M24 = (float)mat.M24,

                M31 = (float)mat.M31,
                M32 = (float)mat.M32,
                M33 = (float)mat.M33,
                M34 = (float)mat.M34,
            };
        }
    }
}
