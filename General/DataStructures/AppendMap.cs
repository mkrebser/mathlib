using System.Collections;
using System.Collections.Generic;

/// <summary>
/// An (key=int, value=T) 'map' with access to underlying data arrays. The user cannot decide which keys
/// to use for this map, they are automatically assigned when calling Add().
/// </summary>
/// <typeparam name="T"></typeparam>
public class AppendMap<T>
{
    public const int InvalidArrayIndex = -1;
    public const int InvalidMapIndex = InvalidArrayIndex;
    const int MinArraySize = 32;

    public struct array_item<K>
    {
        public K item;
        public int index { get; private set; }

        public array_item(K item, int index = InvalidArrayIndex) { this.index = index; this.item = item; }
        public array_item(ref K item, int index = InvalidArrayIndex) { this.index = index; this.item = item; }

        public bool valid () { return index != InvalidMapIndex; }
    }

    /// <summary>
    /// number of items in the array
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// array for this set. You should not modify this array. (If you do, you must preserve the array_item.index field, otherwise the AppendMap will become corrupted).
    /// This array only has valid items from 0....AppendMap.Count. The rest of the array(AppendMap.Count... Array.Length is not used)
    /// </summary>
    public array_item<T>[] Array;
    int[] Map;

    /// <summary>
    /// Get the AppendMap.Array index of a key
    /// </summary>
    /// <param name="map_index"></param>
    /// <returns></returns>
    public int ArrayIndex(int map_index) { return Map[map_index]; }

    int free_index_count;
    int[] free_indices;

    int pop_index()
    {
        if (free_index_count < 1)
            return InvalidMapIndex;

        var index = free_indices[free_index_count - 1];
        free_index_count--;
        return index;
    }
    void push_index(int index)
    {
        //resize if needed
        if (free_indices.Length <= free_index_count)
            increase_size_arr(ref free_indices, free_index_count * 2);

        free_indices[free_index_count] = index;
        free_index_count++;
    }

    public T this[int i]
    {
        get
        {
            return Array[Map[i]].item;
        }
        set
        {
            Array[Map[i]].item = value;
        }
    }

    /// <summary>
    /// All values in this map
    /// </summary>
    public IEnumerable<T> Data
    {
        get
        {
            for (int i = 0; i < Count; i++)
                yield return Array[i].item;
        }
    }

    public struct Pair<K>
    {
        public K item;
        public int key;
    }

    /// <summary>
    /// all value key pairs
    /// </summary>
    public IEnumerable<Pair<T>> Pairs
    {
        get
        {
            for (int i = 0; i < Count; i++)
            {
                var p = new Pair<T>();
                p.item = Array[i].item;
                p.key = Array[i].index;
                yield return p;
            }
        }
    }

    /// <summary>
    /// iterate through a collection of keys. It is safe to remove the items returned from this collection while iterating through it!
    /// | For example, | foreach(var key in map.KeysRemoveSafe) { map.Remove(key); } |
    /// </summary>
    public IEnumerable<int> KeysRemoveSafe
    {
        get
        {
            for (int i = 0; i < Map.Length; i++)
                if (Map[i] != InvalidMapIndex)
                    yield return i;
        }
    }

    /// <summary>
    /// create a new append map with an initial capacity (it won't have to reallocate internal data structures as often with large capacity)
    /// </summary>
    /// <param name="capacity"></param>
    public AppendMap(int capacity = 2)
    {
        Array = new array_item<T>[capacity];
        Map = new int[capacity];
        free_indices = new int[capacity];
        Count = 0;
        Clear(); //clear will fill all of the arrays with invalid values for safety reasons
    }

    /// <summary>
    /// Remove an item at the input map index, outputs that map index of an element that was internally moved (can be -1 for none modified)
    /// </summary>
    /// <param name="remove_index"></param>
    /// <param name="modified_index"></param>
    /// <returns></returns>
    public bool Remove(int remove_index, out int modified_index)
    {
        modified_index = InvalidMapIndex;

        //get array index 
        var remove_array_index = Map[remove_index];
        if (remove_array_index == InvalidArrayIndex)
        {
            return false;
        }

        //remove swap
        var swapped_map_index = remove_swap(remove_array_index);

        //if a swap occured
        if (swapped_map_index != InvalidMapIndex)
        {
            //replace old array index with new array index
            Map[swapped_map_index] = remove_array_index;
            modified_index = swapped_map_index;
        }

        //set map[remove_index] = invalid since nothing is stored at this map index anymore
        Map[remove_index] = InvalidArrayIndex;

        //free the index we just removed so that it can be reused for new items
        push_index(remove_index);

        //decrement the item count
        Count--;

        return true;
    }

