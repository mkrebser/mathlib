using System.Collections;
using System.Collections.Generic;
using System;

public static class ZipExtension
{
    public struct ZipEntry<T1, T2>
    {
        public ZipEntry(int index, T1 value1, T2 value2)
        {
            Index = index;
            Value1 = value1;
            Value2 = value2;
        }

        public int Index { get; private set; }
        public T1 Value1 { get; private set; }
        public T2 Value2 { get; private set; }
    }

    public static IEnumerable<ZipEntry<T1, T2>> Zip<T1, T2>(
        this IEnumerable<T1> collection1, IEnumerable<T2> collection2)
    {
        if (collection1 == null)
            throw new ArgumentNullException("collection1 is null");
        if (collection2 == null)
            throw new ArgumentNullException("collection2 is null");

        int index = 0;
        using (IEnumerator<T1> enumerator1 = collection1.GetEnumerator())
        using (IEnumerator<T2> enumerator2 = collection2.GetEnumerator())
        {
            while (enumerator1.MoveNext() && enumerator2.MoveNext())
            {
                yield return new ZipEntry<T1, T2>(
                    index, enumerator1.Current, enumerator2.Current);
                index++;
            }
        }
    }
}
