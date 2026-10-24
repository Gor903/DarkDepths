using System;
using System.Collections.Generic;
using DarkDepths.Core;
using NUnit.Framework;
using UnityEngine;

namespace DarkDepths.Tests.EditMode
{
    public sealed class PaletteConfigTests
    {
        private const float Tolerance = 0.0001f;

        private PaletteConfig _palette;

        [SetUp]
        public void SetUp()
        {
            _palette = ScriptableObject.CreateInstance<PaletteConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_palette);
        }

        // The hex values of docs/02_VISUAL_STYLE.md; the asset is allowed to drift, the defaults are not.
        [TestCase(PaletteRole.Background, "0F1724")]
        [TestCase(PaletteRole.Structure, "1B2028")]
        [TestCase(PaletteRole.Prop, "3A4656")]
        [TestCase(PaletteRole.Lamp, "E8D9A0")]
        [TestCase(PaletteRole.Danger, "8C2A2A")]
        [TestCase(PaletteRole.Enemy, "050608")]
        public void DefaultsMatchTheStyleGuide(PaletteRole role, string hex)
        {
            Assert.AreEqual(hex, ColorUtility.ToHtmlStringRGB(_palette.Resolve(role)));
        }

        [Test]
        public void EveryRoleResolvesToItsOwnColor()
        {
            var seen = new HashSet<Color>();

            foreach (PaletteRole role in Enum.GetValues(typeof(PaletteRole)))
            {
                Assert.IsTrue(seen.Add(_palette.Resolve(role)), "Two roles share a color: " + role);
            }
        }

        [Test]
        public void NamedColorsAgreeWithResolve()
        {
            Assert.AreEqual(_palette.Background, _palette.Resolve(PaletteRole.Background));
            Assert.AreEqual(_palette.Structure, _palette.Resolve(PaletteRole.Structure));
            Assert.AreEqual(_palette.Prop, _palette.Resolve(PaletteRole.Prop));
            Assert.AreEqual(_palette.Lamp, _palette.Resolve(PaletteRole.Lamp));
            Assert.AreEqual(_palette.Danger, _palette.Resolve(PaletteRole.Danger));
            Assert.AreEqual(_palette.Enemy, _palette.Resolve(PaletteRole.Enemy));
        }

        [Test]
        public void AmbientIsTheBackgroundDimmedButOpaque()
        {
            Color ambient = _palette.AmbientLight;

            Assert.AreEqual(1f, ambient.a, Tolerance);
            Assert.Less(ambient.r, _palette.Background.r);
            Assert.Less(ambient.g, _palette.Background.g);
            Assert.Less(ambient.b, _palette.Background.b);
        }

        [Test]
        public void AmbientKeepsTheBackgroundHue()
        {
            Color ambient = _palette.AmbientLight;

            // Blue stays the dominant channel, or the dark would read as grey instead of cold.
            Assert.Greater(ambient.b, ambient.r);
            Assert.Greater(ambient.b, ambient.g);
        }

        [Test]
        public void WallsStayMatte()
        {
            // docs/10_LIGHTING.md: walls 0.1-0.2, or a highlight travels the wall as a second light source.
            Assert.LessOrEqual(_palette.SmoothnessFor(SurfaceFinish.Wall), 0.2f);
            Assert.AreEqual(0f, _palette.Metallic, Tolerance);
        }

        [Test]
        public void TheFloorCatchesMoreOfTheBeamThanTheWalls()
        {
            float floor = _palette.SmoothnessFor(SurfaceFinish.Floor);

            Assert.Greater(floor, _palette.SmoothnessFor(SurfaceFinish.Wall));
            Assert.LessOrEqual(floor, 0.5f);
        }

        [Test]
        public void FogSitsBelowTheAmbientLift()
        {
            // A fog brighter than the ambient would read as a lit wall at the edge of sight.
            Assert.LessOrEqual(_palette.FogColor.b, _palette.AmbientLight.b);
            Assert.AreEqual(1f, _palette.FogColor.a, Tolerance);
        }

        [Test]
        public void AmbientLeavesTheRoomDark()
        {
            // docs/10_LIGHTING.md caps the lift at a tenth of the background; above that the dark is gone.
            Assert.Less(_palette.AmbientLight.b, _palette.Background.b * 0.1f);
        }

        [Test]
        public void EmissionIsBrighterThanTheLampColor()
        {
            Assert.Greater(_palette.Emission.r, _palette.Lamp.r);
            Assert.Greater(_palette.Emission.g, _palette.Lamp.g);
            Assert.Greater(_palette.Emission.b, _palette.Lamp.b);
        }

        [Test]
        public void FogSpanIsOpen()
        {
            Assert.Greater(_palette.FogEndDistance, _palette.FogStartDistance);
        }

        [Test]
        public void FogEndStaysWithinTheDrawDistanceBudget()
        {
            // docs/07_ARCHITECTURE.md budgets 25-30 m of draw distance.
            Assert.LessOrEqual(_palette.FogEndDistance, 30f);
        }

        [Test]
        public void ShadowsReachAsFarAsSight()
        {
            Assert.GreaterOrEqual(_palette.ShadowDistance, _palette.FogEndDistance);
        }

        [Test]
        public void FillLightStaysBelowTheFlashlight()
        {
            // The flashlight has to be the reason anything is visible; the fill only saves the silhouettes.
            Assert.Less(_palette.FillLightIntensity, 0.2f);
        }
    }
}
