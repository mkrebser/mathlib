using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEngine;
#endif

/// <summary>
/// Ray casting in 3d arrays
/// </summary>
public struct ArrayRayCast
{
    /// <summary>
    /// bounds of the array
    /// </summary>
    public BBoxInt32 bounds;
    /// <summary>
    /// origin of the ray
    /// </summary>
    public Vector3d origin;
    /// <summary>
    /// direction of the ray
    /// </summary>
    public Vector3d direction;
    /// <summary>
    /// maximum distance (sq) of the array
    /// </summary>
    public double dist_sq;
    /// <summary>
    /// func(x,y,z) to evaluate if a position in the array is colliding
    /// </summary>
    public System.Func<Vector3Int, bool> collide_function;
    /// <summary>
    /// If true, intersection information about the array bboxes will be returned and the bbox of each array bbox whill be evaluated for collision. If false, then 'result' data will not be filled. (And the user will likely have to get this info themselves)
    /// </summary>
    public bool evaluate_result;
#if UNITY_EDITOR
    public bool draw;
#endif
    /// <summary>
    /// Do a raycast against a array data structure. Returns true if intersected
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool Cast(out Result result)
    {
        //translated source
        //http://gamedev.stackexchange.com/questions/47362/cast-ray-to-select-block-in-voxel-game

        // From "A Fast Voxel Traversal Algorithm for Ray Tracing"
        // by John Amanatides and Andrew Woo, 1987
        // <http://www.cse.yorku.ca/~amana/research/grid.pdf>
        // <http://citeseer.ist.psu.edu/viewdoc/summary?doi=10.1.1.42.3443>
        // Extensions to the described algorithm:
        //   • Imposed a distance limit.
        //   • The face passed through to reach the current cube is provided to
        //     the callback.

        // The foundation of this algorithm is a parameterized representation of
        // the provided ray,
        //                    origin + t * direction,
        // except that t is not actually stored; rather, at any given point in the
        // traversal, we keep track of the *greater* t values which we would have
        // if we took a step sufficient to cross a cube boundary along that axis
        // (i.e. change the integer part of the coordinate) in the variables
        // tMaxX, tMaxY, and tMaxZ.

        if (ReferenceEquals(null, collide_function))
            throw new System.Exception("Not Initialized");
        result = default(Result);

        //make sure cast will hit the bounds
        if (!VerifyOrigin())
            return false;

        // Break out direction vector.
        var dx = direction.x;
        var dy = direction.y;
        var dz = direction.z;

        // Avoids an infinite loop.
        if (dx == 0 && dy == 0 && dz == 0)
            throw new System.Exception("Raycast in zero direction!");

        // voxel cube face intersection surface
        var face = Vector3Int.zero;
        // Direction to increment x,y,z when stepping.
        var stepX = signum(direction.x);
        var stepY = signum(direction.y);
        var stepZ = signum(direction.z);
        //// See description above. The initial values depend on the fractional
        //// part of the origin.
        var tMaxX = intbound(origin.x, direction.x);
        var tMaxY = intbound(origin.y, direction.y);
        var tMaxZ = intbound(origin.z, direction.z);
        // The change in t when taking a step (always positive).
        var tDeltaX = stepX / direction.x;
        var tDeltaY = stepY / direction.y;
        var tDeltaZ = stepZ / direction.z;

        //we stop checking past a certain distance, however, due to the angle difference of the end points
        //from the origin to various blocks, the ray cast can get longer. Because of this we have to slightly
        //go past the desired distance (change in distance is dependant on distance, if longer, change must be longer)
        dist_sq = dist_sq * 1.2 + 6;

        var cpos = Vector3Int.FloorToInt(origin);  // actual world position of voxel
        var prev_pos = cpos;

        do
        {
#if UNITY_EDITOR
            if (draw) DebugDraw.DrawBox(new BBox(cpos, new Vector3d(1)), Color.yellow);
#endif

            //if position is collideable
            if (collide_function(cpos))
            {
                if (evaluate_result)
                {
                    var block_bbox = new BBox(cpos, new Vector3d(1));
                    //if this is the first block we processed (then we started inside the colliding voxel)
                    if (face == Vector3Int.zero)
                    {
                        result.intersection = block_bbox.StrictFitPoint(origin);
                        result.index = cpos;
                        result.StartedInSurface = true;
                        result.normal = Vector3d.zero;
                        return true;
                    }
                    //get ray cast hit parameters
                    Vector3d normal;
                    result.intersection = exact_pos(face, cpos, 1, origin, direction, out normal);
                    result.normal = normal;
                    result.intersection = block_bbox.StrictFitPoint(result.intersection);
                    //if this intersection happened after our max distance then return false :)
                    if ((result.intersection - origin).sqrMagnitude > dist_sq)
                    {
                        return false;
                    }
                    result.index = cpos;
                }

                //otherwise successful intersection
                return true;
            }

            // tMaxX stores the t-value at which we cross a cube boundary along the
            // X axis, and similarly for Y and Z. Therefore, choosing the least tMax
            // chooses the closest cube boundary. 
            if (tMaxX < tMaxY)
            {
                if (tMaxX < tMaxZ)
                {
                    cpos.x += stepX;  // Update which cube we are now in.
                    tMaxX += tDeltaX;  // Adjust tMaxX to the next X-oriented boundary crossing.
                    face.x = -stepX;  // Record the normal vector of the cube face we entered.
                    face.y = 0;
                    face.z = 0;
                }
                else
                {
                    cpos.z += stepZ;
                    tMaxZ += tDeltaZ;
                    face.x = 0;
                    face.y = 0;
                    face.z = -stepZ;
                }
            }
            else
            {
                if (tMaxY < tMaxZ)
                {
                    cpos.y += stepY;
                    tMaxY += tDeltaY;
                    face.x = 0;
                    face.y = -stepY;
                    face.z = 0;
                }
                else
                {
                    cpos.z += stepZ;
                    tMaxZ += tDeltaZ;
                    face.x = 0;
                    face.y = 0;
                    face.z = -stepZ;
                }
            }
        } while ((cpos - origin).sqrMagnitude <= dist_sq && bounds.IndexContains(cpos));
        return false;
    }

