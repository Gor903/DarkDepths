using UnityEngine;
using UnityEngine.EventSystems;

namespace DarkDepths.Player
{
    // On-screen button that reports both a hold state and a one-shot press.
    public sealed class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private const int NoPointer = int.MinValue;

        private int _pointerId = NoPointer;
        private bool _pressPending;

        public bool IsHeld => _pointerId != NoPointer;

        public bool ConsumePress()
        {
            if (!_pressPending)
            {
                return false;
            }

            _pressPending = false;
            return true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pointerId != NoPointer)
            {
                return;
            }

            _pointerId = eventData.pointerId;
            _pressPending = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId)
            {
                return;
            }

            _pointerId = NoPointer;
        }

        private void OnDisable()
        {
            _pointerId = NoPointer;
            _pressPending = false;
        }
    }
}
