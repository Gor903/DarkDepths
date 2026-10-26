using DarkDepths.Player;
using NUnit.Framework;
using UnityEngine;

namespace DarkDepths.Tests.EditMode
{
    public sealed class BounceLightTests
    {
        private const float Step = 1f / 60f;

        // The defaults of FlashlightConfig, so a failure means the bounce changed, not the asset.
        private static BounceLight Bounce(float gain = 0.5f, float colorInfluence = 0.7f,
            float maxIntensity = 3f)
        {
            return new BounceLight(gain, 0.6f, maxIntensity, colorInfluence, 0.08f, 0.03f, 0.1f, 0.1f);
        }

        // A wall straight ahead at the given distance, lit head-on by a beam of the given colour.
        private static BounceSample Wall(float distance, Color albedo, float beamIntensity = 9f,
            float spotAngle = 45f, Color beamColor = default)
        {
            return new BounceSample(
                new Vector3(0f, 0f, distance),
                Vector3.back,
                albedo,
                Vector3.forward,
                beamColor == default ? Color.white : beamColor,
                beamIntensity,
                spotAngle);
        }

        private static void Settle(BounceLight bounce, in BounceSample sample, int frames, bool lit = true)
        {
            for (int i = 0; i < frames; i++)
            {
                bounce.Advance(lit, sample, Step);
            }
        }

        [Test]
        public void ABeamThatReachesNothingLeavesNoPool()
        {
            BounceLight bounce = Bounce();

            Settle(bounce, default, 60, false);

            Assert.IsFalse(bounce.IsLit);
        }

        [Test]
        public void TheFarWallGivesBackAsMuchAsTheNearOne()
        {
            BounceLight bounce = Bounce();
            var albedo = new Color(0.5f, 0.5f, 0.5f);

            float near = bounce.TargetIntensity(Wall(1.5f, albedo));
            float far = bounce.TargetIntensity(Wall(9f, albedo));

            // The footprint grows with the square of the distance exactly as the beam thins out.
            Assert.AreEqual(near, far, 1e-5f);
        }

        [Test]
        public void ADarkWallGivesBackAlmostNothing()
        {
            BounceLight bounce = Bounce();

            float dark = bounce.TargetIntensity(Wall(4f, new Color32(0x1B, 0x20, 0x28, 0xFF)));
            float pale = bounce.TargetIntensity(Wall(4f, new Color(0.8f, 0.8f, 0.8f)));

            Assert.Less(dark, pale * 0.25f);
        }

        [Test]
        public void AWallTakenEdgeOnGivesBackLessThanOneHeadOn()
        {
            BounceLight bounce = Bounce();
            var albedo = new Color(0.5f, 0.5f, 0.5f);

            BounceSample headOn = Wall(4f, albedo);
            var grazing = new BounceSample(headOn.Point, new Vector3(-0.94f, 0f, -0.34f), albedo,
                Vector3.forward, Color.white, headOn.BeamIntensity, headOn.SpotAngle);

            Assert.Less(bounce.TargetIntensity(grazing), bounce.TargetIntensity(headOn));
        }

        [Test]
        public void AWideConeGivesBackMoreThanANarrowOne()
        {
            BounceLight bounce = Bounce();
            var albedo = new Color(0.5f, 0.5f, 0.5f);

            float narrow = bounce.TargetIntensity(Wall(4f, albedo, spotAngle: 20f));
            float wide = bounce.TargetIntensity(Wall(4f, albedo, spotAngle: 60f));

            Assert.Greater(wide, narrow);
        }

        [Test]
        public void TheCeilingHoldsAgainstAWhiteWallInYourFace()
        {
            BounceLight bounce = Bounce(maxIntensity: 3f);

            float flood = bounce.TargetIntensity(Wall(0.4f, Color.white, beamIntensity: 60f, spotAngle: 60f));

            Assert.AreEqual(3f, flood, 1e-4f);
        }

