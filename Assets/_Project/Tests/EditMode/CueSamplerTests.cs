using DarkDepths.Audio;
using NUnit.Framework;

namespace DarkDepths.Tests.EditMode
{
    public sealed class CueSamplerTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void EveryClipOfACueCanBeReached()
        {
            Assert.AreEqual(0, CueSampler.PickIndex(3, 0f));
            Assert.AreEqual(1, CueSampler.PickIndex(3, 0.5f));
            Assert.AreEqual(2, CueSampler.PickIndex(3, 0.9f));
        }

        [Test]
        public void TheTopOfTheRangeStaysInsideTheClipList()
        {
            Assert.AreEqual(2, CueSampler.PickIndex(3, 1f));
            Assert.AreEqual(2, CueSampler.PickIndex(3, 4f));
        }

        [Test]
        public void ACueWithoutClipsPicksNothing()
        {
            Assert.AreEqual(-1, CueSampler.PickIndex(0, 0.5f));
        }

        [Test]
        public void VariationStaysBetweenTheTwoEnds()
        {
            Assert.AreEqual(0.9f, CueSampler.PickBetween(0.9f, 1f, 0f), Tolerance);
            Assert.AreEqual(0.95f, CueSampler.PickBetween(0.9f, 1f, 0.5f), Tolerance);
            Assert.AreEqual(1f, CueSampler.PickBetween(0.9f, 1f, 1f), Tolerance);
        }

        [Test]
        public void EndsEnteredTheWrongWayRoundStillWork()
        {
            Assert.AreEqual(0.9f, CueSampler.PickBetween(1f, 0.9f, 0f), Tolerance);
            Assert.AreEqual(1f, CueSampler.PickBetween(1f, 0.9f, 1f), Tolerance);
        }

        [Test]
        public void APickOutsideTheRangeCannotPushVolumePastItsLimit()
        {
            Assert.AreEqual(1f, CueSampler.PickBetween(0.9f, 1f, 3f), Tolerance);
            Assert.AreEqual(0.9f, CueSampler.PickBetween(0.9f, 1f, -3f), Tolerance);
        }
    }
}
