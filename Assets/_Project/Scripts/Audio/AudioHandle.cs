using System;

namespace DarkDepths.Audio
{
    // Names one playing voice. The generation makes a handle to an already recycled voice harmless.
    public readonly struct AudioHandle : IEquatable<AudioHandle>
    {
        public static readonly AudioHandle None = default;

        // Stored with an offset so that default(AudioHandle) is the invalid one.
        private readonly int _slotPlusOne;
        private readonly int _generation;

        public AudioHandle(int slot, int generation)
        {
            _slotPlusOne = slot + 1;
            _generation = generation;
        }

        public bool IsValid => _slotPlusOne > 0;

        public int Slot => _slotPlusOne - 1;

        public int Generation => _generation;

        public bool Equals(AudioHandle other)
        {
            return _slotPlusOne == other._slotPlusOne && _generation == other._generation;
        }

        public override bool Equals(object obj)
        {
            return obj is AudioHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (_slotPlusOne * 397) ^ _generation;
        }
    }
}
