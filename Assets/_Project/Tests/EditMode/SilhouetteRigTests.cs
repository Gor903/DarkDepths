using DarkDepths.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace DarkDepths.Tests.EditMode
{
    public sealed class SilhouetteRigTests
    {
        private const float Tolerance = 0.0001f;

        private const float ThighLength = 0.5f;
        private const float ShinLength = 0.5f;
        private const float PelvisHeight = 0.2f;
        private const float SpineLength = 0.3f;
        private const float ChestHeight = 0.4f;
        private const float NeckLength = 0.1f;
        private const float HeadHeight = 0.3f;

        private const float StraightHeight =
            ThighLength + ShinLength + PelvisHeight + SpineLength + ChestHeight + NeckLength + HeadHeight;

        [Test]
        public void EveryJointHangsOffAnEarlierOne()
        {
            Assert.AreEqual(-1, SilhouetteRig.ParentOf(SilhouetteJoint.Hips));

            for (int i = 1; i < SilhouetteRig.JointCount; i++)
            {
                int parent = SilhouetteRig.ParentOf((SilhouetteJoint)i);

                Assert.GreaterOrEqual(parent, 0);
                Assert.Less(parent, i);
            }
        }

        [Test]
        public void TheFeetStandOnTheFloor()
        {
            Assert.AreEqual(0f, new SilhouetteRig(Body()).Bounds.min.y, Tolerance);
            Assert.AreEqual(0f, new SilhouetteRig(Body(20f)).Bounds.min.y, Tolerance);
        }

        [Test]
        public void TheSilhouetteIsAsTallAsItsSegments()
        {
            Assert.AreEqual(StraightHeight, new SilhouetteRig(Body()).Height, Tolerance);
        }

        [Test]
        public void HunchingCostsHeight()
        {
            float hunched = new SilhouetteRig(Body(20f)).Height;

            Assert.Less(hunched, StraightHeight);
            Assert.Greater(hunched, StraightHeight * 0.8f);
        }

        [Test]
        public void TheHeadStaysLevelWhileTheBodyHunches()
        {
            var rig = new SilhouetteRig(Body(20f));

            Vector3 chestUp = rig.BoneToRoot(SilhouetteJoint.Chest).MultiplyVector(Vector3.up);
            Vector3 headUp = rig.BoneToRoot(SilhouetteJoint.Head).MultiplyVector(Vector3.up);

            Assert.Greater(chestUp.z, 0f);
            Assert.AreEqual(0f, headUp.z, Tolerance);
        }

        [Test]
        public void LimbsAreMirrored()
        {
            var rig = new SilhouetteRig(Body());

            Vector3 leftThigh = rig.LocalPosition(SilhouetteJoint.ThighLeft);
            Vector3 rightThigh = rig.LocalPosition(SilhouetteJoint.ThighRight);
            Vector3 leftArm = rig.LocalPosition(SilhouetteJoint.ArmLeft);
            Vector3 rightArm = rig.LocalPosition(SilhouetteJoint.ArmRight);

            Assert.Less(leftThigh.x, 0f);
            Assert.AreEqual(-leftThigh.x, rightThigh.x, Tolerance);
            Assert.Less(leftArm.x, 0f);
            Assert.AreEqual(-leftArm.x, rightArm.x, Tolerance);
            Assert.AreEqual(leftArm.y, rightArm.y, Tolerance);
        }

        [Test]
        public void ThePathFollowsTheHierarchy()
        {
            var rig = new SilhouetteRig(Body());

            Assert.AreEqual("Hips", rig.PathOf(SilhouetteJoint.Hips));
            Assert.AreEqual("Hips/Spine/Chest/Neck/Head", rig.PathOf(SilhouetteJoint.Head));
            Assert.AreEqual("Hips/ThighRight/ShinRight", rig.PathOf(SilhouetteJoint.ShinRight));
            Assert.AreEqual("Hips/Spine/Chest/ArmLeft/ForearmLeft", rig.PathOf(SilhouetteJoint.ForearmLeft));
        }

        [Test]
        public void EveryJointCarriesABox()
        {
            var rig = new SilhouetteRig(Body());

            Assert.AreEqual(SilhouetteRig.JointCount, rig.Boxes.Count);

            for (int i = 0; i < rig.Boxes.Count; i++)
            {
                Assert.AreEqual((SilhouetteJoint)i, rig.Boxes[i].Joint);
                Assert.Greater(rig.Boxes[i].Size.x, 0f);
                Assert.Greater(rig.Boxes[i].Size.y, 0f);
                Assert.Greater(rig.Boxes[i].Size.z, 0f);
            }
        }

        private static SilhouetteProportions Body(float hunchAngle = 0f)
        {
            return new SilhouetteProportions
            {
                ThighLength = ThighLength,
                ShinLength = ShinLength,
                LegThickness = 0.1f,
                PelvisHeight = PelvisHeight,
                WaistWidth = 0.3f,
                WaistDepth = 0.2f,
                SpineLength = SpineLength,
                ChestHeight = ChestHeight,
                ChestWidth = 0.4f,
                ChestDepth = 0.2f,
                NeckLength = NeckLength,
                HeadWidth = 0.2f,
                HeadHeight = HeadHeight,
                HeadDepth = 0.2f,
                UpperArmLength = 0.3f,
                ForearmLength = 0.4f,
                ArmThickness = 0.1f,
                ShoulderDrop = 0.05f,
                HunchAngle = hunchAngle,
                HeadCockAngle = 0f,
                ArmRestAngle = 0f,
                ElbowRestAngle = 0f,
                ArmSpreadAngle = 0f
            };
        }
    }
}
