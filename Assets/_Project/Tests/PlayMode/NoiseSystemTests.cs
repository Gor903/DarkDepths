using System.Collections;
using DarkDepths.Core;
using DarkDepths.Noise;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DarkDepths.Tests.PlayMode
{
    public sealed class NoiseSystemTests
    {
        // A listener that only records, so what reached it can be read back.
        private sealed class Ear : INoiseListener
        {
            public Vector3 Position { get; set; }
            public float HearingMultiplier { get; set; } = 1f;
            public int Heard { get; private set; }
            public float LastLoudness { get; private set; }

            public void OnNoiseHeard(in NoiseEvent noise, float perceivedLoudness)
            {
                Heard++;
                LastLoudness = perceivedLoudness;
            }
        }

        private INoiseService _noise;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Sandbox", LoadSceneMode.Single);
            yield return null;

            _noise = Services.Get<INoiseService>();
            Assert.IsNotNull(_noise, "the Sandbox scene has no NoiseSystem");
        }

        [UnityTest]
        public IEnumerator ANoiseOnTopOfAListenerIsHeardAtFullStrength()
        {
            var ear = new Ear { Position = Vector3.zero };
            _noise.Register(ear);

            try
            {
                _noise.Emit(new NoiseEvent(Vector3.zero, 8f, NoiseType.Footstep));

                Assert.AreEqual(1, ear.Heard);
                Assert.AreEqual(1f, ear.LastLoudness, 0.01f);
            }
            finally
            {
                _noise.Unregister(ear);
            }

            yield break;
        }

        [UnityTest]
        public IEnumerator ANoiseOutOfReachIsNotHeard()
        {
            var ear = new Ear { Position = Vector3.zero };
            _noise.Register(ear);

            try
            {
                // Concrete at a multiplier of one: a walking step reaches eight metres, not twenty.
                _noise.Emit(new NoiseEvent(new Vector3(0f, 0f, 20f), 8f, NoiseType.Footstep));

                Assert.AreEqual(0, ear.Heard);
            }
            finally
            {
                _noise.Unregister(ear);
            }

            yield break;
        }

        [UnityTest]
        public IEnumerator ADeafListenerIsSkipped()
        {
            var ear = new Ear { Position = Vector3.zero, HearingMultiplier = 0f };
            _noise.Register(ear);

            try
            {
                _noise.Emit(new NoiseEvent(Vector3.zero, 15f, NoiseType.Weapon));

                Assert.AreEqual(0, ear.Heard);
            }
            finally
            {
                _noise.Unregister(ear);
            }

            yield break;
        }

        // The corridor is boarded over and the room is concrete: the same step carries further out there.
        [UnityTest]
        public IEnumerator TheSurfaceUnderTheSoundDecidesHowFarItCarries()
        {
            float concrete = Hear(new Vector3(0f, 0f, 2.6f), new Vector3(0f, 0f, 0.6f));
            float wood = Hear(new Vector3(0f, 0f, 6.6f), new Vector3(0f, 0f, 4.6f));

            Assert.Greater(concrete, 0f, "a step two metres away has to be heard at all");
            Assert.Greater(wood, concrete,
                "boards carry a step further than concrete, so the same distance is louder on them");

            yield break;
        }

        // One ear at a time: two of them this close would both hear both steps.
        private float Hear(Vector3 listener, Vector3 source)
        {
            var ear = new Ear { Position = listener };
            _noise.Register(ear);

            try
            {
                _noise.Emit(new NoiseEvent(source, 8f, NoiseType.Footstep));
            }
            finally
            {
                _noise.Unregister(ear);
            }

            return ear.LastLoudness;
        }

        [UnityTest]
        public IEnumerator AnUnregisteredListenerStopsHearing()
        {
            var ear = new Ear { Position = Vector3.zero };
            _noise.Register(ear);
            _noise.Unregister(ear);

            _noise.Emit(new NoiseEvent(Vector3.zero, 8f, NoiseType.Door));

            Assert.AreEqual(0, ear.Heard);
            yield break;
        }
    }
}
