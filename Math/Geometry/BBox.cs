using System;
using System.Collections.Generic;

/// <summary>
/// Axis aligned bounding box
/// </summary>
[System.Serializable]
public struct BBox : IEquatable<BBox>
{
    /// <summary>
    /// Point of lowest magnitude on bbox. Position == Min
    /// </summary>
    public Vector3d Position;
    /// <summary>
    /// Length x Width x Height in bbox
    /// </summary>
    public Vector3d Size;

    /// <summary>
    /// Position == Min
    /// </summary>
    public Vector3d Min
    {
        get
        {
            return Position;
        }
    }
    /// <summary>
    /// Position + Size
    /// </summary>
    public Vector3d Max
    {
        get
        {
            return new Vector3d(Position.x + Size.x, Position.y + Size.y, Position.z + Size.z);
        }
    }
    /// <summary>
    /// Position + Size / 2
    /// </summary>
    public Vector3d Center
    {
        get
        {
            return Min + Size / 2;
        }
        set
        {
            var delta = Center - value;
            Position -= delta;
        }
    }

    public double Volume
    {
        get
        {
            return Size.x * Size.y * Size.z;
        }
    }

    /// <summary>
    /// Sphere that encapsulates this bbox
    /// </summary>
    public Sphere BoundingSphere
    {
        get
        {
            return new Sphere(Position, (Max - Min).magnitude);
        }
    }

    public BBox(in Vector3d pos, in Vector3d size)
    {
        Position = pos; Size = size;
    }

    public static BBox FromMinMax(in Vector3d min, in Vector3d max)
    {
        return new BBox(min, max - min);
    }

