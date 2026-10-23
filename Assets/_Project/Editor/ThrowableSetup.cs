using System.IO;
using DarkDepths.Audio;
using DarkDepths.Core;
using DarkDepths.Player;
using DarkDepths.World;
using UnityEditor;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Builds the bottle of docs/03 and puts it in the player's hand: a small body that falls, is heard
    // where it lands, and pulls whatever is listening away from wherever the player is going.
    public static class ThrowableSetup
    {
        public const string Bottle = "Throwable_Bottle";

        private const string PrefabFolder = "Assets/_Project/Prefabs/Items";
        private const string PrefabPath = PrefabFolder + "/" + Bottle + ".prefab";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player.prefab";
        private const string CuePath = "Assets/_Project/Config/Audio/Impact_Bottle.asset";

        private const float BodyWidth = 0.09f;
        private const float BodyHeight = 0.2f;
        private const float NeckWidth = 0.045f;
        private const float NeckHeight = 0.08f;
        private const float Mass = 0.4f;

        [MenuItem("DarkDepths/Setup/Create Throwable")]
        public static void CreateThrowable()
        {
            LayersSetup.CreateLayers();

            GameObject prefab = BuildBottle();
            AttachToPlayer(prefab);
            AssetDatabase.SaveAssets();
        }

        private static GameObject BuildBottle()
        {
            Directory.CreateDirectory(PrefabFolder);

            PaletteConfig palette = PaletteSetup.LoadOrCreateConfig();
            // The accent colour loot carries: a bottle on a dark floor has to be findable again.
            Material material = PaletteSetup.MaterialFor(PaletteRole.Lamp, palette);

            var factory = new ModuleMeshFactory()
                .Add(new Vector3(0f, BodyHeight * 0.5f, 0f), new Vector3(BodyWidth, BodyHeight, BodyWidth))
                .Add(new Vector3(0f, BodyHeight + NeckHeight * 0.5f, 0f),
                    new Vector3(NeckWidth, NeckHeight, NeckWidth));

            var root = new GameObject(Bottle, typeof(MeshFilter), typeof(MeshRenderer));
            root.GetComponent<MeshFilter>().sharedMesh = factory.Save(Bottle);
            root.GetComponent<MeshRenderer>().sharedMaterial = material;
            root.layer = LayerMask.NameToLayer("Loot");

            // One box over the whole bottle: a thrown object needs a shape to bounce, not a silhouette.
            var collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, (BodyHeight + NeckHeight) * 0.5f, 0f);
            collider.size = new Vector3(BodyWidth, BodyHeight + NeckHeight, BodyWidth);

            var body = root.AddComponent<Rigidbody>();
            body.mass = Mass;
            // Thrown hard at a wall two metres away, a discrete step would put it through the wall.
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            root.AddComponent<ImpactNoise>();

            var audio = root.AddComponent<ImpactAudio>();
            var cue = AssetDatabase.LoadAssetAtPath<AudioCue>(CuePath);
            if (cue != null)
            {
                SerializedWiring.Wire(audio, "_cue", cue);
            }

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return saved;
        }

        private static void AttachToPlayer(GameObject bottle)
        {
            if (!File.Exists(PlayerPrefabPath))
            {
                return;
            }

            GameObject contents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);

            var thrower = contents.GetComponent<PlayerThrower>();
            if (thrower == null)
            {
                thrower = contents.AddComponent<PlayerThrower>();
            }

            SerializedWiring.Wire(thrower, "_input", contents.GetComponent<PlayerInputReader>());
            SerializedWiring.Wire(thrower, "_prefab", bottle.GetComponent<Rigidbody>());
            // Thrown from the camera, so it goes where the player is looking.
            SerializedWiring.Wire(thrower, "_origin", contents.GetComponentInChildren<Camera>(true).transform);

            PrefabUtility.SaveAsPrefabAsset(contents, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }
}
