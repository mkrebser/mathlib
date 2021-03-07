using System.Collections.Generic;
using System;

/// <summary>
/// A box with rotation.
/// </summary>
public struct Box : IEquatable<Box>
{
    //  points
    //
    //    3---------7
    //   / |       /|
    //  2---------6 |
    //  |  |      | |
    //  |  1------|-5
    //  | /       |/
    //  0_________4
    //

    /// <summary>
    /// All vertices for this box
    /// </summary>
    public IEnumerable<Vector3d> Verts
    {
        get
        {
            Vector3d rotation = Rotation.eulerAngles;
            Vector3d s2 = Size / 2;
            Vector3d v;
            v = new Vector3d(Center.x - s2.x, Center.y - s2.y, Center.z - s2.z);
            yield return Geometry.TransformPoint(v, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            v = new Vector3d(Center.x - s2.x, Center.y - s2.y, Center.z + s2.z);
            yield return Geometry.TransformPoint(v, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            v = new Vector3d(Center.x - s2.x, Center.y + s2.y, Center.z - s2.z);
            yield return Geometry.TransformPoint(v, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            v = new Vector3d(Center.x - s2.x, Center.y + s2.y, Center.z + s2.z);
            yield return Geometry.TransformPoint(v, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            v = new Vector3d(Center.x + s2.x, Center.y - s2.y, Center.z - s2.z);
            yield return Geometry.TransformPoint(v, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            v = new Vector3d(Center.x + s2.x, Center.y - s2.y, Center.z + s2.z);
            yield return Geometry.TransformPoint(v, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            v = new Vector3d(Center.x + s2.x, Center.y + s2.y, Center.z - s2.z);
            yield return Geometry.TransformPoint(v, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            v = new Vector3d(Center.x + s2.x, Center.y + s2.y, Center.z + s2.z);
            yield return Geometry.TransformPoint(v, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
        }
    }

    /// <summary>
    /// Rotation of the box
    /// </summary>
    public Quaterniond Rotation;
    /// <summary>
    /// Size of the box in X x Y x Z
    /// </summary>
    public Vector3d Size;
    /// <summary>
    /// Position of the box. Aligned by center.
    /// </summary>
    public Vector3d Center;

    /// <summary>
    /// Position == Center. Position of the box
    /// </summary>
    public Vector3d Position
    {
        get
        {
            return Center;
        }
        set
        {
            Center = value;
        }
    }

    public Box (Vector3d position, Vector3d size)
    {
        Rotation = Quaterniond.identity;
        Size = size;
        Center = position;
    }

    public Box(Vector3d position, Vector3d size, Quaterniond rot)
    {
        Rotation = Quaterniond.identity;
        Size = size;
        Center = position;
        Rotation = rot;
    }

    /// <summary>
    /// Unrotated bbox that represents the Bounds of this rotated bbox
    /// </summary>
    public BBox Bounds
    {
        get
        {
            Vector3d rotation = Rotation.eulerAngles;
            Vector3d s2 = Size / 2;
            Vector3d v0 = new Vector3d(Center.x - s2.x, Center.y - s2.y, Center.z - s2.z);
            v0 = Geometry.TransformPoint(v0, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            Vector3d v1 = new Vector3d(Center.x - s2.x, Center.y - s2.y, Center.z + s2.z);
            v1 = Geometry.TransformPoint(v1, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            Vector3d v2 = new Vector3d(Center.x - s2.x, Center.y + s2.y, Center.z - s2.z);
            v2 = Geometry.TransformPoint(v2, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            Vector3d v3 = new Vector3d(Center.x - s2.x, Center.y + s2.y, Center.z + s2.z);
            v3 = Geometry.TransformPoint(v3, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            Vector3d v4 = new Vector3d(Center.x + s2.x, Center.y - s2.y, Center.z - s2.z);
            v4 = Geometry.TransformPoint(v4, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            Vector3d v5 = new Vector3d(Center.x + s2.x, Center.y - s2.y, Center.z + s2.z);
            v5 = Geometry.TransformPoint(v5, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            Vector3d v6 = new Vector3d(Center.x + s2.x, Center.y + s2.y, Center.z - s2.z);
            v6 = Geometry.TransformPoint(v6, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            Vector3d v7 = new Vector3d(Center.x + s2.x, Center.y + s2.y, Center.z + s2.z);
            v7 = Geometry.TransformPoint(v7, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));

            Vector3d minv = VecMin(
                VecMin(VecMin(v0, v1), VecMin(v2, v3)),
                VecMin(VecMin(v4, v5), VecMin(v6, v7)));
            Vector3d maxv = VecMax(
                VecMax(VecMax(v0, v1), VecMax(v2, v3)),
                VecMax(VecMax(v4, v5), VecMax(v6, v7)));
            return new BBox(minv, maxv - minv);
        }
    }
    /// <summary>
    /// Bounding sphere for this box (fast)
    /// </summary>
    public Sphere BoundingSphere
    {
        get
        {
            return new Sphere(Center, Size.magnitude / 2);
        }
    }

    Vector3d VecMin(Vector3d a, Vector3d b)
    {
        return new Vector3d(System.Math.Min(a.x, b.x), System.Math.Min(a.y, b.y), System.Math.Min(a.z, b.z));
    }
    Vector3d VecMax(Vector3d a, Vector3d b)
    {
        return new Vector3d(System.Math.Max(a.x, b.x), System.Math.Max(a.y, b.y), System.Math.Max(a.z, b.z));
    }

    public bool Equals(Box b)
    {
        return b.Center == Center && b.Size == Size && b.Rotation == Rotation;
    }
    public override bool Equals(object obj)
    {
        return obj is Box ? Equals((Box)obj) : false;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 29;
            hash = hash * 17 + Center.GetHashCode();
            hash = hash * 17 + Size.GetHashCode();
            hash = hash * 17 + Rotation.GetHashCode();
            return hash;
        }
    }
    public static bool operator==(Box a, Box b)
    {
        return a.Equals(b);
    }
    public static bool operator !=(Box a, Box b)
    {
        return !a.Equals(b);
    }

    /// <summary>
    /// Create and return  vertices array
    /// </summary>
    public Vector3d[] VertCopy
    {
        get
        {
            Vector3d[] vs = new Vector3d[8];
            Vector3d rotation = Rotation.eulerAngles;
            Vector3d s2 = Size / 2;
            Vector3d v;
            v = new Vector3d(Center.x - s2.x, Center.y - s2.y, Center.z - s2.z);
            vs[0] = Geometry.TransformPoint(v, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            v = new Vector3d(Center.x - s2.x, Center.y - s2.y, Center.z + s2.z);
            vs[1] = Geometry.TransformPoint(v, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            v = new Vector3d(Center.x - s2.x, Center.y + s2.y, Center.z - s2.z);
            vs[2] = Geometry.TransformPoint(v, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            v = new Vector3d(Center.x - s2.x, Center.y + s2.y, Center.z + s2.z);
            vs[3] = Geometry.TransformPoint(v, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            v = new Vector3d(Center.x + s2.x, Center.y - s2.y, Center.z - s2.z);
            vs[4] = Geometry.TransformPoint(v, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            v = new Vector3d(Center.x + s2.x, Center.y - s2.y, Center.z + s2.z);
            vs[5] = Geometry.TransformPoint(v, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            v = new Vector3d(Center.x + s2.x, Center.y + s2.y, Center.z - s2.z);
            vs[6] = Geometry.TransformPoint(v, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            v = new Vector3d(Center.x + s2.x, Center.y + s2.y, Center.z + s2.z);
            vs[7]= Geometry.TransformPoint(v, Center, Vector3d.zero, rotation, new Vector3d(1, 1, 1));
            return vs;
        }
    }

    /// <summary>
    /// Intersects an input box
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool IntersectsBox(Box b)
    {
        if (!valid || !b.valid)
            throw new System.Exception("Error input Box has no size.");
        if (!BoundingSphere.IntersectsSphere(b.BoundingSphere))
            return false;

        //get points
        Vector3d[] points1 = VertCopy;
        Vector3d[] points2 = b.VertCopy;

        GJK gjk = new GJK();
        return gjk.intersect(points1, points2);
    }
    /// <summary>
    /// Intersects an input BBox?
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool IntersectsBBox(BBox b)
    {
        if (!valid || !b.valid)
            throw new System.Exception("Error input Box has no size.");
        if (!BoundingSphere.IntersectsSphere(b.BoundingSphere))
            return false;

        //get points
        Vector3d[] points1 = VertCopy;
        Vector3d[] points2 = b.VertCopy;

        GJK gjk = new GJK();
        return gjk.intersect(points1, points2);
    }
    /// <summary>
    /// Returns true if this box intersects the input sphere
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public bool IntersectsSphere(Sphere s)
    {
        if (s.Radius == 0)
            throw new System.Exception("Sphere radius cannot be zero");
        if (!valid)
            throw new System.Exception("Error input Box has no size.");
        if (!s.IntersectsSphere(BoundingSphere))
            return false;

        Vector3d[] points1 = VertCopy;
        GJK gjk = new GJK();
        return gjk.intersect(points1, s);
        //uses GJK algorithm
        //see https://mollyrocket.com/849 for a video tutorial

        //i translated this c++ implemtation
        //http://vec3.ca/gjk/implementation/
    }
    /// <summary>
    /// Returns true if this box intersects a capsule
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public bool IntersectsCapsule(Capsule c)
    {
        return c.IntersectsBox(new Box(Position, Size, Rotation));
    }
    /// <summary>
    /// Intersects polygon?
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public bool IntersectsPolygon(Polygon3D p)
    {
        if (p == null)
            throw new System.Exception("null polygon exception");
        return p.IntersectsBox(new Box(Position, Size, Rotation));
    }


    public bool valid
    {
        get
        {
            return Size.x > 0 && Size.y > 0 && Size.z > 0;
        }
    }
}
