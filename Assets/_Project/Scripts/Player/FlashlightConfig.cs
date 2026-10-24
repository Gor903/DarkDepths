using UnityEngine;

namespace DarkDepths.Player
{
    [CreateAssetMenu(menuName = "DarkDepths/Flashlight Config", fileName = "FlashlightConfig")]
    public sealed class FlashlightConfig : ScriptableObject
    {
        [Header("Beam")]
        [Tooltip("Picked against the far wall of a 6 m room: at 3 it never arrived, at 9 it reads.")]
        [SerializeField] private float _intensity = 9f;
        [Tooltip("Shorter than the fog end, or the far half of the beam is swallowed before it lands.")]
        [SerializeField] private float _range = 12f;
        [SerializeField] private float _spotAngle = 45f;
        [Tooltip("Inner cone; the gap to spot angle is what makes the edge soft but still readable.")]
        [SerializeField] private float _innerSpotAngle = 22f;
        [Tooltip("Pale warm, almost neutral; a strongly tinted beam reads as cosy rather than cold.")]
        [SerializeField] private Color _color = new Color(0.961f, 0.945f, 0.898f, 1f);

        [Header("Mount")]
        [Tooltip("Where the torch sits relative to the eye; off-axis is what makes doorways throw a shadow.")]
        [SerializeField] private Vector3 _handOffset = new Vector3(0.18f, -0.15f, 0.1f);
        [Tooltip("Distance the beam is aimed at, so the hot spot still lands on the crosshair.")]
        [SerializeField] private float _convergenceDistance = 7f;

        [Header("Shadows")]
        [SerializeField, Range(0f, 1f)] private float _shadowStrength = 1f;
        [Tooltip("Where the beam starts casting; the default 0.2 m skips geometry right in front of the player.")]
        [SerializeField] private float _shadowNearPlane = 0.08f;

        [Header("Sway")]
        [Tooltip("Seconds the beam trails the view; 0 locks it rigidly to the camera.")]
        [SerializeField] private float _swayTime = 0.12f;
        [Tooltip("Below 1 the beam overshoots slightly on the way back, which is what reads as a hand.")]
        [SerializeField, Range(0.2f, 1.5f)] private float _swayDamping = 0.7f;
        [Tooltip("Hard cap on the trailing angle, so the beam never points where the player is not looking.")]
        [SerializeField] private float _maxSwayAngle = 6f;

        [Header("Breathing and bob")]
        [SerializeField, Range(0f, 2f)] private float _breathAmplitude = 0.3f;
        [SerializeField] private float _breathSpeed = 1.1f;
        [Tooltip("Degrees at full walking speed; the beam steps with the player, it does not bounce.")]
        [SerializeField, Range(0f, 3f)] private float _bobAmplitude = 0.6f;
        [SerializeField] private float _bobSpeed = 8f;
        [Tooltip("Speed that counts as a full stride; above it the bob stops growing.")]
        [SerializeField] private float _bobReferenceSpeed = 2.6f;

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
        [Tooltip("Noise on top of the wobble; left at 0 until the plain wobble has been looked at.")]
        [SerializeField, Range(0f, 0.6f)] private float _flickerNoiseAmplitude;
        [SerializeField] private float _flickerNoiseScale = 1.7f;

        public float Intensity => _intensity;
        public float Range => _range;
        public float SpotAngle => _spotAngle;
        public float InnerSpotAngle => _innerSpotAngle;
        public Color Color => _color;

        public Vector3 HandOffset => _handOffset;
        public float ConvergenceDistance => _convergenceDistance;

        public float ShadowStrength => _shadowStrength;
        public float ShadowNearPlane => _shadowNearPlane;

        public float SwayTime => _swayTime;
        public float SwayDamping => _swayDamping;
        public float MaxSwayAngle => _maxSwayAngle;

        public float BreathAmplitude => _breathAmplitude;
        public float BreathSpeed => _breathSpeed;
        public float BobAmplitude => _bobAmplitude;
        public float BobSpeed => _bobSpeed;
        public float BobReferenceSpeed => _bobReferenceSpeed;

        public float BatteryCapacity => _batteryCapacity;
        public float DrainPerSecond => _drainPerSecond;

        public float LowBatteryThreshold => _lowBatteryThreshold;
        public float LowBatteryMinFactor => _lowBatteryMinFactor;
        public float FlickerSpeed => _flickerSpeed;
        public float FlickerAmplitude => _flickerAmplitude;
        public float FlickerNoiseAmplitude => _flickerNoiseAmplitude;
        public float FlickerNoiseScale => _flickerNoiseScale;

        // The beam is aimed from the hand at the point the player is looking at, not straight down the camera.
        public Quaternion BeamRotation
        {
            get
            {
                Vector3 toAim = new Vector3(0f, 0f, Mathf.Max(_convergenceDistance, 0.01f)) - _handOffset;
                return Quaternion.LookRotation(toAim, Vector3.up);
            }
        }

        public BeamSway CreateSway()
        {
            return new BeamSway(_swayTime, _swayDamping, _maxSwayAngle,
                _breathAmplitude, _breathSpeed, _bobAmplitude, _bobSpeed);
        }
    }
}
