using VRageMath.PackedVector;

namespace Prism.RenderPerf.ShaderTypes;

// must be padded to a multiple of 256 bytes
public struct MaterialInfo
{
    public HalfVector2 UVOffset; // 4 bytes
    public HalfVector2 UVSize;   // 8 bytes

    public float AlphaSaturation; // 12 bytes
    public float SoftParticleDistanceScale; // 16 bytes

    // only UV Offset/Size, AlphaCutout, AlphaSaturation, CanBeAffectedByOtherLights, and SoftParticleDistanceScale are used
    // UseAtlas, Texture, TextureType, Id, TargetSize are used for getting the texture
}
