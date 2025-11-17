using Prism.Maths;
using System.Runtime.InteropServices;
using VRageMath;

namespace Prism.Vanilla.Billboard.ShaderTypes;

[StructLayout(LayoutKind.Sequential)]
struct FrameConstants
{
    public Matrix ViewMatrixAt0;
    public Matrix ProjMatrix;

    public float3 FogColor;
    public float FogDensity;

    public float FogMultiplier;
    private uint _pad1;
    private uint _pad2;
    private uint _pad3;
}
