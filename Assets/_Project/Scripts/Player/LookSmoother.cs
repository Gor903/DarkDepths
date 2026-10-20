using UnityEngine;

namespace DarkDepths.Player
{
    // Spreads each look delta over a few frames without dropping any of it, so a touch swipe stops stepping.
    public sealed class LookSmoother
    {
        private const float FlushThreshold = 1e-5f;

        private readonly float _smoothing;

        private Vector2 _pending;

        public LookSmoother(float smoothing)
        {
            _smoothing = Mathf.Max(0f, smoothing);
        }

        public Vector2 Pending => _pending;

        public Vector2 Step(Vector2 input, float deltaTime)
        {
            _pending += input;

            if (_smoothing <= 0f || deltaTime <= 0f || _pending.sqrMagnitude <= FlushThreshold * FlushThreshold)
            {
                return Flush();
            }

            Vector2 step = _pending * (1f - Mathf.Exp(-deltaTime / _smoothing));
            _pending -= step;
            return step;
        }

        public Vector2 Flush()
        {
            Vector2 all = _pending;
            _pending = Vector2.zero;
            return all;
        }
    }
}
