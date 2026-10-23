using DarkDepths.Audio;
using DarkDepths.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DarkDepths.UI
{
    // Shows a strong sound nearby as a faint mark at the edge of the screen, in the direction it came from.
    // docs/04 forbids anything that matters being audible only; this is the other half of that.
    public sealed class SoundHintOverlay : MonoBehaviour
    {
        [SerializeField] private UserSettings _settings;
        [SerializeField] private RectTransform _marker;
        [SerializeField] private Graphic _graphic;
        [Tooltip("Metres: a sound further out than this is not worth turning towards.")]
        [SerializeField, Min(1f)] private float _maxDistance = 14f;
        [SerializeField, Range(0f, 1f)] private float _maxAlpha = 0.35f;
        [Tooltip("Seconds the mark takes to fade out; no flashing, as docs/04 asks.")]
        [SerializeField, Min(0.1f)] private float _fadeTime = 1.2f;
        [Tooltip("How far out the mark sits, as a fraction of the half-height of the screen.")]
        [SerializeField, Range(0.3f, 1f)] private float _radius = 0.82f;

        private RectTransform _area;
        private Transform _listener;
        private IAudioService _audio;
        private Vector3 _source;
        private float _alpha;

        private void Awake()
        {
            _area = (RectTransform)transform;
            _graphic.raycastTarget = false;
            Fade(0f);
        }

        // Resolved in Start: the service registers itself in Awake, and the camera belongs to the player.
        private void Start()
        {
            _audio = Services.Get<IAudioService>();
            Camera camera = Camera.main;
            if (_audio == null || camera == null)
            {
                Debug.LogWarning("SoundHintOverlay found no audio service or no camera; sounds will not be shown.", this);
                enabled = false;
                return;
            }

            _listener = camera.transform;
            _audio.Hinted += Show;
        }

        private void OnDestroy()
        {
            if (_audio != null)
            {
                _audio.Hinted -= Show;
            }
        }

        private void Update()
        {
            if (_alpha <= 0f)
            {
                return;
            }

            // Followed while it shows: turning towards the sound has to move the mark, or it points nowhere.
            if (SoundHintPlacement.TryPlace(_listener.position, _listener.forward, _source, _maxDistance,
                    out float bearing, out _))
            {
                PlaceMarker(bearing);
            }

            Fade(Mathf.MoveTowards(_alpha, 0f, _maxAlpha * Time.deltaTime / _fadeTime));
        }

        private void Show(Vector3 position)
        {
            if (!_settings.SoundHints)
            {
                return;
            }

            if (!SoundHintPlacement.TryPlace(_listener.position, _listener.forward, position, _maxDistance,
                    out float bearing, out float strength))
            {
                return;
            }

            _source = position;
            PlaceMarker(bearing);
            // A louder hint never replaces a nearer one with a weaker mark halfway through its fade.
            Fade(Mathf.Max(_alpha, strength * _maxAlpha));
        }

        private void PlaceMarker(float bearing)
        {
            float radians = bearing * Mathf.Deg2Rad;
            float radius = _area.rect.height * 0.5f * _radius;

            _marker.anchoredPosition = new Vector2(Mathf.Sin(radians), Mathf.Cos(radians)) * radius;
            _marker.localRotation = Quaternion.Euler(0f, 0f, -bearing);
        }

        // Disabled at nothing, so a quiet room costs no draw call.
        private void Fade(float alpha)
        {
            _alpha = alpha;

            Color color = _graphic.color;
            color.a = alpha;
            _graphic.color = color;
            _graphic.enabled = alpha > 0.001f;
        }
    }
}
