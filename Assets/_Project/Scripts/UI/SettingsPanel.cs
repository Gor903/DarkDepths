using DarkDepths.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DarkDepths.UI
{
    // Stage 1 options screen: brightness and look sensitivity, opened from an on-screen button.
    public sealed class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private UserSettings _settings;
        [SerializeField] private InputGate _inputGate;
        [SerializeField] private GameObject _window;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Slider _lookSensitivitySlider;
        [SerializeField] private Slider _brightnessSlider;
        [SerializeField] private Toggle _reduceEffectsToggle;
        [SerializeField] private Text _lookSensitivityValue;
        [SerializeField] private Text _brightnessValue;

        private int _shownLookSensitivity = int.MinValue;
        private int _shownBrightness = int.MinValue;
        private bool _isOpen;
        private bool _syncing;

        private void Awake()
        {
            _lookSensitivitySlider.minValue = _settings.LookSensitivityMin;
            _lookSensitivitySlider.maxValue = _settings.LookSensitivityMax;
            _brightnessSlider.minValue = _settings.BrightnessMin;
            _brightnessSlider.maxValue = _settings.BrightnessMax;
            _window.SetActive(false);
        }

        private void OnEnable()
        {
            _openButton.onClick.AddListener(Open);
            _closeButton.onClick.AddListener(Close);
            _resetButton.onClick.AddListener(ResetToDefaults);
            _lookSensitivitySlider.onValueChanged.AddListener(OnLookSensitivityChanged);
            _brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
            _reduceEffectsToggle.onValueChanged.AddListener(OnReduceEffectsChanged);
        }

        private void OnDisable()
        {
            _openButton.onClick.RemoveListener(Open);
            _closeButton.onClick.RemoveListener(Close);
            _resetButton.onClick.RemoveListener(ResetToDefaults);
            _lookSensitivitySlider.onValueChanged.RemoveListener(OnLookSensitivityChanged);
            _brightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);
            _reduceEffectsToggle.onValueChanged.RemoveListener(OnReduceEffectsChanged);
            Close();
        }

        private void Open()
        {
            if (_isOpen)
            {
                return;
            }

            _isOpen = true;
            PullFromSettings();
            _window.SetActive(true);
            _openButton.gameObject.SetActive(false);
            _inputGate.Block();
            SetPointerFree(true);
        }

        private void Close()
        {
            if (!_isOpen)
            {
                return;
            }

            _isOpen = false;
            _settings.Save();
            _window.SetActive(false);
            _openButton.gameObject.SetActive(true);
            _inputGate.Release();
            SetPointerFree(false);
        }

        private void ResetToDefaults()
        {
            _settings.ResetToDefaults();
            PullFromSettings();
        }

        private void OnLookSensitivityChanged(float value)
        {
            if (_syncing)
            {
                return;
            }

            _settings.LookSensitivity = value;
            ShowLookSensitivity();
        }

        private void OnBrightnessChanged(float value)
        {
            if (_syncing)
            {
                return;
            }

            _settings.Brightness = value;
            ShowBrightness();
        }

        private void OnReduceEffectsChanged(bool value)
        {
            if (_syncing)
            {
                return;
            }

            _settings.ReduceEffects = value;
        }

        // The sliders write back on change, so pushing stored values in needs the callbacks muted.
        private void PullFromSettings()
        {
            _syncing = true;
            _lookSensitivitySlider.value = _settings.LookSensitivity;
            _brightnessSlider.value = _settings.Brightness;
            _reduceEffectsToggle.isOn = _settings.ReduceEffects;
            _syncing = false;
            ShowLookSensitivity();
            ShowBrightness();
        }

        private void ShowLookSensitivity()
        {
            _shownLookSensitivity = ShowPercent(_lookSensitivityValue, _settings.LookSensitivity, _shownLookSensitivity);
        }

        private void ShowBrightness()
        {
            _shownBrightness = ShowPercent(_brightnessValue, _settings.Brightness, _shownBrightness);
        }

        // Only a changed whole percent rebuilds the label, so dragging a slider does not allocate every frame.
        private static int ShowPercent(Text label, float value, int shown)
        {
            int percent = Mathf.RoundToInt(value * 100f);
            if (percent == shown)
            {
                return shown;
            }

            label.text = percent.ToString() + "%";
            return percent;
        }

        private static void SetPointerFree(bool free)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = free;
#endif
        }
    }
}
