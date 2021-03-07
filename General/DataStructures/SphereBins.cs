using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// A 3d array of bins used to hold spheres. 
/// </summary>
/// <typeparam name="T"></typeparam>
public class SphereBins<T> where T : System.IEquatable<T>
{
    SimpleList<SphereTuple<T>>[,,] bins;

    double b_inv_x;
    double b_inv_y;
    double b_inv_z;
    double bin_len;
    int xlen;
    int ylen;
    int zlen;

    public int id { get; private set; }

    public BBox Bounds { get; private set; }

    Pool<SimpleList<SphereTuple<T>>> _pool;

    /// <summary>
    /// Create a new sphere bins
    /// </summary>
    /// <param name="x"> num bins x</param>
    /// <param name="y"> num bins y</param>
    /// <param name="z"> num bins z</param>
    /// <param name="bin_len"> The length of a single cubic bin </param>
    /// <param name="pool"> pool to use for bin allocation. If null, then this object will create one</param>
    /// <param name="OnBinEmpty"> Optional call back for when the bin becomes empty </param>
    /// <param name="OnBinNotEmpty"> Optional call back for when the bin becomes not empty </param>
    /// <param name="id"> id to assign to this bin object </param>
    public SphereBins(
        int x, 
        int y, 
        int z,
        double bin_len,
        Action<int, Vector3Int, Vector3d> OnBinEmpty = null, 
        Action<int, Vector3Int, Vector3d> OnBinNotEmpty = null,
        int id = 0)
    {
        if (x <= 0 || y <= 0 || z <= 0)
            throw new System.Exception("Error, invalid input. Sphere Bins count.");

        this.id = id;
        this.bins = new SimpleList<SphereTuple<T>>[x, y, z];
        this.b_inv_x = 1.0 / (x * bin_len);
        this.b_inv_y = 1.0 / (y * bin_len);
        this.b_inv_z = 1.0 / (z * bin_len);
        this.bin_len = bin_len;
        this.xlen = x;
        this.ylen = y;
        this.zlen = z;
        this._pool = new Pool<SimpleList<SphereTuple<T>>>();
        this.Bounds = new BBox(Vector3d.zero, new Vector3d(bin_len*x, bin_len*y, bin_len*z));
    }

    /// <summary>
    /// Add an item to the SphereBins and returns an ID.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool Add(in Sphere s, in T data)
    {
        var min = range_min(s.Position, s.Radius);
        var max = range_max(s.Position, s.Radius);

        bool added = false;
        for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
                for (int z = min.z; z <= max.z; z++)
                {
                    if (ReferenceEquals(bins[x, y, z], null)) // If the bin doesn't exist... then add it
                        bins[x, y, z] = _pool.Get();

                    bins[x, y, z].Add(new SphereTuple<T>(s, data));
                    added = true;
                }
        return added;
    }

    internal Vector3Int range_min(in Vector3d pos, in double rad = 0)
    {
        int xmin = (int)((pos.x - rad) * b_inv_x * xlen);
        var ymin = (int)((pos.y - rad) * b_inv_y * ylen);
        var zmin = (int)((pos.z - rad) * b_inv_z * zlen);
        xmin = xmin < 0 ? 0 : (xmin >= xlen ? xlen - 1 : xmin);
        ymin = ymin < 0 ? 0 : (ymin >= ylen ? ylen - 1 : ymin);
        zmin = zmin < 0 ? 0 : (zmin >= zlen ? zlen - 1 : zmin);
        return new Vector3Int(xmin, ymin, zmin);
    }
    internal Vector3Int range_max(in Vector3d pos, in double rad = 0)
    {
        int xmax = (int)((pos.x + rad) * b_inv_x * xlen);
        var ymax = (int)((pos.y + rad) * b_inv_y * ylen);
        var zmax = (int)((pos.z + rad) * b_inv_z * zlen);
        xmax = xmax >= xlen ? xlen - 1 : (xmax < 0 ? 0 : xmax);
        ymax = ymax >= ylen ? ylen - 1 : (ymax < 0 ? 0 : ymax);
        zmax = zmax >= zlen ? zlen - 1 : (zmax < 0 ? 0 : zmax);
        return new Vector3Int(xmax, ymax, zmax);
    }
    internal BBox bounds(int x, int y, int z)
    {
        var bin_sizes = new Vector3d(Bounds.Size.x / xlen, Bounds.Size.y / ylen, Bounds.Size.z / zlen);
        return new BBox(new Vector3d(x * bin_sizes.x, y * bin_sizes.y, z * bin_sizes.z), bin_sizes);
    }

