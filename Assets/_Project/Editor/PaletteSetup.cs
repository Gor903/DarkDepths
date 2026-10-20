using System;
using System.IO;
using DarkDepths.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace DarkDepths.EditorTools
{
    // Turns PaletteConfig into what the renderer actually uses: matte materials, scene lighting, sight distance.
    public static class PaletteSetup
    {
        private const string ConfigFolder = "Assets/_Project/Config";
        private const string ConfigPath = ConfigFolder + "/PaletteConfig.asset";
        private const string MaterialFolder = "Assets/_Project/Art/Materials";
        private const string MaterialPrefix = "Palette_";
        private const string SandboxScenePath = "Assets/_Project/Scenes/Sandbox.unity";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player.prefab";
        private const string LitShader = "Universal Render Pipeline/Lit";

        [MenuItem("DarkDepths/Setup/Create Palette")]
        public static void CreatePalette()
        {
            PaletteConfig palette = LoadOrCreateConfig();

            foreach (PaletteRole role in Enum.GetValues(typeof(PaletteRole)))
            {
                MaterialFor(role, palette);
            }

            ApplyShadowDistance(palette);
            ApplyPlayerCamera(palette);
            ApplySceneLighting(palette);

            AssetDatabase.SaveAssets();
        }

        public static PaletteConfig LoadOrCreateConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<PaletteConfig>(ConfigPath);
            if (existing != null)
            {
                return existing;
            }

            Directory.CreateDirectory(ConfigFolder);
            var created = ScriptableObject.CreateInstance<PaletteConfig>();
            AssetDatabase.CreateAsset(created, ConfigPath);
            return created;
        }

        // Re-applies the palette onto the material that is already there, so scene references survive a re-run.
        public static Material MaterialFor(PaletteRole role, PaletteConfig palette = null)
        {
            palette = palette != null ? palette : LoadOrCreateConfig();

            string path = MaterialFolder + "/" + MaterialPrefix + role + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Directory.CreateDirectory(MaterialFolder);
                material = new Material(Shader.Find(LitShader));
                AssetDatabase.CreateAsset(material, path);
            }

            Color color = palette.Resolve(role);
            material.color = color;
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", palette.Smoothness);
            material.SetFloat("_Metallic", palette.Metallic);

            // Only lamps and valuables glow; everything else has to be lit to be seen.
            bool emissive = role == PaletteRole.Lamp;
            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", palette.Emission);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        // Solid background rather than the default skybox: outside the fog there is nothing to see.
        public static void ApplyCameraBackground(Camera camera, PaletteConfig palette)
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = palette.Background;
        }

        private static void ApplySceneLighting(PaletteConfig palette)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);

            ApplyRenderSettings(palette);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // Works on whichever scene is open, so room setup can re-apply the lighting it just rebuilt around.
        public static void ApplyRenderSettings(PaletteConfig palette)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = palette.AmbientLight;
            RenderSettings.skybox = null;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = palette.Background;
            RenderSettings.fogStartDistance = palette.FogStartDistance;
            RenderSettings.fogEndDistance = palette.FogEndDistance;

            // The directional light is a fill that keeps wall edges readable; the flashlight stays the only real source.
            var fill = UnityEngine.Object.FindAnyObjectByType<Light>();
            if (fill != null && fill.type == LightType.Directional)
            {
                fill.color = palette.Background;
                fill.intensity = palette.FillLightIntensity;
                fill.shadows = LightShadows.None;
            }
        }

        private static void ApplyPlayerCamera(PaletteConfig palette)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                return;
            }

            var camera = prefab.GetComponentInChildren<Camera>(true);
            if (camera == null)
            {
                return;
            }

            ApplyCameraBackground(camera, palette);
            PrefabUtility.SavePrefabAsset(prefab);
        }

        // Shadows have to reach as far as sight does, or they pop into view at the fog edge.
        private static void ApplyShadowDistance(PaletteConfig palette)
        {
            // Every quality level, not just the active one: which level batchmode picks is not ours to guess.
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                WriteShadowDistance(QualitySettings.GetRenderPipelineAssetAt(i), palette.ShadowDistance);
            }

            WriteShadowDistance(GraphicsSettings.defaultRenderPipeline, palette.ShadowDistance);
        }

        private static void WriteShadowDistance(RenderPipelineAsset pipeline, float distance)
        {
            if (pipeline == null)
            {
                return;
            }

            var serialized = new SerializedObject(pipeline);
            SerializedProperty property = serialized.FindProperty("m_ShadowDistance");
            if (property == null || Mathf.Approximately(property.floatValue, distance))
            {
                return;
            }

            property.floatValue = distance;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipeline);
        }
    }
}
