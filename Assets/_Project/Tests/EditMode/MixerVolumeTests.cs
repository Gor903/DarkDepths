using DarkDepths.Audio;
using NUnit.Framework;

namespace DarkDepths.Tests.EditMode
{
    public sealed class MixerVolumeTests
    {
        private const float Tolerance = 0.001f;

        [Test]
        public void FullVolumeLeavesTheFaderWhereItWasAuthored()
        {
            Assert.AreEqual(0f, MixerVolume.ToDecibels(1f), Tolerance);
        }

        [Test]
        public void HalfTheSliderIsSixDecibelsDown()
        {
            Assert.AreEqual(-6.02f, MixerVolume.ToDecibels(0.5f), 0.01f);
        }

        [Test]
        public void ZeroIsSilenceAndNotZeroDecibels()
        {
            Assert.AreEqual(MixerVolume.SilenceDecibels, MixerVolume.ToDecibels(0f), Tolerance);
        }

        [Test]
        public void NothingGoesLouderThanTheAuthoredMix()
        {
            Assert.AreEqual(0f, MixerVolume.ToDecibels(4f), Tolerance);
        }

        [Test]
        public void ANegativeSliderIsTreatedAsSilence()
        {
            Assert.AreEqual(MixerVolume.SilenceDecibels, MixerVolume.ToDecibels(-1f), Tolerance);
        }

        [Test]
        public void TheSliderSurvivesARoundTripThroughTheMixer()
        {
            foreach (float linear in new[] { 0.1f, 0.25f, 0.5f, 0.75f, 1f })
            {
                float roundTrip = MixerVolume.ToLinear(MixerVolume.ToDecibels(linear));
                Assert.AreEqual(linear, roundTrip, Tolerance, "slider at " + linear);
            }
        }

        [Test]
        public void SilenceReadsBackAsAnEmptySlider()
        {
            Assert.AreEqual(0f, MixerVolume.ToLinear(MixerVolume.SilenceDecibels), Tolerance);
            Assert.AreEqual(0f, MixerVolume.ToLinear(-120f), Tolerance);
        }
    }
}