    public bool Equals(BBox bbox)
    {
        return Position == bbox.Position && Size == bbox.Size;
    }
    public bool Equals(in BBox bbox, double epsilon = 0.0000001)
    {
        return Position.ApproximatelyEqual(bbox.Position) && Size.ApproximatelyEqual(bbox.Size);
    }
    public override bool Equals(object obj)
    {
        return obj is BBox ? Equals((BBox)obj) : false;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 29;
            hash = hash * 17 + Position.GetHashCode();
            hash = hash * 17 + Size.GetHashCode();
            return hash;
        }
    }
    public static bool operator ==(in BBox b1, in BBox b2)
    {
        return b1.Equals(b2);
    }
    public static bool operator !=(in BBox b1, in BBox b2)
    {
        return !b1.Equals(b2);
    }

    /// <summary>
    /// Returns true if the input bbox intersects or contains this bbox
    /// </summary>
    /// <param name="b1"></param>
    /// <returns></returns>
    public bool Intersects(in BBox b1)
    {
        var min = Min;
        var max = Max;
        var b1_min = b1.Min;
        var b1_max = b1.Max;
        if (min.x > b1_max.x) return false;
        if (max.x < b1_min.x) return false;
        if (min.y > b1_max.y) return false;
        if (max.y < b1_min.y) return false;
        if (min.z > b1_max.z) return false;
        if (max.z < b1_min.z) return false;
        return true;
    }
    /// <summary>
    /// Returns true if this bbox contains the input bbox without any intersecting points,edges, or faces
    /// </summary>
    /// <param name="b1"></param>
    /// <returns></returns>
    public bool Contains(in BBox b1)
    {
        var min = Min;
        var max = Max;
        var b1_min = b1.Min;
        var b1_max = b1.Max;
        return
            min.x < b1_min.x &&
            min.y < b1_min.y &&
            min.z < b1_min.z &&
            max.x > b1_max.x &&
            max.y > b1_max.y &&
            max.z > b1_max.z;
    }
    /// <summary>
    /// Returns true if the input point is inside the bbox
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool Contains(in Vector3d point)
    {
        return point.x >= Min.x && point.x <= Max.x &&
            point.y >= Min.y && point.y <= Max.y &&
            point.z <= Max.z && point.z >= Min.z;
    }
    /// <summary>
    /// Returns true if the input point is inside the bbox. Returns false if the input point is on the surface.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool StrictContains(in Vector3d point)
    {
        return point.x > Min.x && point.x < Max.x &&
            point.y > Min.y && point.y < Max.y &&
            point.z < Max.z && point.z > Min.z;
    }
    /// <summary>
    /// Returns an expanded bbox that fits this bbox and the input bbox
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public BBox Expand(in BBox b)
    {
        Vector3d newmin = new Vector3d(System.Math.Min(Min.x, b.Min.x),
                                     System.Math.Min(Min.y, b.Min.y),
                                     System.Math.Min(Min.z, b.Min.z));
        Vector3d newmax = new Vector3d(System.Math.Max(Max.x, b.Max.x),
                                      System.Math.Max(Max.y, b.Max.y),
                                      System.Math.Max(Max.z, b.Max.z));
        Vector3d newsize = newmax - newmin;
        return new BBox(newmin, newsize);
    }
    /// <summary>
    /// Distance between point and bbox
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public double Distance(in Vector3d point)
    {
        if (Contains(point)) return 0;
        Vector3d dist_point = point;
        if (point.x < Min.x)
            dist_point.x = Min.x;
        else if (point.x > Max.x)
            dist_point.x = Max.x;
        if (point.y < Min.y)
            dist_point.y = Min.y;
        else if (point.y > Max.y)
            dist_point.y = Max.y;
        if (point.z < Min.z)
            dist_point.z = Min.z;
        else if (point.z > Max.z)
            dist_point.z = Max.z;
        return Vector3d.Distance(dist_point, point);
    }
    /// <summary>
    /// Distance between two bbox
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public double Distance(in BBox b)
    {
        Vector3d dist_point = Vector3d.zero;
        Vector3d dist_point2 = Vector3d.zero;
        if (b.Max.x < Min.x)
        {
            dist_point.x = Min.x;
            dist_point2.x = b.Max.x;
        }
        else if (b.Min.x > Max.x)
        {
            dist_point.x = Max.x;
            dist_point2.x = b.Min.x;
        }
        if (b.Max.y < Min.y)
        {
            dist_point.y = Min.y;
            dist_point2.y = b.Max.y;
        }
        else if (b.Min.y > Max.y)
        {
            dist_point.y = Max.y;
            dist_point2.y = b.Min.y;
        }
        if (b.Max.z < Min.z)
        {
            dist_point.z = Min.z;
            dist_point2.z = b.Max.z;
        }
        else if (b.Min.z > Max.z)
        {
            dist_point.z = Max.z;
            dist_point2.z = b.Min.z;
        }
        return Vector3d.Distance(dist_point, dist_point2);
    }
    /// <summary>
    /// Distance squared between point and bbox
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public double DistanceSq(in Vector3d point)
    {
        if (Contains(point)) return 0;
        Vector3d dist_point = point;
        if (point.x <= Min.x)
            dist_point.x = Min.x;
        else if (point.x >= Max.x)
            dist_point.x = Max.x;
        if (point.y <= Min.y)
            dist_point.y = Min.y;
        else if (point.y >= Max.y)
            dist_point.y = Max.y;
        if (point.z <= Min.z)
            dist_point.z = Min.z;
        else if (point.z >= Max.z)
            dist_point.z = Max.z;
        return (dist_point - point).sqrMagnitude;
    }
    /// <summary>
    /// Distance between point and bbox, input point is assumed to be outside or on the surface of the bbox but not inside
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public double DistanceSq_OUTSIDE(in Vector3d point)
    {
        Vector3d min = Min, max = Max;
        var dist_point = point;
        if (point.x <= min.x)
            dist_point.x = min.x;
        else if (point.x >= max.x)
            dist_point.x = max.x;
        if (point.y <= min.y)
            dist_point.y = min.y;
        else if (point.y >= max.y)
            dist_point.y = max.y;
        if (point.z <= min.z)
            dist_point.z = min.z;
        else if (point.z >= max.z)
            dist_point.z = max.z;
        return (dist_point - point).sqrMagnitude;
    }
    /// <summary>
    /// Distance squared between two bbox
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public double DistanceSq(in BBox b)
    {
        Vector3d dist_point = Vector3d.zero;
        Vector3d dist_point2 = Vector3d.zero;
        if (b.Max.x < Min.x)
        {
            dist_point.x = Min.x;
            dist_point2.x = b.Max.x;
        }
        else if (b.Min.x > Max.x)
        {
            dist_point.x = Max.x;
            dist_point2.x = b.Min.x;
        }
        if (b.Max.y < Min.y)
        {
            dist_point.y = Min.y;
            dist_point2.y = b.Max.y;
        }
        else if (b.Min.y > Max.y)
        {
            dist_point.y = Max.y;
            dist_point2.y = b.Min.y;
        }
        if (b.Max.z < Min.z)
        {
            dist_point.z = Min.z;
            dist_point2.z = b.Max.z;
        }
        else if (b.Min.z > Max.z)
        {
            dist_point.z = Max.z;
            dist_point2.z = b.Min.z;
        }
        return (dist_point2 - dist_point).sqrMagnitude;
    }
    /// <summary>
    /// Returns true if this bbox contains the input bbox without any adge,corner,or surface intersections
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool StrictContains(in BBox b1)
    {
        return b1.Min.x > Min.x && b1.Max.x < Max.x &&
               b1.Min.y > Min.y && b1.Max.y < Max.y &&
               b1.Min.z > Min.z && b1.Max.z < Max.z;
    }
    /// <summary>
    /// Intersects sphere?
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public bool IntersectsSphere(in Sphere s)
    {
        return s.IntersectsBBox(new BBox(Position, Size));
    }
    /// <summary>
    /// Intersects capsule?
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public bool IntersectsCapsule(in Capsule c)
    {
        return c.IntersectsBBox(new BBox(Position, Size));
    }
    /// <summary>
    /// Intersects box?
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool IntersectsBox(in Box b)
    {
        return b.IntersectsBBox(new BBox(Position, Size));
    }
    /// <summary>
    /// Intersects Polygon3D
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public bool IntersectsPolygon3D(in Polygon3D p)
    {
        if (p == null)
            throw new System.Exception("null polygon exception");
        return p.IntersectsBBox(new BBox(Position, Size));
    }

    /// <summary>
    /// Returns the normal of a bbox face that is closest to the input point.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public Vector3Int ClosestNormalToPoint(in Vector3d point)
    {
        var min = Min;
        var max = Max;
        var d_x_pos = Math.Abs(max.x - point.x);
        var d_x_neg = Math.Abs(min.x - point.x);
        var d_y_pos = Math.Abs(point.y - max.y);
        var d_y_neg = Math.Abs(point.y - min.y);
        var d_z_pos = Math.Abs(max.z - point.z);
        var d_z_neg = Math.Abs(min.z - point.z);
        Vector3d chosen = Vector3d.zero;
        Vector3Int dir = Vector3Int.zero;
        chosen.x = d_x_pos < d_x_neg ? d_x_pos : d_x_neg;
        chosen.y = d_y_pos < d_y_neg ? d_y_pos : d_y_neg;
        chosen.z = d_z_pos < d_z_neg ? d_z_pos : d_z_neg;
        dir.x = d_x_pos < d_x_neg ? 1 : -1;
        dir.y = d_y_pos < d_y_neg ? 1 : -1;
        dir.z = d_z_pos < d_z_neg ? 1 : -1;
        if (chosen.x < chosen.y)
        {
            if (chosen.x < chosen.z)
                { return dir.x < 0 ? Vector3Int.left : Vector3Int.right;}
            else
                { return dir.z < 0 ? Vector3Int.back : Vector3Int.forward;}
        }
        else
        {
            if (chosen.y < chosen.z)
                { return dir.y < 0 ? Vector3Int.down : Vector3Int.up;}
            else
                { return dir.z < 0 ? Vector3Int.back : Vector3Int.forward;}
        }
    }

    /// <summary>
    /// Returns a point on the bbox that is closest to the input point.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public Vector3d ClosestPointToPoint(Vector3d v)
    {
        if (Contains(v))
        {
            var n = ClosestNormalToPoint(v);
            if (n.x != 0)
                return n.x < 0 ? new Vector3d(Min.x, v.y, v.z) : new Vector3d(Max.x, v.y, v.z);
            else if (n.y != 0)
                return n.y < 0 ? new Vector3d(v.x, Min.y, v.z) : new Vector3d(v.x, Max.y, v.z);
            else
                return n.z < 0 ? new Vector3d(v.x, v.y, Min.z) : new Vector3d(v.x, v.y, Max.z);
        }
        else
        {
            if (v.x >= Max.x)
                v.x = Max.x;
            else if (v.x <= Min.x)
                v.x = Min.x;
            if (v.y >= Max.y)
                v.y = Max.y;
            else if (v.y <= Min.y)
                v.y = Min.y;
            if (v.z >= Max.z)
                v.z = Max.z;
            else if (v.z <= Min.z)
                v.z = Min.z;
            return v;
        }
    }

    /// <summary>
    /// Fits the input vector to be inside this bbox within an distance 'e'. e=0 will put the point on the surface.
    /// The input point is not changed if it is already in the box
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public Vector3d StrictFitPoint(Vector3d v, double e = 0.0000001)
    {
        if (v.x >= Max.x)
            v.x = Max.x - e;
        else if (v.x <= Min.x)
            v.x = Min.x + e;
        if (v.y >= Max.y)
            v.y = Max.y - e;
        else if (v.y <= Min.y)
            v.y = Min.y + e;
        if (v.z >= Max.z)
            v.z = Max.z - e;
        else if (v.z <= Min.z)
            v.z = Min.z + e;
        return v;
    }

    /// <summary>
    /// Modifies the input bbox to fit inside this bboxint32
    /// </summary>
    /// <param name="bbox"></param>
    public BBox StrictFitBBox(in BBox bbox, double e = 0.0000001)
    {
        var max = this.Max;
        var min = this.Min;

        var bmin = bbox.Min;
        var bmax = bbox.Max;

        if (bmax.x >= max.x)
        {
            bmax.x = max.x - e;
            if (bmin.x > bmax.x) bmin.x = bmax.x - e;
        }
        else if (bmin.x <= min.x)
        {
            bmin.x = min.x + e;
            if (bmax.x < bmin.x) bmax.x = bmin.x + e;
        }
        if (bmax.y >= max.y)
        {
            bmax.y = max.y - e;
            if (bmin.y > bmax.y) bmin.y = bmax.y - e;
        }
        else if (bmin.y <= min.y)
        {
            bmin.y = min.y + e;
            if (bmax.y < bmin.y) bmax.y = bmin.y + e;
        }
        if (bmax.z >= max.z)
        {
            bmax.z = max.z - e;
            if (bmin.z > bmax.z) bmin.z = bmax.z - e;
        }
        else if (bmin.z <= min.z)
        {
            bmin.z = min.z + e;
            if (bmax.z < bmin.z) bmax.z = bmin.z + e;
        }

        return BBox.FromMinMax(bmin, bmax);
    }

    /// <summary>
    /// Assuming the input bbox is colliding, this function will return a translation vector that pushes the colliding bbox out of this bbox along 'dir' direction
    /// </summary>
    /// <param name="dir">normalized direction vector to translate upon</param>
    /// <param name="bbox">colliding bbox that will be translated</param>
    /// <returns> returns the new center of the input bbox </returns>
    public Vector3d ResolveBBoxCollision(in Vector3d dir, in BBox bbox)
    {
        // Do line plane intersections for each dir component. The planes will be defined as minimum translation of the bbox for each axis (in the direction vec)
        // The translation that requires the shortest ditance will be chosen
        var line_vec = dir * (bbox.Size.magnitude + Size.magnitude + 10);
        var bbox_center = bbox.Position + bbox.Size * 0.5;
        var line_start = bbox_center - line_vec;
        var this_min = this.Min;
        var this_max = this.Max;
        var bbox_min = bbox.Min;
        var bbox_max = bbox.Max;

        Vector3d p1, p2, p3;
        double p1_dist_sq = double.MaxValue, p2_dist_sq = double.MaxValue, p3_dist_sq = double.MaxValue;
        Vector3d plane_normal = new Vector3d(dir.x < 0 ? 1 : -1, 0, 0);
        Vector3d plane_point = new Vector3d(dir.x < 0 ? bbox_center.x + (this_min.x - bbox_max.x) : bbox_center.x + (this_max.x - bbox_min.x), 0, 0);
        if (Geometry.LinePlaneIntersection(out p1, line_start, line_vec, plane_normal, plane_point))
            p1_dist_sq = (line_start - p1).sqrMagnitude;

        plane_normal = new Vector3d(0, dir.y < 0 ? 1 : -1, 0);
        plane_point = new Vector3d(0, dir.y < 0 ? bbox_center.y + (this_min.y - bbox_max.y) : bbox_center.y + (this_max.y - bbox_min.y), 0);
        if (Geometry.LinePlaneIntersection(out p2, line_start, line_vec, plane_normal, plane_point))
            p2_dist_sq = (line_start - p2).sqrMagnitude;

        plane_normal = new Vector3d(0, 0, dir.z < 0 ? 1 : -1);
        plane_point = new Vector3d(0, 0, dir.z < 0 ? bbox_center.z + (this_min.z - bbox_max.z) : bbox_center.z + (this_max.z - bbox_min.z));
        if (Geometry.LinePlaneIntersection(out p3, line_start, line_vec, plane_normal, plane_point))
            p3_dist_sq = (line_start - p3).sqrMagnitude;

        if (p1_dist_sq <= p2_dist_sq && p1_dist_sq <= p3_dist_sq)
            return p1;
        if (p2_dist_sq <= p1_dist_sq && p2_dist_sq <= p3_dist_sq)
            return p2;
        return p3;
    }

    public Vector3d ExcludePoint(in Vector3d p)
    {
        if (!Contains(p))
            return p;
        var min_dist_sq = double.MaxValue;
        var dist_sq = double.MaxValue;
        var min = Min;
        var max = Max;
        var test = p;
        var resolution = p;

        test.x = min.x;
        dist_sq = (test - p).sqrMagnitude;
        if (dist_sq < min_dist_sq) { resolution = test; min_dist_sq = dist_sq; }
        test.x = max.x;
        dist_sq = (test - p).sqrMagnitude;
        if (dist_sq < min_dist_sq) { resolution = test; min_dist_sq = dist_sq; }
        test.x = p.x;

        test.y = min.y;
        dist_sq = (test - p).sqrMagnitude;
        if (dist_sq < min_dist_sq) { resolution = test; min_dist_sq = dist_sq; }
        test.y = max.y;
        dist_sq = (test - p).sqrMagnitude;
        if (dist_sq < min_dist_sq) { resolution = test; min_dist_sq = dist_sq; }
        test.y = p.y;

        test.z = min.z;
        dist_sq = (test - p).sqrMagnitude;
        if (dist_sq < min_dist_sq) { resolution = test; min_dist_sq = dist_sq; }
        test.z = max.z;
        dist_sq = (test - p).sqrMagnitude;
        if (dist_sq < min_dist_sq) { resolution = test; min_dist_sq = dist_sq; }

        return resolution;
    }

    public override string ToString()
    {
        return "Pos: " + Position.ToString() + " Size: " + Size.ToString();
    }

    /// <summary>
    /// Return VEctor3[] array containing the points of this bbox
    /// </summary>
    public Vector3d[] VertCopy
    {
        get
        {
            Vector3d[] v = new Vector3d[6];
            v[0] = Min;
            v[1] = new Vector3d(Min.x, Min.y, Min.z + Size.z);
            v[2] = new Vector3d(Min.x, Min.y + Size.y, Min.z);
            v[3] = new Vector3d(Min.x, Min.y + Size.y, Min.z + Size.z);
            v[4] = new Vector3d(Min.x + Size.x, Min.y, Min.z);
            v[5] = new Vector3d(Min.x + Size.x, Min.y, Min.z + Size.z);
            v[6] = new Vector3d(Min.x + Size.x, Min.y + Size.y, Min.z);
            v[7] = Max;
            return v;
        }
    }

    /// <summary>
    /// All vertices in the bbox
    /// </summary>
    public IEnumerable<Vector3d> Verts
    {
        get
        {
            Vector3d v = new Vector3d();
            v = Min;
            yield return v;
            v = new Vector3d(Min.x, Min.y, Min.z + Size.z);
            yield return v;
            v = new Vector3d(Min.x, Min.y + Size.y, Min.z);
            yield return v;
            v = new Vector3d(Min.x, Min.y + Size.y, Min.z + Size.z);
            yield return v;
            v = new Vector3d(Min.x + Size.x, Min.y, Min.z);
            yield return v;
            v = new Vector3d(Min.x + Size.x, Min.y, Min.z + Size.z);
            yield return v;
            v = new Vector3d(Min.x + Size.x, Min.y + Size.y, Min.z);
            yield return v;
            v = Max;
            yield return v;
        }
    }

    public bool valid
    {
        get
        {
            return Size.x > 0 && Size.y > 0 && Size.z > 0;
        }
    }

    public Vector3d p0
    {
        get
        {
            return Min;
        }
    }
    public Vector3d p1
    {
        get
        {
            return new Vector3d(Min.x, Min.y, Min.x + Size.z);
        }
    }
    public Vector3d p2
    {
        get
        {
            return new Vector3d(Min.x, Min.y + Size.y, Min.x);
        }
    }
    public Vector3d p3
    {
        get
        {
            return new Vector3d(Min.x, Min.y + Size.y, Min.x + Size.z);
        }
    }
    public Vector3d p4
    {
        get
        {
            return new Vector3d(Min.x + Size.x, Min.y, Min.x);
        }
    }
    public Vector3d p5
    {
        get
        {
            return new Vector3d(Min.x + Size.x, Min.y, Min.x + Size.z);
        }
    }
    public Vector3d p6
    {
        get
        {
            return new Vector3d(Min.x + Size.x, Min.y + Size.y, Min.x);
        }
    }
    public Vector3d p7
    {
        get
        {
            return Max;
        }
    }

    public static implicit operator BBox(in BBoxInt32 b)
    {
        return new BBox(b.Position, b.Size);
    }
}