    /// <summary>
    /// Get all points along the ray regardless if it collides
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Vector3Int> RayPoints()
    {
        if (ReferenceEquals(null, collide_function))
            throw new System.Exception("Not Initialized");

        //make sure cast will hit the bounds
        if (!VerifyOrigin())
            yield break;

        // Cube containing origin point.
        var cpos = (Vector3Int)origin;

        // Break out direction vector.
        var dx = direction.x;
        var dy = direction.y;
        var dz = direction.z;

        // Direction to increment x,y,z when stepping.
        var stepX = signum(dx);
        var stepY = signum(dy);
        var stepZ = signum(dz);

        // See description above. The initial values depend on the fractional
        // part of the origin.
        var tMaxX = intbound(origin.x, dx);
        var tMaxY = intbound(origin.y, dy);
        var tMaxZ = intbound(origin.z, dz);

        // Buffer for collecting face info
        var face = Vector3Int.zero;

        // Avoids an infinite loop.
        if (dx == 0 && dy == 0 && dz == 0)
            throw new System.Exception("Raycast in zero direction!");

        // The change in t when taking a step (always positive).
        var tDeltaX = stepX / dx;
        var tDeltaY = stepY / dy;
        var tDeltaZ = stepZ / dz;

        //we stop checking past a certain distance, however, due to the angle difference of the end points
        //from the origin to various blocks, the ray cast can get longer. Because of this we have to slightly
        //go past the desired distance (change in distance is dependant on distance, if longer, change must be longer)
        dist_sq = dist_sq * 1.2 + 6;

        while ((cpos - origin).sqrMagnitude <= dist_sq &&
            bounds.IndexContains(cpos))
        {
            yield return cpos;

            // tMaxX stores the t-value at which we cross a cube boundary along the
            // X axis, and similarly for Y and Z. Therefore, choosing the least tMax
            // chooses the closest cube boundary. Only the first case of the four
            // has been commented in detail.
            if (tMaxX < tMaxY)
            {
                if (tMaxX < tMaxZ)
                {
                    // Update which cube we are now in.
                    cpos.x += stepX;
                    // Adjust tMaxX to the next X-oriented boundary crossing.
                    tMaxX += tDeltaX;
                    // Record the normal vector of the cube face we entered.
                    face.x = -stepX;
                    face.y = 0;
                    face.z = 0;
                }
                else
                {
                    cpos.z += stepZ;
                    tMaxZ += tDeltaZ;
                    face.x = 0;
                    face.y = 0;
                    face.z = -stepZ;
                }
            }
            else
            {
                if (tMaxY < tMaxZ)
                {
                    cpos.y += stepY;
                    tMaxY += tDeltaY;
                    face.x = 0;
                    face.y = -stepY;
                    face.z = 0;
                }
                else
                {
                    // Identical to the second case, repeated for simplicity in
                    // the conditionals.
                    cpos.z += stepZ;
                    tMaxZ += tDeltaZ;
                    face.x = 0;
                    face.y = 0;
                    face.z = -stepZ;
                }
            }
        }
    }

    /// <summary>
    /// Get all points along a ray until a collision happens
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Vector3Int> RayCastPoints()
    {
        foreach (var p in RayPoints())
        {
            if (collide_function(p))
            {
                yield return p;
                yield break;
            }
            else
                yield return p;
        }

    }

