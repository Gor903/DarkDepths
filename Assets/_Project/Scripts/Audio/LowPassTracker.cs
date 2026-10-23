using UnityEngine;

namespace DarkDepths.Audio
{
    // How fast a voice goes from clear to muffled and back. Kept free of MonoBehaviour so it can be tested.
    // The cutoff moves geometrically: halving it twice sounds like one step twice, not like one step.
    public sealed class LowPassTracker
    {
        private readonly float _openCutoff;
        private readonly float _blockedCutoff;
        private readonly float _smoothTime;

        // 0 is a clear line to the listener, 1 is a wall in the way.
        private float _blend;

        public LowPassTracker(float openCutoff, float blockedCutoff, float smoothTime)
        {
            _openCutoff = Mathf.Max(1f, openCutoff);
            _blockedCutoff = Mathf.Clamp(blockedCutoff, 1f, _openCutoff);
            _smoothTime = Mathf.Max(0f, smoothTime);
        }

        public float Cutoff => _openCutoff * Mathf.Pow(_blockedCutoff / _openCutoff, _blend);

        public float Blend => _blend;

        // A voice that has just started is already where it belongs; it must not sweep into place.
        public float Reset(bool blocked)
        {
            _blend = blocked ? 1f : 0f;
            return Cutoff;
        }

        public float Step(bool blocked, float deltaTime)
        {
            float target = blocked ? 1f : 0f;
            float rate = _smoothTime > 0f ? deltaTime / _smoothTime : 1f;
            _blend = Mathf.MoveTowards(_blend, target, Mathf.Max(0f, rate));
            return Cutoff;
        }
    }
}
