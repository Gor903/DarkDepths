namespace DarkDepths.Audio
{
    // Names of the volume parameters exposed on the mixer asset; the setup tool and the service share them.
    public static class MixerParameters
    {
        public const string MasterVolume = "MasterVolume";
        public const string SfxVolume = "SfxVolume";
        public const string AmbientVolume = "AmbientVolume";
        public const string MusicVolume = "MusicVolume";
        public const string UiVolume = "UiVolume";

        public static string For(AudioChannel channel)
        {
            switch (channel)
            {
                case AudioChannel.Ambient:
                    return AmbientVolume;
                case AudioChannel.Music:
                    return MusicVolume;
                case AudioChannel.Ui:
                    return UiVolume;
                default:
                    return SfxVolume;
            }
        }
    }
}
