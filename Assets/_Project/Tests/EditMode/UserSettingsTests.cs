using DarkDepths.Core;
using NUnit.Framework;
using UnityEngine;

namespace DarkDepths.Tests.EditMode
{
    public sealed class UserSettingsTests
    {
        private const float Tolerance = 0.0001f;

        private UserSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<UserSettings>();
            _settings.ResetToDefaults();
        }

        [TearDown]
        public void TearDown()
        {
            // The values live in PlayerPrefs, so a test run must not leave the editor on odd settings.
            _settings.ResetToDefaults();
            _settings.Save();
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void DefaultsAreNeutral()
        {
            Assert.AreEqual(1f, _settings.LookSensitivity, Tolerance);
            Assert.AreEqual(1f, _settings.Brightness, Tolerance);
        }

        [Test]
        public void EffectsAreFullUntilTheyAreTurnedDown()
        {
            Assert.IsFalse(_settings.ReduceEffects);

            int changes = 0;
            _settings.Changed += () => changes++;

            _settings.ReduceEffects = true;
            _settings.ReduceEffects = true;

            Assert.IsTrue(_settings.ReduceEffects);
            Assert.AreEqual(1, changes);
        }

        [Test]
        public void ClampsToRange()
        {
            _settings.LookSensitivity = 999f;
            _settings.Brightness = -5f;

            Assert.AreEqual(_settings.LookSensitivityMax, _settings.LookSensitivity, Tolerance);
            Assert.AreEqual(_settings.BrightnessMin, _settings.Brightness, Tolerance);
        }

        [Test]
        public void RaisesChangedOnlyOnRealChange()
        {
            int changes = 0;
            _settings.Changed += () => changes++;

            _settings.Brightness = 1.2f;
            _settings.Brightness = 1.2f;

            Assert.AreEqual(1, changes);
        }

        [Test]
        public void ResetReturnsToDefaults()
        {
            _settings.LookSensitivity = 2f;
            _settings.Brightness = 1.4f;
            _settings.ReduceEffects = true;

            _settings.ResetToDefaults();

            Assert.AreEqual(1f, _settings.LookSensitivity, Tolerance);
            Assert.AreEqual(1f, _settings.Brightness, Tolerance);
            Assert.IsFalse(_settings.ReduceEffects);
        }

        [Test]
        public void SavedValuesSurviveANewInstance()
        {
            _settings.LookSensitivity = 1.7f;
            _settings.Brightness = 0.8f;
            _settings.ReduceEffects = true;
            _settings.AmbientVolume = 0.3f;
            _settings.SoundHints = false;
            _settings.Save();

            var reloaded = ScriptableObject.CreateInstance<UserSettings>();
            try
            {
                Assert.AreEqual(1.7f, reloaded.LookSensitivity, Tolerance);
                Assert.AreEqual(0.8f, reloaded.Brightness, Tolerance);
                Assert.IsTrue(reloaded.ReduceEffects);
                Assert.AreEqual(0.3f, reloaded.AmbientVolume, Tolerance);
                Assert.IsFalse(reloaded.SoundHints);
            }
            finally
            {
                Object.DestroyImmediate(reloaded);
            }
        }

        // docs/04: the mix is authored at full; the sliders only take it down.
        [Test]
        public void VolumesStartAtTheAuthoredMix()
        {
            Assert.AreEqual(1f, _settings.SfxVolume, Tolerance);
            Assert.AreEqual(1f, _settings.AmbientVolume, Tolerance);
            Assert.AreEqual(1f, _settings.MusicVolume, Tolerance);
        }

        [Test]
        public void VolumesClampToSilenceAndFull()
        {
            _settings.SfxVolume = -1f;
            _settings.MusicVolume = 4f;

            Assert.AreEqual(0f, _settings.SfxVolume, Tolerance);
            Assert.AreEqual(1f, _settings.MusicVolume, Tolerance);
        }

        // docs/04 asks that nothing be audible only, so the hints start on.
        [Test]
        public void SoundHintsStartOnAndReportOneChange()
        {
            Assert.IsTrue(_settings.SoundHints);

            int changes = 0;
            _settings.Changed += () => changes++;

            _settings.SoundHints = false;
            _settings.SoundHints = false;

            Assert.IsFalse(_settings.SoundHints);
            Assert.AreEqual(1, changes);
        }

        [Test]
        public void ResetReturnsVolumesAndHints()
        {
            _settings.AmbientVolume = 0.2f;
            _settings.SoundHints = false;

            _settings.ResetToDefaults();

            Assert.AreEqual(1f, _settings.AmbientVolume, Tolerance);
            Assert.IsTrue(_settings.SoundHints);
        }
    }
}