/// <summary>
/// Bbox with integer numbers
/// </summary>
public struct BBoxInt32
{
    public Vector3Int Position;
    public Vector3Int Size;
    public Vector3Int Min
    {
        get
        {
            return Position;
        }
    }
    public Vector3Int Max
    {
        get
        {
            return Position + Size;
        }
    }

    public BBoxInt32(in Vector3Int position, in Vector3Int size)
    {
        Position = position; Size = size;
    }

    public BBoxInt32(in int x, in int y, in int z, in int s)
    {
        Position = new Vector3Int(x, y, z);
        Size = new Vector3Int(s);
    }

    /// <summary>
    /// create from bounds
    /// </summary>
    /// <param name="min">inclusive</param>
    /// <param name="max">exclusive</param>
    /// <returns></returns>
    public static BBoxInt32 FromBounds(in Vector3Int min, in Vector3Int max)
    {
        return new BBoxInt32(min, max - min);
    }

    /// <summary>
    /// Returns a bbox that represents the space of intersection between this bbox and an input bbox
    /// </summary>
    /// <param name="bbox"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool IntersectionOf(in BBoxInt32 bbox, out BBoxInt32 result)
    {
        if (Intersects(bbox))
        {
            var max = Max;
            var bmax = bbox.Max;
            var new_min = new Vector3Int(Math.Max(Min.x, bbox.Min.x), Math.Max(Min.y, bbox.Min.y),
                Math.Max(Min.z, bbox.Min.z));
            var new_max = new Vector3Int(Math.Min(max.x, bmax.x), Math.Min(max.y, bmax.y),
                Math.Min(max.z, bmax.z));
            result = new BBoxInt32(new_min, new_max - new_min);
            return true;
        }
        result = default(BBoxInt32);
        return false;
    }

