using System.IO;
using DarkDepths.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DarkDepths.EditorTools
{
    // Builds the on-screen control canvas and wires it into the player in Sandbox.
    public static class TouchControlsSetup
    {
        private const string ConfigPath = "Assets/_Project/Config/TouchControlsConfig.asset";
        private const string PrefabFolder = "Assets/_Project/Prefabs/UI";
        private const string PrefabPath = PrefabFolder + "/TouchControls.prefab";
        private const string SandboxScenePath = "Assets/_Project/Scenes/Sandbox.unity";

        [MenuItem("DarkDepths/Setup/Create Touch Controls")]
        public static void CreateTouchControls()
        {
            TouchControlsConfig config = LoadOrCreateConfig();
            GameObject prefab = BuildPrefab(config);
            PlaceInSandbox(prefab);
            AssetDatabase.SaveAssets();
        }

        private static TouchControlsConfig LoadOrCreateConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<TouchControlsConfig>(ConfigPath);
            if (existing != null)
            {
                return existing;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
            var created = ScriptableObject.CreateInstance<TouchControlsConfig>();
            AssetDatabase.CreateAsset(created, ConfigPath);
            return created;
        }

        private static GameObject BuildPrefab(TouchControlsConfig config)
        {
            Directory.CreateDirectory(PrefabFolder);

            var root = new GameObject("TouchControls", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform safeArea = CreateStretched("SafeArea", (RectTransform)root.transform, Vector2.zero, Vector2.one);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            RectTransform moveSide = CreateStretched("MoveSide", safeArea, Vector2.zero, new Vector2(0.5f, 1f));
            AddInvisibleRaycastTarget(moveSide);
            var joystick = moveSide.gameObject.AddComponent<VirtualJoystick>();

            RectTransform stick = CreateCentered("Stick", moveSide, config.JoystickRadius * 2f);
            AddImage(stick, new Color(1f, 1f, 1f, 0.16f));
            RectTransform handle = CreateCentered("Handle", stick, config.JoystickRadius * 0.9f);
            Image handleImage = AddImage(handle, config.HandleColor);

            RectTransform lookSide = CreateStretched("LookSide", safeArea, new Vector2(0.5f, 0f), Vector2.one);
            AddInvisibleRaycastTarget(lookSide);
            var lookArea = lookSide.gameObject.AddComponent<LookArea>();

            // Buttons come last so they win the raycast where they overlap the move and look zones.
            RectTransform crouch = CreateCorner("CrouchButton", safeArea, new Vector2(1f, 0f), config.ButtonSize);
            AddImage(crouch, new Color(1f, 1f, 1f, 0.22f));
            var crouchButton = crouch.gameObject.AddComponent<TouchButton>();

            RectTransform flashlight = CreateCorner("FlashlightButton", safeArea, new Vector2(1f, 0f), config.ButtonSize);
            AddImage(flashlight, new Color(1f, 1f, 1f, 0.22f));
            var flashlightButton = flashlight.gameObject.AddComponent<TouchButton>();

            RectTransform interact = CreateCorner("InteractButton", safeArea, new Vector2(1f, 0f), config.ButtonSize);
            AddImage(interact, new Color(1f, 1f, 1f, 0.3f));
            var interactButton = interact.gameObject.AddComponent<TouchButton>();

            Text promptLabel = CreatePromptLabel(safeArea);

            Wire(joystick, "_stick", stick);
            Wire(joystick, "_handle", handle);
            Wire(joystick, "_handleImage", handleImage);

            var controls = root.AddComponent<TouchControls>();
            Wire(controls, "_config", config);
            Wire(controls, "_controlsRoot", safeArea.gameObject);
            Wire(controls, "_moveStick", joystick);
            Wire(controls, "_lookArea", lookArea);
            Wire(controls, "_crouchButton", crouchButton);
            Wire(controls, "_flashlightButton", flashlightButton);
            Wire(controls, "_interactButton", interactButton);
            Wire(controls, "_promptLabel", promptLabel);
            WireArray(controls, "_mirrorTargets", new Object[] { moveSide, lookSide, crouch, flashlight, interact });

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return saved;
        }

        private static void PlaceInSandbox(GameObject prefab)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);

            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            }

            var controls = Object.FindAnyObjectByType<TouchControls>();
            if (controls == null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                controls = instance.GetComponent<TouchControls>();
            }

            var reader = Object.FindAnyObjectByType<PlayerInputReader>();
            if (reader != null)
            {
                Wire(reader, "_touch", controls);
                PrefabUtility.RecordPrefabInstancePropertyModifications(reader);
            }

            // The label lives in the canvas prefab and the interactor in the player prefab,
            // so this reference can only be made on the scene instances.
            var interactor = Object.FindAnyObjectByType<PlayerInteractor>();
            if (interactor != null && controls.PromptLabel != null)
            {
                Wire(interactor, "_promptLabel", controls.PromptLabel);
                PrefabUtility.RecordPrefabInstancePropertyModifications(interactor);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static RectTransform CreateStretched(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static RectTransform CreateCentered(string name, RectTransform parent, float size)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = Vector2.zero;
            return rect;
        }

        private static RectTransform CreateCorner(string name, RectTransform parent, Vector2 corner, float size)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = corner;
            rect.anchorMax = corner;
            rect.pivot = corner;
            rect.sizeDelta = new Vector2(size, size);
            return rect;
        }

        private static RectTransform CreateRect(string name, RectTransform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        // Sits just under the crosshair and never takes raycasts, so it cannot block the look area.
        private static Text CreatePromptLabel(RectTransform parent)
        {
            RectTransform rect = CreateRect("InteractionPrompt", parent);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(700f, 60f);
            rect.anchoredPosition = new Vector2(0f, -40f);

            var label = rect.gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 34;
            label.alignment = TextAnchor.UpperCenter;
            label.color = new Color(0.91f, 0.85f, 0.63f);
            label.raycastTarget = false;
            label.text = string.Empty;
            return label;
        }

        private static Image AddImage(RectTransform rect, Color color)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static void AddInvisibleRaycastTarget(RectTransform rect)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;
        }

        private static void Wire(Object target, string fieldName, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(fieldName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireArray(Object target, string fieldName, Object[] values)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
