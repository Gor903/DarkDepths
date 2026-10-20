using DarkDepths.World;
using NUnit.Framework;
using UnityEngine;

namespace DarkDepths.Tests.EditMode
{
    public sealed class DoorSwingTests
    {
        private const float OpenAngle = 95f;
        private const float SmoothTime = 0.28f;
        private const float MaxSpeed = 260f;

        // A positive side means the interactor stands on the +Z face of the closed leaf.
        private const float InFront = 1f;
        private const float Behind = -1f;

        private static DoorSwing Create()
        {
            return new DoorSwing(OpenAngle, SmoothTime, MaxSpeed);
        }

        private static void Settle(DoorSwing swing)
        {
            for (int i = 0; i < 300 && swing.IsMoving; i++)
            {
                swing.Step(0.02f);
            }
        }

        [Test]
        public void StartsClosedAndStill()
        {
            var swing = Create();

            Assert.IsFalse(swing.IsOpen);
            Assert.IsFalse(swing.IsMoving);
            Assert.AreEqual(0f, swing.Angle);
        }

        [Test]
        public void SwingsAwayFromAnInteractorInFront()
        {
            var swing = Create();

            swing.Toggle(InFront);

            // A leaf swinging toward the interactor would sweep straight through them.
            Assert.AreEqual(OpenAngle, swing.Target);
        }

        [Test]
        public void SwingsAwayFromAnInteractorBehind()
        {
            var swing = Create();

            swing.Toggle(Behind);

            Assert.AreEqual(-OpenAngle, swing.Target);
        }

        [Test]
        public void ReachesTheOpenAngleAndStops()
        {
            var swing = Create();
            swing.Toggle(InFront);

            Settle(swing);

            Assert.AreEqual(OpenAngle, swing.Angle);
            Assert.IsFalse(swing.IsMoving);
        }

        [Test]
        public void ClosesBackToZero()
        {
            var swing = Create();
            swing.Toggle(InFront);
            Settle(swing);

            swing.Toggle(InFront);
            Settle(swing);

            Assert.IsFalse(swing.IsOpen);
            Assert.AreEqual(0f, swing.Angle);
        }

        [Test]
        public void ReopensAfterClosing()
        {
            var swing = Create();
            swing.Toggle(InFront);
            Settle(swing);
            swing.Toggle(InFront);
            Settle(swing);

            swing.Toggle(InFront);
            Settle(swing);

            Assert.IsTrue(swing.IsOpen);
            Assert.AreEqual(OpenAngle, swing.Angle);
        }

        [Test]
        public void PicksTheSideAgainOnceFullyClosed()
        {
            var swing = Create();
            swing.Toggle(InFront);
            Settle(swing);
            swing.Toggle(InFront);
            Settle(swing);

            swing.Toggle(Behind);

            Assert.AreEqual(-OpenAngle, swing.Target);
        }

        [Test]
        public void KeepsItsSideWhenReversedMidSwing()
        {
            var swing = Create();
            swing.Toggle(InFront);
            swing.Step(0.02f);

            // Closing and reopening halfway must not flip the leaf to the other side of the frame.
            swing.Toggle(Behind);
            swing.Toggle(Behind);

            Assert.AreEqual(OpenAngle, swing.Target);
        }

        [Test]
        public void HoldStopsTheLeafWhereItWas()
        {
            var swing = Create();
            swing.Toggle(InFront);
            float before = swing.Angle;
            swing.Step(0.02f);

            swing.Hold(before);

            Assert.AreEqual(before, swing.Angle);
            Assert.IsTrue(swing.IsMoving, "A held leaf still wants its target once the way is clear.");
        }

        [Test]
        public void ResumesAfterBeingHeld()
        {
            var swing = Create();
            swing.Toggle(InFront);
            swing.Step(0.02f);
            swing.Hold(0f);

            Settle(swing);

            Assert.AreEqual(OpenAngle, swing.Angle);
        }

        [Test]
        public void EasesInInsteadOfMovingAtAConstantRate()
        {
            var swing = Create();
            swing.Toggle(InFront);

            float first = swing.Step(0.02f);
            float second = swing.Step(0.02f) - first;

            Assert.Greater(second, first, "A constant rate would make both steps equal; the swing should ramp up.");

            Settle(swing);
            Assert.AreEqual(OpenAngle, swing.Angle);
        }

        [Test]
        public void TakesTheSwingSideFromTheInteractorNotFromTheConfigSign()
        {
            var swing = new DoorSwing(-OpenAngle, SmoothTime, MaxSpeed);

            swing.Toggle(InFront);

            Assert.AreEqual(OpenAngle, swing.Target);
            Assert.IsTrue(Mathf.Approximately(0f, swing.Angle));
        }
    }
}
