namespace DarkDepths.Enemies
{
    // The short reaction of docs/05: stop, turn the head, then go and look. The pause is what lets the
    // player understand that this enemy heard something, and gives them the moment to move away.
    public sealed class AlertedState : IEnemyState
    {
        public EnemyStateId Id => EnemyStateId.Alerted;

        public void Enter(EnemyBrain brain)
        {
            brain.Body.Stop();
            brain.Body.LookAround();
        }

        public void Tick(EnemyBrain brain, float deltaTime)
        {
            if (brain.TimeInState < brain.Definition.AlertDuration)
            {
                return;
            }

            if (brain.Body.CanSeeTarget)
            {
                brain.Switch(EnemyStateId.Chase);
                return;
            }

            // Nothing in sight and nothing left to look at: whatever it was, it is over.
            brain.Switch(brain.HasInterest ? EnemyStateId.Investigate : EnemyStateId.Patrol);
        }

        public void Exit(EnemyBrain brain)
        {
        }
    }
}
