using System.Collections.Generic;
using DarkDepths.Core;
using DarkDepths.Noise;
using UnityEngine;
using UnityEngine.AI;

namespace DarkDepths.Enemies
{
    // The enemy in the scene: a NavMeshAgent the brain steers, an ear on the noise register, and the
    // silhouette's clips. Thinking happens a few times a second (docs/05), not every frame.
    [RequireComponent(typeof(NavMeshAgent))]
    [DisallowMultipleComponent]
    public sealed class EnemyAgent : MonoBehaviour, IEnemyBody, INoiseListener
    {
        [SerializeField] private EnemyDefinition _definition;
        [SerializeField] private SilhouetteAnimator _animator;
        [Tooltip("Walked in order; empty means the enemy stands where it was placed.")]
        [SerializeField] private Transform[] _patrolPoints;

        private readonly List<Vector3> _route = new List<Vector3>();

        private NavMeshAgent _agent;
        private EnemyBrain _brain;
        private INoiseService _noise;
        private float _nextTick;

        public EnemyStateId State => _brain == null ? EnemyStateId.Patrol : _brain.State;

        public Vector3 Position => transform.position;

        public float HearingMultiplier => _definition == null ? 0f : _definition.HearingMultiplier;

        public bool HasArrived
        {
            get
            {
                if (_agent.pathPending)
                {
                    return false;
                }

                return !_agent.hasPath || _agent.remainingDistance <= _definition.ArriveDistance;
            }
        }

        public void MoveTo(Vector3 destination, float speed)
        {
            // An agent standing off the mesh refuses a destination and says so once a frame.
            if (!_agent.isOnNavMesh)
            {
                return;
            }

            _agent.speed = speed;
            _agent.isStopped = false;
            _agent.SetDestination(destination);
        }

        public void Stop()
        {
            if (_agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
            }
        }

        public void LookAround()
        {
            if (_animator != null)
            {
                _animator.TriggerHeadTurn();
            }
        }

        public bool TrySamplePointNear(Vector3 center, float radius, out Vector3 point)
        {
            Vector2 offset = Random.insideUnitCircle * radius;
            Vector3 candidate = center + new Vector3(offset.x, 0f, offset.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                point = hit.position;
                return true;
            }

            point = center;
            return false;
        }

        public void OnNoiseHeard(in NoiseEvent noise, float perceivedLoudness)
        {
            _brain?.Hear(noise.Position, perceivedLoudness);
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_definition == null)
            {
                Debug.LogError("EnemyAgent has no EnemyDefinition; it will not think.", this);
                enabled = false;
                return;
            }

            if (_animator == null)
            {
                _animator = GetComponent<SilhouetteAnimator>();
            }

            _brain = new EnemyBrain(_definition, this);
            _brain.SetPatrol(BuildRoute());
        }

        // Resolved in Start: every service has registered itself by then.
        private void Start()
        {
            _brain.Begin(transform.position);

            _noise = Services.Get<INoiseService>();
            if (_noise == null)
            {
                Debug.LogError("EnemyAgent found no noise service; this enemy is deaf.", this);
                return;
            }

            _noise.Register(this);
            // Spread out, so four enemies never think on the same frame (docs/05).
            _nextTick = Time.time + Random.value * _definition.TickInterval;
        }

        private void OnEnable()
        {
            if (_noise != null)
            {
                _noise.Register(this);
            }
        }

        private void OnDisable()
        {
            if (_noise != null)
            {
                _noise.Unregister(this);
            }
        }

        private void Update()
        {
            if (_animator != null)
            {
                _animator.SetWalking(_agent.velocity.sqrMagnitude > 0.01f);
            }

            if (Time.time < _nextTick)
            {
                return;
            }

            float interval = _definition.TickInterval;
            _brain.Tick(Time.time - _nextTick + interval);
            _nextTick = Time.time + interval;
        }

        private List<Vector3> BuildRoute()
        {
            _route.Clear();
            if (_patrolPoints == null)
            {
                return _route;
            }

            for (int i = 0; i < _patrolPoints.Length; i++)
            {
                if (_patrolPoints[i] != null)
                {
                    _route.Add(_patrolPoints[i].position);
                }
            }

            return _route;
        }
    }
}
