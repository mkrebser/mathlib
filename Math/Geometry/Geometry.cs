using System;
using System.Collections.Generic;

/// <summary>
/// Geometry class. Various geometric functions. Most functions in here do not handle error-prone input such
/// as NaN or Infinity or overflow input to functions. nor do they check for such input.
/// </summary>
public static class Geometry
{
    static double deg2rad = System.Math.PI / 180;

    /// <summary>
    /// Determine if point is left of line 'ab'
    /// </summary>
    /// <param name="point"></param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool Left(in Vector2 point, in Vector2 a, in Vector2 b)
    {
        return ((b.x - a.x) * (point.y - a.y) - (b.y - a.y) * (point.x - a.x)) > 0;
    }
    public static bool Left(in Vector2d point, in Vector2d a, in Vector2d b)
    {
        return ((b.x - a.x) * (point.y - a.y) - (b.y - a.y) * (point.x - a.x)) > 0;
    }
    /// <summary>
    /// Determine if point is left of line 'ab'
    /// </summary>
    /// <param name="point"></param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool LeftOrCollinear(in Vector2 point, in Vector2 a, in Vector2 b)
    {
        return ((b.x - a.x) * (point.y - a.y) - (b.y - a.y) * (point.x - a.x)) >= 0;
    }

    /// <summary>
    /// Finds the intersection between 2 planes. Returns false if they are parallel.
    /// </summary>
    /// <param name="linePoint"></param>
    /// <param name="lineVec"></param>
    /// <param name="plane1"></param>
    /// <param name="plane2"></param>
    public static bool PlanePlaneIntersection(out Vector3d linePoint, out Vector3d lineDir,
        in Plane_ex plane1, in Plane_ex plane2)
    {
        linePoint = new Vector3d(0, 0, 0);

        //Get the normals of the planes.
        Vector3d plane1Normal = plane1.normal;
        Vector3d plane2Normal = plane2.normal;

        lineDir = Vector3d.Cross(plane1Normal, plane2Normal);
        //Determine if the cross product yielded (0,0,0) - parallel
        if (lineDir.x < 0.00001f && lineDir.x > -0.00001f &&
            lineDir.y < 0.00001f && lineDir.y > -0.00001f &&
            lineDir.z < 0.00001f && lineDir.z > -0.00001f)
        {
            linePoint = new Vector3d(0, 0, 0);
            return false;
        }

        //absolute values of the normal
        double ax = (lineDir.x >= 0 ? lineDir.x : -lineDir.x);
        double ay = (lineDir.y >= 0 ? lineDir.y : -lineDir.y);
        double az = (lineDir.z >= 0 ? lineDir.z : -lineDir.z);

        //Determine which coordinate is the largest from 0
        //x is biggest => maxc = 1, y is biggest => maxc = 2, z => maxc = 3
        //we use this to solve the plane equations
        int maxc;
        if (ax > ay)
        {
            if (ax > az)
                maxc = 1;
            else maxc = 3;
        }
        else
        {
            if (ay > az)
                maxc = 2;
            else maxc = 3;
        }

        double d1, d2;
        d1 = -Vector3d.Dot(plane1.normal, plane1.v1);
        d2 = -Vector3d.Dot(plane2.normal, plane2.v1);

        switch (maxc)
        {
            case 1:
                linePoint.x = 0;
                linePoint.y = (d2 * plane1.normal.z - d1 * plane2.normal.z) / lineDir.x;
                linePoint.z = (d1 * plane2.normal.y - d2 * plane1.normal.y) / lineDir.x;
                break;
            case 2:
                linePoint.x = (d1 * plane2.normal.z - d2 * plane1.normal.z) / lineDir.y;
                linePoint.y = 0;
                linePoint.z = (d2 * plane1.normal.x - d1 * plane2.normal.x) / lineDir.y;
                break;
            case 3:
                linePoint.x = (d2 * plane1.normal.y - d1 * plane2.normal.y) / lineDir.z;
                linePoint.y = (d1 * plane2.normal.x - d2 * plane1.normal.x) / lineDir.z;
                linePoint.z = 0;
                break;
        }
        return true;
    }

