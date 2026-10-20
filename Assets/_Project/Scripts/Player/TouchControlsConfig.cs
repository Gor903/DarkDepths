using UnityEngine;

namespace DarkDepths.Player
{
    [CreateAssetMenu(menuName = "DarkDepths/Touch Controls Config", fileName = "TouchControlsConfig")]
    public sealed class TouchControlsConfig : ScriptableObject
    {
        [Header("Joystick (reference px)")]
        [SerializeField] private float _joystickRadius = 140f;
        [SerializeField, Range(0f, 0.9f)] private float _joystickDeadZone = 0.15f;

        [Header("Buttons (reference px, 1920x1080 landscape canvas)")]
        [Tooltip("Keep above the 48 dp minimum from docs/03_PLAYER_AND_CONTROLS.md; confirm on a device.")]
        [SerializeField] private float _buttonSize = 160f;
        [SerializeField] private float _buttonMargin = 48f;

        [Header("Sprint by pushing the stick up")]
        [Tooltip("Normalized stick distance that latches sprint.")]
        [SerializeField, Range(0.5f, 1f)] private float _sprintEngageThreshold = 0.9f;
        [Tooltip("Dropping below this unlatches it; the gap keeps sprint from flickering.")]
        [SerializeField, Range(0.2f, 0.95f)] private float _sprintReleaseThreshold = 0.55f;

        [Header("Stick colors (move to PaletteConfig at T2.1)")]
        [SerializeField] private Color _handleColor = new Color(1f, 1f, 1f, 0.32f);
        [SerializeField] private Color _handleSprintColor = new Color(1f, 1f, 1f, 0.8f);

        [Header("Look")]
        [Tooltip("Multiplies swipe delta before it reaches the same sensitivity as mouse look.")]
        [SerializeField] private float _lookSensitivityScale = 1f;

        [Header("Layout")]
        [SerializeField] private bool _leftHanded;

        public float JoystickRadius => _joystickRadius;
        public float JoystickDeadZone => _joystickDeadZone;
        public float ButtonSize => _buttonSize;
        public float ButtonMargin => _buttonMargin;
        public float SprintEngageThreshold => _sprintEngageThreshold;
        public float SprintReleaseThreshold => _sprintReleaseThreshold;
        public Color HandleColor => _handleColor;
        public Color HandleSprintColor => _handleSprintColor;
        public float LookSensitivityScale => _lookSensitivityScale;
        public bool LeftHanded => _leftHanded;
    }
}
