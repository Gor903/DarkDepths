using UnityEngine;

namespace DarkDepths.Noise
{
    // Anything that can hear. Enemies implement it; the system keeps a register and never looks for them.
    public interface INoiseListener
    {
        Vector3 Position { get; }

        // Scales the reach of every sound for this listener; 0 is deaf.
        float HearingMultiplier { get; }

        void OnNoiseHeard(in NoiseEvent noise, float perceivedLoudness);
    }
}