    /// <summary>
    /// Intersects a bbox?
    /// </summary>
    /// <param name="b1"></param>
    /// <returns></returns>
    public bool Intersects(in BBoxInt32 b1)
    {
        var min = Min;
        var max = Max;
        var b1_min = b1.Min;
        var b1_max = b1.Max;
        if (min.x > b1_max.x) return false;
        if (max.x < b1_min.x) return false;
        if (min.y > b1_max.y) return false;
        if (max.y < b1_min.y) return false;
        if (min.z > b1_max.z) return false;
        if (max.z < b1_min.z) return false;
        return true;
    }

    public bool Intersects(in BBox b1)
    {
        var min = Min;
        var max = Max;
        var b1_min = b1.Min;
        var b1_max = b1.Max;
        if (min.x > b1_max.x) return false;
        if (max.x < b1_min.x) return false;
        if (min.y > b1_max.y) return false;
        if (max.y < b1_min.y) return false;
        if (min.z > b1_max.z) return false;
        if (max.z < b1_min.z) return false;
        return true;
    }

    public bool Contains(in Vector3d point)
    {
        return point.x >= Min.x && point.x <= Max.x &&
               point.y >= Min.y && point.y <= Max.y &&
               point.z <= Max.z && point.z >= Min.z;
    }

    public bool Contains(in Vector3Int point)
    {
        return point.x >= Min.x && point.x <= Max.x &&
               point.y >= Min.y && point.y <= Max.y &&
               point.z <= Max.z && point.z >= Min.z;
    }

