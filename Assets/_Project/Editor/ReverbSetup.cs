using System.IO;
using DarkDepths.Audio;
using DarkDepths.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Gives the sandbox the two spaces docs/04 asks to be told apart by ear: the room and the corridor.
    // The zone is engine-side; what each space sounds like lives in ReverbConfig, so rooms built later
    // answer the same way.
    public static class ReverbSetup
    {
        private const string ConfigFolder = "Assets/_Project/Config";
        private const string ConfigPath = ConfigFolder + "/ReverbConfig.asset";
        private const string SandboxScenePath = "Assets/_Project/Scenes/Sandbox.unity";
        private const string ReverbRoot = "Reverb";

        // Room cells and corridor cells of the test room; the corridor's middle is three cells out.
        private const float CorridorCells = 3f;

        private static readonly ReverbSpace[] Spaces =
        {
            ReverbSpace.Room,
            ReverbSpace.Corridor,
            ReverbSpace.Hall
        };

        private static readonly AudioReverbPreset[] Presets =
        {
            AudioReverbPreset.Room,
            AudioReverbPreset.Hallway,
            AudioReverbPreset.Auditorium
        };

        private static readonly float[] MinDistances = { 3.5f, 2f, 8f };
        private static readonly float[] MaxDistances = { 9f, 5.5f, 25f };

        [MenuItem("DarkDepths/Setup/Create Reverb")]
        public static void CreateReverb()
        {
            BuildConfig();
            AssetDatabase.SaveAssets();

            ApplyToScene();
            AssetDatabase.SaveAssets();
        }

        private static ReverbConfig BuildConfig()
        {
            ReverbConfig config = LoadOrCreate(ConfigPath);

            var serialized = new SerializedObject(config);
            SerializedProperty entries = serialized.FindProperty("_entries");
            // Filled once; after that the table is whatever it was tuned to.
            if (entries.arraySize == Spaces.Length)
            {
                return config;
            }

            entries.arraySize = Spaces.Length;
            for (int i = 0; i < Spaces.Length; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("Space").intValue = (int)Spaces[i];
                entry.FindPropertyRelative("Preset").intValue = (int)Presets[i];
                entry.FindPropertyRelative("MinDistance").floatValue = MinDistances[i];
                entry.FindPropertyRelative("MaxDistance").floatValue = MaxDistances[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            return config;
        }

        private static void ApplyToScene()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);

            // Loaded after the scene opens, like the other setup tools: opening a scene drops unreferenced assets.
            ModuleConfig modules = ModuleSetup.LoadOrCreateConfig();
            var config = AssetDatabase.LoadAssetAtPath<ReverbConfig>(ConfigPath);

            var existing = GameObject.Find(ReverbRoot);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            var root = new GameObject(ReverbRoot);
            float ear = modules.WallHeight * 0.5f;

            // The sandbox has no hall; its preset waits in the table for a room that is one.
            Place(root.transform, config, ReverbSpace.Room, "Room", new Vector3(0f, ear, 0f));
            Place(root.transform, config, ReverbSpace.Corridor, "Corridor",
                new Vector3(0f, ear, modules.CellSize * CorridorCells));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void Place(Transform parent, ReverbConfig config, ReverbSpace space, string zoneName,
            Vector3 position)
        {
            if (!config.TryGet(space, out ReverbConfig.Entry entry))
            {
                return;
            }

            var holder = new GameObject(zoneName);
            holder.transform.SetParent(parent, false);
            holder.transform.localPosition = position;

            var zone = holder.AddComponent<AudioReverbZone>();
            zone.reverbPreset = entry.Preset;
            zone.minDistance = entry.MinDistance;
            zone.maxDistance = entry.MaxDistance;
        }

        private static ReverbConfig LoadOrCreate(string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<ReverbConfig>(path);
            if (existing != null)
            {
                return existing;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var created = ScriptableObject.CreateInstance<ReverbConfig>();
            AssetDatabase.CreateAsset(created, path);
            return created;
        }
    }
}
