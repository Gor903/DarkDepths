using UnityEngine;

namespace DarkDepths.Enemies
{
    // Walking the route, or standing the post when there is no route. Anything heard interrupts it.
    public sealed class PatrolState : IEnemyState
    {
        private Vector3 _target;

        public EnemyStateId Id => EnemyStateId.Patrol;

        public void Enter(EnemyBrain brain)
        {
            // Back on patrol is the moment the last alarm is written off.
            brain.ForgetInterest();

            if (!brain.HasPatrol)
            {
                brain.Body.Stop();
                return;
            }

            _target = brain.NextPatrolPoint();
            brain.Body.MoveTo(_target, brain.Definition.WalkSpeed);
        }

        public void Tick(EnemyBrain brain, float deltaTime)
        {
            // Seen beats heard: the spot is remembered as the thing to look at if they slip away again.
            if (brain.Body.CanSeeTarget)
            {
                brain.Notice(brain.Body.TargetPosition);
                brain.Switch(EnemyStateId.Alerted);
                return;
            }

            if (brain.HasInterest)
            {
                brain.Switch(EnemyStateId.Alerted);
                return;
            }

            if (!brain.HasPatrol || !brain.Body.HasArrived)
            {
                return;
            }

            _target = brain.NextPatrolPoint();
            brain.Body.MoveTo(_target, brain.Definition.WalkSpeed);
        }

        public void Exit(EnemyBrain brain)
        {
        }
    }
}
