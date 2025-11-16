using System.Runtime.InteropServices;
using VRageMath;

namespace Prism.Vanilla.Billboard.ShaderTypes;

[StructLayout(LayoutKind.Sequential)]
struct FrameConstants
{
    public Matrix ViewMatrixAt0;
    public Matrix ProjMatrix;
}
