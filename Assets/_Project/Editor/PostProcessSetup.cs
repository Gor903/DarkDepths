using System.IO;
using DarkDepths.Core;
using DarkDepths.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DarkDepths.EditorTools
{
    // Builds the post-processing docs/02 allows — weak vignette, light grain, bloom on the lamps — and nothing else.
    public static class PostProcessSetup
    {
        private const string ConfigFolder = "Assets/_Project/Config";
        private const string ConfigPath = ConfigFolder + "/PostProcessConfig.asset";
        private const string SettingsPath = ConfigFolder + "/UserSettings.asset";
        private const string ProfilePath = ConfigFolder + "/PostProcessProfile.asset";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player.prefab";
        private const string SandboxScenePath = "Assets/_Project/Scenes/Sandbox.unity";
        private const string VolumeName = "PostProcessing";

        [MenuItem("DarkDepths/Setup/Create Post Processing")]
        public static void CreatePostProcessing()
        {
            PostProcessConfig config = LoadOrCreate<PostProcessConfig>(ConfigPath);
            LoadOrCreate<UserSettings>(SettingsPath);

            BuildProfile(config);
            EnableOnPlayerPrefab();
            AssetDatabase.SaveAssets();

            PlaceVolume();

            AssetDatabase.SaveAssets();
        }

        // Post-processing runs per camera, so the flag belongs with the camera; Create Player calls this too.
        public static void EnableOnCamera(Camera camera)
        {
            var data = camera.GetComponent<UniversalAdditionalCameraData>();
            if (data == null)
            {
                data = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }

            data.renderPostProcessing = true;
            // Anti-aliasing is not on the allowed list and costs a full-screen pass on a phone.
            data.antialiasing = AntialiasingMode.None;
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return existing;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var created = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(created, path);
            return created;
        }

        private static VolumeProfile BuildProfile(PostProcessConfig config)
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                Directory.CreateDirectory(ConfigFolder);
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            // First in the stack and the reason the rest can be judged: without it the beam centre clips to white.
            Tonemapping tonemapping = GetOrAdd<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.Neutral);

            ColorAdjustments grading = GetOrAdd<ColorAdjustments>(profile);
            grading.postExposure.Override(config.PostExposure);
            grading.contrast.Override(config.Contrast);
            grading.saturation.Override(config.Saturation);

            Vignette vignette = GetOrAdd<Vignette>(profile);
            vignette.intensity.Override(config.VignetteIntensity);
            vignette.smoothness.Override(config.VignetteSmoothness);
            vignette.rounded.Override(false);

            FilmGrain grain = GetOrAdd<FilmGrain>(profile);
            grain.type.Override(FilmGrainLookup.Thin1);
            grain.intensity.Override(config.GrainIntensity);
            grain.response.Override(config.GrainResponse);

            Bloom bloom = GetOrAdd<Bloom>(profile);
            bloom.threshold.Override(config.BloomThreshold);
            bloom.intensity.Override(config.BloomIntensity);
            bloom.scatter.Override(config.BloomScatter);
            // A second blur chain the lamps do not need, and the one bloom setting that costs real time.
            bloom.highQualityFiltering.Override(false);

            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet(out T existing))
            {
                return existing;
            }

            T component = profile.Add<T>(true);
            component.name = typeof(T).Name;
            component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            // An override lives inside the profile asset; without this it is gone after the next reload.
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }

        private static void EnableOnPlayerPrefab()
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);

            var camera = contents.GetComponentInChildren<Camera>(true);
            if (camera != null)
            {
                EnableOnCamera(camera);
                PrefabUtility.SaveAsPrefabAsset(contents, PlayerPrefabPath);
            }

            PrefabUtility.UnloadPrefabContents(contents);
        }

        private static void PlaceVolume()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);

            // Loaded after the scene opens: opening one drops every asset nothing in it holds on to, and a
            // reference to a dropped asset writes back as null.
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            var config = AssetDatabase.LoadAssetAtPath<PostProcessConfig>(ConfigPath);
            var settings = AssetDatabase.LoadAssetAtPath<UserSettings>(SettingsPath);

            var existing = GameObject.Find(VolumeName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            var root = new GameObject(VolumeName, typeof(Volume), typeof(PostProcessTuner));
            var volume = root.GetComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;

            var tuner = root.GetComponent<PostProcessTuner>();
            SerializedWiring.Wire(tuner, "_settings", settings);
            SerializedWiring.Wire(tuner, "_config", config);
            SerializedWiring.Wire(tuner, "_volume", volume);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
