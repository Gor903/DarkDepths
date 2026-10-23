namespace DarkDepths.Enemies
{
    // One state of the machine, as docs/05 asks: one class each, entered, ticked and left.
    public interface IEnemyState
    {
        EnemyStateId Id { get; }

        void Enter(EnemyBrain brain);

        void Tick(EnemyBrain brain, float deltaTime);

        void Exit(EnemyBrain brain);
    }
}
