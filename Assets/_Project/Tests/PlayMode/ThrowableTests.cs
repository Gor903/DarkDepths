using System.Collections;
using DarkDepths.Enemies;
using DarkDepths.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DarkDepths.Tests.PlayMode
{
    public sealed class ThrowableTests
    {
        private PlayerThrower _thrower;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Sandbox", LoadSceneMode.Single);
            yield return null;

            _thrower = Object.FindAnyObjectByType<PlayerThrower>();
            Assert.IsNotNull(_thrower, "the player carries nothing to throw");
        }

        [UnityTest]
        public IEnumerator ThrowingCostsOneAndLeavesABottleInTheRoom()
        {
            int before = _thrower.Count;
            Assert.Greater(before, 0, "a run starts with something to throw");

            Assert.IsTrue(_thrower.TryThrow());
            yield return null;

            Assert.AreEqual(before - 1, _thrower.Count);

            var thrown = GameObject.Find("Thrown");
            Assert.IsNotNull(thrown, "the bottles live outside the player, not under them");
            Assert.AreEqual(1, CountActive(thrown.transform), "exactly one bottle is in the air");
        }

        // What the bottle is for: the Listener goes to the noise instead of to the player.
        [UnityTest]
        public IEnumerator ABottleLandingPullsTheListenerToIt()
        {
            var enemy = Object.FindAnyObjectByType<EnemyAgent>();
            Assert.IsNotNull(enemy);
            Assert.AreEqual(EnemyStateId.Patrol, enemy.State);

            Assert.IsTrue(_thrower.TryThrow());

            for (float waited = 0f; waited < 4f && enemy.State == EnemyStateId.Patrol; waited += Time.deltaTime)
            {
                yield return null;
            }

            Assert.AreNotEqual(EnemyStateId.Patrol, enemy.State, "a bottle landing has to be worth a look");
        }

        [UnityTest]
        public IEnumerator AnEmptyPocketThrowsNothing()
        {
            while (_thrower.Count > 0)
            {
                Assert.IsTrue(_thrower.TryThrow());
                // The cooldown is what stops the whole pocket leaving at once.
                yield return new WaitForSeconds(0.5f);
            }

            Assert.IsFalse(_thrower.TryThrow());
            Assert.AreEqual(0, _thrower.Count);
        }

        private static int CountActive(Transform holder)
        {
            int count = 0;
            foreach (Transform child in holder)
            {
                if (child.gameObject.activeSelf)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
