using DarkDepths.Player;
using NUnit.Framework;
using UnityEngine;

namespace DarkDepths.Tests.EditMode
{
    public sealed class BeamSwayTests
    {
        private const float Step = 1f / 60f;

        // The defaults of FlashlightConfig, so a test failure means the beam changed, not the asset.
        private static BeamSway Sway(float breath = 0f, float bob = 0f)
        {
            return new BeamSway(0.12f, 0.7f, 6f, breath, 1.1f, bob, 8f);
        }

        private static Vector2 Settle(BeamSway sway, int frames, float stride = 0f)
        {
            var offset = Vector2.zero;

            for (int i = 0; i < frames; i++)
            {
                offset = sway.Advance(Vector2.zero, stride, Step);
            }

            return offset;
        }

        [Test]
        public void AStillViewLeavesTheBeamWhereItLooks()
        {
            BeamSway sway = Sway();

            Assert.AreEqual(Vector2.zero, Settle(sway, 10));
        }

        [Test]
        public void TheBeamFallsBehindAViewThatTurns()
        {
            BeamSway sway = Sway();

            Vector2 offset = sway.Advance(new Vector2(0f, 4f), 0f, Step);

            // The view turned right, so the beam is left of it until the spring catches up.
            Assert.Less(offset.y, 0f);
        }

        [Test]
        public void TheBeamCatchesUpWithinTheSwayTime()
        {
            BeamSway sway = Sway();
            sway.Advance(new Vector2(0f, 4f), 0f, Step);

            // Four sway times is long enough that anything left is overshoot, not lag.
            Vector2 offset = Settle(sway, 29);

            Assert.Less(Mathf.Abs(offset.y), 0.5f);
        }

        [Test]
        public void TheBeamOvershootsOnTheWayBack()
        {
            BeamSway sway = Sway();
            sway.Advance(new Vector2(0f, 6f), 0f, Step);

            bool crossed = false;
            for (int i = 0; i < 60 && !crossed; i++)
            {
                crossed = sway.Advance(Vector2.zero, 0f, Step).y > 0f;
            }

            Assert.IsTrue(crossed, "A damping below 1 has to carry the beam past the view, not stop at it.");
        }

        [Test]
        public void TheBeamNeverLeavesTheCap()
        {
            BeamSway sway = Sway();

            for (int i = 0; i < 120; i++)
            {
                // A sustained fast turn: far more than a thumb can do in one frame.
                Vector2 offset = sway.Advance(new Vector2(12f, 12f), 0f, Step);
                Assert.LessOrEqual(sway.Lag.magnitude, 6f + 0.001f);
                Assert.LessOrEqual(offset.magnitude, 6f + 0.001f);
            }
        }

        [Test]
        public void AFrameHitchIsIntegratedInOneBoundedStep()
        {
            BeamSway sway = Sway();

            Vector2 offset = sway.Advance(new Vector2(0f, 5f), 0f, 2f);

            Assert.IsFalse(float.IsNaN(offset.x) || float.IsNaN(offset.y));
            Assert.LessOrEqual(offset.magnitude, 6f + 0.001f);
        }

        [Test]
        public void BreathingKeepsTheBeamFromStandingPerfectlyStill()
        {
            BeamSway sway = Sway(breath: 0.3f);

            Vector2 offset = Settle(sway, 20);

            Assert.Greater(offset.magnitude, 0f);
            // Breathing is a tell, not a sway: it stays inside its own amplitude.
            Assert.LessOrEqual(offset.magnitude, 0.3f * 1.5f);
        }

        [Test]
        public void TheBobOnlyMovesWhileWalking()
        {
            BeamSway standing = Sway(bob: 0.6f);
            BeamSway walking = Sway(bob: 0.6f);

            float still = Settle(standing, 20).magnitude;
            float moving = 0f;
            for (int i = 0; i < 20; i++)
            {
                moving = Mathf.Max(moving, Settle(walking, 1, 1f).magnitude);
            }

            Assert.AreEqual(0f, still);
            Assert.Greater(moving, 0f);
        }
    }
}
