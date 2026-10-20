using DarkDepths.Core;
using DarkDepths.Loot;
using DarkDepths.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Primitive room, corridor, door and five pickups for stage 1; real geometry arrives at T2.2.
    public static class TestRoomSetup
    {
        private const string SandboxScenePath = "Assets/_Project/Scenes/Sandbox.unity";
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

            PaletteConfig palette = PaletteSetup.LoadOrCreateConfig();
            Material structure = PaletteSetup.MaterialFor(PaletteRole.Structure, palette);
            Material prop = PaletteSetup.MaterialFor(PaletteRole.Prop, palette);
            // Loot is what the player has to spot in the dark, so it carries the palette's warm accent.
            Material loot = PaletteSetup.MaterialFor(PaletteRole.Lamp, palette);

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);

            Replace(RootName);
            // The 20x20 template plane would stick out from under the room.
            Replace("Ground");

            var root = new GameObject(RootName);
            BuildRoom(root.transform, structure);
            BuildCorridor(root.transform, structure);
            BuildDoor(root.transform, prop);
            BuildPickups(root.transform, loot);

            PaletteSetup.ApplyRenderSettings(palette);

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

            // Floor and ceiling reach back under the doorway; the gap there was a slot straight out of the level.
            const float deckStartZ = 2.9f;
            float deckCenterZ = (deckStartZ + endZ) * 0.5f;
            float deckLength = endZ - deckStartZ;

            Box("Corridor_Floor", parent, new Vector3(0f, -WallThickness * 0.5f, deckCenterZ), new Vector3(2f, WallThickness, deckLength), material);
            Box("Corridor_Ceiling", parent, new Vector3(0f, WallHeight + WallThickness * 0.5f, deckCenterZ), new Vector3(2f, WallThickness, deckLength), material);
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
            Wire(component, "_leaf", leaf.GetComponent<BoxCollider>());
            // The leaf stops rather than sweeping through a body; walls are excluded or it would never leave the frame.
            WireInt(component, "_blockerMask",
                (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Enemy")));
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

        private static void WireInt(Object target, string fieldName, int value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(fieldName).intValue = value;
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
