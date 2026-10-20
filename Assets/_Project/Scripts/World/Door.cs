using DarkDepths.Core;
using UnityEngine;

namespace DarkDepths.World
{
    public sealed class Door : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform _pivot;
        [Tooltip("Collider of the leaf; its box is what the swing path is tested against.")]
        [SerializeField] private BoxCollider _leaf;
        [SerializeField] private float _openAngle = 95f;
        [Tooltip("Seconds to settle at the open or closed angle.")]
        [SerializeField] private float _openTime = 0.28f;
        [SerializeField] private float _maxOpenSpeed = 260f;
        [Tooltip("Layers the leaf refuses to swing through; leave empty to disable the check.")]
        [SerializeField] private LayerMask _blockerMask;
        [Tooltip("Extra thickness on the swept box, so a fast step cannot pass a blocker.")]
        [SerializeField] private float _sweepMargin = 0.08f;
        [SerializeField] private string _openPrompt = "Open";
        [SerializeField] private string _closePrompt = "Close";

        private DoorSwing _swing;
        private Vector3 _leafOffset;
        private Vector3 _leafExtents;

        public string Prompt => _swing.IsOpen ? _closePrompt : _openPrompt;

        public bool CanInteract(GameObject interactor)
        {
            return true;
        }

        public void Interact(GameObject interactor)
        {
            // Which side of the closed leaf the interactor stands on decides which way it swings.
            float side = Vector3.Dot(interactor.transform.position - _pivot.position, transform.forward);
            _swing.Toggle(side);
        }

        private void Awake()
        {
            _swing = new DoorSwing(_openAngle, _openTime, _maxOpenSpeed);

            if (_leaf == null)
            {
                return;
            }

            _leafOffset = _pivot.InverseTransformPoint(_leaf.transform.TransformPoint(_leaf.center));

            Vector3 scale = _leaf.transform.lossyScale;
            _leafExtents = new Vector3(
                Mathf.Abs(_leaf.size.x * scale.x) * 0.5f,
                Mathf.Abs(_leaf.size.y * scale.y) * 0.5f,
                Mathf.Abs(_leaf.size.z * scale.z) * 0.5f + _sweepMargin);
        }

        private void Update()
        {
            if (!_swing.IsMoving)
            {
                return;
            }

            float held = _swing.Angle;
            float next = _swing.Step(Time.deltaTime);

            // A leaf already touching something has to keep going, or a player leaning on it would jam it shut.
            if (IsBlocked(next) && !IsBlocked(held))
            {
                // Waits instead of pushing through; the swing resumes on its own once the way is clear.
                _swing.Hold(held);
                return;
            }

            _pivot.localRotation = Quaternion.Euler(0f, next, 0f);
        }

        private bool IsBlocked(float angle)
        {
            if (_leaf == null || _blockerMask.value == 0)
            {
                return false;
            }

            Quaternion rotation = transform.rotation * Quaternion.Euler(0f, angle, 0f);
            Vector3 center = _pivot.position + rotation * _leafOffset;
            return Physics.CheckBox(center, _leafExtents, rotation, _blockerMask, QueryTriggerInteraction.Ignore);
        }
    }
}
