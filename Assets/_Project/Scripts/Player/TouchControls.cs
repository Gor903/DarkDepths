using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DarkDepths.Player
{
    // Collects the on-screen controls into one object that PlayerInputReader can merge with the action map.
    public sealed class TouchControls : MonoBehaviour
    {
        [SerializeField] private TouchControlsConfig _config;
        [SerializeField] private GameObject _controlsRoot;
        [SerializeField] private VirtualJoystick _moveStick;
        [SerializeField] private LookArea _lookArea;
        [SerializeField] private TouchButton _crouchButton;
        [SerializeField] private TouchButton _flashlightButton;
        [SerializeField] private TouchButton _interactButton;
        [SerializeField] private RectTransform[] _mirrorTargets;
        [Tooltip("Lives on this canvas but is driven by PlayerInteractor, which is wired in the scene.")]
        [SerializeField] private Text _promptLabel;

        public Text PromptLabel => _promptLabel;
        public Vector2 Move => _moveStick.Value;
        public bool SprintHeld => _moveStick.SprintEngaged;

        public Vector2 ConsumeLookDelta()
        {
            return _lookArea.ConsumeDelta() * _config.LookSensitivityScale;
        }

        public bool ConsumeCrouchToggle()
        {
            return _crouchButton.ConsumePress();
        }

        public bool ConsumeFlashlightToggle()
        {
            return _flashlightButton.ConsumePress();
        }

        public bool ConsumeInteract()
        {
            return _interactButton.ConsumePress();
        }

        private void Awake()
        {
            _moveStick.Configure(_config);
            ApplyButtonLayout();

            if (_config.LeftHanded)
            {
                MirrorLayout();
            }

            // Without a touchscreen the transparent zones would swallow mouse clicks in the editor.
            if (Touchscreen.current == null)
            {
                _controlsRoot.SetActive(false);
            }
        }

        private void ApplyButtonLayout()
        {
            float size = _config.ButtonSize;
            float margin = _config.ButtonMargin;

            var crouchRect = (RectTransform)_crouchButton.transform;
            crouchRect.sizeDelta = new Vector2(size, size);
            crouchRect.anchoredPosition = new Vector2(-margin, margin);

            var flashlightRect = (RectTransform)_flashlightButton.transform;
            flashlightRect.sizeDelta = new Vector2(size, size);
            flashlightRect.anchoredPosition = new Vector2(-margin, margin * 2f + size);

            // Interact sits lowest on the right, where the thumb rests; 03 calls for bottom right.
            var interactRect = (RectTransform)_interactButton.transform;
            interactRect.sizeDelta = new Vector2(size, size);
            interactRect.anchoredPosition = new Vector2(-margin * 2f - size, margin);
        }

        private void MirrorLayout()
        {
            for (int i = 0; i < _mirrorTargets.Length; i++)
            {
                Mirror(_mirrorTargets[i]);
            }
        }

        private static void Mirror(RectTransform rect)
        {
            Vector2 anchorMin = rect.anchorMin;
            Vector2 anchorMax = rect.anchorMax;
            rect.anchorMin = new Vector2(1f - anchorMax.x, anchorMin.y);
            rect.anchorMax = new Vector2(1f - anchorMin.x, anchorMax.y);

            Vector2 pivot = rect.pivot;
            rect.pivot = new Vector2(1f - pivot.x, pivot.y);

            Vector2 position = rect.anchoredPosition;
            rect.anchoredPosition = new Vector2(-position.x, position.y);
        }
    }
}
