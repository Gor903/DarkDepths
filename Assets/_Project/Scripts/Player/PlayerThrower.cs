using System;
using DarkDepths.Core;
using UnityEngine;

namespace DarkDepths.Player
{
    // The bottle of docs/03: thrown from where the player is looking, loud where it lands, and gone
    // from the pocket. It is what buys a corridor: the Listener goes to the noise, not to the player.
    [DisallowMultipleComponent]
    public sealed class PlayerThrower : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private Rigidbody _prefab;
        [Tooltip("Thrown from the camera, so it goes where the player is looking.")]
        [SerializeField] private Transform _origin;
        [Tooltip("How many can be in the air or on the floor at once; the oldest is picked back up.")]
        [SerializeField, Range(1, 8)] private int _poolSize = 4;
        [SerializeField, Min(0)] private int _startingCount = 3;
        [SerializeField] private float _throwSpeed = 9f;
        [Tooltip("Metres in front of the camera the bottle leaves the hand.")]
        [SerializeField] private float _releaseDistance = 0.5f;
        [SerializeField] private float _spin = 6f;
        [SerializeField, Min(0f)] private float _cooldown = 0.4f;

        private ObjectPool<Rigidbody> _pool;
        private Transform _holder;
        private float _nextThrow;

        public event Action<int> CountChanged;

        public int Count { get; private set; }

        public bool CanThrow => Count > 0 && Time.time >= _nextThrow;

        public bool TryThrow()
        {
            if (!CanThrow || _pool == null)
            {
                return false;
            }

            Rigidbody body = _pool.Take(_origin.position + _origin.forward * _releaseDistance, _origin.rotation);
            // Taken back from the floor it still carries the last throw; both are replaced, not added to.
            body.linearVelocity = _origin.forward * _throwSpeed;
            body.angularVelocity = UnityEngine.Random.onUnitSphere * _spin;

            _nextThrow = Time.time + _cooldown;
            Count--;
            CountChanged?.Invoke(Count);
            return true;
        }

        public void Add(int count)
        {
            if (count <= 0)
            {
                return;
            }

            Count += count;
            CountChanged?.Invoke(Count);
        }

        private void Awake()
        {
            if (_input == null || _prefab == null || _origin == null)
            {
                Debug.LogError("PlayerThrower is missing its input, bottle or camera; nothing will be thrown.", this);
                enabled = false;
                return;
            }

            // Not under the player: a thrown bottle stays where it landed, it does not follow them.
            _holder = new GameObject("Thrown").transform;
            _pool = new ObjectPool<Rigidbody>(_prefab, _poolSize, _holder);

            Count = _startingCount;
        }

        private void OnDestroy()
        {
            if (_holder != null)
            {
                Destroy(_holder.gameObject);
            }
        }

        private void Update()
        {
            if (_input.ConsumeThrow())
            {
                TryThrow();
            }
        }
    }
}
