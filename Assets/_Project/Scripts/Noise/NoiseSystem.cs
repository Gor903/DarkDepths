using System.Collections.Generic;
using DarkDepths.Core;
using UnityEngine;

namespace DarkDepths.Noise
{
    // Who heard what. Listeners put themselves on the register, a sound is handed to the ones in reach,
    // and nothing here plays anything: noise and audio are two systems (docs/04).
    [DisallowMultipleComponent]
    public sealed class NoiseSystem : MonoBehaviour, INoiseService
    {
        [SerializeField] private NoiseConfig _config;

        private readonly List<INoiseListener> _listeners = new List<INoiseListener>();

        // Dispatch runs off a copy, so a listener may unregister itself the moment it hears something.
        private INoiseListener[] _buffer = new INoiseListener[8];

        public int ListenerCount => _listeners.Count;

        public void Register(INoiseListener listener)
        {
            if (listener != null && !_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void Unregister(INoiseListener listener)
        {
            _listeners.Remove(listener);
        }

        public void Emit(in NoiseEvent noise)
        {
            if (_config == null || noise.Loudness <= 0f)
            {
                return;
            }

            float surface = SurfaceMultiplierAt(noise.Position);
            Remember(noise, noise.Loudness * surface);

            int count = Fill();
            for (int i = 0; i < count; i++)
            {
                Deliver(_buffer[i], noise, surface);
            }
        }

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("NoiseSystem has no NoiseConfig; nothing will be heard.", this);
                enabled = false;
                return;
            }

            Services.Register<INoiseService>(this);
        }

        private void OnDestroy()
        {
            // A destroyed MonoBehaviour behind an interface no longer reads as null, so the slot is cleared here.
            if (Services.Get<INoiseService>() == (INoiseService)this)
            {
                Services.Unregister<INoiseService>();
            }
        }

        private void Deliver(INoiseListener listener, in NoiseEvent noise, float surface)
        {
            float hearing = listener.HearingMultiplier;
            float clear = NoiseFalloff.Radius(noise.Loudness, hearing, surface, false, _config.WallMultiplier);
            float distance = Vector3.Distance(listener.Position, noise.Position);
            if (distance >= clear)
            {
                return;
            }

            // Only the listeners already in reach are worth a raycast.
            float radius = IsBlocked(noise.Position, listener.Position)
                ? NoiseFalloff.Radius(noise.Loudness, hearing, surface, true, _config.WallMultiplier)
                : clear;

            float perceived = NoiseFalloff.Perceived(distance, radius);
            if (perceived > 0f)
            {
                listener.OnNoiseHeard(noise, perceived);
            }
        }

        private int Fill()
        {
            int count = _listeners.Count;
            if (_buffer.Length < count)
            {
                _buffer = new INoiseListener[Mathf.NextPowerOfTwo(count)];
            }

            for (int i = 0; i < count; i++)
            {
                _buffer[i] = _listeners[i];
            }

            return count;
        }

        private bool IsBlocked(Vector3 from, Vector3 to)
        {
            if (_config.WallMask.value == 0)
            {
                return false;
            }

            return Physics.Linecast(from, to, _config.WallMask, QueryTriggerInteraction.Ignore);
        }

        // What the sound is standing on: carpet swallows a step, metal carries it (docs/04).
        private float SurfaceMultiplierAt(Vector3 position)
        {
            Vector3 origin = position + Vector3.up * _config.ProbeHeight;
            SurfaceType surface = _config.DefaultSurface;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit,
                    _config.ProbeHeight + _config.ProbeDistance, _config.ProbeMask, QueryTriggerInteraction.Ignore))
            {
                surface = Surface.TypeOf(hit.collider, _config.DefaultSurface);
            }

            return _config.MultiplierFor(surface);
        }

#if UNITY_EDITOR
        private struct Mark
        {
            public Vector3 Position;
            public float Radius;
            public NoiseType Type;
            public float Time;
        }

        private const int MarkCount = 16;

        private readonly Mark[] _marks = new Mark[MarkCount];
        private int _nextMark;

        private void Remember(in NoiseEvent noise, float radius)
        {
            _marks[_nextMark] = new Mark
            {
                Position = noise.Position,
                Radius = radius,
                Type = noise.Type,
                Time = Time.unscaledTime
            };

            _nextMark = _nextMark + 1 < MarkCount ? _nextMark + 1 : 0;
        }

        // The circle of docs/04: without seeing what a sound reached, its loudness cannot be tuned.
        private void OnDrawGizmos()
        {
            if (_config == null || !Application.isPlaying)
            {
                return;
            }

            float hold = _config.GizmoSeconds;
            for (int i = 0; i < _marks.Length; i++)
            {
                Mark mark = _marks[i];
                float age = Time.unscaledTime - mark.Time;
                if (mark.Radius <= 0f || age > hold)
                {
                    continue;
                }

                Color color = _config.ColorFor(mark.Type);
                color.a = hold > 0f ? 1f - age / hold : 1f;
                Gizmos.color = color;
                Gizmos.DrawWireSphere(mark.Position, mark.Radius);
            }
        }
#else
        private void Remember(in NoiseEvent noise, float radius)
        {
        }
#endif
    }
}
