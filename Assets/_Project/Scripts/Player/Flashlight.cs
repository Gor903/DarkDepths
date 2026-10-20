using UnityEngine;

namespace DarkDepths.Player
{
    [RequireComponent(typeof(Light))]
    public sealed class Flashlight : MonoBehaviour
    {
        [SerializeField] private FlashlightConfig _config;
        [SerializeField] private PlayerInputReader _input;

        private Light _light;
        private Transform _mount;
        private Battery _battery;
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
        }

        private void Awake()
        {
            _light = GetComponent<Light>();
            _mount = transform.parent;
            _battery = new Battery(_config.BatteryCapacity, _config.DrainPerSecond);
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
            if (_mount == null || _config.SwayTime <= 0f)
            {
                return;
            }

            Quaternion view = _mount.rotation;
            Quaternion trailing = Quaternion.Slerp(transform.rotation, view,
                1f - Mathf.Exp(-Time.deltaTime / _config.SwayTime));
            transform.rotation = Quaternion.RotateTowards(view, trailing, _config.MaxSwayAngle);
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
            return intensity * wobble;
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
