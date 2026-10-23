using UnityEngine;

namespace DarkDepths.Audio
{
    // One draw from a cue: the clip that will play and the volume and pitch it plays at.
    public readonly struct AudioCueSample
    {
        public readonly AudioClip Clip;
        public readonly float Volume;
        public readonly float Pitch;

        public AudioCueSample(AudioClip clip, float volume, float pitch)
        {
            Clip = clip;
            Volume = volume;
            Pitch = pitch;
        }

        public bool HasClip => Clip != null;
    }
}
