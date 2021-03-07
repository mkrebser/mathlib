using System.Collections;
using System.Collections.Generic;

public struct Triangle
{
    public Vector3d v1 { get; private set; }
    public Vector3d v2 { get; private set; }
    public Vector3d v3 { get; private set; }

    /// <summary>
    /// perpendicular vector
    /// </summary>
    public Vector3d normal { get; private set; }

    public Triangle(Vector3d p1, Vector3d p2, Vector3d p3)
    {
        v1 = p1; v2 = p2; v3 = p3;
        normal = Vector3d.Cross(v2 - v1, v3 - v1).normalized;
    }

    public Triangle(Vector3d p1, Vector3d p2, Vector3d p3, Vector3d Normal)
    {
        v1 = p1; v2 = p2; v3 = p3;
        normal = Normal;
    }

    public Vector3d Center
    {
        get
        {
            return (v1 + v2 + v3) / 3.0;
        }
    }
}
