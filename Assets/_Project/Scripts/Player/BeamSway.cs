using UnityEngine;

namespace DarkDepths.Player
{
    // How the beam trails the view: a spring that overshoots slightly, plus breathing and a stride bob.
    // Kept free of MonoBehaviour so it can be covered by EditMode tests.
    public sealed class BeamSway
    {
        // A frame hitch integrated in one step would throw the spring across the cap and back.
        private const float MaxStep = 0.05f;
        private const float FullCircle = Mathf.PI * 2f;

        private readonly float _frequency;
        private readonly float _damping;
        private readonly float _maxAngle;
        private readonly float _breathAmplitude;
        private readonly float _breathSpeed;
        private readonly float _bobAmplitude;
        private readonly float _bobSpeed;

        private Vector2 _lag;
        private Vector2 _lagVelocity;
        private float _breathPhase;
        private float _bobPhase;

        public BeamSway(float swayTime, float damping, float maxAngle,
            float breathAmplitude, float breathSpeed, float bobAmplitude, float bobSpeed)
        {
            _frequency = swayTime > 0f ? 1f / swayTime : 0f;
            _damping = Mathf.Max(0f, damping);
            _maxAngle = Mathf.Max(0f, maxAngle);
            _breathAmplitude = Mathf.Max(0f, breathAmplitude);
            _breathSpeed = Mathf.Max(0f, breathSpeed);
            _bobAmplitude = Mathf.Max(0f, bobAmplitude);
            _bobSpeed = Mathf.Max(0f, bobSpeed);
        }

        // Degrees the beam is behind the view, pitch in x and yaw in y, before breathing and bob.
        public Vector2 Lag => _lag;

        // viewDelta is how far the view turned this frame in degrees; stride is 0 standing to 1 at full speed.
        public Vector2 Advance(Vector2 viewDelta, float stride, float deltaTime)
        {
            float step = Mathf.Clamp(deltaTime, 0f, MaxStep);
            stride = Mathf.Clamp01(stride);

            if (_frequency <= 0f)
            {
                _lag = Vector2.zero;
                _lagVelocity = Vector2.zero;
            }
            else
            {
                _lag -= viewDelta;
                Vector2 pull = -(_frequency * _frequency) * _lag - (2f * _damping * _frequency) * _lagVelocity;
                _lagVelocity += pull * step;
                _lag += _lagVelocity * step;
                ClampLag();
            }

            _breathPhase = Mathf.Repeat(_breathPhase + step * _breathSpeed, FullCircle);
            _bobPhase = Mathf.Repeat(_bobPhase + step * _bobSpeed * stride, FullCircle);

            return _lag + Breath() + Bob(stride);
        }

        private void ClampLag()
        {
            float magnitude = _lag.magnitude;
            if (magnitude <= _maxAngle || magnitude <= 0f)
            {
                return;
            }

            _lag *= _maxAngle / magnitude;
            // The spring keeps no energy it cannot spend, or the beam would snap back off the cap.
            _lagVelocity = Vector2.zero;
        }

        // Pitch rises and falls, yaw drifts at half the rate, so the rest position is never exactly still.
        private Vector2 Breath()
        {
            return new Vector2(
                Mathf.Sin(_breathPhase) * _breathAmplitude,
                Mathf.Cos(_breathPhase * 0.5f) * _breathAmplitude * 0.5f);
        }

        // The figure of eight a hand draws while walking: yaw once per stride, pitch twice.
        private Vector2 Bob(float stride)
        {
            float amplitude = _bobAmplitude * stride;
            return new Vector2(
                Mathf.Sin(_bobPhase * 2f) * amplitude * 0.5f,
                Mathf.Sin(_bobPhase) * amplitude);
        }
    }
}
