using DarkDepths.Audio;
using NUnit.Framework;

namespace DarkDepths.Tests.EditMode
{
    public sealed class VoiceAllocatorTests
    {
        private const int Important = 10;
        private const int Ordinary = 128;
        private const int Background = 200;

        [Test]
        public void EveryVoiceIsHandedOutBeforeAnyIsReused()
        {
            var allocator = new VoiceAllocator(3);

            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(allocator.TryAcquire(Ordinary, i, out AudioHandle handle));
                Assert.IsTrue(allocator.IsActive(handle));
            }

            Assert.AreEqual(3, allocator.ActiveCount);
        }

        [Test]
        public void AFullPoolRefusesSomethingNoMoreImportant()
        {
            var allocator = new VoiceAllocator(2);
            allocator.TryAcquire(Ordinary, 0f, out _);
            allocator.TryAcquire(Ordinary, 1f, out _);

            Assert.IsFalse(allocator.TryAcquire(Ordinary, 2f, out AudioHandle handle));
            Assert.IsFalse(handle.IsValid);
            Assert.AreEqual(2, allocator.ActiveCount);
        }

        [Test]
        public void AFullPoolGivesUpItsLeastImportantVoice()
        {
            var allocator = new VoiceAllocator(2);
            allocator.TryAcquire(Important, 0f, out AudioHandle important);
            allocator.TryAcquire(Background, 1f, out AudioHandle background);

            Assert.IsTrue(allocator.TryAcquire(Ordinary, 2f, out AudioHandle taken));
            Assert.AreEqual(background.Slot, taken.Slot);
            Assert.IsTrue(allocator.IsActive(important));
            Assert.AreEqual(2, allocator.ActiveCount);
        }

        [Test]
        public void AmongEquallyUnimportantVoicesTheOldestGoesFirst()
        {
            var allocator = new VoiceAllocator(2);
            allocator.TryAcquire(Background, 0f, out AudioHandle oldest);
            allocator.TryAcquire(Background, 5f, out AudioHandle newest);

            Assert.IsTrue(allocator.TryAcquire(Ordinary, 6f, out AudioHandle taken));
            Assert.AreEqual(oldest.Slot, taken.Slot);
            Assert.IsTrue(allocator.IsActive(newest));
        }

        [Test]
        public void AHandleToAStolenVoiceStopsBeingActive()
        {
            var allocator = new VoiceAllocator(1);
            allocator.TryAcquire(Background, 0f, out AudioHandle stolen);

            Assert.IsTrue(allocator.TryAcquire(Ordinary, 1f, out AudioHandle taken));
            Assert.AreEqual(stolen.Slot, taken.Slot);
            Assert.IsFalse(allocator.IsActive(stolen));
            Assert.IsTrue(allocator.IsActive(taken));
        }

        [Test]
        public void ReleasingTwiceChangesNothingTheSecondTime()
        {
            var allocator = new VoiceAllocator(1);
            allocator.TryAcquire(Ordinary, 0f, out AudioHandle handle);

            Assert.IsTrue(allocator.Release(handle));
            Assert.IsFalse(allocator.Release(handle));
            Assert.AreEqual(0, allocator.ActiveCount);
        }

        [Test]
        public void AReleasedVoiceComesBackAroundForTheNextSound()
        {
            var allocator = new VoiceAllocator(1);
            allocator.TryAcquire(Ordinary, 0f, out AudioHandle first);
            allocator.Release(first);

            Assert.IsTrue(allocator.TryAcquire(Ordinary, 1f, out AudioHandle second));
            Assert.AreEqual(first.Slot, second.Slot);
            Assert.IsFalse(allocator.IsActive(first));
            Assert.IsTrue(allocator.IsActive(second));
        }

        [Test]
        public void ADefaultHandleIsNeverActive()
        {
            var allocator = new VoiceAllocator(2);

            Assert.IsFalse(AudioHandle.None.IsValid);
            Assert.IsFalse(allocator.IsActive(AudioHandle.None));
        }

        [Test]
        public void StoppingEverythingEmptiesThePool()
        {
            var allocator = new VoiceAllocator(3);
            allocator.TryAcquire(Ordinary, 0f, out _);
            allocator.TryAcquire(Ordinary, 1f, out _);

            allocator.ReleaseAll();

            Assert.AreEqual(0, allocator.ActiveCount);
            Assert.AreEqual(3, allocator.Capacity);
        }

        [Test]
        public void APoolCannotBeAskedForLessThanOneVoice()
        {
            var allocator = new VoiceAllocator(0);

            Assert.AreEqual(1, allocator.Capacity);
            Assert.IsTrue(allocator.TryAcquire(Ordinary, 0f, out _));
        }
    }
}
