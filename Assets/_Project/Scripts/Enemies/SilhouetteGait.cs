using UnityEngine;

namespace DarkDepths.Enemies
{
    // The walk cycle of docs/02: one stride of both legs, deliberately lopsided. Angles are forward-positive.
    public sealed class SilhouetteGait
    {
        private const float Tau = Mathf.PI * 2f;
        // Half a cycle apart: while one leg swings the other carries the body.
        private const float OppositeLeg = 0.5f;
        // The knee folds just after the foot leaves the floor, not at the far end of the swing.
        private const float KneeLead = 0.125f;

        public SilhouetteGait(float strideAngle, float kneeAngle, float armSwingAngle, float limpAngle,
            float swayAngle, float headScanAngle, float bobHeight)
        {
            StrideAngle = Mathf.Max(0f, strideAngle);
            KneeAngle = Mathf.Max(0f, kneeAngle);
            ArmSwingAngle = Mathf.Max(0f, armSwingAngle);
            // A limp deeper than the stride would swing the dragging leg backwards instead of shortening it.
            LimpAngle = Mathf.Clamp(limpAngle, 0f, StrideAngle);
            SwayAngle = Mathf.Max(0f, swayAngle);
            HeadScanAngle = Mathf.Max(0f, headScanAngle);
            BobHeight = Mathf.Max(0f, bobHeight);
        }

        public float StrideAngle { get; }
        public float KneeAngle { get; }
        public float ArmSwingAngle { get; }
        public float LimpAngle { get; }
        public float SwayAngle { get; }
        public float HeadScanAngle { get; }
        public float BobHeight { get; }

        // The right leg is the one that drags; what is left of the stride after the limp.
        public float DragStrideAngle => StrideAngle - LimpAngle;

        public float ThighAngle(float phase, bool right)
        {
            return Amplitude(right) * Mathf.Sin(Tau * Step(phase, right));
        }

        // The knee only folds backwards, and only while that leg is swinging through.
        public float ShinAngle(float phase, bool right)
        {
            return -KneeAngle * Mathf.Max(0f, Mathf.Cos(Tau * (Step(phase, right) - KneeLead)));
        }

        // An arm swings against the leg on its own side.
        public float ArmAngle(float phase, bool right)
        {
            return -ArmSwingAngle * Mathf.Sin(Tau * Step(phase, right));
        }

        // Roll towards whichever leg is carrying the weight.
        public float TorsoSway(float phase)
        {
            return SwayAngle * Mathf.Sin(Tau * phase);
        }

        // The head keeps sweeping while the body walks: this one hunts by ear (docs/05).
        public float HeadScan(float phase)
        {
            return HeadScanAngle * Mathf.Sin(Tau * phase);
        }

        // Highest in mid-stance, back down on every footfall, so twice per stride.
        public float Bob(float phase)
        {
            return BobHeight * Mathf.Abs(Mathf.Sin(Tau * phase));
        }

        private float Amplitude(bool right)
        {
            return right ? DragStrideAngle : StrideAngle;
        }

        private static float Step(float phase, bool right)
        {
            return right ? phase + OppositeLeg : phase;
        }
    }
}
