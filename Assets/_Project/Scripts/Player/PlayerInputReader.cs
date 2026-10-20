using UnityEngine;
using UnityEngine.InputSystem;

namespace DarkDepths.Player
{
    // Only place that touches the Input System; the rest of the player code reads these properties.
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _actions;
        [SerializeField] private string _actionMapName = "Player";
        [Tooltip("Optional on-screen controls; keyboard and touch are merged when both are present.")]
        [SerializeField] private TouchControls _touch;

        private InputActionMap _map;
        private InputAction _move;
        private InputAction _look;
        private InputAction _sprint;
        private InputAction _crouch;
        private InputAction _flashlight;
        private InputAction _interact;
        private bool _crouchTogglePending;
        private bool _flashlightTogglePending;
        private bool _interactPending;

        public Vector2 Move
        {
            get
            {
                Vector2 value = _move.ReadValue<Vector2>();
                if (_touch != null)
                {
                    value += _touch.Move;
                }

                return Vector2.ClampMagnitude(value, 1f);
            }
        }

        public Vector2 Look
        {
            get
            {
                Vector2 value = _look.ReadValue<Vector2>();
                if (_touch != null)
                {
                    value += _touch.ConsumeLookDelta();
                }

                return value;
            }
        }

        public bool SprintHeld => _sprint.IsPressed() || (_touch != null && _touch.SprintHeld);

        // Returns true once per crouch press so the caller cannot react to the same press twice.
        public bool ConsumeCrouchToggle()
        {
            // Both sources are polled so a touch press is never left pending behind a keyboard press.
            bool fromTouch = _touch != null && _touch.ConsumeCrouchToggle();
            bool fromKeyboard = _crouchTogglePending;
            _crouchTogglePending = false;
            return fromKeyboard || fromTouch;
        }

        public bool ConsumeFlashlightToggle()
        {
            bool fromTouch = _touch != null && _touch.ConsumeFlashlightToggle();
            bool fromKeyboard = _flashlightTogglePending;
            _flashlightTogglePending = false;
            return fromKeyboard || fromTouch;
        }

        public bool ConsumeInteract()
        {
            bool fromTouch = _touch != null && _touch.ConsumeInteract();
            bool fromKeyboard = _interactPending;
            _interactPending = false;
            return fromKeyboard || fromTouch;
        }

        private void Awake()
        {
            _map = _actions.FindActionMap(_actionMapName, true);
            _move = _map.FindAction("Move", true);
            _look = _map.FindAction("Look", true);
            _sprint = _map.FindAction("Sprint", true);
            _crouch = _map.FindAction("Crouch", true);
            _flashlight = _map.FindAction("Flashlight", true);
            _interact = _map.FindAction("Interact", true);
        }

        private void OnEnable()
        {
            _crouch.performed += OnCrouchPerformed;
            _flashlight.performed += OnFlashlightPerformed;
            _interact.performed += OnInteractPerformed;
            _map.Enable();
        }

        private void OnDisable()
        {
            _map.Disable();
            _crouch.performed -= OnCrouchPerformed;
            _flashlight.performed -= OnFlashlightPerformed;
            _interact.performed -= OnInteractPerformed;
            _crouchTogglePending = false;
            _flashlightTogglePending = false;
            _interactPending = false;
        }

        private void OnCrouchPerformed(InputAction.CallbackContext context)
        {
            _crouchTogglePending = true;
        }

        private void OnFlashlightPerformed(InputAction.CallbackContext context)
        {
            _flashlightTogglePending = true;
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            _interactPending = true;
        }
    }
}
