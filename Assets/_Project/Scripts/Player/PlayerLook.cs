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
        [Tooltip("Optional: scales the configured sensitivity by the player's own setting.")]
        [SerializeField] private UserSettings _settings;

        private Camera _camera;
        private float _pitch;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Start()
        {
            _camera.fieldOfView = _config.FieldOfView;
#if UNITY_EDITOR || UNITY_STANDALONE
            Cursor.lockState = CursorLockMode.Locked;
#endif
        }

        private void LateUpdate()
        {
            Vector2 lookInput = _input.Look;
            float sensitivity = _config.LookSensitivity;
            if (_settings != null)
            {
                sensitivity *= _settings.LookSensitivity;
            }

            _yawTarget.Rotate(0f, lookInput.x * sensitivity, 0f);

            float pitchDelta = lookInput.y * sensitivity * (_config.InvertY ? 1f : -1f);
            _pitch = Mathf.Clamp(_pitch + pitchDelta, _config.MinPitch, _config.MaxPitch);
            transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
    }
}
