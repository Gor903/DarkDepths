namespace DarkDepths.Enemies
{
    // Striking at arm's length, no faster than the cooldown allows. Stepping out of reach puts the enemy
    // back to chasing rather than ending the hunt.
    public sealed class AttackState : IEnemyState
    {
        private float _cooldown;

        public EnemyStateId Id => EnemyStateId.Attack;

        public void Enter(EnemyBrain brain)
        {
            brain.Body.Stop();
            brain.Body.BeginHunt();

            // The first blow lands on arrival; what follows waits out the cooldown.
            _cooldown = 0f;
            Strike(brain);
        }

        public void Tick(EnemyBrain brain, float deltaTime)
        {
            IEnemyBody body = brain.Body;

            if (!body.HasTarget)
            {
                brain.Switch(EnemyStateId.Return);
                return;
            }

            if (body.DistanceToTarget > brain.Definition.AttackRange)
            {
                brain.Switch(EnemyStateId.Chase);
                return;
            }

            _cooldown -= deltaTime;
            if (_cooldown <= 0f)
            {
                Strike(brain);
            }
        }

        public void Exit(EnemyBrain brain)
        {
            brain.Body.EndHunt();
        }

        private void Strike(EnemyBrain brain)
        {
            brain.Body.Strike();
            _cooldown = brain.Definition.AttackCooldown;
        }
    }
}
