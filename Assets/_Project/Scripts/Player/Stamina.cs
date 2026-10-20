namespace DarkDepths.Player
{
    // Sprint budget kept free of MonoBehaviour so it can be covered by EditMode tests.
    public sealed class Stamina
    {
        private readonly float _drainPerSecond;
        private readonly float _recoverPerSecond;
        private readonly float _rearmThreshold;

        private float _current = 1f;
        private bool _locked;

        public Stamina(float drainDuration, float recoverDuration, float rearmThreshold)
        {
            _drainPerSecond = drainDuration > 0f ? 1f / drainDuration : 0f;
            _recoverPerSecond = recoverDuration > 0f ? 1f / recoverDuration : 0f;
            _rearmThreshold = rearmThreshold;
        }

        public float Normalized => _current;

        // Stays false after a full drain until the level climbs back to the rearm threshold.
        public bool CanSprint => !_locked;

        public void Tick(float deltaTime, bool sprinting)
        {
            if (sprinting && !_locked)
            {
                _current -= _drainPerSecond * deltaTime;
                if (_current <= 0f)
                {
                    _current = 0f;
                    _locked = true;
                }

                return;
            }

            _current += _recoverPerSecond * deltaTime;
            if (_current > 1f)
            {
                _current = 1f;
            }

            if (_locked && _current >= _rearmThreshold)
            {
                _locked = false;
            }
        }
    }
}
