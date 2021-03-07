using System.Collections.Generic;

public class Pool<T> where T : new()
{
    int max = 100;
    Queue<T> pool;
    /// <summary>
    /// The maximum amount of objects allowed in the pool
    /// </summary>
    public int MaxSize
    {
        get
        {
            return max;
        }
        set
        {
            max = value;
            if (pool == null) return;
            if (pool.Count > max)
                while (pool.Count > max)
                    pool.Dequeue();
        }
    }
    /// <summary>
    /// Current number of objects in the pool
    /// </summary>
    public int Count
    {
        get
        {
            return pool == null ? 0 : pool.Count;
        }
    }

	public Pool(int maxSize = int.MaxValue)
    {
        max = maxSize;
    }

    /// <summary>
    /// Get a new item from the pool. Uses default constructor
    /// </summary>
    /// <returns></returns>
    public T Get()
    {
        if (pool == null || pool.Count == 0)
        {
            return new T();
        }
        else
        {
            return pool.Dequeue();
        }
    }

    /// <summary>
    /// Add an item to the pool
    /// </summary>
    /// <param name="item"></param>
    public void Add(T item)
    {
        if (pool == null)
            pool = new Queue<T>();
        if (pool.Count < max)
            pool.Enqueue(item);
    }

    public void Clear()
    {
        pool.Clear();
    }
}
