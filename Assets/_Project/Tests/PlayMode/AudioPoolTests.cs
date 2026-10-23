using System.Collections;
using DarkDepths.Audio;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DarkDepths.Tests.PlayMode
{
    public sealed class AudioPoolTests
    {
        // The pool is the one part of the service that only exists once the scene is running.
        [UnityTest]
        public IEnumerator SandboxBuildsTheVoicePoolAndLeavesItIdle()
        {
            yield return SceneManager.LoadSceneAsync("Sandbox", LoadSceneMode.Single);
            yield return null;

            var service = Object.FindAnyObjectByType<AudioService>();
            Assert.IsNotNull(service, "the Sandbox scene has no AudioService");
            Assert.AreEqual(24, service.VoiceCount, "docs/07 budgets 24 simultaneous voices");
            Assert.AreEqual(24, service.transform.childCount);

            // The room's background takes its voices at load; everything else waits to be asked for.
            int ambience = Object.FindObjectsByType<AmbienceEmitter>().Length;
            Assert.AreEqual(ambience, service.ActiveVoices, "only the background should be running");

            var source = service.transform.GetChild(0).GetComponent<AudioSource>();
            Assert.IsNotNull(source);
            Assert.IsFalse(source.playOnAwake);
            Assert.IsNotNull(source.outputAudioMixerGroup, "voices must route through the mixer");
        }
    }
}
