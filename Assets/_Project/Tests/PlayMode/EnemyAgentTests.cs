using System.Collections;
using DarkDepths.Core;
using DarkDepths.Enemies;
using DarkDepths.Noise;
using DarkDepths.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DarkDepths.Tests.PlayMode
{
    public sealed class EnemyAgentTests
    {
        private EnemyAgent _enemy;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Sandbox", LoadSceneMode.Single);
            yield return null;

            _enemy = Object.FindAnyObjectByType<EnemyAgent>();
            Assert.IsNotNull(_enemy, "the Sandbox scene has no enemy");
        }

        // Without baked ground the enemy stands still and says nothing about it.
        [UnityTest]
        public IEnumerator TheListenerStandsOnBakedGround()
        {
            var agent = _enemy.GetComponent<NavMeshAgent>();
            Assert.IsNotNull(agent);
            Assert.IsTrue(agent.isOnNavMesh, "the corridor has no NavMesh under the enemy");

            yield return null;

            Assert.AreEqual(EnemyStateId.Patrol, _enemy.State);
            Assert.IsTrue(agent.hasPath || agent.pathPending, "a patrol with a route has somewhere to be");
        }

        [UnityTest]
        public IEnumerator ANoiseNearbyPutsItOnAlert()
        {
            var noise = Services.Get<INoiseService>();
            Assert.IsNotNull(noise, "the Sandbox scene has no NoiseSystem");

            // Right next to it and as loud as a run: there is no mistaking this one.
            noise.Emit(new NoiseEvent(_enemy.transform.position + Vector3.forward, 15f, NoiseType.Footstep));

            yield return new WaitForSeconds(0.5f);

            Assert.AreNotEqual(EnemyStateId.Patrol, _enemy.State, "a step beside it has to interrupt the patrol");
        }

        // The whole of the task in one go: seen, chased, struck, and the run ends.
        [UnityTest]
        public IEnumerator StandingInFrontOfItCostsTheRun()
        {
            var health = Object.FindAnyObjectByType<PlayerHealth>();
            var target = Services.Get<IPlayerTarget>();
            Assert.IsNotNull(health, "the Sandbox scene has no player health");

            bool died = false;
            target.Died += () => died = true;

            // Dropped into the corridor a step in front of it; the controller has to be off to be moved.
            var body = Object.FindAnyObjectByType<CharacterController>();
            body.enabled = false;
            body.transform.position = _enemy.transform.position + _enemy.transform.forward * 1.5f;
            body.enabled = true;

            bool hunted = false;
            for (float waited = 0f; waited < 8f && !died; waited += Time.deltaTime)
            {
                hunted |= target.IsHunted;
                yield return null;
            }

            Assert.IsTrue(hunted, "being chased has to be something the screen can show");
            Assert.AreEqual(0f, health.Normalized, 0.001f);
            Assert.IsFalse(target.IsAlive);
            Assert.IsTrue(died, "the end of a run has to be announced");
        }

        // A noise too far off is not worth turning around for.
        [UnityTest]
        public IEnumerator AStepAcrossTheBuildingIsIgnored()
        {
            var noise = Services.Get<INoiseService>();
            noise.Emit(new NoiseEvent(_enemy.transform.position + Vector3.right * 40f, 8f, NoiseType.Footstep));

            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual(EnemyStateId.Patrol, _enemy.State);
        }
    }
}
