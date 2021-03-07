using System;

public struct Sphere : IEquatable<Sphere>
{
    /// <summary>
    /// Center == Position
    /// </summary>
    public Vector3d Center;
    /// <summary>
    /// Center == Position
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
    /// <summary>
    /// Radius of sphere
    /// </summary>
    public double Radius;

    public Vector3d BoundsMin
    {
        get
        {
            return Center - new Vector3d(Radius);
        }
    }
    public Vector3d BoundsMax
    {
        get
        {
            return Center + new Vector3d(Radius);
        }
    }

    public BBox Bounds
    {
        get
        {
            return new BBox(BoundsMin, new Vector3d(Radius + Radius));
        }
    }

    public Sphere(in Vector3d center, in double radius)
    {
        Center = center;
        Radius = radius;
    }

    public bool Equals(Sphere s)
    {
        return Center == s.Center && Radius == s.Radius;
    }
    public override bool Equals(object obj)
    {
        return obj is Sphere ? Equals((Sphere)obj) : false;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 29;
            hash = hash * 17 * Center.GetHashCode();
            hash = hash * 17 + Radius.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Returns true if a point is inside or on the surface of the sphere
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public bool ContainsPoint(in Vector3d p)
    {
        return (Center - p).sqrMagnitude <= Radius * Radius;
    }

    /// <summary>
    /// Intersects BBox?
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool IntersectsBBox(in BBox b)
    {
        return (b.StrictFitPoint(Center, 0) - Center).sqrMagnitude <= Radius * Radius;
    }
    /// <summary>
    /// Intersects sphere?
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public bool IntersectsSphere(in Sphere s)
    {
        return (s.Center - Center).sqrMagnitude <= (Radius + s.Radius) * (Radius + s.Radius);
    }
    /// <summary>
    /// Intersects box?
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool IntersectsBox(in Box b)
    {
        return b.IntersectsSphere(new Sphere(Center, Radius));
    }
    /// <summary>
    /// Intersects capsule?
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public bool IntersectsCapsule(in Capsule c)
    {
        return c.IntersectsSphere(new Sphere(Center, Radius));
    }
    /// <summary>
    /// Intersects polygon?
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public bool IntersectsPolygon3D(in Polygon3D p)
    {
        if (p == null)
            throw new System.Exception("Null polygon error");
        return p.IntersectsSphere(new Sphere(Center, Radius));
    }

    public override string ToString()
    {
        return "Sphere: r" + Radius.ToString() + ", center: " + Center.ToString();
    }
}
