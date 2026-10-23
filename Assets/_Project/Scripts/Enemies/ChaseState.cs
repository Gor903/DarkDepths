using UnityEngine;

namespace DarkDepths.Enemies
{
    // Running the player down. Losing sight does not end it at once: the enemy keeps going to where they
    // were, and only after loseTargetTime does it fall back to searching that spot.
    public sealed class ChaseState : IEnemyState
    {
        private Vector3 _lastSeen;
        private float _unseen;

        public EnemyStateId Id => EnemyStateId.Chase;

        public void Enter(EnemyBrain brain)
        {
            _unseen = 0f;
            _lastSeen = brain.Body.TargetPosition;
            brain.Body.BeginHunt();
            brain.Body.MoveTo(_lastSeen, brain.Definition.ChaseSpeed);
        }

        public void Tick(EnemyBrain brain, float deltaTime)
        {
            IEnemyBody body = brain.Body;

            if (!body.HasTarget)
            {
                brain.Switch(EnemyStateId.Return);
                return;
            }

            if (body.CanSeeTarget)
            {
                _unseen = 0f;
                _lastSeen = body.TargetPosition;
                body.MoveTo(_lastSeen, brain.Definition.ChaseSpeed);

                if (body.DistanceToTarget <= brain.Definition.AttackRange)
                {
                    brain.Switch(EnemyStateId.Attack);
                }

                return;
            }

            _unseen += deltaTime;
            if (_unseen < brain.Definition.LoseTargetTime)
            {
                return;
            }

            // The trail goes cold where they were last seen, and that is where the search starts.
            brain.Notice(_lastSeen);
            brain.Switch(EnemyStateId.Search);
        }

        public void Exit(EnemyBrain brain)
        {
            brain.Body.EndHunt();
        }
    }
}
