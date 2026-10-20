using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DarkDepths.Player
{
    // Floating stick: the origin snaps to wherever the finger lands inside this rect.
    [RequireComponent(typeof(RectTransform))]
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private const int NoPointer = int.MinValue;

        [SerializeField] private RectTransform _stick;
        [SerializeField] private RectTransform _handle;
        [SerializeField] private Image _handleImage;

        private RectTransform _area;
        private Canvas _canvas;
        private SprintLatch _sprint;
        private int _pointerId = NoPointer;
        private Vector2 _origin;
        private float _radius = 140f;
        private float _deadZone = 0.15f;
        private Color _handleColor = Color.white;
        private Color _handleSprintColor = Color.white;

        public Vector2 Value { get; private set; }
        public bool SprintEngaged => _sprint != null && _sprint.Engaged;

        public void Configure(TouchControlsConfig config)
        {
            _radius = Mathf.Max(1f, config.JoystickRadius);
            _deadZone = config.JoystickDeadZone;
            _handleColor = config.HandleColor;
            _handleSprintColor = config.HandleSprintColor;
            _sprint = new SprintLatch(config.SprintEngageThreshold, config.SprintReleaseThreshold);

            _stick.sizeDelta = new Vector2(_radius * 2f, _radius * 2f);
            RefreshHandleColor();
        }

        private void Awake()
        {
            _area = (RectTransform)transform;
            _canvas = GetComponentInParent<Canvas>();
            SetStickVisible(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pointerId != NoPointer || !TryGetLocalPoint(eventData, out Vector2 local))
            {
                return;
            }

            _pointerId = eventData.pointerId;
            _origin = local;
            _stick.anchoredPosition = local;
            _handle.anchoredPosition = Vector2.zero;
            Value = Vector2.zero;

            if (_sprint.Release())
            {
                RefreshHandleColor();
            }

            SetStickVisible(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId || !TryGetLocalPoint(eventData, out Vector2 local))
            {
                return;
            }

            Vector2 offset = Vector2.ClampMagnitude(local - _origin, _radius);
            _handle.anchoredPosition = offset;

            Vector2 normalized = offset / _radius;
            if (_sprint.Evaluate(normalized))
            {
                RefreshHandleColor();
            }

            Value = ApplyDeadZone(normalized);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId)
            {
                return;
            }

            _pointerId = NoPointer;
            Value = Vector2.zero;

            if (_sprint.Release())
            {
                RefreshHandleColor();
            }

            SetStickVisible(false);
        }

        private bool TryGetLocalPoint(PointerEventData eventData, out Vector2 local)
        {
            Camera eventCamera = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? _canvas.worldCamera
                : null;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(_area, eventData.position, eventCamera, out local);
        }

        private Vector2 ApplyDeadZone(Vector2 raw)
        {
            float magnitude = raw.magnitude;
            if (magnitude <= _deadZone)
            {
                return Vector2.zero;
            }

            float scaled = (magnitude - _deadZone) / (1f - _deadZone);
            return raw / magnitude * scaled;
        }

        private void RefreshHandleColor()
        {
            _handleImage.color = SprintEngaged ? _handleSprintColor : _handleColor;
        }

        private void SetStickVisible(bool visible)
        {
            _stick.gameObject.SetActive(visible);
        }
    }
}
