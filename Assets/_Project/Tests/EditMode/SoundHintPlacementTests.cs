using DarkDepths.UI;
using NUnit.Framework;
using UnityEngine;

namespace DarkDepths.Tests.EditMode
{
    public sealed class SoundHintPlacementTests
    {
        private const float MaxDistance = 14f;
        private const float Tolerance = 0.001f;

        private static readonly Vector3 Listener = new Vector3(0f, 1.6f, 0f);

        private static bool Place(Vector3 source, out float bearing, out float strength)
        {
            return SoundHintPlacement.TryPlace(Listener, Vector3.forward, source, MaxDistance,
                out bearing, out strength);
        }

        [Test]
        public void ASoundAheadPointsStraightUp()
        {
            Assert.IsTrue(Place(new Vector3(0f, 1.6f, 5f), out float bearing, out _));
            Assert.AreEqual(0f, bearing, Tolerance);
        }

        [Test]
        public void ASoundOnTheRightPointsRight()
        {
            Assert.IsTrue(Place(new Vector3(5f, 1.6f, 0f), out float bearing, out _));
            Assert.AreEqual(90f, bearing, Tolerance);
        }

        [Test]
        public void ASoundOnTheLeftPointsLeft()
        {
            Assert.IsTrue(Place(new Vector3(-5f, 1.6f, 0f), out float bearing, out _));
            Assert.AreEqual(-90f, bearing, Tolerance);
        }

        [Test]
        public void ASoundBehindPointsBack()
        {
            Assert.IsTrue(Place(new Vector3(0f, 1.6f, -5f), out float bearing, out _));
            Assert.AreEqual(180f, Mathf.Abs(bearing), Tolerance);
        }

        // The hint is a direction to turn towards, and the player turns on one axis only.
        [Test]
        public void HeightIsIgnored()
        {
            Place(new Vector3(5f, 1.6f, 0f), out float level, out float levelStrength);
            Place(new Vector3(5f, 9f, 0f), out float above, out float aboveStrength);

            Assert.AreEqual(level, above, Tolerance);
            Assert.AreEqual(levelStrength, aboveStrength, Tolerance);
        }

        [Test]
        public void StrengthFallsOffWithDistance()
        {
            Place(new Vector3(0f, 1.6f, 1f), out _, out float near);
            Place(new Vector3(0f, 1.6f, 7f), out _, out float far);

            Assert.AreEqual(0.5f, far, Tolerance);
            Assert.Greater(near, far);
        }

        [Test]
        public void TooFarIsNotShownAtAll()
        {
            Assert.IsFalse(Place(new Vector3(0f, 1.6f, MaxDistance), out _, out float strength));
            Assert.AreEqual(0f, strength, Tolerance);
        }

        [Test]
        public void ARangeOfNothingShowsNothing()
        {
            Assert.IsFalse(SoundHintPlacement.TryPlace(Listener, Vector3.forward, Vector3.zero, 0f,
                out _, out _));
        }

        // Looking straight down leaves no heading to measure against; the mark goes to the top rather than nowhere.
        [Test]
        public void ACameraLookingDownStillShowsTheSound()
        {
            Assert.IsTrue(SoundHintPlacement.TryPlace(Listener, Vector3.down, new Vector3(3f, 1.6f, 0f),
                MaxDistance, out float bearing, out float strength));
            Assert.AreEqual(0f, bearing, Tolerance);
            Assert.Greater(strength, 0f);
        }

        [Test]
        public void ASoundOnTopOfThePlayerIsAtFullStrength()
        {
            Assert.IsTrue(Place(Listener, out float bearing, out float strength));
            Assert.AreEqual(0f, bearing, Tolerance);
            Assert.AreEqual(1f, strength, Tolerance);
        }
    }
}
