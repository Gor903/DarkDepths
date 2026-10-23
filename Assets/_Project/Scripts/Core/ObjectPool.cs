using UnityEngine;

namespace DarkDepths.Core
{
    // The pool docs/07 asks for: a fixed set of instances made once and handed out, never created in
    // the middle of play. A full pool gives up whichever has been out the longest, so the count of
    // live objects is a number someone chose rather than whatever the player got up to.
    public sealed class ObjectPool<T> where T : Component
    {
        private readonly T[] _instances;
        private readonly bool[] _busy;
        private readonly int[] _taken;

        private int _stamp;

        public ObjectPool(T prefab, int capacity, Transform parent)
        {
            int size = Mathf.Max(1, capacity);
            _instances = new T[size];
            _busy = new bool[size];
            _taken = new int[size];

            for (int i = 0; i < size; i++)
            {
                T instance = Object.Instantiate(prefab, parent);
                instance.name = prefab.name + " " + i.ToString("00");
                instance.gameObject.SetActive(false);
                _instances[i] = instance;
            }
        }

        public int Capacity => _instances.Length;

        public int ActiveCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _busy.Length; i++)
                {
                    if (_busy[i])
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public T Take(Vector3 position, Quaternion rotation)
        {
            int slot = FindFree();
            if (slot < 0)
            {
                slot = FindOldest();
                _instances[slot].gameObject.SetActive(false);
            }

            T instance = _instances[slot];
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);

            _busy[slot] = true;
            _taken[slot] = _stamp++;
            return instance;
        }

        public bool Return(T instance)
        {
            for (int i = 0; i < _instances.Length; i++)
            {
                if (_instances[i] != instance)
                {
                    continue;
                }

                if (!_busy[i])
                {
                    return false;
                }

                _busy[i] = false;
                instance.gameObject.SetActive(false);
                return true;
            }

            return false;
        }

        public void ReturnAll()
        {
            for (int i = 0; i < _instances.Length; i++)
            {
                if (_busy[i])
                {
                    Return(_instances[i]);
                }
            }
        }

        private int FindFree()
        {
            for (int i = 0; i < _busy.Length; i++)
            {
                if (!_busy[i])
                {
                    return i;
                }
            }

            return -1;
        }

        private int FindOldest()
        {
            int oldest = 0;
            for (int i = 1; i < _taken.Length; i++)
            {
                if (_taken[i] < _taken[oldest])
                {
                    oldest = i;
                }
            }

            return oldest;
        }
    }
}
