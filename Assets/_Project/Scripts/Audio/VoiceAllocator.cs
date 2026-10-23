using UnityEngine;

namespace DarkDepths.Audio
{
    // Hands out the fixed number of voices docs/07 budgets. Priority follows AudioSource: 0 matters most.
    public sealed class VoiceAllocator
    {
        private readonly bool[] _busy;
        private readonly int[] _priority;
        private readonly float[] _startedAt;
        private readonly int[] _generation;

        public VoiceAllocator(int capacity)
        {
            int size = Mathf.Max(1, capacity);
            _busy = new bool[size];
            _priority = new int[size];
            _startedAt = new float[size];
            _generation = new int[size];
        }

        public int Capacity => _busy.Length;

        public int ActiveCount
        {
            get
            {
                int count = 0;
                for (int slot = 0; slot < _busy.Length; slot++)
                {
                    if (_busy[slot])
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        // A full pool gives up its least important voice, and only to something more important.
        public bool TryAcquire(int priority, float time, out AudioHandle handle)
        {
            int slot = FindFree();
            if (slot < 0)
            {
                slot = FindVictim(priority);
            }

            if (slot < 0)
            {
                handle = AudioHandle.None;
                return false;
            }

            _generation[slot]++;
            _busy[slot] = true;
            _priority[slot] = priority;
            _startedAt[slot] = time;
            handle = new AudioHandle(slot, _generation[slot]);
            return true;
        }

        public bool IsBusy(int slot)
        {
            return slot >= 0 && slot < _busy.Length && _busy[slot];
        }

        public bool IsActive(in AudioHandle handle)
        {
            if (!handle.IsValid)
            {
                return false;
            }

            int slot = handle.Slot;
            return slot >= 0 && slot < _busy.Length && _busy[slot] && _generation[slot] == handle.Generation;
        }

        public bool Release(in AudioHandle handle)
        {
            if (!IsActive(handle))
            {
                return false;
            }

            _busy[handle.Slot] = false;
            return true;
        }

        public void ReleaseSlot(int slot)
        {
            if (slot >= 0 && slot < _busy.Length)
            {
                _busy[slot] = false;
            }
        }

        public void ReleaseAll()
        {
            for (int slot = 0; slot < _busy.Length; slot++)
            {
                _busy[slot] = false;
            }
        }

        private int FindFree()
        {
            for (int slot = 0; slot < _busy.Length; slot++)
            {
                if (!_busy[slot])
                {
                    return slot;
                }
            }

            return -1;
        }

        private int FindVictim(int priority)
        {
            int victim = -1;
            for (int slot = 0; slot < _busy.Length; slot++)
            {
                if (_priority[slot] <= priority)
                {
                    continue;
                }

                // Equally unimportant voices: the one that has been running longest goes first.
                if (victim < 0 || _priority[slot] > _priority[victim] ||
                    (_priority[slot] == _priority[victim] && _startedAt[slot] < _startedAt[victim]))
                {
                    victim = slot;
                }
            }

            return victim;
        }
    }
}
