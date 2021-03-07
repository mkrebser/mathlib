using System.Collections;
using System.Collections.Generic;

public struct BitArray3D
{
    public BitArray _array;
    public Vector3Int _size;
    private int yz;

    public BitArray3D(in int x, in int y, in int z)
    {
        _array = new BitArray(x * y * z);
        _size = new Vector3Int(x, y, z);
        yz = y * z;
    }

    public bool Get(in int x, in int y, in int z)
    {
        return _array[x * (yz) + y * _size.z + z];
    }

    public void Set(in int x, in int y, in int z, in bool v)
    {
        _array[x * (yz) + y * _size.z + z] = v;
    }

#if UNITY_EDITOR
    public static void test()
    {
        var b1 = new BitArray3D(32, 27, 16);
        var b2 = new bool[32, 27, 16];

        var r1 = new System.Random(0);
        var r2 = new System.Random(0);

        for (int i = 0; i < 32; i++)
            for (int j = 0; j < 27; j++)
                for (int k = 0; k < 16; k++)
                {
                    b1.Set(i, j, k, r1.Next() % 2 == 0);
                    b2[i, j, k] = r2.Next() % 2 == 0;
                }

        for (int i = 0; i < 32; i++)
            for (int j = 0; j < 27; j++)
                for (int k = 0; k < 16; k++)
                {
                    if (b2[i, j, k] != b1.Get(i, j, k))
                        throw new System.Exception("BitArray3D Testing: Failed!");
                }
        Debug.Log("BitArray3D Testing: Passed!");
    }
#endif
}