    /// <summary>
    /// Calculate line intersection. Returns false if skew. Ignores Coinciding lines
    /// </summary>
    /// <param name="intersection"></param>
    /// <param name="linePoint1"></param>
    /// <param name="lineVec1"></param>
    /// <param name="linePoint2"></param>
    /// <param name="lineVec2"></param>
    /// <returns></returns>
    public static bool LineLineIntersection(out Vector3d intersection, in Vector3d linePoint1,
        in Vector3d lineDir1, in Vector3d linePoint2, in Vector3d lineDir2)
    {
        intersection = new Vector3d(0, 0, 0);

        Vector3d lineDir3 = linePoint2 - linePoint1;
        Vector3d crossVec1and2 = Vector3d.Cross(lineDir1, lineDir2);
        Vector3d crossVec3and2 = Vector3d.Cross(lineDir3, lineDir2);

        double planarFactor = Vector3d.Dot(lineDir3, crossVec1and2);
        //Lines are not coplanar. Take into account rounding errors.
        if ((planarFactor >= 0.00001f) || (planarFactor <= -0.00001f))
        {
            return false;
        }

        //Note: sqrMagnitude does x*x+y*y+z*z on the input vector.
        double s = Vector3d.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;

        if ((s >= 0.0f) && (s <= 1.0f))
        {
            intersection = linePoint1 + (lineDir1 * s);
            return true;
        }
        else
        {
            //we aren't using coincidence :P
            /* //check for coincidence
             if (linePoint1 == linePoint2)
                 return true;
             Vector3d norm21 = (linePoint2 - linePoint1).normalized;
             Vector3d norm12 = (linePoint1 - linePoint2).normalized;
             Vector3d dir_norm = lineDir1.normalized;

             //check if dir_norm == norm21 or == dir_norm == norm12
             if ((norm21.x < dir_norm.x + 0.00001f && norm21.y < dir_norm.y + 0.00001f && norm21.z < dir_norm.z + 0.00001f
                 && norm21.x > dir_norm.x - 0.00001f && norm21.y > dir_norm.y - 0.00001f && norm21.z > dir_norm.z - 0.00001f) ||
                 (norm12.x < dir_norm.x + 0.00001f && norm12.y < dir_norm.y + 0.00001f && norm12.z < dir_norm.z + 0.00001f
                 && norm12.x > dir_norm.x - 0.00001f && norm12.y > dir_norm.y - 0.00001f && norm12.z > dir_norm.z - 0.00001f))
                 return true;
             */

            return false;
        }
    }

    /// <summary>
    /// Intersection of a line and plane. Returns false if parallel, otherwise true.
    /// </summary>
    /// <param name="intersection"> output intersection </param>
    /// <param name="linePoint"> line starting point </param>
    /// <param name="lineVec"> line vector (ie, linePoint + lineVec will make the ray endpoint). Direction matters. </param>
    /// <param name="planeNormal"> normal of the plane. </param>
    /// <param name="planePoint"> a point on the plane. </param>
    /// <returns></returns>
    public static bool LinePlaneIntersection(out Vector3d intersection, in Vector3d linePoint,
        in Vector3d lineVec, in Vector3d planeNormal, in Vector3d planePoint)
    {
        Vector3d l1 = linePoint, l2 = linePoint + lineVec;
        double d1 = Vector3d.Dot(planeNormal, planePoint - l1);
        double d2 = Vector3d.Dot(planeNormal, l2 - l1);
        if (d2 == 0) { intersection = Vector3d.zero; return false; }
        intersection = l1 + d1 / d2 * (l2 - l1); return true;
    }

    /// <summary>
    /// Returns true if c lies on ab
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <returns></returns>
    public static bool PointOnSegment(in Vector3d a, in Vector3d b, in Vector3d c)
    {
        double d1 = (c - a).sqrMagnitude;
        double d2 = (c - b).sqrMagnitude;
        double d3 = (b - a).sqrMagnitude;
        return ApproxEqual(d1 + d2, d3, 0.00001d);
        
    }
    static bool ApproxEqual(in double a, in double b, in double epsilon)
    {
        return (a - b) - epsilon <= 0 && (b - a) + epsilon >= 0;
    }

