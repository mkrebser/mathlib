using System;

static class mathconst
{
    public const double rad2degd = 360.0 / (System.Math.PI * 2.0);
    public const float rad2deg = (float)rad2degd;
}

[System.Serializable]
public struct Vector3Int : IEquatable<Vector3Int>
{
    public int x;
    public int y;
    public int z;

    public Vector3Int(in Vector3 v)
    {
        x = (int)v.x; y = (int)v.y; z = (int)v.z;
    }
    public Vector3Int(in int fill)
    {
        x = fill; y = fill; z = fill;
    }
    public Vector3Int(in int X, in int Y, in int Z)
    {
        x = X; y = Y; z = Z;
    }
    public Vector3 ToVector3
    {
        get
        {
            return new Vector3(x, y, z);
        }
    }

    public double magnitude
    {
        get
        {
            return System.Math.Sqrt(x * x + y * y + z * z);
        }
    }
    public double sqrMagnitude
    {
        get
        {
            return x * x + y * y + z * z;
        }
    }

    public static readonly Vector3Int zero = new Vector3Int(0, 0, 0);
    public static readonly Vector3Int one = new Vector3Int(1, 1, 1);

    public bool Equals(Vector3Int v)
    {
        return v.x == x && v.y == y && v.z == z;
    }
    public override bool Equals(object obj)
    {
        return obj is Vector3Int ? Equals((Vector3Int)obj) : false;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 29;
            hash = hash * 17 + x.GetHashCode();
            hash = hash * 19 + y.GetHashCode();
            hash = hash * 19 + z.GetHashCode();
            return hash;
        }
    }
    public static bool operator==(in Vector3Int v1, in Vector3Int v2)
    {
        return v1.Equals(v2);
    }
    public static bool operator!=(in Vector3Int v1, in Vector3Int v2)
    {
        return !(v1 == v2);
    }

    public static Vector3Int operator -(in Vector3Int a, in Vector3Int b)
    {
        return new Vector3Int(a.x - b.x, a.y - b.y, a.z - b.z);
    }
    public static Vector3Int operator -(in Vector3Int a)
    {
        return new Vector3Int(-a.x, -a.y, -a.z);
    }
    public static Vector3Int operator +(in Vector3Int a, in Vector3Int b)
    {
        return new Vector3Int(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    public static Vector3Int operator *(in Vector3Int a, in int b)
    {
        return new Vector3Int(a.x * b, a.y * b, a.z * b);
    }
    public static Vector3Int operator *(in int b, in Vector3Int a)
    {
        return new Vector3Int(a.x * b, a.y * b, a.z * b);
    }
    public static Vector3Int operator /(in Vector3Int a, in int b)
    {
        return new Vector3Int(a.x / b, a.y / b, a.z / b);
    }

    public static int Dot(in Vector3Int a, in Vector3Int b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }
    public static Vector3Int Cross(in Vector3Int a, in Vector3Int b)
    {
        return new Vector3Int(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    }

    /// <summary>
    /// Clamp a to be between min and max inclusive
    /// </summary>
    /// <param name="a"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static Vector3Int VectorClamp(in Vector3Int a, in Vector3Int min, in Vector3Int max)
    {
        return new Vector3Int(
            a.x > max.x ? max.x : (a.x < min.x ? min.x : a.x),
            a.y > max.y ? max.y : (a.y < min.y ? min.y : a.y),
            a.z > max.z ? max.z : (a.z < min.z ? min.z : a.z));
    }
    /// <summary>
    /// inclusive min clamp
    /// </summary>
    /// <param name="v"></param>
    /// <param name="min"></param>
    /// <returns></returns>
    public static Vector3Int MinClamp(in Vector3Int v, in Vector3Int min)
    {
        return new Vector3Int(
            v.x < min.x ? min.x : v.x,
            v.y < min.y ? min.y : v.y,
            v.z < min.z ? min.z : v.z);
    }
    /// <summary>
    /// inclusive max clamp
    /// </summary>
    /// <param name="v"></param>
    /// <param name="min"></param>
    /// <returns></returns>
    public static Vector3Int MaxClamp(in Vector3Int v, in Vector3Int max)
    {
        return new Vector3Int(
            v.x > max.x ? max.x : v.x,
            v.y > max.y ? max.y : v.y,
            v.z > max.z ? max.z : v.z);
    }

    public static Vector3Int forward
    {
        get
        {
            return new Vector3Int(0, 0, 1);
        }
    }
    public static Vector3Int back
    {
        get
        {
            return new Vector3Int(0, 0, -1);
        }
    }
    public static Vector3Int right
    {
        get
        {
            return new Vector3Int(1, 0, 0);
        }
    }
    public static Vector3Int left
    {
        get
        {
            return new Vector3Int(-1, 0, 0);
        }
    }
    public static Vector3Int up
    {
        get
        {
            return new Vector3Int(0, 1, 0);
        }
    }
    public static Vector3Int down
    {
        get
        {
            return new Vector3Int(0, -1, 0);
        }
    }
    /// <summary>
    /// Abosulte value all components
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector3Int Abs(in Vector3Int v)
    {
        return new Vector3Int(Math.Abs(v.x), Math.Abs(v.y), Math.Abs(v.z));
    }

    public override string ToString()
    {
        return "Vector3Int(" + x.ToString() + ", " + y.ToString() + ", " + z.ToString() + ")";
    }

    public static explicit operator Vector3Int(in Vector3d v)
    {
        return new Vector3Int((int)v.x, (int)v.y, (int)v.z);
    }
    public static implicit operator Vector3(in Vector3Int v)
    {
        return new Vector3(v.x, v.y, v.z);
    }
    public static explicit operator Vector3Int(in Vector3 v)
    {
        return new Vector3Int((int)v.x, (int)v.y, (int)v.z);
    }
    public static Vector3Int FloorToInt(in Vector3 v)
    {
        return new Vector3Int(Generic.FloorToInt(v.x), Generic.FloorToInt(v.y), Generic.FloorToInt(v.z));
    }
    public static Vector3Int FloorToInt(Vector3d v)
    {
        return new Vector3Int(Generic.FloorToInt(v.x), Generic.FloorToInt(v.y), Generic.FloorToInt(v.z));
    }
}

[System.Serializable]
public struct Vector2Int : IEquatable<Vector2Int>
{
    public int x;
    public int y;

    public Vector2Int(in Vector2 v)
    {
        x = (int)v.x; y = (int)v.y;;
    }
    public Vector2Int(in int X, in int Y)
    {
        x = X; y = Y;
    }
    public Vector2 ToVector2
    {
        get
        {
            return new Vector2(x, y);
        }
    }

    public static Vector2Int zero
    {
        get
        {
            return new Vector2Int(0, 0);
        }
    }

    public bool Equals(Vector2Int v)
    {
        return v.x == x && v.y == y;
    }
    public override bool Equals(object obj)
    {
        return obj is Vector2Int ? Equals((Vector2Int)obj) : false;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 29;
            hash = hash * 17 + x.GetHashCode();
            hash = hash * 19 + y.GetHashCode();
            return hash;
        }
    }
    public override string ToString()
    {
        return "Vector2Int(" + x.ToString() + ", " + y.ToString() + ")";
    }
    public static bool operator ==(in Vector2Int v1, in Vector2Int v2)
    {
        return v1.Equals(v2);
    }
    public static bool operator !=(in Vector2Int v1, in Vector2Int v2)
    {
        return !(v1 == v2);
    }

    public static Vector2Int operator -(in Vector2Int a, in Vector2Int b)
    {
        return new Vector2Int(a.x - b.x, a.y - b.y);
    }
    public static Vector2Int operator -(in Vector2Int a)
    {
        return new Vector2Int(-a.x, -a.y);
    }
    public static Vector2Int operator +(in Vector2Int a, in Vector2Int b)
    {
        return new Vector2Int(a.x + b.x, a.y + b.y);
    }
    public static Vector2Int operator *(in Vector2Int a, in int b)
    {
        return new Vector2Int(a.x * b, a.y * b);
    }
    public static Vector2Int operator *(in int b, in Vector2Int a)
    {
        return new Vector2Int(a.x * b, a.y * b);
    }
    public static Vector2Int operator /(in Vector2Int a, in int b)
    {
        return new Vector2Int(a.x / b, a.y / b);
    }

    public static float DistanceSqrd(in Vector2Int a, in Vector2Int b)
    {
        float dx = b.x - a.x;
        float dy = b.y - a.y;
        return dx * dx + dy * dy;
    }
}

public struct Vector3x3
{
    /// <summary>
    /// row 0, col 0 to 2. row indexing is from top to bottom. column indexing is from left to right
    /// </summary>
    public Vector3d v1;
    /// <summary>
    /// row 1 col 0 to 2. row indexing is from top to bottom. column indexing is from left to right
    /// </summary>
    public Vector3d v2;
    /// <summary>
    /// row 2 col 0 to 2. row indexing is from top to bottom. column indexing is from left to right
    /// </summary>
    public Vector3d v3;
}

/// <summary>
/// 3 coordinate vector in double precision
/// </summary>
[System.Serializable]
public struct Vector3d : IEquatable<Vector3d>
{
    public double x;
    public double y;
    public double z;

    public Vector3d(in Vector3 v)
    {
        x = v.x; y = v.y; z = v.z;
    }
    public Vector3d(in double X, in double Y, in double Z)
    {
        x = X; y = Y; z = Z;
    }
    public Vector3d(in double fill)
    {
        x = fill; y = fill; z = fill;
    }

    public static Vector3d zero
    {
        get
        {
            return new Vector3d(0, 0, 0);
        }
    }

    public bool Equals(Vector3d v)
    {
        return v.x == x && v.y == y && v.z == z;
    }
    public override bool Equals(object obj)
    {
        return obj is Vector3d ? Equals((Vector3d)obj) : false;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 29;
            hash = hash * 17 + x.GetHashCode();
            hash = hash * 19 + y.GetHashCode();
            hash = hash * 19 + z.GetHashCode();
            return hash;
        }
    }
    public static bool operator ==(in Vector3d v1, in Vector3d v2)
    {
        return v1.Equals(v2);
    }
    public static bool operator !=(in Vector3d v1, in Vector3d v2)
    {
        return !(v1 == v2);
    }

    public static Vector3d Cross(in Vector3d a, in Vector3d b)
    {
        return new Vector3d(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    }
    public static double Dot(in Vector3d a, in Vector3d b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }
    public static Vector3d Cross(in Vector3 a, in Vector3 b)
    {
        return new Vector3d(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    }
    public static double Distance(in Vector3d a, in Vector3d b)
    {
        return Math.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y) + (b.z - a.z) * (b.z - a.z));
    }
    public static Vector3d Normalize(in Vector3d v)
    {
        return v.normalized;
    }
    public static float Dot(in Vector3 a, in Vector3 b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }
    public override string ToString()
    {
        return "Vector3d(" + x.ToString() + "," + y.ToString() + "," + z.ToString() + ")";
    }
    public Vector3d normalized
    {
        get
        {
            double mag = magnitude;
            return new Vector3d(x / mag, y / mag, z / mag);
        }
    }
    /// <summary>
    /// Get/set magnitude. Setting magnitude will not change direction;
    /// </summary>
    public double magnitude
    {
        get
        {
            return System.Math.Sqrt(x * x + y * y + z * z);
        }
        set
        {
            var v = new Vector3d(x, y, z);
            v = v == Vector3.zero ? v :
            v * Math.Sqrt((value * value) / (v.x * v.x + v.y * v.y + v.z * v.z));
            x = v.x; y = v.y; z = v.z;
        }
    }
    public double sqrMagnitude
    {
        get
        {
            return x * x + y * y + z * z;
        }
    }
    /// <summary>
    /// return this vector with all components positive
    /// </summary>
    public Vector3d Abs
    {
        get
        {
            return new Vector3d(Math.Abs(x), Math.Abs(y), Math.Abs(z));
        }
    }

    /// <summary>
    /// CLamp this vector to be between min and max
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public Vector3d Clamp(in Vector3d min, in Vector3d max)
    {
        return new Vector3d(
            x < min.x ? min.x : (x > max.x ? max.x : x),
            y < min.y ? min.y : (y > max.y ? max.y : y),
            z < min.z ? min.z : (z > max.z ? max.z : z));
    }

    /// <summary>
    /// Return true if input vectors are approximately equal
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    public bool ApproximatelyEqual(in Vector3d v, in double e = 0.00001)
    {
        double dx = x - v.x, dy = y -v.y, dz = z - v.z;
        return dx < e && dx > -e && dy < e && dy > -e && dz < e && dz > -e;
    }

    public static Vector3d operator-(in Vector3d a, in Vector3d b)
    {
        return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
    }
    public static Vector3d operator-(in Vector3d a)
    {
        return new Vector3d(-a.x, -a.y, -a.z);
    }
    public static Vector3d operator+(in Vector3d a, in Vector3d b)
    {
        return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    public static Vector3d operator*(in Vector3d a, in double b)
    {
        return new Vector3d(a.x * b, a.y * b, a.z * b);
    }
    public static Vector3d operator*(in double b, in Vector3d a)
    {
        return new Vector3d(a.x * b, a.y * b, a.z * b);
    }
    public static Vector3d operator/(in Vector3d a, in double b)
    {
        return new Vector3d(a.x / b, a.y / b, a.z / b);
    }
    public static Vector3d operator/(in double a, in Vector3d v)
    {
        return new Vector3d(a / v.x, a / v.y, a / v.z);
    }
    /// <summary>
    /// Convert to Vector3 (loose precision)
    /// </summary>
    public Vector3 Vec3
    {
        get
        {
            return new Vector3((float)x, (float)y, (float)z);
        }
    }

    public Vector2d ToVector2XZ() { return new Vector2d(x, z); }

    public static Vector3d one
    {
        get
        {
            return new Vector3d(1, 1, 1);
        }
    }

    public static Vector3d forward
    {
        get
        {
            return new Vector3d(0, 0, 1);
        }
    }
    public static Vector3d back
    {
        get
        {
            return new Vector3d(0, 0, -1);
        }
    }
    public static Vector3d right
    {
        get
        {
            return new Vector3d(1, 0, 0);
        }
    }
    public static Vector3d left
    {
        get
        {
            return new Vector3d(-1, 0, 0);
        }
    }
    public static Vector3d up
    {
        get
        {
            return new Vector3d(0, 1, 0);
        }
    }
    public static Vector3d down
    {
        get
        {
            return new Vector3d(0, -1, 0);
        }
    }
    public static explicit operator Vector3(in Vector3d v)
    {
        return v.Vec3;
    }
    public static implicit operator Vector3d(in Vector3 v)
    {
        return new Vector3d(v);
    }
    public static implicit operator Vector3d(in Vector3Int v)
    {
        return new Vector3d(v.x, v.y, v.z);
    }
}

/// <summary>
/// two coordinated double precision vector
/// </summary>
[System.Serializable]
public struct Vector2d
{
    public double x;
    public double y;

    public Vector2d(in Vector2 v)
    {
        x = v.x; y = v.y;
    }
    public Vector2d(in double X, in double Y)
    {
        x = X; y = Y;
    }

    public static Vector2d zero
    {
        get
        {
            return new Vector2d(0, 0);
        }
    }

    public bool Equals(Vector2d v)
    {
        return v.x == x && v.y == y;
    }
    public override bool Equals(object obj)
    {
        return obj is Vector2d ? Equals((Vector2d)obj) : false;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 29;
            hash = hash * 17 + x.GetHashCode();
            hash = hash * 19 + y.GetHashCode();
            return hash;
        }
    }
    public static bool operator ==(in Vector2d v1, in Vector2d v2)
    {
        return v1.Equals(v2);
    }
    public static bool operator !=(in Vector2d v1, in Vector2d v2)
    {
        return !(v1 == v2);
    }

    public static double Distance(in Vector2d a, in Vector2d b)
    {
        return Math.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y));
    }
    public override string ToString()
    {
        return "Vector3d(" + x.ToString() + "," + y.ToString() + ")";
    }
    public Vector2d normalized
    {
        get
        {
            double mag = magnitude;
            return new Vector2d(x / mag, y / mag);
        }
    }
    public double magnitude
    {
        get
        {
            return System.Math.Sqrt(x * x + y * y);
        }
    }
    public double sqrMagnitude
    {
        get
        {
            return x * x + y * y;
        }
    }

    public static Vector2d operator -(in Vector2d a, in Vector2d b)
    {
        return new Vector2d(a.x - b.x, a.y - b.y);
    }
    public static Vector2d operator -(in Vector2d a)
    {
        return new Vector2d(-a.x, -a.y);
    }
    public static Vector2d operator +(in Vector2d a, in Vector3d b)
    {
        return new Vector2d(a.x + b.x, a.y + b.y);
    }
    public static Vector2d operator +(in Vector2d a, in Vector2d b)
    {
        return new Vector2d(a.x + b.x, a.y + b.y);
    }
    public static Vector2d operator *(in Vector2d a, in double b)
    {
        return new Vector2d(a.x * b, a.y * b);
    }
    public static Vector2d operator *(in double b, in Vector2d a)
    {
        return new Vector2d(a.x * b, a.y * b);
    }
    public static Vector2d operator /(in Vector2d a, in double b)
    {
        return new Vector2d(a.x / b, a.y / b);
    }

    public static double Dot(in Vector2d left, in Vector2d right)
    {
        return left.x * right.x + left.y * right.y;
    }

    /// <summary>
    /// Convert to Vector3 (loose precision)
    /// </summary>
    public Vector2 Vec2
    {
        get
        {
            return new Vector2((float)x, (float)y);
        }
    }

    /// <summary>
    /// angles in degrees
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static double Angle(in Vector2d a, in Vector2d b)
    {
        Vector2d c = b - a;
        double angle = Math.Atan2(c.y, c.x) * mathconst.rad2degd;
        if (angle < 0)
            angle = 360 + angle;
        return angle;
    }

    public static Vector2d right
    {
        get
        {
            return new Vector2d(1, 0);
        }
    }
    public static Vector2d left
    {
        get
        {
            return new Vector2d(-1, 0);
        }
    }
    public static Vector2d up
    {
        get
        {
            return new Vector2d(0, 1);
        }
    }
    public static Vector2d down
    {
        get
        {
            return new Vector2d(0, -1);
        }
    }
    public static explicit operator Vector2(in Vector2d v)
    {
        return v.Vec2;
    }
    public static implicit operator Vector2d(in Vector2 v)
    {
        return new Vector2d(v);
    }

    public Vector3d ToVector3dXZ() { return new Vector3d(x, 0, y); }
}

