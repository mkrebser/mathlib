using System.Collections.Generic;
using System;
using System.Linq;

public class OctreeNodeRef
{

}

public struct AABBPair<K> : IEquatable<AABBPair<K>> where K : IEquatable<K>
{
    public BBox _bbox;
    public K _object;
    public AABBPair(K obj, BBox b) { _object = obj; _bbox = b; }

    public bool Equals(AABBPair<K> o)
    {
        return _object.Equals(o._object);
    }
}

/// <summary>
/// Simple thread safe octree used for storing aabb
/// </summary>
/// <typeparam name="T"></typeparam>
public class Octree<T> where T : IEquatable<T>
{
    class OctreeNode<K> : OctreeNodeRef where K : IEquatable<K>
    {
        /// <summary>
        /// child nodes
        /// </summary>
        public OctreeNode<K>[] _nodes { get; private set; }
        /// <summary>
        /// objects in this node
        /// </summary>
        public SimpleList<AABBPair<K>> _objects { get; private set; }
        /// <summary>
        /// bounds of the node
        /// </summary>
        public BBoxInt32 _bounds { get; private set; }
        /// <summary>
        /// octree array size index
        /// </summary>
        public int _octree_index { get; private set; }
        /// <summary>
        /// parent node.
        /// </summary>
        public OctreeNode<K> _parent { get; private set; }
        /// <summary>
        /// Is this a leaf node?
        /// </summary>
        /// <returns></returns>
        public bool IsLeaf() { return _octree_index < 1; }
        /// <summary>
        /// number of immediate child nodes
        /// </summary>
        public int _child_node_count = 0;
        /// <summary>
        /// Clear the node
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < 8; i++)
                _nodes[i] = null;
            _objects.Clear();
            _bounds = default(BBoxInt32);
            _octree_index = 0;
            _parent = null;
            _child_node_count = 0;
        }
        public bool IsUsed()
        {
            return IsLeaf() ? _objects.Count > 0 : _child_node_count > 0;
        }

        public void Init(BBoxInt32 bbox, int index, OctreeNode<K> parent)
        {
            if (ReferenceEquals(null, _nodes))
            {
                _nodes = new OctreeNode<K>[8];
            }
            else
            {
                for (int i = 0; i < 8; i++)
                    if (!ReferenceEquals(null, _nodes[i]))
                        throw new System.Exception("Error, octree 'nodes' is already initialized!");
                if (_child_node_count != 0)
                    throw new System.Exception("Error, octree node init(), new node has children");
            }

            if (ReferenceEquals(null, _objects))
            {
                _objects = new SimpleList<AABBPair<K>>();
            }
            else
            {
                if (_objects.Count > 0)
                    throw new System.Exception("Error, octree 'objects' is already initialized!");
            }

            _bounds = bbox;
            _octree_index = index;
            _parent = parent;
        }
        static int count = 0;
        public int id;
        public OctreeNode()
        {
            _nodes = new OctreeNode<K>[8];
            _objects = new SimpleList<AABBPair<K>>();
            id = count++;
        }
    }

    int _min_exp;
    int _max_exp;
    OctreeNode<T> _root;
    BBoxInt32[,] _octree_child_aabbs; // aabbs sizes for each octree level. 
    AsynchPool<OctreeNode<T>> _pool;
    double _inv_smallest_bin_size;
    const double kBBoxExpansionRange = 0.01;

    /// <summary>
    /// Bounds of the Octree. All object bbox should fit inside here
    /// </summary>
    public BBox Bounds { get; private set; } // Note* that the bounds the user can get is slightly smaller than the real bounds
    /// <summary>
    /// Smallest allowed Octree bin size
    /// </summary>
    public Vector3Int MinBinSize {  get { return _octree_child_aabbs[exp_to_index(_min_exp + 1), 0].Size; } }

    /// <summary>
    /// Make a new octree of size (2^max_exponent) with minimum resolution of (2^min_exponent) (minimum resolution is box size of '1')
    /// </summary>
    /// <param name="max_exponent">inclusive max exponent [0-31]</param>
    /// <param name="min_exponent">inclusive min exponent [0-31]</param>
    public Octree(int max_exponent, int min_exponent)
    {
        if (max_exponent > 31 || max_exponent <= min_exponent || max_exponent < 0 || min_exponent < 0)
            throw new System.Exception("Bad exponents");
        _min_exp = min_exponent;
        _max_exp = max_exponent;
        _pool = new AsynchPool<OctreeNode<T>>();
        _root = _pool.Get();
        _root.Init(new BBoxInt32(Vector3Int.zero, new Vector3Int(1 << max_exponent)), max_exponent - min_exponent, null);
        _octree_child_aabbs = new BBoxInt32[max_exponent - min_exponent + 1, 8]; //note* that since this is half sizes, _octree_child_aabbs[0,N] will never be used
        Bounds = new BBox(_root._bounds.Position + new Vector3d(kBBoxExpansionRange), _root._bounds.Size - new Vector3d(kBBoxExpansionRange * 2));
        for (int exp = min_exponent; exp <= max_exponent; exp++)
        {
            BBoxInt32 bounds = new BBoxInt32(Vector3Int.zero, new Vector3Int(1 << exp));
            var half_size = bounds.Size / 2;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        var position = new Vector3Int(x * half_size.x, y * half_size.y, z * half_size.z);
                        _octree_child_aabbs[exp_to_index(exp), x * 2 * 2 + y * 2 + z] = new BBoxInt32(position, half_size);
                    }
                }
            }
        }
        _inv_smallest_bin_size = 1.0 / (1 << min_exponent);
    }

    /// <summary>
    /// Adds a new octree node, the object is pushed all the way down to the lowest level. The lowest parent node that contains all nodes with the object is returned.
    /// </summary>
    /// <param name="bbox"> bbox of the object </param>
    /// <param name="obj"> object to add </param>
    /// <returns> the lowest node that fully contains the object </returns>
    public OctreeNodeRef Add(BBox bbox, T obj)
    {
        if (bbox.Size.x < 0 || bbox.Size.y < 0 || bbox.Size.z < 0)
            throw new System.Exception("Invalid Octree bbox for add");
        if (!Bounds.Contains(bbox)) // Check against 'Bounds' rather than _root._bounds because no expansion occurs
        {
            throw new System.Exception("Error, bbox out of bounds!");
        }

        return push_down(ref bbox, ref obj, _root);
    }

    /// <summary>
    /// Removes an object from the tree
    /// </summary>
    /// <param name="bbox"> bbox of the object </param>
    /// <param name="obj"> the object to remove </param>
    /// <param name="root"> search starting point </param>
    /// <returns> Returns true if it was removed </returns>
    public bool Remove(BBox bbox, T obj, OctreeNodeRef root_ref = null)
    {
        if (bbox.Size.x < 0 || bbox.Size.y < 0 || bbox.Size.z < 0)
            throw new System.Exception("Invalid Octree bbox for remove");
        expand_bbox_for_remove(ref bbox); // expand bbox by small amount (see expand function for explanation)
        var root = (OctreeNode<T>)root_ref;
        if (!_root._bounds.Contains(bbox)) // check against _root_bounds instead of 'this.Bounds' because _root._bounds has not been shrunken like 'this.Bounds'
        {
            throw new System.Exception("Error, bbox out of bounds!");
        }
        OctreeNode<T> new_root;
        return remove_obj(ref bbox, ref obj, root, out new_root);
    }

    /// <summary>
    /// Update the bbox of an object in the tree
    /// </summary>
    /// <param name="bbox_new"> new bounding box </param>
    /// <param name="bbox_old"> old bounding box </param>
    /// <param name="obj"> the object that is being updated </param>
    /// <param name="root"> search starting point </param>
    /// <returns> Returns the new node the object was added to </returns>
    public OctreeNodeRef Update(BBox bbox_new, BBox bbox_old, T obj, OctreeNodeRef root_ref)
    {
        if (bbox_new.Size.x < 0 || bbox_new.Size.y < 0 || bbox_new.Size.z < 0 || bbox_old.Size.x < 0 || bbox_old.Size.y < 0 || bbox_old.Size.z < 0)
            throw new System.Exception("Invalid Octree bbox for update");
        var root = (OctreeNode<T>)root_ref;
        if (ReferenceEquals(null, root))
        {
            throw new System.Exception("Error, you must add a node before updating!");
        }

        if (to_tree_iter(bbox_new.Min).Equals(to_tree_iter(bbox_old.Min)) &&
            to_tree_iter(bbox_new.Max).Equals(to_tree_iter(bbox_old.Max))) // Check if the two bbox still cover the same octree leaf nodes
        {
            if (bbox_new.Equals(bbox_old, 0.0000001)) // If they are really close, then just dont do anything
            {
                return root;
            }
            update_bounds(ref bbox_old, ref bbox_new, ref obj, root); // If they do, then the tree doesnt have to be modified, so just swap bboxes in place
            return root;
        }
        else // otherwise, the old bbox has to be removed- then the new one needs to be added
        {
            expand_bbox_for_remove(ref bbox_old);
            if (!_root._bounds.Contains(bbox_new) || !_root._bounds.Contains(bbox_old))
            {
                throw new System.Exception("Error, bbox out of bounds!");
            }

            { // Remove from the tree
                OctreeNode<T> new_root;
                if (!remove_obj(ref bbox_old, ref obj, root, out new_root)) // remove the node
                    throw new System.Exception("Failed to remove node when updating, make sure to use the same object.");
                root = new_root; // re assign root
            }

            while (root != null) // iterate up through the node parents until a node that will fit the new bbox is found
            {
                if (root._bounds.Contains(bbox_new)) // if a parent node can fit the new bbox into it
                {
                    return push_down(ref bbox_new, ref obj, root); // add the bbox to the tree. Note* if the new bbox requires fewer nodes to be stored in than the old, 'push_down' will automatically handle this
                }
                root = root._parent;
            }
            throw new System.Exception("Internal Error, Couldn't find bbox to add to... Invalid tree");
        }
    }

    /// <summary>
    /// Perform intersections test on the octree
    /// </summary>
    /// <param name="bbox"> bbox to intersect the octree with </param>
    /// <param name="obj"> object to exclude intersection tests with </param>
    /// <param name="intersections"> set to add all intersected objects to </param>
    /// <param name="root_ref"> intersection search start node </param>
    /// <returns></returns>
    public bool Intersects(BBox bbox, T obj, HashSet<T> intersections, OctreeNodeRef root_ref = null)
    {
        if (bbox.Size.x < 0 || bbox.Size.y < 0 || bbox.Size.z < 0)
            throw new System.Exception("Invalid Octree bbox for bbox intersect");
        var root = (OctreeNode<T>)root_ref;
        return intersect(ref bbox, ref obj, ReferenceEquals(null, root) ? _root : root, intersections);
    }

    /// <summary>
    /// Perform a ray cast on the octree.
    /// </summary>
    /// <param name="line_start"> ray start point </param>
    /// <param name="line_end"> ray end point </param>
    /// <param name="intersections"> list of objects hit by the ray </param>
    /// <param name="root_ref"> ray cast search start node </param>
    /// <returns> true if any intersections </returns>
    public bool RayCast(Vector3d line_start, Vector3d line_end, HashSet<T> intersections, OctreeNodeRef root_ref = null)
    {
        var root = (OctreeNode<T>)root_ref;
        var ray = new Ray_ex(line_start, (line_end - line_start).normalized);
        double t0 = 0, t1 = (line_start - line_end).magnitude;
        var bbox = (BBox)root._bounds;
        if (!Geometry.linebboxintersect(ref bbox, ref ray, t0, t1))
        {
            return false;
        }
        return line_intersection(ref ray, t0, t1, root, intersections);
    }

    /// <summary>
    /// Delete every node from the tree. Returns (Number of Nodes Deleted, Number of object references removed) pair. NOTE* The root node is never deleted
    /// </summary>
    public ValueTuple<int, int> Clear()
    {
        var root_bounds = _root._bounds;
        var root_index = _root._octree_index;
        ValueTuple<int, int> result;
        result.Item1 = remove_nodes(_root, out result.Item2);
        _root.Init(root_bounds, root_index, null); // since 'remove_nodes' clears the root node, we need to reinitialize it
        return result;
    }

    /// <summary>
    /// Select objects in the tree using search predicates. No particular order is used. May have multuple duplicate entries.
    /// </summary>
    /// <param name="search_node_func"> Whether or not to search an octree child node given its BBox </param>
    /// <param name="select_obj_func"> Whether or not to select an object given the object and its BBox</param>
    /// <param name="list"> list to add objects to </param>
    /// <param name="root_ref"> Search start point </param>
    /// <returns> Returns every node that is evaluated to 'true' by the predicates </returns>
    public void Select(Func<BBoxInt32, bool> search_node_func, Func<AABBPair<T>, bool> select_obj_func, List<AABBPair<T>> list, OctreeNodeRef root_ref = null)
    {
        var root = (OctreeNode<T>)root_ref;
        select(root, search_node_func, select_obj_func, list);
    }

    /// <summary>
    /// Add all objects in the tree to the input list
    /// </summary>
    /// <param name="list"></param>
    public void GetAllObjects(List<AABBPair<T>> list)
    {
        select(_root, search_node_func_select_all, select_obj_func_select_all, list);
    }

    // Helper methods //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Push down octree nodes until the object is added to all leaf nodes that it can possibly intersect
    /// </summary>
    /// <param name="bbox"></param>
    /// <param name="obj"></param>
    /// <param name="root"></param>
    /// <returns></returns>
    OctreeNode<T> push_down(ref BBox bbox, ref T obj, OctreeNode<T> root)
    {
        OctreeNode<T> lowest_root = root;
        int intersect_count = 0;

        // If we are at the bottom of the octree...
        if (root.IsLeaf())
        {
            // Add the items to the list (since there cannot be any more children)
            root._objects.Add(new AABBPair<T>(obj, bbox));
            return root;
        }

        for (int i = 0; i < 8; i++)
        {
            if (get_node_child_aabb(root, i).Intersects(bbox))
            {
                if (ReferenceEquals(root._nodes[i], null)) // If the node is null... Then make a new one and initialize it
                {
                    root._nodes[i] = _pool.Get();
                    root._nodes[i].Init(get_node_child_aabb(root, i), root._octree_index - 1, root);
                    root._child_node_count++;
                }
                lowest_root = push_down(ref bbox, ref obj, root._nodes[i]); // invoke push down on intersected node...
                intersect_count++;
            }
        }

        if (intersect_count < 1)
            throw new System.Exception("Error, didn't add anything to the octree.");
        if (intersect_count > 1) // If more than 1 intersection occured, than any of the lower nodes cannot be the lowest root
            lowest_root = root;

        return lowest_root;
    }

    /// <summary>
    /// remove a node
    /// </summary>
    /// <param name="bbox"></param>
    /// <param name="obj"></param>
    /// <param name="root"></param>
    /// <param name="obj_count"></param>
    /// <returns></returns>
    bool remove_obj(ref BBox bbox, ref T obj, OctreeNode<T> root, out OctreeNode<T> new_root)
    {
        root = ReferenceEquals(null, root) ? _root : root;
        var result = remove_down(ref bbox, ref obj, root) > 0; // do remove function
        new_root = remove_up(root); // Remove nodes upwards incase the node we removed on is no longer empty...
        return result;
    }

    /// <summary>
    /// Remove the object from all leaf nodes it can possibly intersect
    /// </summary>
    /// <param name="bbox"> bounds of the object being removed </param>
    /// <param name="obj"> object being removed </param>
    /// <param name="root"> root node </param>
    /// <returns> true if removed </returns>
    int remove_down(ref BBox bbox, ref T obj, OctreeNode<T> root)
    {
        int removed_count = 0;
        if (root.IsLeaf()) // If this is a leaf
        {
            return root._objects.RemoveSwap(new AABBPair<T>(obj, bbox)) ? 1 : 0; // Remove the object, return '1' if removed, '0' otherwise
        }

        for (int i = 0; i < 8; i++) // Iterate through each child node
        {
            if (get_node_child_aabb(root, i).Intersects(bbox)) // See if there is an intersection
            {
                if (ReferenceEquals(null, root._nodes[i]))
                    continue; // Note* This is able to happen because we slightly expand the 'bounds'
                removed_count += remove_down(ref bbox, ref obj, root._nodes[i]);
            }
        }

        // try to remove child nodes
        for (int i = 0; i < 8; i++)
        {
            var node = root._nodes[i];
            if (!ReferenceEquals(null, node) && !node.IsUsed()) // if the node is not being used...
            {
                node.Clear();
                root._nodes[i] = null;
                _pool.Add(node);
                root._child_node_count--;
            }
        }

        return removed_count;
    }

    /// <summary>
    /// Remove root from its parent and keep going up so long as the parents have no nodes
    /// </summary>
    /// <param name="bbox"></param>
    /// <param name="obj"></param>
    /// <param name="root"></param>
    /// <returns></returns>
    OctreeNode<T> remove_up(OctreeNode<T> root)
    {
        if (ReferenceEquals(null, root._parent))
        {
            return root;
        }

        if (!root.IsUsed())
        {
            var parent = root._parent;
            bool removed = false;
            for (int i = 0; i < 8; i++)
            {
                if (ReferenceEquals(parent._nodes[i], root))
                {
                    root.Clear();
                    parent._nodes[i] = null;
                    parent._child_node_count--;
                    _pool.Add(root);
                    removed = true;
                }
            }
            if (!removed)
                throw new System.Exception("Error, couldnt remove node from parent, invalid tree");
            return remove_up(parent);
        }
        else
        {
            return root;
        }
    }

    /// <summary>
    /// Update the bounds of the object without adding/removing it from any nodes. NOTE* you must verify that the new and old bbox fit into exactly the same octree nodes before invoking this function
    /// </summary>
    /// <param name="bbox"></param>
    /// <param name="new_bbox"></param>
    /// <param name="obj"></param>
    /// <param name="root"></param>
    void update_bounds(ref BBox old_bbox, ref BBox new_bbox, ref T obj, OctreeNode<T> root)
    {
        if (root.IsLeaf())
        {
            int index;
            if (root._objects.TryFind<AABBPair<T>>(new AABBPair<T>(obj, old_bbox), out index))
            {
                root._objects.Array[index]._bbox = new_bbox; // update bbox
                return;
            }
            throw new System.Exception("Error, couldn't find object for bounds update!");
        }

        for (int i = 0; i < 8; i++) // Iterate through the child nodes
        {
            if (get_node_child_aabb(root, i).Intersects(old_bbox)) // See if there is an intersection
            {
                if (ReferenceEquals(null, root._nodes[i])) // If there was an intersection.. But the child was null, then the object cannot be removed due to an invalid tree
                {
                    throw new System.Exception("Error, invalid node found while updating bbox!");
                }
                update_bounds(ref old_bbox, ref new_bbox, ref obj, root._nodes[i]); //recurse on child node
            }
        }
    }

    /// <summary>
    /// Perform bbox intersection
    /// </summary>
    /// <param name="bbox"></param>
    /// <param name="obj"></param>
    /// <param name="root"></param>
    /// <param name="intersections"></param>
    /// <returns></returns>
    bool intersect(ref BBox bbox, ref T obj, OctreeNode<T> root, HashSet<T> intersections)
    {
        if (root.IsLeaf())
        {
            bool added = false;
            for (int i = 0; i < root._objects.Count; i++) // go through all of the objects in this node
            {
                if (root._objects.Array[i]._bbox.Intersects(bbox) && !root._objects.Array[i]._object.Equals(obj)) // if any intersect and are not the queried object...
                {
                    intersections.Add(root._objects.Array[i]._object); // Add them to the hashset (Note* a hashset is used to resolve objects being added multiple times)
                    added = true;
                }
            }
            return added;
        }

        bool result = false;
        for (int i = 0; i < 8; i++) // Iterate through the child nodes
        {
            if (get_node_child_aabb(root, i).Intersects(bbox)) // See if there is an intersection
            {
                if (ReferenceEquals(null, root._nodes[i])) // If there was an intersection.. But the child was null, then the tree is invalid
                {
                    throw new System.Exception("Error, invalid node found while doing bbox intersection bbox!");
                }
                result = intersect(ref bbox, ref obj, root._nodes[i], intersections) || result; //recurse on child node
            }
        }

        return result;
    }

    /// <summary>
    /// perform line intersection
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="intersections"></param>
    /// <returns></returns>
    bool line_intersection(ref Ray_ex ray, double t0, double t1, OctreeNode<T> root, HashSet<T> intersections)
    {
        if (root.IsLeaf())
        {
            bool added = false;
            for (int i = 0; i < root._objects.Count; i++) // go through all of the objects in this node
            {
                if (Geometry.linebboxintersect(ref root._objects.Array[i]._bbox, ref ray, t0, t1)) // if any intersect and are not the queried object...
                {
                    intersections.Add(root._objects.Array[i]._object); // Add them to the hashset (Note* a hashset is used to resolve objects being added multiple times)
                    added = true;
                }
            }
            return added;
        }

        bool result = false;
        for (int i = 0; i < 8; i++) // Iterate through the child nodes
        {
            var child_bbox = get_node_child_aabb(root, i);
            if (Geometry.linebboxintersect(ref child_bbox, ref ray, t0, t1)) // See if there is an intersection
            {
                if (ReferenceEquals(null, root._nodes[i])) // If there was an intersection.. But the child was null, then the tree is invalid
                {
                    throw new System.Exception("Error, invalid node found while doing bbox intersection bbox!");
                }
                result = line_intersection(ref ray, t0, t1, root._nodes[i], intersections) || result; //recurse on child node
            }
        }

        return result;
    }

    /// <summary>
    /// Removes all nodes below the input node. Returns the number of nodes that were removed. NOTE* that this will invalidate the tree (object counts) if it is not invoked on the root node
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    int remove_nodes(OctreeNode<T> root, out int objs_removed)
    {
        if (root.IsLeaf())
        {
            root.Clear();
            objs_removed = root._objects.Count;
            return 0; // leafs remove 0 child nodes
        }
        int removed = 0;
        objs_removed = 0;
        for (int i = 0; i < 8; i++) // Iterate through the child nodes
        {
            if (!ReferenceEquals(null, root._nodes[i]))
            {
                int child_objs_removed = 0;
                removed += remove_nodes(root._nodes[i], out child_objs_removed); // Remove sub-nodes of this child node
                objs_removed += child_objs_removed;

                var child_node = root._nodes[i]; // Remove the child node
                root._nodes[i] = null;
                _pool.Add(child_node); // add it to the pool
                removed++; // increment removed child count
            }
        }
        root.Clear(); // Clear this node
        return removed;
    }

    /// <summary>
    /// iterates through all nodes in the tree, using the input predicates. Adds the objects to a list
    /// </summary>
    void select(OctreeNode<T> root, Func<BBoxInt32, bool> search_node_func, Func<AABBPair<T>, bool> select_obj_func, List<AABBPair<T>> list)
    {
        if (root.IsLeaf())
        {
            for (int i = 0; i < root._objects.Count; i++) // go through all of the objects in this node
            {
                if (select_obj_func(root._objects.Array[i])) // if predicate is true...
                {
                    list.Add(root._objects.Array[i]); // Add to list
                }
            }
        }
        for (int i = 0; i < 8; i++) // Iterate through the child nodes
        {
            if (search_node_func(get_node_child_aabb(root, i))) // See if the predicate evaluates to true
            {
                if (!ReferenceEquals(null, root._nodes[i])) // If the node exists
                {
                    select(root._nodes[i], search_node_func, select_obj_func, list);
                }
            }
        }
    }

