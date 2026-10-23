using System;
using UnityEngine;

namespace DarkDepths.Audio
{
    // One sound as the game refers to it: the clips to choose from and how it should be played.
    [CreateAssetMenu(menuName = "DarkDepths/Audio Cue", fileName = "AudioCue")]
    public sealed class AudioCue : ScriptableObject
    {
        [SerializeField] private AudioClip[] _clips = Array.Empty<AudioClip>();
        [SerializeField] private AudioChannel _channel = AudioChannel.Sfx;

        [Header("Variation")]
        [SerializeField, Range(0f, 1f)] private float _volumeMin = 0.9f;
        [SerializeField, Range(0f, 1f)] private float _volumeMax = 1f;
        [SerializeField, Range(0.5f, 2f)] private float _pitchMin = 0.95f;
        [SerializeField, Range(0.5f, 2f)] private float _pitchMax = 1.05f;

        [Header("Playback")]
        [SerializeField] private bool _loop;
        [Tooltip("As on AudioSource: 0 is the voice that survives, 256 the first one dropped when the pool is full.")]
        [SerializeField, Range(0, 256)] private int _priority = 128;

        [Header("Hearing")]
        [Tooltip("docs/04: a sound this strong is also shown at the edge of the screen, never heard alone.")]
        [SerializeField] private bool _hints;

        [Header("Space")]
        [Tooltip("1 is a sound in the room, 0 a sound in the player's head — UI and music belong at 0.")]
        [SerializeField, Range(0f, 1f)] private float _spatialBlend = 1f;
        [SerializeField] private float _minDistance = 1f;
        [SerializeField] private float _maxDistance = 25f;

        public AudioChannel Channel => _channel;

        public bool Loop => _loop;

        public int Priority => _priority;

        public bool Hints => _hints;

        public float SpatialBlend => _spatialBlend;

        public float MinDistance => _minDistance;

        public float MaxDistance => Mathf.Max(_minDistance, _maxDistance);

        public bool HasClips => _clips != null && _clips.Length > 0;

        public AudioCueSample Sample(float clipPick, float volumePick, float pitchPick)
        {
            int index = CueSampler.PickIndex(_clips == null ? 0 : _clips.Length, clipPick);
            if (index < 0)
            {
                return default;
            }

            return new AudioCueSample(
                _clips[index],
                CueSampler.PickBetween(_volumeMin, _volumeMax, volumePick),
                CueSampler.PickBetween(_pitchMin, _pitchMax, pitchPick));
        }
    }
}
