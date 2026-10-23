using DarkDepths.Core;
using DarkDepths.Noise;
using UnityEngine;

namespace DarkDepths.World
{
    // A thing that falls or is knocked into something is heard, and how hard it hit decides how far.
    // A bounce or a slide along the floor is one noise, not a string of them.
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public sealed class ImpactNoise : MonoBehaviour
    {
        [SerializeField] private NoiseSound _sound = NoiseSound.ObjectFall;
        [Tooltip("Metres a second under which a touch is not an impact at all.")]
        [SerializeField, Min(0f)] private float _quietSpeed = 1f;
        [Tooltip("Metres a second at which the impact reaches the full distance in the table.")]
        [SerializeField, Min(0.1f)] private float _loudSpeed = 6f;
        [Tooltip("Seconds before the same object can be heard hitting something again.")]
        [SerializeField, Min(0f)] private float _cooldown = 0.25f;

        private INoiseService _noise;
        private float _nextEmit;

        // Resolved in Start: every service has registered itself by then.
        private void Start()
        {
            _noise = Services.Get<INoiseService>();
            if (_noise == null)
            {
                Debug.LogError("ImpactNoise found no noise service; this object falls unheard.", this);
                enabled = false;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_noise == null || Time.time < _nextEmit)
            {
                return;
            }

            float speed = collision.relativeVelocity.magnitude;
            if (speed <= _quietSpeed)
            {
                return;
            }

            _nextEmit = Time.time + _cooldown;

            // Where it struck, not where its pivot is: a long crate is heard at the end that landed.
            Vector3 position = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            _noise.Emit(_sound, position, NoiseType.ObjectImpact, gameObject,
                Mathf.InverseLerp(_quietSpeed, Mathf.Max(_loudSpeed, _quietSpeed + 0.1f), speed));
        }
    }
}
