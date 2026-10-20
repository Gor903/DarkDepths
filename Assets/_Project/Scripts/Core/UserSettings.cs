using System;
using UnityEngine;

namespace DarkDepths.Core
{
    // Player-facing options. Values live in PlayerPrefs until the profile save arrives at T6.1.
    [CreateAssetMenu(menuName = "DarkDepths/User Settings", fileName = "UserSettings")]
    public sealed class UserSettings : ScriptableObject
    {
        private const string LookSensitivityKey = "settings.lookSensitivity";
        private const string BrightnessKey = "settings.brightness";

        [Header("Look sensitivity, as a multiplier over PlayerLookConfig")]
        [SerializeField] private float _lookSensitivityMin = 0.4f;
        [SerializeField] private float _lookSensitivityMax = 2.5f;
        [SerializeField] private float _lookSensitivityDefault = 1f;

        [Header("Brightness, 1 = the scene as authored")]
        [SerializeField] private float _brightnessMin = 0.7f;
        [SerializeField] private float _brightnessMax = 1.5f;
        [SerializeField] private float _brightnessDefault = 1f;

        private float _lookSensitivity;
        private float _brightness;
        private bool _loaded;

        public event Action Changed;

        public float LookSensitivityMin => _lookSensitivityMin;
        public float LookSensitivityMax => _lookSensitivityMax;
        public float BrightnessMin => _brightnessMin;
        public float BrightnessMax => _brightnessMax;

        public float LookSensitivity
        {
            get
            {
                EnsureLoaded();
                return _lookSensitivity;
            }
            set
            {
                EnsureLoaded();
                Set(ref _lookSensitivity, Mathf.Clamp(value, _lookSensitivityMin, _lookSensitivityMax));
            }
        }

        public float Brightness
        {
            get
            {
                EnsureLoaded();
                return _brightness;
            }
            set
            {
                EnsureLoaded();
                Set(ref _brightness, Mathf.Clamp(value, _brightnessMin, _brightnessMax));
            }
        }

        public void ResetToDefaults()
        {
            EnsureLoaded();
            LookSensitivity = _lookSensitivityDefault;
            Brightness = _brightnessDefault;
        }

        public void Save()
        {
            EnsureLoaded();
            PlayerPrefs.SetFloat(LookSensitivityKey, _lookSensitivity);
            PlayerPrefs.SetFloat(BrightnessKey, _brightness);
            PlayerPrefs.Save();
        }

        // Loaded on first access rather than in OnEnable, so asset import order cannot leave a reader with zeros.
        private void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;
            _lookSensitivity = Mathf.Clamp(PlayerPrefs.GetFloat(LookSensitivityKey, _lookSensitivityDefault),
                _lookSensitivityMin, _lookSensitivityMax);
            _brightness = Mathf.Clamp(PlayerPrefs.GetFloat(BrightnessKey, _brightnessDefault),
                _brightnessMin, _brightnessMax);
        }

        private void Set(ref float field, float value)
        {
            if (Mathf.Approximately(field, value))
            {
                return;
            }

            field = value;
            Changed?.Invoke();
        }
    }
}
