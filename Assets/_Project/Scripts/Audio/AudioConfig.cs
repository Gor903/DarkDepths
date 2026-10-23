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

        public AudioMixer Mixer => _mixer;

        public AudioMixerGroup MasterGroup => _masterGroup;

        public int VoiceCount => _voiceCount;

        public AudioRolloffMode Rolloff => _rolloff;

        public float DopplerLevel => _dopplerLevel;

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