[System.Serializable]
public struct Vector4Int : IEquatable<Vector4Int>
{
    public int x;
    public int y;
    public int z;
    public int w;

    public Vector4Int(in Vector4 v)
    {
        x = (int)v.x; y = (int)v.y; z = (int)v.z; w = (int)v.w;
    }
    public Vector4Int(in int fill)
    {
        x = fill; y = fill; z = fill; w = fill;
    }
    public Vector4Int(in int X, in int Y, in int Z, in int W)
    {
        x = X; y = Y; z = Z; w = W;
    }
    public Vector4 ToVector4
    {
        get
        {
            return new Vector4(x, y, z, w);
        }
    }

    public static Vector4Int zero
    {
        get
        {
            return new Vector4Int(0, 0, 0, 0);
        }
    }

    public bool Equals(Vector4Int v)
    {
        return v.x == x && v.y == y && v.z == z && v.w == w;
    }
    public override bool Equals(object obj)
    {
        return obj is Vector4Int ? Equals((Vector4Int)obj) : false;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 29;
            hash = hash * 17 + w.GetHashCode();
            hash = hash * 17 + z.GetHashCode();
            hash = hash * 17 + y.GetHashCode();
            hash = hash * 17 + x.GetHashCode();
            return hash;
        }
    }
    public static bool operator ==(in Vector4Int v1, in Vector4Int v2)
    {
        return v1.Equals(v2);
    }
    public static bool operator !=(in Vector4Int v1, in Vector4Int v2)
    {
        return !(v1 == v2);
    }

    public static Vector4Int operator -(in Vector4Int a, in Vector4Int b)
    {
        return new Vector4Int(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
    }
    public static Vector4Int operator -(in Vector4Int a)
    {
        return new Vector4Int(-a.x, -a.y, -a.z, -a.w);
    }
    public static Vector4Int operator +(in Vector4Int a, in Vector4Int b)
    {
        return new Vector4Int(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
    }
    public static Vector4Int operator *(in Vector4Int a, in int b)
    {
        return new Vector4Int(a.x * b, a.y * b, a.z * b, a.w * b);
    }
    public static Vector4Int operator *(in int b, in Vector4Int a)
    {
        return new Vector4Int(a.x * b, a.y * b, a.z * b, a.w * b);
    }
    public static Vector4Int operator /(in Vector4Int a, in int b)
    {
        return new Vector4Int(a.x / b, a.y / b, a.z / b, a.w / b);
    }

    public static int Dot(in Vector4Int a, in Vector4Int b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
    }

    /// <summary>
    /// Clamp a to be between min and max inclusive
    /// </summary>
    /// <param name="a"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static Vector4Int VectorClamp(in Vector4Int a, in Vector4Int min, in Vector4Int max)
    {
        return new Vector4Int(
            a.x > max.x ? max.x : (a.x < min.x ? min.x : a.x),
            a.y > max.y ? max.y : (a.y < min.y ? min.y : a.y),
            a.z > max.z ? max.z : (a.z < min.z ? min.z : a.z),
            a.w > max.w ? max.w : (a.w < min.w ? min.w : a.w));
    }

    /// <summary>
    /// Abosulte value all components
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector4Int Abs(in Vector4Int v)
    {
        return new Vector4Int(Math.Abs(v.x), Math.Abs(v.y), Math.Abs(v.z), Math.Abs(v.w));
    }

    public override string ToString()
    {
        return "Vector4Int(" + x.ToString() + ", " + y.ToString() + ", " + z.ToString() + ", " + w.ToString() + ")";
    }

    public static explicit operator Vector4Int(in Vector4 v)
    {
        return new Vector4Int((int)v.x, (int)v.y, (int)v.z, (int)v.w);
    }

}

public struct Vector4d
{
    public double x;
    public double y;
    public double z;
    public double w;

    public Vector4d(in double x, in double y, in double z, in double w)
    {
        this.x = x; this.y = y; this.z = z; this.w = w;
    }
}

public static class VectorExtensions
{
    public static bool ApproximatelyEqual(in Vector3 v1, in Vector3 v2, in float e = 0.0001f)
    {
        return approx(v1.x, v2.x, e) && approx(v1.y, v2.y, e) && approx(v1.z, v2.z, e);
    }

    public static bool ApproximatelyEqual(in Vector2 v1, in Vector2 v2, in float e = 0.0001f)
    {
        return approx(v1.x, v2.x, e) && approx(v1.y, v2.y, e);
    }

    static bool approx(in float a, in float b, in float e = 0.001f)
    {
        return System.Math.Abs(a - b) < e;
    }

    public static Vector3 Abs(this Vector3 v)
    {
        return new Vector3(System.Math.Abs(v.x), System.Math.Abs(v.y), System.Math.Abs(v.z));
    }

    public static Vector2 ToVector2XZ(this Vector3 v) { return new Vector2(v.x, v.z); }

    public static Vector3 ToVector3XZ(this Vector2 v, in float y = 0) { return new Vector3(v.x, y, v.y); }

    public static Vector3 MultiplyComponents(in Vector3 a, in Vector3 b) { return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z); }
    public static Vector2 MultiplyComponents(in Vector2 a, in Vector2 b) { return new Vector2(a.x * b.x, a.y * b.y); }

    public static float MaxComponent(this Vector3 v)
    {
        return System.Math.Max(v.x, System.Math.Max(v.y, v.z));
    }
    public static float MaxComponent(this Vector2 v)
    {
        return System.Math.Max(v.x, v.y);
    }

    /// <summary>
    /// Clamp the point to be within 'distance' from 'from'
    /// </summary>
    /// <param name="from"></param>
    /// <param name="distance"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public static Vector3 DistanceClamp(in Vector3 from, in float distance, in Vector3 point)
    {
        var dist_sq = (point - from).sqrMagnitude;
        var req_dist_sq = distance * distance;

        if (dist_sq <= req_dist_sq)
            return point;
        else
        {
            return (point - from).normalized * distance + from;
        }
    }

    public static Vector2 Rotate(this Vector2 v, in float angles_radians)
    {
        var cos_a = (float)System.Math.Cos(angles_radians);
        var sin_a = (float)System.Math.Sin(angles_radians);

        return new Vector2(v.x * cos_a - v.y * sin_a, v.x * sin_a + v.y * cos_a);
    }

    public static Vector4 NewVector4(in Vector2 a, in Vector2 b)
    {
        return new Vector4(a.x, a.y, b.x, b.y);
    }

    static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
    {
        if (val.CompareTo(min) < 0) return min;
        else if (val.CompareTo(max) > 0) return max;
        else return val;
    }

    public static Vector3 Clamp(this Vector3 v, in Vector3 min, in Vector3 max)
    {
        return new Vector3(
            Clamp(v.x, min.x, max.x),
            Clamp(v.y, min.y, max.y),
            Clamp(v.z, min.z, max.z));
    }

    public static Vector2 Clamp(this Vector2 v, in Vector2 min, in Vector2 max)
    {
        return new Vector2(
            Clamp(v.x, min.x, max.x),
            Clamp(v.y, min.y, max.y));
    }
}

