using DarkDepths.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DarkDepths.UI
{
    // What a run looks like when it ends badly: the screen goes down to black, the controls stop
    // answering, and a line says so. What the player keeps is decided by the run, which comes later.
    public sealed class DeathOverlay : MonoBehaviour
    {
        [SerializeField] private InputGate _inputGate;
        [SerializeField] private Graphic _curtain;
        [SerializeField] private Graphic _label;
        [Tooltip("Seconds the screen takes to go dark.")]
        [SerializeField, Min(0.1f)] private float _fadeTime = 1.4f;

        private IPlayerTarget _target;
        private bool _dying;
        private float _alpha;

        private void Awake()
        {
            _curtain.raycastTarget = false;
            _label.raycastTarget = false;
            Fade(0f);
        }

        // Resolved in Start: the player registers itself in Awake.
        private void Start()
        {
            _target = Services.Get<IPlayerTarget>();
            if (_target == null)
            {
                Debug.LogWarning("DeathOverlay found no player; death will not be shown.", this);
                enabled = false;
                return;
            }

            _target.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (_target != null)
            {
                _target.Died -= OnDied;
            }
        }

        private void Update()
        {
            if (!_dying || _alpha >= 1f)
            {
                return;
            }

            Fade(Mathf.MoveTowards(_alpha, 1f, Time.deltaTime / _fadeTime));
        }

        private void OnDied()
        {
            if (_dying)
            {
                return;
            }

            _dying = true;
            // Taken once and never released: the run is over, and nothing after this reads input.
            _inputGate.Block();
        }

        private void Fade(float alpha)
        {
            _alpha = alpha;

            Color color = _curtain.color;
            color.a = alpha;
            _curtain.color = color;
            _curtain.enabled = alpha > 0.001f;

            Color labelColor = _label.color;
            // The line comes in behind the dark, so it is read against black rather than over the room.
            labelColor.a = Mathf.InverseLerp(0.6f, 1f, alpha);
            _label.color = labelColor;
            _label.enabled = labelColor.a > 0.001f;
        }
    }
}
