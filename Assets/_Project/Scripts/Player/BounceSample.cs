using UnityEngine;

namespace DarkDepths.Player
{
    // One reading of where the beam landed and what it landed with. Colours are linear, angles degrees.
    public readonly struct BounceSample
    {
        public readonly Vector3 Point;
        public readonly Vector3 Normal;
        public readonly Color Albedo;
        public readonly Vector3 BeamForward;
        public readonly Color BeamColor;
        public readonly float BeamIntensity;
        public readonly float SpotAngle;

        public BounceSample(Vector3 point, Vector3 normal, Color albedo,
            Vector3 beamForward, Color beamColor, float beamIntensity, float spotAngle)
        {
            Point = point;
            Normal = normal;
            Albedo = albedo;
            BeamForward = beamForward;
            BeamColor = beamColor;
            BeamIntensity = beamIntensity;
            SpotAngle = spotAngle;
        }
    }
}
