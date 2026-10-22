using UnityEngine;

namespace DarkDepths.Core
{
    // How much of each effect the player's choice leaves; an effect scaled to nothing is switched off, not dimmed.
    public sealed class PostProcessBudget
    {
        // Below this an override costs its render pass and gives back nothing visible.
        private const float Visible = 0.001f;

        public PostProcessBudget(float vignette, float grain, float bloom, float reducedScale, bool reduced)
        {
            float scale = reduced ? Mathf.Clamp01(reducedScale) : 1f;

            VignetteIntensity = Mathf.Max(0f, vignette) * scale;
            GrainIntensity = Mathf.Max(0f, grain) * scale;
            BloomIntensity = Mathf.Max(0f, bloom) * scale;
        }

        public float VignetteIntensity { get; }
        public float GrainIntensity { get; }
        public float BloomIntensity { get; }

        public bool VignetteActive => VignetteIntensity > Visible;
        public bool GrainActive => GrainIntensity > Visible;
        public bool BloomActive => BloomIntensity > Visible;

        public bool AnyActive => VignetteActive || GrainActive || BloomActive;
    }
}