    /// <summary>
    /// Returns true if this bbox contains the input bbox without any intersecting points,edges, or faces
    /// </summary>
    /// <param name="b1"></param>
    /// <returns></returns>
    public bool Contains(in BBox b1)
    {
        var min = Min;
        var max = Max;
        var b1_min = b1.Min;
        var b1_max = b1.Max;
        return
            min.x < b1_min.x &&
            min.y < b1_min.y &&
            min.z < b1_min.z &&
            max.x > b1_max.x &&
            max.y > b1_max.y &&
            max.z > b1_max.z;
    }

    /// <summary>
    /// Does a point containment test similar to array indexing (Min inclusive, Max exclusive)
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool IndexContains(in Vector3Int point)
    {
        return point.x >= Min.x && point.x < Max.x &&
               point.y >= Min.y && point.y < Max.y &&
               point.z >= Min.z && point.z < Max.z;
    }

    public static explicit operator BBoxInt32(in BBox b)
    {
        return new BBoxInt32((Vector3Int)b.Position, (Vector3Int)b.Size);
    }

    /// <summary>
    /// Fits the input vector to be inside this bbox within an distance 'e'. e=0 will put the point on the surface.
    /// The input point is not changed if it is already in the box
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public Vector3d StrictFitPoint(Vector3d v, in double e = 0.0000001)
    {
        var max = this.Max;
        var min = this.Min;

        if (v.x >= max.x)
            v.x = max.x - e;
        else if (v.x <= min.x)
            v.x = min.x + e;
        if (v.y >= max.y)
            v.y = max.y - e;
        else if (v.y <= min.y)
            v.y = min.y + e;
        if (v.z >= max.z)
            v.z = max.z - e;
        else if (v.z <= min.z)
            v.z = min.z + e;
        return v;
    }

