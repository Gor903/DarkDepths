using System.IO;
using DarkDepths.Loot;
using DarkDepths.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace DarkDepths.EditorTools
{
    // Primitive room, corridor, door and five pickups for stage 1; real geometry arrives at T2.2.
    public static class TestRoomSetup
    {
        private const string SandboxScenePath = "Assets/_Project/Scenes/Sandbox.unity";
        private const string MaterialFolder = "Assets/_Project/Art/Materials";
        private const string RootName = "TestRoom";

        private const float WallHeight = 3f;
        private const float WallThickness = 0.2f;
        private const float DoorWidth = 1.2f;
        private const float DoorHeight = 2.2f;

        private static readonly string[] PickupNames = { "Battery", "Medkit", "Bottle", "Baton", "Loot" };

        private static readonly Vector3[] PickupPositions =
        {
            new Vector3(-2f, 0.15f, -2f),
            new Vector3(2f, 0.15f, -2f),
            new Vector3(-2f, 0.15f, 2f),
            new Vector3(2f, 0.15f, 2f),
            new Vector3(0f, 0.15f, 0.6f)
        };

        [MenuItem("DarkDepths/Setup/Create Test Room")]
        public static void CreateTestRoom()
        {
            LayersSetup.CreateLayers();

            Material structure = LoadOrCreateMaterial("Placeholder_Structure", "#1B2028");
            Material prop = LoadOrCreateMaterial("Placeholder_Prop", "#3A4656");

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);

            Replace(RootName);
            // The 20x20 template plane would stick out from under the room.
            Replace("Ground");

            var root = new GameObject(RootName);
            BuildRoom(root.transform, structure);
            BuildCorridor(root.transform, structure);
            BuildDoor(root.transform, prop);
            BuildPickups(root.transform, prop);

            ApplyDarkLighting();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void BuildRoom(Transform parent, Material material)
        {
            const float half = 3f;
            float mid = WallHeight * 0.5f;

            Box("Room_Floor", parent, new Vector3(0f, -WallThickness * 0.5f, 0f), new Vector3(6f, WallThickness, 6f), material);
            Box("Room_Ceiling", parent, new Vector3(0f, WallHeight + WallThickness * 0.5f, 0f), new Vector3(6f, WallThickness, 6f), material);
            Box("Room_Wall_West", parent, new Vector3(-half - WallThickness * 0.5f, mid, 0f), new Vector3(WallThickness, WallHeight, 6.4f), material);
            Box("Room_Wall_East", parent, new Vector3(half + WallThickness * 0.5f, mid, 0f), new Vector3(WallThickness, WallHeight, 6.4f), material);
            Box("Room_Wall_South", parent, new Vector3(0f, mid, -half - WallThickness * 0.5f), new Vector3(6f, WallHeight, WallThickness), material);

            // North wall is split around the doorway.
            float sideWidth = (6f - DoorWidth) * 0.5f;
            float sideOffset = DoorWidth * 0.5f + sideWidth * 0.5f;
            float northZ = half + WallThickness * 0.5f;
            Box("Room_Wall_North_Left", parent, new Vector3(-sideOffset, mid, northZ), new Vector3(sideWidth, WallHeight, WallThickness), material);
            Box("Room_Wall_North_Right", parent, new Vector3(sideOffset, mid, northZ), new Vector3(sideWidth, WallHeight, WallThickness), material);
            Box("Room_Wall_North_Lintel", parent,
                new Vector3(0f, DoorHeight + (WallHeight - DoorHeight) * 0.5f, northZ),
                new Vector3(DoorWidth, WallHeight - DoorHeight, WallThickness), material);
        }

        private static void BuildCorridor(Transform parent, Material material)
        {
            const float startZ = 3.2f;
            const float endZ = 9f;
            float centerZ = (startZ + endZ) * 0.5f;
            float length = endZ - startZ;
            float mid = WallHeight * 0.5f;

            Box("Corridor_Floor", parent, new Vector3(0f, -WallThickness * 0.5f, centerZ), new Vector3(2f, WallThickness, length), material);
            Box("Corridor_Ceiling", parent, new Vector3(0f, WallHeight + WallThickness * 0.5f, centerZ), new Vector3(2f, WallThickness, length), material);
            Box("Corridor_Wall_West", parent, new Vector3(-1f - WallThickness * 0.5f, mid, centerZ), new Vector3(WallThickness, WallHeight, length), material);
            Box("Corridor_Wall_East", parent, new Vector3(1f + WallThickness * 0.5f, mid, centerZ), new Vector3(WallThickness, WallHeight, length), material);
            Box("Corridor_Wall_End", parent, new Vector3(0f, mid, endZ + WallThickness * 0.5f), new Vector3(2.4f, WallHeight, WallThickness), material);
        }

        private static void BuildDoor(Transform parent, Material material)
        {
            var door = new GameObject("Door");
            door.transform.SetParent(parent, false);

            var pivot = new GameObject("DoorPivot");
            pivot.transform.SetParent(door.transform, false);
            pivot.transform.localPosition = new Vector3(-DoorWidth * 0.5f, 0f, 3.1f);

            GameObject leaf = Box("DoorLeaf", pivot.transform,
                new Vector3(DoorWidth * 0.5f, DoorHeight * 0.5f, 0f),
                new Vector3(DoorWidth, DoorHeight, 0.08f), material);
            leaf.layer = LayerMask.NameToLayer("Interactable");
            leaf.isStatic = false;

            var component = door.AddComponent<Door>();
            Wire(component, "_pivot", pivot.transform);
        }

        private static void BuildPickups(Transform parent, Material material)
        {
            var group = new GameObject("Pickups");
            group.transform.SetParent(parent, false);

            int interactable = LayerMask.NameToLayer("Interactable");
            for (int i = 0; i < PickupNames.Length; i++)
            {
                GameObject box = Box(PickupNames[i], group.transform, PickupPositions[i], new Vector3(0.3f, 0.3f, 0.3f), material);
                box.layer = interactable;
                box.isStatic = false;

                var pickup = box.AddComponent<Pickup>();
                WireString(pickup, "_displayName", PickupNames[i]);
            }
        }

        // Stage 1 stopgap so the flashlight has darkness to work against; T2.1 owns real lighting.
        private static void ApplyDarkLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.035f, 0.042f, 0.055f);
            RenderSettings.fog = false;

            var sun = Object.FindAnyObjectByType<UnityEngine.Light>();
            if (sun != null && sun.type == LightType.Directional)
            {
                sun.intensity = 0.05f;
                sun.shadows = LightShadows.Soft;
            }
        }

        private static GameObject Box(string name, Transform parent, Vector3 center, Vector3 size, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = center;
            box.transform.localScale = size;
            box.layer = LayerMask.NameToLayer("Wall");
            box.isStatic = true;
            box.GetComponent<MeshRenderer>().sharedMaterial = material;
            return box;
        }

        private static Material LoadOrCreateMaterial(string name, string hex)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            Directory.CreateDirectory(MaterialFolder);
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                material.color = color;
            }

            material.SetFloat("_Smoothness", 0.05f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void Replace(string objectName)
        {
            var existing = GameObject.Find(objectName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }

        private static void Wire(Object target, string fieldName, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(fieldName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireString(Object target, string fieldName, string value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(fieldName).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
