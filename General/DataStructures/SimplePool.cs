
/// <summary>
/// Simple blocking pool
/// </summary>
/// <typeparam name="T"></typeparam>
public class SimplePool<T>
{
    T[] _data;
    volatile int head = -1;
    object lock_obj = new object();
    public int Count { get { return _data.Length - head; } }

    public SimplePool(int capacity = 1024)
    {
        _data = new T[capacity];
    }

    public SimplePool(T[] initial_data)
    {
        _data = initial_data;
    }

    /// <summary>
    /// pull an item from the buffer
    /// </summary>
    /// <param name="new_object"> A function that returns a new object if the pool is empty </param>
    /// <returns></returns>
    public T pop(System.Func<T> new_object)
    {
        T val;
        lock (lock_obj)
        {
            //if stack is empty, then return default(T)
            if (head < 0)
                return new_object();
            val = _data[head];
            head--;
        }
        return val;
    }

    /// <summary>
    /// push an item to the buffer
    /// </summary>
    /// <param name="i"> the object to be freed</param>
    /// <param name="initialize"> function that will reset the input object </param>
    public void push(T i, System.Action<T> initialize)
    {
        initialize(i);

        lock (lock_obj)
        {
            //if stack is full
            if (head + 1 >= _data.Length)
            {
                //allocate mpre space
                var tmp = _data;
                _data = new T[_data.Length * 2];
                System.Array.Copy(tmp, _data, tmp.Length);
            }

            head++;
            _data[head] = i;
        }
    }
}
