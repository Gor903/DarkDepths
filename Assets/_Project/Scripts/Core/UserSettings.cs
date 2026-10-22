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
        private const string ReduceEffectsKey = "settings.reduceEffects";

        [Header("Look sensitivity, as a multiplier over PlayerLookConfig")]
        [SerializeField] private float _lookSensitivityMin = 0.4f;
        [SerializeField] private float _lookSensitivityMax = 2.5f;
        [SerializeField] private float _lookSensitivityDefault = 1f;

        [Header("Brightness, 1 = the scene as authored")]
        [SerializeField] private float _brightnessMin = 0.7f;
        [SerializeField] private float _brightnessMax = 1.5f;
        [SerializeField] private float _brightnessDefault = 1f;

        [Header("Visual effects")]
        [Tooltip("Turns the post-processing of docs/02 down; off by default, on for a phone that cannot afford it.")]
        [SerializeField] private bool _reduceEffectsDefault;

        private float _lookSensitivity;
        private float _brightness;
        private bool _reduceEffects;
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

        public bool ReduceEffects
        {
            get
            {
                EnsureLoaded();
                return _reduceEffects;
            }
            set
            {
                EnsureLoaded();
                if (_reduceEffects == value)
                {
                    return;
                }

                _reduceEffects = value;
                Changed?.Invoke();
            }
        }

        public void ResetToDefaults()
        {
            EnsureLoaded();
            LookSensitivity = _lookSensitivityDefault;
            Brightness = _brightnessDefault;
            ReduceEffects = _reduceEffectsDefault;
        }

        public void Save()
        {
            EnsureLoaded();
            PlayerPrefs.SetFloat(LookSensitivityKey, _lookSensitivity);
            PlayerPrefs.SetFloat(BrightnessKey, _brightness);
            PlayerPrefs.SetInt(ReduceEffectsKey, _reduceEffects ? 1 : 0);
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
            _reduceEffects = PlayerPrefs.GetInt(ReduceEffectsKey, _reduceEffectsDefault ? 1 : 0) != 0;
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
