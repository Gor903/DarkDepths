using System.IO;
using DarkDepths.Player;
using UnityEditor;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Adds the bounce light to the player that already exists, so the prefab is patched and not rebuilt.
    public static class FlashlightBounceSetup
    {
        private const string ConfigPath = "Assets/_Project/Config/FlashlightConfig.asset";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player.prefab";
        private const string FlashlightName = "Flashlight";

        [MenuItem("DarkDepths/Setup/Create Flashlight Bounce")]
        public static void CreateFlashlightBounce()
        {
            // The hit mask is built from a layer name, so the layers have to exist first.
            LayersSetup.CreateLayers();

            var config = AssetDatabase.LoadAssetAtPath<FlashlightConfig>(ConfigPath);
            if (config == null)
            {
                Debug.LogError("FlashlightConfig not found at " + ConfigPath);
                return;
            }

            if (!File.Exists(PlayerPrefabPath))
            {
                Debug.LogError("Player prefab not found at " + PlayerPrefabPath);
                return;
            }

            GameObject contents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);

            Transform flashlight = Find(contents.transform, FlashlightName);
            if (flashlight == null)
            {
                PrefabUtility.UnloadPrefabContents(contents);
                Debug.LogError("No '" + FlashlightName + "' under the player prefab.");
                return;
            }

            var bounce = flashlight.GetComponent<FlashlightBounce>();
            if (bounce == null)
            {
                bounce = flashlight.gameObject.AddComponent<FlashlightBounce>();
            }

            SerializedWiring.Wire(bounce, "_config", config);
            // Everything but the player: the ray starts inside the capsule it would otherwise hit first.
            SerializedWiring.WireInt(bounce, "_hitMask", ~(1 << LayerMask.NameToLayer("Player")));

            PrefabUtility.SaveAsPrefabAsset(contents, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(contents);

            // The asset holds only fields it has seen before; the new bounce block needs a rewrite.
            AssetDatabase.ForceReserializeAssets(new[] { ConfigPath });
            AssetDatabase.SaveAssets();

            Debug.Log("Flashlight bounce attached to " + PlayerPrefabPath);
        }

        private static Transform Find(Transform root, string name)
        {
            if (root.name == name)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = Find(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
