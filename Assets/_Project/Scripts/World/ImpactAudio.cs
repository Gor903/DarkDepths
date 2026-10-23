using DarkDepths.Audio;
using DarkDepths.Core;
using UnityEngine;

namespace DarkDepths.World
{
    // What a landing sounds like to the player, next to what it sounds like to the building.
    // Same rules as the noise: a touch is not an impact, and a bounce is one sound, not a burst.
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public sealed class ImpactAudio : MonoBehaviour
    {
        [SerializeField] private AudioCue _cue;
        [Tooltip("Metres a second under which a touch is not an impact at all.")]
        [SerializeField, Min(0f)] private float _quietSpeed = 1f;
        [Tooltip("Metres a second at which the impact is played at full volume.")]
        [SerializeField, Min(0.1f)] private float _loudSpeed = 6f;
        [Tooltip("Seconds before the same object can be heard hitting something again.")]
        [SerializeField, Min(0f)] private float _cooldown = 0.25f;

        private IAudioService _audio;
        private float _nextPlay;

        // Resolved in Start: every service has registered itself by then.
        private void Start()
        {
            _audio = Services.Get<IAudioService>();
            if (_audio == null)
            {
                Debug.LogError("ImpactAudio found no audio service; this object lands silently.", this);
                enabled = false;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_audio == null || _cue == null || Time.time < _nextPlay)
            {
                return;
            }

            float speed = collision.relativeVelocity.magnitude;
            if (speed <= _quietSpeed)
            {
                return;
            }

            _nextPlay = Time.time + _cooldown;

            Vector3 position = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            _audio.Play(_cue, position,
                Mathf.InverseLerp(_quietSpeed, Mathf.Max(_loudSpeed, _quietSpeed + 0.1f), speed));
        }
    }
}
