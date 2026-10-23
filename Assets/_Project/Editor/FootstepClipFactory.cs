using System.IO;
using System.Text;
using DarkDepths.Noise;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Synthesises stand-in footstep clips so the surface system can be heard before recorded audio exists.
    // One filtered noise burst plus a short ring per step: enough to tell concrete from metal, nothing more.
    public static class FootstepClipFactory
    {
        public const int Variations = 3;

        private const int SampleRate = 22050;
        private const float AttackSeconds = 0.002f;
        private const float ReleaseSeconds = 0.01f;

        private readonly struct Recipe
        {
            public readonly float Duration;
            public readonly float NoiseCutoff;
            public readonly float NoiseDecay;
            public readonly float HighPass;
            public readonly float[] Partials;
            public readonly float PartialDecay;
            public readonly float PartialMix;
            public readonly float Level;

            public Recipe(float duration, float noiseCutoff, float noiseDecay, float highPass,
                float[] partials, float partialDecay, float partialMix, float level)
            {
                Duration = duration;
                NoiseCutoff = noiseCutoff;
                NoiseDecay = noiseDecay;
                HighPass = highPass;
                Partials = partials;
                PartialDecay = partialDecay;
                PartialMix = partialMix;
                Level = level;
            }
        }

        public static string[] Create(SurfaceType surface, string folder)
        {
            Directory.CreateDirectory(folder);

            Recipe recipe = RecipeFor(surface);
            var paths = new string[Variations];

            for (int variation = 0; variation < Variations; variation++)
            {
                // Each variation is the same step a little higher or lower, the way two boots never match.
                float tune = 0.94f + 0.07f * variation;
                float[] samples = Render(recipe, tune, Seed(surface, variation));

                paths[variation] = folder + "/Footstep_" + surface + "_" + (variation + 1).ToString("00") + ".wav";
                File.WriteAllBytes(paths[variation], Encode(samples));
            }

            return paths;
        }

        private static Recipe RecipeFor(SurfaceType surface)
        {
            switch (surface)
            {
                case SurfaceType.Carpet:
                    // Soft and dull: no ring at all, and the quietest of the set.
                    return new Recipe(0.2f, 700f, 0.045f, 0f, null, 0f, 0f, 0.5f);
                case SurfaceType.Wood:
                    return new Recipe(0.24f, 3000f, 0.03f, 0f, new[] { 220f, 450f }, 0.07f, 0.45f, 0.8f);
                case SurfaceType.Metal:
                    return new Recipe(0.38f, 5000f, 0.022f, 400f, new[] { 880f, 1730f, 2590f }, 0.16f, 0.5f, 0.85f);
                case SurfaceType.Glass:
                    return new Recipe(0.3f, 8000f, 0.016f, 1500f, new[] { 2400f, 3700f, 5100f }, 0.11f, 0.55f, 0.7f);
                default:
                    return new Recipe(0.16f, 2500f, 0.028f, 0f, new[] { 150f }, 0.045f, 0.35f, 0.8f);
            }
        }

        private static float[] Render(in Recipe recipe, float tune, uint seed)
        {
            // A higher-tuned step is also a shorter one, so the variations do not all end together.
            float duration = recipe.Duration * (2f - tune);
            int count = Mathf.CeilToInt(duration * SampleRate);
            var samples = new float[count];

            uint state = seed;
            float lowPass = 0f;
            float highPass = 0f;
            float lowCoefficient = Coefficient(recipe.NoiseCutoff * tune);
            float highCoefficient = recipe.HighPass > 0f ? Coefficient(recipe.HighPass * tune) : 0f;

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SampleRate;

                lowPass += (NextNoise(ref state) - lowPass) * lowCoefficient;
                float noise = lowPass;
                if (highCoefficient > 0f)
                {
                    highPass += (noise - highPass) * highCoefficient;
                    noise -= highPass;
                }

                float value = noise * Mathf.Exp(-t / recipe.NoiseDecay);

                if (recipe.Partials != null)
                {
                    float ring = 0f;
                    for (int p = 0; p < recipe.Partials.Length; p++)
                    {
                        ring += Mathf.Sin(2f * Mathf.PI * recipe.Partials[p] * tune * t) / (p + 1f);
                    }

                    value += ring * recipe.PartialMix * Mathf.Exp(-t / recipe.PartialDecay);
                }

                // Two milliseconds in and ten out: without them the clip clicks at both ends.
                float attack = Mathf.Min(1f, t / AttackSeconds);
                float release = Mathf.Min(1f, (duration - t) / ReleaseSeconds);
                samples[i] = value * attack * Mathf.Max(0f, release);
            }

            Normalize(samples, recipe.Level);
            return samples;
        }

        private static void Normalize(float[] samples, float level)
        {
            float peak = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                peak = Mathf.Max(peak, Mathf.Abs(samples[i]));
            }

            if (peak <= 0f)
            {
                return;
            }

            float scale = level / peak;
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scale;
            }
        }

        private static float Coefficient(float cutoff)
        {
            return Mathf.Clamp01(1f - Mathf.Exp(-2f * Mathf.PI * cutoff / SampleRate));
        }

        // Deterministic noise: the same surface and variation must produce the same file on every run.
        private static float NextNoise(ref uint state)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state / (float)uint.MaxValue * 2f - 1f;
        }

        private static uint Seed(SurfaceType surface, int variation)
        {
            return (uint)(((int)surface + 1) * 73856093 ^ (variation + 1) * 19349663) | 1u;
        }

        private static byte[] Encode(float[] samples)
        {
            const int bitsPerSample = 16;
            const int channels = 1;
            int dataSize = samples.Length * sizeof(short);

            using (var stream = new MemoryStream(44 + dataSize))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + dataSize);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));

                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)channels);
                writer.Write(SampleRate);
                writer.Write(SampleRate * channels * bitsPerSample / 8);
                writer.Write((short)(channels * bitsPerSample / 8));
                writer.Write((short)bitsPerSample);

                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(dataSize);
                for (int i = 0; i < samples.Length; i++)
                {
                    writer.Write((short)Mathf.Clamp(Mathf.RoundToInt(samples[i] * short.MaxValue),
                        short.MinValue, short.MaxValue));
                }

                writer.Flush();
                return stream.ToArray();
            }
        }
    }
}