    /// <summary>
    /// Modifies the input bbox to fit inside this bboxint32
    /// </summary>
    /// <param name="bbox"></param>
    public BBox StrictFitBBox(in BBox bbox, in double e = 0.0000001)
    {
        var max = this.Max;
        var min = this.Min;

        var bmin = bbox.Min;
        var bmax = bbox.Max;

        if (bmax.x >= max.x)
        {
            bmax.x = max.x - e;
            if (bmin.x > bmax.x) bmin.x = bmax.x - e;
        }
        else if (bmin.x <= min.x)
        {
            bmin.x = min.x + e;
            if (bmax.x < bmin.x) bmax.x = bmin.x + e;
        }
        if (bmax.y >= max.y)
        {
            bmax.y = max.y - e;
            if (bmin.y > bmax.y) bmin.y = bmax.y - e;
        }
        else if (bmin.y <= min.y)
        {
            bmin.y = min.y + e;
            if (bmax.y < bmin.y) bmax.y = bmin.y + e;
        }
        if (bmax.z >= max.z)
        {
            bmax.z = max.z - e;
            if (bmin.z > bmax.z) bmin.z = bmax.z - e;
        }
        else if (bmin.z <= min.z)
        {
            bmin.z = min.z + e;
            if (bmax.z < bmin.z) bmax.z = bmin.z + e;
        }

        return BBox.FromMinMax(bmin, bmax);
    }

    /// <summary>
    /// Returns true if the input point is inside the bbox. Returns false if the input point is on the surface.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool StrictContains(in Vector3d point)
    {
        return point.x > Min.x && point.x < Max.x &&
            point.y > Min.y && point.y < Max.y &&
            point.z < Max.z && point.z > Min.z;
    }

    public bool Equals(BBoxInt32 bbox)
    {
        return Position == bbox.Position && Size == bbox.Size;
    }
    public override bool Equals(object obj)
    {
        return obj is BBoxInt32 ? Equals((BBoxInt32)obj) : false;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 29;
            hash = hash * 17 + Position.GetHashCode();
            hash = hash * 17 + Size.GetHashCode();
            return hash;
        }
    }
    public static bool operator ==(in BBoxInt32 b1, in BBoxInt32 b2)
    {
        return b1.Equals(b2);
    }
    public static bool operator !=(in BBoxInt32 b1, in BBoxInt32 b2)
    {
        return !b1.Equals(b2);
    }
    public override string ToString()
    {
        return "Pos: " + Position.ToString() + " Size: " + Size.ToString();
    }
}