using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#endif

public static class Assert
{
    public static void Condition(bool cond, string message="Failed Assertion.")
    {
        if (!cond)
            throw new System.Exception(message);
    }

    public static void Condition_UnityEditorOnly(bool cond, string message = "Failed Assertion.")
    {
#if UNITY_EDITOR
        if (!cond)
            throw new System.Exception(message);
#endif
    }
}