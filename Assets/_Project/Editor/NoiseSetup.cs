using System.IO;
using DarkDepths.Noise;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Creates the noise config of docs/04 and puts the system that reads it into the sandbox.
    public static class NoiseSetup
    {
        private const string ConfigFolder = "Assets/_Project/Config";
        private const string ConfigPath = ConfigFolder + "/NoiseConfig.asset";
        private const string SandboxScenePath = "Assets/_Project/Scenes/Sandbox.unity";
        private const string SystemName = "Noise";

        [MenuItem("DarkDepths/Setup/Create Noise")]
        public static void CreateNoise()
        {
            // Both masks are built from a layer name, so the layers have to exist first.
            LayersSetup.CreateLayers();

            NoiseConfig config = LoadOrCreate();
            int wall = 1 << LayerMask.NameToLayer("Wall");
            // Walls and floors are both built on the Wall layer: one blocks a sound, the other carries it.
            SerializedWiring.WireInt(config, "_wallMask", wall);
            SerializedWiring.WireInt(config, "_probeMask", wall);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            PlaceSystem();
            AssetDatabase.SaveAssets();
        }

        private static void PlaceSystem()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);

            // Loaded after the scene opens, like the other setup tools: opening a scene drops unreferenced assets.
            var config = AssetDatabase.LoadAssetAtPath<NoiseConfig>(ConfigPath);

            var existing = GameObject.Find(SystemName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            var root = new GameObject(SystemName, typeof(NoiseSystem));
            SerializedWiring.Wire(root.GetComponent<NoiseSystem>(), "_config", config);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static NoiseConfig LoadOrCreate()
        {
            var existing = AssetDatabase.LoadAssetAtPath<NoiseConfig>(ConfigPath);
            if (existing != null)
            {
                return existing;
            }

            Directory.CreateDirectory(ConfigFolder);
            var created = ScriptableObject.CreateInstance<NoiseConfig>();
            AssetDatabase.CreateAsset(created, ConfigPath);
            return created;
        }
    }
}
