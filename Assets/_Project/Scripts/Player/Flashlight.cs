using UnityEngine;

namespace DarkDepths.Player
{
    [RequireComponent(typeof(Light))]
    public sealed class Flashlight : MonoBehaviour
    {
        [SerializeField] private FlashlightConfig _config;
        [SerializeField] private PlayerInputReader _input;
        [Tooltip("What the walking bob is driven by; found up the hierarchy when it is left empty.")]
        [SerializeField] private FirstPersonController _controller;

        private Light _light;
        private Transform _mount;
        private Battery _battery;
        private BeamSway _sway;
        private Quaternion _beamRotation;
        private Quaternion _lastViewRotation;
        private bool _isOn;
        private float _flickerPhase;

        public bool IsOn => _isOn;
        public float BatteryNormalized => _battery.Normalized;

        public void RefillBattery(float amount)
        {
            _battery.Refill(amount);
        }

        // Public so the setup script can serialize the same beam the component applies at runtime.
        public void ApplyBeamSettings()
        {
            if (_light == null)
            {
                _light = GetComponent<Light>();
            }

            _light.type = LightType.Spot;
            _light.color = _config.Color;
            _light.range = _config.Range;
            _light.spotAngle = _config.SpotAngle;
            _light.innerSpotAngle = _config.InnerSpotAngle;
            _light.intensity = _config.Intensity;

            // Without shadows the cone lights whatever is behind the walls too, and reads as haze, not a torch.
            _light.shadows = LightShadows.Soft;
            _light.shadowStrength = _config.ShadowStrength;
            _light.shadowNearPlane = _config.ShadowNearPlane;

            // Held beside the eye rather than at it: that offset is what gives door frames a shadow to cast.
            transform.localPosition = _config.HandOffset;
            transform.localRotation = _config.BeamRotation;
        }

        private void Awake()
        {
            _light = GetComponent<Light>();
            _mount = transform.parent;
            // A prefab built before the bob existed has nothing wired, and the torch is always under the player.
            if (_controller == null)
            {
                _controller = GetComponentInParent<FirstPersonController>();
            }

            _battery = new Battery(_config.BatteryCapacity, _config.DrainPerSecond);
            _sway = _config.CreateSway();
            _beamRotation = _config.BeamRotation;
            _lastViewRotation = _mount != null ? _mount.rotation : transform.rotation;
            ApplyBeamSettings();
            SetOn(false);
        }

        private void Update()
        {
            if (_input.ConsumeFlashlightToggle())
            {
                SetOn(!_isOn);
            }

            if (!_isOn)
            {
                return;
            }

            _battery.Drain(Time.deltaTime);
            if (_battery.IsEmpty)
            {
                SetOn(false);
                return;
            }

            _light.intensity = CurrentIntensity(Time.deltaTime);
        }

        // The beam trails the view by a few hundredths of a second, which is what sells a hand-held torch.
        private void LateUpdate()
        {
            if (_mount == null)
            {
                return;
            }

            Vector2 offset = _sway.Advance(ViewDelta(), Stride(), Time.deltaTime);
            transform.localRotation = Quaternion.Euler(offset.x, offset.y, 0f) * _beamRotation;
        }

        // How far the view turned since the last frame, pitch in x and yaw in y, wrapped to a signed angle.
        private Vector2 ViewDelta()
        {
            Quaternion view = _mount.rotation;
            Vector3 delta = (Quaternion.Inverse(_lastViewRotation) * view).eulerAngles;
            _lastViewRotation = view;

            return new Vector2(Wrap(delta.x), Wrap(delta.y));
        }

        private static float Wrap(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }

        private float Stride()
        {
            if (_controller == null || _config.BobReferenceSpeed <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(_controller.HorizontalSpeed / _config.BobReferenceSpeed);
        }

        private float CurrentIntensity(float deltaTime)
        {
            float normalized = _battery.Normalized;
            if (normalized > _config.LowBatteryThreshold)
            {
                _flickerPhase = 0f;
                return _config.Intensity;
            }

            // Below the threshold the beam sags, then wobbles slowly on top of that.
            float remaining = Mathf.InverseLerp(0f, _config.LowBatteryThreshold, normalized);
            float intensity = _config.Intensity * Mathf.Lerp(_config.LowBatteryMinFactor, 1f, remaining);

            _flickerPhase += deltaTime * _config.FlickerSpeed;
            float wobble = 1f - _config.FlickerAmplitude * (1f - Mathf.Cos(_flickerPhase)) * 0.5f;
            return intensity * wobble * Noise();
        }

        // Breaks up the regularity of the wobble; at amplitude 0 it costs one multiply and changes nothing.
        private float Noise()
        {
            if (_config.FlickerNoiseAmplitude <= 0f)
            {
                return 1f;
            }

            float noise = Mathf.PerlinNoise(_flickerPhase * _config.FlickerNoiseScale, 0f);
            return 1f - _config.FlickerNoiseAmplitude * (1f - noise);
        }

        // Toggling is instant: no flash-up animation, per docs/03_PLAYER_AND_CONTROLS.md.
        private void SetOn(bool on)
        {
            if (on && _battery.IsEmpty)
            {
                return;
            }

            _isOn = on;
            _light.enabled = on;
            if (on)
            {
                _light.intensity = _config.Intensity;
            }
        }
    }
}
