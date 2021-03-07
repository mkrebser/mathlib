
public struct Matrix4x4Struct
{
    //https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Matrix4x4.cs

    //labeled row,col

    public float m00;
    public float m01;
    public float m02;
    public float m03;

    public float m10;
    public float m11;
    public float m12;
    public float m13;

    public float m20;
    public float m21;
    public float m22;
    public float m23;

    public float m30;
    public float m31;
    public float m32;
    public float m33;

    // Get inverse of a matrix. Returns identity matrix if not found.
    public Matrix4x4Struct CalculateInverse()
    {
        //ref https://stackoverflow.com/questions/1148309/inverting-a-4x4-matrix

        var A2323 = this.m22 * this.m33 - this.m23 * this.m32;
        var A1323 = this.m21 * this.m33 - this.m23 * this.m31;
        var A1223 = this.m21 * this.m32 - this.m22 * this.m31;
        var A0323 = this.m20 * this.m33 - this.m23 * this.m30;
        var A0223 = this.m20 * this.m32 - this.m22 * this.m30;
        var A0123 = this.m20 * this.m31 - this.m21 * this.m30;
        var A2313 = this.m12 * this.m33 - this.m13 * this.m32;
        var A1313 = this.m11 * this.m33 - this.m13 * this.m31;
        var A1213 = this.m11 * this.m32 - this.m12 * this.m31;
        var A2312 = this.m12 * this.m23 - this.m13 * this.m22;
        var A1312 = this.m11 * this.m23 - this.m13 * this.m21;
        var A1212 = this.m11 * this.m22 - this.m12 * this.m21;
        var A0313 = this.m10 * this.m33 - this.m13 * this.m30;
        var A0213 = this.m10 * this.m32 - this.m12 * this.m30;
        var A0312 = this.m10 * this.m23 - this.m13 * this.m20;
        var A0212 = this.m10 * this.m22 - this.m12 * this.m20;
        var A0113 = this.m10 * this.m31 - this.m11 * this.m30;
        var A0112 = this.m10 * this.m21 - this.m11 * this.m20;

        var det = this.m00 * (this.m11 * A2323 - this.m12 * A1323 + this.m13 * A1223)
            - this.m01 * (this.m10 * A2323 - this.m12 * A0323 + this.m13 * A0223)
            + this.m02 * (this.m10 * A1323 - this.m11 * A0323 + this.m13 * A0123)
            - this.m03 * (this.m10 * A1223 - this.m11 * A0223 + this.m12 * A0123);
        det = 1 / det;

        if (det == 0.0f)
            return Matrix4x4Struct.identity;

        Matrix4x4Struct m;
        m.m00 = det * (this.m11 * A2323 - this.m12 * A1323 + this.m13 * A1223);
        m.m01 = det * -(this.m01 * A2323 - this.m02 * A1323 + this.m03 * A1223);
        m.m02 = det * (this.m01 * A2313 - this.m02 * A1313 + this.m03 * A1213);
        m.m03 = det * -(this.m01 * A2312 - this.m02 * A1312 + this.m03 * A1212);
        m.m10 = det * -(this.m10 * A2323 - this.m12 * A0323 + this.m13 * A0223);
        m.m11 = det * (this.m00 * A2323 - this.m02 * A0323 + this.m03 * A0223);
        m.m12 = det * -(this.m00 * A2313 - this.m02 * A0313 + this.m03 * A0213);
        m.m13 = det * (this.m00 * A2312 - this.m02 * A0312 + this.m03 * A0212);
        m.m20 = det * (this.m10 * A1323 - this.m11 * A0323 + this.m13 * A0123);
        m.m21 = det * -(this.m00 * A1323 - this.m01 * A0323 + this.m03 * A0123);
        m.m22 = det * (this.m00 * A1313 - this.m01 * A0313 + this.m03 * A0113);
        m.m23 = det * -(this.m00 * A1312 - this.m01 * A0312 + this.m03 * A0112);
        m.m30 = det * -(this.m10 * A1223 - this.m11 * A0223 + this.m12 * A0123);
        m.m31 = det * (this.m00 * A1223 - this.m01 * A0223 + this.m02 * A0123);
        m.m32 = det * -(this.m00 * A1213 - this.m01 * A0213 + this.m02 * A0113);
        m.m33 = det * (this.m00 * A1212 - this.m01 * A0212 + this.m02 * A0112);
        return m;
    }

    public Matrix4x4Struct(in Vector3 p, in Quaternion r, in Vector3 s)
    {
        // transform matrix
        float xx = r.x * r.x;
        float xy = r.x * r.y;
        float xz = r.x * r.z;
        float xw = r.x * r.w;
        float yy = r.y * r.y;
        float yz = r.y * r.z;
        float yw = r.y * r.w;
        float zz = r.z * r.z;
        float zw = r.z * r.w;

        m00 = (1 - 2 * (yy + zz)) * s.x;
        m10 = (2 * (xy + zw)) * s.x;
        m20 = (2 * (xz - yw)) * s.x;
        m30 = 0;

        m01 = (2 * (xy - zw)) * s.y;
        m11 = (1 - 2 * (xx + zz)) * s.y;
        m21 = (2 * (yz + xw)) * s.y;
        m31 = 0;

        m02 = (2 * (xz + yw)) * s.z;
        m12 = (2 * (yz - xw)) * s.z;
        m22 = (1 - 2 * (xx + yy)) * s.z;
        m32 = 0;

        m03 = p.x;
        m13 = p.y;
        m23 = p.z;
        m33 = 1;
    }

