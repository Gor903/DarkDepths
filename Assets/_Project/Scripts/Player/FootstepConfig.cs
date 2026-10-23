using DarkDepths.Audio;
using DarkDepths.Noise;
using UnityEngine;

namespace DarkDepths.Player
{
    // How often the player's steps land, how loud they are, and where the sound is looked up from.
    [CreateAssetMenu(menuName = "DarkDepths/Footstep Config", fileName = "FootstepConfig")]
    public sealed class FootstepConfig : ScriptableObject
    {
        [Header("Cadence, metres per step")]
        [SerializeField] private float _crouchStride = 1f;
        [SerializeField] private float _walkStride = 1.5f;
        [SerializeField] private float _sprintStride = 2f;
        [Tooltip("Share of a stride walked before the first step after standing still.")]
        [SerializeField, Range(0f, 1f)] private float _firstStepFraction = 0.4f;
        [Tooltip("Below this speed the player counts as standing.")]
        [SerializeField] private float _idleSpeed = 0.15f;

        [Header("Loudness")]
        [SerializeField, Range(0f, 1f)] private float _crouchVolume = 0.45f;
        [SerializeField, Range(0f, 1f)] private float _walkVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _sprintVolume = 1f;

        [Header("Surface probe")]
        [Tooltip("The ray starts this far above the feet, so a step on a kerb still finds the floor.")]
        [SerializeField] private float _probeHeight = 0.4f;
        [SerializeField] private float _probeDistance = 1.2f;
        [SerializeField] private LayerMask _probeMask = ~0;
        [SerializeField] private SurfaceType _defaultSurface = SurfaceType.Concrete;
        [SerializeField] private SurfaceAudioSet _surfaces;

        public float CrouchStride => _crouchStride;

        public float WalkStride => _walkStride;

        public float SprintStride => _sprintStride;

        public float FirstStepFraction => _firstStepFraction;

        public float IdleSpeed => _idleSpeed;

        public float ProbeHeight => _probeHeight;

        public float ProbeDistance => _probeDistance;

        public LayerMask ProbeMask => _probeMask;

        public SurfaceType DefaultSurface => _defaultSurface;

        public SurfaceAudioSet Surfaces => _surfaces;

        public float VolumeFor(Gait gait)
        {
            switch (gait)
            {
                case Gait.Crouch:
                    return _crouchVolume;
                case Gait.Sprint:
                    return _sprintVolume;
                default:
                    return _walkVolume;
            }
        }
    }
}
