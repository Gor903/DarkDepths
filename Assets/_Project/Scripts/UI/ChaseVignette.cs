using DarkDepths.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DarkDepths.UI
{
    // The visible half of being hunted (docs/04): the edges of the screen darken while something is
    // after the player, and lift again when it gives up. It fades, so there is never a flash.
    public sealed class ChaseVignette : MonoBehaviour
    {
        [SerializeField] private Graphic _graphic;
        [SerializeField, Range(0f, 1f)] private float _maxAlpha = 0.45f;
        [Tooltip("Seconds to darken; letting go is half again as slow, so the relief is not instant.")]
        [SerializeField, Min(0.05f)] private float _fadeTime = 0.6f;

        private IPlayerTarget _target;
        private float _alpha;

        private void Awake()
        {
            _graphic.raycastTarget = false;
            Fade(0f);
        }

        // Resolved in Start: the player registers itself in Awake.
        private void Start()
        {
            _target = Services.Get<IPlayerTarget>();
            if (_target == null)
            {
                Debug.LogWarning("ChaseVignette found no player; nothing will be shown.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            bool hunted = _target.IsHunted;
            float target = hunted ? _maxAlpha : 0f;
            float speed = _maxAlpha / (hunted ? _fadeTime : _fadeTime * 1.5f);

            if (!Mathf.Approximately(_alpha, target))
            {
                Fade(Mathf.MoveTowards(_alpha, target, speed * Time.deltaTime));
            }
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
