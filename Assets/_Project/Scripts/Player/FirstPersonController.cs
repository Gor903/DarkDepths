using UnityEngine;

namespace DarkDepths.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        private const float EyeSnapDistance = 0.001f;

        [SerializeField] private PlayerMovementConfig _config;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private Transform _cameraPivot;

        private CharacterController _controller;
        private Stamina _stamina;
        private bool _isCrouching;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        private float _targetEyeHeight;
        private float _eyeVelocity;

        public bool IsCrouching => _isCrouching;
        // What the flashlight bob is scaled by; the vertical component is gravity, not walking.
        public float HorizontalSpeed => _horizontalVelocity.magnitude;
        public bool IsSprinting { get; private set; }
        public Gait CurrentGait => _isCrouching ? Gait.Crouch : IsSprinting ? Gait.Sprint : Gait.Walk;
        public float StaminaNormalized => _stamina.Normalized;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _stamina = new Stamina(_config.SprintDuration, _config.SprintRecovery, _config.SprintRearmThreshold);
            _controller.radius = _config.Radius;
            ApplyBodyHeight(_config.StandingHeight);
            _targetEyeHeight = _config.StandingEyeHeight;
            SetEyeHeight(_targetEyeHeight);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            UpdateCrouchState();

            Vector2 moveInput = _input.Move;
            bool wantsSprint = _input.SprintHeld && !_isCrouching && moveInput.sqrMagnitude > 0.01f;
            IsSprinting = wantsSprint && _stamina.CanSprint;
            _stamina.Tick(deltaTime, IsSprinting);

            MoveBody(moveInput, deltaTime);
            UpdateEyeHeight(deltaTime);
        }

        private void UpdateCrouchState()
        {
            if (!_input.ConsumeCrouchToggle())
            {
                return;
            }

            _isCrouching = !_isCrouching;
            ApplyBodyHeight(_isCrouching ? _config.CrouchHeight : _config.StandingHeight);
            _targetEyeHeight = _isCrouching ? _config.CrouchEyeHeight : _config.StandingEyeHeight;
        }

        private void MoveBody(Vector2 moveInput, float deltaTime)
        {
            float speed = _isCrouching
                ? _config.CrouchSpeed
                : IsSprinting ? _config.SprintSpeed : _config.WalkSpeed;

            Vector3 direction = transform.right * moveInput.x + transform.forward * moveInput.y;
            if (direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }

            // Ramping to the target instead of snapping is what stops starts and stops feeling like a switch.
            Vector3 target = direction * speed;
            float rate = target.sqrMagnitude > _horizontalVelocity.sqrMagnitude
                ? _config.Acceleration
                : _config.Deceleration;
            _horizontalVelocity = rate > 0f
                ? Vector3.MoveTowards(_horizontalVelocity, target, rate * deltaTime)
                : target;

            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = _config.GroundedStick;
            }
            else
            {
                _verticalVelocity += _config.Gravity * deltaTime;
            }

            Vector3 velocity = _horizontalVelocity;
            velocity.y = _verticalVelocity;
            _controller.Move(velocity * deltaTime);
        }

        private void ApplyBodyHeight(float height)
        {
            _controller.height = height;
            _controller.center = new Vector3(0f, height * 0.5f, 0f);
        }

        private void UpdateEyeHeight(float deltaTime)
        {
            Vector3 local = _cameraPivot.localPosition;
            if (Mathf.Approximately(local.y, _targetEyeHeight))
            {
                return;
            }

            float next = Mathf.SmoothDamp(local.y, _targetEyeHeight, ref _eyeVelocity,
                _config.CrouchTransitionTime, Mathf.Infinity, deltaTime);

            // SmoothDamp only approaches its target, so the last millimetre is snapped and the writes stop.
            if (Mathf.Abs(_targetEyeHeight - next) < EyeSnapDistance)
            {
                next = _targetEyeHeight;
                _eyeVelocity = 0f;
            }

            local.y = next;
            _cameraPivot.localPosition = local;
        }

        private void SetEyeHeight(float height)
        {
            Vector3 local = _cameraPivot.localPosition;
            local.y = height;
            _cameraPivot.localPosition = local;
        }
    }
}
