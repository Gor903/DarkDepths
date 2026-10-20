namespace DarkDepths.Player
{
    // Flashlight charge, kept free of MonoBehaviour so it can be covered by EditMode tests.
    public sealed class Battery
    {
        private readonly float _capacity;
        private readonly float _drainPerSecond;

        private float _charge;

        public Battery(float capacity, float drainPerSecond)
        {
            _capacity = capacity > 0f ? capacity : 1f;
            _drainPerSecond = drainPerSecond;
            _charge = _capacity;
        }

        public float Charge => _charge;
        public float Normalized => _charge / _capacity;
        public bool IsEmpty => _charge <= 0f;

        public void Drain(float deltaTime)
        {
            if (_charge <= 0f)
            {
                return;
            }

            _charge -= _drainPerSecond * deltaTime;
            if (_charge < 0f)
            {
                _charge = 0f;
            }
        }

        // A battery pickup adds charge; anything above capacity is discarded.
        public void Refill(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            _charge += amount;
            if (_charge > _capacity)
            {
                _charge = _capacity;
            }
        }
    }
}
