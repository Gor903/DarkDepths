using UnityEngine;

namespace DarkDepths.Core
{
    // The one place the look is set: colours, how matte the surfaces are and how far sight reaches.
    [CreateAssetMenu(menuName = "DarkDepths/Palette Config", fileName = "PaletteConfig")]
    public sealed class PaletteConfig : ScriptableObject
    {
        private const float MinFogSpan = 1f;

        [Header("Colors")]
        [Tooltip("Background and deep shadow; also the colour the fog fades everything into.")]
        [SerializeField] private Color _background = new Color32(0x0F, 0x17, 0x24, 0xFF);
        [SerializeField] private Color _structure = new Color32(0x1B, 0x20, 0x28, 0xFF);
        [SerializeField] private Color _prop = new Color32(0x3A, 0x46, 0x56, 0xFF);
        [Tooltip("Warm pale yellow: rare lamps and valuable items, the only thing that glows.")]
        [SerializeField] private Color _lamp = new Color32(0xE8, 0xD9, 0xA0, 0xFF);
        [Tooltip("Danger signal; a rare accent, never a permanent light.")]
        [SerializeField] private Color _danger = new Color32(0x8C, 0x2A, 0x2A, 0xFF);
        [SerializeField] private Color _enemy = new Color32(0x05, 0x06, 0x08, 0xFF);

        [Header("Surfaces")]
        [Tooltip("Matte: a highlight travelling across a wall would read as a second light source.")]
        [SerializeField, Range(0f, 1f)] private float _smoothness = 0.05f;
        [SerializeField, Range(0f, 1f)] private float _metallic;
        [Tooltip("Multiplier on the lamp colour for emissive surfaces.")]
        [SerializeField] private float _lampEmission = 2.5f;

        [Header("Lighting")]
        [Tooltip("How much of the background colour lifts the scene, so walls keep a readable silhouette.")]
        [SerializeField, Range(0f, 1f)] private float _ambientIntensity = 0.18f;
        [Tooltip("Intensity of the scene fill light; the flashlight has to stay the main source.")]
        [SerializeField, Range(0f, 1f)] private float _fillLightIntensity = 0.05f;

        [Header("Distance (m)")]
        [SerializeField] private float _fogStartDistance = 6f;
        [Tooltip("Sight reaches this far; the draw distance budget in docs/07 is 25-30 m.")]
        [SerializeField] private float _fogEndDistance = 28f;
        [SerializeField] private float _shadowDistance = 30f;

        public Color Background => _background;
        public Color Structure => _structure;
        public Color Prop => _prop;
        public Color Lamp => _lamp;
        public Color Danger => _danger;
        public Color Enemy => _enemy;

        public float Smoothness => _smoothness;
        public float Metallic => _metallic;
        public Color Emission => _lamp * _lampEmission;

        public float FillLightIntensity => _fillLightIntensity;

        public float FogStartDistance => _fogStartDistance;
        // Fog that ends before it starts renders as a hard wall, so the span is kept open here.
        public float FogEndDistance => Mathf.Max(_fogEndDistance, _fogStartDistance + MinFogSpan);
        public float ShadowDistance => _shadowDistance;

        // Flat ambient tinted by the background, which is what keeps unlit corners dark but not black.
        public Color AmbientLight
        {
            get
            {
                Color ambient = _background * _ambientIntensity;
                ambient.a = 1f;
                return ambient;
            }
        }

        public Color Resolve(PaletteRole role)
        {
            switch (role)
            {
                case PaletteRole.Background: return _background;
                case PaletteRole.Prop: return _prop;
                case PaletteRole.Lamp: return _lamp;
                case PaletteRole.Danger: return _danger;
                case PaletteRole.Enemy: return _enemy;
                default: return _structure;
            }
        }
    }
}
