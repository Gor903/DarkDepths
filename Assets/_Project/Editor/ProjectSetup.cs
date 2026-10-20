using System.IO;
using DarkDepths.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Creates the scenes described in docs/07_ARCHITECTURE.md; re-running keeps scenes that already exist.
    public static class ProjectSetup
    {
        private const string ScenesFolder = "Assets/_Project/Scenes";
        private const string BootScenePath = ScenesFolder + "/Boot.unity";
        private const string SandboxScenePath = ScenesFolder + "/Sandbox.unity";

        [MenuItem("DarkDepths/Setup/Create Scenes")]
        public static void CreateScenes()
        {
            Directory.CreateDirectory(ScenesFolder);

            if (!File.Exists(BootScenePath))
            {
                CreateBootScene();
            }

            if (!File.Exists(SandboxScenePath))
            {
                CreateSandboxScene();
            }

            AssetDatabase.Refresh();
            RegisterScenesInBuild();
        }

        private static void CreateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Bootstrap", typeof(Bootstrap));
            EditorSceneManager.SaveScene(scene, BootScenePath);
        }

        private static void CreateSandboxScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(2f, 1f, 2f);
            ground.isStatic = true;
            EditorSceneManager.SaveScene(scene, SandboxScenePath);
        }

        // Boot must stay at index 0 so a device build starts from it.
        private static void RegisterScenesInBuild()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootScenePath, true),
                new EditorBuildSettingsScene(SandboxScenePath, true)
            };
        }
    }
}
