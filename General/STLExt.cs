using System.Collections;
using System.Collections.Generic;

public static class ArrayExt
{
    public static void Fill<T>(this T[,,] arr, in int x, in int y, in int z, in T val)
    {
        for (int i = 0; i < x; i++)
            for (int j = 0; j < y; j++)
                for (int k = 0; k < z; k++)
                    arr[i, j, k] = val;
    }
    public static void Fill<T>(this T[,,] arr, in int x, in int y, in int z, in int xlen, in int ylen, in int zlen, in T val)
    {
        int xs = x + xlen; int ys = y + ylen; int zs = z + zlen;
        for (int i = x; i < xs; i++)
            for (int j = y; j < ys; j++)
                for (int k = z; k < zs; k++)
                    arr[i, j, k] = val;
    }
    public static void Fill<T>(this T[,] arr, in int x, in int y, in T val)
    {
        for (int i = 0; i < x; i++)
            for (int j = 0; j < y; j++)
                arr[i, j] = val;
    }
    public static void Fill<T>(this T[] arr, in int x, in T val)
    {
        for (int i = 0; i < x; i++)
                arr[i] = val;
    }
}
