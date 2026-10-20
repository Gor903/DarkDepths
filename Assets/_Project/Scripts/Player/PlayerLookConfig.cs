using UnityEngine;

namespace DarkDepths.Player
{
    [CreateAssetMenu(menuName = "DarkDepths/Player Look Config", fileName = "PlayerLookConfig")]
    public sealed class PlayerLookConfig : ScriptableObject
    {
        [SerializeField] private float _fieldOfView = 70f;
        [Tooltip("Degrees of rotation per unit of look input.")]
        [SerializeField] private float _lookSensitivity = 0.12f;
        [SerializeField] private bool _invertY;
        [SerializeField] private float _minPitch = -85f;
        [SerializeField] private float _maxPitch = 85f;

        public float FieldOfView => _fieldOfView;
        public float LookSensitivity => _lookSensitivity;
        public bool InvertY => _invertY;
        public float MinPitch => _minPitch;
        public float MaxPitch => _maxPitch;
    }
}
