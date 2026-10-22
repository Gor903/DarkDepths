using DarkDepths.Core;
using NUnit.Framework;

namespace DarkDepths.Tests.EditMode
{
    public sealed class PostProcessBudgetTests
    {
        private const float Tolerance = 0.0001f;

        private const float Vignette = 0.3f;
        private const float Grain = 0.2f;
        private const float Bloom = 0.6f;

        [Test]
        public void AtFullStrengthTheConfiguredValuesCarryThrough()
        {
            var budget = new PostProcessBudget(Vignette, Grain, Bloom, 0f, false);

            Assert.AreEqual(Vignette, budget.VignetteIntensity, Tolerance);
            Assert.AreEqual(Grain, budget.GrainIntensity, Tolerance);
            Assert.AreEqual(Bloom, budget.BloomIntensity, Tolerance);
            Assert.IsTrue(budget.AnyActive);
        }

        [Test]
        public void ReducingScalesEveryEffectTogether()
        {
            var budget = new PostProcessBudget(Vignette, Grain, Bloom, 0.5f, true);

            Assert.AreEqual(Vignette * 0.5f, budget.VignetteIntensity, Tolerance);
            Assert.AreEqual(Grain * 0.5f, budget.GrainIntensity, Tolerance);
            Assert.AreEqual(Bloom * 0.5f, budget.BloomIntensity, Tolerance);
        }

        [Test]
        public void ReducingToNothingSwitchesTheStackOff()
        {
            var budget = new PostProcessBudget(Vignette, Grain, Bloom, 0f, true);

            Assert.AreEqual(0f, budget.VignetteIntensity, Tolerance);
            Assert.IsFalse(budget.VignetteActive);
            Assert.IsFalse(budget.GrainActive);
            Assert.IsFalse(budget.BloomActive);
            Assert.IsFalse(budget.AnyActive);
        }

        [Test]
        public void AScaleAboveOneCannotTurnEffectsUp()
        {
            var budget = new PostProcessBudget(Vignette, Grain, Bloom, 4f, true);

            Assert.AreEqual(Vignette, budget.VignetteIntensity, Tolerance);
        }

        [Test]
        public void NegativeSettingsCollapseInsteadOfInverting()
        {
            var budget = new PostProcessBudget(-Vignette, -Grain, -Bloom, -1f, false);

            Assert.AreEqual(0f, budget.VignetteIntensity, Tolerance);
            Assert.AreEqual(0f, budget.GrainIntensity, Tolerance);
            Assert.AreEqual(0f, budget.BloomIntensity, Tolerance);
            Assert.IsFalse(budget.AnyActive);
        }

        [Test]
        public void OneEffectLeftStandingKeepsTheStackOn()
        {
            var budget = new PostProcessBudget(0f, 0f, Bloom, 0f, false);

            Assert.IsFalse(budget.VignetteActive);
            Assert.IsTrue(budget.BloomActive);
            Assert.IsTrue(budget.AnyActive);
        }
    }
}