#if !UNITY

/// <summary>
/// 3 coordinate vector in double precision
/// </summary>
[System.Serializable]
public struct Vector3 : IEquatable<Vector3>
{
    public float x;
    public float y;
    public float z;

    public Vector3(in Vector3d v)
    {
        x = (float)v.x; y = (float)v.y; z = (float)v.z;
    }
    public Vector3(in float X, in float Y, in float Z)
    {
        x = X; y = Y; z = Z;
    }
    public Vector3(in float fill)
    {
        x = fill; y = fill; z = fill;
    }

    public static Vector3 zero
    {
        get
        {
            return new Vector3(0, 0, 0);
        }
    }

    public bool Equals(Vector3 v)
    {
        return v.x == x && v.y == y && v.z == z;
    }
    public override bool Equals(object obj)
    {
        return obj is Vector3 ? Equals((Vector3)obj) : false;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 29;
            hash = hash * 17 + x.GetHashCode();
            hash = hash * 19 + y.GetHashCode();
            hash = hash * 19 + z.GetHashCode();
            return hash;
        }
    }
    public static bool operator ==(in Vector3 v1, in Vector3 v2)
    {
        return v1.Equals(v2);
    }
    public static bool operator !=(in Vector3 v1, in Vector3 v2)
    {
        return !(v1 == v2);
    }

    public static Vector3 Cross(in Vector3 a, in Vector3 b)
    {
        return new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    }
    public static float Dot(in Vector3 a, in Vector3 b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }
    public static float Distance(in Vector3 a, in Vector3 b)
    {
        return (float)Math.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y) + (b.z - a.z) * (b.z - a.z));
    }
    public static Vector3 Normalize(in Vector3 v)
    {
        return v.normalized;
    }
    public override string ToString()
    {
        return "Vector3(" + x.ToString() + "," + y.ToString() + "," + z.ToString() + ")";
    }
    public Vector3 normalized
    {
        get
        {
            float mag = magnitude;
            return new Vector3(x / mag, y / mag, z / mag);
        }
    }
    /// <summary>
    /// Get/set magnitude. Setting magnitude will not change direction;
    /// </summary>
    public float magnitude
    {
        get
        {
            return (float)System.Math.Sqrt(x * x + y * y + z * z);
        }
        set
        {
            var v = new Vector3(x, y, z);
            v = v == Vector3.zero ? v :
            v * (float)Math.Sqrt((value * value) / (v.x * v.x + v.y * v.y + v.z * v.z));
            x = (float)v.x; y = (float)v.y; z = (float)v.z;
        }
    }
    public float sqrMagnitude
    {
        get
        {
            return x * x + y * y + z * z;
        }
    }
    /// <summary>
    /// return this vector with all components positive
    /// </summary>
    public Vector3 Abs
    {
        get
        {
            return new Vector3(Math.Abs(x), Math.Abs(y), Math.Abs(z));
        }
    }

    /// <summary>
    /// CLamp this vector to be between min and max
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public Vector3 Clamp(in Vector3 min, in Vector3 max)
    {
        return new Vector3(
            x < min.x ? min.x : (x > max.x ? max.x : x),
            y < min.y ? min.y : (y > max.y ? max.y : y),
            z < min.z ? min.z : (z > max.z ? max.z : z));
    }

    /// <summary>
    /// Return true if input vectors are approximately equal
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    public bool ApproximatelyEqual(in Vector3 v, in float e = 0.00001f)
    {
        float dx = x - v.x, dy = y - v.y, dz = z - v.z;
        return dx < e && dx > -e && dy < e && dy > -e && dz < e && dz > -e;
    }

    public static Vector3 operator -(in Vector3 a, in Vector3 b)
    {
        return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }
    public static Vector3 operator -(in Vector3 a)
    {
        return new Vector3(-a.x, -a.y, -a.z);
    }
    public static Vector3 operator +(in Vector3 a, in Vector3 b)
    {
        return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    public static Vector3 operator *(in Vector3 a, in float b)
    {
        return new Vector3(a.x * b, a.y * b, a.z * b);
    }
    public static Vector3 operator *(in float b, in Vector3 a)
    {
        return new Vector3(a.x * b, a.y * b, a.z * b);
    }
    public static Vector3 operator /(in Vector3 a, in float b)
    {
        return new Vector3(a.x / b, a.y / b, a.z / b);
    }
    public static Vector3 operator /(in float a, in Vector3 v)
    {
        return new Vector3(a / v.x, a / v.y, a / v.z);
    }

    public Vector2 ToVector2XZ() { return new Vector2(x, z); }

    public static Vector3 one
    {
        get
        {
            return new Vector3(1, 1, 1);
        }
    }

    public static Vector3 forward
    {
        get
        {
            return new Vector3(0, 0, 1);
        }
    }
    public static Vector3 back
    {
        get
        {
            return new Vector3(0, 0, -1);
        }
    }
    public static Vector3 right
    {
        get
        {
            return new Vector3(1, 0, 0);
        }
    }
    public static Vector3 left
    {
        get
        {
            return new Vector3(-1, 0, 0);
        }
    }
    public static Vector3 up
    {
        get
        {
            return new Vector3(0, 1, 0);
        }
    }
    public static Vector3 down
    {
        get
        {
            return new Vector3(0, -1, 0);
        }
    }
}

