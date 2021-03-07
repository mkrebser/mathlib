using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Helper methods for converting data types to char[] array without doing gc allocations
/// </summary>
public static class CharOps
{
    const string _int_min = "-2147483648";
    const string _big_float = "+Inf";
    const string _small_float = "-Inf";

    /// <summary>
    /// append a string
    /// </summary>
    /// <param name="arr"></param>
    /// <param name="pos"></param>
    /// <param name="s"></param>
    /// <returns></returns>
    public static int Append(this char[] arr, int pos, string s)
    {
        int n = 0;
        for (int i = pos; i < arr.Length && n < s.Length; i++, n++)
            arr[i] = s[n];
        return n;
    }

    /// <summary>
    /// Append a char array
    /// </summary>
    /// <param name="arr"></param>
    /// <param name="pos"></param>
    /// <param name="s"></param>
    /// <returns></returns>
    public static int Append(this char[] arr, int pos, char[] s)
    {
        int n = 0;
        for (int i = pos; i < arr.Length && n < s.Length; i++, n++)
            arr[i] = s[n];
        return n;
    }

    /// <summary>
    /// Append a single character
    /// </summary>
    /// <param name="arr"></param>
    /// <param name="pos"></param>
    /// <param name="s"></param>
    /// <returns></returns>
    public static int Append(this char[] arr, int pos, char s)
    {
        if (pos < arr.Length)
        {
            arr[pos] = s;
            return 1;
        }
        return 0;
    }

    /// <summary>
    /// Append an integer (ascii)
    /// </summary>
    /// <param name="arr"></param>
    /// <param name="pos"></param>
    /// <param name="s"></param>
    public static int Append(this char[] arr, int pos, int s)
    {
        if (s == 0)
        {
            return arr.Append(pos, (char)48);
        }
        else if (s == int.MinValue) // int min cannot be represented as a positive digit because of twos complement
        {
            return arr.Append(pos, _int_min);
        }

        int div = 1000000000; // 10 digits for int
        int index = pos;
        bool first = false;
        bool added_neg = false;
        int integer = s < 0 ? -s : s; // make positive value
        while (div > 0 && index < arr.Length)
        {
            var digit = integer / div;
            first = first || digit != 0;
            if (first)
            {
                if (!added_neg && s < 0)
                {
                    arr[index] = '-';
                    index++;
                    added_neg = true;

                    if (index >= arr.Length) // check if exceed arr length
                        return index - pos;
                }

                arr[index] = (char)(digit + 48); // 48 is ascii for '0'
                index++;
            }
            integer -= digit * div;
            div /= 10; // Divide div by 10
        }
        return index - pos; // return number of chars added
    }

    /// <summary>
    /// Append a floating point value. Note* numbers larger or smaller than 100,000,000 are not supported
    /// </summary>
    /// <param name="arr"></param>
    /// <param name="pos"></param>
    /// <param name="s"></param>
    /// <param name="decimal_places"></param>
    /// <returns></returns>
    public static int Append(this char[] arr, int pos, float s, int decimal_places = 2)
    { //Note*, this function does not work for float values greater than int max/ less than int min
        decimal_places = decimal_places > 6 ? 6 : decimal_places; // we just max out at 6

        if (s > 100000000)
        {
            return arr.Append(pos, _big_float);
        }
        else if (s < -100000000)
        {
            return arr.Append(pos, _small_float);
        }
        int wrote = 0;
        int int_part = (int)s;

        if (int_part == 0) // If int part is zero, we need to make sure to add the negtaive sign
        {
            wrote += arr.Append(pos, '-');
        }

        wrote += arr.Append(pos + wrote, int_part);

        if (decimal_places > 0)
        {
            float dec_part = System.Math.Abs(s - int_part);
            wrote += arr.Append(pos + wrote, '.');
            
            for (int i = 1; i <= decimal_places; i++)
            {
                dec_part *= 10;
                wrote += arr.Append(pos + wrote, (char)((int)dec_part + 48));
                dec_part = dec_part - (int)dec_part;
            }
        }

        return wrote;
    }

    public static void Clear(this char[] arr)
    {
        if (ReferenceEquals(null, arr))
            return;
        for (int i = 0; i < arr.Length; i++)
            arr[i] = '\0';
    }

#if UNITY_EDITOR
    public static void test()
    {
        { // int test
            char[] arr = new char[32];
            arr.Append(0, 123456789);
            Debug.Log(new string(arr));
            arr.Clear();

            arr.Append(0, -123456789);
            Debug.Log(new string(arr));
            arr.Clear();

            arr.Append(0, -1234567890);
            Debug.Log(new string(arr));
            arr.Clear();

            arr.Append(0, 0);
            Debug.Log(new string(arr));
            arr.Clear();

            arr.Append(0, int.MaxValue);
            Debug.Log(new string(arr));
            arr.Clear();

            arr.Append(0, int.MinValue);
            Debug.Log(new string(arr));
            arr.Clear();

            arr.Append(0, 2020202020);
            Debug.Log(new string(arr));
            arr.Clear();
        }

        { // float test
            char[] arr = new char[32];
            arr.Append(0, 123456789.234f);
            Debug.Log(new string(arr));
            arr.Clear();

            arr.Append(0, -99456123.234f);
            Debug.Log(new string(arr));
            arr.Clear();

            arr.Append(0, -1234561.2345235f);
            Debug.Log(new string(arr));
            arr.Clear();

            arr.Append(0, 0.0f);
            Debug.Log(new string(arr));
            arr.Clear();

            arr.Append(0, (float)int.MaxValue + 100000.0f);
            Debug.Log(new string(arr));
            arr.Clear();

            arr.Append(0, (float)int.MinValue - 100000.0f);
            Debug.Log(new string(arr));
            arr.Clear();

            arr.Append(0, 2020.02f);
            Debug.Log(new string(arr));
            arr.Clear();

            arr.Append(0, -0.234234f);
            Debug.Log(new string(arr));
            arr.Clear();

            arr.Append(0, 0.234234f);
            Debug.Log(new string(arr));
            arr.Clear();

            arr.Append(0, -0.234234f, 6);
            Debug.Log(new string(arr));
            arr.Clear();

            arr.Append(0, 0.234234f, 6);
            Debug.Log(new string(arr));
            arr.Clear();
        }
    }
#endif
}
