using System;
using DarkDepths.Core;
using UnityEngine;

namespace DarkDepths.Player
{
    // How much the player can take before a run ends. The rules of what is lost arrive with the run itself;
    // this only decides the moment of death and tells whoever is listening.
    [DisallowMultipleComponent]
    public sealed class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _maxHealth = 100f;

        private float _health;

        public event Action Died;

        public event Action<float> Changed;

        public bool IsAlive => _health > 0f;

        public float Normalized => _maxHealth > 0f ? Mathf.Clamp01(_health / _maxHealth) : 0f;

        public void Damage(float amount)
        {
            if (amount <= 0f || !IsAlive)
            {
                return;
            }

            _health = Mathf.Max(0f, _health - amount);
            Changed?.Invoke(Normalized);

            if (_health <= 0f)
            {
                Died?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || !IsAlive)
            {
                return;
            }

            _health = Mathf.Min(_maxHealth, _health + amount);
            Changed?.Invoke(Normalized);
        }

        private void Awake()
        {
            _health = _maxHealth;
        }
    }
}
