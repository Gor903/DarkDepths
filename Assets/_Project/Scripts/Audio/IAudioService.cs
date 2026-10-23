using UnityEngine;

namespace DarkDepths.Audio
{
    // What the rest of the game is allowed to ask of the audio side; a test can stand in for it.
    public interface IAudioService
    {
        AudioHandle Play(AudioCue cue, Vector3 position);

        // The same cue played softer: a crouched step is the same sound, not a different one.
        AudioHandle Play(AudioCue cue, Vector3 position, float volumeScale);

        AudioHandle PlayAttached(AudioCue cue, Transform follow);

        bool IsPlaying(in AudioHandle handle);

        void Stop(in AudioHandle handle);

        void StopAll();

        void SetVolume(AudioChannel channel, float linear);

        float GetVolume(AudioChannel channel);

        void SetMasterVolume(float linear);

        float GetMasterVolume();
    }
}
