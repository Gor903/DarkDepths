using System;
using DarkDepths.Core;
using UnityEngine;

namespace DarkDepths.Loot
{
    // Stage 1 placeholder: proves the interaction path. Inventory and ItemDefinition arrive at T5.1.
    public sealed class Pickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _displayName = "Item";

        private string _prompt;

        // Raised before the object leaves the room, so a listener still has its place to play from.
        public event Action Taken;

        public string Prompt => _prompt;

        public bool CanInteract(GameObject interactor)
        {
            return gameObject.activeSelf;
        }

        public void Interact(GameObject interactor)
        {
            Taken?.Invoke();
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            // Built once so reading Prompt every frame allocates nothing.
            _prompt = "Take " + _displayName;
        }
    }
}
