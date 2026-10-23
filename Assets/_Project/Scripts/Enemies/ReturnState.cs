namespace DarkDepths.Enemies
{
    // The way back to the route. Something heard on the way is still worth turning around for.
    public sealed class ReturnState : IEnemyState
    {
        public EnemyStateId Id => EnemyStateId.Return;

        public void Enter(EnemyBrain brain)
        {
            brain.Body.MoveTo(brain.HasPatrol ? brain.CurrentPatrolPoint() : brain.Home,
                brain.Definition.WalkSpeed);
        }

        public void Tick(EnemyBrain brain, float deltaTime)
        {
            if (brain.Body.CanSeeTarget)
            {
                brain.Switch(EnemyStateId.Chase);
                return;
            }

            if (brain.HasInterest)
            {
                brain.Switch(EnemyStateId.Alerted);
                return;
            }

            if (brain.Body.HasArrived)
            {
                brain.Switch(EnemyStateId.Patrol);
            }
        }

        public void Exit(EnemyBrain brain)
        {
        }
    }
}
