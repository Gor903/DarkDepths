using UnityEngine;

namespace DarkDepths.World
{
    // Angle state of a swinging leaf, kept free of MonoBehaviour so it can be covered by EditMode tests.
    public sealed class DoorSwing
    {
        private const float SnapDegrees = 0.25f;

        private readonly float _openAngle;
        private readonly float _smoothTime;
        private readonly float _maxSpeed;

        private float _angle;
        private float _velocity;
        private float _sign = 1f;

        public DoorSwing(float openAngle, float smoothTime, float maxSpeed)
        {
            _openAngle = Mathf.Abs(openAngle);
            _smoothTime = Mathf.Max(0.01f, smoothTime);
            _maxSpeed = maxSpeed > 0f ? maxSpeed : Mathf.Infinity;
        }

        public bool IsOpen { get; private set; }
        public float Angle => _angle;
        public float Target => IsOpen ? _openAngle * _sign : 0f;
        public bool IsMoving => !Mathf.Approximately(_angle, Target);

        // A leaf that swings away from whoever opened it can never sweep through them.
        public void Toggle(float interactorSide)
        {
            if (!IsOpen && Mathf.Approximately(_angle, 0f))
            {
                _sign = interactorSide >= 0f ? 1f : -1f;
            }

            IsOpen = !IsOpen;
        }

        // Returns the angle this step wants; the caller may refuse it and call Hold instead.
        public float Step(float deltaTime)
        {
            float target = Target;
            float next = Mathf.SmoothDamp(_angle, target, ref _velocity, _smoothTime, _maxSpeed, deltaTime);
            if (Mathf.Abs(target - next) < SnapDegrees)
            {
                next = target;
                _velocity = 0f;
            }

            _angle = next;
            return next;
        }

        // Puts the leaf back where it was this frame: something is standing in the way of the next step.
        public void Hold(float angle)
        {
            _angle = angle;
            _velocity = 0f;
        }
    }
}
