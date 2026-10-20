using UnityEngine;

namespace DarkDepths.Player
{
    [CreateAssetMenu(menuName = "DarkDepths/Player Look Config", fileName = "PlayerLookConfig")]
    public sealed class PlayerLookConfig : ScriptableObject
    {
        [Header("Camera")]
        [SerializeField] private float _fieldOfView = 70f;
        [Tooltip("Has to stay well under the body radius, or the near plane clips away walls the body is touching.")]
        [SerializeField] private float _nearClipPlane = 0.05f;
        [Tooltip("Extra field of view while sprinting; 0 turns the kick off.")]
        [SerializeField] private float _sprintFieldOfViewBoost = 5f;
        [SerializeField] private float _fieldOfViewTransition = 0.18f;

        [Header("Look")]
        [Tooltip("Degrees of rotation per unit of look input.")]
        [SerializeField] private float _lookSensitivity = 0.12f;
        [Tooltip("Seconds of smoothing over the look input; 0 applies every delta raw.")]
        [SerializeField] private float _lookSmoothing = 0.045f;
        [SerializeField] private bool _invertY;
        [SerializeField] private float _minPitch = -85f;
        [SerializeField] private float _maxPitch = 85f;

        public float FieldOfView => _fieldOfView;
        public float NearClipPlane => _nearClipPlane;
        public float SprintFieldOfViewBoost => _sprintFieldOfViewBoost;
        public float FieldOfViewTransition => _fieldOfViewTransition;

        public float LookSensitivity => _lookSensitivity;
        public float LookSmoothing => _lookSmoothing;
        public bool InvertY => _invertY;
        public float MinPitch => _minPitch;
        public float MaxPitch => _maxPitch;
    }
}
