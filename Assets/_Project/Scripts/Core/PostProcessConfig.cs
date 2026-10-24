using UnityEngine;

namespace DarkDepths.Core
{
    // The post-processing docs/02 allows and nothing else: tone mapping, grading, a weak vignette, grain, bloom.
    [CreateAssetMenu(menuName = "DarkDepths/Post Process Config", fileName = "PostProcessConfig")]
    public sealed class PostProcessConfig : ScriptableObject
    {
        [Header("Grading")]
        [Tooltip("Without tone mapping the centre of the beam clips to flat white.")]
        [SerializeField, Range(-100f, 100f)] private float _saturation = -20f;
        [SerializeField, Range(-100f, 100f)] private float _contrast = 10f;
        [Tooltip("Stops, applied before grading; a negative value is what sets the floor of the dark.")]
        [SerializeField, Range(-2f, 2f)] private float _postExposure = -0.3f;

        [Header("Vignette")]
        [Tooltip("Weak: the corners darken, they do not close in.")]
        [SerializeField, Range(0f, 1f)] private float _vignetteIntensity = 0.3f;
        [SerializeField, Range(0.01f, 1f)] private float _vignetteSmoothness = 0.45f;

        [Header("Film grain")]
        [SerializeField, Range(0f, 1f)] private float _grainIntensity = 0.12f;
        [Tooltip("How much the grain fades out of lit areas; the dark is where it belongs.")]
        [SerializeField, Range(0f, 1f)] private float _grainResponse = 0.8f;

        [Header("Bloom")]
        [Tooltip("Above 1 only the emissive lamps pass, which is the only thing allowed to glow.")]
        [SerializeField] private float _bloomThreshold = 1.1f;
        [SerializeField] private float _bloomIntensity = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _bloomScatter = 0.6f;

        [Header("Reduced effects")]
        [Tooltip("What is left of each effect when the player turns visual effects down; 0 switches them off.")]
        [SerializeField, Range(0f, 1f)] private float _reducedScale;

        public float Saturation => _saturation;
        public float Contrast => _contrast;
        public float PostExposure => _postExposure;

        public float VignetteIntensity => _vignetteIntensity;
        public float VignetteSmoothness => _vignetteSmoothness;

        public float GrainIntensity => _grainIntensity;
        public float GrainResponse => _grainResponse;

        public float BloomThreshold => _bloomThreshold;
        public float BloomIntensity => _bloomIntensity;
        public float BloomScatter => _bloomScatter;

        public float ReducedScale => _reducedScale;

        public PostProcessBudget CreateBudget(bool reduced)
        {
            return new PostProcessBudget(_vignetteIntensity, _grainIntensity, _bloomIntensity, _reducedScale, reduced);
        }
    }
}
