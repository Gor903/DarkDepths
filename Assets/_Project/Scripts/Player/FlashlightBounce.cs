using UnityEngine;

namespace DarkDepths.Player
{
    // Feeds BounceLight from the scene: cast a fan down the cone, read the surfaces, drive one point light.
    // Never touches the beam itself, so battery, flicker and sway keep working as they are.
    // Runs after Flashlight so the fan follows the beam of this frame rather than the one before it.
    [DefaultExecutionOrder(50)]
    [RequireComponent(typeof(Light))]
    public sealed class FlashlightBounce : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        // Six to a ring: enough to catch a cupboard entering the cone from any side, cheap enough to cast.
        private const int RaysPerRing = 6;
        // A fan crosses a handful of surfaces at once, so one remembered albedo is no longer enough.
        private const int AlbedoCacheSize = 8;

        [SerializeField] private FlashlightConfig _config;
        [Tooltip("What the beam can land on; the player is left out, the ray starts inside its own capsule.")]
        [SerializeField] private LayerMask _hitMask = ~0;

        private Light _beam;
        private Light _pool;
        private BounceLight _bounce;
        private BounceSample[] _samples;
        private readonly Collider[] _cachedColliders = new Collider[AlbedoCacheSize];
        private readonly Color[] _cachedAlbedo = new Color[AlbedoCacheSize];
        private int _cacheNext;

        private void Awake()
        {
            _beam = GetComponent<Light>();
            _bounce = _config.CreateBounce();
            _samples = new BounceSample[1 + Mathf.Max(0, _config.BounceRings) * RaysPerRing];

            // Left at the scene root: the pool belongs to the wall it fell on, not to the hand.
            var holder = new GameObject("FlashlightBounce");
            _pool = holder.AddComponent<Light>();
            _pool.type = LightType.Point;
            // Shadows here would double the cost of the one thing that is meant to be cheap.
            _pool.shadows = LightShadows.None;
            _pool.range = _config.BounceRange;
            _pool.intensity = 0f;
            _pool.enabled = false;
        }

        private void OnDestroy()
        {
            if (_pool != null)
            {
                Destroy(_pool.gameObject);
            }
        }

        private void LateUpdate()
        {
            if (!_config.BounceEnabled)
            {
                _bounce.Clear();
                _pool.enabled = false;
                return;
            }

            Vector3 position = _bounce.Position;
            Color color = _bounce.Color;
            float intensity = 0f;
            bool lit = false;

            if (_beam.enabled && _beam.intensity > 0f)
            {
                int rays = CastFan(out int hits);
                lit = _bounce.TryResolve(_samples, hits, rays, out position, out color, out intensity);
            }

            _bounce.Advance(lit, position, color, intensity, Time.deltaTime);

            _pool.transform.position = _bounce.Position;
            _pool.intensity = _bounce.Intensity;
            _pool.color = _bounce.Color.gamma;
            _pool.range = _config.BounceRange;
            _pool.enabled = _bounce.IsLit;
        }

        // The axis plus a ring or two spread across the cone; returns how many were cast, hits how many landed.
        private int CastFan(out int hits)
        {
            hits = 0;
            // Clamped to what Awake sized the buffer for, so turning the knob at runtime cannot overrun it.
            int rings = Mathf.Clamp(_config.BounceRings, 0, (_samples.Length - 1) / RaysPerRing);
            float outer = _beam.spotAngle * 0.5f * _config.BounceSpread;
            int rays = 0;

            for (int ring = 0; ring <= rings; ring++)
            {
                // Rings share the cone evenly, and every other one is turned half a step so they interleave.
                float polar = rings == 0 ? 0f : outer * ring / rings;
                int around = ring == 0 ? 1 : RaysPerRing;
                float twist = (ring & 1) == 0 ? 0f : 180f / RaysPerRing;

                for (int step = 0; step < around; step++)
                {
                    rays++;
                    Vector3 direction = Direction(polar, twist + 360f * step / RaysPerRing);
                    if (!Physics.Raycast(transform.position, direction, out RaycastHit hit,
                            _config.BounceMaxDistance, _hitMask, QueryTriggerInteraction.Ignore))
                    {
                        continue;
                    }

                    _samples[hits++] = new BounceSample(hit.point, hit.normal, Albedo(hit.collider).linear,
                        direction, _beam.color.linear, _beam.intensity, _beam.spotAngle);
                }
            }

            return rays;
        }

        // Tilted off the axis by the polar angle, then spun around the axis by the azimuth.
        private Vector3 Direction(float polar, float azimuth)
        {
            if (polar <= 0f)
            {
                return transform.forward;
            }

            return Quaternion.AngleAxis(azimuth, transform.forward)
                   * Quaternion.AngleAxis(polar, transform.right)
                   * transform.forward;
        }

        // Looked up once per surface and remembered, so a fan crossing one wall still costs one lookup.
        private Color Albedo(Collider collider)
        {
            for (int i = 0; i < AlbedoCacheSize; i++)
            {
                if (_cachedColliders[i] == collider)
                {
                    return _cachedAlbedo[i];
                }
            }

            Color albedo = Color.gray;
            var renderer = collider.GetComponentInParent<Renderer>();
            Material material = renderer != null ? renderer.sharedMaterial : null;
            if (material != null)
            {
                if (material.HasProperty(BaseColorId))
                {
                    albedo = material.GetColor(BaseColorId);
                }
                else if (material.HasProperty(ColorId))
                {
                    albedo = material.GetColor(ColorId);
                }
            }

            _cachedColliders[_cacheNext] = collider;
            _cachedAlbedo[_cacheNext] = albedo;
            _cacheNext = (_cacheNext + 1) % AlbedoCacheSize;

            return albedo;
        }
    }
}
