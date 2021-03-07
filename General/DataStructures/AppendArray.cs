using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Array that is easy to add/remove from the back (much faster than standard list for removal)
/// </summary>
/// <typeparam name="T"></typeparam>
public class AppendArray<T> : IEnumerable<T>
{
    public const int InvalidArrayIndex = -1;
    const int MinArraySize = 32;

    public struct array_item<K>
    {
        public K item;
        public bool valid;

        /// <summary>
        /// make a new item
        /// </summary>
        /// <param name="item"></param>
        public array_item(K item, bool valid = true) { this.valid = valid; this.item = item; }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return AllItems;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return AllItems;
    }

    public IEnumerator<T> AllItems
    {
        get
        {
            for (int i = 0; i < Count; i++)
                yield return Array[i].item;
        }
    }

    public IEnumerable<T> AllItemsReverse
    {
        get
        {
            for (int i = Count - 1; i > -1; i--)
                yield return Array[i].item;
        }
    }

    /// <summary>
    /// number of items in the array
    /// </summary>
    public int Count { get; private set; }
    /// <summary>
    /// array for this set
    /// </summary>
    public array_item<T>[] Array { get; private set; }

    public array_item<T> this[int i]
    {
        get
        {
            return Array[i];
        }
        set
        {
            Array[i] = value;
        }
    }

    public AppendArray(int capacity = 2)
    {
        Array = new array_item<T>[capacity];
        Count = 0;
    }

    public AppendArray(IEnumerable<T> collection, int capacity = 2)
    {
        Array = new array_item<T>[capacity < 2 ? 2 : capacity];
        Count = 0;
        foreach (var t in collection)
            Add(t);
    }

    /// <summary>
    /// Remove the item at the input index and move the item at the back of the array to the removed item index
    /// </summary>
    /// <returns> returns true if removed, false if not removed </returns>
    public bool RemoveSwap(int index)
    {
        T val;
        return RemoveSwap(index, out val);
    }

    /// <summary>
    /// Remove the item at the input index and move the item at the back of the array to the removed item index
    /// </summary>
    /// <param name="index"></param>
    /// <returns> returns true if removed, false if not removed </returns>
    public bool RemoveSwap(int index, out T removed)
    {
        removed = default(T);

        if (index < 0 || index >= Count)
            throw new System.Exception("Index Error");

        if (!Array[index].valid) //none remove, return invalid
            return false;

        removed = Array[index].item; //assign removed item

        if (index == Count - 1) //if removing the last item, set to invalid
        {
            Array[index] = default(array_item<T>); //set to invlid
            Count--; //decrement
            return true; //return invalid so no other value was modified
        }
        else //otherwise swap
        {
            Array[index] = Array[Count - 1]; //swap
            Array[Count - 1] = default(array_item<T>);  //set to invalid
            Count--; //decrement
            return true; //return index
        }
    }

    /// <summary>
    /// Remove the item at the input index and move the item at the back of the array to the removed item index
    /// </summary>
    /// <param name="index"></param>
    /// <returns> returns true if removed, false if not removed </returns>
    public bool RemoveSwapGet(int index, out T removed, out T swapped)
    {
        removed = default(T);
        swapped = default(T);

        if (index < 0 || index >= Count)
            throw new System.Exception("Index Error");

        if (!Array[index].valid) //none remove, return invalid
            return false;

        removed = Array[index].item; //assign removed item

        if (index == Count - 1) //if removing the last item, set to invalid
        {
            Array[index] = default(array_item<T>); //set to invlid
            Count--; //decrement
            return true; //return invalid so no other value was modified
        }
        else //otherwise swap
        {
            Array[index] = Array[Count - 1]; //swap
            swapped = Array[index].item;
            Array[Count - 1] = default(array_item<T>);  //set to invalid
            Count--; //decrement
            return true; //return index
        }
    }

    /// <summary>
    /// Add a value and return its index
    /// </summary>
    /// <param name="value"></param>
    /// <param name="pool"></param>
    /// <returns></returns>
    public int Add(T value)
    {
        //get new array if needed
        if (Array.Length <= Count)
        {
            var new_array = new array_item<T>[(Array.Length + 1) * 2];
            System.Array.Copy(Array, new_array, Array.Length);
            Array = new_array;
        }

        int index = Count;
        Array[index] = new array_item<T>(value);
        Count++;
        return index;
    }

    public void Clear()
    {
        if (Array != null)
        {
            for (int i = 0; i < Array.Length; i++)
                Array[i] = default(array_item<T>);
            Count = 0;
        }
    }

    public bool TryPop(out T value)
    {
        value= default(T);
        var index = Count - 1;
        if (index > -1)
        {
            value = Array[index].item;
            Array[index] = default(array_item<T>);
            Count--;
            return true;
        }
        else
            return false;
    }
}
