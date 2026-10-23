using DarkDepths.Core;
using DarkDepths.Loot;
using DarkDepths.Noise;
using DarkDepths.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Assembles the sandbox room, corridor and door out of the modular kit, on the grid rooms will use at T7.
    public static class TestRoomSetup
    {
        private const string SandboxScenePath = "Assets/_Project/Scenes/Sandbox.unity";
        private const string RootName = "TestRoom";

        // The room is three cells square and the corridor three cells long, measured in ModuleConfig cells.
        private const int RoomCells = 3;
        private const int CorridorCells = 3;

        // Down the corridor, facing the door: the one spot where the silhouette is read against an empty wall.
        private static readonly Vector3 EnemyPosition = new Vector3(0f, 0f, 5f);
        private const float EnemyYaw = 180f;

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
            ModuleSetup.CreateModules();
            EnemySetup.CreateEnemies();

            ModuleConfig modules = ModuleSetup.LoadOrCreateConfig();
            PaletteConfig palette = PaletteSetup.LoadOrCreateConfig();
            // Loot is what the player has to spot in the dark, so it carries the palette's warm accent.
            Material loot = PaletteSetup.MaterialFor(PaletteRole.Lamp, palette);

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);

            Replace(RootName);
            // The 20x20 template plane would stick out from under the room.
            Replace("Ground");

            var root = new GameObject(RootName);
            var geometry = new GameObject("Geometry");
            geometry.transform.SetParent(root.transform, false);

            BuildRoom(geometry.transform, modules);
            BuildCorridor(geometry.transform, modules);
            BuildDoor(root.transform, modules);
            BuildFurniture(root.transform, modules);
            BuildPickups(root.transform, loot);
            BuildEnemy(root.transform);

            PaletteSetup.ApplyRenderSettings(palette);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void BuildRoom(Transform parent, ModuleConfig modules)
        {
            float cell = modules.CellSize;
            float half = RoomCells * cell * 0.5f;
            float wallZ = half + modules.WallOffset;

            // One pass per column: the room is square, so the same offset walks both wall runs.
            for (int i = 0; i < RoomCells; i++)
            {
                float offset = Offset(i, RoomCells, cell);

                for (int j = 0; j < RoomCells; j++)
                {
                    var tile = new Vector3(offset, 0f, Offset(j, RoomCells, cell));
                    Place(ModuleSetup.Floor, parent, tile, 0f);
                    Place(ModuleSetup.Ceiling, parent, tile, 0f);
                }

                Place(ModuleSetup.Wall, parent, new Vector3(offset, 0f, -wallZ), 0f);
                // The middle segment of the north wall is where the corridor leaves the room.
                string north = i == RoomCells / 2 ? ModuleSetup.WallDoorway : ModuleSetup.Wall;
                Place(north, parent, new Vector3(offset, 0f, wallZ), 0f);

                Place(ModuleSetup.Wall, parent, new Vector3(-wallZ, 0f, offset), 90f);
                Place(ModuleSetup.Wall, parent, new Vector3(wallZ, 0f, offset), 90f);
            }
        }

        private static void BuildCorridor(Transform parent, ModuleConfig modules)
        {
            float cell = modules.CellSize;
            float start = RoomCells * cell * 0.5f;
            float wallX = cell * 0.5f + modules.WallOffset;

            for (int i = 0; i < CorridorCells; i++)
            {
                float z = start + cell * (i + 0.5f);

                // The corridor is boarded over: a second surface, so the change of footstep is audible.
                GameObject floor = Place(ModuleSetup.Floor, parent, new Vector3(0f, 0f, z), 0f);
                SerializedWiring.WireInt(floor.GetComponent<Surface>(), "_type", (int)SurfaceType.Wood);

                Place(ModuleSetup.Ceiling, parent, new Vector3(0f, 0f, z), 0f);
                Place(ModuleSetup.Wall, parent, new Vector3(-wallX, 0f, z), 90f);
                Place(ModuleSetup.Wall, parent, new Vector3(wallX, 0f, z), 90f);
            }

            float end = start + cell * CorridorCells + modules.WallOffset;
            Place(ModuleSetup.Wall, parent, new Vector3(0f, 0f, end), 0f);
        }

        private static void BuildDoor(Transform parent, ModuleConfig modules)
        {
            float wallZ = RoomCells * modules.CellSize * 0.5f + modules.WallOffset;
            Place(ModuleSetup.Door, parent, new Vector3(0f, 0f, wallZ), 0f);
        }

        private static void BuildFurniture(Transform parent, ModuleConfig modules)
        {
            var group = new GameObject("Furniture");
            group.transform.SetParent(parent, false);

            float half = RoomCells * modules.CellSize * 0.5f;

            // Against the west wall, turned to face the room: furniture is cover and a sightline breaker.
            Place(ModuleSetup.Shelf, group.transform,
                new Vector3(-half + modules.ShelfDepth * 0.5f, 0f, 0f), 90f);
            Place(ModuleSetup.Table, group.transform, new Vector3(1.4f, 0f, 0f), 0f);
            Place(ModuleSetup.Crate, group.transform, new Vector3(-1.2f, 0f, -2.2f), 25f);
        }

        private static void BuildPickups(Transform parent, Material material)
        {
            var group = new GameObject("Pickups");
            group.transform.SetParent(parent, false);

            int interactable = LayerMask.NameToLayer("Interactable");
            for (int i = 0; i < PickupNames.Length; i++)
            {
                GameObject box = Box(PickupNames[i], group.transform, PickupPositions[i], material);
                box.layer = interactable;
                box.isStatic = false;

                var pickup = box.AddComponent<Pickup>();
                SerializedWiring.WireString(pickup, "_displayName", PickupNames[i]);
            }
        }

        private static void BuildEnemy(Transform parent)
        {
            GameObject prefab = EnemySetup.LoadListener();
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.localPosition = EnemyPosition;
            instance.transform.localRotation = Quaternion.Euler(0f, EnemyYaw, 0f);
            TransformUtils.SetInspectorRotation(instance.transform, new Vector3(0f, EnemyYaw, 0f));
        }

        // Cell index to a position along a row centred on the origin.
        private static float Offset(int index, int count, float cell)
        {
            return (index - (count - 1) * 0.5f) * cell;
        }

        private static GameObject Place(string moduleName, Transform parent, Vector3 position, float yaw)
        {
            GameObject prefab = ModuleSetup.Load(moduleName);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            // Without the hint the inspector shows a rotated module as 0 degrees and fights whoever edits it.
            TransformUtils.SetInspectorRotation(instance.transform, new Vector3(0f, yaw, 0f));
            return instance;
        }

        private static GameObject Box(string name, Transform parent, Vector3 center, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = center;
            box.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
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
    }
}