#if UNITY_EDITOR
    public void debug_draw(bool draw_all)
    {
        draw_update(_root, draw_all);
    }
    void draw_update(OctreeNode<T> root, bool draw_all)
    {
        if (draw_all)
            DebugDraw.DrawBox(root._bounds, UnityEngine.Color.white);

        if (root.IsLeaf())
        {
            DebugDraw.DrawBox(root._bounds, UnityEngine.Color.green);
            for (int i = 0; i < root._objects.Count; i++) // go through all of the objects in this node
            {
                DebugDraw.DrawBox(root._objects.Array[i]._bbox, UnityEngine.Color.red);
            }
        }
        for (int i = 0; i < 8; i++) // Iterate through the child nodes
        {
            if (root._nodes[i] != null) 
            {
                if (!ReferenceEquals(null, root._nodes[i])) // If the node exists
                {
                    draw_update(root._nodes[i], draw_all);
                }
            }
        }
    }
#endif

    int exp_to_index(int exp)
    {
        return exp - _min_exp;
    }

    /// <summary>
    /// convert a position to an hypothetical index if the octree was a 3d array at the lowest octree level
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    Vector3Int to_tree_iter(Vector3d v)
    {
        return new Vector3Int((int)(v.x * _inv_smallest_bin_size), (int)(v.y * _inv_smallest_bin_size), (int)(v.z * _inv_smallest_bin_size));
    }
    static bool search_node_func_select_all(BBoxInt32 b) { return true; }
    static bool select_obj_func_select_all(AABBPair<T> o) { return true; }
    BBoxInt32 get_node_child_aabb(OctreeNode<T> node, int index)
    {
        return new BBoxInt32(_octree_child_aabbs[node._octree_index, index].Position + node._bounds.Position, _octree_child_aabbs[node._octree_index, index].Size);
    }
    void expand_bbox_for_remove(ref BBox bbox)
    {
        // Expand the bbox search area by '0.01' in all direction to account for any floating point errors/rounding issues for searching through the bins
        const double e = kBBoxExpansionRange;
        const double e2 = kBBoxExpansionRange * 2;
        bbox.Position = new Vector3d(bbox.Position.x - e, bbox.Position.y - e, bbox.Position.z - e);
        bbox.Size = new Vector3d(bbox.Size.x + e2, bbox.Size.y + e2, bbox.Size.z + e2);
    }
}

