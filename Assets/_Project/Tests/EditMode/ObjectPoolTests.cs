using DarkDepths.Core;
using NUnit.Framework;
using UnityEngine;

namespace DarkDepths.Tests.EditMode
{
    public sealed class ObjectPoolTests
    {
        private GameObject _prefab;
        private Transform _parent;
        private ObjectPool<Rigidbody> _pool;

        [SetUp]
        public void SetUp()
        {
            _prefab = new GameObject("Prefab", typeof(Rigidbody));
            _parent = new GameObject("Pool").transform;
            _pool = new ObjectPool<Rigidbody>(_prefab.GetComponent<Rigidbody>(), 2, _parent);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_parent.gameObject);
            Object.DestroyImmediate(_prefab);
        }

        // Everything is made once, up front: nothing is created while the game is running.
        [Test]
        public void TheInstancesExistBeforeAnyoneAsks()
        {
            Assert.AreEqual(2, _pool.Capacity);
            Assert.AreEqual(2, _parent.childCount);
            Assert.AreEqual(0, _pool.ActiveCount);
        }

        [Test]
        public void TakingPlacesAndShowsOne()
        {
            var where = new Vector3(1f, 2f, 3f);
            Rigidbody taken = _pool.Take(where, Quaternion.identity);

            Assert.IsNotNull(taken);
            Assert.IsTrue(taken.gameObject.activeSelf);
            Assert.AreEqual(where, taken.transform.position);
            Assert.AreEqual(1, _pool.ActiveCount);
        }

        [Test]
        public void TwoTakesAreTwoDifferentObjects()
        {
            Rigidbody first = _pool.Take(Vector3.zero, Quaternion.identity);
            Rigidbody second = _pool.Take(Vector3.one, Quaternion.identity);

            Assert.AreNotSame(first, second);
            Assert.AreEqual(2, _pool.ActiveCount);
        }

        // A full pool does not grow: whatever has been out longest comes back.
        [Test]
        public void AFullPoolRecyclesTheOldest()
        {
            Rigidbody first = _pool.Take(Vector3.zero, Quaternion.identity);
            _pool.Take(Vector3.one, Quaternion.identity);

            Rigidbody third = _pool.Take(new Vector3(5f, 0f, 0f), Quaternion.identity);

            Assert.AreSame(first, third, "the oldest one is the one reused");
            Assert.AreEqual(2, _pool.ActiveCount);
            Assert.AreEqual(new Vector3(5f, 0f, 0f), third.transform.position);
        }

        [Test]
        public void ReturningHidesItAndFreesTheSlot()
        {
            Rigidbody taken = _pool.Take(Vector3.zero, Quaternion.identity);

            Assert.IsTrue(_pool.Return(taken));
            Assert.IsFalse(taken.gameObject.activeSelf);
            Assert.AreEqual(0, _pool.ActiveCount);
        }

        [Test]
        public void ReturningTwiceChangesNothing()
        {
            Rigidbody taken = _pool.Take(Vector3.zero, Quaternion.identity);
            _pool.Return(taken);

            Assert.IsFalse(_pool.Return(taken));
            Assert.AreEqual(0, _pool.ActiveCount);
        }

        [Test]
        public void SomethingElseIsNotFromThisPool()
        {
            var stranger = new GameObject("Stranger", typeof(Rigidbody));
            try
            {
                Assert.IsFalse(_pool.Return(stranger.GetComponent<Rigidbody>()));
            }
            finally
            {
                Object.DestroyImmediate(stranger);
            }
        }

        [Test]
        public void EverythingComesBackAtOnce()
        {
            _pool.Take(Vector3.zero, Quaternion.identity);
            _pool.Take(Vector3.one, Quaternion.identity);

            _pool.ReturnAll();

            Assert.AreEqual(0, _pool.ActiveCount);
        }
    }
}