    /// <summary>
    /// Remove an item from the pool
    /// </summary>
    /// <param name="id"></param>
    public bool Remove(in Sphere s, in T data)
    {
        bool removed = false;
        var min = range_min(s.Position, s.Radius + 0.1);
        var max = range_max(s.Position, s.Radius + 0.1);
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                for (int z = min.z; z <= max.z; z++)
                {
                    if (!ReferenceEquals(null, bins[x, y, z]))
                    {
                        removed = bins[x, y, z].RemoveSwap(new SphereTuple<T>(s, data)) || removed;

                        if (bins[x,y,z].Count < 1) // free the list if it is no longer used
                        {
                            _pool.Add(bins[x, y, z]);
                            bins[x, y, z] = null;
                        }
                    }
                }
            }
        }
        return removed;
    }

    /// <summary>
    /// Update data to have new sphere
    /// </summary>
    /// <param name="s_old"> old sphere this data was added with </param>
    /// <param name="s_new"> new sphere to associate with this data </param>
    /// <param name="data"> data </param>
    public void Update(in Sphere s_old, in Sphere s_new, in T data)
    {
        var min_old = range_min(s_old.Position, s_old.Radius);
        var max_old = range_max(s_old.Position, s_old.Radius);

        var min_new = range_min(s_new.Position, s_new.Radius);
        var max_new = range_max(s_new.Position, s_new.Radius);

        //if the sphere is in the same range of bins
        if (min_old.Equals(min_new) && max_old.Equals(max_new))
        {
            //otherwise, update the sphere value
            var min = min_old;
            var max = max_old;
            for (int x = min.x; x <= max.x; x++)
                for (int y = min.y; y <= max.y; y++)
                    for (int z = min.z; z <= max.z; z++)
                    {
                        if (ReferenceEquals(bins[x, y, z], null)) // If the bin doesn't exist... then add it
                            throw new System.Exception("Error, all bins should exist in update func");

                        if (!bins[x, y, z].RemoveSwap(new SphereTuple<T>(s_old, data)))
                            throw new System.Exception("Error failed to update item that should exist!");
                        bins[x, y, z].Add(new SphereTuple<T>(s_new, data));
                    }

            return;
        }

        if (!Remove(s_old, data))
            throw new System.Exception("Error, did not remove item!");
        if (!Add(s_new, data))
            throw new System.Exception("Error, did not add object to the map in sphere bins update.");
    }

    /// <summary>
    /// Get bin at position. Result may be null.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public SimpleList<SphereTuple<T>> GetBin(in int x, in int y, in int z)
    {
        return bins[x, y, z];
    }

    /// <summary>
    /// Intersects sphere?
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public bool Intersects(in Sphere s)
    {
        var min = range_min(s.Position, s.Radius);
        var max = range_max(s.Position, s.Radius);

        for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
                for (int z = min.z; z <= max.z; z++)
                {
                    if (!ReferenceEquals(null, bins[x,y,z]))
                    {
                        var list = bins[x, y, z];
                        for (int i = 0; i < list.Count; i++)
                            if (list.Array[i].sphere.IntersectsSphere(s))
                                return true;
                    }
                }
        return false;
    }

    /// <summary>
    /// Build a list of sphere intersections. The list will contain multiple duplicates
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public void Intersects(in List<T> intersects, in Sphere s)
    {
        var min = range_min(s.Position, s.Radius);
        var max = range_max(s.Position, s.Radius);

        for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
                for (int z = min.z; z <= max.z; z++)
                {
                    if (!ReferenceEquals(null, bins[x, y, z]))
                    {
                        var list = bins[x, y, z];
                        for (int i = 0; i < list.Count; i++)
                            if (list.Array[i].sphere.IntersectsSphere(s))
                                intersects.Add(list.Array[i].data);
                    }
                }
    }

    Vector3d bin_to_array_pos(in Vector3d pos)
    {
        return new Vector3d(pos.x * b_inv_x * xlen, pos.y * b_inv_y * ylen, pos.z * b_inv_z * zlen);
    }

    /// <summary>
    /// Narrowphase raycast function, Used for sphere bins raycast against objects whos spheres have intersected the ray.
    /// </summary>
    /// <typeparam name="K"> Object type </typeparam>
    /// <param name="origin"> Ray origin </param>
    /// <param name="dir"> Ray direction </param>
    /// <param name="length"> Ray length </param>
    /// <param name="obj"> Object that a raycast is being done against </param>
    /// <param name="result"> Raycast result. </param>
    /// <param name="sphere_bins_intersection_position"> Resulting narrow cast raycast intersection position that has a position relative to the sphere bins. </param>
    /// <returns></returns>
    public delegate bool NarrowPhaseCast<K>(Vector3d origin, Vector3d dir, double length, K obj, Physics.RayCast.Flags flags, out Physics.RayCast.RayCastResult<K> result, out Vector3 sphere_bins_intersection_position);

    /// <summary>
    /// build a list of raycast intersections.
    /// </summary>
    /// <param name="intersects"> list of intersections. May contain duplicates. </param>
    /// <param name="line_start"> ray cast start </param>
    /// <param name="dir"> normalized ray cast direction </param>
    /// <param name="length"> length of the ray </param>
    /// <param name="narrow_cast_func"> Ray cast narrow phase function to do further ray cast testing on sphere bin objects as they are processed. </param>
    public bool RayCast(List<Physics.RayCast.RayCastResult<T>> intersects, in Vector3d line_start, in Vector3d dir, in double length, SphereBinsRayCast s, in Physics.RayCast.Flags flags, in T avoid_self = default(T))
    {
        var bin_sizes = new Vector3d(Bounds.Size.x / xlen, Bounds.Size.y / ylen, Bounds.Size.z / zlen);

        s._flags = flags;
        s._bins = this.bins;
        s._line_begin = line_start;
        s._line_end = line_start + dir * length;
        s._intersects = intersects;
        s._dir = dir;
        s._length = length;
        s._avoid_self = avoid_self;
        s._array_cast.origin = bin_to_array_pos(line_start);
        s._array_cast.direction = (new Vector3d(dir.x / bin_sizes.x, dir.y / bin_sizes.y, dir.z / bin_sizes.z)).normalized; //Note* direction gets 'normalized' by the (x,y,z) bin lengths. The array cast impl assumes (1,1,1) array bin size
        s._array_cast.bounds = new BBoxInt32(Vector3Int.zero, new Vector3Int(xlen, ylen, zlen));
        s._array_cast.dist_sq = (s._array_cast.origin - bin_to_array_pos(s._line_end)).sqrMagnitude;
        ArrayRayCast.Result result;
        s._array_cast.Cast(out result);
        return intersects.Count > 0;
    }

    /// <summary>
    /// Get first sphere intersection from raycast
    /// </summary>
    /// <param name="line_start"> ray cast start </param>
    /// <param name="dir"> normalized ray cast direction </param>
    /// <param name="length"> length of the ray </param>
    /// <param name="narrow_cast_func"> Ray cast narrow phase function to do further ray cast testing on sphere bin objects as they are processed. </param>
    public bool RayCast(in Vector3d line_start, in Vector3d dir, in double length, out Physics.RayCast.RayCastResult<T> hit, SphereBinsRayCast s, in Physics.RayCast.Flags flags, in T avoid_self = default(T))
    {
        var bin_sizes = new Vector3d(Bounds.Size.x / xlen, Bounds.Size.y / ylen, Bounds.Size.z / zlen);

        s._flags = flags;
        s._bins = this.bins;
        s._line_begin = line_start;
        s._line_end = line_start + dir * length;
        s._dir = dir;
        s._length = length;
        s._avoid_self = avoid_self;
        s._array_cast.origin = bin_to_array_pos(line_start);
        s._array_cast.direction = (new Vector3d(dir.x / bin_sizes.x, dir.y / bin_sizes.y, dir.z / bin_sizes.z)).normalized; //Note* direction gets 'normalized' by the (x,y,z) bin lengths. The array cast impl assumes (1,1,1) array bin size
        s._array_cast.bounds = new BBoxInt32(Vector3Int.zero, new Vector3Int(xlen, ylen, zlen));
        s._array_cast.dist_sq = (s._array_cast.origin - bin_to_array_pos(s._line_end)).sqrMagnitude;
        ArrayRayCast.Result result;
        var ray_cast_hit = s._array_cast.Cast(out result);
        hit = s._eval_hit;
        return ray_cast_hit;
    }
    public class SphereBinsRayCast
    {
        public ArrayRayCast _array_cast; // array raycast object
        public SimpleList<SphereTuple<T>>[,,] _bins; // sphere bins ref
        public Vector3d _line_end; //raycast end
        public Vector3d _line_begin; //raycast begin
        public Vector3d _dir; // raycast direction
        public double _length; //max allowed ray cast length
        public List<Physics.RayCast.RayCastResult<T>> _intersects; // multi raycast result
        public Physics.RayCast.RayCastResult<T> _eval_hit; // Single raycast result
        public Dictionary<T, Physics.RayCast.RayCastResult<T>> _cached_results; // Cached raycast result so duplicate objects arent raycasted on multiple times
        public T _avoid_self; // Optional avoid object while raycasting (For example, the player doesn't want to raycast against themself)
        public Physics.RayCast.Flags _flags;

        public NarrowPhaseCast<T> _narrow_cast_func; // optional narrowphase cast (doing extra raycast against each object in the sphere bins)

        public SphereBinsRayCast(NarrowPhaseCast<T> narrow_cast_func = null)
        {
            _cached_results = new Dictionary<T, Physics.RayCast.RayCastResult<T>>();
            _narrow_cast_func = narrow_cast_func;
            _array_cast.collide_function = Collide;
        }

        bool Collide(Vector3Int pos)
        {
            if (ReferenceEquals(null, this._intersects))
            {
                return Eval(pos);
            }
            else
            {
                return Add(pos);
            }
        }

        bool Eval(in Vector3Int pos)
        { // Eval function. This will return true on the first hit object and assign '_eval_hit'
            if (ReferenceEquals(null, _bins[pos.x, pos.y, pos.z]))
                return false;
            var list = _bins[pos.x, pos.y, pos.z]; // Get the bin at the desired array position
            double min_dist_sq = double.MaxValue;
            Physics.RayCast.RayCastResult<T> temp_hit;
            Vector3 hit_pos;
            bool hit = false;
            for (int i = 0; i < list.Count; i++) // Go through all objects in the bin
                if (raycast_object(ref list.Array[i].data, ref list.Array[i].sphere, this._flags, out temp_hit, out hit_pos)) // Do a raycast against them
                {
                    var dist_sq = (_line_begin - hit_pos).sqrMagnitude;
                    if (dist_sq < min_dist_sq) // Select the closest raycast hit in the bin
                    {
                        _eval_hit = temp_hit;
                        hit = true;
                    }
                }
            return hit;
        }
        bool Add(in Vector3Int pos)
        { // Add function. This will just go through every bin the ray touches and add it to the list if it intersects
            if (ReferenceEquals(null, _bins[pos.x, pos.y, pos.z]))
                return false;
            var list = _bins[pos.x, pos.y, pos.z];
            Physics.RayCast.RayCastResult<T> temp_hit;
            Vector3 hit_pos;
            for (int i = 0; i < list.Count; i++) // Go through all objects in the bin
                if (raycast_object(ref list.Array[i].data, ref list.Array[i].sphere, this._flags, out temp_hit, out hit_pos)) // add them to the list if a raycast against them is successfull
                {
                    _intersects.Add(temp_hit);
                }
            return false;
        }
        bool raycast_object(ref T obj, ref Sphere s, Physics.RayCast.Flags flags, out Physics.RayCast.RayCastResult<T> result, out Vector3 intersection_position)
        {
            if (Geometry.LineSegmentPointDistanceSqrd(_line_begin, _line_end, s.Center) < s.Radius * s.Radius)
            {
                // If this is the casting object, then just return
                if (obj.Equals(_avoid_self))
                {
                    result = default(Physics.RayCast.RayCastResult<T>);
                    intersection_position = result.intersection;
                    return false;
                }

                // Check if we have already done this raycast
                if (_cached_results.TryGetValue(obj, out result))
                {
                    result = default(Physics.RayCast.RayCastResult<T>);
                    intersection_position = result.intersection;
                    return result.collided;
                }

                if (ReferenceEquals(null, _narrow_cast_func))
                { // If there is no narrow cast function, then we will collect raycast information about the bounding sphere
                    if (s.ContainsPoint(_line_begin)) // If the ray started in the sphere...
                    {
                        result.started_in_surface = true;
                        result.hit_object = obj;
                        result.collided = true;
                        result.intersection = (Vector3)_line_begin;
                        result.normal = default(Vector3);
                        intersection_position = result.intersection;
                        _cached_results.Add(obj, result);
                        return true;
                    }

                    Vector3d i1, i2;
                    if (Geometry.LineSphereIntersect(s, _line_begin, _line_end, out i1, out i2)) // Otherwise.. Get intersection points
                    {
                        result.intersection = (Vector3)((_line_begin - i1).sqrMagnitude < (_line_begin - i2).sqrMagnitude ? i1 : i2);
                        result.collided = true;
                        result.hit_object = obj;
                        result.normal = (result.intersection - (Vector3)s.Center).normalized;
                        result.started_in_surface = default(bool);
                        _cached_results.Add(obj, result);
                        intersection_position = result.intersection;
                        return true;
                    }
                    result = default(Physics.RayCast.RayCastResult<T>);
                    intersection_position = result.intersection;
                    _cached_results.Add(obj, result);
                    return false;
                }
                else
                { // if _narrow_cas_func != null, then just use the supplied narrow cast function
                    var collided = _narrow_cast_func(_line_begin, _dir, _length, obj, flags, out result, out intersection_position);
                    result.collided = collided;
                    _cached_results.Add(obj, result);
                    return result.collided;
                }
            }
            else
            {
                result = default(Physics.RayCast.RayCastResult<T>);
                intersection_position = result.intersection;
                return false;
            }
        }

        /// <summary>
        /// Reset sphere bins. Must be called after every raycast!
        /// </summary>
        public void Reset()
        {
            _eval_hit = default(Physics.RayCast.RayCastResult<T>);
            _intersects = null;
            _line_end = _line_begin = default(Vector3d);
            _bins = null;
            _dir = default(Vector3d);
            _length = default(double);
            _cached_results.Clear();
            _array_cast.Reset();
        }
    }

    public void Clear()
    {
        for (int i = 0; i < xlen; i++)
            for (int j = 0; j < ylen; j++)
                for (int k = 0; k < zlen; k++)
                {
                    if (!ReferenceEquals(null, bins[i, j, k]))
                    {
                        var bin = bins[i, j, k];
                        bins[i, j, k] = null;
                        bin.Clear();
                        _pool.Add(bin);
                    }
                }
    }

