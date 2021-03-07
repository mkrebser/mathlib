using System.Collections;
using System.Collections.Generic;

public static class MathConstants
{
    public const double epsilon = 0.0000000001;
    public const float epsilonf = 0.000001f;
}

public static class Generic
{
	public static bool ApproximatelyEqual(in double a, in double b, in double e = MathConstants.epsilon)
    {
        return System.Math.Abs(a - b) < e;
    }

    public static double ClampInclusive(in double value, in double min, in double max)
    {
        return value < min ? min : (value > max ? max : value);
    }

    public static int FloorToInt(in double d)
    {
        return (int)System.Math.Floor(d);
    }
}
