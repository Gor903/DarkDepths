using UnityEngine;

namespace DarkDepths.Player
{
    [CreateAssetMenu(menuName = "DarkDepths/Flashlight Config", fileName = "FlashlightConfig")]
    public sealed class FlashlightConfig : ScriptableObject
    {
        [Header("Beam")]
        [SerializeField] private float _intensity = 3f;
        [SerializeField] private float _range = 14f;
        [SerializeField] private float _spotAngle = 55f;
        [Tooltip("Inner cone; the gap to spot angle is what makes the edge soft but still readable.")]
        [SerializeField] private float _innerSpotAngle = 32f;
        [Tooltip("Warm pale yellow from the palette; pure white reads as a flat wash on grey walls.")]
        [SerializeField] private Color _color = new Color(0.909f, 0.851f, 0.627f, 1f);

        [Header("Shadows")]
        [SerializeField, Range(0f, 1f)] private float _shadowStrength = 1f;
        [Tooltip("Where the beam starts casting; the default 0.2 m skips geometry right in front of the player.")]
        [SerializeField] private float _shadowNearPlane = 0.08f;

        [Header("Sway")]
        [Tooltip("Seconds the beam trails the view; 0 locks it rigidly to the camera.")]
        [SerializeField] private float _swayTime = 0.07f;
        [Tooltip("Hard cap on the trailing angle, so the beam never points where the player is not looking.")]
        [SerializeField] private float _maxSwayAngle = 6f;

        [Header("Battery")]
        [SerializeField] private float _batteryCapacity = 120f;
        [SerializeField] private float _drainPerSecond = 1f;

        [Header("Low battery")]
        [SerializeField, Range(0f, 1f)] private float _lowBatteryThreshold = 0.2f;
        [Tooltip("Intensity multiplier at an almost empty battery.")]
        [SerializeField, Range(0f, 1f)] private float _lowBatteryMinFactor = 0.35f;
        [Tooltip("Slow wobble, never a strobe.")]
        [SerializeField] private float _flickerSpeed = 2.5f;
        [SerializeField, Range(0f, 0.6f)] private float _flickerAmplitude = 0.22f;

        public float Intensity => _intensity;
        public float Range => _range;
        public float SpotAngle => _spotAngle;
        public float InnerSpotAngle => _innerSpotAngle;
        public Color Color => _color;

        public float ShadowStrength => _shadowStrength;
        public float ShadowNearPlane => _shadowNearPlane;

        public float SwayTime => _swayTime;
        public float MaxSwayAngle => _maxSwayAngle;

        public float BatteryCapacity => _batteryCapacity;
        public float DrainPerSecond => _drainPerSecond;

        public float LowBatteryThreshold => _lowBatteryThreshold;
        public float LowBatteryMinFactor => _lowBatteryMinFactor;
        public float FlickerSpeed => _flickerSpeed;
        public float FlickerAmplitude => _flickerAmplitude;
    }
}
