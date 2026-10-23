using DarkDepths.Core;
using DarkDepths.Noise;
using UnityEngine;

namespace DarkDepths.World
{
    // A door is one of the loudest things a careless player does. Which row of the loudness table this
    // particular door uses is set on it: a dry hinge is the creaking one.
    [RequireComponent(typeof(Door))]
    [DisallowMultipleComponent]
    public sealed class DoorNoise : MonoBehaviour
    {
        [SerializeField] private NoiseSound _swingSound = NoiseSound.DoorOpen;
        [SerializeField] private NoiseSound _latchSound = NoiseSound.DoorOpen;
        [Tooltip("The latch is the quiet end of a door: a fraction of the reach of the swing.")]
        [SerializeField, Range(0f, 1f)] private float _latchScale = 0.6f;

        private Door _door;
        private INoiseService _noise;

        private void Awake()
        {
            _door = GetComponent<Door>();
        }

        private void OnEnable()
        {
            _door.Opening += EmitSwing;
            _door.Closing += EmitSwing;
            _door.Latched += EmitLatch;
        }

        private void OnDisable()
        {
            _door.Opening -= EmitSwing;
            _door.Closing -= EmitSwing;
            _door.Latched -= EmitLatch;
        }

        // Resolved in Start: every service has registered itself by then.
        private void Start()
        {
            _noise = Services.Get<INoiseService>();
            if (_noise == null)
            {
                Debug.LogError("DoorNoise found no noise service; the door opens unheard.", this);
                enabled = false;
            }
        }

        private void EmitSwing()
        {
            Emit(_swingSound, 1f);
        }

        private void EmitLatch()
        {
            Emit(_latchSound, _latchScale);
        }

        private void Emit(NoiseSound sound, float scale)
        {
            if (_noise != null)
            {
                _noise.Emit(sound, transform.position, NoiseType.Door, gameObject, scale);
            }
        }
    }
}