#if UNITY_EDITOR
namespace OctreeTest
{
    public static class Test
    {
        public static void DoTest()
        {
            var r = new System.Random(0);
            {
                int n = 200;
                Vector3Int size = new Vector3Int(1 << 4);
                Octree<int> val = new Octree<int>(16, 2);
                do_test(val, size, n, r);
            }
            {
                int n = 50;
                Vector3Int size = new Vector3Int(1 << 2);
                Octree<int> val = new Octree<int>(8, 2);
                do_test(val, size, n, r);
            }
        }

        static void do_test(Octree<int> tree, Vector3d size_range, int n, Random r)
        {
            var bounds = tree.Bounds;

            {
                // Do add remove test
                List<int> values = new List<int>();
                List<BBox> aabbs = new List<BBox>();
                List<OctreeNodeRef> refs = new List<OctreeNodeRef>();
                for (int i = 0; i < n; i++)
                {
                    var bbox = rand_bbox(size_range, bounds, r);
                    refs.Add(tree.Add(bbox, i));
                    values.Add(i);
                    aabbs.Add(bbox);
                }

                for (int i = 0; i < n; i++)
                {
                    if (!tree.Remove(aabbs[i], values[i], r.Next() % 2 == 0 ? refs[i] : null))
                    {
                        throw new System.Exception("Test Failed, [2.5] failed to remove node");
                    }
                }

                var result = tree.Clear();
                if (result.Item2 != 0 || result.Item1 != 0)
                    throw new System.Exception("Test failed, [3] Tree failed to clear all nodes in add remove test.");
            }

            {
                // Get all objects test
                List<int> values = new List<int>();
                List<BBox> aabbs = new List<BBox>();
                List<OctreeNodeRef> refs = new List<OctreeNodeRef>();
                for (int i = 0; i < n; i++)
                {
                    var bbox = rand_bbox(size_range, bounds, r);
                    refs.Add(tree.Add(bbox, i));
                    values.Add(i);
                    aabbs.Add(bbox);
                }

                var obj_list = new List<AABBPair<int>>();
                tree.GetAllObjects(obj_list);
                var set = new HashSet<int>(obj_list.Select(x => x._object));
                foreach (var val in values)
                    set.Remove(val);

                if (set.Count > 0)
                    throw new System.Exception("Test failed, [4] Get All objects test failed");

                tree.Clear();
            }

            {
                // Update and intersection test
                List<int> values = new List<int>();
                List<BBox> aabbs = new List<BBox>();
                List<OctreeNodeRef> refs = new List<OctreeNodeRef>();
                var min_mov = tree.MinBinSize.ToVector3.magnitude * 0.25;

                // add nodes
                for (int i = 0; i < n; i++)
                {
                    var bbox = rand_bbox(size_range, bounds, r);
                    refs.Add(tree.Add(bbox, i));
                    values.Add(i);
                    aabbs.Add(bbox);
                }

                // Move the nodes around
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (r.Next() % 4 != 0)
                        { // Do a small move
                            var new_bbox = aabbs[i];
                            new_bbox.Position = new_bbox.Position + rand_dir(r) * min_mov;
                            new_bbox.Position = new_bbox.Position.Clamp(Vector3d.zero, new Vector3d(double.MaxValue));
                            new_bbox.Size = new_bbox.Size + new_bbox.Size * 0.1 * (r.Next() % 2 == 0 ? -1 : 1);
                            new_bbox.Size = new_bbox.Size.Clamp(new Vector3d(0.1), new Vector3d(double.MaxValue));
                            new_bbox = tree.Bounds.StrictFitBBox(new_bbox);
                            refs[i] = tree.Update(new_bbox, aabbs[i], values[i], refs[i]);
                            aabbs[i] = new_bbox;
                        }
                        else
                        { // Do a random move
                            var new_bbox = rand_bbox(size_range, bounds, r);
                            refs[i] = tree.Update(new_bbox, aabbs[i], values[i], refs[i]);
                            aabbs[i] = new_bbox;
                        }
                    } 
                }