#if UNITY_EDITOR
    public void DebugDrawBins()
    {
        DebugDraw.DrawBox(Bounds, UnityEngine.Color.white);
        for (int x = 0; x < xlen; x++)
            for (int y = 0; y < ylen; y++)
                for (int z = 0; z < zlen; z++)
                    if (bins[x,y,z] != null)
                    {
                        DebugDraw.DrawBox(bounds(x,y,z), UnityEngine.Color.green);
                        foreach (var s in bins[x,y,z])
                        {
                            DebugDraw.DrawBox(s.sphere.Bounds, UnityEngine.Color.red);
                        }
                    }
    }
#endif
}

/// <summary>
/// Tuple of sphere and data type T
/// </summary>
/// <typeparam name="K"></typeparam>
public struct SphereTuple<K> : System.IEquatable<SphereTuple<K>> where K : System.IEquatable<K>
{
    /// <summary>
    /// SPhere for this data
    /// </summary>
    public Sphere sphere;
    /// <summary>
    /// data 
    /// </summary>
    public K data;

    public SphereTuple(in Sphere sphere, in K data)
    {
        this.sphere = sphere;
        this.data = data;
    }

    public bool Equals(SphereTuple<K> other)
    {
        return data.Equals(other.data);
    }
}

namespace Physics
{
    public static class RayCast
    {
        /// <summary>
        /// Data input parameters for raycasts
        /// </summary>
        public struct Flags
        {
            int _flags;
        }

        public struct RayCastResult<T>
        {
            /// <summary>
            /// closest point of intersection. (casting this value to an integer vector will yield the collided voxel index)
            /// </summary>
            public Vector3 intersection;
            /// <summary>
            /// Normal of the position that was hit.
            /// </summary>
            public Vector3 normal;
            /// <summary>
            /// Object that was hit
            /// </summary>
            public T hit_object;
            /// <summary>
            /// was there a collision
            /// </summary>
            public bool collided;
            /// <summary>
            /// Did the ray cast start inside a collideable surface?
            /// </summary>
            public bool started_in_surface;
        }
    }
}