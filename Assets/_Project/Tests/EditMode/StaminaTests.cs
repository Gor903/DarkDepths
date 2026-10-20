using DarkDepths.Player;
using NUnit.Framework;

namespace DarkDepths.Tests.EditMode
{
    public sealed class StaminaTests
    {
        private const float Drain = 5f;
        private const float Recover = 8f;
        private const float Rearm = 0.2f;
        private const float Tolerance = 0.001f;

        // Extra simulated time so float accumulation cannot leave a test sitting exactly on a boundary.
        private const float Margin = 0.1f;

        [Test]
        public void StartsFull()
        {
            var stamina = new Stamina(Drain, Recover, Rearm);

            Assert.AreEqual(1f, stamina.Normalized, Tolerance);
            Assert.IsTrue(stamina.CanSprint);
        }

        [Test]
        public void EmptiesAfterSprintDuration()
        {
            var stamina = new Stamina(Drain, Recover, Rearm);

            Sprint(stamina, Drain);

            Assert.AreEqual(0f, stamina.Normalized, Tolerance);
        }

        [Test]
        public void LocksSprintWhenEmptied()
        {
            var stamina = new Stamina(Drain, Recover, Rearm);

            Sprint(stamina, Drain + Margin);

            Assert.IsFalse(stamina.CanSprint);
        }

        [Test]
        public void StaysLockedBelowRearmThreshold()
        {
            var stamina = new Stamina(Drain, Recover, Rearm);
            Sprint(stamina, Drain + Margin);

            // Half the time needed to reach the rearm threshold.
            Rest(stamina, Rearm * Recover * 0.5f);

            Assert.IsFalse(stamina.CanSprint);
            Assert.Less(stamina.Normalized, Rearm);
        }

        [Test]
        public void UnlocksAtRearmThreshold()
        {
            var stamina = new Stamina(Drain, Recover, Rearm);
            Sprint(stamina, Drain + Margin);

            Rest(stamina, Rearm * Recover + Margin);

            Assert.IsTrue(stamina.CanSprint);
        }

        [Test]
        public void RefillsAfterRecoveryDurationAndClamps()
        {
            var stamina = new Stamina(Drain, Recover, Rearm);
            Sprint(stamina, Drain + Margin);

            Rest(stamina, Recover * 2f);

            Assert.AreEqual(1f, stamina.Normalized, Tolerance);
        }

        [Test]
        public void DoesNotDrainWhileNotSprinting()
        {
            var stamina = new Stamina(Drain, Recover, Rearm);

            Rest(stamina, Drain);

            Assert.AreEqual(1f, stamina.Normalized, Tolerance);
        }

        private static void Sprint(Stamina stamina, float seconds)
        {
            Step(stamina, seconds, true);
        }

        private static void Rest(Stamina stamina, float seconds)
        {
            Step(stamina, seconds, false);
        }

        private static void Step(Stamina stamina, float seconds, bool sprinting)
        {
            const float deltaTime = 0.02f;
            int steps = (int)(seconds / deltaTime);
            for (int i = 0; i < steps; i++)
            {
                stamina.Tick(deltaTime, sprinting);
            }
        }
    }
}
