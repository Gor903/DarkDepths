using DarkDepths.Core;
using NUnit.Framework;
using UnityEngine;

namespace DarkDepths.Tests.EditMode
{
    public sealed class ConcealmentTests
    {
        private const float RevealDistance = 2f;

        private static readonly Vector3 Place = new Vector3(3f, 0f, -1.6f);

        // docs/05: anything not searching walks past whatever is inside.
        [Test]
        public void SomeoneNotSearchingWalksPast()
        {
            Assert.IsTrue(Concealment.Hides(Place, Place, RevealDistance, false),
                "standing on the spot without looking for anything still finds nothing");
        }

        [Test]
        public void ASearcherRightOnItFindsWhatIsInside()
        {
            Assert.IsFalse(Concealment.Hides(Place, Place + new Vector3(1f, 0f, 0f), RevealDistance, true));
        }

        [Test]
        public void ASearcherAcrossTheRoomDoesNot()
        {
            Assert.IsTrue(Concealment.Hides(Place, Place + new Vector3(5f, 0f, 0f), RevealDistance, true));
        }

        [Test]
        public void TheEdgeOfTheDistanceStillHides()
        {
            Vector3 atEdge = Place + new Vector3(RevealDistance, 0f, 0f);
            Assert.IsTrue(Concealment.Hides(Place, atEdge, RevealDistance, true), "exactly at the edge is not inside it");
        }

        // A hiding place with no reach to be found in is simply a good one.
        [Test]
        public void NoDistanceMeansItAlwaysHides()
        {
            Assert.IsTrue(Concealment.Hides(Place, Place, 0f, true));
        }
    }
}
