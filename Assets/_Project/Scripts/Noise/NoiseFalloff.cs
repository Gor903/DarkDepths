using UnityEngine;

namespace DarkDepths.Noise
{
    // The arithmetic of docs/04, kept free of MonoBehaviour: how far a sound reaches a given listener,
    // and how loud it is when it gets there. Balance cannot be tuned without this being testable.
    public static class NoiseFalloff
    {
        // Metres this listener can hear this sound over. A wall does not stop it, it halves its reach.
        public static float Radius(float loudness, float hearingMultiplier, float surfaceMultiplier,
            bool blocked, float wallMultiplier)
        {
            if (loudness <= 0f || hearingMultiplier <= 0f || surfaceMultiplier <= 0f)
            {
                return 0f;
            }

            float radius = loudness * hearingMultiplier * surfaceMultiplier;
            return blocked ? radius * Mathf.Max(0f, wallMultiplier) : radius;
        }

        // 1 at the source, falling to nothing at the edge of the radius, 0 outside it.
        public static float Perceived(float distance, float radius)
        {
            if (radius <= 0f || distance >= radius)
            {
                return 0f;
            }

            return 1f - Mathf.Max(0f, distance) / radius;
        }
    }
}