    /// <summary>
    /// Distance between two line segments
    /// </summary>
    /// <param name="s1p0"></param>
    /// <param name="s1p1"></param>
    /// <param name="s2p0"></param>
    /// <param name="s2p1"></param>
    /// <returns></returns>
    public static double LineSegLineSegDistance(in Vector3d s1p0, in Vector3d s1p1, in Vector3d s2p0, in Vector3d s2p1)
    {
        // Copyright 2001 softSurfer, 2012 Dan Sunday
        // This code may be freely used, distributed and modified for any purpose
        // providing that this copyright notice is included with it.
        // SoftSurfer makes no warranty for this code, and cannot be held
        // liable for any real or imagined damage resulting from its use.
        // Users of this code must verify correctness for their application.

        double epsilon = 0.000001d;
        Vector3d u = s1p1 - s1p0;
        Vector3d v = s2p1 - s2p0;
        Vector3d w = s1p0 - s2p0;
        double a = Vector3d.Dot(u, u); 
        double b = Vector3d.Dot(u, v);
        double c = Vector3d.Dot(v, v); 
        double d = Vector3d.Dot(u, w);
        double e = Vector3d.Dot(v, w);
        double D = a * c - b * b;  
        double sc, sN, sD = D; 
        double tc, tN, tD = D;  
        if (D < epsilon)
        { 
            sN = 0.0d;
            sD = 1.0d; 
            tN = e;
            tD = c;
        }
        else
        {   
            sN = (b * e - c * d);
            tN = (a * e - b * d);
            if (sN < 0.0d)
            {
                sN = 0.0d;
                tN = e;
                tD = c;
            }
            else if (sN > sD)
            { 
                sN = sD;
                tN = e + b;
                tD = c;
            }
        }
        if (tN < 0.0d)
        {  
            tN = 0.0d;
            if (-d < 0.0d)
                sN = 0.0d;
            else if (-d > a)
                sN = sD;
            else
            {
                sN = -d;
                sD = a;
            }
        }
        else if (tN > tD)
        {  
            tN = tD;
            if ((-d + b) < 0.0d)
                sN = 0d;
            else if ((-d + b) > a)
                sN = sD;
            else
            {
                sN = (-d + b);
                sD = a;
            }
        }
        sc = (System.Math.Abs(sN) < epsilon ? 0.0 : sN / sD);
        tc = (System.Math.Abs(tN) < epsilon ? 0.0 : tN / tD);
        Vector3d dP = w + (sc * u) - (tc * v);
        return dP.magnitude; 
    }
    /// <summary>
    /// Distance between two line segments
    /// </summary>
    /// <param name="s1p0"></param>
    /// <param name="s1p1"></param>
    /// <param name="s2p0"></param>
    /// <param name="s2p1"></param>
    /// <returns></returns>
    public static double LineSegLineSegDistanceSqrd(in Vector3d s1p0, in Vector3d s1p1, in Vector3d s2p0, in Vector3d s2p1)
    {
        // Copyright 2001 softSurfer, 2012 Dan Sunday
        // This code may be freely used, distributed and modified for any purpose
        // providing that this copyright notice is included with it.
        // SoftSurfer makes no warranty for this code, and cannot be held
        // liable for any real or imagined damage resulting from its use.
        // Users of this code must verify correctness for their application.

        double epsilon = 0.000001d;
        Vector3d u = s1p1 - s1p0;
        Vector3d v = s2p1 - s2p0;
        Vector3d w = s1p0 - s2p0;
        double a = Vector3d.Dot(u, u);
        double b = Vector3d.Dot(u, v);
        double c = Vector3d.Dot(v, v);
        double d = Vector3d.Dot(u, w);
        double e = Vector3d.Dot(v, w);
        double D = a * c - b * b;
        double sc, sN, sD = D;
        double tc, tN, tD = D;
        if (D < epsilon)
        {
            sN = 0.0d;
            sD = 1.0d;
            tN = e;
            tD = c;
        }
        else
        {
            sN = (b * e - c * d);
            tN = (a * e - b * d);
            if (sN < 0.0d)
            {
                sN = 0.0d;
                tN = e;
                tD = c;
            }
            else if (sN > sD)
            {
                sN = sD;
                tN = e + b;
                tD = c;
            }
        }
        if (tN < 0.0d)
        {
            tN = 0.0d;
            if (-d < 0.0d)
                sN = 0.0d;
            else if (-d > a)
                sN = sD;
            else
            {
                sN = -d;
                sD = a;
            }
        }
        else if (tN > tD)
        {
            tN = tD;
            if ((-d + b) < 0.0d)
                sN = 0d;
            else if ((-d + b) > a)
                sN = sD;
            else
            {
                sN = (-d + b);
                sD = a;
            }
        }
        sc = (System.Math.Abs(sN) < epsilon ? 0.0 : sN / sD);
        tc = (System.Math.Abs(tN) < epsilon ? 0.0 : tN / tD);
        Vector3d dP = w + (sc * u) - (tc * v);
        return dP.sqrMagnitude;
    }
    /// <summary>
    /// Returns true if the point 'p' is inside the triangle defined by (t1,t2,t3).
    /// Assumes triangle and point are in the same plane!
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <param name="t3"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static bool PointInsideTriangle(in Vector3d t1, in Vector3d t2, in Vector3d t3, in Vector3d p)
    {
        //Compute vectors        
        Vector3d v0 = t3 - t1;
        Vector3d v1 = t2 - t1;
        Vector3d v2 = p - t1;

        //Compute dot products
        double dot00 = v0.x * v0.x + v0.y * v0.y + v0.z * v0.z;
        double dot01 = v0.x * v1.x + v0.y * v1.y + v0.z * v1.z;
        double dot02 = v0.x * v2.x + v0.y * v2.y + v0.z * v2.z;
        double dot11 = v1.x * v1.x + v1.y * v1.y + v1.z * v1.z;
        double dot12 = v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;

        //Compute barycentric coordinates
        double denom = 1 / (dot00 * dot11 - dot01 * dot01);
        double u = (dot11 * dot02 - dot01 * dot12) * denom;
        double v = (dot00 * dot12 - dot01 * dot02) * denom;

        //Check if point is in triangle
        return (u >= -0.0001f) && (v >= -0.0001f) && (u + v < 1.0001f);
    }

