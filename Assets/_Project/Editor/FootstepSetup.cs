using System.IO;
using DarkDepths.Audio;
using DarkDepths.Noise;
using DarkDepths.Player;
using UnityEditor;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Builds the footstep side of docs/04: a cue per surface, the table that maps one to the other,
    // and the component on the player that plays them.
    public static class FootstepSetup
    {
        private const string ClipFolder = "Assets/_Project/Audio/SFX/Footsteps";
        private const string CueFolder = "Assets/_Project/Config/Audio";
        private const string ConfigFolder = "Assets/_Project/Config";
        private const string SetPath = ConfigFolder + "/FootstepSurfaces.asset";
        private const string ConfigPath = ConfigFolder + "/FootstepConfig.asset";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player.prefab";

        private static readonly SurfaceType[] Surfaces =
        {
            SurfaceType.Concrete,
            SurfaceType.Carpet,
            SurfaceType.Wood,
            SurfaceType.Metal,
            SurfaceType.Glass
        };

        [MenuItem("DarkDepths/Setup/Create Footsteps")]
        public static void CreateFootsteps()
        {
            // The probe mask is built from a layer name, so the layers have to exist first.
            LayersSetup.CreateLayers();

            var cues = new AudioCue[Surfaces.Length];
            for (int i = 0; i < Surfaces.Length; i++)
            {
                cues[i] = BuildCue(Surfaces[i]);
            }

            SurfaceAudioSet set = BuildSet(cues);
            FootstepConfig config = BuildConfig(set);
            AssetDatabase.SaveAssets();

            AttachToPlayer(config);
            AssetDatabase.SaveAssets();
        }

        private static AudioCue BuildCue(SurfaceType surface)
        {
            string[] paths = FootstepClipFactory.Create(surface, ClipFolder);
            var clips = new UnityEngine.Object[paths.Length];

            for (int i = 0; i < paths.Length; i++)
            {
                AssetDatabase.ImportAsset(paths[i], ImportAssetOptions.ForceUpdate);
                ApplyImportSettings(paths[i]);
                clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(paths[i]);
            }

            AudioCue cue = LoadOrCreate<AudioCue>(CueFolder + "/Footstep_" + surface + ".asset");
            SerializedWiring.WireArray(cue, "_clips", clips);
            SerializedWiring.WireInt(cue, "_channel", (int)AudioChannel.Sfx);
            SerializedWiring.WireFloat(cue, "_volumeMin", 0.85f);
            SerializedWiring.WireFloat(cue, "_volumeMax", 1f);
            SerializedWiring.WireFloat(cue, "_pitchMin", 0.92f);
            SerializedWiring.WireFloat(cue, "_pitchMax", 1.08f);
            // Below the default, so a step is the first thing dropped when the pool runs out.
            SerializedWiring.WireInt(cue, "_priority", 140);
            SerializedWiring.WireFloat(cue, "_spatialBlend", 1f);
            SerializedWiring.WireFloat(cue, "_minDistance", 1f);
            SerializedWiring.WireFloat(cue, "_maxDistance", 18f);

            EditorUtility.SetDirty(cue);
            return cue;
        }

        // Short impacts: kept decompressed so a step never waits on a decode, ADPCM so they stay small.
        private static void ApplyImportSettings(string path)
        {
            if (!(AssetImporter.GetAtPath(path) is AudioImporter importer))
            {
                return;
            }

            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.ADPCM;

            if (importer.defaultSampleSettings.loadType == settings.loadType &&
                importer.defaultSampleSettings.compressionFormat == settings.compressionFormat &&
                importer.forceToMono)
            {
                return;
            }

            importer.defaultSampleSettings = settings;
            importer.forceToMono = true;
            importer.SaveAndReimport();
        }

        private static SurfaceAudioSet BuildSet(AudioCue[] cues)
        {
            SurfaceAudioSet set = LoadOrCreate<SurfaceAudioSet>(SetPath);

            var serialized = new SerializedObject(set);
            SerializedProperty entries = serialized.FindProperty("_entries");
            entries.arraySize = Surfaces.Length;

            for (int i = 0; i < Surfaces.Length; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("Surface").intValue = (int)Surfaces[i];
                entry.FindPropertyRelative("Cue").objectReferenceValue = cues[i];
            }

            serialized.FindProperty("_fallback").objectReferenceValue = cues[0];
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(set);
            return set;
        }

        private static FootstepConfig BuildConfig(SurfaceAudioSet set)
        {
            FootstepConfig config = LoadOrCreate<FootstepConfig>(ConfigPath);
            SerializedWiring.Wire(config, "_surfaces", set);
            // Floors are built on the Wall layer; a probe that misses falls back to the default surface.
            SerializedWiring.WireInt(config, "_probeMask", 1 << LayerMask.NameToLayer("Wall"));
            EditorUtility.SetDirty(config);
            return config;
        }

        private static void AttachToPlayer(FootstepConfig config)
        {
            if (!File.Exists(PlayerPrefabPath))
            {
                return;
            }

            GameObject contents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);

            var footsteps = contents.GetComponent<FootstepPlayer>();
            if (footsteps == null)
            {
                footsteps = contents.AddComponent<FootstepPlayer>();
            }

            SerializedWiring.Wire(footsteps, "_config", config);
            SerializedWiring.Wire(footsteps, "_controller", contents.GetComponent<FirstPersonController>());

            PrefabUtility.SaveAsPrefabAsset(contents, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(contents);
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
