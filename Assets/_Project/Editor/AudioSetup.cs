using System;
using System.IO;
using System.Reflection;
using DarkDepths.Audio;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;

namespace DarkDepths.EditorTools
{
    // Builds the mixer of docs/04 — Master with a limiter, SFX, Ambient, Music, UI — and the service that uses it.
    // The mixer has no public authoring API, so the editor-internal controller is reached through reflection.
    public static class AudioSetup
    {
        private const string MixerFolder = "Assets/_Project/Audio/Mixer";
        private const string MixerPath = MixerFolder + "/GameMixer.mixer";
        private const string ConfigFolder = "Assets/_Project/Config";
        private const string ConfigPath = ConfigFolder + "/AudioConfig.asset";
        private const string SandboxScenePath = "Assets/_Project/Scenes/Sandbox.unity";
        private const string ServiceName = "Audio";

        private const string MasterGroup = "Master";
        private const string SfxGroup = "SFX";
        private const string AmbientGroup = "Ambient";
        private const string MusicGroup = "Music";
        private const string UiGroup = "UI";

        private const string Limiter = "Compressor";

        private const BindingFlags Members =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private static readonly Assembly EditorAssembly = typeof(AssetDatabase).Assembly;

        [MenuItem("DarkDepths/Setup/Create Audio")]
        public static void CreateAudio()
        {
            AudioMixer mixer = BuildMixer();
            AudioConfig config = LoadOrCreate<AudioConfig>(ConfigPath);
            WireConfig(config, mixer);
            AssetDatabase.SaveAssets();

            PlaceService();
            AssetDatabase.SaveAssets();
        }

        private static AudioMixer BuildMixer()
        {
            Directory.CreateDirectory(MixerFolder);

            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (mixer == null)
            {
                MethodInfo create = Type("AudioMixerController").GetMethod("CreateMixerControllerAtPath", Members);
                mixer = (AudioMixer)create.Invoke(null, new object[] { MixerPath });
            }

            object master = Get(mixer, "masterGroup");
            ExposeVolume(mixer, master, MixerParameters.MasterVolume);
            AddLimiter(mixer, master);

            BuildView(mixer, master,
                EnsureGroup(mixer, master, SfxGroup, MixerParameters.SfxVolume),
                EnsureGroup(mixer, master, AmbientGroup, MixerParameters.AmbientVolume),
                EnsureGroup(mixer, master, MusicGroup, MixerParameters.MusicVolume),
                EnsureGroup(mixer, master, UiGroup, MixerParameters.UiVolume));

            EditorUtility.SetDirty(mixer);
            AssetDatabase.SaveAssets();
            return mixer;
        }

        private static AudioMixerGroup EnsureGroup(AudioMixer mixer, object master, string name, string parameter)
        {
            AudioMixerGroup group = FindGroup(mixer, name);
            if (group == null)
            {
                object created = Call(mixer, "CreateNewGroup", name, false);
                Call(mixer, "AddChildToParent", created, master);

                // A group and its effects are sub-assets of the mixer; without this they vanish on reload.
                Persist(created, mixer);
                foreach (object effect in (Array)Get(created, "effects"))
                {
                    Persist(effect, mixer);
                }

                group = (AudioMixerGroup)created;
            }

            ExposeVolume(mixer, group, parameter);
            return group;
        }

        // Which groups the Audio Mixer window shows. A mixer built outside the window has no view at all,
        // and the window reads view zero without checking.
        private static void BuildView(AudioMixer mixer, params object[] groups)
        {
            PropertyInfo property = mixer.GetType().GetProperty("views", Members);
            Type viewType = property.PropertyType.GetElementType();
            FieldInfo guidsField = viewType.GetField("guids", Members);
            FieldInfo nameField = viewType.GetField("name", Members);

            Array guids = Array.CreateInstance(guidsField.FieldType.GetElementType(), groups.Length);
            for (int i = 0; i < groups.Length; i++)
            {
                guids.SetValue(Get(groups[i], "groupID"), i);
            }

            object view = Activator.CreateInstance(viewType);
            guidsField.SetValue(view, guids);
            nameField.SetValue(view, "View");

            Array views = Array.CreateInstance(viewType, 1);
            views.SetValue(view, 0);
            property.SetValue(mixer, views);
            Set(mixer, "currentViewIndex", 0);
        }

