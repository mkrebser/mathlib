using System;

public struct Capsule : IEquatable<Capsule>
{
    public double Height;
    public double Radius;
    public Vector3d Center;
    public Quaterniond Rotation;

    /// <summary>
    /// Center of top sphere
    /// </summary>
    public Vector3d Up
    {
        get
        {
            double halfheight = Height / 2;
            var up = Rotation * Vector3.up;
            return Center + up * halfheight;
        }
    }
    /// <summary>
    /// Center of bottom sphere
    /// </summary>
    public Vector3d Down
    {
        get
        {
            double halfheight = Height / 2;
            var down = Rotation * -Vector3.up;
            return Center + down * halfheight;
        }
    }

    /// <summary>
    /// BBox surrounding capsule
    /// </summary>
    public BBox Bounds
    {
        get
        {
            return bounds(Up, Down);
        }
    }

    /// <summary>
    /// Bounding sphere for the capsule
    /// </summary>
    public Sphere BoundingSphere
    {
        get
        {
            return new Sphere(Center, Height / 2 + Radius);
        }
    }

    public bool valid
    {
        get
        {
            return Height >= 0 && Radius >= 0;
        }
    }

    BBox bounds(Vector3d up, Vector3d down)
    {
        Vector3d max = new Vector3d(Math.Max(up.x + Radius, down.x + Radius),
                                    Math.Max(up.y + Radius, down.y + Radius),
                                    Math.Max(up.z + Radius, down.z + Radius));
        Vector3d min = new Vector3d(Math.Min(up.x - Radius, down.x - Radius),
                                    Math.Min(up.y - Radius, down.y - Radius),
                                    Math.Min(up.z - Radius, down.z - Radius));
        return new BBox(min, max - min);
    }

    public Capsule(double height, double radius)
    {
        Height = height;
        Radius = radius;
        Center = Vector3d.zero;
        Rotation = Quaterniond.identity;
    }
    public Capsule(Vector3d position, double height, double radius)
    {
        Height = height;
        Radius = radius;
        Center = position;
        Rotation = Quaterniond.identity;
    }
    public Capsule(Vector3d position, Quaterniond rotation, double height, double radius)
    {
        Height = height;
        Radius = radius;
        Center = position;
        Rotation = rotation;
    }

    public static Capsule FromSphereCast(Sphere s, Vector3d direction, double distance)
    {
        var forward = Vector3d.Cross(direction, Vector3d.Cross(direction, Vector3d.up).normalized).normalized;
        var up = direction;
        return new Capsule(s.Center+ direction * distance * 0.5, Quaterniond.LookRotation(forward, up), distance, s.Radius);
    }

    /// <summary>
    /// Returns true if point is inside or on the surface of capsule
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool ContainsPoint(Vector3d point)
    {
        return Geometry.LineSegmentPointDistanceSqrd(Up, Down, point) <= Radius * Radius;
    }

