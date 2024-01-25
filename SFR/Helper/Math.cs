namespace SFDCT.Helper;

internal static class Math
{
    internal static float Lerp(float a, float b, float f) => a * (1.0f - f) + b * f;

    internal static float InverseLerp(float a, float b, float f) => (f - a) / (b - a);

    internal static int Clamp(int val, int max, int min)
    {
        if (val < min)
        {
            return min;
        }
        if (val > max)
        {
            return max;
        }
        return val;
    }

    internal static float ClampF(float val, float max, float min)
    {
        if (val < min)
        {
            return min;
        }
        if (val > max)
        {
            return max;
        }
        return val;
    }
}