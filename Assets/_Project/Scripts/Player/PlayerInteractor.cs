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
        [SerializeField] private LayerMask _interactableMask;

        private readonly RaycastHit[] _hits = new RaycastHit[4];
        private Collider _resolvedCollider;
        private IInteractable _resolvedInteractable;
        private string _shownPrompt = string.Empty;

        private void Update()
        {
            IInteractable target = FindTarget();
            bool available = target != null && target.CanInteract(gameObject);

            ShowPrompt(available ? target.Prompt : string.Empty);

            if (available && _input.ConsumeInteract())
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

            return Resolve(_hits[nearest].collider);
        }

        // GetComponentInParent runs only when the aimed-at collider changes, not every frame.
        private IInteractable Resolve(Collider hitCollider)
        {
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
