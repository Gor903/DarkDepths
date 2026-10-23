using UnityEngine;

namespace DarkDepths.Enemies
{
    // One kind of enemy, in numbers. The starting values are the Listener's, from docs/05.
    [CreateAssetMenu(menuName = "DarkDepths/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [Header("Movement (m/s)")]
        [SerializeField] private float _walkSpeed = 1.5f;
        [SerializeField] private float _chaseSpeed = 3.5f;
        [Tooltip("Metres from a destination at which it counts as reached.")]
        [SerializeField] private float _arriveDistance = 0.4f;

        [Header("Hearing")]
        [Tooltip("Scales the reach of every noise for this enemy; the Listener hears half as far again.")]
        [SerializeField] private float _hearingMultiplier = 1.5f;
        [Tooltip("Below this perceived loudness a noise is not worth turning around for.")]
        [SerializeField, Range(0f, 1f)] private float _perceivedThreshold = 0.15f;

        [Header("Sight")]
        [SerializeField] private float _sightRange = 3f;
        [SerializeField, Range(0f, 180f)] private float _sightAngle = 90f;
        [Tooltip("How much further the player is seen with the flashlight on.")]
        [SerializeField, Range(0f, 3f)] private float _lightSensitivity = 0.5f;

        [Header("Attack")]
        [SerializeField] private float _attackRange = 1.2f;
        [SerializeField] private float _attackDamage = 30f;
        [SerializeField] private float _attackCooldown = 1.5f;

        [Header("Behaviour (s)")]
        [Tooltip("The pause on hearing something: docs/05 puts it between half a second and one and a half.")]
        [SerializeField] private float _alertDuration = 0.9f;
        [SerializeField] private float _investigateTime = 4f;
        [SerializeField] private float _searchTime = 10f;
        [SerializeField] private float _searchRadius = 6f;
        [SerializeField] private float _loseTargetTime = 3f;

        [Header("Thinking")]
        [Tooltip("Decisions a second; docs/05 asks for five to ten, never once a frame.")]
        [SerializeField, Range(1f, 20f)] private float _tickRate = 8f;

        public float WalkSpeed => _walkSpeed;

        public float ChaseSpeed => _chaseSpeed;

        public float ArriveDistance => _arriveDistance;

        public float HearingMultiplier => _hearingMultiplier;

        public float PerceivedThreshold => _perceivedThreshold;

        public float SightRange => _sightRange;

        public float SightAngle => _sightAngle;

        public float LightSensitivity => _lightSensitivity;

        public float AttackRange => _attackRange;

        public float AttackDamage => _attackDamage;

        public float AttackCooldown => _attackCooldown;

        public float AlertDuration => _alertDuration;

        public float InvestigateTime => _investigateTime;

        public float SearchTime => _searchTime;

        public float SearchRadius => _searchRadius;

        public float LoseTargetTime => _loseTargetTime;

        public float TickInterval => 1f / Mathf.Max(1f, _tickRate);
    }
}
