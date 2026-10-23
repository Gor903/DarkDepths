using DarkDepths.Noise;
using NUnit.Framework;

namespace DarkDepths.Tests.EditMode
{
    public sealed class NoiseFalloffTests
    {
        private const float Tolerance = 0.0001f;
        private const float WallMultiplier = 0.5f;

        [Test]
        public void ReachIsLoudnessTimesHearingTimesSurface()
        {
            float radius = NoiseFalloff.Radius(8f, 1.5f, 1.1f, false, WallMultiplier);

            Assert.AreEqual(8f * 1.5f * 1.1f, radius, Tolerance);
        }

        // docs/04: a wall does not stop a sound, it halves how far it carries.
        [Test]
        public void AWallHalvesTheReach()
        {
            float clear = NoiseFalloff.Radius(8f, 1f, 1f, false, WallMultiplier);
            float blocked = NoiseFalloff.Radius(8f, 1f, 1f, true, WallMultiplier);

            Assert.AreEqual(clear * WallMultiplier, blocked, Tolerance);
        }

        [Test]
        public void ADeafListenerHearsNothing()
        {
            Assert.AreEqual(0f, NoiseFalloff.Radius(15f, 0f, 1.3f, false, WallMultiplier), Tolerance);
        }

        [Test]
        public void ASilentEventReachesNobody()
        {
            Assert.AreEqual(0f, NoiseFalloff.Radius(0f, 1f, 1f, false, WallMultiplier), Tolerance);
        }

        [Test]
        public void ASurfaceThatSwallowsEverythingLeavesNoReach()
        {
            Assert.AreEqual(0f, NoiseFalloff.Radius(8f, 1f, 0f, false, WallMultiplier), Tolerance);
        }

        [Test]
        public void AtTheSourceItIsFullStrength()
        {
            Assert.AreEqual(1f, NoiseFalloff.Perceived(0f, 10f), Tolerance);
        }

        [Test]
        public void HalfwayOutItIsHalfStrength()
        {
            Assert.AreEqual(0.5f, NoiseFalloff.Perceived(5f, 10f), Tolerance);
        }

        [Test]
        public void AtTheEdgeAndBeyondItIsNothing()
        {
            Assert.AreEqual(0f, NoiseFalloff.Perceived(10f, 10f), Tolerance);
            Assert.AreEqual(0f, NoiseFalloff.Perceived(12f, 10f), Tolerance);
        }

        [Test]
        public void NoReachMeansNothingIsHeardEvenOnTopOfIt()
        {
            Assert.AreEqual(0f, NoiseFalloff.Perceived(0f, 0f), Tolerance);
        }

        // Carpet against metal is the comparison the balance of docs/04 rests on.
        [Test]
        public void TheSameStepCarriesFurtherOnMetalThanOnCarpet()
        {
            float carpet = NoiseFalloff.Radius(8f, 1f, 0.6f, false, WallMultiplier);
            float metal = NoiseFalloff.Radius(8f, 1f, 1.3f, false, WallMultiplier);

            Assert.Less(carpet, metal);
            Assert.AreEqual(4.8f, carpet, Tolerance);
            Assert.AreEqual(10.4f, metal, Tolerance);
        }
    }
}