    bool VerifyOrigin()
    {
        //contain inside smallest voxel to avoid border issues
        origin = Geometry.StrictFitPoint(Vector3Int.FloorToInt(origin), Vector3Int.FloorToInt(origin) + new Vector3Int(1), origin);

        //fit starting point to be inside bounds
        if (!bounds.StrictContains(origin))
        {
            Vector3d n; Vector3d origind;
            var bresult = Geometry.LineBBoxIntersect(bounds, origin, direction, out origind, out n);
            origin = bounds.StrictFitPoint(origind, 0.00001);

            //if the line doesnt intersect the bbox, then just return
            if (!bresult)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Resets everything but the 'collide_function' delegate
    /// </summary>
    public void Reset()
    {
        bounds = default(BBoxInt32);
        origin = default(Vector3d);
        direction = default(Vector3d);
        dist_sq = default(double);
        evaluate_result = default(bool);
#if UNITY_EDITOR
        draw = default(bool);
#endif
    }

//helper methods
static double intbound(double s, double ds)
    {
        // Find the smallest positive t such that s+t*ds is an integer.
        if (ds < 0)
        {
            return intbound(-s, -ds);
        }
        else
        {
            s = mod(s, 1);
            // problem is now s+t*ds = 1
            return (1 - s) / ds;
        }
    }
    static int signum(double x)
    {
        return x > 0 ? 1 : x < 0 ? -1 : 0;
    }
    static double mod(double value, int modulus)
    {
        return (value % modulus + modulus) % modulus;
    }

    /// <summary>
    /// Return exact position of line cube face intersection
    /// </summary>
    /// <param name="face"></param>
    /// <param name="cube_pos"></param>
    /// <param name="voxel_len"></param>
    /// <param name="start"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    static Vector3d exact_pos(Vector3Int face, Vector3Int cube_pos,
        int voxel_len, Vector3d start, Vector3d dir, out Vector3d normal)
    {
        Vector3d r;
        var min = cube_pos;
        var max = min + new Vector3Int(voxel_len, voxel_len, voxel_len);
        if (face.x != 0)
        {
            if (face.x > 0)
            {
                normal = Vector3d.right;
                Geometry.LinePlaneIntersection(out r, start, dir, Vector3d.right, max);
            }
            else
            {
                normal = Vector3d.left;
                Geometry.LinePlaneIntersection(out r, start, dir, Vector3d.left, min);
            }
        }
        else if (face.y != 0)
        {
            if (face.y > 0)
            {
                normal = Vector3.up;
                Geometry.LinePlaneIntersection(out r, start, dir, Vector3d.up, max);
            }
            else
            {
                normal = Vector3d.down;
                Geometry.LinePlaneIntersection(out r, start, dir, Vector3d.down, min);
            }
        }
        else if (face.z != 0)
        {
            if (face.z > 0)
            {
                normal = Vector3d.forward;
                Geometry.LinePlaneIntersection(out r, start, dir, Vector3d.forward, max);
            }
            else
            {
                normal = Vector3d.back;
                Geometry.LinePlaneIntersection(out r, start, dir, Vector3d.back, min);
            }
        }
        else
            throw new System.Exception("Error, no input face.");
        return r;
    }

    public struct Result
    {
        /// <summary>
        /// closest point of intersection
        /// </summary>
        public Vector3d intersection;
        /// <summary>
        /// normal of hit
        /// </summary>
        public Vector3d normal;
        /// <summary>
        /// index of hit
        /// </summary>
        public Vector3Int index;
        /// <summary>
        /// was ray intersecting when it began?
        /// </summary>
        public bool StartedInSurface;
    }

#if UNITY_EDITOR
    public static IEnumerator test()
    {
        var time = 3.0f;
        var density = 0.02f;
        var arr = new GameObject[30, 30, 30];
        var len = 30 * Mathf.Sqrt(2);

        ArrayRayCast v;
        v.collide_function = (Vector3Int index) => { return arr[index.x, index.y, index.z] != null; };
        v.bounds = new BBoxInt32(Vector3Int.zero, new Vector3Int(30));
        v.dist_sq = len * len;
        v.evaluate_result = true;
        v.draw = false;

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
            var pos = (Vector3.one * 15) + new Vector3(Random.Range(-1, 1f), Random.Range(-1, 1f), Random.Range(-1, 1f)).normalized * 15;
            if (VectorExtensions.ApproximatelyEqual(pos, Vector3.zero)) pos = -Vector3.one;
            var dir = (new Vector3(Random.Range(5, 25f), Random.Range(5, 25f), Random.Range(5, 25f)) - pos).normalized;
            v.origin = pos;
            v.direction = dir;

            foreach (var p in v.RayCastPoints())
            {
                DebugDraw.DrawBox(p, new Vector3d(1), Color.white, time);
            }

            Result result;
            if (v.Cast(out result))
            {
                DebugDraw.DrawLine(v.origin, result.intersection, Color.blue, time);
                DebugDraw.DrawBox(result.index, new Vector3d(1) * 1.1f, Color.red, time);
            }
            else
                DebugDraw.DrawLine(pos, pos + dir * len, Color.green, time);

            yield return new WaitForSeconds(time + 0.1f);
        }
    }
#endif
}
