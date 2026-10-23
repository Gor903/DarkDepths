using System.IO;
using DarkDepths.Audio;
using DarkDepths.Loot;
using DarkDepths.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Builds the rest of the sound docs/04 asks for around the player: the background a room has,
    // the two creaks and the latch of a door, and the sound of taking something.
    public static class RoomAudioSetup
    {
        private const string AmbientFolder = "Assets/_Project/Audio/Ambient";
        private const string DoorFolder = "Assets/_Project/Audio/SFX/Doors";
        private const string ItemFolder = "Assets/_Project/Audio/SFX/Items";
        private const string CueFolder = "Assets/_Project/Config/Audio";

        private const string SandboxScenePath = "Assets/_Project/Scenes/Sandbox.unity";
        private const string DoorPrefabPath = "Assets/_Project/Prefabs/Modules/Module_Door.prefab";

        private const string RoomTone = "Ambience_RoomTone";
        private const string Vent = "Ambience_Vent";
        private const string DoorOpen = "Door_Open";
        private const string DoorClose = "Door_Close";
        private const string DoorLatch = "Door_Latch";
        private const string PickupTake = "Pickup_Take";
        private const string BottleImpact = "Impact_Bottle";

        private const string AmbienceRoot = "Ambience";

        // Down the corridor, four cells out: far enough that the hiss tells the player which way it runs.
        private const float VentCells = 4f;

        [MenuItem("DarkDepths/Setup/Create Room Audio")]
        public static void CreateRoomAudio()
        {
            BuildCues();
            AssetDatabase.SaveAssets();

            AttachToDoorPrefab();
            AssetDatabase.SaveAssets();

            ApplyToScene();
            AssetDatabase.SaveAssets();
        }

        private static void BuildCues()
        {
            // The room tone is wide and the vent is a point: one is the floor of the mix, the other a direction.
            BuildLoop(RoomTone, AmbienceClipFactory.CreateRoomTone(AmbientFolder), 0.35f, 0.35f, 6f, 26f);
            BuildLoop(Vent, AmbienceClipFactory.CreateVent(AmbientFolder), 0.5f, 1f, 1.5f, 14f);

            // A door carries across a room, so it is one of the sounds the screen has to show as well.
            BuildOneShot(DoorOpen, InteractionClipFactory.CreateDoorOpen(DoorFolder), 110, 1.5f, 22f, true);
            BuildOneShot(DoorClose, InteractionClipFactory.CreateDoorClose(DoorFolder), 110, 1.5f, 22f, true);
            BuildOneShot(DoorLatch, InteractionClipFactory.CreateDoorLatch(DoorFolder), 110, 1.5f, 18f, true);
            BuildOneShot(PickupTake, InteractionClipFactory.CreatePickup(ItemFolder), 120, 1f, 12f, false);
            // A bottle thrown to pull something away has to be heard, and shown, from across a room.
            BuildOneShot(BottleImpact, InteractionClipFactory.CreateBottleImpact(ItemFolder), 100, 1.5f, 26f, true);
        }

        private static AudioCue BuildLoop(string cueName, string clipPath, float volume, float spatialBlend,
            float minDistance, float maxDistance)
        {
            AudioCue cue = Cue(cueName, new[] { clipPath });
            SerializedWiring.WireInt(cue, "_channel", (int)AudioChannel.Ambient);
            SerializedWiring.WireBool(cue, "_loop", true);
            // A background that is re-rolled every time it restarts would be heard restarting.
            SerializedWiring.WireFloat(cue, "_volumeMin", volume);
            SerializedWiring.WireFloat(cue, "_volumeMax", volume);
            SerializedWiring.WireFloat(cue, "_pitchMin", 1f);
            SerializedWiring.WireFloat(cue, "_pitchMax", 1f);
            // Above the one-shots: a loop that loses its voice leaves a hole in the room, not a missed step.
            SerializedWiring.WireInt(cue, "_priority", 90);
            SerializedWiring.WireFloat(cue, "_spatialBlend", spatialBlend);
            SerializedWiring.WireFloat(cue, "_minDistance", minDistance);
            SerializedWiring.WireFloat(cue, "_maxDistance", maxDistance);

            EditorUtility.SetDirty(cue);
            return cue;
        }

        private static AudioCue BuildOneShot(string cueName, string[] clipPaths, int priority,
            float minDistance, float maxDistance, bool hints)
        {
            AudioCue cue = Cue(cueName, clipPaths);
            SerializedWiring.WireInt(cue, "_channel", (int)AudioChannel.Sfx);
            SerializedWiring.WireBool(cue, "_loop", false);
            SerializedWiring.WireFloat(cue, "_volumeMin", 0.85f);
            SerializedWiring.WireFloat(cue, "_volumeMax", 1f);
            SerializedWiring.WireFloat(cue, "_pitchMin", 0.95f);
            SerializedWiring.WireFloat(cue, "_pitchMax", 1.05f);
            SerializedWiring.WireInt(cue, "_priority", priority);
            SerializedWiring.WireBool(cue, "_hints", hints);
            SerializedWiring.WireFloat(cue, "_spatialBlend", 1f);
            SerializedWiring.WireFloat(cue, "_minDistance", minDistance);
            SerializedWiring.WireFloat(cue, "_maxDistance", maxDistance);

            EditorUtility.SetDirty(cue);
            return cue;
        }

        private static AudioCue Cue(string cueName, string[] clipPaths)
        {
            var clips = new Object[clipPaths.Length];
            for (int i = 0; i < clipPaths.Length; i++)
            {
                AssetDatabase.ImportAsset(clipPaths[i], ImportAssetOptions.ForceUpdate);
                AudioImport.Apply(clipPaths[i]);
                clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPaths[i]);
            }

            AudioCue cue = LoadOrCreate<AudioCue>(CueFolder + "/" + cueName + ".asset");
            SerializedWiring.WireArray(cue, "_clips", clips);
            return cue;
        }

        private static void AttachToDoorPrefab()
        {
            if (!File.Exists(DoorPrefabPath))
            {
                return;
            }

            GameObject contents = PrefabUtility.LoadPrefabContents(DoorPrefabPath);

            var audio = contents.GetComponent<DoorAudio>();
            if (audio == null)
            {
                audio = contents.AddComponent<DoorAudio>();
            }

            SerializedWiring.Wire(audio, "_openCue", LoadCue(DoorOpen));
            SerializedWiring.Wire(audio, "_closeCue", LoadCue(DoorClose));
            SerializedWiring.Wire(audio, "_latchCue", LoadCue(DoorLatch));
            // The leaf, so a door is heard where it stands rather than at the root of its prefab.
            SerializedWiring.Wire(audio, "_source", contents.transform.Find("Pivot/Leaf"));

            PrefabUtility.SaveAsPrefabAsset(contents, DoorPrefabPath);
            PrefabUtility.UnloadPrefabContents(contents);
        }

        private static void ApplyToScene()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);

            // Loaded after the scene opens, like the other setup tools: opening a scene drops unreferenced assets.
            ModuleConfig modules = ModuleSetup.LoadOrCreateConfig();
            AudioCue pickupCue = LoadCue(PickupTake);

            PlaceAmbience(modules);
            WirePickups(pickupCue);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void PlaceAmbience(ModuleConfig modules)
        {
            var existing = GameObject.Find(AmbienceRoot);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            var root = new GameObject(AmbienceRoot);

            // Up under the ceiling, where a room's hum and a vent actually sit.
            Place(root.transform, "RoomTone", LoadCue(RoomTone),
                new Vector3(0f, modules.WallHeight * 0.65f, 0f));
            Place(root.transform, "Vent", LoadCue(Vent),
                new Vector3(0f, modules.WallHeight - 0.4f, modules.CellSize * VentCells));
        }

        private static void Place(Transform parent, string emitterName, AudioCue cue, Vector3 position)
        {
            var holder = new GameObject(emitterName);
            holder.transform.SetParent(parent, false);
            holder.transform.localPosition = position;

            var emitter = holder.AddComponent<AmbienceEmitter>();
            SerializedWiring.Wire(emitter, "_cue", cue);
        }

        private static void WirePickups(AudioCue cue)
        {
            Pickup[] pickups = Object.FindObjectsByType<Pickup>(FindObjectsInactive.Include);
            for (int i = 0; i < pickups.Length; i++)
            {
                var audio = pickups[i].GetComponent<PickupAudio>();
                if (audio == null)
                {
                    audio = pickups[i].gameObject.AddComponent<PickupAudio>();
                }

                SerializedWiring.Wire(audio, "_cue", cue);
            }
        }

        private static AudioCue LoadCue(string cueName)
        {
            return AssetDatabase.LoadAssetAtPath<AudioCue>(CueFolder + "/" + cueName + ".asset");
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
    }
}
