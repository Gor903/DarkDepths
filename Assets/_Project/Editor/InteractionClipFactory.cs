using System.IO;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Synthesises the stand-in sounds of touching the room: a door's two creaks, its latch, and taking an item.
    // A creak is a gliding tone chopped by stick-slip chatter; a latch and a pickup are a noise burst plus a ring.
    public static class InteractionClipFactory
    {
        public const int Variations = 2;

        private const int SampleRate = 22050;
        private const float AttackSeconds = 0.002f;
        private const float ReleaseSeconds = 0.012f;

        private readonly struct Creak
        {
            public readonly float Duration;
            public readonly float StartHz;
            public readonly float EndHz;
            public readonly float ChatterHz;
            public readonly float NoiseCutoff;
            public readonly float NoiseMix;
            public readonly float Level;

            public Creak(float duration, float startHz, float endHz, float chatterHz,
                float noiseCutoff, float noiseMix, float level)
            {
                Duration = duration;
                StartHz = startHz;
                EndHz = endHz;
                ChatterHz = chatterHz;
                NoiseCutoff = noiseCutoff;
                NoiseMix = noiseMix;
                Level = level;
            }
        }

        private readonly struct Impact
        {
            public readonly float Duration;
            public readonly float NoiseCutoff;
            public readonly float HighPass;
            public readonly float NoiseDecay;
            public readonly float ToneHz;
            public readonly float ToneDecay;
            public readonly float ToneMix;
            public readonly float Level;

            public Impact(float duration, float noiseCutoff, float highPass, float noiseDecay,
                float toneHz, float toneDecay, float toneMix, float level)
            {
                Duration = duration;
                NoiseCutoff = noiseCutoff;
                HighPass = highPass;
                NoiseDecay = noiseDecay;
                ToneHz = toneHz;
                ToneDecay = toneDecay;
                ToneMix = toneMix;
                Level = level;
            }
        }

        // Opening drags the leaf out of a frame it has stood in: lower, longer, slower chatter than closing.
        private static readonly Creak DoorOpen = new Creak(0.8f, 190f, 125f, 15f, 2200f, 0.25f, 0.55f);
        private static readonly Creak DoorClose = new Creak(0.55f, 135f, 185f, 21f, 2600f, 0.25f, 0.55f);

        private static readonly Impact DoorLatch = new Impact(0.18f, 6000f, 1200f, 0.004f, 95f, 0.05f, 0.5f, 0.7f);
        private static readonly Impact ItemTaken = new Impact(0.22f, 4200f, 900f, 0.04f, 330f, 0.03f, 0.35f, 0.6f);

        // Glass on concrete: a bright scatter over a short ring, loud enough to be worth walking to.
        private static readonly Impact BottleLanding = new Impact(0.42f, 9000f, 1800f, 0.03f, 2600f, 0.1f, 0.5f, 0.85f);

        public static string[] CreateDoorOpen(string folder)
        {
            return Render("Door_Open", folder, DoorOpen, 11);
        }

        public static string[] CreateDoorClose(string folder)
        {
            return Render("Door_Close", folder, DoorClose, 23);
        }

        public static string[] CreateDoorLatch(string folder)
        {
            return Render("Door_Latch", folder, DoorLatch, 37);
        }

        public static string[] CreatePickup(string folder)
        {
            return Render("Pickup_Take", folder, ItemTaken, 51);
        }

        public static string[] CreateBottleImpact(string folder)
        {
            return Render("Impact_Bottle", folder, BottleLanding, 67);
        }

        private static string[] Render(string clipName, string folder, in Creak recipe, int family)
        {
            Directory.CreateDirectory(folder);
            var paths = new string[Variations];

            for (int variation = 0; variation < Variations; variation++)
            {
                float tune = Tune(variation);
                paths[variation] = Path(clipName, folder, variation);
                File.WriteAllBytes(paths[variation],
                    WavFile.Encode(RenderCreak(recipe, tune, Seed(family, variation)), SampleRate));
            }

            return paths;
        }

        private static string[] Render(string clipName, string folder, in Impact recipe, int family)
        {
            Directory.CreateDirectory(folder);
            var paths = new string[Variations];

            for (int variation = 0; variation < Variations; variation++)
            {
                float tune = Tune(variation);
                paths[variation] = Path(clipName, folder, variation);
                File.WriteAllBytes(paths[variation],
                    WavFile.Encode(RenderImpact(recipe, tune, Seed(family, variation)), SampleRate));
            }

            return paths;
        }

        private static float[] RenderCreak(in Creak recipe, float tune, uint seed)
        {
            float duration = recipe.Duration * (2f - tune);
            int count = Mathf.CeilToInt(duration * SampleRate);
            var samples = new float[count];

            uint state = seed;
            float lowPass = 0f;
            float coefficient = Coefficient(recipe.NoiseCutoff);
            float startHz = recipe.StartHz * tune;
            float endHz = recipe.EndHz * tune;

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SampleRate;
                // Phase of a tone that glides from one frequency to the other over the whole clip.
                float phase = 2f * Mathf.PI * (startHz * t + (endHz - startHz) * t * t / (2f * duration));
                float tone = Mathf.Sin(phase);

                // Stick and slip: the leaf grips and lets go, which is what a creak is made of.
                float chatter = Mathf.Abs(Mathf.Sin(Mathf.PI * recipe.ChatterHz * tune * t));
                chatter = 0.3f + 0.7f * chatter * chatter * chatter;

                lowPass += (WavFile.NextNoise(ref state) - lowPass) * coefficient;

                float value = (tone * (1f - recipe.NoiseMix) + lowPass * recipe.NoiseMix) * chatter;
                samples[i] = value * Envelope(t, duration, 0.03f, 0.1f);
            }

            WavFile.Normalize(samples, recipe.Level);
            return samples;
        }

        private static float[] RenderImpact(in Impact recipe, float tune, uint seed)
        {
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

                lowPass += (WavFile.NextNoise(ref state) - lowPass) * lowCoefficient;
                float noise = lowPass;
                if (highCoefficient > 0f)
                {
                    highPass += (noise - highPass) * highCoefficient;
                    noise -= highPass;
                }

                float value = noise * Mathf.Exp(-t / recipe.NoiseDecay);
                value += Mathf.Sin(2f * Mathf.PI * recipe.ToneHz * tune * t) * recipe.ToneMix *
                         Mathf.Exp(-t / recipe.ToneDecay);

                samples[i] = value * Envelope(t, duration, AttackSeconds, ReleaseSeconds);
            }

            WavFile.Normalize(samples, recipe.Level);
            return samples;
        }

        // Without the ramps at both ends the clip clicks where it starts and where it stops.
        private static float Envelope(float t, float duration, float attack, float release)
        {
            float rise = Mathf.Min(1f, t / attack);
            float fall = Mathf.Min(1f, (duration - t) / release);
            return rise * Mathf.Max(0f, fall);
        }

        // Each variation is the same sound a little higher and shorter, so a door never repeats itself exactly.
        private static float Tune(int variation)
        {
            return 0.96f + 0.06f * variation;
        }

        private static string Path(string clipName, string folder, int variation)
        {
            return folder + "/" + clipName + "_" + (variation + 1).ToString("00") + ".wav";
        }

        private static float Coefficient(float cutoff)
        {
            return Mathf.Clamp01(1f - Mathf.Exp(-2f * Mathf.PI * cutoff / SampleRate));
        }

        private static uint Seed(int family, int variation)
        {
            return (uint)(family * 73856093 ^ (variation + 1) * 19349663) | 1u;
        }
    }
}
