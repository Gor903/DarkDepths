using System;
using DarkDepths.Noise;
using UnityEngine;

namespace DarkDepths.Audio
{
    // Which cue a surface sounds like. Footsteps use it now; impacts and drops will use the same table.
    [CreateAssetMenu(menuName = "DarkDepths/Surface Audio Set", fileName = "SurfaceAudioSet")]
    public sealed class SurfaceAudioSet : ScriptableObject
    {
        [Serializable]
        private struct Entry
        {
            public SurfaceType Surface;
            public AudioCue Cue;
        }

        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();
        [Tooltip("Used for a surface the table does not list, so a missing entry is quiet, not silent.")]
        [SerializeField] private AudioCue _fallback;

        public AudioCue CueFor(SurfaceType surface)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i].Surface == surface && _entries[i].Cue != null)
                {
                    return _entries[i].Cue;
                }
            }

            return _fallback;
        }
    }
}
