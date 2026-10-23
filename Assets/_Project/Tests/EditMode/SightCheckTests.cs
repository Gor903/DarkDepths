using DarkDepths.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace DarkDepths.Tests.EditMode
{
    public sealed class SightCheckTests
    {
        private const float Range = 3f;
        private const float Angle = 90f;
        private const float Tolerance = 0.001f;

        private static readonly Vector3 Eye = new Vector3(0f, 1.6f, 0f);

        [Test]
        public void TheFlashlightGivesThePlayerAway()
        {
            Assert.AreEqual(Range, SightCheck.RangeFor(Range, 0.5f, false), Tolerance);
            Assert.AreEqual(Range * 1.5f, SightCheck.RangeFor(Range, 0.5f, true), Tolerance);
        }

        [Test]
        public void ABlindEnemySeesNothingLitOrNot()
        {
            Assert.AreEqual(0f, SightCheck.RangeFor(0f, 2f, true), Tolerance);
            Assert.IsFalse(SightCheck.IsInView(Eye, Vector3.forward, Eye + Vector3.forward, 0f, Angle));
        }

        [Test]
        public void StraightAheadAndCloseIsSeen()
        {
            Assert.IsTrue(SightCheck.IsInView(Eye, Vector3.forward, Eye + Vector3.forward * 2f, Range, Angle));
        }

        [Test]
        public void TooFarIsNotSeen()
        {
            Assert.IsFalse(SightCheck.IsInView(Eye, Vector3.forward, Eye + Vector3.forward * 4f, Range, Angle));
        }

        // Ninety degrees means forty-five to each side, not ninety.
        [Test]
        public void OutsideTheConeIsNotSeen()
        {
            Vector3 side = Eye + new Vector3(2f, 0f, 0.2f);
            Assert.IsFalse(SightCheck.IsInView(Eye, Vector3.forward, side, Range, Angle));

            Vector3 oblique = Eye + new Vector3(1f, 0f, 1.4f);
            Assert.IsTrue(SightCheck.IsInView(Eye, Vector3.forward, oblique, Range, Angle));
        }

        [Test]
        public void BehindIsNeverSeen()
        {
            Assert.IsFalse(SightCheck.IsInView(Eye, Vector3.forward, Eye - Vector3.forward * 1.5f, Range, Angle));
        }

        // Height is not part of the cone: a player on a crate in front is still in front.
        [Test]
        public void HeightDoesNotNarrowTheCone()
        {
            Vector3 above = Eye + new Vector3(0f, 1.2f, 1.5f);
            Assert.IsTrue(SightCheck.IsInView(Eye, Vector3.forward, above, Range, Angle));
        }

        [Test]
        public void StandingOnTopOfTheEnemyIsSeen()
        {
            Assert.IsTrue(SightCheck.IsInView(Eye, Vector3.forward, Eye, Range, Angle));
        }
    }
}
