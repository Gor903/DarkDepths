using DarkDepths.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DarkDepths.UI
{
    // Puts PostProcessConfig onto the scene volume and takes it back down when the player asks for fewer effects.
    [RequireComponent(typeof(Volume))]
    public sealed class PostProcessTuner : MonoBehaviour
    {
        [SerializeField] private UserSettings _settings;
        [SerializeField] private PostProcessConfig _config;
        [SerializeField] private Volume _volume;

        private Vignette _vignette;
        private FilmGrain _grain;
        private Bloom _bloom;
        private Tonemapping _tonemapping;
        private ColorAdjustments _grading;

        private void Awake()
        {
            if (_volume == null)
            {
                _volume = GetComponent<Volume>();
            }

            // The instanced profile, not the shared asset: turning effects down must not edit what is on disk.
            VolumeProfile profile = _volume.profile;
            profile.TryGet(out _vignette);
            profile.TryGet(out _grain);
            profile.TryGet(out _bloom);
            profile.TryGet(out _tonemapping);
            profile.TryGet(out _grading);
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
            PostProcessBudget budget = _config.CreateBudget(_settings.ReduceEffects);

            // Grading is not an effect the player can turn down: it is how the image is exposed.
            if (_grading != null)
            {
                _grading.postExposure.value = _config.PostExposure;
                _grading.contrast.value = _config.Contrast;
                _grading.saturation.value = _config.Saturation;
            }

            if (_vignette != null)
            {
                _vignette.intensity.value = budget.VignetteIntensity;
                _vignette.smoothness.value = _config.VignetteSmoothness;
                _vignette.active = budget.VignetteActive;
            }

            if (_grain != null)
            {
                _grain.intensity.value = budget.GrainIntensity;
                _grain.response.value = _config.GrainResponse;
                _grain.active = budget.GrainActive;
            }

            if (_bloom != null)
            {
                _bloom.intensity.value = budget.BloomIntensity;
                _bloom.threshold.value = _config.BloomThreshold;
                _bloom.scatter.value = _config.BloomScatter;
                _bloom.active = budget.BloomActive;
            }

            // With nothing left to render the whole stack is skipped rather than run at zero strength;
            // tone mapping and grading are the exception, because without them the beam clips to white.
            _volume.enabled = budget.AnyActive || _tonemapping != null || _grading != null;
        }
    }
}
