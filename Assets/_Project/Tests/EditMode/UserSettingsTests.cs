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

            _settings.ResetToDefaults();

            Assert.AreEqual(1f, _settings.LookSensitivity, Tolerance);
            Assert.AreEqual(1f, _settings.Brightness, Tolerance);
        }

        [Test]
        public void SavedValuesSurviveANewInstance()
        {
            _settings.LookSensitivity = 1.7f;
            _settings.Brightness = 0.8f;
            _settings.Save();

            var reloaded = ScriptableObject.CreateInstance<UserSettings>();
            try
            {
                Assert.AreEqual(1.7f, reloaded.LookSensitivity, Tolerance);
                Assert.AreEqual(0.8f, reloaded.Brightness, Tolerance);
            }
            finally
            {
                Object.DestroyImmediate(reloaded);
            }
        }
    }
}
