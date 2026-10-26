using System.Collections;
using System.IO;
using DarkDepths.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DarkDepths.Tests.PlayMode
{
    // A capture tool rather than a check: the bounce only lives in Awake and LateUpdate, so the one way
    // to photograph it is from play mode. Explicit keeps it out of a normal run; select it by filter.
    // Needs a GPU: run without -nographics and with DISPLAY set, or every frame comes out empty.
    [Explicit]
    public sealed class BounceShots
    {
        private const string ShotFolder = "Builds/Shots/Bounce";
        private const int Width = 1280;
        private const int Height = 720;
        // Long enough for the position and intensity damping of BounceLight to arrive.
        private const int SettleFrames = 50;

        private struct Viewpoint
        {
            public string Name;
            public Vector3 Eye;
            public float Yaw;
            public float Pitch;
        }

        // Eye height is the standing eye of PlayerMovementConfig; the room is 6 m across, door at +z.
        private static readonly Viewpoint[] Viewpoints =
        {
            new Viewpoint { Name = "room", Eye = new Vector3(0f, 1.65f, -2.6f), Yaw = 0f, Pitch = 2f },
            new Viewpoint { Name = "floor", Eye = new Vector3(0f, 1.65f, -1f), Yaw = 0f, Pitch = 40f },
            new Viewpoint { Name = "corridor", Eye = new Vector3(0f, 1.65f, 2.2f), Yaw = 0f, Pitch = 0f },
            new Viewpoint { Name = "grazing", Eye = new Vector3(-2f, 1.65f, -2f), Yaw = 55f, Pitch = 6f }
        };

        // Panning past the locker, which is where the pool is accused of waiting for the axis to arrive.
        private const string LockerName = "Module_Locker";
        private const float SweepStart = 50f;
        private const float SweepEnd = 95f;
        private const float SweepStep = 5f;
        private static readonly Vector3 SweepEye = new Vector3(0f, 1.65f, -1.6f);

        [UnityTest]
        public IEnumerator Sweep()
        {
            yield return SceneManager.LoadSceneAsync("Sandbox", LoadSceneMode.Single);
            yield return null;

            var flashlight = Object.FindAnyObjectByType<Flashlight>();
            Assert.IsNotNull(flashlight, "no flashlight in the sandbox");
            Light beam = flashlight.GetComponent<Light>();
            Camera camera = flashlight.GetComponentInParent<Camera>();
            Assert.IsNotNull(camera, "the flashlight is not under a camera");

            GameObject locker = GameObject.Find(LockerName);
            Assert.IsNotNull(locker, "no locker in the sandbox to pan across");
            var lockerCollider = locker.GetComponentInChildren<Collider>();
            Assert.IsNotNull(lockerCollider, "the locker has no collider to measure against");

            Stop(Object.FindAnyObjectByType<FirstPersonController>());
            Stop(Object.FindAnyObjectByType<PlayerLook>());
            var body = Object.FindAnyObjectByType<CharacterController>();
            if (body != null)
            {
                body.enabled = false;
            }

            beam.enabled = true;
            Directory.CreateDirectory(ShotFolder);
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);

            for (float yaw = SweepStart; yaw <= SweepEnd + 0.01f; yaw += SweepStep)
            {
                camera.transform.SetPositionAndRotation(SweepEye, Quaternion.Euler(0f, yaw, 0f));
                Physics.SyncTransforms();

                for (int frame = 0; frame < SettleFrames; frame++)
                {
                    yield return null;
                }

                Light pool = Pool();
                // How far the pool sits from the locker is the whole question: zero means it landed on it.
                float toLocker = pool == null
                    ? -1f
                    : Vector3.Distance(lockerCollider.ClosestPoint(pool.transform.position),
                        pool.transform.position);
                bool axisOnLocker = Physics.Raycast(flashlight.transform.position,
                    flashlight.transform.forward, out RaycastHit axisHit, 12f)
                    && axisHit.collider == lockerCollider;

                Debug.Log($"Sweep yaw {yaw:0}: axis on locker {axisOnLocker}"
                          + $", pool {toLocker:0.00} m from it"
                          + $", intensity {(pool == null ? 0f : pool.intensity):0.000}");

                Shoot(camera, target, "sweep-" + yaw.ToString("000"));
            }

            camera.targetTexture = null;
            Object.Destroy(target);
            Debug.Log("Sweep written to " + Path.GetFullPath(ShotFolder));
        }

        [UnityTest]
        public IEnumerator Capture()
        {
            yield return SceneManager.LoadSceneAsync("Sandbox", LoadSceneMode.Single);
            yield return null;

            var flashlight = Object.FindAnyObjectByType<Flashlight>();
            Assert.IsNotNull(flashlight, "no flashlight in the sandbox");

            var bounce = flashlight.GetComponent<FlashlightBounce>();
            Assert.IsNotNull(bounce, "the flashlight has no bounce component");

            Light beam = flashlight.GetComponent<Light>();
            Camera camera = flashlight.GetComponentInParent<Camera>();
            Assert.IsNotNull(camera, "the flashlight is not under a camera");

            // The camera has to stay where it is put, so whatever would move it steps aside.
            Stop(Object.FindAnyObjectByType<FirstPersonController>());
            Stop(Object.FindAnyObjectByType<PlayerLook>());

            // A CharacterController is a collider, not a behaviour, and it would slide the camera back down.
            var body = Object.FindAnyObjectByType<CharacterController>();
            if (body != null)
            {
                body.enabled = false;
            }

            // The torch toggles on input; with no input to give it, the beam is switched on directly.
            beam.enabled = true;

            Directory.CreateDirectory(ShotFolder);
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);

            foreach (Viewpoint viewpoint in Viewpoints)
            {
                camera.transform.SetPositionAndRotation(viewpoint.Eye,
                    Quaternion.Euler(viewpoint.Pitch, viewpoint.Yaw, 0f));
                Physics.SyncTransforms();

                bounce.enabled = true;
                for (int frame = 0; frame < SettleFrames; frame++)
                {
                    yield return null;
                }

                Light pool = Pool();
                Shoot(camera, target, viewpoint.Name + "-bounce");
                Debug.Log("Bounce at " + viewpoint.Name + ": "
                          + (pool == null ? "no pool light" : "intensity " + pool.intensity.ToString("0.000")
                              + ", range " + pool.range.ToString("0.0")
                              + ", enabled " + pool.enabled));

                // The same frame without the pool, so the pair differs by one light and nothing else.
                bounce.enabled = false;
                if (pool != null)
                {
                    pool.enabled = false;
                }

                yield return null;
                Shoot(camera, target, viewpoint.Name + "-plain");
            }

            camera.targetTexture = null;
            Object.Destroy(target);

            Debug.Log("Bounce shots written to " + Path.GetFullPath(ShotFolder));
        }

        // The pool is a bare light parked at the scene root by FlashlightBounce.Awake.
        private static Light Pool()
        {
            GameObject holder = GameObject.Find("FlashlightBounce");
            return holder == null ? null : holder.GetComponent<Light>();
        }

        private static void Stop(Behaviour behaviour)
        {
            if (behaviour != null)
            {
                behaviour.enabled = false;
            }
        }

        private static void Shoot(Camera camera, RenderTexture target, string name)
        {
            camera.targetTexture = target;
            camera.Render();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;

            var image = new Texture2D(Width, Height, TextureFormat.RGBA32, false, false);
            image.ReadPixels(new Rect(0f, 0f, Width, Height), 0, 0);
            image.Apply();

            RenderTexture.active = previous;
            camera.targetTexture = null;

            File.WriteAllBytes(Path.Combine(ShotFolder, name + ".png"), image.EncodeToPNG());
            Object.Destroy(image);
        }
    }
}
