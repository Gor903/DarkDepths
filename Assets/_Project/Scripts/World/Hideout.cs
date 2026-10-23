using DarkDepths.Core;
using UnityEngine;

namespace DarkDepths.World
{
    // A place to be inside of. Stepping in hides whoever did it until something searching comes right
    // up to it (docs/05); the same press steps back out.
    [DisallowMultipleComponent]
    public sealed class Hideout : MonoBehaviour, IInteractable
    {
        [Tooltip("Where whoever hides here stands, and which way they face looking out.")]
        [SerializeField] private Transform _anchor;
        [Tooltip("Metres: a searcher closer than this finds what is inside.")]
        [SerializeField, Min(0f)] private float _revealDistance = 2f;
        [SerializeField] private string _enterPrompt = "Hide";

        private IHideable _occupant;

        public bool IsOccupied => _occupant != null && _occupant.IsHidden;

        public string Prompt => _enterPrompt;

        public bool CanInteract(GameObject interactor)
        {
            return _anchor != null && !IsOccupied;
        }

        public void Interact(GameObject interactor)
        {
            var hideable = interactor.GetComponent<IHideable>();
            if (hideable == null || _anchor == null)
            {
                return;
            }

            if (hideable.IsHidden)
            {
                hideable.Leave();
                _occupant = null;
                return;
            }

            hideable.Hide(_anchor, transform.position, _revealDistance);
            _occupant = hideable;
        }
    }
}
