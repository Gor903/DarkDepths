using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace DarkDepths.EditorTools
{
    // Bakes the ground the enemies walk on. The room is built from colliders on the Wall layer, so the
    // surface is collected from those: floors become walkable, walls and furniture do not.
    public static class NavigationSetup
    {
        private const string SandboxScenePath = "Assets/_Project/Scenes/Sandbox.unity";
        private const string DataFolder = "Assets/_Project/Scenes";
        private const string DataPath = DataFolder + "/Sandbox_NavMesh.asset";
        private const string SurfaceName = "Navigation";

        [MenuItem("DarkDepths/Setup/Create Navigation")]
        public static void CreateNavigation()
        {
            LayersSetup.CreateLayers();

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);

            var existing = GameObject.Find(SurfaceName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            var root = new GameObject(SurfaceName);
            var surface = root.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = 1 << LayerMask.NameToLayer("Wall");
            surface.BuildNavMesh();

            Persist(surface);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        // A mesh built from a script lives in memory; without an asset it is gone on the next load.
        private static void Persist(NavMeshSurface surface)
        {
            if (surface.navMeshData == null)
            {
                Debug.LogError("The NavMesh came out empty; nothing walkable was collected.");
                return;
            }

            Directory.CreateDirectory(DataFolder);

            var stored = AssetDatabase.LoadAssetAtPath<NavMeshData>(DataPath);
            if (stored == null)
            {
                AssetDatabase.CreateAsset(surface.navMeshData, DataPath);
            }
            else
            {
                EditorUtility.CopySerialized(surface.navMeshData, stored);
                surface.navMeshData = stored;
            }

            EditorUtility.SetDirty(surface);
        }
    }
}
