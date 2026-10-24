using UnityEditor;

namespace DarkDepths.EditorTools
{
    // A field added to a config after its asset was written is missing on disk and silently falls back to
    // the C# initializer. Rewriting the assets puts every field back in the file, where it can be edited.
    public static class ConfigReserialize
    {
        private const string ConfigFolder = "Assets/_Project/Config";

        [MenuItem("DarkDepths/Setup/Reserialize Configs")]
        public static void Reserialize()
        {
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { ConfigFolder });
            var paths = new string[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                paths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
            }

            AssetDatabase.ForceReserializeAssets(paths);
            AssetDatabase.SaveAssets();
        }
    }
}
