using DarkDepths.Core;
using UnityEngine;

namespace DarkDepths.Audio
{
    // A background sound that belongs to a place: a vent, a hum, a machine down the corridor.
    // It holds one voice for as long as it is enabled and takes it back if a louder sound steals it.
    [DisallowMultipleComponent]
    public sealed class AmbienceEmitter : MonoBehaviour
    {
        [SerializeField] private AudioCue _cue;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [Tooltip("Seconds between checks that the loop still has a voice; a full pool can take it away.")]
        [SerializeField, Min(0.1f)] private float _checkInterval = 2f;

        private IAudioService _audio;
        private AudioHandle _handle;
        private float _nextCheck;

        public bool IsPlaying => _audio != null && _audio.IsPlaying(_handle);

        // Resolved in Start: every service has registered itself by then.
        private void Start()
        {
            _audio = Services.Get<IAudioService>();
            if (_audio == null || _cue == null)
            {
                Debug.LogError("AmbienceEmitter has no cue or no audio service; the room stays silent.", this);
                enabled = false;
                return;
            }

            if (!_cue.Loop)
            {
                Debug.LogWarning("AmbienceEmitter plays a cue that does not loop; the background falls quiet.", this);
            }

            Claim();
        }

        private void OnEnable()
        {
            // Before Start there is no service to ask; the first claim happens there.
            if (_audio != null)
            {
                Claim();
            }
        }

        private void OnDisable()
        {
            if (_audio != null)
            {
                _audio.Stop(_handle);
                _handle = AudioHandle.None;
            }
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextCheck)
            {
                return;
            }

            if (_audio.IsPlaying(_handle))
            {
                _nextCheck = Time.unscaledTime + _checkInterval;
                return;
            }

            Claim();
        }

        private void Claim()
        {
            _handle = _audio.Play(_cue, transform.position, _volume);
            _nextCheck = Time.unscaledTime + _checkInterval;
        }
    }
}
