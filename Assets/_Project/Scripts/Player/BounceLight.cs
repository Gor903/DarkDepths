using UnityEngine;

namespace DarkDepths.Player
{
    // The faint pool a torch leaves around the patch it lights, faked as one unshadowed point light.
    // Kept free of MonoBehaviour so it can be covered by EditMode tests.
    public sealed class BounceLight
    {
        // Below this the pool is switched off rather than left on at a brightness nothing can see.
        private const float DarkThreshold = 0.001f;
        // A surface taken edge-on still returns something, or the pool blinks out when the beam rakes a wall.
        private const float MinIncidence = 0.2f;
        // Keeps the hue divide away from zero when the beam lands on something nearly black.
        private const float MinChannel = 1e-4f;

        private readonly float _gain;
        private readonly float _normalOffset;
        private readonly float _maxIntensity;
        private readonly float _colorInfluence;
        private readonly float _positionSmoothTime;
        private readonly float _intensityRiseTime;
        private readonly float _intensityFallTime;
        private readonly float _colorSmoothTime;

        private Vector3 _position;
        private Vector3 _positionVelocity;
        private float _intensity;
        private float _intensityVelocity;
        private Color _color = Color.white;

        public BounceLight(float gain, float normalOffset, float maxIntensity, float colorInfluence,
            float positionSmoothTime, float intensityRiseTime, float intensityFallTime, float colorSmoothTime)
        {
            _gain = Mathf.Max(0f, gain);
            _normalOffset = Mathf.Max(0f, normalOffset);
            _maxIntensity = Mathf.Max(0f, maxIntensity);
            _colorInfluence = Mathf.Clamp01(colorInfluence);
            _positionSmoothTime = Mathf.Max(0f, positionSmoothTime);
            _intensityRiseTime = Mathf.Max(0f, intensityRiseTime);
            _intensityFallTime = Mathf.Max(0f, intensityFallTime);
            _colorSmoothTime = Mathf.Max(0f, colorSmoothTime);
        }

        public bool IsLit => _intensity > DarkThreshold;
        public Vector3 Position => _position;
        public float Intensity => _intensity;

        // Linear, as the maths works in; the renderer wants it converted back.
        public Color Color => _color;

        // Pass lit false when the torch is off or the beam reached nothing, and the pool fades out.
        public void Advance(bool lit, in BounceSample sample, float deltaTime)
        {
            if (!lit)
            {
                Advance(false, _position, _color, 0f, deltaTime);
                return;
            }

            Advance(true, TargetPosition(sample), TargetColor(sample), TargetIntensity(sample), deltaTime);
        }

        // What the whole fan of rays boiled down to, already resolved by TryResolve.
        public void Advance(bool lit, Vector3 targetPosition, Color targetColor, float targetIntensity,
            float deltaTime)
        {
            if (!lit)
            {
                targetIntensity = 0f;
                targetPosition = _position;
                targetColor = _color;
            }
            else
            {
                // Coming back from dark it appears where and as bright as it belongs, rather than
                // sliding in off the last wall and growing: the torch itself toggles instantly.
                if (!IsLit)
                {
                    _position = targetPosition;
                    _color = targetColor;
                    _intensity = targetIntensity;
                    _positionVelocity = Vector3.zero;
                    _intensityVelocity = 0f;
                }
            }

            _position = Vector3.SmoothDamp(_position, targetPosition, ref _positionVelocity,
                _positionSmoothTime, Mathf.Infinity, deltaTime);
            // Brightening lands with the beam; dimming is left slow, since a pool that pops out is the louder of the two.
            float intensitySmoothTime = targetIntensity > _intensity ? _intensityRiseTime : _intensityFallTime;
            _intensity = Mathf.SmoothDamp(_intensity, targetIntensity, ref _intensityVelocity,
                intensitySmoothTime, Mathf.Infinity, deltaTime);
            _color = UnityEngine.Color.Lerp(_color, targetColor, ColorStep(deltaTime));
        }

        // Boils what the fan of rays found down to the one pool that stands in for all of it. Weighted by
        // the energy each ray brings back, so a pale cupboard off to one side pulls the pool onto itself
        // instead of waiting for the axis to arrive. rays counts the ones that were cast, misses included:
        // a cone half into open air returns half a pool rather than a full one hanging off the near edge.
        public bool TryResolve(BounceSample[] samples, int count, int rays,
            out Vector3 position, out Color color, out float intensity)
        {
            position = _position;
            color = _color;
            intensity = 0f;
            if (samples == null || count <= 0 || rays <= 0)
            {
                return false;
            }

            float total = 0f;
            Vector3 weightedPoint = Vector3.zero;
            var weightedColor = new Color(0f, 0f, 0f, 0f);

            for (int i = 0; i < count; i++)
            {
                float energy = TargetIntensity(samples[i]);
                if (energy <= 0f)
                {
                    continue;
                }

                total += energy;
                weightedPoint += TargetPosition(samples[i]) * energy;
                weightedColor += TargetColor(samples[i]) * energy;
            }

            if (total <= DarkThreshold)
            {
                return false;
            }

            position = weightedPoint / total;
            color = weightedColor / total;
            intensity = Mathf.Min(total / rays, _maxIntensity);

            return true;
        }

        public void Clear()
        {
            _intensity = 0f;
            _intensityVelocity = 0f;
            _positionVelocity = Vector3.zero;
        }

        // Energy on the footprint is (I / d^2) * pi * (d * tan a)^2 = I * pi * tan^2 a: the distance cancels,
        // so a far wall is no dimmer than a near one. What varies is how much the surface gives back.
        public float TargetIntensity(in BounceSample sample)
        {
            float luma = Mathf.Clamp01(
                0.2126f * sample.Albedo.r + 0.7152f * sample.Albedo.g + 0.0722f * sample.Albedo.b);
            float tangent = Mathf.Tan(sample.SpotAngle * 0.5f * Mathf.Deg2Rad);
            float incidence = Mathf.Max(MinIncidence, Vector3.Dot(-sample.BeamForward, sample.Normal));

            float energy = sample.BeamIntensity * luma * Mathf.PI * tangent * tangent * incidence * _gain;
            return Mathf.Clamp(energy, 0f, _maxIntensity);
        }

        // Floated off the wall rather than sat on it, or the pool would be a hot ring at the contact point.
        public Vector3 TargetPosition(in BounceSample sample)
        {
            return sample.Point + sample.Normal * _normalOffset;
        }

        // Hue only: the surface tints the pool, but how bright it is stays the job of the intensity.
        public Color TargetColor(in BounceSample sample)
        {
            Color tint = new Color(
                sample.Albedo.r * sample.BeamColor.r,
                sample.Albedo.g * sample.BeamColor.g,
                sample.Albedo.b * sample.BeamColor.b);
            float peak = Mathf.Max(Mathf.Max(tint.r, tint.g), Mathf.Max(tint.b, MinChannel));
            var hue = new Color(tint.r / peak, tint.g / peak, tint.b / peak);

            return UnityEngine.Color.Lerp(sample.BeamColor, hue, _colorInfluence);
        }

        // Frame rate independent, so the colour settles in the same time at 30 and at 120 frames.
        private float ColorStep(float deltaTime)
        {
            return 1f - Mathf.Exp(-deltaTime / Mathf.Max(MinChannel, _colorSmoothTime));
        }
    }
}
