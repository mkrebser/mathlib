using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#endif

public struct TransformStruct
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public Matrix4x4Struct _matrix;

    public TransformStruct(Vector3 pos, Quaternion rot, Vector3 s)
    {
        Position = pos; Rotation = rot; Scale = s; _matrix = new Matrix4x4Struct(Position, Rotation, Scale);
    }

#if UNITY
    public TransformStruct(Transform t) { Position = t.position; Rotation = t.rotation; Scale = t.localScale; _matrix = new Matrix4x4Struct(Position, Rotation, Scale); }
#endif

    public void UpdateMatrix()
    {
        _matrix = new Matrix4x4Struct(Position, Rotation, Scale);
    }

    public override string ToString()
    {
        return string.Format("Position: {0}\nRotation: {1}\nScale: {2}",
            new object[3] { Position.ToString(), Rotation.ToString(), Scale.ToString() });
    }
}