        [Test]
        public void TheGainScalesThePool()
        {
            var albedo = new Color(0.5f, 0.5f, 0.5f);

            float half = Bounce(gain: 0.5f).TargetIntensity(Wall(4f, albedo));
            float full = Bounce(gain: 1f).TargetIntensity(Wall(4f, albedo));

            Assert.AreEqual(half * 2f, full, 1e-4f);
        }

        [Test]
        public void ThePoolSitsOffTheSurfaceNotOnIt()
        {
            BounceLight bounce = Bounce();
            BounceSample wall = Wall(4f, Color.gray);

            Vector3 position = bounce.TargetPosition(wall);

            // Normal points back down -z, so the pool floats towards the player by the offset.
            Assert.AreEqual(4f - 0.6f, position.z, 1e-4f);
        }

        [Test]
        public void TheSurfaceTintsThePoolWithoutBrighteningIt()
        {
            BounceLight bounce = Bounce(colorInfluence: 1f);
            var red = new Color(0.4f, 0.05f, 0.05f);

            Color color = bounce.TargetColor(Wall(4f, red));

            // Hue of the wall, normalised: the brightness of the pool lives in the intensity alone.
            Assert.AreEqual(1f, color.r, 1e-4f);
            Assert.Greater(color.r, color.g);
            Assert.AreEqual(color.g, color.b, 1e-4f);
        }

        [Test]
        public void NoColourInfluenceKeepsTheTorchColour()
        {
            BounceLight bounce = Bounce(colorInfluence: 0f);
            var beam = new Color(0.96f, 0.94f, 0.9f);

            Color color = bounce.TargetColor(Wall(4f, new Color(0.1f, 0.6f, 0.1f), beamColor: beam));

            Assert.AreEqual(beam.r, color.r, 1e-4f);
            Assert.AreEqual(beam.g, color.g, 1e-4f);
            Assert.AreEqual(beam.b, color.b, 1e-4f);
        }

        [Test]
        public void ThePoolAppearsInPlaceInsteadOfSlidingInFromTheLastWall()
        {
            BounceLight bounce = Bounce();
            BounceSample behind = Wall(2f, Color.gray);
            Settle(bounce, behind, 60);

            // The torch goes off, then comes back pointing somewhere else entirely.
            Settle(bounce, default, 120, false);
            Assert.IsFalse(bounce.IsLit);

            var ahead = new BounceSample(new Vector3(20f, 0f, 0f), Vector3.left, Color.gray,
                Vector3.right, Color.white, 9f, 45f);
            bounce.Advance(true, ahead, Step);

            Assert.AreEqual(bounce.TargetPosition(ahead), bounce.Position);
        }

        [Test]
        public void ThePoolArrivesAtFullBrightnessInsteadOfFadingUp()
        {
            BounceLight bounce = Bounce();
            BounceSample wall = Wall(4f, new Color(0.6f, 0.6f, 0.6f));

            // The very first frame the beam lands on something, with nothing lit before it.
            bounce.Advance(true, wall, Step);

            Assert.AreEqual(bounce.TargetIntensity(wall), bounce.Intensity, 1e-4f);
        }

        [Test]
        public void CrossingOntoAPalerSurfaceBrightensWithinTwoFrames()
        {
            BounceLight quick = Bounce();
            // The same pool with one time for both directions, to show which number does the work.
            var even = new BounceLight(0.5f, 0.6f, 3f, 0.7f, 0.08f, 0.1f, 0.1f, 0.1f);

            BounceSample dark = Wall(4f, new Color(0.12f, 0.12f, 0.12f));
            Settle(quick, dark, 60);
            Settle(even, dark, 60);

            // The beam slides off the dark wall onto a pale cupboard without ever leaving a surface.
            BounceSample pale = Wall(4f, new Color(0.8f, 0.8f, 0.8f));
            Settle(quick, pale, 2);
            Settle(even, pale, 2);

            float target = quick.TargetIntensity(pale);
            Assert.Greater(quick.Intensity, target * 0.6f);
            Assert.Less(even.Intensity, target * 0.35f);
        }

