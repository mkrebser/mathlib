#if UNITY

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DebugDraw
{
    public static void DrawLine(Vector3d p, Vector3d p2, Color c, float duration)
    {
        Debug.DrawLine(p.Vec3, p2.Vec3, c, duration);
    }
    public static void DrawLine(Vector3d p, Vector3d p2, Color c)
    {
        Debug.DrawLine(p.Vec3, p2.Vec3, c);
    }
    /// <summary>
    /// Draws a plane
    /// </summary>
    /// <param name="position"></param>
    /// <param name="normal"></param>
    public static void DrawPlane(Vector3d position, Vector3d normal, float duration)
    {
        if (normal == Vector3.zero)
        {
            return;
        }

        Vector3 v3;

        if (normal.normalized != Vector3.forward)
            v3 = Vector3.Cross(normal.Vec3, Vector3.forward).normalized * normal.Vec3.magnitude;
        else
            v3 = Vector3.Cross(normal.Vec3, Vector3.up).normalized * normal.Vec3.magnitude; ;

        var corner0 = position + v3;
        var corner2 = position - v3;
        var q = Quaternion.AngleAxis(90.0f, normal.Vec3);
        v3 = q * v3;
        var corner1 = position + v3;
        var corner3 = position - v3;

        Debug.DrawLine(corner0.Vec3, corner2.Vec3, Color.green, duration);
        Debug.DrawLine(corner1.Vec3, corner3.Vec3, Color.green, duration);
        Debug.DrawLine(corner0.Vec3, corner1.Vec3, Color.green, duration);
        Debug.DrawLine(corner1.Vec3, corner2.Vec3, Color.green, duration);
        Debug.DrawLine(corner2.Vec3, corner3.Vec3, Color.green, duration);
        Debug.DrawLine(corner3.Vec3, corner0.Vec3, Color.green, duration);
        Debug.DrawRay(position.Vec3, normal.Vec3, Color.red, duration);
    }
    /// <summary>
    /// Draws a box. Used for debug purposes.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="extents"></param>
    /// <param name="color"></param>
    /// <param name="duration"></param>
    public static void DrawBox(Vector3d pos, Vector3d extents, Color color, float duration)
    {
        Debug.DrawLine(pos.Vec3, new Vector3d(pos.x, pos.y, pos.z + extents.z).Vec3, color, duration);
        Debug.DrawLine(pos.Vec3, new Vector3d(pos.x + extents.x, pos.y, pos.z).Vec3, color, duration);
        Debug.DrawLine(pos.Vec3, new Vector3d(pos.x, pos.y + extents.y, pos.z).Vec3, color, duration);
        Vector3d p1 = new Vector3d(pos.x, pos.y + extents.y, pos.z);
        Debug.DrawLine(p1.Vec3, new Vector3d(pos.x, pos.y + extents.y, pos.z + extents.z).Vec3, color, duration);
        Debug.DrawLine(p1.Vec3, new Vector3d(pos.x + extents.x, pos.y + extents.y, pos.z).Vec3, color, duration);
        Vector3d p2 = pos + extents;
        Debug.DrawLine(p2.Vec3, new Vector3d(p2.x, p2.y, p2.z - extents.z).Vec3, color, duration);
        Debug.DrawLine(p2.Vec3, new Vector3d(p2.x - extents.x, p2.y, p2.z).Vec3, color, duration);
        Debug.DrawLine(p2.Vec3, new Vector3d(p2.x, p2.y - extents.y, p2.z).Vec3, color, duration);
        Vector3d p3 = new Vector3d(p2.x, p2.y - extents.y, p2.z);
        Debug.DrawLine(p3.Vec3, new Vector3d(p2.x, p2.y - extents.y, p2.z - extents.z).Vec3, color, duration);
        Debug.DrawLine(p3.Vec3, new Vector3d(p2.x - extents.x, p2.y - extents.y, p2.z).Vec3, color, duration);
        Debug.DrawLine(new Vector3d(pos.x + extents.x, pos.y, pos.z).Vec3,
            new Vector3d(pos.x + extents.x, pos.y + extents.y, pos.z).Vec3, color, duration);
        Debug.DrawLine(new Vector3d(pos.x, pos.y, pos.z + extents.z).Vec3,
            new Vector3d(pos.x, pos.y + extents.y, pos.z + extents.z).Vec3, color, duration);
    }
    /// <summary>
    /// Draws a box. Used for debug purposes.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="extents"></param>
    /// <param name="color"></param>
    public static void DrawBox(Vector3d pos, Vector3d extents, Color color)
    {
        Debug.DrawLine(pos.Vec3, new Vector3d(pos.x, pos.y, pos.z + extents.z).Vec3, color);
        Debug.DrawLine(pos.Vec3, new Vector3d(pos.x + extents.x, pos.y, pos.z).Vec3, color);
        Debug.DrawLine(pos.Vec3, new Vector3d(pos.x, pos.y + extents.y, pos.z).Vec3, color);
        Vector3d p1 = new Vector3d(pos.x, pos.y + extents.y, pos.z).Vec3;
        Debug.DrawLine(p1.Vec3, new Vector3d(pos.x, pos.y + extents.y, pos.z + extents.z).Vec3, color);
        Debug.DrawLine(p1.Vec3, new Vector3d(pos.x + extents.x, pos.y + extents.y, pos.z).Vec3, color);
        Vector3d p2 = pos + extents;
        Debug.DrawLine(p2.Vec3, new Vector3d(p2.x, p2.y, p2.z - extents.z).Vec3, color);
        Debug.DrawLine(p2.Vec3, new Vector3d(p2.x - extents.x, p2.y, p2.z).Vec3, color);
        Debug.DrawLine(p2.Vec3, new Vector3d(p2.x, p2.y - extents.y, p2.z).Vec3, color);
        Vector3d p3 = new Vector3d(p2.x, p2.y - extents.y, p2.z).Vec3;
        Debug.DrawLine(p3.Vec3, new Vector3d(p2.x, p2.y - extents.y, p2.z - extents.z).Vec3, color);
        Debug.DrawLine(p3.Vec3, new Vector3d(p2.x - extents.x, p2.y - extents.y, p2.z).Vec3, color);
        Debug.DrawLine(new Vector3d(pos.x + extents.x, pos.y, pos.z).Vec3,
            new Vector3d(pos.x + extents.x, pos.y + extents.y, pos.z).Vec3, color);
        Debug.DrawLine(new Vector3d(pos.x, pos.y, pos.z + extents.z).Vec3,
            new Vector3d(pos.x, pos.y + extents.y, pos.z + extents.z).Vec3, color);
    }
    /// <summary>
    /// Draw a bbox cast
    /// </summary>
    /// <param name="b"></param>
    /// <param name="dir"></param>
    /// <param name="length"></param>
    public static void DrawBBoxCast(BBox b, Vector3 dir, float length, Color color, float duration = 30.0f)
    {
        Vector3 p1 = b.Position.Vec3;
        Vector3 p2 = b.Position.Vec3 + (dir * length);
        Vector3 s = b.Size.Vec3;
        DrawBox(p1, b.Size.Vec3, color, duration);
        DrawBox(p2, b.Size.Vec3, color, duration);
        Debug.DrawLine(p1, p2, color, duration);
        Debug.DrawLine(new Vector3(p1.x, p1.y, p1.z + s.z), new Vector3(p2.x, p2.y, p2.z + s.z), color, duration);
        Debug.DrawLine(new Vector3(p1.x, p1.y + s.y, p1.z), new Vector3(p2.x, p2.y + s.y, p2.z), color, duration);
        Debug.DrawLine(new Vector3(p1.x, p1.y + s.y, p1.z + s.z), new Vector3(p2.x, p2.y + s.y, p2.z + s.z), color, duration);
        Debug.DrawLine(new Vector3(p1.x + s.x, p1.y, p1.z), new Vector3(p2.x + s.x, p2.y, p2.z), color, duration);
        Debug.DrawLine(new Vector3(p1.x + s.x, p1.y, p1.z + s.z), new Vector3(p2.x + s.x, p2.y, p2.z + s.z), color, duration);
        Debug.DrawLine(new Vector3(p1.x + s.x, p1.y + s.y, p1.z), new Vector3(p2.x + s.x, p2.y + s.y, p2.z), color, duration);
        Debug.DrawLine(p1 + s, p2 + s, color, duration);
    }
    /// <summary>
    /// Draw a polygon 3D
    /// </summary>
    /// <param name="p"></param>
    /// <param name="c"></param>
    /// <param name="duration"></param>
    public static void DrawPolygon3D(Polygon3D p, Color c, float duration)
    {
        if (p == null)
            throw new System.Exception("Error, cannot draw null polygon");
        foreach (var v in p.triangles_vertices)
        {
            Debug.DrawLine(v.v1.Vec3, v.v2.Vec3, c, duration);
            Debug.DrawLine(v.v2.Vec3, v.v3.Vec3, c, duration);
            Debug.DrawLine(v.v3.Vec3, v.v1.Vec3, c, duration);
        }

        Debug.DrawLine(p.Center.Vec3 + Vector3.up * 0.25f, p.Center.Vec3 + Vector3.down * 0.25f, Color.green, duration);
        Debug.DrawLine(p.Center.Vec3 + Vector3.left * 0.25f, p.Center.Vec3 + Vector3.right * 0.25f, Color.red, duration);
        Debug.DrawLine(p.Center.Vec3 + Vector3.forward * 0.25f, p.Center.Vec3 + Vector3.back * 0.25f, Color.blue, duration);
    }
    /// <summary>
    /// Draw a polygon 3D for one frame
    /// </summary>
    /// <param name="p"></param>
    /// <param name="c"></param>
    /// <param name="duration"></param>
    public static void DrawPolygon3D(Polygon3D p, Color c)
    {
        if (p == null)
            throw new System.Exception("Error, cannot draw null polygon");
        foreach (var v in p.triangles_vertices)
        {
            Debug.DrawLine(v.v1.Vec3, v.v2.Vec3, c);
            Debug.DrawLine(v.v2.Vec3, v.v3.Vec3, c);
            Debug.DrawLine(v.v3.Vec3, v.v1.Vec3, c);
        }
        Debug.DrawLine(p.Center.Vec3 + Vector3.up * 0.25f, p.Center.Vec3 + Vector3.down * 0.25f, Color.green);
        Debug.DrawLine(p.Center.Vec3 + Vector3.left * 0.25f, p.Center.Vec3 + Vector3.right * 0.25f, Color.red);
        Debug.DrawLine(p.Center.Vec3 + Vector3.forward * 0.25f, p.Center.Vec3 + Vector3.back * 0.25f, Color.blue);
    }
    /// <summary>
    /// Draw a polygon 3D for one frame
    /// </summary>
    /// <param name="p"></param>
    /// <param name="c"></param>
    /// <param name="duration"></param>
    public static void DrawPolygon3D(Polygon3D p, Color c, float duration, Vector3 offest)
    {
        if (p == null)
            throw new System.Exception("Error, cannot draw null polygon");
        foreach (var v in p.triangles_vertices)
        {
            Debug.DrawLine(v.v1.Vec3 + offest, v.v2.Vec3 + offest, c, duration);
            Debug.DrawLine(v.v2.Vec3 + offest, v.v3.Vec3 + offest, c, duration);
            Debug.DrawLine(v.v3.Vec3 + offest, v.v1.Vec3 + offest, c, duration);
        }
        Debug.DrawLine(p.Center.Vec3 + Vector3.up * 0.25f + offest, p.Center.Vec3 + Vector3.down * 0.25f + offest, Color.green, duration);
        Debug.DrawLine(p.Center.Vec3 + Vector3.left * 0.25f + offest, p.Center.Vec3 + Vector3.right * 0.25f + offest, Color.red, duration);
        Debug.DrawLine(p.Center.Vec3 + Vector3.forward * 0.25f + offest, p.Center.Vec3 + Vector3.back * 0.25f + offest, Color.blue, duration);
    }
    /// <summary>
    /// Draw a box
    /// </summary>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="duration"></param>
    public static void DrawBox(Box b, Color c, float duration)
    {
        Vector3d[] v = b.Verts.ToArray();
        Debug.DrawLine(v[0].Vec3, v[1].Vec3, c, duration);
        Debug.DrawLine(v[0].Vec3, v[2].Vec3, c, duration);
        Debug.DrawLine(v[0].Vec3, v[4].Vec3, c, duration);
        Debug.DrawLine(v[7].Vec3, v[3].Vec3, c, duration);
        Debug.DrawLine(v[7].Vec3, v[6].Vec3, c, duration);
        Debug.DrawLine(v[7].Vec3, v[5].Vec3, c, duration);
        Debug.DrawLine(v[2].Vec3, v[6].Vec3, c, duration);
        Debug.DrawLine(v[2].Vec3, v[3].Vec3, c, duration);
        Debug.DrawLine(v[5].Vec3, v[1].Vec3, c, duration);
        Debug.DrawLine(v[5].Vec3, v[4].Vec3, c, duration);
        Debug.DrawLine(v[3].Vec3, v[1].Vec3, c, duration);
        Debug.DrawLine(v[6].Vec3, v[4].Vec3, c, duration);
    }
    /// <summary>
    /// Draw a box
    /// </summary>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="duration"></param>
    public static void DrawBox(Box b, Color c)
    {
        Vector3d[] v = b.Verts.ToArray();
        Debug.DrawLine(v[0].Vec3, v[1].Vec3, c);
        Debug.DrawLine(v[0].Vec3, v[2].Vec3, c);
        Debug.DrawLine(v[0].Vec3, v[4].Vec3, c);
        Debug.DrawLine(v[7].Vec3, v[3].Vec3, c);
        Debug.DrawLine(v[7].Vec3, v[6].Vec3, c);
        Debug.DrawLine(v[7].Vec3, v[5].Vec3, c);
        Debug.DrawLine(v[2].Vec3, v[6].Vec3, c);
        Debug.DrawLine(v[2].Vec3, v[3].Vec3, c);
        Debug.DrawLine(v[5].Vec3, v[1].Vec3, c);
        Debug.DrawLine(v[5].Vec3, v[4].Vec3, c);
        Debug.DrawLine(v[3].Vec3, v[1].Vec3, c);
        Debug.DrawLine(v[6].Vec3, v[4].Vec3, c);
    }
    /// <summary>
    /// Draw a box
    /// </summary>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="duration"></param>
    public static void DrawBox(BBox b, Color c, float duration)
    {
        DrawBox(b.Position.Vec3, b.Size.Vec3, c, duration);
    }
    /// <summary>
    /// Draw a box
    /// </summary>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="duration"></param>
    public static void DrawBox(BBox b, Color c)
    {
        DrawBox(b.Position.Vec3, b.Size.Vec3, c);
    }
    /// <summary>
    /// it really just draws 3 lines.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="c"></param>
    /// <param name="duration"></param>
    public static void DrawSphere(Sphere s, Color c, float duration)
    {
        Vector3d p1 = new Vector3d(s.Center.x + s.Radius, s.Center.y, s.Center.z);
        Vector3d p2 = new Vector3d(s.Center.x - s.Radius, s.Center.y, s.Center.z);
        Vector3d p3 = new Vector3d(s.Center.x, s.Center.y + s.Radius, s.Center.z);
        Vector3d p4 = new Vector3d(s.Center.x, s.Center.y - s.Radius, s.Center.z);
        Vector3d p5 = new Vector3d(s.Center.x, s.Center.y, s.Center.z + s.Radius);
        Vector3d p6 = new Vector3d(s.Center.x, s.Center.y, s.Center.z - s.Radius);
        Debug.DrawLine(p1.Vec3, p2.Vec3, c, duration);
        Debug.DrawLine(p3.Vec3, p4.Vec3, c, duration);
        Debug.DrawLine(p5.Vec3, p6.Vec3, c, duration);
    }
    /// <summary>
    /// it really just draws 3 lines.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="c"></param>
    /// <param name="duration"></param>
    public static void DrawSphere(Sphere s, Color c)
    {
        Vector3d p1 = new Vector3d(s.Center.x + s.Radius, s.Center.y, s.Center.z);
        Vector3d p2 = new Vector3d(s.Center.x - s.Radius, s.Center.y, s.Center.z);
        Vector3d p3 = new Vector3d(s.Center.x, s.Center.y + s.Radius, s.Center.z);
        Vector3d p4 = new Vector3d(s.Center.x, s.Center.y - s.Radius, s.Center.z);
        Vector3d p5 = new Vector3d(s.Center.x, s.Center.y, s.Center.z + s.Radius);
        Vector3d p6 = new Vector3d(s.Center.x, s.Center.y, s.Center.z - s.Radius);
        Debug.DrawLine(p1.Vec3, p2.Vec3, c);
        Debug.DrawLine(p3.Vec3, p4.Vec3, c);
        Debug.DrawLine(p5.Vec3, p6.Vec3, c);
    }
    /// <summary>
    /// Draw a point
    /// </summary>
    /// <param name="p"></param>
    /// <param name="c"></param>
    /// <param name="duration"></param>
    /// <param name="len"></param>
    public static void DrawPoint(Vector3d p, float duration, float len = 0.25f)
    {
        DrawLine(p - Vector3d.right * len, p + Vector3d.right * len, Color.red, duration);
        DrawLine(p - Vector3d.up * len, p + Vector3d.up * len, Color.green, duration);
        DrawLine(p - Vector3d.forward * len, p + Vector3d.forward * len, Color.blue, duration);
    }

    public static void DrawPoint(Vector3d p, float duration, Color c, float len = 0.25f)
    {
        DrawLine(p - Vector3d.right * len, p + Vector3d.right * len, c, duration);
        DrawLine(p - Vector3d.up * len, p + Vector3d.up * len, c, duration);
        DrawLine(p - Vector3d.forward * len, p + Vector3d.forward * len, c, duration);
    }

    public static void DrawVertexList(List<Vector3d> list, Color c, float duration = -1)
    {
        if (list == null || list.Count < 2) return; 

        for (int i = 1; i < list.Count; i++)
        {
            if (duration < 0)
                DebugDraw.DrawLine(list[i], list[i - 1], c);
            else
                DebugDraw.DrawLine(list[i], list[i - 1], c, duration);
        }
        if (duration < 0)
            DebugDraw.DrawLine(list[0], list[list.Count - 1], c);
        else
            DebugDraw.DrawLine(list[0], list[list.Count - 1], c, duration);
    }
}

#endif