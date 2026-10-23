namespace DarkDepths.Core
{
    // Anything that can be hurt. The player implements it now; enemies will when they can be fought back.
    public interface IDamageable
    {
        bool IsAlive { get; }

        void Damage(float amount);
    }
}
