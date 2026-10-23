using DarkDepths.Noise;
using NUnit.Framework;
using UnityEngine;

namespace DarkDepths.Tests.EditMode
{
    public sealed class NoiseConfigTests
    {
        private const float Tolerance = 0.0001f;

        private NoiseConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<NoiseConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        // The table a fresh config starts with is the one in docs/04.
        [Test]
        public void StepsCarryTheDocumentedDistances()
        {
            Assert.AreEqual(3f, _config.LoudnessOf(NoiseSound.CrouchStep), Tolerance);
            Assert.AreEqual(8f, _config.LoudnessOf(NoiseSound.WalkStep), Tolerance);
            Assert.AreEqual(15f, _config.LoudnessOf(NoiseSound.RunStep), Tolerance);
        }

        [Test]
        public void CrouchingIsQuieterThanWalkingAndWalkingThanRunning()
        {
            Assert.Less(_config.LoudnessOf(NoiseSound.CrouchStep), _config.LoudnessOf(NoiseSound.WalkStep));
            Assert.Less(_config.LoudnessOf(NoiseSound.WalkStep), _config.LoudnessOf(NoiseSound.RunStep));
        }

        [Test]
        public void ADroppedObjectIsLouderThanADoor()
        {
            Assert.AreEqual(5f, _config.LoudnessOf(NoiseSound.DoorOpen), Tolerance);
            Assert.AreEqual(12f, _config.LoudnessOf(NoiseSound.ObjectFall), Tolerance);
            Assert.Greater(_config.LoudnessOf(NoiseSound.ObjectFall), _config.LoudnessOf(NoiseSound.DoorOpen));
        }

        [Test]
        public void SurfacesMultiplyAsDocumented()
        {
            Assert.AreEqual(0.6f, _config.MultiplierFor(SurfaceType.Carpet), Tolerance);
            Assert.AreEqual(1f, _config.MultiplierFor(SurfaceType.Concrete), Tolerance);
            Assert.AreEqual(1.1f, _config.MultiplierFor(SurfaceType.Wood), Tolerance);
            Assert.AreEqual(1.3f, _config.MultiplierFor(SurfaceType.Metal), Tolerance);
            Assert.AreEqual(1.6f, _config.MultiplierFor(SurfaceType.Glass), Tolerance);
        }

        [Test]
        public void AWallLeavesHalfTheReach()
        {
            Assert.AreEqual(0.5f, _config.WallMultiplier, Tolerance);
        }

        // Every type gets its own colour, or the gizmos cannot be told apart while tuning.
        [Test]
        public void EveryNoiseTypeHasAGizmoColour()
        {
            foreach (NoiseType type in System.Enum.GetValues(typeof(NoiseType)))
            {
                Assert.AreNotEqual(Color.white, _config.ColorFor(type), type + " has no colour of its own");
            }
        }
    }
}
