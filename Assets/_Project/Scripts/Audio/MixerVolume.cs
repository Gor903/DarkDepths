using UnityEngine;

namespace DarkDepths.Audio
{
    // Sliders are linear 0..1, the mixer works in decibels; silence is the bottom of the fader, not 0 dB.
    public static class MixerVolume
    {
        public const float SilenceDecibels = -80f;

        private const float Silent = 0.0001f;

        public static float ToDecibels(float linear)
        {
            if (linear <= Silent)
            {
                return SilenceDecibels;
            }

            return Mathf.Max(SilenceDecibels, Mathf.Log10(Mathf.Clamp01(linear)) * 20f);
        }

        public static float ToLinear(float decibels)
        {
            if (decibels <= SilenceDecibels)
            {
                return 0f;
            }

            return Mathf.Clamp01(Mathf.Pow(10f, decibels / 20f));
        }
    }
}
