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

        [Header("Bounce")]
        [Tooltip("Fake reflected light where the beam lands; off leaves the beam as the only source.")]
        [SerializeField] private bool _bounceEnabled = true;
        [Tooltip("How far down the beam a surface still counts; past this the pool stays dark.")]
        [SerializeField] private float _bounceMaxDistance = 12f;
        [Tooltip("Rings of rays around the beam axis; 0 leaves the pool blind to anything off-centre.")]
        [SerializeField, Range(0, 3)] private int _bounceRings = 2;
        [Tooltip("How far out the outer ring sits, as a share of the cone; 1 puts it on the cone edge.")]
        [SerializeField, Range(0f, 1f)] private float _bounceSpread = 0.85f;
        [Tooltip("Overall strength of the reflected light; start at 0.5 and tune against the far wall.")]
        [SerializeField, Range(0f, 2f)] private float _bounceGain = 0.5f;
        [Tooltip("How far the pool floats off the surface; bigger reads as a wider, softer glow.")]
        [SerializeField] private float _bounceNormalOffset = 0.6f;
        [Tooltip("Reach of the pool, kept short so far corners stay black.")]
        [SerializeField] private float _bounceRange = 5f;
        [Tooltip("0 keeps the torch colour, 1 takes the colour of whatever it landed on.")]
        [SerializeField, Range(0f, 1f)] private float _bounceColorInfluence = 0.7f;
        [Tooltip("Ceiling on the pool, so a white wall at arm's length cannot flood the room.")]
        [SerializeField] private float _bounceMaxIntensity = 3f;
        [SerializeField] private float _bouncePositionSmoothTime = 0.08f;
        [Tooltip("How fast the pool brightens; short, or it arrives after the beam it belongs to.")]
        [SerializeField] private float _bounceIntensityRiseTime = 0.03f;
        [Tooltip("How slowly it dims once the beam leaves; longer than the rise, so nothing pops out.")]
        [SerializeField] private float _bounceIntensityFallTime = 0.1f;
        [SerializeField] private float _bounceColorSmoothTime = 0.1f;

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

        public bool BounceEnabled => _bounceEnabled;
        public float BounceMaxDistance => _bounceMaxDistance;
        public int BounceRings => _bounceRings;
        public float BounceSpread => _bounceSpread;
        public float BounceRange => _bounceRange;

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

        public BounceLight CreateBounce()
        {
            return new BounceLight(_bounceGain, _bounceNormalOffset, _bounceMaxIntensity,
                _bounceColorInfluence, _bouncePositionSmoothTime, _bounceIntensityRiseTime,
                _bounceIntensityFallTime, _bounceColorSmoothTime);
        }
    }
}