    /// <summary>
    /// Returns true if points are coplanar. epsilon is a positive value. 
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <param name="t3"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static bool CoPlanar(in Vector3d t1, in Vector3d t2, in Vector3d t3, in Vector3d p, in double epsilon = 0.000001f)
    {
        double val = Vector3d.Dot((p - t1), Vector3d.Cross((t3 - t1), (t2 - t1)));
        return val < epsilon && val > -epsilon;
    }

    /// <summary>
    /// Returns the closest distance from a line segment and a point
    /// </summary>
    /// <param name="v"></param>
    /// <param name="w"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static double LineSegmentPointDistance(in Vector3d s0, in Vector3d s1, in Vector3d P)
    {
        // Copyright 2001 softSurfer, 2012 Dan Sunday
        // This code may be freely used, distributed and modified for any purpose
        // providing that this copyright notice is included with it.
        // SoftSurfer makes no warranty for this code, and cannot be held
        // liable for any real or imagined damage resulting from its use.
        // Users of this code must verify correctness for their application.

        Vector3d v = s1 - s0;
        Vector3d w = P - s0;

        double c1 = Vector3d.Dot(w, v);
        if (c1 <= 0)
            return Vector3d.Distance(P, s0);

        double c2 = Vector3d.Dot(v, v);
        if (c2 <= c1)
            return Vector3d.Distance(P, s1);

        double b = c1 / c2;
        Vector3d Pb = s0 + b * v;
        return Vector3d.Distance(P, Pb);
    }

    /// <summary>
    /// Returns the closest squared distance from a line segment and a point
    /// </summary>
    /// <param name="v"></param>
    /// <param name="w"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static double LineSegmentPointDistanceSqrd(in Vector3d s0, in Vector3d s1, in Vector3d P)
    {
        Vector3d v = s1 - s0;
        Vector3d w = P - s0;

        double c1 = Vector3d.Dot(w, v);
        if (c1 <= 0)
            return (P - s0).sqrMagnitude;

        double c2 = Vector3d.Dot(v, v);
        if (c2 <= c1)
            return (P - s1).sqrMagnitude;

        double b = c1 / c2;
        Vector3d Pb = s0 + b * v;
        return (P - Pb).sqrMagnitude;
    }

