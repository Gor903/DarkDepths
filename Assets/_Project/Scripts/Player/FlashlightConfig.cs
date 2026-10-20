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

        public float BatteryCapacity => _batteryCapacity;
        public float DrainPerSecond => _drainPerSecond;

        public float LowBatteryThreshold => _lowBatteryThreshold;
        public float LowBatteryMinFactor => _lowBatteryMinFactor;
        public float FlickerSpeed => _flickerSpeed;
        public float FlickerAmplitude => _flickerAmplitude;
    }
}
