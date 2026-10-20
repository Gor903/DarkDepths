using DarkDepths.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DarkDepths.UI
{
    // Lifts or crushes the frame with one transparent quad; stage 1 has no post-processing stack to drive.
    public sealed class BrightnessOverlay : MonoBehaviour
    {
        [SerializeField] private UserSettings _settings;
        [Tooltip("Mixed in above neutral brightness; warm rather than pure white to keep the palette.")]
        [SerializeField] private Color _liftColor = new Color(0.95f, 0.93f, 0.86f, 1f);
        [SerializeField] private Color _crushColor = Color.black;
        [SerializeField, Range(0f, 0.6f)] private float _maxLiftAlpha = 0.16f;
        [SerializeField, Range(0f, 0.6f)] private float _maxCrushAlpha = 0.35f;

        private Graphic _graphic;

        private void Awake()
        {
            _graphic = GetComponent<Graphic>();
            _graphic.raycastTarget = false;
        }

        private void OnEnable()
        {
            _settings.Changed += Apply;
            Apply();
        }

        private void OnDisable()
        {
            _settings.Changed -= Apply;
        }

        private void Apply()
        {
            float brightness = _settings.Brightness;
            if (brightness >= 1f)
            {
                float amount = Mathf.InverseLerp(1f, _settings.BrightnessMax, brightness);
                SetOverlay(_liftColor, amount * _maxLiftAlpha);
                return;
            }

            float darkening = Mathf.InverseLerp(1f, _settings.BrightnessMin, brightness);
            SetOverlay(_crushColor, darkening * _maxCrushAlpha);
        }

        // Disabled at neutral so the default setting costs no draw call.
        private void SetOverlay(Color color, float alpha)
        {
            color.a = alpha;
            _graphic.color = color;
            _graphic.enabled = alpha > 0.001f;
        }
    }
}
