using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable IDE1006 // Naming Styles
namespace Prism.Maths
{
    /// <summary>
    /// A 32-bit floating point matrix with 4 rows and 3 columns.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct float4x3
    {
        public static readonly float4x3 Identity = new float4x3(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0);

        public float M11;
        public float M12;
        public float M13;

        public float M21;
        public float M22;
        public float M23;

        public float M31;
        public float M32;
        public float M33;

        public float M41;
        public float M42;
        public float M43;

        public float4x3(VRageMath.Matrix matrix)
        {
            M11 = matrix.M11;
            M12 = matrix.M12;
            M13 = matrix.M13;

            M21 = matrix.M21;
            M22 = matrix.M22;
            M23 = matrix.M23;

            M31 = matrix.M31;
            M32 = matrix.M32;
            M33 = matrix.M33;

            M41 = matrix.M41;
            M42 = matrix.M42;
            M43 = matrix.M43;
        }

        public float4x3(
            float m11, float m12, float m13,
            float m21, float m22, float m23,
            float m31, float m32, float m33,
            float m41, float m42, float m43)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M31 = m31;
            M32 = m32;
            M33 = m33;
            M41 = m41;
            M42 = m42;
            M43 = m43;
        }

        public float4x3 Multiply(ref VRageMath.Matrix matrix2)
        {
            // assume [M14, M24, M34, M44] are [0, 0, 0, 1] respectively

            float m11 = M11 * matrix2.M11 + M12 * matrix2.M21 + M13 * matrix2.M31;
            float m12 = M11 * matrix2.M12 + M12 * matrix2.M22 + M13 * matrix2.M32;
            float m13 = M11 * matrix2.M13 + M12 * matrix2.M23 + M13 * matrix2.M33;
            float m21 = M21 * matrix2.M11 + M22 * matrix2.M21 + M23 * matrix2.M31;
            float m22 = M21 * matrix2.M12 + M22 * matrix2.M22 + M23 * matrix2.M32;
            float m23 = M21 * matrix2.M13 + M22 * matrix2.M23 + M23 * matrix2.M33;
            float m31 = M31 * matrix2.M11 + M32 * matrix2.M21 + M33 * matrix2.M31;
            float m32 = M31 * matrix2.M12 + M32 * matrix2.M22 + M33 * matrix2.M32;
            float m33 = M31 * matrix2.M13 + M32 * matrix2.M23 + M33 * matrix2.M33;
            float m41 = M41 * matrix2.M11 + M42 * matrix2.M21 + M43 * matrix2.M31 + matrix2.M41;
            float m42 = M41 * matrix2.M12 + M42 * matrix2.M22 + M43 * matrix2.M32 + matrix2.M42;
            float m43 = M41 * matrix2.M13 + M42 * matrix2.M23 + M43 * matrix2.M33 + matrix2.M43;

            return new float4x3(m11, m12, m13, m21, m22, m23, m31, m32, m33, m41, m42, m43);
        }

        public float3x4 MultiplyTranspose(ref VRageMath.Matrix matrix2)
        {
            float m11 = M11 * matrix2.M11 + M12 * matrix2.M21 + M13 * matrix2.M31;
            float m12 = M11 * matrix2.M12 + M12 * matrix2.M22 + M13 * matrix2.M32;
            float m13 = M11 * matrix2.M13 + M12 * matrix2.M23 + M13 * matrix2.M33;
            float m21 = M21 * matrix2.M11 + M22 * matrix2.M21 + M23 * matrix2.M31;
            float m22 = M21 * matrix2.M12 + M22 * matrix2.M22 + M23 * matrix2.M32;
            float m23 = M21 * matrix2.M13 + M22 * matrix2.M23 + M23 * matrix2.M33;
            float m31 = M31 * matrix2.M11 + M32 * matrix2.M21 + M33 * matrix2.M31;
            float m32 = M31 * matrix2.M12 + M32 * matrix2.M22 + M33 * matrix2.M32;
            float m33 = M31 * matrix2.M13 + M32 * matrix2.M23 + M33 * matrix2.M33;
            float m41 = M41 * matrix2.M11 + M42 * matrix2.M21 + M43 * matrix2.M31 + matrix2.M41;
            float m42 = M41 * matrix2.M12 + M42 * matrix2.M22 + M43 * matrix2.M32 + matrix2.M42;
            float m43 = M41 * matrix2.M13 + M42 * matrix2.M23 + M43 * matrix2.M33 + matrix2.M43;

            return new float3x4(m11, m21, m31, m41, m12, m22, m32, m42, m13, m23, m33, m43);
        }

        public float3x4 Transpose()
        {
            return new float3x4(M11, M21, M31, M41, M12, M22, M32, M42, M13, M23, M33, M43);
        }
    }
}