                // do intersections for everyone
                HashSet<int> intersection_set = new HashSet<int>();
                List<int> manual_intersect = new List<int>();
                for (int i = 0; i < n; i++)
                {
                    tree.Intersects(aabbs[i], values[i], intersection_set, refs[i]);

                    for (int j = 0; j < n; j++)
                    {
                        if (i != j && aabbs[i].Intersects(aabbs[j]))
                        {
                            manual_intersect.Add(j);
                        }
                    }

                    foreach (var obj in manual_intersect)
                        if (!manual_intersect.Contains(obj))
                            throw new System.Exception("Test failed, intersection & update test failed to intersect " + aabbs[i] + ", " + aabbs[obj]);
                    if (manual_intersect.Count > 0)
                        throw new System.Exception("Test failed, intersection & update. Obtained wrong intersection for bbox " + aabbs[i]);

                    manual_intersect.Clear();
                    intersection_set.Clear();
                }

                tree.Clear();
            }
        }

        static System.ValueTuple<BBox, BBox> GenerateIntersectingBBox<T>(BBox bounds, System.Random r)
        {
            BBox bbox1;
            var pos_1 = new Vector3d(r.NextDouble() * bounds.Size.x + bounds.Position.x, r.NextDouble() * bounds.Size.y + bounds.Position.y, r.NextDouble() * bounds.Size.z + bounds.Position.z); // get a rand point inside bounds
            var range_1 = bounds.Max - pos_1;
            bbox1 = new BBox(pos_1, new Vector3d(r.NextDouble() * range_1.x, r.NextDouble() * range_1.y, r.NextDouble() * range_1.z)); // make a rand size that will fit the bbox into bounds

            var point_2 = new Vector3d(r.NextDouble() * bbox1.Size.x + bbox1.Position.x, r.NextDouble() * bbox1.Size.y + bbox1.Position.y, r.NextDouble() * bbox1.Size.z + bbox1.Position.z); // get a rand point inside bbox2
            var point2_2 = new Vector3d(r.NextDouble() * bounds.Size.x + bounds.Position.x, r.NextDouble() * bounds.Size.y + bounds.Position.y, r.NextDouble() * bounds.Size.z + bounds.Position.z); // get a rand point inside bounds

            var min_2 = new Vector3d(System.Math.Min(point_2.x, point2_2.x), System.Math.Min(point2_2.y, point2_2.y), System.Math.Min(point2_2.z, point2_2.z));
            var max_2 = new Vector3d(System.Math.Max(point_2.x, point2_2.x), System.Math.Max(point2_2.y, point2_2.y), System.Math.Max(point2_2.z, point2_2.z));
            var bbox2 = BBox.FromMinMax(min_2, max_2);

            if (!bbox1.Intersects(bbox2))
                throw new System.Exception("Error1, test is invalid " + bbox1 + ", " + bbox2);
            if (!bounds.Contains(bbox1) || !bounds.Contains(bbox2))
                throw new System.Exception("Error2, test is invalid" + bbox1 + ", " + bbox2);

            return new System.ValueTuple<BBox, BBox>(bbox1, bbox2);
        }

        static BBox rand_bbox(Vector3d max_size, BBox bounds, System.Random r)
        {
            var pos_1 = new Vector3d(r.NextDouble() * bounds.Size.x + bounds.Position.x, r.NextDouble() * bounds.Size.y + bounds.Position.y, r.NextDouble() * bounds.Size.z + bounds.Position.z); // get a rand point inside bounds
            var range_1 = new Vector3d(Math.Min(bounds.Max.x - pos_1.x, max_size.x), Math.Min(bounds.Max.y - pos_1.y, max_size.y), Math.Min(bounds.Max.z - pos_1.z, max_size.z));
            var bbox = new BBox(pos_1, new Vector3d(r.NextDouble() * range_1.x, r.NextDouble() * range_1.y, r.NextDouble() * range_1.z)); // make a rand size that will fit the bbox into bounds
            if (!bounds.Contains(bbox))
                throw new System.Exception("Error3, test is invalid" + bbox);
            return bbox;
        }

        static Vector3d rand_dir(Random r)
        {
            return new Vector3d((r.NextDouble() - 0.5) * 2, (r.NextDouble() - 0.5) * 2, (r.NextDouble() - 0.5) * 2).normalized;
        }
    }
}
#endif