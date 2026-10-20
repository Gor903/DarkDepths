using UnityEngine;

namespace DarkDepths.Core
{
    // Keeps a full-screen rect inside the safe area so notches and gesture bars never cover a control.
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _appliedArea;
        private ScreenOrientation _appliedOrientation;

        private void Awake()
        {
            _rect = (RectTransform)transform;
        }

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            if (Screen.safeArea != _appliedArea || Screen.orientation != _appliedOrientation)
            {
                Apply();
            }
        }

        private void Apply()
        {
            Rect area = Screen.safeArea;
            _appliedArea = area;
            _appliedOrientation = Screen.orientation;

            float width = Screen.width;
            float height = Screen.height;
            if (width <= 0f || height <= 0f)
            {
                return;
            }

            _rect.anchorMin = new Vector2(area.xMin / width, area.yMin / height);
            _rect.anchorMax = new Vector2(area.xMax / width, area.yMax / height);
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
