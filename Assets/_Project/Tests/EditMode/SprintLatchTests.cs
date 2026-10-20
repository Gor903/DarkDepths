using DarkDepths.Player;
using NUnit.Framework;
using UnityEngine;

namespace DarkDepths.Tests.EditMode
{
    public sealed class SprintLatchTests
    {
        private const float Engage = 0.9f;
        private const float Release = 0.55f;

        [Test]
        public void StartsDisengaged()
        {
            var latch = new SprintLatch(Engage, Release);

            Assert.IsFalse(latch.Engaged);
        }

        [Test]
        public void EngagesWhenPushedToTheTop()
        {
            var latch = new SprintLatch(Engage, Release);

            bool changed = latch.Evaluate(new Vector2(0f, 1f));

            Assert.IsTrue(changed);
            Assert.IsTrue(latch.Engaged);
        }

        [Test]
        public void DoesNotEngageBelowThreshold()
        {
            var latch = new SprintLatch(Engage, Release);

            latch.Evaluate(new Vector2(0f, 0.8f));

            Assert.IsFalse(latch.Engaged);
        }

        [Test]
        public void DoesNotEngageSideways()
        {
            var latch = new SprintLatch(Engage, Release);

            latch.Evaluate(new Vector2(1f, 0f));

            Assert.IsFalse(latch.Engaged);
        }

        [Test]
        public void DoesNotEngageBackwards()
        {
            var latch = new SprintLatch(Engage, Release);

            latch.Evaluate(new Vector2(0f, -1f));

            Assert.IsFalse(latch.Engaged);
        }

        [Test]
        public void StaysEngagedBetweenReleaseAndEngageThresholds()
        {
            var latch = new SprintLatch(Engage, Release);
            latch.Evaluate(new Vector2(0f, 1f));

            latch.Evaluate(new Vector2(0f, 0.7f));

            Assert.IsTrue(latch.Engaged, "Hysteresis should hold sprint between the two thresholds.");
        }

        [Test]
        public void DisengagesBelowReleaseThreshold()
        {
            var latch = new SprintLatch(Engage, Release);
            latch.Evaluate(new Vector2(0f, 1f));

            bool changed = latch.Evaluate(new Vector2(0f, 0.4f));

            Assert.IsTrue(changed);
            Assert.IsFalse(latch.Engaged);
        }

        [Test]
        public void DisengagesWhenTurnedSideways()
        {
            var latch = new SprintLatch(Engage, Release);
            latch.Evaluate(new Vector2(0f, 1f));

            latch.Evaluate(new Vector2(0.9f, 0.3f));

            Assert.IsFalse(latch.Engaged);
        }

        [Test]
        public void EvaluateReportsNoChangeWhenStateHolds()
        {
            var latch = new SprintLatch(Engage, Release);
            latch.Evaluate(new Vector2(0f, 1f));

            bool changed = latch.Evaluate(new Vector2(0f, 0.95f));

            Assert.IsFalse(changed);
            Assert.IsTrue(latch.Engaged);
        }

        [Test]
        public void ReleaseReportsChangeOnlyWhenEngaged()
        {
            var latch = new SprintLatch(Engage, Release);

            Assert.IsFalse(latch.Release());

            latch.Evaluate(new Vector2(0f, 1f));
            Assert.IsTrue(latch.Release());
            Assert.IsFalse(latch.Engaged);
        }

        [Test]
        public void ClampsReleaseThresholdBelowEngage()
        {
            var latch = new SprintLatch(0.9f, 0.95f);
            latch.Evaluate(new Vector2(0f, 1f));

            // A release threshold above engage would make sprint impossible to hold.
            latch.Evaluate(new Vector2(0f, 0.87f));

            Assert.IsTrue(latch.Engaged);
        }
    }
}
