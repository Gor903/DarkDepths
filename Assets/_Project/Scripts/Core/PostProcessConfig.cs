using UnityEngine;

namespace DarkDepths.Core
{
    // The post-processing docs/02 allows and nothing else: a weak vignette, light grain, bloom on the lamps.
    [CreateAssetMenu(menuName = "DarkDepths/Post Process Config", fileName = "PostProcessConfig")]
    public sealed class PostProcessConfig : ScriptableObject
    {
        [Header("Vignette")]
        [Tooltip("Weak: the corners darken, they do not close in.")]
        [SerializeField, Range(0f, 1f)] private float _vignetteIntensity = 0.28f;
        [SerializeField, Range(0.01f, 1f)] private float _vignetteSmoothness = 0.4f;

        [Header("Film grain")]
        [SerializeField, Range(0f, 1f)] private float _grainIntensity = 0.18f;
        [Tooltip("How much the grain fades out of lit areas; the dark is where it belongs.")]
        [SerializeField, Range(0f, 1f)] private float _grainResponse = 0.8f;

        [Header("Bloom")]
        [Tooltip("Above 1 only the emissive lamps pass, which is the only thing allowed to glow.")]
        [SerializeField] private float _bloomThreshold = 1.1f;
        [SerializeField] private float _bloomIntensity = 0.6f;
        [SerializeField, Range(0f, 1f)] private float _bloomScatter = 0.6f;

        [Header("Reduced effects")]
        [Tooltip("What is left of each effect when the player turns visual effects down; 0 switches them off.")]
        [SerializeField, Range(0f, 1f)] private float _reducedScale;

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
