using System.Collections;
using System.Collections.Generic;

/// <summary>
/// simple queue implemented on an array with underlying array exposed
/// </summary>
public class ArrayQueue<T> : IEnumerable<T>
{
    /// <summary>
    /// array the queue is implemented on
    /// </summary>
    public T[] data;
    /// <summary>
    /// number of items in the queue
    /// </summary>
    public int Count { get; private set; }
    /// <summary>
    /// index of first item in the queue. Only valid if the queue is not empty
    /// </summary>
    public int Head { get; private set; }
    /// <summary>
    /// index of last item in the queue. Only valid if the queue is not empty
    /// </summary>
    public int Tail
    {
        get
        {
            var sum = Head + Count - 1;
            return sum >= data.Length ? sum - data.Length : sum;
        }
    }

    /// <summary>
    /// returns true if the queue is empty
    /// </summary>
    /// <returns></returns>
    public bool Empty() {return Count <= 0; } 

    public ArrayQueue(int capacity = 2)
    {
        data = new T[capacity];
        Count = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return DequeueIterator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.DequeueIterator;
    }

    /// <summary>
    /// Iterator that will dequeue the entire queue. It is safe to enqueue while using this iterator
    /// </summary>
    public IEnumerator<T> DequeueIterator
    {
        get
        {
            var iterator = new iterator<T>(this);
            T val;
            while (iterator.DequeueNext(out val))
            {
                yield return val;
            }
        }
    }

    /// <summary>
    /// Iterator that will dequeue the entire queue in reverse order (from back of the queue to front)
    /// It is not safe to enqueue or dequeue while using this iterator
    /// </summary>
    public IEnumerable<T> ReverseDequeueIterator
    {
        get
        {
            while (!Empty())
                yield return DequeueLast();
        }
    }

    /// <summary>
    /// Iterator that does not modify the queue. It is not safe to modify the queue while using this iterator
    /// </summary>
    public IEnumerable<T> Iterator
    {
        get
        {
            var max_len = Count;
            for (int count = 0, index = Head; count < max_len; count++, index++, index = index >= data.Length ? 0 : index)
            {
                yield return data[index];
            }
        }
    }

    void increase_size_arr(ref T[] arr, int new_size)
    {
        if (new_size <= arr.Length)
            return;

        var new_arr = new T[new_size];

        int index = 0;
        while (!Empty())
        {
            new_arr[index] = Dequeue();
            index++;
        }
        Head = 0;
        Count = index;
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

    /// <summary>
    /// Add something to the back of the queue
    /// </summary>
    /// <param name="item"></param>
    public void Enqueue(T item)
    {
        if (Count >= data.Length)
        {
            increase_size_arr(ref data, data.Length * 2);
        }

        var index = Tail;
        index++;

        if (index >= data.Length)
        {
            index = 0;
        }

        data[index] = item;
        Count++;
    }

    /// <summary>
    /// remove the front of the queue
    /// </summary>
    /// <returns></returns>
    public T Dequeue()
    {
        if (Count <= 0)
            throw new System.Exception("Error, queue is empty");
        var index = Head;
        Head++;
        if (Head >= data.Length)
        {
            Head = 0;
        }
        Count--;
        return data[index];
    }

    /// <summary>
    /// first element in queue
    /// </summary>
    /// <returns></returns>
    public T First()
    {
        if (Count <= 0)
            throw new System.Exception("Error, queue is empty");
        return data[Head];
    }

    /// <summary>
    /// last element in queue
    /// </summary>
    /// <returns></returns>
    public T Last()
    {
        if (Count <= 0)
            throw new System.Exception("Error, queue is empty");
        return data[Tail];
    }

    public void Clear()
    {
        clear_arr(data);
        Head = 0;
        Count = 0;
    }

    /// <summary>
    /// remove the item at the back of the queue
    /// </summary>
    /// <returns></returns>
    public T DequeueLast()
    {
        if (Count <= 0)
            throw new System.Exception("Queue is empty");
        var index = Tail;
        Count--;
        return data[index];
    }

    /// <summary>
    /// struct that will iterate 'N' times in the queue
    /// </summary>
    /// <typeparam name="K"></typeparam>
    public struct iterator<K>
    {
        public int count;
        public int max_count;
        public ArrayQueue<K> queue;

        public iterator(ArrayQueue<K> queue)
        {
            this.queue = queue;
            this.count = 0;
            this.max_count = this.queue.Count;
        }

        /// <summary>
        /// Returns true if the iterator was able to dequeue an item. False otherwise. Outputs the dequeued item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool DequeueNext(out K value)
        {
            //check if passed iteration limit
            if (count >= max_count)
            {
                value = default(K);
                return false;
            }

            //get value
            value = queue.Dequeue();
            //increment
            count++;
            //return success
            return true;
        }
    }

#if UNITY_EDITOR
    public void test()
    {
        test1(new ArrayQueue<int>());
        test1(new ArrayQueue<int>(1000000));
    }
    void test1(ArrayQueue<int> q)
    {
        for (int i = 0; i < 20; i++)
            q.Enqueue(i);

        for (int i = 0; i < 5; i++)
            q.Dequeue();

        for (int i = 0; i < 10; i++)
            q.Enqueue(i);

        for (int i = 0; i < 5; i++)
            q.Dequeue();

        while (!q.Empty())
            Debug.Log(q.Dequeue());
        Debug.Log("--------------------------------------");


        for (int i = 0; i < 10; i++)
            q.Enqueue(i);

        for (int i = 0; i < 10; i++)
            q.Dequeue();

        for (int i = 0; i < 10; i++)
            q.Enqueue(i);

        while (!q.Empty())
            Debug.Log(q.Dequeue());

        Debug.Log("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
    }
#endif
}
