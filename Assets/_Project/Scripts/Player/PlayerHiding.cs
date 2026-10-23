using DarkDepths.Core;
using UnityEngine;

namespace DarkDepths.Player
{
    // Being inside something. The player stops moving and stops being seen; what they can still do is
    // look out and leave. Where they stood is remembered, so leaving puts them back on their own feet.
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    public sealed class PlayerHiding : MonoBehaviour, IHideable
    {
        [SerializeField] private FirstPersonController _controller;
        [SerializeField] private PlayerTarget _target;
        [SerializeField] private string _leavePrompt = "Leave";

        private CharacterController _body;
        private Vector3 _leftFrom;

        public bool IsHidden { get; private set; }

        public string LeavePrompt => _leavePrompt;

        public void Hide(Transform anchor, Vector3 place, float revealDistance)
        {
            if (IsHidden || anchor == null)
            {
                return;
            }

            IsHidden = true;
            _leftFrom = transform.position;

            // The controller owns the transform while it is on; it has to let go before anyone moves it.
            _body.enabled = false;
            transform.SetPositionAndRotation(anchor.position, anchor.rotation);
            _controller.enabled = false;

            _target.Hide(place, revealDistance);
        }

        public void Leave()
        {
            if (!IsHidden)
            {
                return;
            }

            IsHidden = false;
            transform.position = _leftFrom;
            _body.enabled = true;
            _controller.enabled = true;

            _target.Reveal();
        }

        private void Awake()
        {
            _body = GetComponent<CharacterController>();

            if (_controller == null || _target == null)
            {
                Debug.LogError("PlayerHiding is missing the controller or the target; cover will not work.", this);
                enabled = false;
            }
        }
    }
}
