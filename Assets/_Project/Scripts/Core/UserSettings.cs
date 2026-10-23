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
        private const string SfxVolumeKey = "settings.volume.sfx";
        private const string AmbientVolumeKey = "settings.volume.ambient";
        private const string MusicVolumeKey = "settings.volume.music";
        private const string SoundHintsKey = "settings.soundHints";

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

        [Header("Volume, 1 = the mix as authored")]
        [SerializeField, Range(0f, 1f)] private float _sfxVolumeDefault = 1f;
        [SerializeField, Range(0f, 1f)] private float _ambientVolumeDefault = 1f;
        [SerializeField, Range(0f, 1f)] private float _musicVolumeDefault = 1f;

        [Header("Hearing")]
        [Tooltip("docs/04: nothing may be audible only, so a strong sound nearby also shows at the edge of the screen.")]
        [SerializeField] private bool _soundHintsDefault = true;

        private float _lookSensitivity;
        private float _brightness;
        private bool _reduceEffects;
        private float _sfxVolume;
        private float _ambientVolume;
        private float _musicVolume;
        private bool _soundHints;
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
                Set(ref _reduceEffects, value);
            }
        }

        public float SfxVolume
        {
            get
            {
                EnsureLoaded();
                return _sfxVolume;
            }
            set
            {
                EnsureLoaded();
                Set(ref _sfxVolume, Mathf.Clamp01(value));
            }
        }

        public float AmbientVolume
        {
            get
            {
                EnsureLoaded();
                return _ambientVolume;
            }
            set
            {
                EnsureLoaded();
                Set(ref _ambientVolume, Mathf.Clamp01(value));
            }
        }

        public float MusicVolume
        {
            get
            {
                EnsureLoaded();
                return _musicVolume;
            }
            set
            {
                EnsureLoaded();
                Set(ref _musicVolume, Mathf.Clamp01(value));
            }
        }

        public bool SoundHints
        {
            get
            {
                EnsureLoaded();
                return _soundHints;
            }
            set
            {
                EnsureLoaded();
                Set(ref _soundHints, value);
            }
        }

        public void ResetToDefaults()
        {
            EnsureLoaded();
            LookSensitivity = _lookSensitivityDefault;
            Brightness = _brightnessDefault;
            ReduceEffects = _reduceEffectsDefault;
            SfxVolume = _sfxVolumeDefault;
            AmbientVolume = _ambientVolumeDefault;
            MusicVolume = _musicVolumeDefault;
            SoundHints = _soundHintsDefault;
        }

        public void Save()
        {
            EnsureLoaded();
            PlayerPrefs.SetFloat(LookSensitivityKey, _lookSensitivity);
            PlayerPrefs.SetFloat(BrightnessKey, _brightness);
            PlayerPrefs.SetInt(ReduceEffectsKey, _reduceEffects ? 1 : 0);
            PlayerPrefs.SetFloat(SfxVolumeKey, _sfxVolume);
            PlayerPrefs.SetFloat(AmbientVolumeKey, _ambientVolume);
            PlayerPrefs.SetFloat(MusicVolumeKey, _musicVolume);
            PlayerPrefs.SetInt(SoundHintsKey, _soundHints ? 1 : 0);
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
            _sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, _sfxVolumeDefault));
            _ambientVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(AmbientVolumeKey, _ambientVolumeDefault));
            _musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, _musicVolumeDefault));
            _soundHints = PlayerPrefs.GetInt(SoundHintsKey, _soundHintsDefault ? 1 : 0) != 0;
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

        private void Set(ref bool field, bool value)
        {
            if (field == value)
            {
                return;
            }

            field = value;
            Changed?.Invoke();
        }
    }
}
