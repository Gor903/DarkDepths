using System.Collections;
using DarkDepths.Core;
using DarkDepths.Enemies;
using DarkDepths.Player;
using DarkDepths.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DarkDepths.Tests.PlayMode
{
    public sealed class HideoutTests
    {
        private Hideout _hideout;
        private PlayerHiding _hiding;
        private IPlayerTarget _target;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Sandbox", LoadSceneMode.Single);
            yield return null;

            _hideout = Object.FindAnyObjectByType<Hideout>();
            _hiding = Object.FindAnyObjectByType<PlayerHiding>();
            _target = Services.Get<IPlayerTarget>();

            Assert.IsNotNull(_hideout, "the test room has nothing to hide in");
            Assert.IsNotNull(_hiding, "the player cannot take cover");
        }

        [UnityTest]
        public IEnumerator SteppingInMovesThePlayerAndStopsTheirFeet()
        {
            var controller = Object.FindAnyObjectByType<FirstPersonController>();
            Vector3 before = _hiding.transform.position;

            _hideout.Interact(_hiding.gameObject);
            yield return null;

            Assert.IsTrue(_hiding.IsHidden);
            Assert.IsTrue(_target.IsHidden);
            Assert.IsFalse(controller.enabled, "a hidden player does not walk off");
            Assert.AreNotEqual(before, _hiding.transform.position);

            _hideout.Interact(_hiding.gameObject);
            yield return null;

            Assert.IsFalse(_hiding.IsHidden);
            Assert.IsTrue(controller.enabled);
            Assert.AreEqual(before, _hiding.transform.position, "leaving puts them back where they stood");
        }

        // The point of cover: standing in front of it, the Listener has nothing to chase.
        [UnityTest]
        public IEnumerator TheListenerWalksPastSomeoneInCover()
        {
            var enemy = Object.FindAnyObjectByType<EnemyAgent>();
            _hideout.Interact(_hiding.gameObject);
            yield return null;

            // Put it right in front of the opening, looking straight at where the player is standing.
            enemy.transform.position = _hiding.transform.position + Vector3.forward * 1.5f;
            enemy.transform.rotation = Quaternion.LookRotation(_hiding.transform.position - enemy.transform.position);

            yield return new WaitForSeconds(0.6f);

            Assert.IsFalse(_target.IsHunted, "cover that can be seen through is not cover");
            Assert.AreNotEqual(EnemyStateId.Chase, enemy.State);
            Assert.AreNotEqual(EnemyStateId.Attack, enemy.State);
        }
    }
}