    /// <summary>
    /// Intersects sphere?
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public bool IntersectsSphere(Sphere s)
    {
        if (s.Radius == 0 || Radius == 0)
            throw new System.Exception("Error, zero radius");
        double d = Geometry.LineSegmentPointDistance(Up, Down, s.Center);
        return d <= s.Radius + Radius;
    }
    /// <summary>
    /// Intersects bounding box?
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool IntersectsBBox(BBox b)
    {
        if (b.Size.x == 0 || b.Size.y == 0 || b.Size.z == 0)
            throw new System.Exception("Error, zero box size");
        if (Radius == 0)
            throw new System.Exception("Error, zero radius");
        Vector3d up = Up;
        Vector3d down = Down;

        //do a quick bbox test
        if (!b.Intersects(bounds(up, down)))
            return false;

        //check if capsule spheres intersect bbox
        if (b.IntersectsSphere(new Sphere(up, Radius)))
            return true;
        if (b.IntersectsSphere(new Sphere(down, Radius)))
            return true;

        //we tested the endpoint of the capsule, so this is now a cyclinder bbox test
        //check if cyclinder line segment intersects bbox planes
        if (Geometry.RayBBoxIntersect(b, new Ray_ex(up, (down - up).normalized), Height))
            return true;

        //  points
        //
        //    3---------7
        //   / |       /|        y
        //  2---------6 |        |
        //  |  |      | |        *---x
        //  |  1------|-5       /
        //  | /       |/       z
        //  0_________4
        //
        //check that all distance between bbox edges and capsule segment is less than r
        double radsq = Radius * Radius;
        //p0p1
        Vector3d p0 = b.Min;
        Vector3d p1 = new Vector3d(b.Min.x, b.Min.y, b.Min.z + b.Size.z);
        Vector3d p2 = new Vector3d(b.Min.x, b.Min.y + b.Size.y, b.Min.z);
        Vector3d p3 = new Vector3d(b.Min.x, b.Min.y + b.Size.z, b.Min.z + b.Size.z);
        Vector3d p4 = new Vector3d(b.Min.x + b.Size.x, b.Min.y, b.Min.z);
        Vector3d p5 = new Vector3d(b.Min.x + b.Size.x, b.Min.y, b.Min.z + b.Size.z);
        Vector3d p6 = new Vector3d(b.Min.x + b.Size.x, b.Min.y + b.Size.y, b.Min.z);
        Vector3d p7 = b.Max;
        if (Geometry.LineSegLineSegDistanceSqrd(up, down, p0, p1) <= radsq)
            return true;
        if (Geometry.LineSegLineSegDistanceSqrd(up, down, p0, p2) <= radsq)
            return true;
        if (Geometry.LineSegLineSegDistanceSqrd(up, down, p0, p4) <= radsq)
            return true;
        if (Geometry.LineSegLineSegDistanceSqrd(up, down, p7, p3) <= radsq)
            return true;
        if (Geometry.LineSegLineSegDistanceSqrd(up, down, p7, p5) <= radsq)
            return true;
        if (Geometry.LineSegLineSegDistanceSqrd(up, down, p7, p6) <= radsq)
            return true;
        if (Geometry.LineSegLineSegDistanceSqrd(up, down, p2, p3) <= radsq)
            return true;
        if (Geometry.LineSegLineSegDistanceSqrd(up, down, p2, p6) <= radsq)
            return true;
        if (Geometry.LineSegLineSegDistanceSqrd(up, down, p5, p1) <= radsq)
            return true;
        if (Geometry.LineSegLineSegDistanceSqrd(up, down, p5, p4) <= radsq)
            return true;
        if (Geometry.LineSegLineSegDistanceSqrd(up, down, p6, p4) <= radsq)
            return true;
        if (Geometry.LineSegLineSegDistanceSqrd(up, down, p3, p1) <= radsq)
            return true;

        //otherwise false
        return false;
    }
    /// <summary>
    /// Intersects box?
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool IntersectsBox(Box b)
    {
        if (b.Size.x == 0 || b.Size.y == 0 || b.Size.z == 0)
            throw new System.Exception("Error, zero box size");
        if (!valid)
            throw new System.Exception("invalid capsule");
        if (!IntersectsSphere(b.BoundingSphere))
            return false;

        GJK gjk = new GJK();
        Vector3d[] points = b.VertCopy;
        return gjk.intersect(points, new Capsule(Center, Rotation, Height, Radius));
    }
    /// <summary>
    /// Intersects capsule?
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public bool IntersectsCapsule(Capsule c)
    {
        if (!valid || !c.valid)
            throw new System.Exception("Invalid capsule exception");
        double d = Geometry.LineSegLineSegDistanceSqrd(Up, Down, c.Up, c.Down);
        return d <= Math.Max(Radius * Radius,  c.Radius * c.Radius);
    }
    /// <summary>
    /// Intersects polygon?
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public bool IntersectsPolygon3D(Polygon3D p)
    {
        if (p == null)
            throw new System.Exception("null polygon exception");
        return p.IntersectsCapsule(new Capsule(Center, Rotation, Height, Radius));
    }

    bool ApproxZero(double d, double epsilon)
    {
        return d - epsilon < 0 && d + epsilon > 0;
    }

    public bool Equals(Capsule s)
    {
        return s.Center == Center && s.Height == Height && s.Rotation == Rotation && s.Radius == Radius;
    }
    public override bool Equals(object obj)
    {
        return obj is Capsule ? Equals((Capsule)obj) : false;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 29;
            hash = hash * 17 + Radius.GetHashCode();
            hash = hash * 17 + Center.GetHashCode();
            hash = hash * 17 + Rotation.GetHashCode();
            hash = hash * 17 + Height.GetHashCode();
            return hash;
        }
    }
}
