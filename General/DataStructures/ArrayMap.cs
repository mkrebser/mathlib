using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Works like a dictionary but the keys are always integers that are mapped with an array (so make sure your integer keys aren't too big!- it will alloc memory to the largest integer key)
/// </summary>
/// <typeparam name="T"></typeparam>
public class ArrayMap<T> : IEnumerable<T>
{
    /// <summary>
    /// Exposed array for Densly packed data
    /// </summary>
    public T[] _data; // densely packed data
    private int[] _data_to_sparse_map; // mapping of indices from data->sparse keys
    private int[] _sparse_map; // sparsely packed keys

    private int _dense_count; // number of items in dense data array
    public int Count { get { return _dense_count; } }

    public const int kInvalidKey = -1;

    public ArrayMap(int capacity=2)
    {
        _data = new T[capacity];
        _sparse_map = new int[capacity];
        _data_to_sparse_map = new int[capacity];
        _sparse_map.Fill(_sparse_map.Length, kInvalidKey);
        _data_to_sparse_map.Fill(_data_to_sparse_map.Length, kInvalidKey);
    }

    /// <summary>
    /// Add a (key,value) pair. Doesn't overwrite.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    public void Add(in int key, in T data)
    {
        if (key < 0) throw new System.Exception("Invalid key!");
        if (key >= _sparse_map.Length || _sparse_map[key] == kInvalidKey) // Add new
        {
            _sparse_map = _sparse_map.SetAtIndex(key, _dense_count, do_default_fill: true, default_value: kInvalidKey);
            _data = _data.SetAtIndex(_dense_count, data);
            _data_to_sparse_map = _data_to_sparse_map.SetAtIndex(_dense_count, key, do_default_fill: true, default_value: kInvalidKey);
            _dense_count++;
        }
        else // Overwrite
        {
            throw new System.Exception("ArrayMap 'Add' does not overwrite existing data. Use ArrayMap.Set");
        }
    }

    /// <summary>
    /// Overwrite existing data
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    public void Set(in int key, in T data)
    {
        if (key < 0 || key >= _sparse_map.Length || _sparse_map[key] == kInvalidKey) throw new System.Exception("Non existing key! Cannot overwrite ArrayMap value.");
        _data[_sparse_map[key]] = data;
    }

    /// <summary>
    /// Add or overwrite
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    public void SetAdd(in int key, in T data)
    {
        if (key < 0) throw new System.Exception("Invalid key!");
        if (key >= _sparse_map.Length || _sparse_map[key] == kInvalidKey) // Add new
        {
            _sparse_map = _sparse_map.SetAtIndex(key, _dense_count, do_default_fill: true, default_value: kInvalidKey);
            _data = _data.SetAtIndex(_dense_count, data);
            _data_to_sparse_map = _data_to_sparse_map.SetAtIndex(_dense_count, key, do_default_fill: true, default_value: kInvalidKey);
            _dense_count++;
        }
        else // Overwrite
        {
            _data[_sparse_map[key]] = data;
        }
    }

    public bool Remove(in int key)
    {
        if (key < 0 || key >= _sparse_map.Length) throw new System.Exception("Invalid key!");
        if (_sparse_map[key] == kInvalidKey || _dense_count <= 0) return false;

        int last_dense_index = _dense_count - 1;

        int last_item_key = _data_to_sparse_map[last_dense_index];
        if (last_item_key == kInvalidKey) throw new System.Exception("Error, array map corrupted");
        if (last_item_key == key) // if swapping with self.. Then no swap is needed
        {
            _data[last_dense_index] = default(T); // reset data
            _data_to_sparse_map[last_dense_index] = kInvalidKey; // reset dense->sparse map
            _sparse_map[key] = kInvalidKey;  // reset key
            _dense_count--; // decrement count!
        }
        else // Otherwise perform a swap
        {
            int dense_index = _sparse_map[key]; // get dense index of item to be removed

            _data[dense_index] = _data[last_dense_index]; // perform swap
            _data[last_dense_index] = default(T);
            _data_to_sparse_map[dense_index] = last_item_key;
            _data_to_sparse_map[last_dense_index] = kInvalidKey;
            _sparse_map[key] = kInvalidKey;
            _sparse_map[last_item_key] = dense_index;

            _dense_count--; // decrement count
        }

        return true;
    }

    /// <summary>
    /// Tries to get value at specified index. Returns false if it doesn't exist.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool TryGetValue(in int key, out T data)
    {
        if (key < 0 || key >= _sparse_map.Length || _sparse_map[key] == kInvalidKey)
        {
            data = default(T);
            return false;
        }

        data = _data[_sparse_map[key]];
        return true;
    }

    public bool ContainsKey(in int key)
    {
        if (key < 0 || key >= _sparse_map.Length || _sparse_map[key] == kInvalidKey)
        {
            return false;
        }
        return true;
    }

    public void Clear()
    {
        for (int i = 0; i < this.Count; i++)
        {
            _sparse_map[_data_to_sparse_map[i]] = kInvalidKey; // set in-use sparse map keys to invalid
            _data_to_sparse_map[i] = kInvalidKey; // reset dense to sparse keys
        }
        for (int i = 0; i < this.Count; i++)
        {
            _data[i] = default(T);
        }
        _dense_count = 0;
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
                yield return _data[i];
        }
    }

    public int GetKey(in int data_index)
    {
        if (data_index < 0 || data_index >= Count)
            throw new System.Exception("Error, invalid index");
        return _data_to_sparse_map[data_index];
    }

    public T GetValue(in int key)
    {
        if (key < 0 || key >= _sparse_map.Length || _sparse_map[key] == kInvalidKey)
        {
            throw new System.Exception("Error, input array map key does not exist. Cannot return dense data index.");
        }
        return _data[_sparse_map[key]];
    }

    public ref T GetValueRef(in int key, in string exc_message = "Error, input array map key does not exist. Cannot return dense data index.")
    {
        if (key < 0 || key >= _sparse_map.Length || _sparse_map[key] == kInvalidKey)
        {
            throw new System.Exception(exc_message);
        }
        return ref _data[_sparse_map[key]];
    }

    public int GetValueIndex(in int key)
    {
        if (key < 0 || key >= _sparse_map.Length || _sparse_map[key] == kInvalidKey)
        {
            throw new System.Exception("Error, input array map key does not exist. Cannot return dense data index.");
        }
        return _sparse_map[key];
    }

#if UNITY_EDITOR
    public static void test()
    {
        var m = new ArrayMap<int>();
        var s = new Dictionary<int, int>();
        var len = 10000;
        for (int i = 0; i < len; i++)
        {
            s.Add(i, i);
            m.Add(i, i);
        }

        var r = new SimpleRandom(0);
        for (int i = 0; i < len; i+=3)
        {
            var ri = r.Next(0, len);

            if (r.NextSingleN() < 0)
            {
                s.Remove(ri);
                m.Remove(ri);
            }
            else
            {
                if (!m.ContainsKey(ri))
                {
                    s.Add(ri, ri);
                    m.Add(ri, ri);
                }
                else
                {
                    m.Set(ri, ri);
                    s[ri] = ri;
                }
            }
        }

        var m_l = m.ToList();
        m_l.Sort();
        var s_l = s.Values.ToList();
        s_l.Sort();

        if (!s_l.SequenceEqual(m_l))
            throw new System.Exception("Error, seq not equal");

        m.Clear();
        if (m._sparse_map.Any(x => x != kInvalidKey))
            throw new System.Exception("Error, failed to clear map");
    }
#endif
}
