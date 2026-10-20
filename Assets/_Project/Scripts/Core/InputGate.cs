using UnityEngine;

namespace DarkDepths.Core
{
    // Lets UI suspend gameplay input without the player and the UI knowing about each other.
    [CreateAssetMenu(menuName = "DarkDepths/Input Gate", fileName = "InputGate")]
    public sealed class InputGate : ScriptableObject
    {
        private int _blockers;

        public bool IsBlocked => _blockers > 0;

        public void Block()
        {
            _blockers++;
        }

        public void Release()
        {
            _blockers = Mathf.Max(0, _blockers - 1);
        }

        // The asset outlives play mode, so a session that ended while blocked must not latch the next one.
        private void OnEnable()
        {
            _blockers = 0;
        }
    }
}
