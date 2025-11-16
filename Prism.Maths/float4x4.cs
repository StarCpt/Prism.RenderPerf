using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VRageMath;

#pragma warning disable IDE1006 // Naming Styles
namespace Prism.Maths
{
    /// <summary>
    /// A 32-bit floating point matrix with 4 rows and 4 columns.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct float4x4
    {
        public static readonly float4x4 Identity = new float4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);

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

        public float M41;
        public float M42;
        public float M43;
        public float M44;

        public float4x4(
            float m11, float m12, float m13, float m14,
            float m21, float m22, float m23, float m24,
            float m31, float m32, float m33, float m34,
            float m41, float m42, float m43, float m44)
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
            M41 = m41;
            M42 = m42;
            M43 = m43;
            M44 = m44;
        }

        public static implicit operator float4x4(in Matrix mat) => Unsafe.As<Matrix, float4x4>(ref Unsafe.AsRef(in mat));
        public static implicit operator Matrix(in float4x4 mat) => Unsafe.As<float4x4, Matrix>(ref Unsafe.AsRef(in mat));

        public float4x4 Transpose()
        {
            return new float4x4(M11, M21, M31, M41, M12, M22, M32, M42, M13, M23, M33, M43, M14, M24, M34, M44);
        }

        public void TransposeInPlace()
        {
            (M12, M21) = (M21, M12);
            (M13, M31) = (M31, M13);
            (M14, M41) = (M41, M14);

            (M23, M32) = (M32, M23);
            (M24, M42) = (M42, M24);
            (M34, M43) = (M43, M34);
        }

        public float4x4 TransposeRotation()
        {
            return new float4x4(M11, M21, M31, M14, M12, M22, M32, M24, M13, M23, M33, M34, M41, M42, M43, M44);
        }

        public void TransposeRotationInPlace()
        {
            (M12, M21) = (M21, M12);
            (M13, M31) = (M31, M13);
            (M23, M32) = (M32, M23);
        }

        // same as transposing and casting to col_float4x4
        public col_float4x4 ToColumnMajor()
        {
            return new col_float4x4(M11, M21, M31, M41, M12, M22, M32, M42, M13, M23, M33, M43, M14, M24, M34, M44);
        }
    }
}
