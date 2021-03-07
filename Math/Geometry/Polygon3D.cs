using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Convex polyhedra only for now. This class will not behave correctly for
/// incorrectly formated convex polyhedra. I didn't know of the term 'polyhedra' until after the class was made :)
/// </summary>
public class Polygon3D
{
    /// <summary>
    /// Position of this polygon in 3D space. Position == Center
    /// </summary>
    public Vector3d Position
    {
        get
        {
            return position;
        }
        set
        {
            if (value != position)
            {
                if (!Modified)
                    last_position = position;
                Modified = true;
            }
            position = value;
        }
    }
    Vector3d position = Vector3d.zero;
    /// <summary>
    /// Rotation of this polygon in 3D space
    /// </summary>
    public Quaterniond Rotation
    {
        get
        {
            return rotation;
        }
        set
        {
            if (value != rotation)
            {
                if (!Modified)
                    last_rotation = rotation.eulerAngles;
                Modified = true;
            }
            rotation = value;
        }
    }
    Quaterniond rotation = Quaterniond.identity;
    /// <summary>
    /// Scale of this polygon based on original size
    /// </summary>
    public Vector3d Scale
    {
        get
        {
            return scale;
        }
        set
        {
            if (value == Vector3d.zero)
                throw new System.Exception("Scaling to zero is not allowed");
            if (value != scale)
            {
                if (!Modified)
                    last_scale = scale;
                Modified = true;
            }
            scale = value;
        }
    }
    Vector3d scale = new Vector3d(1, 1, 1);
    Vector3d[] verts = null;
    int[] tris = null;
    FixedPolygon3D parent_ref = null;

    Vector3d last_position = Vector3d.zero;
    Vector3d last_rotation = Vector3d.zero;
    Vector3d last_scale = new Vector3d(1, 1, 1);

    Vector3d position_delta
    {
        get
        {
            return Position - last_position;
        }
    }
    Vector3d rotation_delta
    {
        get
        {
            return Rotation.eulerAngles - last_rotation;
        }
    }
    Vector3d scale_delta
    {
        get
        {
            return new Vector3d(Scale.x / last_scale.x, Scale.y / last_scale.y, Scale.z / last_scale.z);
        }
    }

    bool _mod = false;
    /// <summary>
    /// If true, then the points stored in 'verts' are not updated to the most recent transform change
    /// </summary>
    bool Modified
    {
        get
        {
            return parent_ref != null ? true : _mod;
        }
        set
        {
            //if this poly gon uses a parent for vertices, then the verts array is never updated
            //and always must be transform, so we fix this value to true
            if (parent_ref != null)
                _mod = true;
            //otherwise set value
            else
            {
                _mod = value;
                //if false, then we transformed the points, so reset deltas
                if (value == false)
                {
                    last_position = Vector3d.zero;
                    last_rotation = Vector3d.zero;
                    last_scale = new Vector3d(1, 1, 1);
                }
            }
        }
    }

    /// <summary>
    /// distance of furthest point from the center
    /// </summary>
    Vector3d maxpoint
    {
        get
        {
            return transform(maxp, this);
        }
    }
    Vector3d maxp;
    /// <summary>
    /// Bounding sphere for the polygon.
    /// </summary>
    public Sphere BoundingSphere
    {
        get
        {
            return new Sphere(Position, (maxpoint - Position).magnitude);
        }
    }

    Vector3d center;
    /// <summary>
    /// Center of mass for this Polygon3D
    /// </summary>
    public Vector3d Center
    {
        get
        {
            if (parent_ref != null)
                return Position;
            else
            {
                if (Modified)
                {
                    return center + position_delta;
                }
                else
                    return center;
            }
        }
    }

