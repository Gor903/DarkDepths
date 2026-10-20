using DarkDepths.Player;
using NUnit.Framework;
using UnityEngine;

namespace DarkDepths.Tests.EditMode
{
    public sealed class LookSmootherTests
    {
        private const float Smoothing = 0.05f;
        private const float Frame = 1f / 60f;

        [Test]
        public void PassesInputStraightThroughWhenSmoothingIsOff()
        {
            var smoother = new LookSmoother(0f);

            Vector2 step = smoother.Step(new Vector2(10f, -4f), Frame);

            Assert.AreEqual(new Vector2(10f, -4f), step);
            Assert.AreEqual(Vector2.zero, smoother.Pending);
        }

        [Test]
        public void HoldsBackPartOfASpikeInsteadOfApplyingItAtOnce()
        {
            var smoother = new LookSmoother(Smoothing);

            Vector2 step = smoother.Step(new Vector2(60f, 0f), Frame);

            Assert.Less(step.x, 60f);
            Assert.Greater(step.x, 0f);
        }

        [Test]
        public void DeliversEveryDegreeOfASpikeOverTime()
        {
            var smoother = new LookSmoother(Smoothing);
            float total = smoother.Step(new Vector2(60f, 0f), Frame).x;

            for (int i = 0; i < 120; i++)
            {
                total += smoother.Step(Vector2.zero, Frame).x;
            }

            // Smoothing may delay the rotation but must never silently eat part of a swipe.
            Assert.AreEqual(60f, total, 0.01f);
            Assert.AreEqual(Vector2.zero, smoother.Pending);
        }

        [Test]
        public void KeepsUpWithSustainedInput()
        {
            var smoother = new LookSmoother(Smoothing);
            float total = 0f;

            for (int i = 0; i < 240; i++)
            {
                total += smoother.Step(new Vector2(2f, 0f), Frame).x;
            }

            // Only a fraction of one frame of input may ever be outstanding.
            Assert.That(total, Is.GreaterThan(470f).And.LessThanOrEqualTo(480f));
        }

        [Test]
        public void SettlesExactlyInsteadOfCreepingForever()
        {
            var smoother = new LookSmoother(Smoothing);
            smoother.Step(new Vector2(1f, 1f), Frame);

            for (int i = 0; i < 200; i++)
            {
                smoother.Step(Vector2.zero, Frame);
            }

            Assert.AreEqual(Vector2.zero, smoother.Pending);
        }

        [Test]
        public void FlushesEverythingPendingWhenTheFrameTimeIsUnusable()
        {
            var smoother = new LookSmoother(Smoothing);
            smoother.Step(new Vector2(30f, 0f), Frame);

            Vector2 rest = smoother.Step(Vector2.zero, 0f);

            Assert.Greater(rest.x, 0f);
            Assert.AreEqual(Vector2.zero, smoother.Pending);
        }

        [Test]
        public void SmoothsBothAxesTogether()
        {
            var smoother = new LookSmoother(Smoothing);

            Vector2 step = smoother.Step(new Vector2(20f, -20f), Frame);

            Assert.AreEqual(step.x, -step.y, 0.0001f);
        }
    }
}
