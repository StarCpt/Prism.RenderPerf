using Prism.Maths;
using System.Runtime.InteropServices;
using VRageMath.PackedVector;

namespace Prism.Vanilla.Billboard.ShaderTypes;

[StructLayout(LayoutKind.Explicit, Pack = 4)]
struct BillboardDataUnion
{
    [FieldOffset(0)]
    public QuadBillboardData Quad;

    [FieldOffset(0)]
    public TriBillboardData Tri;

    [FieldOffset(0)]
    public PointBillboardData Point;
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
struct QuadBillboardData
{
    public float3 V0;
    public uint CustomViewProjection;

    public float3 V1;
    public float Reflectivity;

    public float3 V2;
    public float AlphaCutout;

    public float3 V3;
    public float SoftParticleDistanceScale;

    public HalfVector2 UVOffset;
    public HalfVector2 UVSize;
    public HalfVector4 Color;
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
struct TriBillboardData
{
    public float3 V0;
    public HalfVector2 UV0;

    public float3 V1;
    public HalfVector2 UV1;

    public float3 V2;
    public HalfVector2 UV2;

    public uint CustomViewProjection;
    public float Reflectivity;
    public float AlphaCutout;
    public float SoftParticleDistanceScale;

    public HalfVector4 Color;
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
struct PointBillboardData
{
    public float3 Position;
    public HalfVector2 RadiusAndAngle;

    public uint CustomViewProjection;
    public float Reflectivity;
    public float AlphaCutout;
    public float SoftParticleDistanceScale;

    public HalfVector2 UVOffset;
    public HalfVector2 UVSize;
    public HalfVector4 Color;
}
