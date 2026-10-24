using System.IO;
using DarkDepths.Core;
using DarkDepths.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // The A/B pass of docs/10_LIGHTING.md asks for the same camera every time: these are those cameras.
    // Each viewpoint is rendered twice, with the beam and without, into Builds/Shots.
    public static class LightingShots
    {
        private const string SandboxScenePath = "Assets/_Project/Scenes/Sandbox.unity";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player.prefab";
        private const string ShotFolder = "Builds/Shots";

        private const int Width = 1280;
        private const int Height = 720;

        private struct Viewpoint
        {
            public string Name;
            public Vector3 Eye;
            public float Yaw;
            public float Pitch;
        }

        // Eye height is the standing eye of PlayerMovementConfig; the room is 6 m across with the door at +z.
        private static readonly Viewpoint[] Viewpoints =
        {
            new Viewpoint { Name = "room", Eye = new Vector3(0f, 1.65f, -2.6f), Yaw = 0f, Pitch = 2f },
            new Viewpoint { Name = "props", Eye = new Vector3(-1.5f, 1.65f, -1f), Yaw = 60f, Pitch = 4f },
            new Viewpoint { Name = "corridor", Eye = new Vector3(0f, 1.65f, 2.2f), Yaw = 0f, Pitch = 0f },
            new Viewpoint { Name = "floor", Eye = new Vector3(0f, 1.65f, -1f), Yaw = 0f, Pitch = 40f }
        };

        // The two numbers docs/10_LIGHTING.md leaves open: how bright the beam is and how far the dark lifts.
        private static readonly float[] Intensities = { 3f, 6f, 9f, 12f };
        private static readonly float[] Ambients = { 0.07f, 0.2f, 0.35f, 0.5f };

        [MenuItem("DarkDepths/Shots/Capture Sweep")]
        public static void CaptureSweep()
        {
            EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);
            Directory.CreateDirectory(ShotFolder);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var camera = player.GetComponentInChildren<Camera>(true);
            var flashlight = player.GetComponentInChildren<Flashlight>(true);
            flashlight.ApplyBeamSettings();
            Light beam = flashlight.GetComponent<Light>();

            PaletteConfig palette = PaletteSetup.LoadOrCreateConfig();
            Color ambient = RenderSettings.ambientLight;
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);

            Viewpoint room = Viewpoints[0];
            camera.transform.SetPositionAndRotation(room.Eye, Quaternion.Euler(room.Pitch, room.Yaw, 0f));

            foreach (float intensity in Intensities)
            {
                beam.intensity = intensity;
                Shoot(camera, beam, target, "sweep-beam-" + intensity, true);
            }

            beam.intensity = flashlight.GetComponent<Light>().intensity;
            foreach (float lift in Ambients)
            {
                Color color = palette.Background * lift;
                color.a = 1f;
                RenderSettings.ambientLight = color;
                Shoot(camera, beam, target, "sweep-ambient-" + lift, false);
            }

            RenderSettings.ambientLight = ambient;
            camera.targetTexture = null;
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(player);
        }

        [MenuItem("DarkDepths/Shots/Capture Lighting")]
        public static void Capture()
        {
            EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);
            Directory.CreateDirectory(ShotFolder);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var camera = player.GetComponentInChildren<Camera>(true);
            var flashlight = player.GetComponentInChildren<Flashlight>(true);

            // The same call the component makes in Awake, so the shot is of the beam the game builds.
            flashlight.ApplyBeamSettings();
            Light beam = flashlight.GetComponent<Light>();

            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);

            foreach (Viewpoint viewpoint in Viewpoints)
            {
                camera.transform.SetPositionAndRotation(viewpoint.Eye,
                    Quaternion.Euler(viewpoint.Pitch, viewpoint.Yaw, 0f));

                Shoot(camera, beam, target, viewpoint.Name, true);
                Shoot(camera, beam, target, viewpoint.Name, false);
            }

            camera.targetTexture = null;
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(player);

            // The scene is never saved: the shot rig is a visitor, not a change to the sandbox.
            Debug.Log("Lighting shots written to " + Path.GetFullPath(ShotFolder));
        }

        private static void Shoot(Camera camera, Light beam, RenderTexture target, string name, bool lit)
        {
            beam.enabled = lit;

            camera.targetTexture = target;
            camera.Render();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;

            var image = new Texture2D(Width, Height, TextureFormat.RGBA32, false, false);
            image.ReadPixels(new Rect(0f, 0f, Width, Height), 0, 0);
            image.Apply();

            RenderTexture.active = previous;
            camera.targetTexture = null;

            string suffix = lit ? "beam" : "dark";
            File.WriteAllBytes(Path.Combine(ShotFolder, name + "-" + suffix + ".png"), image.EncodeToPNG());
            Object.DestroyImmediate(image);
        }
    }
}
