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
        [SerializeField, Range(0f, 1f)] private float _wallSmoothness = 0.15f;
        [Tooltip("The floor is the one surface allowed to catch the beam, which is what gives the room a shape.")]
        [SerializeField, Range(0f, 1f)] private float _floorSmoothness = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _metallic;
        [Tooltip("Multiplier on the lamp colour for emissive surfaces.")]
        [SerializeField] private float _lampEmission = 2.5f;

        [Header("Lighting")]
        [Tooltip("How much of the background colour lifts the scene; above 0.1 the dark stops being dark.")]
        [SerializeField, Range(0f, 1f)] private float _ambientIntensity = 0.07f;
        [Tooltip("Intensity of the scene fill light; 0 switches it off, which is where the look starts.")]
        [SerializeField, Range(0f, 1f)] private float _fillLightIntensity;

        [Header("Shadows")]
        [Tooltip("Pipeline-wide depth bias; raise it against acne, lower it against light leaking under walls.")]
        [SerializeField, Range(0f, 10f)] private float _shadowDepthBias = 1f;
        [SerializeField, Range(0f, 10f)] private float _shadowNormalBias = 1f;

        [Header("Distance (m)")]
        [SerializeField] private float _fogStartDistance = 3f;
        [Tooltip("Sight reaches this far; the draw distance budget in docs/07 is 25-30 m.")]
        [SerializeField] private float _fogEndDistance = 18f;
        [Tooltip("Slightly under the ambient lift, so the fog reads as depth and not as a grey wall.")]
        [SerializeField, Range(0f, 1f)] private float _fogIntensity = 0.05f;
        [SerializeField] private float _shadowDistance = 20f;

        public Color Background => _background;
        public Color Structure => _structure;
        public Color Prop => _prop;
        public Color Lamp => _lamp;
        public Color Danger => _danger;
        public Color Enemy => _enemy;

        public float Metallic => _metallic;
        public Color Emission => _lamp * _lampEmission;

        public float FillLightIntensity => _fillLightIntensity;
        public float ShadowDepthBias => _shadowDepthBias;
        public float ShadowNormalBias => _shadowNormalBias;

        public float FogStartDistance => _fogStartDistance;
        // Fog that ends before it starts renders as a hard wall, so the span is kept open here.
        public float FogEndDistance => Mathf.Max(_fogEndDistance, _fogStartDistance + MinFogSpan);
        public float ShadowDistance => _shadowDistance;

        public float SmoothnessFor(SurfaceFinish finish)
        {
            return finish == SurfaceFinish.Floor ? _floorSmoothness : _wallSmoothness;
        }

        // Flat ambient tinted by the background, which is what keeps unlit corners dark but not black.
        public Color AmbientLight => Dimmed(_ambientIntensity);

        // Darker than the ambient, or the far wall would glow brighter than the room it closes off.
        public Color FogColor => Dimmed(Mathf.Min(_fogIntensity, _ambientIntensity));

        private Color Dimmed(float intensity)
        {
            Color color = _background * intensity;
            color.a = 1f;
            return color;
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
