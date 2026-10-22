using UnityEngine;

namespace DarkDepths.Enemies
{
    // The only way anything drives the silhouette's clips; the AI of stage 4 talks to this, not to the Animator.
    [RequireComponent(typeof(Animator))]
    public sealed class SilhouetteAnimator : MonoBehaviour
    {
        private static readonly int WalkingParameter = Animator.StringToHash("Walking");
        private static readonly int HeadTurnParameter = Animator.StringToHash("HeadTurn");

        [SerializeField] private Animator _animator;

        private bool _walking;

        public bool IsWalking => _walking;

        public void SetWalking(bool walking)
        {
            // Re-sending the same value every tick would restart nothing, but it still crosses into native code.
            if (walking == _walking)
            {
                return;
            }

            _walking = walking;
            _animator.SetBool(WalkingParameter, walking);
        }

        public void TriggerHeadTurn()
        {
            _animator.SetTrigger(HeadTurnParameter);
        }

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
        }
    }
}
