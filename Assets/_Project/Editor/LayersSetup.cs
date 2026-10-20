using UnityEditor;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Creates exactly the layers listed in docs/07_ARCHITECTURE.md, nothing else.
    public static class LayersSetup
    {
        private static readonly string[] RequiredLayers =
        {
            "Player",
            "Enemy",
            "Wall",
            "Interactable",
            "RoomBounds",
            "Loot"
        };

        [MenuItem("DarkDepths/Setup/Create Layers")]
        public static void CreateLayers()
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets.Length == 0)
            {
                Debug.LogError("TagManager.asset not found.");
                return;
            }

            var tagManager = new SerializedObject(assets[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            for (int i = 0; i < RequiredLayers.Length; i++)
            {
                EnsureLayer(layers, RequiredLayers[i]);
            }

            tagManager.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }

        private static void EnsureLayer(SerializedProperty layers, string layerName)
        {
            for (int i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
                {
                    return;
                }
            }

            // Slots 0 to 7 belong to Unity.
            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(slot.stringValue))
                {
                    continue;
                }

                slot.stringValue = layerName;
                return;
            }

            Debug.LogError($"No free user layer slot left for '{layerName}'.");
        }
    }
}
