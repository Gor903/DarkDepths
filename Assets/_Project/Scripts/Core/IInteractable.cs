using UnityEngine;

namespace DarkDepths.Core
{
    // Interactor is passed as a GameObject: a Player-typed parameter would make Core depend on Player
    // and break the one-way module graph in docs/07_ARCHITECTURE.md.
    public interface IInteractable
    {
        string Prompt { get; }

        bool CanInteract(GameObject interactor);

        void Interact(GameObject interactor);
    }
}