    /// <summary>
    /// All vertices on this polygon3d
    /// </summary>
    public IEnumerable<Vector3d> vertices
    {
        get
        {
            Vector3d[] vs = parent_ref == null ? verts : parent_ref.verts;
            for (int i = 0; i < vs.Length; i++)
                yield return transform(vs[i], this);

        }
    }
    /// <summary>
    /// triangles for this polygon3D
    /// </summary>
    public IEnumerable<int> triangles
    {
        get
        {
            int[] vs = parent_ref == null ? tris : parent_ref.tris;
            for (int i = 0; i < vs.Length; i++)
                yield return vs[i];
        }
    }
    /// <summary>
    /// All sets of triangles for this polygon. A single Vector3dx3 represents the 3 points of a triangle
    /// </summary>
    public IEnumerable<Vector3x3> triangles_vertices
    {
        get
        {
            int[] ts = parent_ref == null ? tris : parent_ref.tris;
            Vector3d[] vs = parent_ref == null ? verts : parent_ref.verts;

            for (int i = 0; i + 3 <= ts.Length; i += 3)
            {
                Vector3x3 x3;
                x3.v1 = transform(vs[ts[i]], this);
                x3.v2 = transform(vs[ts[i + 1]], this);
                x3.v3 = transform(vs[ts[i + 2]], this);
                yield return x3;
            }
        }
    }

    /// <summary>
    /// Construct a polygon3D with vertices and valid triangles array.
    /// </summary>
    /// <param name="verts"></param>
    /// <param name="tris"></param>
    public Polygon3D(Vector3d[] _verts, int[] _tris, bool set_about_origin = true)
    {
        if (_verts == null || _tris == null)
            throw new System.Exception("Null Polygon Input");
        //copy to ensure unique references, and outside sources cannot modify arrays
        verts = Copy<Vector3d>(_verts);
        tris = Copy<int>(_tris);

        //compute bounds and average
        Vector3d min = new Vector3d(double.MaxValue, double.MaxValue, double.MaxValue);
        Vector3d max = new Vector3d(double.MinValue, double.MinValue, double.MinValue);
        double max_dist = double.MinValue;
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3d v = verts[i];
            if (v.x < min.x)
                min.x = v.x;
            if (v.y < min.y)
                min.y = v.y;
            if (v.z < min.z)
                min.z = v.z;
            if (v.x > max.x)
                max.x = v.x;
            if (v.y > max.y)
                max.y = v.y;
            if (v.z > max.z)
                max.z = v.z;
        }
        //set about origin (move all the vertices so that the center = (0,0,0))
        //not that center is approximated using doubleing point so it could be off by some
        center = CenterOfMass(this);

