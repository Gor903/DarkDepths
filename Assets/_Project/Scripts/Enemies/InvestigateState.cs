using UnityEngine;

namespace DarkDepths.Enemies
{
    // Walking to where the noise came from and looking around once there. A louder noise on the way
    // takes over; an empty spot ends in a search.
    public sealed class InvestigateState : IEnemyState
    {
        private Vector3 _target;
        private float _waited;
        private bool _arrived;

        public EnemyStateId Id => EnemyStateId.Investigate;

        public void Enter(EnemyBrain brain)
        {
            _waited = 0f;
            _arrived = false;
            Retarget(brain);
        }

        public void Tick(EnemyBrain brain, float deltaTime)
        {
            if (brain.Body.CanSeeTarget)
            {
                brain.Switch(EnemyStateId.Chase);
                return;
            }

            // Interest only ever changes upwards, so a different target means a louder noise.
            if (brain.Interest != _target)
            {
                _arrived = false;
                _waited = 0f;
                Retarget(brain);
                return;
            }

            if (!_arrived)
            {
                if (!brain.Body.HasArrived)
                {
                    return;
                }

                _arrived = true;
                brain.Body.Stop();
                brain.Body.LookAround();
                return;
            }

            _waited += deltaTime;
            if (_waited >= brain.Definition.InvestigateTime)
            {
                brain.Switch(EnemyStateId.Search);
            }
        }

        public void Exit(EnemyBrain brain)
        {
        }

        private void Retarget(EnemyBrain brain)
        {
            _target = brain.Interest;
            brain.Body.MoveTo(_target, brain.Definition.WalkSpeed);
        }
    }
}
