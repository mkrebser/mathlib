using System.Collections;
using System.Collections.Generic;

public static class ArrayOps
{
    /// <summary>
    /// Will add an item to an array at the desired index. If array is too small.. It will be resized to fit (x2)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <param name="index"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static T[] SetAtIndex<T>(this T[] arr, in int index, in T item, in bool do_default_fill = false, in T default_value = default(T))
    {
        if (index < 0)
            throw new System.Exception("Bad index for array set at index");

        var array = arr;

        if (index >= array.Length)
        {
            var new_arr = new T[(index + 1) * 2]; // make new array
            for (int i = 0; i < array.Length; i++) // copy old array
                new_arr[i] = array[i];

            if (do_default_fill) // If new array should be filled with some default value
            {
                for (int i = array.Length; i < new_arr.Length; i++)
                    new_arr[i] = default_value;
            }

            array = new_arr;
        }

        array[index] = item;
        return array;
    }
}
