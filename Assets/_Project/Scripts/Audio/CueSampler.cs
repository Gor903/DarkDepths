using UnityEngine;

namespace DarkDepths.Audio
{
    // Variation picked from plain 0..1 numbers, so the caller owns the randomness and the rules stay testable.
    public static class CueSampler
    {
        public static int PickIndex(int count, float pick)
        {
            if (count <= 0)
            {
                return -1;
            }

            return Mathf.Clamp(Mathf.FloorToInt(Mathf.Clamp01(pick) * count), 0, count - 1);
        }

        public static float PickBetween(float first, float second, float pick)
        {
            float low = Mathf.Min(first, second);
            float high = Mathf.Max(first, second);
            return Mathf.Lerp(low, high, Mathf.Clamp01(pick));
        }
    }
}
