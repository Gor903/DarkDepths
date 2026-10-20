using DarkDepths.Player;
using NUnit.Framework;

namespace DarkDepths.Tests.EditMode
{
    public sealed class BatteryTests
    {
        private const float Capacity = 120f;
        private const float DrainPerSecond = 1f;
        private const float Tolerance = 0.001f;

        [Test]
        public void StartsFull()
        {
            var battery = new Battery(Capacity, DrainPerSecond);

            Assert.AreEqual(Capacity, battery.Charge, Tolerance);
            Assert.AreEqual(1f, battery.Normalized, Tolerance);
            Assert.IsFalse(battery.IsEmpty);
        }

        [Test]
        public void DrainsOneUnitPerSecond()
        {
            var battery = new Battery(Capacity, DrainPerSecond);

            Drain(battery, 30f);

            Assert.AreEqual(90f, battery.Charge, 0.05f);
        }

        [Test]
        public void EmptiesAfterCapacitySeconds()
        {
            var battery = new Battery(Capacity, DrainPerSecond);

            Drain(battery, Capacity + 1f);

            Assert.IsTrue(battery.IsEmpty);
            Assert.AreEqual(0f, battery.Charge, Tolerance);
        }

        [Test]
        public void NeverGoesNegative()
        {
            var battery = new Battery(Capacity, DrainPerSecond);

            Drain(battery, Capacity * 3f);

            Assert.AreEqual(0f, battery.Charge, Tolerance);
        }

        [Test]
        public void RefillAddsCharge()
        {
            var battery = new Battery(Capacity, DrainPerSecond);
            Drain(battery, 60f);

            battery.Refill(20f);

            Assert.AreEqual(80f, battery.Charge, 0.05f);
        }

        [Test]
        public void RefillClampsToCapacity()
        {
            var battery = new Battery(Capacity, DrainPerSecond);
            Drain(battery, 10f);

            battery.Refill(Capacity);

            Assert.AreEqual(Capacity, battery.Charge, Tolerance);
        }

        [Test]
        public void RefillRevivesAnEmptyBattery()
        {
            var battery = new Battery(Capacity, DrainPerSecond);
            Drain(battery, Capacity + 1f);

            battery.Refill(10f);

            Assert.IsFalse(battery.IsEmpty);
        }

        [Test]
        public void IgnoresNonPositiveRefill()
        {
            var battery = new Battery(Capacity, DrainPerSecond);
            Drain(battery, 20f);
            float before = battery.Charge;

            battery.Refill(0f);
            battery.Refill(-50f);

            Assert.AreEqual(before, battery.Charge, Tolerance);
        }

        [Test]
        public void LowBatteryThresholdIsReachedAtTwentyPercent()
        {
            var battery = new Battery(Capacity, DrainPerSecond);

            // 120 s capacity minus 96 s of use leaves exactly the 20% mark from docs/03_PLAYER_AND_CONTROLS.md.
            Drain(battery, 96f);

            Assert.AreEqual(0.2f, battery.Normalized, 0.005f);
        }

        private static void Drain(Battery battery, float seconds)
        {
            const float deltaTime = 0.02f;
            int steps = (int)(seconds / deltaTime);
            for (int i = 0; i < steps; i++)
            {
                battery.Drain(deltaTime);
            }
        }
    }
}
