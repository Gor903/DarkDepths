using DarkDepths.Core;
using UnityEngine;

namespace DarkDepths.World
{
    public sealed class Door : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform _pivot;
        [SerializeField] private float _openAngle = 95f;
        [SerializeField] private float _openSpeed = 200f;
        [SerializeField] private string _openPrompt = "Open";
        [SerializeField] private string _closePrompt = "Close";

        private bool _isOpen;
        private float _currentAngle;

        public string Prompt => _isOpen ? _closePrompt : _openPrompt;

        public bool CanInteract(GameObject interactor)
        {
            return true;
        }

        public void Interact(GameObject interactor)
        {
            _isOpen = !_isOpen;
        }

        private void Update()
        {
            float target = _isOpen ? _openAngle : 0f;
            if (Mathf.Approximately(_currentAngle, target))
            {
                return;
            }

            _currentAngle = Mathf.MoveTowards(_currentAngle, target, _openSpeed * Time.deltaTime);
            _pivot.localRotation = Quaternion.Euler(0f, _currentAngle, 0f);
        }
    }
}
