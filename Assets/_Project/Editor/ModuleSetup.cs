using System.IO;
using DarkDepths.Core;
using DarkDepths.Noise;
using DarkDepths.World;
using UnityEditor;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Builds the modular kit rooms are assembled from: deck, walls, a door and the furniture that breaks sightlines.
    public static class ModuleSetup
    {
        public const string Floor = "Module_Floor";
        public const string Ceiling = "Module_Ceiling";
        public const string Wall = "Module_Wall";
        public const string WallDoorway = "Module_Wall_Doorway";
        public const string Door = "Module_Door";
        public const string Shelf = "Module_Shelf";
        public const string Table = "Module_Table";
        public const string Crate = "Module_Crate";

        private const string ConfigFolder = "Assets/_Project/Config";
        private const string ConfigPath = ConfigFolder + "/ModuleConfig.asset";
        private const string PrefabFolder = "Assets/_Project/Prefabs/Modules";

        private const int ShelfBoardCount = 3;

        [MenuItem("DarkDepths/Setup/Create Modules")]
        public static void CreateModules()
        {
            LayersSetup.CreateLayers();

            ModuleConfig modules = LoadOrCreateConfig();
            PaletteConfig palette = PaletteSetup.LoadOrCreateConfig();
            Material structure = PaletteSetup.MaterialFor(PaletteRole.Structure, palette);
            Material prop = PaletteSetup.MaterialFor(PaletteRole.Prop, palette);

            Directory.CreateDirectory(PrefabFolder);

            BuildFloor(modules, structure);
            BuildCeiling(modules, structure);
            BuildWall(modules, structure);
            BuildWallDoorway(modules, structure);
            BuildDoor(modules, prop);
            BuildShelf(modules, prop);
            BuildTable(modules, prop);
            BuildCrate(modules, prop);

            AssetDatabase.SaveAssets();
        }

        public static ModuleConfig LoadOrCreateConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ModuleConfig>(ConfigPath);
            if (existing != null)
            {
                return existing;
            }

            Directory.CreateDirectory(ConfigFolder);
            var created = ScriptableObject.CreateInstance<ModuleConfig>();
            AssetDatabase.CreateAsset(created, ConfigPath);
            return created;
        }

        public static GameObject Load(string moduleName)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/" + moduleName + ".prefab");
        }

        private static void BuildFloor(ModuleConfig modules, Material material)
        {
            // The walkable surface is y = 0, so the slab hangs below it and rooms never need a height offset.
            var factory = new ModuleMeshFactory()
                .Add(new Vector3(0f, -modules.DeckThickness * 0.5f, 0f),
                    new Vector3(modules.CellSize, modules.DeckThickness, modules.CellSize));

            GameObject root = CreateRenderer(Floor, Floor, factory, material);
            // What the player is walking on. Concrete by default; a room overrides it per tile.
            root.AddComponent<Surface>();
            SavePrefab(root);
        }

        private static void BuildCeiling(ModuleConfig modules, Material material)
        {
            var factory = new ModuleMeshFactory()
                .Add(new Vector3(0f, modules.WallHeight + modules.DeckThickness * 0.5f, 0f),
                    new Vector3(modules.CellSize, modules.DeckThickness, modules.CellSize));

            SaveModule(Ceiling, factory, material);
        }

        private static void BuildWall(ModuleConfig modules, Material material)
        {
            var factory = new ModuleMeshFactory()
                .Add(new Vector3(0f, modules.WallHeight * 0.5f, 0f),
                    new Vector3(modules.CellSize, modules.WallHeight, modules.WallThickness));

            SaveModule(Wall, factory, material);
        }

        private static void BuildWallDoorway(ModuleConfig modules, Material material)
        {
            var opening = new WallOpening(modules.CellSize, modules.WallHeight, modules.DoorWidth, modules.DoorHeight);
            var factory = new ModuleMeshFactory();

            for (int i = 0; i < opening.Count; i++)
            {
                WallPanel panel = opening[i];
                factory.Add(new Vector3(panel.CenterX, panel.CenterY, 0f),
                    new Vector3(panel.Width, panel.Height, modules.WallThickness));
            }

            SaveModule(WallDoorway, factory, material);
        }

        private static void BuildDoor(ModuleConfig modules, Material material)
        {
            var root = new GameObject(Door);

            var pivot = new GameObject("Pivot");
            pivot.transform.SetParent(root.transform, false);
            // Hinged at the edge of the opening, so the leaf sweeps the doorway instead of its own centre.
            pivot.transform.localPosition = new Vector3(-modules.DoorWidth * 0.5f, 0f, 0f);

            var factory = new ModuleMeshFactory()
                .Add(new Vector3(modules.DoorWidth * 0.5f, modules.DoorHeight * 0.5f, 0f),
                    new Vector3(modules.DoorWidth, modules.DoorHeight, modules.DoorLeafThickness));

            GameObject leaf = CreateRenderer("Leaf", Door + "_Leaf", factory, material);
            leaf.transform.SetParent(pivot.transform, false);
            leaf.layer = LayerMask.NameToLayer("Interactable");
            leaf.isStatic = false;

            var door = root.AddComponent<DarkDepths.World.Door>();
            SerializedWiring.Wire(door, "_pivot", pivot.transform);
            SerializedWiring.Wire(door, "_leaf", leaf.GetComponent<BoxCollider>());
            // The leaf stops rather than sweeping through a body; walls are excluded or it would never leave the frame.
            SerializedWiring.WireInt(door, "_blockerMask",
                (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Enemy")));

            SavePrefab(root);
        }

        private static void BuildShelf(ModuleConfig modules, Material material)
        {
            float t = modules.BoardThickness;
            float inner = modules.ShelfWidth - t * 2f;
            var factory = new ModuleMeshFactory()
                .Add(new Vector3(-(modules.ShelfWidth - t) * 0.5f, modules.ShelfHeight * 0.5f, 0f),
                    new Vector3(t, modules.ShelfHeight, modules.ShelfDepth))
                .Add(new Vector3((modules.ShelfWidth - t) * 0.5f, modules.ShelfHeight * 0.5f, 0f),
                    new Vector3(t, modules.ShelfHeight, modules.ShelfDepth));

            for (int i = 1; i <= ShelfBoardCount; i++)
            {
                float y = modules.ShelfHeight * i / (ShelfBoardCount + 1);
                factory.Add(new Vector3(0f, y, 0f), new Vector3(inner, t, modules.ShelfDepth));
            }

            SaveModule(Shelf, factory, material);
        }

        private static void BuildTable(ModuleConfig modules, Material material)
        {
            float t = modules.BoardThickness;
            float legHeight = modules.TableHeight - t;
            float legX = (modules.TableWidth - t) * 0.5f - t;
            float legZ = (modules.TableDepth - t) * 0.5f - t;

            var factory = new ModuleMeshFactory()
                .Add(new Vector3(0f, modules.TableHeight - t * 0.5f, 0f),
                    new Vector3(modules.TableWidth, t, modules.TableDepth));

            for (int i = 0; i < 4; i++)
            {
                float x = (i & 1) == 0 ? -legX : legX;
                float z = (i & 2) == 0 ? -legZ : legZ;
                factory.Add(new Vector3(x, legHeight * 0.5f, z), new Vector3(t, legHeight, t));
            }

            SaveModule(Table, factory, material);
        }

        private static void BuildCrate(ModuleConfig modules, Material material)
        {
            float t = modules.BoardThickness;
            float body = modules.CrateSize - t;

            var factory = new ModuleMeshFactory()
                .Add(new Vector3(0f, body * 0.5f, 0f), new Vector3(modules.CrateSize, body, modules.CrateSize))
                // A lid that overhangs is what reads as a crate rather than a cube at flashlight distance.
                .Add(new Vector3(0f, body + t * 0.5f, 0f),
                    new Vector3(modules.CrateSize + t, t, modules.CrateSize + t));

            SaveModule(Crate, factory, material);
        }

        private static void SaveModule(string moduleName, ModuleMeshFactory factory, Material material)
        {
            GameObject root = CreateRenderer(moduleName, moduleName, factory, material);
            SavePrefab(root);
        }

        private static GameObject CreateRenderer(string name, string meshName, ModuleMeshFactory factory, Material material)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.GetComponent<MeshFilter>().sharedMesh = factory.Save(meshName);
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            go.layer = LayerMask.NameToLayer("Wall");
            go.isStatic = true;

            // One BoxCollider per box: cheaper than a mesh collider and exact for geometry made of boxes.
            foreach (Bounds box in factory.Boxes)
            {
                var collider = go.AddComponent<BoxCollider>();
                collider.center = box.center;
                collider.size = box.size;
            }

            return go;
        }

        private static void SavePrefab(GameObject root)
        {
            PrefabUtility.SaveAsPrefabAsset(root, PrefabFolder + "/" + root.name + ".prefab");
            Object.DestroyImmediate(root);
        }
    }
}
