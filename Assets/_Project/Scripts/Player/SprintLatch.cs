using UnityEngine;

namespace DarkDepths.Player
{
    // Hysteresis for stick-driven sprint: latches near the top of the ring, releases lower down.
    public sealed class SprintLatch
    {
        private readonly float _engageThreshold;
        private readonly float _releaseThreshold;

        private bool _engaged;

        public SprintLatch(float engageThreshold, float releaseThreshold)
        {
            _engageThreshold = engageThreshold;
            _releaseThreshold = Mathf.Min(releaseThreshold, engageThreshold - 0.05f);
        }

        public bool Engaged => _engaged;

        // Returns true when the state flipped, so callers only refresh visuals on a change.
        public bool Evaluate(Vector2 normalizedOffset)
        {
            bool pushedForward = normalizedOffset.y > Mathf.Abs(normalizedOffset.x);
            float magnitude = normalizedOffset.magnitude;

            bool next = _engaged
                ? pushedForward && magnitude >= _releaseThreshold
                : pushedForward && magnitude >= _engageThreshold;

            if (next == _engaged)
            {
                return false;
            }

            _engaged = next;
            return true;
        }

        public bool Release()
        {
            if (!_engaged)
            {
                return false;
            }

            _engaged = false;
            return true;
        }
    }
}
