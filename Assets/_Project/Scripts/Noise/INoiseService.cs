using UnityEngine;

namespace DarkDepths.Noise
{
    // What the rest of the game is allowed to ask of the noise side; a test can stand in for it.
    public interface INoiseService
    {
        void Register(INoiseListener listener);

        void Unregister(INoiseListener listener);

        void Emit(in NoiseEvent noise);

        // The same, with the reach read from the config's table; scale softens it for a weaker version
        // of the same sound. Whoever makes a noise names it and never carries the metres around.
        void Emit(NoiseSound sound, Vector3 position, NoiseType type, GameObject source, float scale);
    }
}
