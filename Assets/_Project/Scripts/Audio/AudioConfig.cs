using UnityEngine;
using UnityEngine.Audio;

namespace DarkDepths.Audio
{
    // The mixer the service talks to and the size of the voice pool it keeps.
    [CreateAssetMenu(menuName = "DarkDepths/Audio Config", fileName = "AudioConfig")]
    public sealed class AudioConfig : ScriptableObject
    {
        [SerializeField] private AudioMixer _mixer;

        [Header("Groups")]
        [SerializeField] private AudioMixerGroup _masterGroup;
        [SerializeField] private AudioMixerGroup _sfxGroup;
        [SerializeField] private AudioMixerGroup _ambientGroup;
        [SerializeField] private AudioMixerGroup _musicGroup;
        [SerializeField] private AudioMixerGroup _uiGroup;

        [Header("Pool")]
        [Tooltip("Simultaneous voices; docs/07 budgets 24 on the target phone.")]
        [SerializeField, Range(4, 64)] private int _voiceCount = 24;

        [Header("3D playback")]
        [SerializeField] private AudioRolloffMode _rolloff = AudioRolloffMode.Logarithmic;
        [Tooltip("The player walks, nothing flies past: doppler only smears the pitch.")]
        [SerializeField, Range(0f, 1f)] private float _dopplerLevel;

        [Header("Occlusion")]
        [Tooltip("What counts as something to be heard through: walls and the leaf of a closed door.")]
        [SerializeField] private LayerMask _occlusionMask;
        [SerializeField, Range(1000f, 22000f)] private float _openCutoff = 22000f;
        [Tooltip("docs/04 puts a muffled sound between 800 and 1500 Hz.")]
        [SerializeField, Range(200f, 4000f)] private float _blockedCutoff = 1100f;
        [Tooltip("Seconds to go from clear to muffled; a wall passed at a run must not click.")]
        [SerializeField, Range(0.01f, 1f)] private float _occlusionSmoothTime = 0.2f;
        [Tooltip("Rays a frame; the rest of the voices keep the answer they had.")]
        [SerializeField, Range(1, 24)] private int _occlusionChecksPerFrame = 4;
        [Tooltip("Metres below which a sound is treated as being on top of the listener.")]
        [SerializeField, Min(0f)] private float _occlusionMinDistance = 1.2f;

        public AudioMixer Mixer => _mixer;

        public AudioMixerGroup MasterGroup => _masterGroup;

        public int VoiceCount => _voiceCount;

        public AudioRolloffMode Rolloff => _rolloff;

        public float DopplerLevel => _dopplerLevel;

        public LayerMask OcclusionMask => _occlusionMask;

        public float OpenCutoff => _openCutoff;

        public float BlockedCutoff => _blockedCutoff;

        public float OcclusionSmoothTime => _occlusionSmoothTime;

        public int OcclusionChecksPerFrame => _occlusionChecksPerFrame;

        public float OcclusionMinDistance => _occlusionMinDistance;

        public AudioMixerGroup GroupFor(AudioChannel channel)
        {
            switch (channel)
            {
                case AudioChannel.Ambient:
                    return _ambientGroup;
                case AudioChannel.Music:
                    return _musicGroup;
                case AudioChannel.Ui:
                    return _uiGroup;
                default:
                    return _sfxGroup;
            }
        }
    }
}
