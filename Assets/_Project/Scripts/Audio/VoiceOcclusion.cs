using UnityEngine;

namespace DarkDepths.Audio
{
    // Muffles the voices that have something between them and the listener (docs/04).
    // Every voice is smoothed every frame, but only a few of them re-test the line each frame:
    // a wall does not appear between two frames, and 24 rays a frame would be paid for nothing.
    public sealed class VoiceOcclusion
    {
        private readonly AudioLowPassFilter[] _filters;
        private readonly LowPassTracker[] _trackers;
        private readonly bool[] _blocked;

        private readonly LayerMask _mask;
        private readonly float _minDistance;
        private readonly int _checksPerFrame;

        private int _cursor;
        private int _checksLeft;

        public VoiceOcclusion(AudioLowPassFilter[] filters, AudioConfig config)
        {
            _filters = filters;
            _trackers = new LowPassTracker[filters.Length];
            _blocked = new bool[filters.Length];

            for (int slot = 0; slot < filters.Length; slot++)
            {
                _trackers[slot] = new LowPassTracker(config.OpenCutoff, config.BlockedCutoff, config.OcclusionSmoothTime);
                Apply(slot, config.OpenCutoff);
            }

            _mask = config.OcclusionMask;
            _minDistance = config.OcclusionMinDistance;
            _checksPerFrame = Mathf.Max(1, config.OcclusionChecksPerFrame);
        }

        public void BeginFrame()
        {
            // A frame that spent nothing has its cursor past the last voice in use; start the walk over.
            if (_checksLeft >= _checksPerFrame)
            {
                _cursor = 0;
            }

            _checksLeft = _checksPerFrame;
        }

        // A voice starts where it belongs: a sound behind a wall is muffled from its first sample.
        public void Prime(int slot, Vector3 source, Vector3 listener, float spatialBlend)
        {
            _blocked[slot] = spatialBlend > 0f && IsBlocked(source, listener);
            Apply(slot, _trackers[slot].Reset(_blocked[slot]));
        }

        public void Follow(int slot, Vector3 source, Vector3 listener, float spatialBlend, float deltaTime)
        {
            // A sound played in the player's head has no wall to be behind.
            if (spatialBlend <= 0f)
            {
                _blocked[slot] = false;
            }
            else if (Due(slot))
            {
                _blocked[slot] = IsBlocked(source, listener);
            }

            Apply(slot, _trackers[slot].Step(_blocked[slot], deltaTime));
        }

        public void Release(int slot)
        {
            _blocked[slot] = false;
            Apply(slot, _trackers[slot].Reset(false));
        }

        private void Apply(int slot, float cutoff)
        {
            AudioLowPassFilter filter = _filters[slot];
            // A scene being torn down takes the filters with it, while a voice is still being let go.
            if (filter != null)
            {
                filter.cutoffFrequency = cutoff;
            }
        }

        // Slots are visited in order, so the cursor walks the pool a few voices per frame.
        private bool Due(int slot)
        {
            if (_checksLeft <= 0 || slot < _cursor)
            {
                return false;
            }

            _checksLeft--;
            _cursor = slot + 1 < _filters.Length ? slot + 1 : 0;
            return true;
        }

        private bool IsBlocked(Vector3 source, Vector3 listener)
        {
            Vector3 delta = listener - source;
            float distance = delta.magnitude;
            if (distance <= _minDistance)
            {
                return false;
            }

            return Physics.Raycast(source, delta / distance, distance, _mask, QueryTriggerInteraction.Ignore);
        }
    }
}
