namespace DarkDepths.Noise
{
    // What the rest of the game is allowed to ask of the noise side; a test can stand in for it.
    public interface INoiseService
    {
        void Register(INoiseListener listener);

        void Unregister(INoiseListener listener);

        void Emit(in NoiseEvent noise);
    }
}
