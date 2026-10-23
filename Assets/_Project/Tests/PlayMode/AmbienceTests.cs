using System.Collections;
using DarkDepths.Audio;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DarkDepths.Tests.PlayMode
{
    public sealed class AmbienceTests
    {
        // The background is the one sound that has to be there before the player does anything.
        [UnityTest]
        public IEnumerator SandboxBackgroundHoldsItsVoicesOnTheAmbientGroup()
        {
            yield return SceneManager.LoadSceneAsync("Sandbox", LoadSceneMode.Single);
            yield return null;

            AmbienceEmitter[] emitters =
                Object.FindObjectsByType<AmbienceEmitter>();
            Assert.IsNotEmpty(emitters, "the sandbox room has no background at all");

            for (int i = 0; i < emitters.Length; i++)
            {
                Assert.IsTrue(emitters[i].IsPlaying, emitters[i].name + " holds no voice");
            }

            var service = Object.FindAnyObjectByType<AudioService>();
            Assert.IsNotNull(service);
            Assert.AreEqual(emitters.Length, service.ActiveVoices,
                "nothing but the background should be playing in an empty room");

            int loops = 0;
            foreach (Transform child in service.transform)
            {
                var source = child.GetComponent<AudioSource>();
                if (source.clip == null)
                {
                    continue;
                }

                loops++;
                Assert.IsTrue(source.loop, "a background voice that does not loop falls quiet");
                Assert.AreEqual("Ambient", source.outputAudioMixerGroup.name,
                    "the background belongs on the Ambient group, where its own volume slider reaches it");
            }

            Assert.AreEqual(emitters.Length, loops);
        }
    }
}