    public Matrix4x4Struct(in Vector4 column0, in Vector4 column1, in Vector4 column2, in Vector4 column3)
    {
        this.m00 = column0.x; this.m01 = column1.x; this.m02 = column2.x; this.m03 = column3.x;
        this.m10 = column0.y; this.m11 = column1.y; this.m12 = column2.y; this.m13 = column3.y;
        this.m20 = column0.z; this.m21 = column1.z; this.m22 = column2.z; this.m23 = column3.z;
        this.m30 = column0.w; this.m31 = column1.w; this.m32 = column2.w; this.m33 = column3.w;
    }

    public float this[in int row, in int column]
    {
        get
        {
            return this[row + column * 4];
        }

        set
        {
            this[row + column * 4] = value;
        }
    }

    public float this[in int index]
    {
        get
        {
            switch (index)
            {
                case 0: return m00;
                case 1: return m10;
                case 2: return m20;
                case 3: return m30;
                case 4: return m01;
                case 5: return m11;
                case 6: return m21;
                case 7: return m31;
                case 8: return m02;
                case 9: return m12;
                case 10: return m22;
                case 11: return m32;
                case 12: return m03;
                case 13: return m13;
                case 14: return m23;
                case 15: return m33;
                default:
                    throw new System.IndexOutOfRangeException("Invalid matrix index!");
            }
        }

        set
        {
            switch (index)
            {
                case 0: m00 = value; break;
                case 1: m10 = value; break;
                case 2: m20 = value; break;
                case 3: m30 = value; break;
                case 4: m01 = value; break;
                case 5: m11 = value; break;
                case 6: m21 = value; break;
                case 7: m31 = value; break;
                case 8: m02 = value; break;
                case 9: m12 = value; break;
                case 10: m22 = value; break;
                case 11: m32 = value; break;
                case 12: m03 = value; break;
                case 13: m13 = value; break;
                case 14: m23 = value; break;
                case 15: m33 = value; break;

                default:
                    throw new System.IndexOutOfRangeException("Invalid matrix index!");
            }
        }
    }

    public override int GetHashCode()
    {
        return Col0.GetHashCode() ^ (Col1.GetHashCode() << 2) ^ (Col2.GetHashCode() >> 2) ^ (Col3.GetHashCode() >> 1);
    }

    public override bool Equals(object other)
    {
        if (!(other is Matrix4x4Struct)) return false;

        return Equals((Matrix4x4Struct)other);
    }

    public bool Equals(in Matrix4x4Struct other)
    {
        return Col0.Equals(other.Col0)
            && Col1.Equals(other.Col1)
            && Col2.Equals(other.Col2)
            && Col3.Equals(other.Col3);
    }

    public Vector4 Col0 { get { return new Vector4(m00, m10, m20, m30); } set { m00 = value.x; m10 = value.y; m20 = value.z; m30 = value.w; } }
    public Vector4 Col1 { get { return new Vector4(m01, m11, m21, m31); } set { m01 = value.x; m11 = value.y; m21 = value.z; m31 = value.w; } }
    public Vector4 Col2 { get { return new Vector4(m02, m12, m22, m32); } set { m02 = value.x; m12 = value.y; m22 = value.z; m32 = value.w; } }
    public Vector4 Col3 { get { return new Vector4(m03, m13, m23, m33); } set { m03 = value.x; m13 = value.y; m23 = value.z; m33 = value.w; } }

    public Vector4 Row0 { get { return new Vector4(m00, m01, m02, m03); } set { m00 = value.x; m01 = value.y; m02 = value.z; m03 = value.w; } }
    public Vector4 Row1 { get { return new Vector4(m10, m11, m12, m13); } set { m10 = value.x; m11 = value.y; m12 = value.z; m13 = value.w; } }
    public Vector4 Row2 { get { return new Vector4(m20, m21, m22, m23); } set { m20 = value.x; m21 = value.y; m22 = value.z; m23 = value.w; } }
    public Vector4 Row3 { get { return new Vector4(m30, m31, m32, m33); } set { m30 = value.x; m31 = value.y; m32 = value.z; m33 = value.w; } }

