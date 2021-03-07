using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class PolygonIntersection
{
    static IEnumerable<Vector4d> Edges(IEnumerable<Vector2d> verts)
    {
        bool first_iter = true;
        Vector2d first = default(Vector2d);
        Vector2d prev = default(Vector2d);
        int count = 0;

        foreach (var v in verts)
        {
            if (first_iter)
            {
                first_iter = false;
                first = v;
            }
            else
            {
                yield return new Vector4d(v.x, v.y, prev.x, prev.y);
            }

            prev = v;
            count++;
        }

        if (count > 2)
            yield return new Vector4d(prev.x, prev.y, first.x, first.y);      
    }

    public static bool LineIntersects2DConvexPolygon(IEnumerable<Vector2d> verts, Vector2d line_start, Vector2d line_end)
    {
        //http://geomalgorithms.com/a13-_intersect-4.html
        //is the proper way, we are not doing the proper way here :}
        //we are doing a slower algorithm (linear time)

        Vector2d intersection;

        //test all edges in the set
        foreach (var e in Edges(verts))
            if (LineSegIntersect(new Vector2d(e.x, e.y), new Vector2d(e.z, e.w), line_start, line_end, out intersection))
                return true;

        //do a point containment test, if the point is not inside the polygon, then
        //some time it will be left or right of edges
        //however, if it is inside then it will always be left or always right (depending on how edges are wrapped)
        bool all_left = true;
        foreach (var e in Edges(verts))
            if (!Geometry.Left((Vector2)line_start, new Vector2((float)e.x, (float)e.y), new Vector2((float)e.z, (float)e.w)))
                all_left = false;

        return all_left;
    }

    static bool LineSegIntersect(Vector2d l1p1, Vector2d l1p2, Vector2d l2p1, Vector2d l2p2, out Vector2d intersection)
    {
        var s1 = l1p2 - l1p1;
        var s2 = l2p2 - l2p1;

        var s = (-s1.y * (l1p1.x - l2p1.x) + s1.x * (l1p1.y - l2p1.y)) / (-s2.x * s1.y + s1.x * s2.y);
        var t = (s2.x * (l1p1.y - l2p1.y) - s2.y * (l1p1.x - l2p1.x)) / (-s2.x * s1.y + s1.x * s2.y);

        if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
        {
            intersection = new Vector2d(l1p1.x + (t * s1.x), l1p1.y + (t * s1.y));
            return true;
        }
        else
        {
            intersection = default(Vector2d);
            return false;
        }
    }

#if UNITY_EDITOR
    public static IEnumerator test()
    {
        while (Application.isPlaying)
        {
            var time = 1.0f;
            var p = new Vector2(0, 0);

            var p2d = RandomPolygon(new Rect(p, new Vector2(30, 30)));
            DebugDraw.DrawVertexList(p2d.Select(x => new Vector3d(x.x, 0, x.y)).ToList(), Color.blue, time);

            var line_start = new Vector2(Random.Range(0, 30), Random.Range(0, 30));
            var line_dir = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;

            line_start = 30 * Mathf.Sqrt(2) * line_dir + line_start;
            var line_end = 30 * Mathf.Sqrt(2) * -line_dir + line_start;

            if (LineIntersects2DConvexPolygon(p2d, line_start, line_end))
                DebugDraw.DrawLine(line_start.ToVector3XZ(), line_end.ToVector3XZ(), Color.red, time);
            else
                DebugDraw.DrawLine(line_start.ToVector3XZ(), line_end.ToVector3XZ(), Color.green, time);

            yield return new WaitForSeconds(time + 0.1f);
        }
    }


    static System.Random randGen;
    /// <summary>
    /// Returns a new Randomized ConvexPolygon
    /// </summary>
    /// <param name="minPoints"></param>
    /// <param name="maxPoints"></param>
    /// <returns></returns>
    static List<Vector2d> RandomPolygon(Rect bounds,
        int minPoints = 3, int maxPoints = 32)
    {
        if (randGen == null)
            randGen = new System.Random();
        if (maxPoints < minPoints || minPoints < 3 || maxPoints < 3)
            throw new System.Exception("Invalid number of points range");

        maxPoints = randGen.Next(minPoints, maxPoints);

        //generate random points
        List<Vector2d> pointList = new List<Vector2d>();
        for (int i = 0; i < maxPoints; i++)
        {
            Vector2d point = new Vector2d(randGen.NextDouble() * bounds.width + bounds.x,
                randGen.NextDouble() * bounds.height+ bounds.y);
            pointList.Add(point);
        }

        //find bottom most point
        double miny = double.MaxValue;
        int index = 0;
        for (int i = 0; i < pointList.Count; i++)
        {
            if (pointList[i].y < miny)
            {
                miny = pointList[i].y;
                index = i;
            }
        }

        //graham scan
        List<System.Tuple<double, Vector2d>> byangle = new List<System.Tuple<double, Vector2d>>();
        for (int i = 0; i < pointList.Count; i++)
        {
            if (i != index)
                byangle.Add(new System.Tuple<double, Vector2d>(Vector2d.Angle(pointList[index],
                    pointList[i]), pointList[i]));
        }
        //sort by angle
        var points = new LinkedList<Vector2d>(byangle.OrderBy(x => x.Item1).Select(y => y.Item2));
        points.AddFirst(pointList[index]);

        //add points to list
        var node = points.First;
        while (node.Next.Next != null)
        {
            Vector2d a = node.Value;
            Vector2d b = node.Next.Value;
            Vector2d c = node.Next.Next.Value;

            if (!Geometry.Left(c, a, b))
            {
                points.Remove(node.Next);
                if (node.Previous != null)
                    node = node.Previous;
            }
            else
                node = node.Next;
        }
        //handle last point (c# linked list isnt allowed to be circular)
        Vector2d u = node.Value;
        Vector2d v = node.Next.Value;
        Vector2d t = pointList[index];
        if (!Geometry.Left(t, u, v))
            points.Remove(node.Next);
        //make counter clock wise order
        var l = new List<Vector2d>(points); l.Reverse();
        return l;
    }

#endif
}
