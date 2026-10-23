using System.IO;
using DarkDepths.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DarkDepths.EditorTools
{
    // Builds the player prefab and drops it into Sandbox; re-running reuses whatever already exists.
    public static class PlayerSetup
    {
        private const string ConfigFolder = "Assets/_Project/Config";
        private const string PrefabFolder = "Assets/_Project/Prefabs";
        private const string PrefabPath = PrefabFolder + "/Player.prefab";
        private const string SandboxScenePath = "Assets/_Project/Scenes/Sandbox.unity";

        [MenuItem("DarkDepths/Setup/Create Player")]
        public static void CreatePlayer()
        {
            LayersSetup.CreateLayers();
            InputActionsSetup.CreateInputActions();

            var movement = LoadOrCreate<PlayerMovementConfig>(ConfigFolder + "/PlayerMovementConfig.asset");
            var look = LoadOrCreate<PlayerLookConfig>(ConfigFolder + "/PlayerLookConfig.asset");
            var flashlight = LoadOrCreate<FlashlightConfig>(ConfigFolder + "/FlashlightConfig.asset");
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsSetup.AssetPath);

            GameObject prefab = BuildPrefab(movement, look, flashlight, actions);
            // Rebuilding the prefab drops everything not created here, footsteps included.
            FootstepSetup.CreateFootsteps();
            PlaceInSandbox(prefab);

            AssetDatabase.SaveAssets();
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

        private static GameObject BuildPrefab(PlayerMovementConfig movement, PlayerLookConfig look,
            FlashlightConfig flashlightConfig, InputActionAsset actions)
        {
            Directory.CreateDirectory(PrefabFolder);

            var root = new GameObject("Player");
            var controller = root.AddComponent<CharacterController>();
            controller.radius = movement.Radius;
            controller.height = movement.StandingHeight;
            controller.center = new Vector3(0f, movement.StandingHeight * 0.5f, 0f);
            controller.slopeLimit = 45f;
            controller.stepOffset = 0.3f;
            controller.skinWidth = 0.02f;

            var pivot = new GameObject("CameraPivot");
            pivot.transform.SetParent(root.transform, false);
            pivot.transform.localPosition = new Vector3(0f, movement.StandingEyeHeight, 0f);

            var cameraGo = new GameObject("PlayerCamera", typeof(Camera), typeof(AudioListener));
            cameraGo.tag = "MainCamera";
            cameraGo.transform.SetParent(pivot.transform, false);

            // Same values PlayerLook applies at runtime, so the Scene view clips exactly like the build does.
            var cameraComponent = cameraGo.GetComponent<Camera>();
            cameraComponent.fieldOfView = look.FieldOfView;
            cameraComponent.nearClipPlane = look.NearClipPlane;
            PaletteSetup.ApplyCameraBackground(cameraComponent, PaletteSetup.LoadOrCreateConfig());
            PostProcessSetup.EnableOnCamera(cameraComponent);

            // Mounted on the camera so the beam always follows where the player is looking.
            var flashlightGo = new GameObject("Flashlight", typeof(Light));
            flashlightGo.transform.SetParent(cameraGo.transform, false);

            var reader = root.AddComponent<PlayerInputReader>();
            var fpc = root.AddComponent<FirstPersonController>();
            var lookComponent = cameraGo.AddComponent<PlayerLook>();
            var flashlight = flashlightGo.AddComponent<Flashlight>();
            var interactor = root.AddComponent<PlayerInteractor>();

            root.layer = LayerMask.NameToLayer("Player");

            Wire(reader, "_actions", actions);

            Wire(interactor, "_input", reader);
            Wire(interactor, "_camera", cameraComponent);
            // Walls are in the mask so they block the ray; a wall hit simply resolves to no interactable.
            WireInt(interactor, "_interactableMask",
                (1 << LayerMask.NameToLayer("Interactable")) | (1 << LayerMask.NameToLayer("Wall")));

            Wire(flashlight, "_config", flashlightConfig);
            Wire(flashlight, "_input", reader);
            flashlight.ApplyBeamSettings();
            flashlightGo.GetComponent<Light>().enabled = false;

            Wire(fpc, "_config", movement);
            Wire(fpc, "_input", reader);
            Wire(fpc, "_cameraPivot", pivot.transform);

            Wire(lookComponent, "_config", look);
            Wire(lookComponent, "_input", reader);
            Wire(lookComponent, "_yawTarget", root.transform);
            Wire(lookComponent, "_controller", fpc);

            // What the enemies reach the player through, and what runs out when they reach it.
            var health = root.AddComponent<PlayerHealth>();
            var target = root.AddComponent<PlayerTarget>();
            Wire(target, "_health", health);
            Wire(target, "_flashlight", flashlight);
            Wire(target, "_eye", cameraGo.transform);

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return saved;
        }

        private static void Wire(Object target, string fieldName, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(fieldName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireInt(Object target, string fieldName, int value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(fieldName).intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void PlaceInSandbox(GameObject prefab)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);

            // The template camera would fight the player camera for the MainCamera tag.
            var templateCamera = GameObject.Find("Main Camera");
            if (templateCamera != null)
            {
                Object.DestroyImmediate(templateCamera);
            }

            if (GameObject.Find("Player") == null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.position = new Vector3(0f, 0.1f, 0f);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