        // docs/04 asks for a limiter on Master; the built-in set has no limiter, so the compressor stands in
        // with a fast attack and a threshold below the loudest sound.
        private static void AddLimiter(AudioMixer mixer, object master)
        {
            object effect = FindEffect(master, Limiter);
            if (effect == null)
            {
                effect = Type("AudioMixerEffectController")
                    .GetConstructors(Members)[0]
                    .Invoke(new object[] { Limiter });
                Call(effect, "PreallocateGUIDs");

                var effects = (Array)Get(master, "effects");
                Call(master, "InsertEffect", effect, effects.Length);
                Persist(effect, mixer);
            }

            object snapshot = Get(mixer, "TargetSnapshot");
            Call(effect, "SetValueForParameter", mixer, snapshot, "Threshold", -6f);
            Call(effect, "SetValueForParameter", mixer, snapshot, "Attack", 10f);
            Call(effect, "SetValueForParameter", mixer, snapshot, "Release", 200f);
            Call(effect, "SetValueForParameter", mixer, snapshot, "Make up gain", 0f);
        }

        private static void ExposeVolume(AudioMixer mixer, object group, string parameter)
        {
            object guid = Call(group, "GetGUIDForVolume");
            if (!(bool)Call(mixer, "ContainsExposedParameter", guid))
            {
                object path = Type("AudioGroupParameterPath")
                    .GetConstructors(Members)[0]
                    .Invoke(new[] { group, guid });
                Call(mixer, "AddExposedParameter", path);
            }

            // Exposing gives the parameter a generated name; the game looks it up by ours.
            var parameters = (Array)Get(mixer, "exposedParameters");
            Type exposed = Type("ExposedAudioParameter");
            FieldInfo guidField = exposed.GetField("guid", Members);
            FieldInfo nameField = exposed.GetField("name", Members);

            for (int i = 0; i < parameters.Length; i++)
            {
                object entry = parameters.GetValue(i);
                if (!guidField.GetValue(entry).Equals(guid))
                {
                    continue;
                }

                nameField.SetValue(entry, parameter);
                parameters.SetValue(entry, i);
            }

            Set(mixer, "exposedParameters", parameters);
        }

        private static void WireConfig(AudioConfig config, AudioMixer mixer)
        {
            SerializedWiring.Wire(config, "_mixer", mixer);
            SerializedWiring.Wire(config, "_masterGroup", FindGroup(mixer, MasterGroup));
            SerializedWiring.Wire(config, "_sfxGroup", FindGroup(mixer, SfxGroup));
            SerializedWiring.Wire(config, "_ambientGroup", FindGroup(mixer, AmbientGroup));
            SerializedWiring.Wire(config, "_musicGroup", FindGroup(mixer, MusicGroup));
            SerializedWiring.Wire(config, "_uiGroup", FindGroup(mixer, UiGroup));
            EditorUtility.SetDirty(config);
        }

        private static void PlaceService()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);

            // Loaded after the scene opens, like the other setup tools: opening a scene drops unreferenced assets.
            var config = AssetDatabase.LoadAssetAtPath<AudioConfig>(ConfigPath);

            var existing = GameObject.Find(ServiceName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }

            var root = new GameObject(ServiceName, typeof(AudioService));
            SerializedWiring.Wire(root.GetComponent<AudioService>(), "_config", config);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static AudioMixerGroup FindGroup(AudioMixer mixer, string name)
        {
            AudioMixerGroup[] matches = mixer.FindMatchingGroups(name);
            foreach (AudioMixerGroup candidate in matches)
            {
                if (candidate.name == name)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static object FindEffect(object group, string effectName)
        {
            foreach (object effect in (Array)Get(group, "effects"))
            {
                if ((string)Get(effect, "effectName") == effectName)
                {
                    return effect;
                }
            }

            return null;
        }

        private static void Persist(object asset, AudioMixer mixer)
        {
            var target = asset as UnityEngine.Object;
            if (target != null && !EditorUtility.IsPersistent(target))
            {
                AssetDatabase.AddObjectToAsset(target, mixer);
            }
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

        private static Type Type(string name)
        {
            return EditorAssembly.GetType("UnityEditor.Audio." + name);
        }

        private static object Call(object target, string method, params object[] args)
        {
            return target.GetType().GetMethod(method, Members).Invoke(target, args);
        }

        private static object Get(object target, string property)
        {
            return target.GetType().GetProperty(property, Members).GetValue(target);
        }

        private static void Set(object target, string property, object value)
        {
            target.GetType().GetProperty(property, Members).SetValue(target, value);
        }
    }
}