/// <summary>
/// two coordinated double precision vector
/// </summary>
[System.Serializable]
public struct Vector2
{
    public float x;
    public float y;

    public Vector2(in Vector2d v)
    {
        x = (float)v.x; y = (float)v.y;
    }
    public Vector2(in float X, in float Y)
    {
        x = X; y = Y;
    }

    public static Vector2 zero
    {
        get
        {
            return new Vector2(0, 0);
        }
    }

    public bool Equals(Vector2 v)
    {
        return v.x == x && v.y == y;
    }
    public override bool Equals(object obj)
    {
        return obj is Vector2d ? Equals((Vector2)obj) : false;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 29;
            hash = hash * 17 + x.GetHashCode();
            hash = hash * 19 + y.GetHashCode();
            return hash;
        }
    }
    public static bool operator ==(in Vector2 v1, in Vector2 v2)
    {
        return v1.Equals(v2);
    }
    public static bool operator !=(in Vector2 v1, in Vector2 v2)
    {
        return !(v1 == v2);
    }

    public static float Distance(in Vector2 a, in Vector2 b)
    {
        return (float)Math.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y));
    }
    public override string ToString()
    {
        return "Vector3d(" + x.ToString() + "," + y.ToString() + ")";
    }
    public Vector2 normalized
    {
        get
        {
            float mag = magnitude;
            return new Vector2(x / mag, y / mag);
        }
    }
    public float magnitude
    {
        get
        {
            return (float)System.Math.Sqrt(x * x + y * y);
        }
    }
    public float sqrMagnitude
    {
        get
        {
            return x * x + y * y;
        }
    }

    public static Vector2 operator -(in Vector2 a, in Vector2 b)
    {
        return new Vector2(a.x - b.x, a.y - b.y);
    }
    public static Vector2 operator -(in Vector2 a)
    {
        return new Vector2(-a.x, -a.y);
    }
    public static Vector2 operator +(in Vector2 a, in Vector3 b)
    {
        return new Vector2(a.x + b.x, a.y + b.y);
    }
    public static Vector2 operator +(in Vector2 a, in Vector2 b)
    {
        return new Vector2(a.x + b.x, a.y + b.y);
    }
    public static Vector2 operator *(in Vector2 a, in float b)
    {
        return new Vector2(a.x * b, a.y * b);
    }
    public static Vector2 operator *(in float b, in Vector2 a)
    {
        return new Vector2(a.x * b, a.y * b);
    }
    public static Vector2 operator /(in Vector2 a, in float b)
    {
        return new Vector2(a.x / b, a.y / b);
    }

    public static double Dot(in Vector2 left, in Vector2 right)
    {
        return left.x * right.x + left.y * right.y;
    }

    /// <summary>
    /// angles in degrees
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static double Angle(in Vector2 a, in Vector2 b)
    {
        Vector2 c = b - a;
        double angle = Math.Atan2(c.y, c.x) * mathconst.rad2deg;
        if (angle < 0)
            angle = 360 + angle;
        return angle;
    }

    public static Vector2 right
    {
        get
        {
            return new Vector2(1, 0);
        }
    }
    public static Vector2 left
    {
        get
        {
            return new Vector2(-1, 0);
        }
    }
    public static Vector2 up
    {
        get
        {
            return new Vector2(0, 1);
        }
    }
    public static Vector2 down
    {
        get
        {
            return new Vector2(0, -1);
        }
    }

    public Vector3d ToVector3dXZ() { return new Vector3d(x, 0, y); }
}

[System.Serializable]
public struct Vector4
{
    public float x;
    public float y;
    public float z;
    public float w;

    public Vector4(in float x, in float y, in float z, in float w)
    {
        this.x = x; this.y = y; this.z = z; this.w = w;
    }
    public Vector4(in float x, in float y, in float z)
    {
        this.x = x; this.y = y; this.z = z; this.w = 0;
    }
}

#endif