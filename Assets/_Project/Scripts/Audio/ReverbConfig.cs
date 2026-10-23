using System;
using UnityEngine;

namespace DarkDepths.Audio
{
    // What each kind of space sounds like, in one asset: rooms are placed from this table,
    // so a corridor built at the other end of the building answers the same way as this one.
    [CreateAssetMenu(menuName = "DarkDepths/Reverb Config", fileName = "ReverbConfig")]
    public sealed class ReverbConfig : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public ReverbSpace Space;
            public AudioReverbPreset Preset;
            [Tooltip("Metres: inside this radius the space is fully itself.")]
            public float MinDistance;
            [Tooltip("Metres: past this radius it is gone; between the two it blends with the neighbour.")]
            public float MaxDistance;
        }

        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();

        public bool TryGet(ReverbSpace space, out Entry entry)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i].Space == space)
                {
                    entry = _entries[i];
                    return true;
                }
            }

            entry = default;
            return false;
        }
    }
}
