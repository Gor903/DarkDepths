using System.Collections;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DarkDepths.Tests.PlayMode
{
    public sealed class SandboxStartupTests
    {
        // Any logged error or exception during these frames fails the test and prints its text.
        [UnityTest]
        public IEnumerator SandboxStartsWithoutErrors()
        {
            yield return SceneManager.LoadSceneAsync("Sandbox", LoadSceneMode.Single);

            for (int frame = 0; frame < 30; frame++)
            {
                yield return null;
            }

            Assert.AreEqual("Sandbox", SceneManager.GetActiveScene().name);
        }
    }
}
