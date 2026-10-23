using System.Collections.Generic;
using DarkDepths.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace DarkDepths.Tests.EditMode
{
    public sealed class EnemyBrainTests
    {
        // A body that goes nowhere and reports what it was told to do.
        private sealed class StubBody : IEnemyBody
        {
            public Vector3 Position { get; set; }
            public bool HasArrived { get; set; }
            public Vector3 Destination { get; private set; }
            public float Speed { get; private set; }
            public int Moves { get; private set; }
            public int Stops { get; private set; }
            public int Looks { get; private set; }
            public bool HasGround { get; set; } = true;

            public void MoveTo(Vector3 destination, float speed)
            {
                Destination = destination;
                Speed = speed;
                Moves++;
                HasArrived = false;
            }

            public void Stop()
            {
                Stops++;
            }

            public void LookAround()
            {
                Looks++;
            }

            public bool TrySamplePointNear(Vector3 center, float radius, out Vector3 point)
            {
                // Always the same offset: the brain must not depend on where the point landed.
                point = center + new Vector3(radius, 0f, 0f);
                return HasGround;
            }
        }

        private EnemyDefinition _definition;
        private StubBody _body;
        private EnemyBrain _brain;

        [SetUp]
        public void SetUp()
        {
            _definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            _body = new StubBody();
            _brain = new EnemyBrain(_definition, _body);
            _brain.Begin(Vector3.zero);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_definition);
        }

        private void Tick(float seconds)
        {
            _brain.Tick(seconds);
        }

        [Test]
        public void ItStartsOnPatrol()
        {
            Assert.AreEqual(EnemyStateId.Patrol, _brain.State);
        }

        [Test]
        public void WithNoRouteItStandsItsPost()
        {
            Assert.AreEqual(0, _body.Moves);
            Assert.AreEqual(1, _body.Stops);
        }

        [Test]
        public void WithARouteItWalksThePointsInOrderAndStartsOver()
        {
            var route = new List<Vector3> { new Vector3(0f, 0f, 5f), new Vector3(3f, 0f, 0f) };
            _brain.SetPatrol(route);
            _brain.Switch(EnemyStateId.Patrol);

            Assert.AreEqual(route[0], _body.Destination);
            Assert.AreEqual(_definition.WalkSpeed, _body.Speed, 0.001f);

            _body.HasArrived = true;
            Tick(0.1f);
            Assert.AreEqual(route[1], _body.Destination);

            _body.HasArrived = true;
            Tick(0.1f);
            Assert.AreEqual(route[0], _body.Destination, "the route starts over at the end");
        }

        // docs/05: a noise under the threshold is not worth turning around for.
        [Test]
        public void AQuietNoiseIsIgnored()
        {
            _brain.Hear(new Vector3(4f, 0f, 0f), _definition.PerceivedThreshold * 0.5f);
            Tick(0.1f);

            Assert.AreEqual(EnemyStateId.Patrol, _brain.State);
        }

        [Test]
        public void ANoiseLeadsThroughAlertedToInvestigate()
        {
            var where = new Vector3(4f, 0f, 0f);
            _brain.Hear(where, 0.8f);
            Tick(0.1f);

            Assert.AreEqual(EnemyStateId.Alerted, _brain.State);
            Assert.AreEqual(1, _body.Looks, "the head turns before the body does");

            Tick(_definition.AlertDuration);

            Assert.AreEqual(EnemyStateId.Investigate, _brain.State);
            Assert.AreEqual(where, _body.Destination);
        }

        [Test]
        public void TheLoudestNoiseWins()
        {
            _brain.Hear(new Vector3(4f, 0f, 0f), 0.4f);
            _brain.Hear(new Vector3(-6f, 0f, 0f), 0.9f);
            _brain.Hear(new Vector3(9f, 0f, 0f), 0.2f);

            Assert.AreEqual(new Vector3(-6f, 0f, 0f), _brain.Interest);
            Assert.AreEqual(0.9f, _brain.InterestLoudness, 0.001f);
        }

        [Test]
        public void ALouderNoiseOnTheWayTakesOver()
        {
            Investigate(new Vector3(4f, 0f, 0f), 0.4f);

            var louder = new Vector3(-5f, 0f, 0f);
            _brain.Hear(louder, 0.9f);
            Tick(0.1f);

            Assert.AreEqual(EnemyStateId.Investigate, _brain.State);
            Assert.AreEqual(louder, _body.Destination);
        }

        [Test]
        public void NothingAtTheNoiseEndsInASearch()
        {
            Investigate(new Vector3(4f, 0f, 0f), 0.8f);

            _body.HasArrived = true;
            Tick(0.1f);
            Assert.AreEqual(EnemyStateId.Investigate, _brain.State, "it looks around before giving up");

            Tick(_definition.InvestigateTime);

            Assert.AreEqual(EnemyStateId.Search, _brain.State);
        }

        [Test]
        public void ASearchCastsAroundTheLastKnownSpot()
        {
            var where = new Vector3(4f, 0f, 0f);
            Investigate(where, 0.8f);
            _body.HasArrived = true;
            Tick(0.1f);
            Tick(_definition.InvestigateTime);

            Assert.AreEqual(EnemyStateId.Search, _brain.State);
            Assert.AreEqual(where + new Vector3(_definition.SearchRadius, 0f, 0f), _body.Destination);

            int moves = _body.Moves;
            _body.HasArrived = true;
            Tick(0.1f);
            Assert.AreEqual(moves + 1, _body.Moves, "reaching a point is followed by another one");
        }

        [Test]
        public void ASearchWithNowhereToWalkStandsAndListens()
        {
            _body.HasGround = false;
            Investigate(new Vector3(4f, 0f, 0f), 0.8f);
            _body.HasArrived = true;
            Tick(0.1f);
            Tick(_definition.InvestigateTime);

            int moves = _body.Moves;
            Assert.AreEqual(EnemyStateId.Search, _brain.State);
            Assert.AreEqual(moves, _body.Moves, "there is nowhere to go, so it does not go");
        }

        [Test]
        public void ASearchThatFindsNothingGoesHome()
        {
            Investigate(new Vector3(4f, 0f, 0f), 0.8f);
            _body.HasArrived = true;
            Tick(0.1f);
            Tick(_definition.InvestigateTime);
            Assert.AreEqual(EnemyStateId.Search, _brain.State);

            Tick(_definition.SearchTime);
            Assert.AreEqual(EnemyStateId.Return, _brain.State);
            Assert.AreEqual(_brain.Home, _body.Destination);

            _body.HasArrived = true;
            Tick(0.1f);
            Assert.AreEqual(EnemyStateId.Patrol, _brain.State);
        }

        // A noise during a search is a fresh lead, not the end of one.
        [Test]
        public void ANoiseDuringASearchStartsANewInvestigation()
        {
            Investigate(new Vector3(4f, 0f, 0f), 0.8f);
            _body.HasArrived = true;
            Tick(0.1f);
            Tick(_definition.InvestigateTime);
            Assert.AreEqual(EnemyStateId.Search, _brain.State);

            var fresh = new Vector3(-2f, 0f, 0f);
            _brain.Hear(fresh, 0.3f);
            Tick(0.1f);

            Assert.AreEqual(EnemyStateId.Investigate, _brain.State);
            Assert.AreEqual(fresh, _body.Destination);
        }

        [Test]
        public void ANoiseOnTheWayHomeTurnsItAroundAgain()
        {
            _brain.Switch(EnemyStateId.Return);
            _brain.Hear(new Vector3(7f, 0f, 0f), 0.5f);
            Tick(0.1f);

            Assert.AreEqual(EnemyStateId.Alerted, _brain.State);
        }

        [Test]
        public void GoingBackOnPatrolWritesOffWhatItWasChasing()
        {
            _brain.Hear(new Vector3(4f, 0f, 0f), 0.8f);
            Assert.IsTrue(_brain.HasInterest);

            _brain.Switch(EnemyStateId.Patrol);

            Assert.IsFalse(_brain.HasInterest);
        }

        private void Investigate(Vector3 where, float loudness)
        {
            _brain.Hear(where, loudness);
            Tick(0.1f);
            Tick(_definition.AlertDuration);
            Assert.AreEqual(EnemyStateId.Investigate, _brain.State);
        }
    }
}
