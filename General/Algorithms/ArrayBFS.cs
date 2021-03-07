using System.Collections;
using System.Collections.Generic;

/// <summary>
/// BFS for 2d and 3d arrays
/// </summary>
public class ArrayBFS
{
    public BBoxInt32 bounds;
    ArrayQueue<Vector3Int> points;
    HashSet<Vector3Int> visited;
    public List<Vector3Int> results;

    Vector3Int min;
    Vector3Int max;

    public ArrayBFS(int search_list_capacity)
    {
        points = new ArrayQueue<Vector3Int>(search_list_capacity);
        visited = new HashSet<Vector3Int>();
        results = new List<Vector3Int>(search_list_capacity);
    }

    public void GetPoints(Vector3Int start, System.Func<Vector3Int, bool> func)
    {
        points.Clear();
        visited.Clear();
        results.Clear();

        calc_min_max();

        if (func(start))
        {
            AddPointNeighbors(start);
            results.Add(start);
        }

        while (!points.Empty())
        {
            var p = points.Dequeue();
            if (func(p))
            {
                AddPointNeighbors(p);
                results.Add(p);
            }
        }
    }

    void AddPointNeighbors(Vector3Int point)
    {
        var n = new Vector3Int(point.x + 1, point.y, point.z);
        if (index_in_bounds(n) && !visited.Contains(n)) { visited.Add(n); points.Enqueue(n); }

        n = new Vector3Int(point.x - 1, point.y, point.z);
        if (index_in_bounds(n) && !visited.Contains(n)) { visited.Add(n); points.Enqueue(n); }

        n = new Vector3Int(point.x, point.y + 1, point.z);
        if (index_in_bounds(n) && !visited.Contains(n)) { visited.Add(n); points.Enqueue(n); }

        n = new Vector3Int(point.x, point.y - 1, point.z);
        if (index_in_bounds(n) && !visited.Contains(n)) { visited.Add(n); points.Enqueue(n); }

        n = new Vector3Int(point.x, point.y, point.z + 1);
        if (index_in_bounds(n) && !visited.Contains(n)) { visited.Add(n); points.Enqueue(n); }

        n = new Vector3Int(point.x, point.y, point.z - 1);
        if (index_in_bounds(n) && !visited.Contains(n)) { visited.Add(n); points.Enqueue(n); }
    }

    void calc_min_max()
    {
        this.min = bounds.Min;
        this.max = bounds.Max;
    }

    bool index_in_bounds(Vector3Int p)
    {
        return p.x >= min.x && p.x < max.x &&
               p.y >= min.y && p.y < max.y &&
               p.z >= min.z && p.z < max.z;
    }

#if UNITY_EDITOR
    using UnityEngine;
    public static IEnumerator test()
    {
        var time = 2.0f;
        var density = 0.02f;
        var arr = new GameObject[30, 30, 30];

        ArrayBFS v = new ArrayBFS(10000);
        v.bounds = new BBoxInt32(Vector3Int.zero, new Vector3Int(30));

        for (int x = 0; x < 30; x++)
            for (int y = 0; y < 30; y++)
                for (int z = 0; z < 30; z++)
                {
                    if (Random.Range(0, 1f) > density) continue;
                    arr[x, y, z] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    arr[x, y, z].transform.position = new Vector3(x, y, z) + Vector3.one * 0.5f;
                }

        while (Application.isPlaying)
        {
            var start = new Vector3Int(Random.Range(0, 30), Random.Range(0, 30), Random.Range(0, 30));
            var sphere = new Sphere(start, 8);

            v.GetPoints(start, (Vector3Int index) => { return sphere.ContainsPoint(index); });
            HashSet<Vector3Int> points = new HashSet<Vector3Int>(v.results);

            foreach (var p in points)
                DebugDraw.DrawBox(p, Vector3.one, Color.white, time);

            yield return new WaitForSeconds(time + 0.1f);

            foreach (var p in points)
                if (arr[p.x, p.y, p.z] != null)
                    DebugDraw.DrawBox(p, Vector3.one, Color.red, time);

            yield return new WaitForSeconds(time + 0.1f);
        }
    }
#endif
}
