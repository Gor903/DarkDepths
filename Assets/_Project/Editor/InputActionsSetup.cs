using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DarkDepths.EditorTools
{
    // Writes the Input Actions asset from code so the bindings live in version control as reviewable JSON.
    public static class InputActionsSetup
    {
        public const string AssetPath = "Assets/_Project/Config/DarkDepths.inputactions";

        // The asset is generated from this method, so hand edits in the Input Actions window are overwritten.
        [MenuItem("DarkDepths/Setup/Create Input Actions")]
        public static void CreateInputActions()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = "DarkDepths";

            InputActionMap map = asset.AddActionMap("Player");

            InputAction move = map.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            InputAction look = map.AddAction("Look", InputActionType.Value, expectedControlLayout: "Vector2");
            look.AddBinding("<Mouse>/delta");

            InputAction sprint = map.AddAction("Sprint", InputActionType.Button);
            sprint.AddBinding("<Keyboard>/leftShift");

            InputAction crouch = map.AddAction("Crouch", InputActionType.Button);
            crouch.AddBinding("<Keyboard>/c");

            InputAction flashlight = map.AddAction("Flashlight", InputActionType.Button);
            flashlight.AddBinding("<Keyboard>/f");

            InputAction interact = map.AddAction("Interact", InputActionType.Button);
            interact.AddBinding("<Keyboard>/e");

            Directory.CreateDirectory(Path.GetDirectoryName(AssetPath));
            File.WriteAllText(AssetPath, asset.ToJson());
            Object.DestroyImmediate(asset);

            AssetDatabase.ImportAsset(AssetPath);
        }
    }
}
