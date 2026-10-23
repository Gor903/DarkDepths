using DarkDepths.Player;
using NUnit.Framework;

namespace DarkDepths.Tests.EditMode
{
    public sealed class StrideMeterTests
    {
        private const float CrouchStride = 1f;
        private const float WalkStride = 1.5f;
        private const float SprintStride = 2f;
        private const float Tolerance = 0.0001f;

        // A fraction of one means no head start: the first step costs a whole stride like every other.
        private static StrideMeter Meter(float firstStepFraction = 1f)
        {
            return new StrideMeter(CrouchStride, WalkStride, SprintStride, firstStepFraction);
        }

        [Test]
        public void OneStrideIsOneStep()
        {
            StrideMeter meter = Meter();

            Assert.IsTrue(meter.Advance(WalkStride, Gait.Walk));
        }

        [Test]
        public void HalfAStrideIsNoStepYet()
        {
            StrideMeter meter = Meter();

            Assert.IsFalse(meter.Advance(WalkStride * 0.5f, Gait.Walk));
            Assert.IsTrue(meter.Advance(WalkStride * 0.5f, Gait.Walk));
        }

        [Test]
        public void TheLeftoverOfAStrideCarriesIntoTheNextOne()
        {
            StrideMeter meter = Meter();
            meter.Advance(WalkStride * 1.5f, Gait.Walk);

            // Half a stride is already behind, so half a stride more is the next step.
            Assert.IsFalse(meter.Advance(WalkStride * 0.4f, Gait.Walk));
            Assert.IsTrue(meter.Advance(WalkStride * 0.2f, Gait.Walk));
        }

        [Test]
        public void CrouchingTakesMoreStepsOverTheSameDistanceThanSprinting()
        {
            Assert.AreEqual(CrouchStride, Meter().StrideFor(Gait.Crouch), Tolerance);
            Assert.AreEqual(WalkStride, Meter().StrideFor(Gait.Walk), Tolerance);
            Assert.AreEqual(SprintStride, Meter().StrideFor(Gait.Sprint), Tolerance);

            Assert.That(StepsOver(10f, Gait.Crouch), Is.EqualTo(10).Within(1));
            Assert.That(StepsOver(10f, Gait.Sprint), Is.EqualTo(5).Within(1));
        }

        [Test]
        public void ALongFrameFiresOneStepAndNotABurst()
        {
            StrideMeter meter = Meter();

            Assert.IsTrue(meter.Advance(WalkStride * 20f, Gait.Walk));
            Assert.IsFalse(meter.Advance(0.01f, Gait.Walk));
        }

        [Test]
        public void StandingStillCostsNothing()
        {
            StrideMeter meter = Meter();

            Assert.IsFalse(meter.Advance(0f, Gait.Walk));
            Assert.IsFalse(meter.Advance(-5f, Gait.Walk));
        }

        [Test]
        public void TheFirstStepAfterAStopComesEarly()
        {
            StrideMeter meter = Meter(0.4f);

            Assert.IsFalse(meter.Advance(WalkStride * 0.3f, Gait.Walk));
            Assert.IsTrue(meter.Advance(WalkStride * 0.15f, Gait.Walk));
        }

        [Test]
        public void StoppingRearmsTheFirstStep()
        {
            StrideMeter meter = Meter(0.4f);
            meter.Advance(WalkStride * 0.9f, Gait.Walk);

            meter.Idle();

            Assert.AreEqual(0.6f, meter.Phase, Tolerance);
            Assert.IsTrue(meter.Advance(WalkStride * 0.4f, Gait.Walk));
        }

        [Test]
        public void ChangingGaitKeepsHowFarIntoTheStrideTheWalkerIs()
        {
            StrideMeter meter = Meter();
            meter.Advance(WalkStride * 0.5f, Gait.Walk);

            // Half a stride in, whatever the stride now measures.
            Assert.IsFalse(meter.Advance(SprintStride * 0.4f, Gait.Sprint));
            Assert.IsTrue(meter.Advance(SprintStride * 0.2f, Gait.Sprint));
        }

        [Test]
        public void AStrideOfZeroDoesNotDivideByNothing()
        {
            var meter = new StrideMeter(0f, 0f, 0f, 1f);

            Assert.Greater(meter.StrideFor(Gait.Walk), 0f);
            Assert.IsTrue(meter.Advance(1f, Gait.Walk));
        }

        private static int StepsOver(float distance, Gait gait)
        {
            StrideMeter meter = Meter();
            int steps = 0;
            const int slices = 1000;

            for (int i = 0; i < slices; i++)
            {
                if (meter.Advance(distance / slices, gait))
                {
                    steps++;
                }
            }

            return steps;
        }
    }
}
