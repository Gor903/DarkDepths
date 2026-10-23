using System.IO;
using DarkDepths.Core;
using DarkDepths.Player;
using DarkDepths.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DarkDepths.EditorTools
{
    // Builds the settings canvas, then wires the settings asset into the player prefab and Sandbox.
    public static class SettingsSetup
    {
        private const string ConfigFolder = "Assets/_Project/Config";
        private const string SettingsPath = ConfigFolder + "/UserSettings.asset";
        private const string InputGatePath = ConfigFolder + "/InputGate.asset";
        private const string PrefabFolder = "Assets/_Project/Prefabs/UI";
        private const string PrefabPath = PrefabFolder + "/SettingsPanel.prefab";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player.prefab";
        private const string SandboxScenePath = "Assets/_Project/Scenes/Sandbox.unity";

        private const float RowHeight = 98f;
        private const float MarkerLength = 180f;
        private const float MarkerThickness = 8f;
        private const float HandleSize = 64f;
        private const float ButtonWidth = 240f;
        private const float ButtonHeight = 88f;
        private const float CheckInset = 12f;

        private static readonly Color TextColor = new Color(0.91f, 0.85f, 0.63f);
        private static readonly Color PanelColor = new Color(0.06f, 0.06f, 0.07f, 0.94f);
        private static readonly Color BackdropColor = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color TrackColor = new Color(1f, 1f, 1f, 0.16f);
        private static readonly Color ButtonColor = new Color(1f, 1f, 1f, 0.22f);

        [MenuItem("DarkDepths/Setup/Create Settings")]
        public static void CreateSettings()
        {
            var settings = LoadOrCreate<UserSettings>(SettingsPath);
            var gate = LoadOrCreate<InputGate>(InputGatePath);

            GameObject prefab = BuildPrefab(settings, gate);
            WirePlayerPrefab(settings, gate);
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

        private static GameObject BuildPrefab(UserSettings settings, InputGate gate)
        {
            Directory.CreateDirectory(PrefabFolder);

            var root = new GameObject("SettingsPanel", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Above the touch controls so the open button and the window both take the touch first.
            canvas.sortingOrder = 200;

            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var rootRect = (RectTransform)root.transform;

            // First child, so it tints the frame but never covers the settings window itself.
            RectTransform overlayRect = CreateStretched("BrightnessOverlay", rootRect, Vector2.zero, Vector2.one);
            Image overlayImage = AddImage(overlayRect, Color.clear);
            overlayImage.raycastTarget = false;
            var overlay = overlayRect.gameObject.AddComponent<BrightnessOverlay>();
            Wire(overlay, "_settings", settings);

            // A sound that matters is shown as well as heard, on the ring just inside the safe area.
            RectTransform hintRect = CreateStretched("SoundHints", rootRect, Vector2.zero, Vector2.one);
            RectTransform marker = CreateCentered("Marker", hintRect, new Vector2(MarkerLength, MarkerThickness));
            Image markerImage = AddImage(marker, new Color(TextColor.r, TextColor.g, TextColor.b, 0f));
            markerImage.raycastTarget = false;
            var hints = hintRect.gameObject.AddComponent<SoundHintOverlay>();
            Wire(hints, "_settings", settings);
            Wire(hints, "_marker", marker);
            Wire(hints, "_graphic", markerImage);

            RectTransform safeArea = CreateStretched("SafeArea", rootRect, Vector2.zero, Vector2.one);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            RectTransform openRect = CreateRect("OpenButton", safeArea);
            openRect.anchorMin = new Vector2(1f, 1f);
            openRect.anchorMax = new Vector2(1f, 1f);
            openRect.pivot = new Vector2(1f, 1f);
            openRect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
            openRect.anchoredPosition = new Vector2(-32f, -32f);
            Button openButton = AddButton(openRect, "Settings");

            RectTransform window = CreateStretched("Window", rootRect, Vector2.zero, Vector2.one);
            AddImage(window, BackdropColor);

            RectTransform panel = CreateCentered("Panel", window, new Vector2(1040f, 940f));
            AddImage(panel, PanelColor);

            CreateLabel(panel, "Title", "Settings", 44, TextAnchor.UpperCenter,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(40f, -96f), new Vector2(-40f, -36f));

            Slider lookSlider = CreateRow(panel, 0, "Look sensitivity", out Text lookValue);
            Slider brightnessSlider = CreateRow(panel, 1, "Brightness", out Text brightnessValue);
            Toggle reduceEffectsToggle = CreateToggleRow(panel, 2, "Reduce effects");
            Slider sfxSlider = CreateRow(panel, 3, "Sound effects", out Text sfxValue);
            Slider ambientSlider = CreateRow(panel, 4, "Ambience", out Text ambientValue);
            Slider musicSlider = CreateRow(panel, 5, "Music", out Text musicValue);
            Toggle soundHintsToggle = CreateToggleRow(panel, 6, "Sound hints");

            RectTransform resetRect = CreateCorner("ResetButton", panel, new Vector2(0f, 0f));
            resetRect.anchoredPosition = new Vector2(40f, 40f);
            Button resetButton = AddButton(resetRect, "Reset");

            RectTransform closeRect = CreateCorner("CloseButton", panel, new Vector2(1f, 0f));
            closeRect.anchoredPosition = new Vector2(-40f, 40f);
            Button closeButton = AddButton(closeRect, "Close");

            var settingsPanel = root.AddComponent<SettingsPanel>();
            Wire(settingsPanel, "_settings", settings);
            Wire(settingsPanel, "_inputGate", gate);
            Wire(settingsPanel, "_window", window.gameObject);
            Wire(settingsPanel, "_openButton", openButton);
            Wire(settingsPanel, "_closeButton", closeButton);
            Wire(settingsPanel, "_resetButton", resetButton);
            Wire(settingsPanel, "_lookSensitivitySlider", lookSlider);
            Wire(settingsPanel, "_brightnessSlider", brightnessSlider);
            Wire(settingsPanel, "_reduceEffectsToggle", reduceEffectsToggle);
            Wire(settingsPanel, "_sfxVolumeSlider", sfxSlider);
            Wire(settingsPanel, "_ambientVolumeSlider", ambientSlider);
            Wire(settingsPanel, "_musicVolumeSlider", musicSlider);
            Wire(settingsPanel, "_soundHintsToggle", soundHintsToggle);
            Wire(settingsPanel, "_lookSensitivityValue", lookValue);
            Wire(settingsPanel, "_brightnessValue", brightnessValue);
            Wire(settingsPanel, "_sfxVolumeValue", sfxValue);
            Wire(settingsPanel, "_ambientVolumeValue", ambientValue);
            Wire(settingsPanel, "_musicVolumeValue", musicValue);

            // Saved closed; SettingsPanel reopens it on demand and the scene view stays readable.
            window.gameObject.SetActive(false);

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return saved;
        }

        // Wired on the prefab asset so every scene that uses the player picks the settings up.
        private static void WirePlayerPrefab(UserSettings settings, InputGate gate)
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);

            var look = contents.GetComponentInChildren<PlayerLook>(true);
            if (look != null)
            {
                Wire(look, "_settings", settings);
            }

            var reader = contents.GetComponentInChildren<PlayerInputReader>(true);
            if (reader != null)
            {
                Wire(reader, "_inputGate", gate);
            }

            PrefabUtility.SaveAsPrefabAsset(contents, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(contents);
        }

        private static void PlaceInSandbox(GameObject prefab)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);

            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            }

            if (Object.FindAnyObjectByType<SettingsPanel>() == null)
            {
                PrefabUtility.InstantiatePrefab(prefab);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static Slider CreateRow(RectTransform panel, int index, string label, out Text value)
        {
            RectTransform row = CreateRowRect(panel, index, label);

            value = CreateLabel(row, "Value", "100%", 34, TextAnchor.MiddleRight,
                new Vector2(0.86f, 0f), Vector2.one, Vector2.zero, Vector2.zero);

            return CreateSlider(row);
        }

        private static Toggle CreateToggleRow(RectTransform panel, int index, string label)
        {
            return CreateToggle(CreateRowRect(panel, index, label));
        }

        private static RectTransform CreateRowRect(RectTransform panel, int index, string label)
        {
            float top = -150f - index * RowHeight;

            RectTransform row = CreateRect("Row_" + label.Replace(" ", string.Empty), panel);
            row.anchorMin = new Vector2(0f, 1f);
            row.anchorMax = new Vector2(1f, 1f);
            row.pivot = new Vector2(0.5f, 1f);
            row.offsetMin = new Vector2(40f, 0f);
            row.offsetMax = new Vector2(-40f, 0f);
            row.sizeDelta = new Vector2(row.sizeDelta.x, RowHeight);
            row.anchoredPosition = new Vector2(0f, top);

            CreateLabel(row, "Label", label, 34, TextAnchor.MiddleLeft,
                Vector2.zero, new Vector2(0.4f, 1f), Vector2.zero, Vector2.zero);

            return row;
        }

        private static Slider CreateSlider(RectTransform row)
        {
            RectTransform rect = CreateRect("Slider", row);
            rect.anchorMin = new Vector2(0.42f, 0.5f);
            rect.anchorMax = new Vector2(0.82f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(0f, -HandleSize * 0.5f);
            rect.offsetMax = new Vector2(0f, HandleSize * 0.5f);

            RectTransform background = CreateStretched("Background", rect, new Vector2(0f, 0.35f), new Vector2(1f, 0.65f));
            AddImage(background, TrackColor);

            RectTransform fillArea = CreateStretched("Fill Area", rect, new Vector2(0f, 0.35f), new Vector2(1f, 0.65f));
            fillArea.offsetMin = new Vector2(HandleSize * 0.5f, 0f);
            fillArea.offsetMax = new Vector2(-HandleSize * 0.5f, 0f);
            RectTransform fill = CreateStretched("Fill", fillArea, Vector2.zero, Vector2.one);
            Image fillImage = AddImage(fill, new Color(TextColor.r, TextColor.g, TextColor.b, 0.55f));

            RectTransform handleArea = CreateStretched("Handle Slide Area", rect, Vector2.zero, Vector2.one);
            handleArea.offsetMin = new Vector2(HandleSize * 0.5f, 0f);
            handleArea.offsetMax = new Vector2(-HandleSize * 0.5f, 0f);
            RectTransform handle = CreateStretched("Handle", handleArea, Vector2.zero, Vector2.one);
            handle.sizeDelta = new Vector2(HandleSize, 0f);
            Image handleImage = AddImage(handle, new Color(TextColor.r, TextColor.g, TextColor.b, 0.95f));

            var slider = rect.gameObject.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.transition = Selectable.Transition.ColorTint;
            // Anchors on fill and handle are driven by the slider itself, so only the images are set here.
            fillImage.raycastTarget = false;
            return slider;
        }

        // A box with a filled centre: the same reach as a slider handle, and readable without a glyph font.
        private static Toggle CreateToggle(RectTransform row)
        {
            RectTransform rect = CreateRect("Toggle", row);
            rect.anchorMin = new Vector2(0.42f, 0.5f);
            rect.anchorMax = new Vector2(0.42f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(HandleSize, HandleSize);

            Image background = AddImage(rect, TrackColor);

            RectTransform checkRect = CreateStretched("Checkmark", rect, Vector2.zero, Vector2.one);
            checkRect.offsetMin = new Vector2(CheckInset, CheckInset);
            checkRect.offsetMax = new Vector2(-CheckInset, -CheckInset);
            Image check = AddImage(checkRect, new Color(TextColor.r, TextColor.g, TextColor.b, 0.95f));
            check.raycastTarget = false;

            var toggle = rect.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = background;
            toggle.graphic = check;
            toggle.transition = Selectable.Transition.ColorTint;
            return toggle;
        }

        private static Button AddButton(RectTransform rect, string label)
        {
            Image image = AddImage(rect, ButtonColor);
            CreateLabel(rect, "Label", label, 32, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            return button;
        }

        private static Text CreateLabel(RectTransform parent, string name, string text, int fontSize,
            TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var label = rect.gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = TextColor;
            label.raycastTarget = false;
            label.text = text;
            return label;
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

        private static RectTransform CreateCentered(string name, RectTransform parent, Vector2 size)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            return rect;
        }

        private static RectTransform CreateCorner(string name, RectTransform parent, Vector2 corner)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = corner;
            rect.anchorMax = corner;
            rect.pivot = corner;
            rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
            return rect;
        }

        private static RectTransform CreateRect(string name, RectTransform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private static Image AddImage(RectTransform rect, Color color)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static void Wire(Object target, string fieldName, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(fieldName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
