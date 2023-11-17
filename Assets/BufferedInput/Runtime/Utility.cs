/*
 * Author: Porter Squires
 */

using UnityEngine;

namespace BufferedInput
{
    public class Utility
    {
        public static bool WithinThreshold(float val, float threshold=0.01f)
        {
            return Mathf.Abs(val) <= threshold;
        }

        public static bool Approximately(float val1, float val2, float threshold=0.01f)
        {
            return Mathf.Abs(val2 - val1) <= threshold;
        }
    }
}