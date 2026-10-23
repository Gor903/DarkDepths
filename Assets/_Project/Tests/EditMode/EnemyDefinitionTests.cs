using DarkDepths.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace DarkDepths.Tests.EditMode
{
    public sealed class EnemyDefinitionTests
    {
        private const float Tolerance = 0.0001f;

        private EnemyDefinition _definition;

        [SetUp]
        public void SetUp()
        {
            _definition = ScriptableObject.CreateInstance<EnemyDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_definition);
        }

        // A fresh definition is the Listener of docs/05, down to the numbers.
        [Test]
        public void ItStartsAsTheListener()
        {
            Assert.AreEqual(1.5f, _definition.WalkSpeed, Tolerance);
            Assert.AreEqual(3.5f, _definition.ChaseSpeed, Tolerance);
            Assert.AreEqual(1.5f, _definition.HearingMultiplier, Tolerance);
            Assert.AreEqual(3f, _definition.SightRange, Tolerance);
            Assert.AreEqual(90f, _definition.SightAngle, Tolerance);
            Assert.AreEqual(1.2f, _definition.AttackRange, Tolerance);
            Assert.AreEqual(30f, _definition.AttackDamage, Tolerance);
            Assert.AreEqual(1.5f, _definition.AttackCooldown, Tolerance);
            Assert.AreEqual(4f, _definition.InvestigateTime, Tolerance);
            Assert.AreEqual(10f, _definition.SearchTime, Tolerance);
            Assert.AreEqual(3f, _definition.LoseTargetTime, Tolerance);
        }

        // What makes it a Listener rather than a Hunter: it hears well and barely sees.
        [Test]
        public void ItHearsBetterThanItSees()
        {
            Assert.Greater(_definition.HearingMultiplier, 1f);
            Assert.LessOrEqual(_definition.SightRange, 3f);
        }

        [Test]
        public void ItRunsFasterThanItWalksAndStrikesCloserThanItSees()
        {
            Assert.Greater(_definition.ChaseSpeed, _definition.WalkSpeed);
            Assert.Less(_definition.AttackRange, _definition.SightRange);
        }

        // docs/05: five to ten decisions a second, never one a frame.
        [Test]
        public void ItThinksAFewTimesASecond()
        {
            Assert.GreaterOrEqual(_definition.TickInterval, 0.1f - Tolerance);
            Assert.LessOrEqual(_definition.TickInterval, 0.2f + Tolerance);
        }

        // The pause on hearing something has to be noticed and then over: docs/05 puts it in this range.
        [Test]
        public void ThePauseIsLongEnoughToReadAndShortEnoughToSurvive()
        {
            Assert.GreaterOrEqual(_definition.AlertDuration, 0.5f);
            Assert.LessOrEqual(_definition.AlertDuration, 1.5f);
        }

        [Test]
        public void ASearchOutlastsTheLookThatStartedIt()
        {
            Assert.Greater(_definition.SearchTime, _definition.InvestigateTime);
            Assert.Greater(_definition.SearchRadius, 0f);
        }
    }
}
