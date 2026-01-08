namespace Prism.Maths;

internal class MathHelpers
{
    public static float Clamp(float value, float min, float max)
    {
        return VRageMath.MathHelper.Clamp(value, min, max);
    }

    public static int Clamp(int value, int min, int max)
    {
        return VRageMath.MathHelper.Clamp(value, min, max);
    }
}
