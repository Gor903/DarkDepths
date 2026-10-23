using System;
using DarkDepths.Core;
using UnityEngine;

namespace DarkDepths.Player
{
    // The one place the enemies reach the player through: where they are, whether their light gives them
    // away, how many are after them, and the blow itself.
    [RequireComponent(typeof(PlayerHealth))]
    [DisallowMultipleComponent]
    public sealed class PlayerTarget : MonoBehaviour, IPlayerTarget
    {
        [SerializeField] private PlayerHealth _health;
        [SerializeField] private Flashlight _flashlight;
        [Tooltip("Where the player is seen from; the camera, not the floor under it.")]
        [SerializeField] private Transform _eye;

        private int _hunters;

        public event Action Died;

        public bool IsAlive => _health.IsAlive;

        public Vector3 Position => transform.position;

        public Vector3 EyePosition => _eye != null ? _eye.position : transform.position;

        public bool IsLit => _flashlight != null && _flashlight.IsOn;

        public bool IsHunted => _hunters > 0;

        public void Damage(float amount)
        {
            _health.Damage(amount);
        }

        // Counted rather than set: two enemies giving up one after the other must not clear the warning early.
        public void AddHunter()
        {
            _hunters++;
        }

        public void RemoveHunter()
        {
            _hunters = Mathf.Max(0, _hunters - 1);
        }

        private void Awake()
        {
            if (_health == null)
            {
                _health = GetComponent<PlayerHealth>();
            }

            _health.Died += OnDied;
            Services.Register<IPlayerTarget>(this);
        }

        private void OnDestroy()
        {
            _health.Died -= OnDied;

            // A destroyed MonoBehaviour behind an interface no longer reads as null, so the slot is cleared here.
            if (Services.Get<IPlayerTarget>() == (IPlayerTarget)this)
            {
                Services.Unregister<IPlayerTarget>();
            }
        }

        private void OnDied()
        {
            _hunters = 0;
            Died?.Invoke();
        }
    }
}