        [Test]
        public void LeavingASurfaceFadesOutRatherThanSnappingOff()
        {
            BounceLight bounce = Bounce();
            BounceSample wall = Wall(4f, new Color(0.6f, 0.6f, 0.6f));
            Settle(bounce, wall, 60);

            Settle(bounce, default, 2, false);

            // Two frames of fall barely move it, where two frames of rise all but arrive.
            Assert.Greater(bounce.Intensity, bounce.TargetIntensity(wall) * 0.5f);
        }

        [Test]
        public void ThePoolReachesTheWallItWasGivenAndStaysThere()
        {
            BounceLight bounce = Bounce();
            BounceSample wall = Wall(4f, new Color(0.6f, 0.6f, 0.6f));

            Settle(bounce, wall, 120);

            Assert.IsTrue(bounce.IsLit);
            Assert.AreEqual(bounce.TargetIntensity(wall), bounce.Intensity, 0.01f);
            Assert.AreEqual(bounce.TargetPosition(wall).z, bounce.Position.z, 0.01f);
        }

        [Test]
        public void ACupboardOffTheAxisPullsThePoolOntoItself()
        {
            BounceLight bounce = Bounce();
            // One ray down the axis onto a dark far wall, one off to the side onto a pale cupboard.
            var fan = new[]
            {
                Wall(6f, new Color(0.1f, 0.1f, 0.1f)),
                new BounceSample(new Vector3(2.5f, 0f, 2f), Vector3.left, new Color(0.8f, 0.8f, 0.8f),
                    new Vector3(0.78f, 0f, 0.63f), Color.white, 9f, 45f)
            };

            Assert.IsTrue(bounce.TryResolve(fan, 2, 2, out Vector3 position, out _, out float intensity));

            // The pale one carries the weight, so the pool sits by it rather than halfway to the far wall.
            Assert.Greater(position.x, 1.5f);
            Assert.Greater(intensity, bounce.TargetIntensity(fan[0]));
        }

        [Test]
        public void AConeHalfIntoOpenAirGivesBackHalfAPool()
        {
            BounceLight bounce = Bounce();
            BounceSample wall = Wall(4f, new Color(0.6f, 0.6f, 0.6f));
            var fan = new[] { wall, wall, wall, wall };

            bounce.TryResolve(fan, 4, 4, out _, out _, out float full);
            // The same four rays, two of which reached nothing at all.
            bounce.TryResolve(fan, 2, 4, out _, out _, out float half);

            Assert.AreEqual(full * 0.5f, half, 1e-4f);
        }

        [Test]
        public void AFanAcrossOneWallMatchesTheSingleRayItReplaces()
        {
            BounceLight bounce = Bounce();
            BounceSample wall = Wall(4f, new Color(0.6f, 0.6f, 0.6f));
            var fan = new[] { wall, wall, wall, wall, wall, wall, wall };

            Assert.IsTrue(bounce.TryResolve(fan, 7, 7, out Vector3 position, out _, out float intensity));

            // Nothing about a flat wall should change when the fan replaced the one ray: same pool, same place.
            Assert.AreEqual(bounce.TargetIntensity(wall), intensity, 1e-4f);
            Assert.AreEqual(bounce.TargetPosition(wall).z, position.z, 1e-4f);
        }

        [Test]
        public void AFanThatReachedNothingLeavesNoPool()
        {
            BounceLight bounce = Bounce();

            Assert.IsFalse(bounce.TryResolve(new BounceSample[4], 0, 4, out _, out _, out float intensity));
            Assert.AreEqual(0f, intensity);
        }

        [Test]
        public void ClearingDropsThePool()
        {
            BounceLight bounce = Bounce();
            Settle(bounce, Wall(4f, Color.white), 120);
            Assert.IsTrue(bounce.IsLit);

            bounce.Clear();

            Assert.IsFalse(bounce.IsLit);
        }
    }
}
