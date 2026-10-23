using UnityEngine;

namespace DarkDepths.Player
{
    // Turns distance covered into steps. Counting metres rather than seconds keeps the cadence tied to the
    // speed the player actually has, including while accelerating.
    public sealed class StrideMeter
    {
        private const float MinStride = 0.05f;

        private readonly float _crouchStride;
        private readonly float _walkStride;
        private readonly float _sprintStride;
        private readonly float _restingPhase;

        private float _phase;

        public StrideMeter(float crouchStride, float walkStride, float sprintStride, float firstStepFraction)
        {
            _crouchStride = Mathf.Max(MinStride, crouchStride);
            _walkStride = Mathf.Max(MinStride, walkStride);
            _sprintStride = Mathf.Max(MinStride, sprintStride);
            // Standing still leaves the meter part-way through a stride, so walking on starts with a step
            // instead of a silent stride.
            _restingPhase = 1f - Mathf.Clamp01(firstStepFraction);
            _phase = _restingPhase;
        }

        public float Phase => _phase;

        public bool Advance(float distance, Gait gait)
        {
            if (distance <= 0f)
            {
                return false;
            }

            _phase += distance / StrideFor(gait);
            if (_phase < 1f)
            {
                return false;
            }

            // One step per call: a teleport or a long frame must not fire a burst of them.
            _phase -= Mathf.Floor(_phase);
            return true;
        }

        public void Idle()
        {
            _phase = _restingPhase;
        }

        public float StrideFor(Gait gait)
        {
            switch (gait)
            {
                case Gait.Crouch:
                    return _crouchStride;
                case Gait.Sprint:
                    return _sprintStride;
                default:
                    return _walkStride;
            }
        }
    }
}
