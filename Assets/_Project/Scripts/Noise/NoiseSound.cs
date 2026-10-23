namespace DarkDepths.Noise
{
    // The rows of the loudness table in docs/04. One sound the game can make, so its reach is a number
    // in a config and never a literal in the thing that makes it.
    public enum NoiseSound
    {
        CrouchStep = 0,
        WalkStep = 1,
        RunStep = 2,
        FlashlightClick = 3,
        DoorOpen = 4,
        DoorCreak = 5,
        ObjectFall = 6,
        ThrowImpact = 7,
        BatonHit = 8,
        Generator = 9
    }
}
