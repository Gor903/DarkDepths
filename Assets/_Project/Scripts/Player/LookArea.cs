using UnityEngine;
using UnityEngine.EventSystems;

namespace DarkDepths.Player
{
    // Swipe surface for the camera. Delta is measured since the previous read, so no script order dependency.
    public sealed class LookArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private const int NoPointer = int.MinValue;

        private int _pointerId = NoPointer;
        private Vector2 _current;
        private Vector2 _lastRead;

        public Vector2 ConsumeDelta()
        {
            if (_pointerId == NoPointer)
            {
                return Vector2.zero;
            }

            Vector2 delta = _current - _lastRead;
            _lastRead = _current;
            return delta;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pointerId != NoPointer)
            {
                return;
            }

            _pointerId = eventData.pointerId;
            _current = eventData.position;
            _lastRead = _current;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId)
            {
                return;
            }

            _current = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId)
            {
                return;
            }

            _pointerId = NoPointer;
        }
    }
}
