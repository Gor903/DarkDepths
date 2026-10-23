using DarkDepths.Core;
using DarkDepths.Noise;
using UnityEngine;

namespace DarkDepths.Player
{
    // Every step the player takes is also something the building can hear. How far it carries is the
    // gait's row of the loudness table and what the floor is made of, both of them in NoiseConfig.
    [RequireComponent(typeof(FootstepPlayer))]
    [DisallowMultipleComponent]
    public sealed class FootstepNoise : MonoBehaviour
    {
        private FootstepPlayer _steps;
        private INoiseService _noise;

        private void Awake()
        {
            _steps = GetComponent<FootstepPlayer>();
        }

        private void OnEnable()
        {
            _steps.Stepped += Emit;
        }

        private void OnDisable()
        {
            _steps.Stepped -= Emit;
        }

        // Resolved in Start: every service has registered itself by then.
        private void Start()
        {
            _noise = Services.Get<INoiseService>();
            if (_noise == null)
            {
                Debug.LogError("FootstepNoise found no noise service; the player walks unheard.", this);
                enabled = false;
            }
        }

        private void Emit(Vector3 foot, Gait gait)
        {
            if (_noise != null)
            {
                _noise.Emit(SoundFor(gait), foot, NoiseType.Footstep, gameObject, 1f);
            }
        }

        // Which row of the table a gait uses; the metres themselves are never in code.
        private static NoiseSound SoundFor(Gait gait)
        {
            switch (gait)
            {
                case Gait.Crouch:
                    return NoiseSound.CrouchStep;
                case Gait.Sprint:
                    return NoiseSound.RunStep;
                default:
                    return NoiseSound.WalkStep;
            }
        }
    }
}
