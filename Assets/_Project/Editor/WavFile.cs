using System.IO;
using System.Text;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // The one place a synthesised clip becomes a file: 16-bit mono PCM, the format Unity imports without a plugin.
    public static class WavFile
    {
        public static void Normalize(float[] samples, float level)
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

        // Deterministic noise: the same seed must produce the same file on every run.
        public static float NextNoise(ref uint state)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state / (float)uint.MaxValue * 2f - 1f;
        }

        public static byte[] Encode(float[] samples, int sampleRate)
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
                writer.Write(sampleRate);
                writer.Write(sampleRate * channels * bitsPerSample / 8);
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
