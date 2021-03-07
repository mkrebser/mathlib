using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A pool that uses keys (ids) and read-only thread items to allow multi threading
/// </summary>
/// <typeparam name="T"></typeparam>
public class AsynchPool<T> where T : new()
{
    int max = 100;
    ConcurrentQueue<T> pool = new ConcurrentQueue<T>();
    System.Func<T> MakeNew;

    /// <summary>
    /// [get=ThreadSafe, set=NOT ThreadSafe] The maximum amount of objects allowed in the pool. Note that this is just an approximation, exact max size is not guarenteed.
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
                {
                    T val;
                    pool.TryDequeue(out val);
                }
        }
    }
    /// <summary>
    /// Current number of objects in the pool. Note that this is just an approximation, exact count is not guarenteed.
    /// </summary>
    public int Count
    {
        get
        {
            return pool == null ? 0 : pool.Count;
        }
    }

    public AsynchPool(int maxSize = int.MaxValue)
    {
        max = maxSize;
    }

    /// <summary>
    /// Get a new item from the pool. Uses default constructor.
    /// </summary>
    /// <returns></returns>
    public T Get()
    {
        T val;
        if (pool.TryDequeue(out val))
        {
            return val;
        }
        else
        {
            return new T();
        }
    }

    /// <summary>
    /// Add an item to the pool
    /// </summary>
    /// <param name="item"></param>
    public void Add(T item)
    {
        if (pool.Count < max) // Note* it is possible for the pool to go past max size- we dont really care though
            pool.Enqueue(item);
    }

    /// <summary>
    /// Clear the pool. Clearing the pool to empty is not guarenteed if other threads are adding to it concurrently.
    /// </summary>
    public void Clear()
    {
        T val;
        while (!pool.IsEmpty)
        {
            pool.TryDequeue(out val);
        }
    }
}


/// <summary>
/// A pool that uses keys (ids) and read-only thread items to allow multi threading. Same as AsynchPool but uses a delegate for objetc creation.
/// </summary>
/// <typeparam name="T"></typeparam>
public class AsynchPoolNoNew<T>
{
    int max = 100;
    ConcurrentQueue<T> pool = new ConcurrentQueue<T>();
    System.Func<T> MakeNew;

    /// <summary>
    /// [get=ThreadSafe, set=NOT ThreadSafe] The maximum amount of objects allowed in the pool. Note that this is just an approximation, exact max size is not guarenteed.
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
                {
                    T val;
                    pool.TryDequeue(out val);
                }
        }
    }
    /// <summary>
    /// Current number of objects in the pool. Note that this is just an approximation, exact count is not guarenteed.
    /// </summary>
    public int Count
    {
        get
        {
            return pool == null ? 0 : pool.Count;
        }
    }

    public AsynchPoolNoNew(System.Func<T> makeNew, int maxSize = int.MaxValue)
    {
        if (ReferenceEquals(null, makeNew))
            throw new System.Exception("Error, null make new function!");
        max = maxSize;
        MakeNew = makeNew;
    }

    /// <summary>
    /// Get a new item from the pool. Uses default constructor.
    /// </summary>
    /// <returns></returns>
    public T Get()
    {
        T val;
        if (pool.TryDequeue(out val))
        {
            return val;
        }
        else
        {
            return MakeNew();
        }
    }

    /// <summary>
    /// Add an item to the pool
    /// </summary>
    /// <param name="item"></param>
    public void Add(T item)
    {
        if (pool.Count < max) // Note* it is possible for the pool to go past max size- we dont really care though
            pool.Enqueue(item);
    }

    /// <summary>
    /// Clear the pool. Clearing the pool to empty is not guarenteed if other threads are adding to it concurrently.
    /// </summary>
    public void Clear()
    {
        T val;
        while (!pool.IsEmpty)
        {
            pool.TryDequeue(out val);
        }
    }
}