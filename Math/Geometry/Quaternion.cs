﻿
using System;

//
// Summary:
//     ///
//     Quaternionds are used to represent rotations.
//     ///
//[DefaultMember("Item")]
public struct Quaterniond : IEquatable<Quaterniond>
{
    //thanks to this dude
    //https://gist.github.com/aeroson/043001ca12fe29ee911e

    // A custom implementation of UnityEngine.Quaterniond
    // Base is decompiled UnityEngine.Quaterniond
    // Doesn't implement methods marked Obsolete
    // Does implicit coversions to and from UnityEngine.Quaterniond

    // Uses code from:
    // https://raw.githubusercontent.com/mono/opentk/master/Source/OpenTK/Math/Quaterniond.cs
    // http://answers.unity3d.com/questions/467614/what-is-the-source-code-of-quaternionlookrotation.html
    // http://stackoverflow.com/questions/12088610/conversion-between-euler-quaternion-like-in-unity3d-engine
    // http://stackoverflow.com/questions/11492299/quaternion-to-euler-angles-algorithm-how-to-convert-to-y-up-and-between-ha


    const double radToDeg = (double)(180.0 / Math.PI);
    const double degToRad = (double)(Math.PI / 180.0);

    public const double kEpsilon = 1E-06f; // should probably be used in the 0 tests in LookRotation or Slerp
    public Vector3d xyz
    {
        set
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }
        get
        {
            return new Vector3d(x, y, z);
        }
    }
    /// <summary>
    ///   <para>X component of the Quaterniond. Don't modify this directly unless you know quaternions inside out.</para>
    /// </summary>
    public double x;
    /// <summary>
    ///   <para>Y component of the Quaterniond. Don't modify this directly unless you know quaternions inside out.</para>
    /// </summary>
    public double y;
    /// <summary>
    ///   <para>Z component of the Quaterniond. Don't modify this directly unless you know quaternions inside out.</para>
    /// </summary>
    public double z;
    /// <summary>
    ///   <para>W component of the Quaterniond. Don't modify this directly unless you know quaternions inside out.</para>
    /// </summary>
    public double w;

    public double this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return this.x;
                case 1:
                    return this.y;
                case 2:
                    return this.z;
                case 3:
                    return this.w;
                default:
                    throw new IndexOutOfRangeException("Invalid Quaterniond index!");
            }
        }
        set
        {
            switch (index)
            {
                case 0:
                    this.x = value;
                    break;
                case 1:
                    this.y = value;
                    break;
                case 2:
                    this.z = value;
                    break;
                case 3:
                    this.w = value;
                    break;
                default:
                    throw new IndexOutOfRangeException("Invalid Quaterniond index!");
            }
        }
    }
    /// <summary>
    ///   <para>The identity rotation (RO).</para>
    /// </summary>
    public static Quaterniond identity
    {
        get
        {
            return new Quaterniond(0f, 0f, 0f, 1f);
        }
    }
    /// <summary>
    ///   <para>Returns the euler angle representation of the rotation.</para>
    /// </summary>
    public Vector3d eulerAngles
    {
        get
        {
            return Quaterniond.Internal_ToEulerRad(this);
        }
        set
        {
            this = Quaterniond.Euler(value);
        }
    }





    #region public double Length

    /// <summary>
    /// Gets the length (magnitude) of the quaternion.
    /// </summary>
    /// <seealso cref="LengthSquared"/>
    public double Length
    {
        get
        {
            return (double)System.Math.Sqrt(x * x + y * y + z * z + w * w);
        }
    }

    #endregion

    #region public double LengthSquared

    /// <summary>
    /// Gets the square of the quaternion length (magnitude).
    /// </summary>
    public double LengthSquared
    {
        get
        {
            return x * x + y * y + z * z + w * w;
        }
    }

    #endregion

    #region public void Normalize()

    /// <summary>
    /// Scales the Quaterniond to unit length.
    /// </summary>
    public void Normalize()
    {
        double scale = 1.0f / this.Length;
        xyz *= scale;
        w *= scale;
    }

    #endregion


    #region Normalize

    /// <summary>
    /// Scale the given quaternion to unit length
    /// </summary>
    /// <param name="q">The quaternion to normalize</param>
    /// <returns>The normalized quaternion</returns>
    public static Quaterniond Normalize(Quaterniond q)
    {
        Quaterniond result;
        Normalize(ref q, out result);
        return result;
    }

    /// <summary>
    /// Scale the given quaternion to unit length
    /// </summary>
    /// <param name="q">The quaternion to normalize</param>
    /// <param name="result">The normalized quaternion</param>
    public static void Normalize(ref Quaterniond q, out Quaterniond result)
    {
        double scale = 1.0f / q.Length;
        result = new Quaterniond(q.xyz * scale, q.w * scale);
    }

    #endregion


    /// <summary>
    ///   <para>Constructs new Quaterniond with given x,y,z,w components.</para>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="w"></param>
    public Quaterniond(double x, double y, double z, double w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    /// <summary>
    /// Construct a new Quaterniond from vector and w components
    /// </summary>
    /// <param name="v">The vector part</param>
    /// <param name="w">The w part</param>
    public Quaterniond(Vector3d v, double w)
    {
        this.x = v.x;
        this.y = v.y;
        this.z = v.z;
        this.w = w;
    }


    /// <summary>
    ///   <para>Set x, y, z and w components of an existing Quaterniond.</para>
    /// </summary>
    /// <param name="new_x"></param>
    /// <param name="new_y"></param>
    /// <param name="new_z"></param>
    /// <param name="new_w"></param>
    public void Set(double new_x, double new_y, double new_z, double new_w)
    {
        this.x = new_x;
        this.y = new_y;
        this.z = new_z;
        this.w = new_w;
    }
    /// <summary>
    ///   <para>The dot product between two rotations.</para>
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public static double Dot(Quaterniond a, Quaterniond b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
    }
    /// <summary>
    ///   <para>Creates a rotation which rotates /angle/ degrees around /axis/.</para>
    /// </summary>
    /// <param name="angle"></param>
    /// <param name="axis"></param>
    public static Quaterniond AngleAxis(double angle, Vector3d axis)
    {
        return Quaterniond.INTERNAL_CALL_AngleAxis(angle, ref axis);
    }
    private static Quaterniond INTERNAL_CALL_AngleAxis(double degress, ref Vector3d axis)
    {
        if (axis.sqrMagnitude == 0.0f)
            return identity;

        Quaterniond result = identity;
        var radians = degress * degToRad;
        radians *= 0.5f;
        axis = axis.normalized;
        axis = axis * (double)System.Math.Sin(radians);
        result.x = axis.x;
        result.y = axis.y;
        result.z = axis.z;
        result.w = (double)System.Math.Cos(radians);

        return Normalize(result);
    }
    public void ToAngleAxis(out double angle, out Vector3d axis)
    {
        Quaterniond.Internal_ToAxisAngleRad(this, out axis, out angle);
        angle *= radToDeg;
    }
    /// <summary>
    ///   <para>Creates a rotation which rotates from /fromDirection/ to /toDirection/.</para>
    /// </summary>
    /// <param name="fromDirection"></param>
    /// <param name="toDirection"></param>
    public static Quaterniond FromToRotation(Vector3d fromDirection, Vector3d toDirection)
    {
        return RotateTowards(LookRotation(fromDirection), LookRotation(toDirection), double.MaxValue);
    }
    /// <summary>
    ///   <para>Creates a rotation which rotates from /fromDirection/ to /toDirection/.</para>
    /// </summary>
    /// <param name="fromDirection"></param>
    /// <param name="toDirection"></param>
    public void SetFromToRotation(Vector3d fromDirection, Vector3d toDirection)
    {
        this = Quaterniond.FromToRotation(fromDirection, toDirection);
    }
    /// <summary>
    ///   <para>Creates a rotation with the specified /forward/ and /upwards/ directions.</para>
    /// </summary>
    /// <param name="forward">The direction to look in.</param>
    /// <param name="upwards">The vector that defines in which direction up is.</param>
    public static Quaterniond LookRotation(Vector3d forward, Vector3d upwards)
    {
        return Quaterniond.INTERNAL_CALL_LookRotation(ref forward, ref upwards);
    }

    public static Quaterniond LookRotation(Vector3d forward)
    {
        Vector3d up = Vector3d.up;
        return Quaterniond.INTERNAL_CALL_LookRotation(ref forward, ref up);
    }

    // from http://answers.unity3d.com/questions/467614/what-is-the-source-code-of-quaternionlookrotation.html
    private static Quaterniond INTERNAL_CALL_LookRotation(ref Vector3d forward, ref Vector3d up)
    {
        forward = Vector3d.Normalize(forward);
        Vector3d right = Vector3d.Normalize(Vector3d.Cross(up, forward));
        up = Vector3d.Cross(forward, right);
        var m00 = right.x;
        var m01 = right.y;
        var m02 = right.z;
        var m10 = up.x;
        var m11 = up.y;
        var m12 = up.z;
        var m20 = forward.x;
        var m21 = forward.y;
        var m22 = forward.z;


        double num8 = (m00 + m11) + m22;
        var quaternion = new Quaterniond();
        if (num8 > 0f)
        {
            var num = (double)Math.Sqrt(num8 + 1f);
            quaternion.w = num * 0.5f;
            num = 0.5f / num;
            quaternion.x = (m12 - m21) * num;
            quaternion.y = (m20 - m02) * num;
            quaternion.z = (m01 - m10) * num;
            return quaternion;
        }
        if ((m00 >= m11) && (m00 >= m22))
        {
            var num7 = (double)Math.Sqrt(((1f + m00) - m11) - m22);
            var num4 = 0.5f / num7;
            quaternion.x = 0.5f * num7;
            quaternion.y = (m01 + m10) * num4;
            quaternion.z = (m02 + m20) * num4;
            quaternion.w = (m12 - m21) * num4;
            return quaternion;
        }
        if (m11 > m22)
        {
            var num6 = (double)Math.Sqrt(((1f + m11) - m00) - m22);
            var num3 = 0.5f / num6;
            quaternion.x = (m10 + m01) * num3;
            quaternion.y = 0.5f * num6;
            quaternion.z = (m21 + m12) * num3;
            quaternion.w = (m20 - m02) * num3;
            return quaternion;
        }
        var num5 = (double)Math.Sqrt(((1f + m22) - m00) - m11);
        var num2 = 0.5f / num5;
        quaternion.x = (m20 + m02) * num2;
        quaternion.y = (m21 + m12) * num2;
        quaternion.z = 0.5f * num5;
        quaternion.w = (m01 - m10) * num2;
        return quaternion;
    }

    public void SetLookRotation(Vector3d view)
    {
        Vector3d up = Vector3d.up;
        this.SetLookRotation(view, up);
    }
    /// <summary>
    ///   <para>Creates a rotation with the specified /forward/ and /upwards/ directions.</para>
    /// </summary>
    /// <param name="view">The direction to look in.</param>
    /// <param name="up">The vector that defines in which direction up is.</param>
    public void SetLookRotation(Vector3d view, Vector3d up)
    {
        this = Quaterniond.LookRotation(view, up);
    }
    /// <summary>
    ///   <para>Spherically interpolates between /a/ and /b/ by t. The parameter /t/ is clamped to the range [0, 1].</para>
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="t"></param>
    public static Quaterniond Slerp(Quaterniond a, Quaterniond b, double t)
    {
        return Quaterniond.INTERNAL_CALL_Slerp(ref a, ref b, t);
    }

    private static Quaterniond INTERNAL_CALL_Slerp(ref Quaterniond a, ref Quaterniond b, double t)
    {
        if (t > 1) t = 1;
        if (t < 0) t = 0;
        return INTERNAL_CALL_SlerpUnclamped(ref a, ref b, t);
    }
    /// <summary>
    ///   <para>Spherically interpolates between /a/ and /b/ by t. The parameter /t/ is not clamped.</para>
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="t"></param>
    public static Quaterniond SlerpUnclamped(Quaterniond a, Quaterniond b, double t)
    {
        return Quaterniond.INTERNAL_CALL_SlerpUnclamped(ref a, ref b, t);
    }

    private static Quaterniond INTERNAL_CALL_SlerpUnclamped(ref Quaterniond a, ref Quaterniond b, double t)
    {
        // if either input is zero, return the other.
        if (a.LengthSquared == 0.0f)
        {
            if (b.LengthSquared == 0.0f)
            {
                return identity;
            }
            return b;
        }
        else if (b.LengthSquared == 0.0f)
        {
            return a;
        }


        double cosHalfAngle = a.w * b.w + Vector3d.Dot(a.xyz, b.xyz);

        if (cosHalfAngle >= 1.0f || cosHalfAngle <= -1.0f)
        {
            // angle = 0.0f, so just return one input.
            return a;
        }
        else if (cosHalfAngle < 0.0f)
        {
            b.xyz = -b.xyz;
            b.w = -b.w;
            cosHalfAngle = -cosHalfAngle;
        }

        double blendA;
        double blendB;
        if (cosHalfAngle < 0.99f)
        {
            // do proper slerp for big angles
            double halfAngle = (double)System.Math.Acos(cosHalfAngle);
            double sinHalfAngle = (double)System.Math.Sin(halfAngle);
            double oneOverSinHalfAngle = 1.0f / sinHalfAngle;
            blendA = (double)System.Math.Sin(halfAngle * (1.0f - t)) * oneOverSinHalfAngle;
            blendB = (double)System.Math.Sin(halfAngle * t) * oneOverSinHalfAngle;
        }
        else
        {
            // do lerp if angle is really small.
            blendA = 1.0f - t;
            blendB = t;
        }

        Quaterniond result = new Quaterniond(blendA * a.xyz + blendB * b.xyz, blendA * a.w + blendB * b.w);
        if (result.LengthSquared > 0.0f)
            return Normalize(result);
        else
            return identity;
    }
    /// <summary>
    ///   <para>Interpolates between /a/ and /b/ by /t/ and normalizes the result afterwards. The parameter /t/ is clamped to the range [0, 1].</para>
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="t"></param>
    public static Quaterniond Lerp(Quaterniond a, Quaterniond b, double t)
    {
        if (t > 1) t = 1;
        if (t < 0) t = 0;
        return INTERNAL_CALL_Slerp(ref a, ref b, t); // TODO: use lerp not slerp, "Because quaternion works in 4D. Rotation in 4D are linear" ???
    }
    /// <summary>
    ///   <para>Interpolates between /a/ and /b/ by /t/ and normalizes the result afterwards. The parameter /t/ is not clamped.</para>
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="t"></param>
    public static Quaterniond LerpUnclamped(Quaterniond a, Quaterniond b, double t)
    {
        return INTERNAL_CALL_Slerp(ref a, ref b, t);
    }
    /// <summary>
    ///   <para>Rotates a rotation /from/ towards /to/.</para>
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="maxDegreesDelta"></param>
    public static Quaterniond RotateTowards(Quaterniond from, Quaterniond to, double maxDegreesDelta)
    {
        double num = Quaterniond.Angle(from, to);
        if (num == 0f)
        {
            return to;
        }
        double t = Math.Min(1f, maxDegreesDelta / num);
        return Quaterniond.SlerpUnclamped(from, to, t);
    }
    /// <summary>
    ///   <para>Returns the Inverse of /rotation/.</para>
    /// </summary>
    /// <param name="rotation"></param>
    public static Quaterniond Inverse(Quaterniond rotation)
    {
        double lengthSq = rotation.LengthSquared;
        if (lengthSq != 0.0)
        {
            double i = 1.0f / lengthSq;
            return new Quaterniond(rotation.xyz * -i, rotation.w * i);
        }
        return rotation;
    }
    /// <summary>
    ///   <para>Returns a nicely formatted string of the Quaterniond.</para>
    /// </summary>
    /// <param name="format"></param>
    public override string ToString()
    {
        return string.Format("({0:F1}, {1:F1}, {2:F1}, {3:F1})", new object[]
        {
                this.x,
                this.y,
                this.z,
                this.w
        });
    }
    /// <summary>
    ///   <para>Returns a nicely formatted string of the Quaterniond.</para>
    /// </summary>
    /// <param name="format"></param>
    public string ToString(string format)
    {
        return string.Format("({0}, {1}, {2}, {3})", new object[]
        {
                this.x.ToString(format),
                this.y.ToString(format),
                this.z.ToString(format),
                this.w.ToString(format)
        });
    }
    /// <summary>
    ///   <para>Returns the angle in degrees between two rotations /a/ and /b/.</para>
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public static double Angle(Quaterniond a, Quaterniond b)
    {
        double f = Quaterniond.Dot(a, b);
        return Math.Acos(Math.Min(Math.Abs(f), 1f)) * 2f * radToDeg;
    }

    /// <summary>
    ///   <para>Returns a rotation that rotates z degrees around the z axis, x degrees around the x axis, and y degrees around the y axis (in that order).</para>
    /// </summary>
    /// <param name="euler"></param>
    public static Quaterniond Euler(Vector3d euler)
    {
        return Euler(euler.x, euler.y, euler.z);
    }

    // from http://stackoverflow.com/questions/12088610/conversion-between-euler-quaternion-like-in-unity3d-engine
    private static Vector3d Internal_ToEulerRad(Quaterniond rotation)
    {
        var q1 = rotation;
        var sqw = q1.w * q1.w;
        var sqx = q1.x * q1.x;
        var sqy = q1.y * q1.y;
        var sqz = q1.z * q1.z;
        var unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
        var test = q1.x * q1.w - q1.y * q1.z;
        Vector3d v;

        if (test > 0.49999f * unit)
        { // singularity at north pole
            v.y = 2f * Math.Atan2(q1.y, q1.x);
            v.x = Math.PI / 2;
            v.z = 0;
            return NormalizeAngles(v * radToDeg);
        }
        if (test < -0.49999f * unit)
        { // singularity at south pole
            v.y = -2f * Math.Atan2(q1.y, q1.x);
            v.x = -Math.PI / 2;
            v.z = 0;
            return NormalizeAngles(v * radToDeg);
        }
        Quaterniond q = new Quaterniond(q1.w, q1.z, q1.x, q1.y);
        v.y = Math.Atan2(2 * q.x * q.w + 2 * q.y * q.z, 1 - 2 * (q.z * q.z + q.w * q.w));     // Yaw
        v.x = Math.Asin(2 * (q.x * q.z - q.w * q.y));                             // Pitch
        v.z = Math.Atan2(2 * q.x * q.y + 2 * q.z * q.w, 1 - 2 * (q.y * q.y + q.z * q.z));      // Roll
        return NormalizeAngles(v * radToDeg);
    }
    private static Vector3d NormalizeAngles(Vector3d angles)
    {
        angles.x = NormalizeAngle(angles.x);
        angles.y = NormalizeAngle(angles.y);
        angles.z = NormalizeAngle(angles.z);
        return angles;
    }

    private static double NormalizeAngle(double angle)
    {
        while (angle >= 360)
            angle -= 360;
        while (angle < 0)
            angle += 360;
        return angle;
    }

    // from http://stackoverflow.com/questions/11492299/quaternion-to-euler-angles-algorithm-how-to-convert-to-y-up-and-between-ha
    public static Quaterniond Euler(double yaw, double pitch, double roll)
    {
        const double deg2rad = (System.Math.PI * 2.0) / 360.0;
        yaw *= deg2rad;
        pitch *= deg2rad;
        roll *= deg2rad;

        double yawOver2 = yaw * 0.5f;
        double cosYawOver2 = Math.Cos(yawOver2);
        double sinYawOver2 = (float)System.Math.Sin(yawOver2);
        double pitchOver2 = pitch * 0.5f;
        double cosPitchOver2 = Math.Cos(pitchOver2);
        double sinPitchOver2 = Math.Sin(pitchOver2);
        double rollOver2 = roll * 0.5f;
        double cosRollOver2 = Math.Cos(rollOver2);
        double sinRollOver2 = Math.Sin(rollOver2);
        Quaterniond result;
        result.w = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
        result.x = sinYawOver2 * cosPitchOver2 * cosRollOver2 + cosYawOver2 * sinPitchOver2 * sinRollOver2;
        result.y = cosYawOver2 * sinPitchOver2 * cosRollOver2 - sinYawOver2 * cosPitchOver2 * sinRollOver2;
        result.z = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;

        return result;

    }
    private static void Internal_ToAxisAngleRad(Quaterniond q, out Vector3d axis, out double angle)
    {
        if (Math.Abs(q.w) > 1.0f)
            q.Normalize();


        angle = 2.0f * (double)System.Math.Acos(q.w); // angle
        double den = (double)System.Math.Sqrt(1.0 - q.w * q.w);
        if (den > 0.0001f)
        {
            axis = q.xyz / den;
        }
        else
        {
            // This occurs when the angle is zero. 
            // Not a problem: just set an arbitrary normalized axis.
            axis = new Vector3d(1, 0, 0);
        }
    }

    #region Obsolete methods
    /*
    [Obsolete("Use Quaterniond.Euler instead. This function was deprecated because it uses radians instead of degrees")]
    public static Quaterniond EulerRotation(double x, double y, double z)
    {
        return Quaterniond.Internal_FromEulerRad(new Vector3d(x, y, z));
    }
    [Obsolete("Use Quaterniond.Euler instead. This function was deprecated because it uses radians instead of degrees")]
    public static Quaterniond EulerRotation(Vector3d euler)
    {
        return Quaterniond.Internal_FromEulerRad(euler);
    }
    [Obsolete("Use Quaterniond.Euler instead. This function was deprecated because it uses radians instead of degrees")]
    public void SetEulerRotation(double x, double y, double z)
    {
        this = Quaterniond.Internal_FromEulerRad(new Vector3d(x, y, z));
    }
    [Obsolete("Use Quaterniond.Euler instead. This function was deprecated because it uses radians instead of degrees")]
    public void SetEulerRotation(Vector3d euler)
    {
        this = Quaterniond.Internal_FromEulerRad(euler);
    }
    [Obsolete("Use Quaterniond.eulerAngles instead. This function was deprecated because it uses radians instead of degrees")]
    public Vector3d ToEuler()
    {
        return Quaterniond.Internal_ToEulerRad(this);
    }
    [Obsolete("Use Quaterniond.Euler instead. This function was deprecated because it uses radians instead of degrees")]
    public static Quaterniond EulerAngles(double x, double y, double z)
    {
        return Quaterniond.Internal_FromEulerRad(new Vector3d(x, y, z));
    }
    [Obsolete("Use Quaterniond.Euler instead. This function was deprecated because it uses radians instead of degrees")]
    public static Quaterniond EulerAngles(Vector3d euler)
    {
        return Quaterniond.Internal_FromEulerRad(euler);
    }
    [Obsolete("Use Quaterniond.ToAngleAxis instead. This function was deprecated because it uses radians instead of degrees")]
    public void ToAxisAngle(out Vector3d axis, out double angle)
    {
        Quaterniond.Internal_ToAxisAngleRad(this, out axis, out angle);
    }
    [Obsolete("Use Quaterniond.Euler instead. This function was deprecated because it uses radians instead of degrees")]
    public void SetEulerAngles(double x, double y, double z)
    {
        this.SetEulerRotation(new Vector3d(x, y, z));
    }
    [Obsolete("Use Quaterniond.Euler instead. This function was deprecated because it uses radians instead of degrees")]
    public void SetEulerAngles(Vector3d euler)
    {
        this = Quaterniond.EulerRotation(euler);
    }
    [Obsolete("Use Quaterniond.eulerAngles instead. This function was deprecated because it uses radians instead of degrees")]
    public static Vector3d ToEulerAngles(Quaterniond rotation)
    {
        return Quaterniond.Internal_ToEulerRad(rotation);
    }
    [Obsolete("Use Quaterniond.eulerAngles instead. This function was deprecated because it uses radians instead of degrees")]
    public Vector3d ToEulerAngles()
    {
        return Quaterniond.Internal_ToEulerRad(this);
    }
    [Obsolete("Use Quaterniond.AngleAxis instead. This function was deprecated because it uses radians instead of degrees")]
    public static Quaterniond AxisAngle(Vector3d axis, double angle)
    {
        return Quaterniond.INTERNAL_CALL_AxisAngle(ref axis, angle);
    }
    private static Quaterniond INTERNAL_CALL_AxisAngle(ref Vector3d axis, double angle)
    {
    }
    [Obsolete("Use Quaterniond.AngleAxis instead. This function was deprecated because it uses radians instead of degrees")]
    public void SetAxisAngle(Vector3d axis, double angle)
    {
        this = Quaterniond.AxisAngle(axis, angle);
    }
    */
    #endregion
    public override int GetHashCode()
    {
        return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2 ^ this.w.GetHashCode() >> 1;
    }
    public override bool Equals(object other)
    {
        if (!(other is Quaterniond))
        {
            return false;
        }
        Quaterniond quaternion = (Quaterniond)other;
        return this.x.Equals(quaternion.x) && this.y.Equals(quaternion.y) && this.z.Equals(quaternion.z) && this.w.Equals(quaternion.w);
    }

    public bool Equals(Quaterniond other)
    {
        return this.x.Equals(other.x) && this.y.Equals(other.y) && this.z.Equals(other.z) && this.w.Equals(other.w);
    }

    public static Quaterniond operator *(Quaterniond lhs, Quaterniond rhs)
    {
        return new Quaterniond(lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y, lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z, lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x, lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);
    }
    public static Vector3d operator *(Quaterniond rotation, Vector3d point)
    {
        double num = rotation.x * 2f;
        double num2 = rotation.y * 2f;
        double num3 = rotation.z * 2f;
        double num4 = rotation.x * num;
        double num5 = rotation.y * num2;
        double num6 = rotation.z * num3;
        double num7 = rotation.x * num2;
        double num8 = rotation.x * num3;
        double num9 = rotation.y * num3;
        double num10 = rotation.w * num;
        double num11 = rotation.w * num2;
        double num12 = rotation.w * num3;
        Vector3d result;
        result.x = (1f - (num5 + num6)) * point.x + (num7 - num12) * point.y + (num8 + num11) * point.z;
        result.y = (num7 + num12) * point.x + (1f - (num4 + num6)) * point.y + (num9 - num10) * point.z;
        result.z = (num8 - num11) * point.x + (num9 + num10) * point.y + (1f - (num4 + num5)) * point.z;
        return result;
    }
    public static bool operator ==(Quaterniond lhs, Quaterniond rhs)
    {
        return Quaterniond.Dot(lhs, rhs) > 0.999999f;
    }
    public static bool operator !=(Quaterniond lhs, Quaterniond rhs)
    {
        return Quaterniond.Dot(lhs, rhs) <= 0.999999f;
    }

    public static explicit operator Quaternion(Quaterniond me)
    {
        return new Quaternion((float)me.x, (float)me.y, (float)me.z, (float)me.w);
    }
    public static implicit operator Quaterniond(Quaternion other)
    {
        return new Quaterniond(other.x, other.y, other.z, other.w);
    }
}

public static class QuaternionExtentsions
{
    public static bool ApproximatelyEqual(Quaternion a, Quaternion b, float e = 0.0001f)
    {
        return Math.Abs(a.x - b.x) < e && Math.Abs(a.y - b.y) < e && Math.Abs(a.z - b.z) < e && Math.Abs(a.w - b.w) < e;
    }
}

#if !UNITY
public struct Quaternion
{
    public float x;
    public float y;
    public float z;
    public float w;
    public Quaternion(in float x, in float y, in float z, in float w)
    {
        this.x = x; this.y = y; this.z = z; this.w = w;
    }
}
#endif