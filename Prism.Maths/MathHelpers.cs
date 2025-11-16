namespace Prism.Maths;

internal class MathHelpers
{
    public static float Clamp(float value, float min, float max)
    {
#if NETCOREAPP2_0_OR_GREATER
        return Math.Clamp(value, min, max);
#else
        if (value < min) return min;
        else if (value > max) return max;
        return value;
#endif
    }

    public static int Clamp(int value, int min, int max)
    {
#if NETCOREAPP2_0_OR_GREATER
        return Math.Clamp(value, min, max);
#else
        if (value < min) return min;
        else if (value > max) return max;
        return value;
#endif
    }
}
