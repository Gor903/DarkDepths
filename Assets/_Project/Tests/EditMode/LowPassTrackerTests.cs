using DarkDepths.Audio;
using NUnit.Framework;
using UnityEngine;

namespace DarkDepths.Tests.EditMode
{
    public sealed class LowPassTrackerTests
    {
        private const float Open = 22000f;
        private const float Blocked = 1100f;
        private const float SmoothTime = 0.2f;

        private static LowPassTracker Tracker()
        {
            return new LowPassTracker(Open, Blocked, SmoothTime);
        }

        [Test]
        public void StartsWideOpen()
        {
            Assert.AreEqual(Open, Tracker().Cutoff, 0.01f);
        }

        [Test]
        public void ReachesTheMuffledCutoffAfterTheSmoothTime()
        {
            LowPassTracker tracker = Tracker();
            for (int step = 0; step < 20; step++)
            {
                tracker.Step(true, SmoothTime / 20f);
            }

            Assert.AreEqual(Blocked, tracker.Cutoff, 0.01f);
        }

        [Test]
        public void HalfwayThroughItIsHalfwayInRatio()
        {
            LowPassTracker tracker = Tracker();
            tracker.Step(true, SmoothTime * 0.5f);

            // Half of the way measured the way the ear measures it: the geometric middle, not the arithmetic one.
            float expected = Mathf.Sqrt(Open * Blocked);
            Assert.AreEqual(expected, tracker.Cutoff, expected * 0.001f);
        }

        [Test]
        public void NeverPassesEitherEnd()
        {
            LowPassTracker tracker = Tracker();
            for (int step = 0; step < 50; step++)
            {
                float cutoff = tracker.Step(true, SmoothTime);
                Assert.GreaterOrEqual(cutoff, Blocked - 0.01f);
                Assert.LessOrEqual(cutoff, Open + 0.01f);
            }
        }

        [Test]
        public void OpensBackUpWhenTheWayIsClear()
        {
            LowPassTracker tracker = Tracker();
            tracker.Step(true, SmoothTime);
            tracker.Step(false, SmoothTime);

            Assert.AreEqual(Open, tracker.Cutoff, 0.01f);
        }

        [Test]
        public void ResetSnapsInsteadOfSweeping()
        {
            LowPassTracker tracker = Tracker();

            Assert.AreEqual(Blocked, tracker.Reset(true), 0.01f);
            Assert.AreEqual(Open, tracker.Reset(false), 0.01f);
        }

        [Test]
        public void ASmoothTimeOfZeroIsImmediate()
        {
            var tracker = new LowPassTracker(Open, Blocked, 0f);

            Assert.AreEqual(Blocked, tracker.Step(true, 0.016f), 0.01f);
        }

        [Test]
        public void ACutoffAboveTheOpenOneIsClamped()
        {
            var tracker = new LowPassTracker(Open, Open * 2f, SmoothTime);

            Assert.AreEqual(Open, tracker.Step(true, SmoothTime), 0.01f);
        }
    }
}
