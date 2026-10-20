using DarkDepths.Core;
using UnityEngine;

namespace DarkDepths.Player
{
    // Yaw turns the body, pitch turns the camera, so movement always follows the look direction.
    public sealed class PlayerLook : MonoBehaviour
    {
        [SerializeField] private PlayerLookConfig _config;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private Transform _yawTarget;
        [Tooltip("Drives the sprint field of view kick; found on a parent when left empty.")]
        [SerializeField] private FirstPersonController _controller;
        [Tooltip("Optional: scales the configured sensitivity by the player's own setting.")]
        [SerializeField] private UserSettings _settings;

        private Camera _camera;
        private LookSmoother _smoother;
        private float _pitch;
        private float _fieldOfView;
        private float _fieldOfViewVelocity;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _smoother = new LookSmoother(_config.LookSmoothing);
            _fieldOfView = _config.FieldOfView;

            if (_controller == null)
            {
                _controller = GetComponentInParent<FirstPersonController>();
            }

            // Applied before the first frame draws: a near plane as wide as the body makes touched walls vanish.
            _camera.nearClipPlane = _config.NearClipPlane;
            _camera.fieldOfView = _fieldOfView;
        }

        private void Start()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Cursor.lockState = CursorLockMode.Locked;
#endif
        }

        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;

            float sensitivity = _config.LookSensitivity;
            if (_settings != null)
            {
                sensitivity *= _settings.LookSensitivity;
            }

            Vector2 step = _smoother.Step(_input.Look * sensitivity, deltaTime);

            _yawTarget.Rotate(0f, step.x, 0f);

            float pitchDelta = step.y * (_config.InvertY ? 1f : -1f);
            _pitch = Mathf.Clamp(_pitch + pitchDelta, _config.MinPitch, _config.MaxPitch);
            transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

            UpdateFieldOfView(deltaTime);
        }

        private void UpdateFieldOfView(float deltaTime)
        {
            if (_controller == null || _config.SprintFieldOfViewBoost <= 0f)
            {
                return;
            }

            float target = _config.FieldOfView + (_controller.IsSprinting ? _config.SprintFieldOfViewBoost : 0f);
            if (Mathf.Approximately(_fieldOfView, target))
            {
                return;
            }

            _fieldOfView = Mathf.SmoothDamp(_fieldOfView, target, ref _fieldOfViewVelocity,
                _config.FieldOfViewTransition, Mathf.Infinity, deltaTime);
            _camera.fieldOfView = _fieldOfView;
        }
    }
}
