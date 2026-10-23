using System.Collections;
using DarkDepths.Core;
using DarkDepths.Noise;
using DarkDepths.Player;
using DarkDepths.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DarkDepths.Tests.PlayMode
{
    public sealed class NoiseEmitterTests
    {
        // A listener that only records, so what reached it can be read back.
        private sealed class Ear : INoiseListener
        {
            public Vector3 Position { get; set; }
            public float HearingMultiplier => 1f;
            public int Heard { get; private set; }
            public NoiseType LastType { get; private set; }

            public void OnNoiseHeard(in NoiseEvent noise, float perceivedLoudness)
            {
                Heard++;
                LastType = noise.Type;
            }
        }

        private INoiseService _noise;
        private Ear _ear;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Sandbox", LoadSceneMode.Single);
            yield return null;

            _noise = Services.Get<INoiseService>();
            Assert.IsNotNull(_noise, "the Sandbox scene has no NoiseSystem");

            _ear = new Ear();
            _noise.Register(_ear);
        }

        [TearDown]
        public void TearDown()
        {
            _noise.Unregister(_ear);
        }

        [UnityTest]
        public IEnumerator AWalkingPlayerIsHeard()
        {
            var player = Object.FindAnyObjectByType<FirstPersonController>();
            Assert.IsNotNull(player, "the Sandbox scene has no player");

            var body = player.GetComponent<CharacterController>();
            _ear.Position = body.transform.position;

            // The controller is driven by input, which a test has none of; the body is walked directly
            // instead, so the controller is held still for it.
            player.enabled = false;

            try
            {
                // Walked rather than teleported, and measured in metres rather than seconds: a step is
                // counted in ground covered, and a batchmode frame is far too short to cover any. The downward
                // nudge only keeps the body on the floor; pushed into it, it would stop moving forward.
                // South is the one direction out of the middle of the room with no furniture in it.
                for (int frame = 0; frame < 200 && _ear.Heard == 0; frame++)
                {
                    body.Move(new Vector3(0f, -0.002f, -0.05f));
                    yield return null;
                }
            }
            finally
            {
                player.enabled = true;
            }

            Assert.Greater(_ear.Heard, 0, "walking away from a spot has to be heard from it");
            Assert.AreEqual(NoiseType.Footstep, _ear.LastType);
        }

        [UnityTest]
        public IEnumerator AnOpenedDoorIsHeardAcrossTheRoom()
        {
            var door = Object.FindAnyObjectByType<Door>();
            var player = Object.FindAnyObjectByType<FirstPersonController>();
            Assert.IsNotNull(door, "the Sandbox scene has no door");
            Assert.IsNotNull(player, "the Sandbox scene has no player");

            _ear.Position = player.transform.position;
            door.Interact(player.gameObject);

            Assert.AreEqual(1, _ear.Heard, "a door opened three metres away has to be heard");
            Assert.AreEqual(NoiseType.Door, _ear.LastType);
            yield break;
        }

        [UnityTest]
        public IEnumerator AFallingObjectIsHeardWhereItLands()
        {
            _ear.Position = Vector3.zero;

            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.localScale = Vector3.one * 0.3f;
            box.transform.position = new Vector3(1f, 2.5f, 0f);
            box.AddComponent<Rigidbody>();
            box.AddComponent<ImpactNoise>();

            try
            {
                for (float waited = 0f; waited < 3f && _ear.Heard == 0; waited += Time.deltaTime)
                {
                    yield return null;
                }

                Assert.Greater(_ear.Heard, 0, "a box dropped two metres onto concrete has to be heard");
                Assert.AreEqual(NoiseType.ObjectImpact, _ear.LastType);
            }
            finally
            {
                Object.Destroy(box);
            }
        }

        // A box that bounces and slides is one noise, not a burst of them.
        [UnityTest]
        public IEnumerator ALandingIsHeardOnceWhileItSettles()
        {
            _ear.Position = Vector3.zero;

            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.localScale = Vector3.one * 0.3f;
            box.transform.position = new Vector3(-1f, 2.5f, 0f);
            box.AddComponent<Rigidbody>();
            box.AddComponent<ImpactNoise>();

            try
            {
                yield return new WaitForSeconds(1.5f);

                Assert.AreEqual(1, _ear.Heard, "the cooldown has to hold the landing to a single noise");
            }
            finally
            {
                Object.Destroy(box);
            }
        }
    }
}
