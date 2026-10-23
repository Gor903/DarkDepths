using DarkDepths.Audio;
using DarkDepths.Core;
using DarkDepths.Noise;
using UnityEngine;

namespace DarkDepths.Player
{
    // Plays one sound per step, picked from what the player is walking on.
    [RequireComponent(typeof(CharacterController))]
    public sealed class FootstepPlayer : MonoBehaviour
    {
        [SerializeField] private FootstepConfig _config;
        [SerializeField] private FirstPersonController _controller;

        private CharacterController _body;
        private StrideMeter _meter;
        private IAudioService _audio;

        private void Awake()
        {
            if (_config == null || _controller == null)
            {
                Debug.LogError("FootstepPlayer is missing its config or controller; steps stay silent.", this);
                enabled = false;
                return;
            }

            _body = GetComponent<CharacterController>();
            _meter = new StrideMeter(_config.CrouchStride, _config.WalkStride, _config.SprintStride,
                _config.FirstStepFraction);
        }

        // Resolved in Start: every service has registered itself by then.
        private void Start()
        {
            _audio = Services.Get<IAudioService>();
            if (_audio == null)
            {
                Debug.LogError("FootstepPlayer found no audio service; steps stay silent.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (!_body.isGrounded)
            {
                _meter.Idle();
                return;
            }

            Vector3 velocity = _body.velocity;
            velocity.y = 0f;
            float speed = velocity.magnitude;
            if (speed < _config.IdleSpeed)
            {
                _meter.Idle();
                return;
            }

            Gait gait = _controller.CurrentGait;
            if (_meter.Advance(speed * Time.deltaTime, gait))
            {
                PlayStep(gait);
            }
        }

        private void PlayStep(Gait gait)
        {
            Vector3 origin = transform.position + Vector3.up * _config.ProbeHeight;
            Vector3 foot = transform.position;
            SurfaceType surface = _config.DefaultSurface;

            // The single-hit Raycast returns the nearest collider and allocates nothing; a one-slot
            // RaycastNonAlloc buffer would hand back whichever hit physics found first.
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit,
                    _config.ProbeHeight + _config.ProbeDistance, _config.ProbeMask, QueryTriggerInteraction.Ignore))
            {
                foot = hit.point;
                surface = Surface.TypeOf(hit.collider, _config.DefaultSurface);
            }

            AudioCue cue = _config.Surfaces == null ? null : _config.Surfaces.CueFor(surface);
            if (cue != null)
            {
                _audio.Play(cue, foot, _config.VolumeFor(gait));
            }
        }
    }
}