    /// <summary>
    /// Returns true if input planes are parallel. Epsilon is a positive number
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    public static bool ParallelPlanes(in Plane p1, in Plane p2, in double epsilon = 0.0000001f)
    {
        Vector3d lineDir = Vector3d.Cross(p1.normal, p2.normal);
        //Determine if the cross product yielded (0,0,0) - parallel
        if (lineDir.x < epsilon && lineDir.x > -epsilon &&
            lineDir.y < epsilon && lineDir.y > -epsilon &&
            lineDir.z < epsilon && lineDir.z > -epsilon)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Line segment intersect bbox? Does not include improper intersections with edges or corners
    /// </summary>
    /// <param name="b"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static bool LineBBoxIntersect(BBox b, in Vector3d start, in Vector3d end)
    {
        var r = new Ray_ex(start, (end - start).normalized);
        return linebboxintersect(ref b, ref r, 0, (start - end).magnitude);
    }
    /// <summary>
    /// Ray intersect bbox? Does not include improper intersections with edges or corners
    /// </summary>
    /// <param name="b"></param>
    /// <param name="ray"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static bool RayBBoxIntersect(BBox b, in Ray_ex ray, in double length)
    {
        var r = new Ray_ex(ray.origin, ray.direction);
        return linebboxintersect(ref b, ref r, 0, length);
    }
    /// <summary>
    /// line bbox intersect, input arg are not modified
    /// </summary>
    /// <returns></returns>
    public static bool linebboxintersect(ref BBox b, ref Ray_ex r, double t0, double t1)
    {
        var b_min = b.Min;
        var b_max = b.Max;
        return linebboxintersect(ref b_min, ref b_max, ref r, t0, t1);
    }
    /// <summary>
    /// line bbox intersect, input arg are not modified
    /// </summary>
    /// <returns></returns>
    public static bool linebboxintersect(ref BBoxInt32 b, ref Ray_ex r, double t0, double t1)
    {
        var b_min = (Vector3d)b.Min;
        var b_max = (Vector3d)b.Max;
        return linebboxintersect(ref b_min, ref b_max, ref r, t0, t1);
    }
    /// <summary>
    /// line bbox intersect, input arg are not modified
    /// </summary>
    /// <returns></returns>
    public static bool linebboxintersect(ref Vector3d b_min, ref Vector3d b_max, ref Ray_ex r, double t0, double t1)
    {
        //based on http://people.csail.mit.edu/amy/papers/box-jgt.pdf

        double tmin, tmax, tymin, tymax, tzmin, tzmax;

        tmin = ((r.sign.x == 1 ? b_max : b_min).x - r.origin.x) * r.inv_direction.x;
        tmax = ((1 - r.sign.x == 1 ? b_max : b_min).x - r.origin.x) * r.inv_direction.x;
        tymin = ((r.sign.y == 1 ? b_max : b_min).y - r.origin.y) * r.inv_direction.y;
        tymax = ((1 - r.sign.y == 1 ? b_max : b_min).y - r.origin.y) * r.inv_direction.y;

        if (tmin > tymax || tymin > tmax)
            return false;

        if (tymin > tmin)
            tmin = tymin;

        if (tymax < tmax)
            tmax = tymax;

        tzmin = ((r.sign.z == 1 ? b_max : b_min).z - r.origin.z) * r.inv_direction.z;
        tzmax = ((1 - r.sign.z == 1 ? b_max : b_min).z - r.origin.z) * r.inv_direction.z;

        if (tmin > tzmax || tzmin > tmax)
            return false;

        if (tzmin > tmin)
            tmin = tzmin;

        if (tzmax < tmax)
            tmax = tzmax;

        return tmin <= t1 && tmax >= t0;
    }

    /// <summary>
    /// Surface intersection. Returns true if intersected and outs intersection.
    /// Does not include coplanar intersections. Returns point where line first intersects bbox along 'dir'.
    /// Intersection is with a line and an axis aligned bounding box.
    /// </summary>
    /// <param name="b"></param>
    /// <param name="point"></param>
    /// <param name="dir"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static bool LineBBoxIntersect(in BBox b, in Vector3d point, in Vector3d dir, 
        out Vector3d intersection, out Vector3d plane_normal)
    {
        var min = b.Min; var max = b.Max;
        Vector3d new_intersect = default(Vector3d);
        intersection = plane_normal = default(Vector3d);
        double best_dist_sq = double.MaxValue;
        double new_dist_sq = double.MaxValue;
        bool result = false;

        //check x-direction side
        if (dir.x != 0)
        {
            //check -x side of bbox
            if (LinePlaneIntersection(out new_intersect, point, dir, Vector3d.left, min))
            {
                new_dist_sq = (new_intersect - point).sqrMagnitude;
                if (new_intersect.y >= min.y && new_intersect.y <= max.y && new_intersect.z >= min.z &&
                    new_intersect.z <= max.z && new_dist_sq < best_dist_sq)
                {
                    plane_normal = Vector3d.left;
                    best_dist_sq = new_dist_sq;
                    intersection = new_intersect;
                    result = true;
                }
            }

            //check +x side of bbox
            if (LinePlaneIntersection(out new_intersect, point, dir, Vector3d.right, max))
            {
                new_dist_sq = (new_intersect - point).sqrMagnitude;
                if (new_intersect.y >= min.y && new_intersect.y <= max.y && new_intersect.z >= min.z &&
                    new_intersect.z <= max.z && new_dist_sq < best_dist_sq)
                {
                    plane_normal = Vector3d.right;
                    best_dist_sq = new_dist_sq;
                    intersection = new_intersect;
                    result = true;
                }
            }
        }

        //check y-direction side
        if (dir.y != 0)
        {
            //check -y side of bbox
            if (LinePlaneIntersection(out new_intersect, point, dir, Vector3d.down, min))
            {
                new_dist_sq = (new_intersect - point).sqrMagnitude;
                if (new_intersect.x >= min.x && new_intersect.x <= max.x && new_intersect.z >= min.z &&
                    new_intersect.z <= max.z && new_dist_sq < best_dist_sq)
                {
                    plane_normal = Vector3d.down;
                    best_dist_sq = new_dist_sq;
                    intersection = new_intersect;
                    result = true;
                }
            }

            //check +y side of bbox
            if (LinePlaneIntersection(out new_intersect, point, dir, Vector3d.up, max))
            {
                new_dist_sq = (new_intersect - point).sqrMagnitude;
                if (new_intersect.x >= min.x && new_intersect.x <= max.x && new_intersect.z >= min.z &&
                    new_intersect.z <= max.z && new_dist_sq < best_dist_sq)
                {
                    plane_normal = Vector3d.up;
                    best_dist_sq = new_dist_sq;
                    intersection = new_intersect;
                    result = true;
                }
            }
        }

        //check z-direction side
        if (dir.z != 0)
        {
            //check -z side of bbox
            if (LinePlaneIntersection(out new_intersect, point, dir, Vector3d.back, min))
            {
                new_dist_sq = (new_intersect - point).sqrMagnitude;
                if (new_intersect.x >= min.x && new_intersect.x <= max.x && new_intersect.y >= min.y &&
                    new_intersect.y <= max.y && new_dist_sq < best_dist_sq)
                {
                    plane_normal = Vector3d.back;
                    best_dist_sq = new_dist_sq;
                    intersection = new_intersect;
                    result = true;
                }
            }

            //check +z side of bbox
            if (LinePlaneIntersection(out new_intersect, point, dir, Vector3d.forward, max))
            {
                new_dist_sq = (new_intersect - point).sqrMagnitude;
                if (new_intersect.x >= min.x && new_intersect.x <= max.x && new_intersect.y >= min.y &&
                    new_intersect.y <= max.y && new_dist_sq < best_dist_sq)
                {
                    plane_normal = Vector3d.forward;
                    best_dist_sq = new_dist_sq;
                    intersection = new_intersect;
                    result = true;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Transform a point about the origin. Transforms are translation 'position', Rotation 'rotation', Scale 'scale'
    /// </summary>
    /// <returns></returns>
    public static Vector3d TransformPoint(in Vector3d point, in Vector3d origin, in Vector3d position, in Vector3d eulerangles, in Vector3d scale)
    {
        //useful: http://inside.mines.edu/fs_home/gmurray/ArbitraryAxisRotation/

        Vector3d s = scale;
        Vector3d r = eulerangles * deg2rad;
        Vector3d p = position;

        double cosx = System.Math.Cos(r.x);
        double sinx = System.Math.Sin(r.x);

        double cosy = System.Math.Cos(r.y);
        double siny = System.Math.Sin(r.y);

        double cosz = System.Math.Cos(r.z);
        double sinz = System.Math.Sin(r.z);

        Vector3d v = point - origin;

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

        return v2 + origin;
    }

    /// <summary>
    /// CLosest point on a line
    /// </summary>
    /// <param name="l1"></param>
    /// <param name="l2"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static Vector3d ClosestPointOnLine(in Vector3d l1, in Vector3d l2, in Vector3d p)
    {
        var a = l2 - l1;
        var t = (-a.x * (l1.x - p.x) - a.y * (l1.y - p.y) - a.z * (l1.z - p.z)) / 
            (a.x * a.x + a.y * a.y + a.z * a.z);
        return l1 + t * a;
    }

    /// <summary>
    /// 2d intersect line and circle
    /// </summary>
    /// <param name="circle">circle center</param>
    /// <param name="l1">line point 1</param>
    /// <param name="l2">line point 2</param>
    /// <param name="r">cirle radius</param>
    /// <param name="t1">'t1' parameter</param>
    /// <param name="t2">'t2' parameter</param>
    /// <returns></returns>
    static bool LineCircleIntersect(in Vector2d circle, in Vector2d l1, in Vector2d l2,
        in double r, out double t1, out double t2)
    {
        t1 = 0; t2 = 0;
        var f = circle - l1;
        var d = l2 - l1;
        var a = d.sqrMagnitude;
        var b = 2 * Vector2d.Dot(f, d);
        var c = f.sqrMagnitude - r * r;
        var discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
            return false;
        else
        {
            discriminant = Math.Sqrt(discriminant);
            t1 = (-b - discriminant) / (2 * a);
            t2 = (-b + discriminant) / (2 * a);
            return true;
        }
    }

    /// <summary>
    /// intersection line and sphere. Returns true if intersected
    /// </summary>
    /// <param name="s"></param>
    /// <param name="l1">line1point1</param>
    /// <param name="l2">line1point2</param>
    /// <param name="p1">output intersection 1</param>
    /// <param name="p2">output intersection 2</param>
    /// <returns></returns>
    public static bool LineSphereIntersect(in Sphere s, in Vector3d l1, in Vector3d l2, out Vector3d p1, out Vector3d p2)
    {
        p1 = p2 = default(Vector3d);
        var d = s.Center;
        var l2l1 = l2 - l1;
        double a = l2l1.sqrMagnitude;
        double b = 2 * Vector3d.Dot(l2l1, l1 - d);
        double c = d.sqrMagnitude + l1.sqrMagnitude - (2 * Vector3d.Dot(d, l1)) - (s.Radius * s.Radius);
        double determ = (b * b) - (4 * a * c);
        if (determ < 0)
            return false;
        else
        {
            determ = Math.Sqrt(determ);
            var t1 = (-b + determ) / (2 * a);
            var t2 = (-b - determ) / (2 * a);
            p1 = l1 + t1 * l2l1;
            p2 = l1 + t2 * l2l1;
            return true;
        }
    }

    /// <summary>
    /// Line segment and sphere intersection test
    /// </summary>
    /// <param name="s"></param>
    /// <param name="l1"></param>
    /// <param name="l2"></param>
    /// <returns></returns>
    public static bool LineSegSphereIntersect(in Sphere s, in Vector3d l1, in Vector3d l2)
    {
        return LineSegmentPointDistanceSqrd(l1, l2, s.Position) < s.Radius * s.Radius;
    }

    /// <summary>
    /// Returns true if the input point is inside or collinear with the input bounds
    /// </summary>
    /// <param name="point"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool InBounds(in Vector3d point, in Vector3d min, in Vector3d max)
    {
        return point.x >= min.x && point.x <= max.x && point.y >= min.y && point.y <= max.y &&
            point.z >= min.z && point.z <= max.z;
    }

    /// <summary>
    /// Fits the input vector to be inside this bbox within an distance 'e'. e=0 will put the point on the surface.
    /// The input point is not changed if it is already in the box
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector3d StrictFitPoint(in Vector3d Min, in Vector3d Max, Vector3d v, in float e = 0.0001f)
    {
        if (v.x >= Max.x - e)
            v.x = Max.x - e;
        else if (v.x <= Min.x + e)
            v.x = Min.x + e;
        if (v.y >= Max.y - e)
            v.y = Max.y - e;
        else if (v.y <= Min.y + e)
            v.y = Min.y + e;
        if (v.z >= Max.z - e)
            v.z = Max.z - e;
        else if (v.z <= Min.z + e)
            v.z = Min.z + e;
        return v;
    }
}

#if !UNITY
public struct Plane
{
    public Vector3 normal;
}
#endif

/// <summary>
/// Extended plane.
/// </summary>
public struct Plane_ex
{
    /// <summary>
    /// Contained plane
    /// </summary>
    public Vector3d normal;
    /// <summary>
    /// Some point on the plane.
    /// </summary>
    public Vector3d v1;

    public Plane_ex(in Vector3d normal, in Vector3d p1)
    {
        this.normal = normal;
        v1 = p1;
    }
}

/// <summary>
/// Line Struct
/// </summary>
public struct Line
{
    Vector3d a;
    Vector3d b;
    Vector3d dir;

    public Vector3d p1
    {
        get
        {
            return a;
        }
    }
    public Vector3d p2
    {
        get
        {
            return b;
        }
    }
    public Vector3d n
    {
        get
        {
            return dir;
        }
    }

    public Line(in Vector3d _p1, in Vector3d _p2)
    {
        a = _p1; b = _p2;
        dir = a - b;
    }
    public Line(in Vector3d _p1, in Vector3d _p2, Vector3d n)
    {
        a = _p1; b = _p2;
        dir = n;
    }

    /// <summary>
    /// Returns point on the line where x-axis component == input 'x' value
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public Vector3d AtXEqual(in double x)
    {
        //x(t) = x0 + nxt, y(t) = y0 + nyt, z(t) = z0 + nzt
        //nx = x1 - x0, ny = y1 - y0, nz = z1 - z0
        if (dir.x == 0)
            return new Vector3d(x, a.y, a.z);
        var t = (x - a.x) / dir.x;
        return new Vector3d(x, a.y + dir.y * t, a.z + dir.z * t);
    }

    /// <summary>
    /// Returns point on the line where y-axis component == input 'y' value
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public Vector3d AtYEqual(in double y)
    {
        //x(t) = x0 + nxt, y(t) = y0 + nyt, z(t) = z0 + nzt
        //nx = x1 - x0, ny = y1 - y0, nz = z1 - z0
        if (dir.y == 0)
            return new Vector3d(a.x, y, a.z);
        var t = (y - a.y) / dir.y;
        return new Vector3d(a.x + dir.x * t, y, a.z + dir.z * t);
    }

    /// <summary>
    /// Returns point on the line where z-axis component == input 'z' value
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public Vector3d AtZEqual(in double z)
    {
        //x(t) = x0 + nxt, y(t) = y0 + nyt, z(t) = z0 + nzt
        //nx = x1 - x0, ny = y1 - y0, nz = z1 - z0
        if (dir.z == 0)
            return new Vector3d(a.x, a.y, z);
        var t = (z - a.z) / dir.z;
        return new Vector3d(a.x + dir.x * t, a.y + dir.y * t, z);
    }
}

/// <summary>
/// Extended Ray
/// </summary>
public struct Ray_ex
{
    public Ray_ex(in Vector3d _origin, in Vector3d _direction)
    {
        origin = _origin;
        direction = _direction;
        inv_direction = new Vector3d(1 / _direction.x, 1 / _direction.y, 1 / _direction.z);
    }
    public Vector3d origin;
    public Vector3d direction;
    public Vector3d inv_direction;
    public Vector3Int sign
    {
        get
        {
            return new Vector3Int(inv_direction.x < 0 ? 1 : 0, inv_direction.y < 0 ? 1 : 0, inv_direction.z < 0 ? 1 : 0);
        }
    }
}