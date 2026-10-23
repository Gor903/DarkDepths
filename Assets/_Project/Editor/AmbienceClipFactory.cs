using System;
using System.IO;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Synthesises the stand-in background loops of docs/04: the hum a room has and the vent it has to be told from.
    // Tones are cut to a whole number of cycles per loop and the noise bed is folded into its own head,
    // so the file repeats without a seam.
    public static class AmbienceClipFactory
    {
        private const int SampleRate = 22050;

        private const string RoomToneName = "Ambience_RoomTone";
        private const string VentName = "Ambience_Vent";

        // Low, slow and nearly featureless: the floor the other sounds stand on.
        private const float RoomToneSeconds = 6f;
        private const float RoomToneCutoff = 260f;
        private const float RoomToneLevel = 0.3f;
        private static readonly int[] RoomToneCycles = { 300, 450, 672 };

        // A narrow band of hiss with a slow swell, which is what makes a vent findable by ear.
        private const float VentSeconds = 4f;
        private const float VentLowCutoff = 2600f;
        private const float VentHighCutoff = 620f;
        private const float VentLevel = 0.4f;
        private const int VentSwellCycles = 2;
        private const float VentSwellDepth = 0.3f;

        private const float FadeSeconds = 0.5f;

        public static string CreateRoomTone(string folder)
        {
            float[] samples = Bed(RoomToneSeconds, RoomToneCutoff, 0f, Seed(1));
            int count = samples.Length;

            for (int i = 0; i < count; i++)
            {
                float phase = i / (float)count;
                float tone = 0f;
                for (int p = 0; p < RoomToneCycles.Length; p++)
                {
                    tone += Mathf.Sin(2f * Mathf.PI * RoomToneCycles[p] * phase) / (p + 1f);
                }

                samples[i] = samples[i] * 0.6f + tone * 0.4f;
            }

            WavFile.Normalize(samples, RoomToneLevel);
            return Write(RoomToneName, folder, samples);
        }

        public static string CreateVent(string folder)
        {
            float[] samples = Bed(VentSeconds, VentLowCutoff, VentHighCutoff, Seed(2));
            int count = samples.Length;

            for (int i = 0; i < count; i++)
            {
                float phase = i / (float)count;
                // A whole number of swells per loop, or the loop would step on its own level.
                float swell = 1f - VentSwellDepth * 0.5f * (1f - Mathf.Cos(2f * Mathf.PI * VentSwellCycles * phase));
                samples[i] *= swell;
            }

            WavFile.Normalize(samples, VentLevel);
            return Write(VentName, folder, samples);
        }

        // Filtered noise of the given length, with its tail crossfaded into its head so the loop has no seam.
        private static float[] Bed(float seconds, float lowCutoff, float highCutoff, uint seed)
        {
            int count = Mathf.CeilToInt(seconds * SampleRate);
            int fade = Mathf.Min(count / 2, Mathf.CeilToInt(FadeSeconds * SampleRate));
            var rendered = new float[count + fade];

            uint state = seed;
            float lowPass = 0f;
            float highPass = 0f;
            float lowCoefficient = Coefficient(lowCutoff);
            float highCoefficient = highCutoff > 0f ? Coefficient(highCutoff) : 0f;

            for (int i = 0; i < rendered.Length; i++)
            {
                lowPass += (WavFile.NextNoise(ref state) - lowPass) * lowCoefficient;
                float value = lowPass;
                if (highCoefficient > 0f)
                {
                    highPass += (value - highPass) * highCoefficient;
                    value -= highPass;
                }

                rendered[i] = value;
            }

            var samples = new float[count];
            Array.Copy(rendered, samples, count);

            for (int i = 0; i < fade; i++)
            {
                float t = i / (float)fade;
                samples[i] = samples[i] * t + rendered[count + i] * (1f - t);
            }

            return samples;
        }

        private static float Coefficient(float cutoff)
        {
            return Mathf.Clamp01(1f - Mathf.Exp(-2f * Mathf.PI * cutoff / SampleRate));
        }

        private static uint Seed(int index)
        {
            return (uint)(index * 83492791) | 1u;
        }

        private static string Write(string clipName, string folder, float[] samples)
        {
            Directory.CreateDirectory(folder);
            string path = folder + "/" + clipName + ".wav";
            File.WriteAllBytes(path, WavFile.Encode(samples, SampleRate));
            return path;
        }
    }
}
