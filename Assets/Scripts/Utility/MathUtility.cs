using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathUtility
{
    public static bool Equivalent(float a, float b, float diffThreshold = 0.01f)
    {
        return Mathf.Abs(a - b) <= diffThreshold;
    }
}
