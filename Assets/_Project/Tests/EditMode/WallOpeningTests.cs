using DarkDepths.World;
using NUnit.Framework;

namespace DarkDepths.Tests.EditMode
{
    public sealed class WallOpeningTests
    {
        private const float Tolerance = 0.0001f;

        private const float WallWidth = 2f;
        private const float WallHeight = 3f;
        private const float DoorWidth = 1.2f;
        private const float DoorHeight = 2.2f;

        [Test]
        public void ADoorwayLeavesTwoSidesAndAHeader()
        {
            var opening = new WallOpening(WallWidth, WallHeight, DoorWidth, DoorHeight);

            Assert.AreEqual(3, opening.Count);
        }

        [Test]
        public void PanelsCoverTheWallMinusTheOpening()
        {
            var opening = new WallOpening(WallWidth, WallHeight, DoorWidth, DoorHeight);

            float expected = WallWidth * WallHeight - DoorWidth * DoorHeight;
            Assert.AreEqual(expected, opening.SolidArea, Tolerance);
        }

        [Test]
        public void SidesAreSymmetric()
        {
            var opening = new WallOpening(WallWidth, WallHeight, DoorWidth, DoorHeight);

            WallPanel left = opening[0];
            WallPanel right = opening[1];

            Assert.AreEqual(-left.CenterX, right.CenterX, Tolerance);
            Assert.AreEqual(left.Width, right.Width, Tolerance);
            Assert.AreEqual((WallWidth - DoorWidth) * 0.5f, left.Width, Tolerance);
        }

        [Test]
        public void SidesReachFromFloorToCeiling()
        {
            var opening = new WallOpening(WallWidth, WallHeight, DoorWidth, DoorHeight);

            Assert.AreEqual(WallHeight, opening[0].Height, Tolerance);
            Assert.AreEqual(WallHeight * 0.5f, opening[0].CenterY, Tolerance);
        }

        [Test]
        public void TheHeaderSitsOnTopOfTheOpening()
        {
            var opening = new WallOpening(WallWidth, WallHeight, DoorWidth, DoorHeight);

            WallPanel header = opening[opening.Count - 1];
            float headerHeight = WallHeight - DoorHeight;

            Assert.AreEqual(DoorWidth, header.Width, Tolerance);
            Assert.AreEqual(headerHeight, header.Height, Tolerance);
            Assert.AreEqual(DoorHeight + headerHeight * 0.5f, header.CenterY, Tolerance);
        }

        [Test]
        public void AnOpeningAsWideAsTheWallLeavesOnlyTheHeader()
        {
            var opening = new WallOpening(WallWidth, WallHeight, WallWidth, DoorHeight);

            Assert.AreEqual(1, opening.Count);
            Assert.AreEqual(WallWidth, opening[0].Width, Tolerance);
        }

        [Test]
        public void AnOpeningAsTallAsTheWallLeavesNoHeader()
        {
            var opening = new WallOpening(WallWidth, WallHeight, DoorWidth, WallHeight);

            Assert.AreEqual(2, opening.Count);
            Assert.AreEqual(WallWidth * WallHeight - DoorWidth * WallHeight, opening.SolidArea, Tolerance);
        }

        [Test]
        public void AFullOpeningLeavesNothingSolid()
        {
            var opening = new WallOpening(WallWidth, WallHeight, WallWidth, WallHeight);

            Assert.AreEqual(0, opening.Count);
            Assert.AreEqual(0f, opening.SolidArea, Tolerance);
        }

        [Test]
        public void NoOpeningLeavesTheWallSolid()
        {
            var opening = new WallOpening(WallWidth, WallHeight, 0f, 0f);

            Assert.AreEqual(WallWidth * WallHeight, opening.SolidArea, Tolerance);
        }

        [Test]
        public void AnOversizedOpeningIsClampedToTheWall()
        {
            var opening = new WallOpening(WallWidth, WallHeight, WallWidth * 3f, WallHeight * 3f);

            Assert.AreEqual(WallWidth, opening.OpeningWidth, Tolerance);
            Assert.AreEqual(WallHeight, opening.OpeningHeight, Tolerance);
            Assert.AreEqual(0f, opening.SolidArea, Tolerance);
        }

        [Test]
        public void NegativeSizesCollapseInsteadOfInverting()
        {
            var opening = new WallOpening(-WallWidth, -WallHeight, -DoorWidth, -DoorHeight);

            Assert.AreEqual(0, opening.Count);
            Assert.AreEqual(0f, opening.SolidArea, Tolerance);
        }
    }
}
