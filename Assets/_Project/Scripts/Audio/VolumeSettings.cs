using DarkDepths.Core;
using UnityEngine;

namespace DarkDepths.Audio
{
    // Keeps the mixer at the volumes the player set. docs/04 gives sliders to SFX, Ambient and Music;
    // UI keeps the level it was mixed at, so a menu never goes silent with the game.
    [DisallowMultipleComponent]
    public sealed class VolumeSettings : MonoBehaviour
    {
        [SerializeField] private UserSettings _settings;

        private IAudioService _audio;

        // Resolved in Start: every service has registered itself by then.
        private void Start()
        {
            _audio = Services.Get<IAudioService>();
            if (_audio == null || _settings == null)
            {
                Debug.LogError("VolumeSettings has no settings or no audio service; the sliders will do nothing.", this);
                enabled = false;
                return;
            }

            _settings.Changed += Apply;
            Apply();
        }

        private void OnEnable()
        {
            // Before Start there is no service; the first apply happens there.
            if (_audio != null)
            {
                _settings.Changed += Apply;
                Apply();
            }
        }

        private void OnDisable()
        {
            if (_audio != null)
            {
                _settings.Changed -= Apply;
            }
        }

        private void Apply()
        {
            _audio.SetVolume(AudioChannel.Sfx, _settings.SfxVolume);
            _audio.SetVolume(AudioChannel.Ambient, _settings.AmbientVolume);
            _audio.SetVolume(AudioChannel.Music, _settings.MusicVolume);
        }
    }
}