        if (set_about_origin)
        {
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3d v = verts[i];
                verts[i] = v - center;
                double dist = verts[i].sqrMagnitude;
                if (dist > max_dist)
                {
                    max_dist = dist;
                    maxp = verts[i];
                }
            }
            //we have to subtract the center with itself as well so that the senter represents the origin
            center = Vector3d.zero;
        }
    }

    /// <summary>
    /// Create a polygon3D with a fixed polygon. (Memory efficient, this polygon will not allocate vertice or triangle arrays)
    /// The trade off is computation speed, intersections have to do a vertice copy before being computed. This is recommended
    /// if there are many large polygon3D objects
    /// </summary>
    /// <param name="parent"></param>
    public Polygon3D(FixedPolygon3D parent)
    {
        if (parent == null || parent.verts == null || parent.tris == null)
            throw new System.Exception("Null Polygon Input");
        parent_ref = parent;
        Modified = true;
        maxp = parent_ref.MaxPoint;
    }

    /// <summary>
    /// Intersection of two 3D polygons
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public bool IntersectsPolygon3D(Polygon3D p)
    {
        if (p.verts == null || verts == null || p.verts.Length == 0 || verts.Length == 0)
            throw new System.Exception("Error, empty vertices array");

        //sphere test
        if (!BoundingSphere.IntersectsSphere(p.BoundingSphere))
            return false;

        Polygon3D p1 = this;
        Polygon3D p2 = p;

        //get points
        Vector3d[] points1 = p1.parent_ref == null ? p1.verts : p1.parent_ref.verts_copy;
        Vector3d[] points2 = p2.parent_ref == null ? p2.verts : p2.parent_ref.verts_copy;

        //transform the points
        p1.transform(points1, p1);
        p2.transform(points2, p2);

        GJK gjk = new GJK();
        return gjk.intersect(points1, points2);
    }

    /// <summary>
    /// Does polygon intersect Box?
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool IntersectsBox(Box b)
    {
        if (!b.valid)
            throw new System.Exception("Error input Box has no size.");
        if (verts == null || verts.Length == 0)
            throw new System.Exception("Error, empty vertices array");

        Polygon3D p1 = this;

        //sphere test
        if (!BoundingSphere.IntersectsSphere(b.BoundingSphere))
            return false;

        //get points
        Vector3d[] points1 = p1.parent_ref == null ? p1.verts : p1.parent_ref.verts_copy;
        Vector3d[] points2 = b.VertCopy;

        //transform the points
        p1.transform(points1, p1);

        GJK gjk = new GJK();
        return gjk.intersect(points1, points2);
    }

    /// <summary>
    /// Does the polygon intersect the input BBox?
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool IntersectsBBox(BBox b)
    {
        if (!b.valid)
            throw new System.Exception("Error input Box has no size.");
        if (verts == null || verts.Length == 0)
            throw new System.Exception("Error, empty vertices array");

        Polygon3D p1 = this;

        //sphere test
        if (!BoundingSphere.IntersectsSphere(b.BoundingSphere))
            return false;

        //get points
        Vector3d[] points1 = p1.parent_ref == null ? p1.verts : p1.parent_ref.verts_copy;
        Vector3d[] points2 = b.VertCopy;

        //transform the points
        p1.transform(points1, p1);

        GJK gjk = new GJK();
        return gjk.intersect(points1, points2);
    }

    /// <summary>
    /// Polygon intersect sphere?
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public bool IntersectsSphere(Sphere s)
    {
        if (s.Radius == 0)
            throw new System.Exception("Error input Box has no size.");
        if (verts == null || verts.Length == 0)
            throw new System.Exception("Error, empty vertices array");

        Polygon3D p1 = this;

        //sphere test
        if (!BoundingSphere.IntersectsSphere(s))
            return false;

        //get points
        Vector3d[] points1 = p1.parent_ref == null ? p1.verts : p1.parent_ref.verts_copy;

        //transform the points
        p1.transform(points1, p1);

        GJK gjk = new GJK();
        return gjk.intersect(points1, s);
    }

    /// <summary>
    /// Intersects capsule?
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public bool IntersectsCapsule(Capsule c)
    {
        if (!c.valid)
            throw new System.Exception("Invalid capsule");
        if (verts == null || verts.Length == 0)
            throw new System.Exception("Error, empty vertices array");

        Polygon3D p1 = this;

        //sphere test
        if (!BoundingSphere.IntersectsSphere(c.BoundingSphere))
            return false;

        //get points
        Vector3d[] points1 = p1.parent_ref == null ? p1.verts : p1.parent_ref.verts_copy;

        //transform the points
        p1.transform(points1, p1);

        GJK gjk = new GJK();
        return gjk.intersect(points1, c);
    }

    /// <summary>
    /// Transform the input vertice array to reflect the position of this polygon. T
    /// If 'poly.Modified' is false, then the input array will not change
    /// </summary>
    /// <param name="verts"></param>
    void transform(Vector3d[] vs, Polygon3D poly) 
    {
        //useful: http://inside.mines.edu/fs_home/gmurray/ArbitraryAxisRotation/

        //only if the polygon has been modified, otherwise the points are up-to-date
        if (poly.Modified)
        {
            //if there is no parent ref, then we must transform based on the delta values
            //otherwise we just use the actual position,rotation,scale
            Vector3d s = poly.parent_ref != null ? poly.scale : poly.scale_delta;
            Vector3d r = (poly.parent_ref != null ? poly.rotation.eulerAngles : poly.rotation_delta) * System.Math.PI / 180;
            Vector3d p = poly.parent_ref != null ? poly.position : poly.position_delta;

            double cosx = System.Math.Cos(r.x);
            double sinx = System.Math.Sin(r.x);

            double cosy = System.Math.Cos(r.y);
            double siny = System.Math.Sin(r.y);

            double cosz = System.Math.Cos(r.z);
            double sinz = System.Math.Sin(r.z);

            for (int i = 0; i < vs.Length; i++)
            {
                Vector3d v = vs[i] - center;

                //scale
                v.x *= s.x;
                v.y *= s.y;
                v.z *= s.z;
                Vector3d v2 = v;

                //rotation
                //about z-axis
                v2.x = v.x * cosz - v.y * sinz;
                v2.y = v.x * sinz + v.y * cosz;
                v = v2;

                //y-axis
                v2.x = v.x * cosy - v.z * siny;
                v2.z = v.x * siny + v.z * cosy;
                v = v2;

                //about x-axis
                v2.y = v.y * cosx - v.z * sinx;
                v2.z = v.y * sinx + v.z * cosx;

                //translate
                v2.x += p.x;
                v2.y += p.y;
                v2.z += p.z;

                vs[i] = v2 + center;
            }

            //transform center,min,max
            center = center + position_delta;

            poly.Modified = false;
        }
    }
    /// <summary>
    /// Inputs a point and outputs a transformed point based on the transform of the input polygon
    /// </summary>
    /// <param name="point"></param>
    /// <param name="poly"></param>
    /// <returns></returns>
    Vector3d transform(Vector3d point, Polygon3D poly)
    {
        if (poly.Modified)
        {
            Vector3d s = poly.parent_ref != null ? poly.scale : poly.scale_delta;
            Vector3d r = poly.parent_ref != null ? poly.rotation.eulerAngles : poly.rotation_delta;
            Vector3d p = poly.parent_ref != null ? poly.position : poly.position_delta;
            return Geometry.TransformPoint(point, center, p, r, s);
        }
        else
            return point;
    }

    /// <summary>
    /// Copy elements in arrays
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <returns></returns>
    T[] Copy<T>(T[] arr)
    {
        T[] new_arr = new T[arr.Length];
        for (int i = 0; i < arr.Length; i++)
            new_arr[i] = arr[i];
        return new_arr;
    }

    /// <summary>
    /// A polygon object that cannot be modified
    /// </summary>
    public class FixedPolygon3D
    {
        /// <summary>
        /// Returns a COPY of verts
        /// </summary>
        public Vector3d[] verts_copy
        {
            get
            {
                Vector3d[] new_arr = new Vector3d[verts.Length];
                for (int i = 0; i < verts.Length; i++)
                    new_arr[i] = verts[i];
                return new_arr;
            }
        }
        public readonly int[] tris;
        public readonly Vector3d[] verts;

        Vector3d maxp;
        public Vector3d MaxPoint
        {
            get
            {
                return maxp;
            }
        }

        /// <summary>
        /// Create a fixed polygon
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="triangles"></param>
        /// <param name="do_bbox_align">This will change the vertices so that 
        /// they are aligned with the minimum point on the bounding box. That is, Min(bbox) = (0,0,0)</param>
        public FixedPolygon3D(Vector3d[] vertices, int[] triangles)
        {
            if (vertices == null || triangles == null)
                throw new System.Exception("Null Polygon Input");
            verts = vertices; tris = triangles;

            //compute bounds and average
            Vector3d min = new Vector3d(double.MaxValue, double.MaxValue, double.MaxValue);
            Vector3d max = new Vector3d(double.MinValue, double.MinValue, double.MinValue);
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3d v = vertices[i];
                if (v.x < min.x)
                    min.x = v.x;
                if (v.y < min.y)
                    min.y = v.y;
                if (v.z < min.z)
                    min.z = v.z;
                if (v.x > max.x)
                    max.x = v.x;
                if (v.y > max.y)
                    max.y = v.y;
                if (v.z > max.z)
                    max.z = v.z;

            }
            //set about origin
            double max_dist = double.MinValue;
            Vector3d center = CenterOfMass(verts, tris);
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3d v = vertices[i];
                vertices[i] = v - center;
                double dist = verts[i].sqrMagnitude;
                if (dist > max_dist)
                {
                    max_dist = dist;
                    maxp = vertices[i];
                }
            }
            max_dist = System.Math.Sqrt(max_dist) / 2;
        }
    }

    /// <summary>
    /// compute Center of mass for polygon3D
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    static Vector3d CenterOfMass(Vector3d[] verts, int[] tris)
    {
        if (verts.Length < 4)
            throw new System.Exception("Polygon must have atleast 4 vertices");
        if (verts.Length == 4)
        {
            return TetrahedronCenterOfMass(verts[0], verts[1], verts[2], verts[3]);
        }
        else
        {
            //if this is a valid covex hull
            //select one triangle (any)
            //there will always be atleast one triangle with a different surface normal
            int t1 = 0, t2 = 0;
            Vector3d norm1 = Vector3d.Cross(verts[tris[1]] - verts[tris[0]], verts[tris[2]] - verts[tris[0]]);
            for (int i = 3; i < tris.Length; i += 3)
            {
                Vector3d a = verts[tris[i]];
                Vector3d b = verts[tris[i + 1]];
                Vector3d c = verts[tris[i + 2]];
                Vector3d norm = Vector3d.Cross(b - a, c - a);
                if (norm1 != norm)
                {
                    t2 = i;
                    break;
                }
            }
            if (t1 == t2)
                throw new System.Exception("Invalid polygon, cannot use a plane.");
            //since 'p' is convex and t1 is a different face than t2
            //then a line from center(t1) to center(t2) is guarenteed  to intersect the polygon properly (not a adge or face intersection)
            Vector3d t1sum = verts[tris[t1]] + verts[tris[t1 + 1]] + verts[tris[t1 + 2]];
            Vector3d t2sum = verts[tris[t2]] + verts[tris[t2 + 1]] + verts[tris[t2 + 2]];
            //centers of each triangle
            Vector3d t1_center = new Vector3d(t2sum.x / 3, t2sum.y / 3, t2sum.z / 3);
            Vector3d t2_center = new Vector3d(t1sum.x / 3, t1sum.y / 3, t1sum.z / 3);
            //a point in the middle of a line from the center of t1 to the center of t2, this point is inside the convex hull
            Vector3d point = t1_center + (t2_center - t1_center).magnitude / 2 * (t2_center - t1_center).normalized;

            double volume = 0;
            double[] volumes = new double[tris.Length / 3];
            int n = 0;
            //for each triangle, we make a tetrahedron where the 4th point is the point we just found
            //all triangle can for a tetrahedron with the point due to the convexity of the hull
            for (int i = 0; i < tris.Length; i += 3)
            {
                //get tetrahedron vertices
                Vector3d a = verts[tris[i]];
                Vector3d b = verts[tris[i + 1]];
                Vector3d c = verts[tris[i + 2]];
                Vector3d d = point;

                //compute volume of polygon by summing volume of tetrahedrons
                //also store the volume of this tetrahedron for next step
                double tet_volume = VolumeTetrahedron(a, b, c, d);
                volumes[n] = tet_volume;
                volume += tet_volume;
                n++;
            }

            Vector3d polygon_center_of_mass = Vector3d.zero;
            n = 0;
            for (int i = 0; i < tris.Length; i += 3)
            {
                //get tetrahedron vertices
                Vector3d a = verts[tris[i]];
                Vector3d b = verts[tris[i + 1]];
                Vector3d c = verts[tris[i + 2]];
                Vector3d d = point;

                //compute the center of mass for this tetrahedron,
                //which accounts for all possible space inside this tetrahedron
                Vector3d tet_center_of_mass = TetrahedronCenterOfMass(a, b, c, d);

                //compute how much space this tetrahedron occupies in the polygon
                //the volume ratio of all tetrahedrons sums to 1 (we are generating a probabilty density function)
                double volume_ratio = volumes[n] / volume;

                //the center of mass for the polygon is
                // Sum for All Tetrahedrons ( Tetrahedron center of mass x Space Tetrahedron occupies)
                polygon_center_of_mass += tet_center_of_mass * volume_ratio;
                n++;
            }

            return polygon_center_of_mass;
        }
    }
    /// <summary>
    /// compute Center of mass for polygon3D
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    static Vector3d CenterOfMass(Polygon3D p)
    {
        return CenterOfMass(p.verts, p.tris);
    }
    static Vector3d TetrahedronCenterOfMass(Vector3d a, Vector3d b, Vector3d c, Vector3d d)
    {
        Vector3d sum = a + b + c + d;
        return new Vector3d(sum.x / 4, sum.y / 4, sum.z / 4);
    }
    static double VolumeTetrahedron(Vector3d a, Vector3d b, Vector3d c, Vector3d d)
    {
        return System.Math.Abs(Vector3d.Dot(a - d, Vector3d.Cross(b - d, c - d))) / 6.0f;
    }
}
