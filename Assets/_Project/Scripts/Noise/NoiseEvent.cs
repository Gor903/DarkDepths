using UnityEngine;

namespace DarkDepths.Noise
{
    // One sound as the game's ears receive it. Noise and audio are separate (docs/04): this carries no clip,
    // and a silent trigger or a sound nobody can hear are both allowed.
    public readonly struct NoiseEvent
    {
        public readonly Vector3 Position;

        // Metres it reaches a listener who hears at one, before surfaces and walls are taken into account.
        public readonly float Loudness;

        public readonly NoiseType Type;

        // May be null: a sound does not have to have an owner.
        public readonly GameObject Source;

        public NoiseEvent(Vector3 position, float loudness, NoiseType type, GameObject source = null)
        {
            Position = position;
            Loudness = loudness;
            Type = type;
            Source = source;
        }
    }
}
