using UnityEngine;

namespace DarkDepths.Player
{
    [CreateAssetMenu(menuName = "DarkDepths/Player Movement Config", fileName = "PlayerMovementConfig")]
    public sealed class PlayerMovementConfig : ScriptableObject
    {
        [Header("Speed (m/s)")]
        [SerializeField] private float _crouchSpeed = 1.2f;
        [SerializeField] private float _walkSpeed = 2.5f;
        [SerializeField] private float _sprintSpeed = 4.5f;

        [Header("Body (m)")]
        [SerializeField] private float _standingHeight = 1.8f;
        [SerializeField] private float _crouchHeight = 1.2f;
        [SerializeField] private float _standingEyeHeight = 1.65f;
        [SerializeField] private float _crouchEyeHeight = 1.0f;
        [SerializeField] private float _radius = 0.3f;
        [SerializeField] private float _crouchTransitionSpeed = 6f;

        [Header("Stamina (s)")]
        [SerializeField] private float _sprintDuration = 5f;
        [SerializeField] private float _sprintRecovery = 8f;
        [Tooltip("Stamina level that has to be reached before sprinting is allowed again.")]
        [SerializeField, Range(0f, 1f)] private float _sprintRearmThreshold = 0.2f;

        [Header("Physics")]
        [SerializeField] private float _gravity = -9.81f;
        [Tooltip("Downward velocity kept while grounded so the controller stays glued to the floor.")]
        [SerializeField] private float _groundedStick = -2f;

        public float CrouchSpeed => _crouchSpeed;
        public float WalkSpeed => _walkSpeed;
        public float SprintSpeed => _sprintSpeed;

        public float StandingHeight => _standingHeight;
        public float CrouchHeight => _crouchHeight;
        public float StandingEyeHeight => _standingEyeHeight;
        public float CrouchEyeHeight => _crouchEyeHeight;
        public float Radius => _radius;
        public float CrouchTransitionSpeed => _crouchTransitionSpeed;

        public float SprintDuration => _sprintDuration;
        public float SprintRecovery => _sprintRecovery;
        public float SprintRearmThreshold => _sprintRearmThreshold;

        public float Gravity => _gravity;
        public float GroundedStick => _groundedStick;
    }
}
