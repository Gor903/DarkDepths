namespace DarkDepths.Audio
{
    // Mixer routing of docs/04: every cue plays into one of these, all of them sit under Master.
    public enum AudioChannel
    {
        Sfx = 0,
        Ambient = 1,
        Music = 2,
        Ui = 3
    }
}
