using DarkDepths.Enemies;
using NUnit.Framework;

namespace DarkDepths.Tests.EditMode
{
    public sealed class SilhouetteGaitTests
    {
        private const float Tolerance = 0.0001f;

        private const float StrideAngle = 30f;
        private const float KneeAngle = 40f;
        private const float ArmSwingAngle = 20f;
        private const float LimpAngle = 10f;
        private const float SwayAngle = 5f;
        private const float HeadScanAngle = 8f;
        private const float BobHeight = 0.04f;

        private const int Samples = 64;

        [Test]
        public void AStrideLoopsBackToItsStart()
        {
            SilhouetteGait gait = Gait();

            for (int i = 0; i < 2; i++)
            {
                bool right = i == 1;
                Assert.AreEqual(gait.ThighAngle(0f, right), gait.ThighAngle(1f, right), Tolerance);
                Assert.AreEqual(gait.ShinAngle(0f, right), gait.ShinAngle(1f, right), Tolerance);
                Assert.AreEqual(gait.ArmAngle(0f, right), gait.ArmAngle(1f, right), Tolerance);
            }

            Assert.AreEqual(gait.TorsoSway(0f), gait.TorsoSway(1f), Tolerance);
            Assert.AreEqual(gait.HeadScan(0f), gait.HeadScan(1f), Tolerance);
            Assert.AreEqual(gait.Bob(0f), gait.Bob(1f), Tolerance);
        }

        [Test]
        public void LegsSwingInOppositePhase()
        {
            SilhouetteGait gait = Gait();

            Assert.Greater(gait.ThighAngle(0.25f, false), 0f);
            Assert.Less(gait.ThighAngle(0.25f, true), 0f);
        }

        [Test]
        public void TheRightLegDragsBehindTheLeft()
        {
            SilhouetteGait gait = Gait();

            Assert.AreEqual(StrideAngle, gait.ThighAngle(0.25f, false), Tolerance);
            Assert.AreEqual(StrideAngle - LimpAngle, gait.ThighAngle(0.75f, true), Tolerance);
        }

        [Test]
        public void ALimpDeeperThanTheStrideStandsTheLegStill()
        {
            var gait = new SilhouetteGait(StrideAngle, KneeAngle, ArmSwingAngle, StrideAngle * 2f, SwayAngle,
                HeadScanAngle, BobHeight);

            Assert.AreEqual(0f, gait.DragStrideAngle, Tolerance);

            for (int i = 0; i <= Samples; i++)
            {
                Assert.AreEqual(0f, gait.ThighAngle((float)i / Samples, true), Tolerance);
            }
        }

        [Test]
        public void TheKneeOnlyFoldsBackwards()
        {
            SilhouetteGait gait = Gait();
            float deepest = 0f;

            for (int i = 0; i <= Samples; i++)
            {
                float phase = (float)i / Samples;
                for (int side = 0; side < 2; side++)
                {
                    float angle = gait.ShinAngle(phase, side == 1);
                    Assert.LessOrEqual(angle, Tolerance);
                    deepest = angle < deepest ? angle : deepest;
                }
            }

            Assert.AreEqual(-KneeAngle, deepest, 0.1f);
        }

        [Test]
        public void ArmsSwingAgainstTheirOwnLeg()
        {
            SilhouetteGait gait = Gait();

            Assert.Greater(gait.ThighAngle(0.25f, false), 0f);
            Assert.Less(gait.ArmAngle(0.25f, false), 0f);

            Assert.Greater(gait.ThighAngle(0.75f, true), 0f);
            Assert.Less(gait.ArmAngle(0.75f, true), 0f);
        }

        [Test]
        public void TheBodyRisesTwicePerStride()
        {
            SilhouetteGait gait = Gait();

            Assert.AreEqual(0f, gait.Bob(0f), Tolerance);
            Assert.AreEqual(BobHeight, gait.Bob(0.25f), Tolerance);
            Assert.AreEqual(0f, gait.Bob(0.5f), Tolerance);
            Assert.AreEqual(BobHeight, gait.Bob(0.75f), Tolerance);
        }

        [Test]
        public void NegativeSettingsCollapseInsteadOfInverting()
        {
            var gait = new SilhouetteGait(-StrideAngle, -KneeAngle, -ArmSwingAngle, -LimpAngle, -SwayAngle,
                -HeadScanAngle, -BobHeight);

            Assert.AreEqual(0f, gait.ThighAngle(0.25f, false), Tolerance);
            Assert.AreEqual(0f, gait.ShinAngle(0.25f, false), Tolerance);
            Assert.AreEqual(0f, gait.ArmAngle(0.25f, false), Tolerance);
            Assert.AreEqual(0f, gait.TorsoSway(0.25f), Tolerance);
            Assert.AreEqual(0f, gait.HeadScan(0.25f), Tolerance);
            Assert.AreEqual(0f, gait.Bob(0.25f), Tolerance);
        }

        private static SilhouetteGait Gait()
        {
            return new SilhouetteGait(StrideAngle, KneeAngle, ArmSwingAngle, LimpAngle, SwayAngle, HeadScanAngle,
                BobHeight);
        }
    }
}
