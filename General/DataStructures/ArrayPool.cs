
/// <summary>
/// A pool of arrays of T. This class is thread safe in a blocking manor.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ArrayPool<T> 
{
    T[][][] pool; //pool arrays for arrays of sizes 2,4,8,16,32,etc...
    int[] pool_stack_counts; //the head of the stack is an empty array

    const int starting_pool_size = 32;

    /// <summary>
    /// Maximum number of items a pool can have. This number sums the array counts for each pool.
    /// A pool containing 10 arrays of length 10 would have have a total length of 100
    /// </summary>
    public int MaxPoolSize { get; private set; }

    object lock_object = new object();

    /// <summary>
    /// where max size is the total number of elements across all arrays in the pool
    /// </summary>
    /// <param name="max_size"></param>
    public ArrayPool(int max_size = 20000)
    {
        resize_pool(starting_pool_size);
        MaxPoolSize = max_size;
    }

    void resize_pool(int new_size)
    {
        var new_pool = new T[new_size][][];
        var new_counts = new int[new_size];
        for (int i = 0; i < new_size; i++)
            new_counts[i] = ReferenceEquals(pool_stack_counts, null) || this.pool_stack_counts.Length <= i ?
                -1 : pool_stack_counts[i];

        for (int i = 0; i < new_size; i++)
        {
            new_pool[i] = ReferenceEquals(null, this.pool) || this.pool.Length <= i ? null : this.pool[i];
        }

        this.pool = new_pool;
        this.pool_stack_counts = new_counts;
    }

    int get_pool_index(int bin_size_needed)
    {
        if (bin_size_needed < 0)
            throw new System.Exception("Error, value < 0");
        if (bin_size_needed == 0)
            return 0;
        return (int)System.Math.Ceiling(System.Math.Log(bin_size_needed, 2));
    }

    T[][] get_array_pool(int bin_size_needed)
    {
        int pool_i = get_pool_index(bin_size_needed);

        if (pool_i >= pool.Length)
            resize_pool(pool_i + 16);

        return pool[pool_i];
    }

    /// <summary>
    /// Get an array from this pool that will be atleast the input desired length.
    /// Note* All array returned by this function have a length that is a power of 2.
    /// </summary>
    /// <param name="min_array_length"></param>
    /// <returns></returns>
    public T[] get_array(int min_array_length)
    {
        int pool_i;
        T[] result;

        //get pooled array
        lock (lock_object)
        {
            result = _get_array(min_array_length, out pool_i);
        }

        //if null, then no existing arrays exist, so return a new one
        if (ReferenceEquals(result, null))
            return new T[1 << pool_i];
        //else return pooled array
        else
            return result;
    }

    T[] _get_array(int min_array_length, out int pool_i)
    {
        //get index
        pool_i = get_pool_index(min_array_length);
        //resize if needed
        if (pool_i >= pool.Length)
            resize_pool(pool_i + 16);
        //get local pool
        var local_pool = pool[pool_i];

        //if null, create a new pool and return a new array
        if (local_pool == null)
        {
            pool[pool_i] = new T[starting_pool_size][]; //add new pool
            pool_stack_counts[pool_i] = -1; //initialize stack to empty
            return null; //return new array (we return null witch signals a new array is needed)
        }
        else
        {
            var stack_count = pool_stack_counts[pool_i]; //get stack count
            if (stack_count == -1) //if less then zero, pool is empty
            {
                return null; //return new array (we return null witch signals a new array is needed)
            }
            else if (stack_count >= 0)
            {
                var arr = local_pool[stack_count]; //get free array
                pool_stack_counts[pool_i]--; //decrement stack
                return arr; //return the pooled array
            }
            else
                throw new System.Exception("Invalid stack count");
        }
    }

    T[][] increase_local_pool_size(int pool_i, T[][] old_local_pool)
    {
        //get array lengths for the local pool
        var data_size = 1 << pool_i;
        //calculate new local pool length
        var new_length = old_local_pool.Length * 2;
        //calculate the total length of this pool
        var new_item_count = new_length * data_size;
        //if it's too big, then reduce too max allowed size
        if (new_item_count > MaxPoolSize)
        {
            new_length = MaxPoolSize / data_size;
            new_length = new_length < 1 ? 1 : new_length; //always allow for atleast 1
        }

        //make a new local pool
        var new_pool = new T[new_length][];
        for (int i = 0; i < old_local_pool.Length; i++)
        {
            //assign the references to the new pool
            if (old_local_pool[i] != null)
                new_pool[i] = old_local_pool[i];
        }

        //overwrite the old pool
        pool[pool_i] = new_pool;
        //return the new pool
        return new_pool;
    }

    /// <summary>
    /// Clears and frees the input array to this pool. Must have a length that is a power of 2
    /// </summary>
    /// <param name="array"></param>
    public void free_array(T[] array)
    {
        //clear array
        if (array.Length < 100)
            for (int i = 0; i < array.Length; i++)
                array[i] = default(T);
        else
            System.Array.Clear(array, 0, array.Length);

        lock (lock_object)
        {
            _free_array(array);
        }
    }

    void _free_array(T[] array)
    {
        int pool_i = get_pool_index(array.Length);
        //verify this array is a power of 2
        if (array.Length != (1 << (pool_i)))
            throw new System.Exception("Expected array to be power of two.");

        if (pool_i >= pool.Length)
            resize_pool(pool_i + 16);
        //get local pool
        var local_pool = pool[pool_i];
        if (local_pool == null)
            pool[pool_i] = local_pool = new T[starting_pool_size][]; //add new pool

        //increment stack
        var new_stack_count = pool_stack_counts[pool_i] + 1;

        //if too big, then just return
        if (new_stack_count * array.Length >= MaxPoolSize)
            return;
        //otherwise set the new count
        else
            pool_stack_counts[pool_i] = new_stack_count;

        //if too many items on the stack, then increase pool size
        if (new_stack_count >= local_pool.Length)
            local_pool = increase_local_pool_size(pool_i, local_pool);

        //add array
        local_pool[new_stack_count] = array;
    }
}

