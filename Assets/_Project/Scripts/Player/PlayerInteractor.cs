using DarkDepths.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DarkDepths.Player
{
    // Casts from screen centre, per docs/03_PLAYER_AND_CONTROLS.md, and shows the prompt next to the crosshair.
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private Camera _camera;
        [SerializeField] private Text _promptLabel;
        [SerializeField] private float _range = 2f;
        [Tooltip("Fallback sphere around the ray; a thumb-steered crosshair rarely lands dead centre.")]
        [SerializeField] private float _aimRadius = 0.12f;
        [SerializeField] private LayerMask _interactableMask;
        [Tooltip("Optional: inside a hiding place the press belongs to leaving it, not to what is in view.")]
        [SerializeField] private PlayerHiding _hiding;

        private readonly RaycastHit[] _hits = new RaycastHit[4];
        private Collider _resolvedCollider;
        private IInteractable _resolvedInteractable;
        private string _shownPrompt = string.Empty;

        private void Update()
        {
            if (_hiding != null && _hiding.IsHidden)
            {
                ShowPrompt(_hiding.LeavePrompt);
                if (_input.ConsumeInteract())
                {
                    _hiding.Leave();
                }

                return;
            }

            IInteractable target = FindTarget();
            bool available = target != null && target.CanInteract(gameObject);

            ShowPrompt(available ? target.Prompt : string.Empty);

            // Always consumed: a press made while aiming at nothing must not fire at the next thing in view.
            bool pressed = _input.ConsumeInteract();
            if (available && pressed)
            {
                target.Interact(gameObject);
                // The object may have just disabled itself, so drop the cached lookup.
                _resolvedCollider = null;
                _resolvedInteractable = null;
            }
        }

        private IInteractable FindTarget()
        {
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            int count = Physics.RaycastNonAlloc(ray, _hits, _range, _interactableMask, QueryTriggerInteraction.Ignore);
            IInteractable precise = Resolve(Nearest(count));
            if (precise != null)
            {
                return precise;
            }

            // The sweep also reports a collider the camera is standing inside, which a thin ray never does.
            count = Physics.SphereCastNonAlloc(ray, _aimRadius, _hits, _range, _interactableMask,
                QueryTriggerInteraction.Ignore);
            return Resolve(Nearest(count));
        }

        private Collider Nearest(int count)
        {
            if (count == 0)
            {
                return null;
            }

            int nearest = 0;
            for (int i = 1; i < count; i++)
            {
                if (_hits[i].distance < _hits[nearest].distance)
                {
                    nearest = i;
                }
            }

            return _hits[nearest].collider;
        }

        // GetComponentInParent runs only when the aimed-at collider changes, not every frame.
        private IInteractable Resolve(Collider hitCollider)
        {
            if (hitCollider == null)
            {
                _resolvedCollider = null;
                _resolvedInteractable = null;
                return null;
            }

            if (ReferenceEquals(hitCollider, _resolvedCollider))
            {
                return _resolvedInteractable;
            }

            _resolvedCollider = hitCollider;
            _resolvedInteractable = hitCollider.GetComponentInParent<IInteractable>();
            return _resolvedInteractable;
        }

        private void ShowPrompt(string prompt)
        {
            if (prompt == _shownPrompt)
            {
                return;
            }

            _shownPrompt = prompt;
            _promptLabel.text = prompt;
        }
    }
}
