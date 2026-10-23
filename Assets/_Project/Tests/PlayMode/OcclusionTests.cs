using System.Collections;
using DarkDepths.Audio;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DarkDepths.Tests.PlayMode
{
    public sealed class OcclusionTests
    {
        // The sandbox is the case docs/04 describes: a hum in the room the player stands in,
        // and a vent down the corridor with a shut door between.
        [UnityTest]
        public IEnumerator AVentBehindAShutDoorIsHeardThroughIt()
        {
            yield return SceneManager.LoadSceneAsync("Sandbox", LoadSceneMode.Single);
            yield return new WaitForSeconds(1f);

            var service = Object.FindAnyObjectByType<AudioService>();
            Assert.IsNotNull(service, "the Sandbox scene has no AudioService");

            float vent = CutoffOf(service, "Ambience_Vent");
            float room = CutoffOf(service, "Ambience_RoomTone");

            Assert.Less(vent, 5000f, "a vent behind a shut door has to sound like it");
            Assert.Greater(room, 15000f, "the room's own hum stands in the room with the player");
        }

        private static float CutoffOf(AudioService service, string clipName)
        {
            foreach (Transform child in service.transform)
            {
                var source = child.GetComponent<AudioSource>();
                if (source.clip == null || source.clip.name != clipName)
                {
                    continue;
                }

                var filter = child.GetComponent<AudioLowPassFilter>();
                Assert.IsNotNull(filter, "every voice carries its own low-pass filter");
                return filter.cutoffFrequency;
            }

            Assert.Fail("no voice is playing " + clipName);
            return 0f;
        }
    }
}