    public override string ToString()
    {
        return System.String.Format("{0:F5}\t{1:F5}\t{2:F5}\t{3:F5}\n{4:F5}\t{5:F5}\t{6:F5}\t{7:F5}\n{8:F5}\t{9:F5}\t{10:F5}\t{11:F5}\n{12:F5}\t{13:F5}\t{14:F5}\t{15:F5}\n", m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23, m30, m31, m32, m33);
    }
    public string ToString(in string format)
    {
        return System.String.Format("{0}\t{1}\t{2}\t{3}\n{4}\t{5}\t{6}\t{7}\n{8}\t{9}\t{10}\t{11}\n{12}\t{13}\t{14}\t{15}\n",
            m00.ToString(format), m01.ToString(format), m02.ToString(format), m03.ToString(format),
            m10.ToString(format), m11.ToString(format), m12.ToString(format), m13.ToString(format),
            m20.ToString(format), m21.ToString(format), m22.ToString(format), m23.ToString(format),
            m30.ToString(format), m31.ToString(format), m32.ToString(format), m33.ToString(format));
    }

    public Vector3 MultiplyPoint3x4(in Vector3 point)
    {
        Vector3 res;
        res.x = this.m00 * point.x + this.m01 * point.y + this.m02 * point.z + this.m03;
        res.y = this.m10 * point.x + this.m11 * point.y + this.m12 * point.z + this.m13;
        res.z = this.m20 * point.x + this.m21 * point.y + this.m22 * point.z + this.m23;
        return res;
    }

    // Multiplies two matrices.
    public static Matrix4x4Struct operator *(in Matrix4x4Struct lhs, in Matrix4x4Struct rhs)
    {
        Matrix4x4Struct res;
        res.m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20 + lhs.m03 * rhs.m30;
        res.m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21 + lhs.m03 * rhs.m31;
        res.m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22 + lhs.m03 * rhs.m32;
        res.m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03 * rhs.m33;

        res.m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20 + lhs.m13 * rhs.m30;
        res.m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21 + lhs.m13 * rhs.m31;
        res.m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22 + lhs.m13 * rhs.m32;
        res.m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13 * rhs.m33;

        res.m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20 + lhs.m23 * rhs.m30;
        res.m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21 + lhs.m23 * rhs.m31;
        res.m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22 + lhs.m23 * rhs.m32;
        res.m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23 * rhs.m33;

        res.m30 = lhs.m30 * rhs.m00 + lhs.m31 * rhs.m10 + lhs.m32 * rhs.m20 + lhs.m33 * rhs.m30;
        res.m31 = lhs.m30 * rhs.m01 + lhs.m31 * rhs.m11 + lhs.m32 * rhs.m21 + lhs.m33 * rhs.m31;
        res.m32 = lhs.m30 * rhs.m02 + lhs.m31 * rhs.m12 + lhs.m32 * rhs.m22 + lhs.m33 * rhs.m32;
        res.m33 = lhs.m30 * rhs.m03 + lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33 * rhs.m33;

        return res;
    }

    // Transforms a [[Vector4]] by a matrix.
    public static Vector4 operator *(in Matrix4x4Struct lhs, in Vector4 vector)
    {
        Vector4 res;
        res.x = lhs.m00 * vector.x + lhs.m01 * vector.y + lhs.m02 * vector.z + lhs.m03 * vector.w;
        res.y = lhs.m10 * vector.x + lhs.m11 * vector.y + lhs.m12 * vector.z + lhs.m13 * vector.w;
        res.z = lhs.m20 * vector.x + lhs.m21 * vector.y + lhs.m22 * vector.z + lhs.m23 * vector.w;
        res.w = lhs.m30 * vector.x + lhs.m31 * vector.y + lhs.m32 * vector.z + lhs.m33 * vector.w;
        return res;
    }

    // Transforms a position by this matrix, with a perspective divide. (generic)
    public Vector3 MultiplyPoint(in Vector3 point)
    {
        Vector3 res;
        float w;
        res.x = this.m00 * point.x + this.m01 * point.y + this.m02 * point.z + this.m03;
        res.y = this.m10 * point.x + this.m11 * point.y + this.m12 * point.z + this.m13;
        res.z = this.m20 * point.x + this.m21 * point.y + this.m22 * point.z + this.m23;
        w = this.m30 * point.x + this.m31 * point.y + this.m32 * point.z + this.m33;

        w = 1F / w;
        res.x *= w;
        res.y *= w;
        res.z *= w;
        return res;
    }

    // Transforms a direction by this matrix.
    public Vector3 MultiplyVector(in Vector3 vector)
    {
        Vector3 res;
        res.x = this.m00 * vector.x + this.m01 * vector.y + this.m02 * vector.z;
        res.y = this.m10 * vector.x + this.m11 * vector.y + this.m12 * vector.z;
        res.z = this.m20 * vector.x + this.m21 * vector.y + this.m22 * vector.z;
        return res;
    }

#pragma warning disable 414
    static readonly Matrix4x4Struct identity = new Matrix4x4Struct(
        new Vector4(1, 0, 0, 0),
        new Vector4(0, 1, 0, 0),
        new Vector4(0, 0, 1, 0),
        new Vector4(0, 0, 0, 1));
#pragma warning restore 414
}
