using System.Runtime.InteropServices;
using VRageMath;

#pragma warning disable IDE1006 // Naming Styles
namespace Prism.Maths
{
    // column-major float4x4 matrix
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct col_float4x4
    {
        public static readonly col_float4x4 Identity = new col_float4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);

        public float M11;
        public float M21;
        public float M31;
        public float M41;

        public float M12;
        public float M22;
        public float M32;
        public float M42;

        public float M13;
        public float M23;
        public float M33;
        public float M43;

        public float M14;
        public float M24;
        public float M34;
        public float M44;

        public col_float4x4(
            float m11, float m21, float m31, float m41,
            float m12, float m22, float m32, float m42,
            float m13, float m23, float m33, float m43,
            float m14, float m24, float m34, float m44)
        {
            M11 = m11;
            M21 = m21;
            M31 = m31;
            M41 = m41;
            M12 = m12;
            M22 = m22;
            M32 = m32;
            M42 = m42;
            M13 = m13;
            M23 = m23;
            M33 = m33;
            M43 = m43;
            M14 = m14;
            M24 = m24;
            M34 = m34;
            M44 = m44;
        }

        public col_float4x4(Matrix matrix)
        {
            M11 = matrix.M11;
            M21 = matrix.M21;
            M31 = matrix.M31;
            M41 = matrix.M41;
            M12 = matrix.M12;
            M22 = matrix.M22;
            M32 = matrix.M32;
            M42 = matrix.M42;
            M13 = matrix.M13;
            M23 = matrix.M23;
            M33 = matrix.M33;
            M43 = matrix.M43;
            M14 = matrix.M14;
            M24 = matrix.M24;
            M34 = matrix.M34;
            M44 = matrix.M44;
        }

        public col_float4x4 Transpose()
        {
            return new col_float4x4(M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44);
        }

        // same as transposing and casting to float4x4
        public float4x4 ToRowMajor()
        {
            return new float4x4(M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44);
        }
    }
}