    /// <summary>
    /// Remove the item at the input index and outputs it.
    /// </summary>
    /// <param name="index"></param>
    /// <returns> returns true if removed, false if not removed </returns>
    public bool Remove(int remove_index, out T removed)
    {
        removed = Map[remove_index] != InvalidArrayIndex ? Array[Map[remove_index]].item : default(T);
        return Remove(remove_index);
    }

    /// <summary>
    /// remove an item at the input map index, return true if successful
    /// </summary>
    /// <param name="remove_index"></param>
    /// <returns></returns>
    public bool Remove(int remove_index)
    {
        int index;
        return Remove(remove_index, out index);
    }

    /// <summary>
    /// Remove the item at the input index and move the item at the back of the array to the removed item index
    /// </summary>
    /// <param name="index"></param>
    /// <returns> returns map index of swapped item </returns>
    int remove_swap(int remove_index)
    {
        if (remove_index == Count - 1) //if removing the last item, set to invalid
        {
            Array[remove_index] = new array_item<T>(default(T), InvalidMapIndex); //set to invlid
            return InvalidMapIndex; //return invalid index (no swap occured)
        }
        else //otherwise swap
        {
            var map_index = Array[Count - 1].index;
            Array[remove_index] = Array[Count - 1]; //swap
            Array[Count - 1] = new array_item<T>(default(T), InvalidMapIndex); //set to invalid
            return map_index; //return map index
        }
    }

    /// <summary>
    /// Add a value and return its map index (key). This key is unique for all elements and must be used to remove the item from the map.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="pool"></param>
    /// <returns></returns>
    public int Add(T value)
    {
        return Add(ref value);
    }

    /// <summary>
    /// Add a value and return its map index (key). This key is unique for all elements and must be used to remove the item from the map.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="pool"></param>
    /// <returns></returns>
    public int Add(ref T value)
    {
        //try to reuse an index
        var map_index = pop_index();
        //if no reuseable index was found...
        if (map_index == InvalidMapIndex)
        {
            //choose the 'count' as the next index (appending)
            map_index = Count;

            //increase map size if necessary
            if (map_index >= Map.Length)
            {
                //map and array always the same length
                increase_size_arr(ref Map, Map.Length * 2, InvalidArrayIndex);
                increase_size_arr(ref Array, Array.Length * 2, new array_item<T>(default(T), InvalidMapIndex));
            }
        }

        //we always append to the array, so adding always uses array index 'count'
        Map[map_index] = Count;
        //set value
        Array[Count] = new array_item<T>(ref value, map_index);
        //increment count
        Count++;
        return map_index;
    }

    void increase_size_arr<K>(ref K[] arr, int new_size, K default_value = default(K))
    {
        if (new_size <= arr.Length)
            return;

        var new_arr = new K[new_size];
        System.Array.Copy(arr, new_arr, arr.Length);

        for (int i = arr.Length; i < new_arr.Length; i++)
            new_arr[i] = default_value;

        arr = new_arr;
    }

    void clear_arr<K>(K[] arr, K default_value = default(K))
    {
        if (arr != null)
        {
            for (int i = 0; i < arr.Length; i++)
                arr[i] = default_value;
        }
    }

    public void Clear()
    {
        clear_arr(Array, new array_item<T>(default(T), InvalidMapIndex));
        clear_arr(Map, InvalidArrayIndex);
        clear_arr(free_indices, InvalidMapIndex);
    }

    public bool Contains(int map_index) { return map_index > -1 && map_index < Map.Length && Map[map_index] != InvalidArrayIndex; }

    public bool TryGetValue(int map_index, out T value)
    {
        if (map_index > -1 && map_index < Map.Length && Map[map_index] != InvalidArrayIndex)
        {
            value = default(T);
            return false;
        }
        else
        {
            value = Array[Map[map_index]].item;
            return true;
        }
    }

#if UNITY_EDITOR
    public void test()
    {
        test1(new AppendMap<int>());
        test1(new AppendMap<int>(100000));
    }

    void test1(AppendMap<int> m)
    {
        for (int i = 0; i < 20; i++)
        {
            m.Add(i);
        }

        for (int i = 0; i < 20; i += 2)
            m.Remove(i);

        for (int i = 0; i < 20; i += 2)
            m.Add(i);

        foreach (var d in m.Pairs)
            Debug.Log(d.key + ", " + d.item);
    }
#endif
}
