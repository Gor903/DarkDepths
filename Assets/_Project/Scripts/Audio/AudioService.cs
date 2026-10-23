using System;
using DarkDepths.Core;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace DarkDepths.Audio
{
    // Owns the voice pool and the mixer faders; nothing else creates an AudioSource.
    [DisallowMultipleComponent]
    public sealed class AudioService : MonoBehaviour, IAudioService
    {
        [SerializeField] private AudioConfig _config;

        private AudioSource[] _sources;
        private Transform[] _followed;
        private bool[] _attached;
        private VoiceAllocator _allocator;
        private VoiceOcclusion _occlusion;
        private Transform _listener;

        public event Action<Vector3> Hinted;

        public int VoiceCount => _sources == null ? 0 : _sources.Length;

        public int ActiveVoices => _allocator == null ? 0 : _allocator.ActiveCount;

        public AudioHandle Play(AudioCue cue, Vector3 position)
        {
            return PlayInternal(cue, position, null, 1f);
        }

        public AudioHandle Play(AudioCue cue, Vector3 position, float volumeScale)
        {
            return PlayInternal(cue, position, null, volumeScale);
        }

        public AudioHandle PlayAttached(AudioCue cue, Transform follow)
        {
            if (follow == null)
            {
                return AudioHandle.None;
            }

            return PlayInternal(cue, follow.position, follow, 1f);
        }

        public bool IsPlaying(in AudioHandle handle)
        {
            return _allocator != null && _allocator.IsActive(handle);
        }

        public void Stop(in AudioHandle handle)
        {
            if (_allocator != null && _allocator.IsActive(handle))
            {
                StopSlot(handle.Slot);
            }
        }

        public void StopAll()
        {
            if (_allocator == null)
            {
                return;
            }

            for (int slot = 0; slot < _sources.Length; slot++)
            {
                if (_allocator.IsBusy(slot))
                {
                    StopSlot(slot);
                }
            }
        }

        public void SetVolume(AudioChannel channel, float linear)
        {
            SetParameter(MixerParameters.For(channel), linear);
        }

        public float GetVolume(AudioChannel channel)
        {
            return GetParameter(MixerParameters.For(channel));
        }

        public void SetMasterVolume(float linear)
        {
            SetParameter(MixerParameters.MasterVolume, linear);
        }

        public float GetMasterVolume()
        {
            return GetParameter(MixerParameters.MasterVolume);
        }

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("AudioService has no AudioConfig; no sound will play.", this);
                enabled = false;
                return;
            }

            BuildPool();

            // Cached here and not looked for again: a scene has one listener, and it is on the player.
            AudioListener listener = FindAnyObjectByType<AudioListener>();
            if (listener == null)
            {
                Debug.LogWarning("AudioService found no AudioListener; nothing will be occluded.", this);
            }
            else
            {
                _listener = listener.transform;
            }

            Services.Register<IAudioService>(this);
        }

        private void OnDestroy()
        {
            // A destroyed MonoBehaviour behind an interface no longer reads as null, so the slot is cleared here.
            if (Services.Get<IAudioService>() == (IAudioService)this)
            {
                Services.Unregister<IAudioService>();
            }
        }

        // Voices follow their source and free themselves here, so Play never has to sweep the pool.
        private void LateUpdate()
        {
            if (_allocator == null)
            {
                return;
            }

            bool occlude = _listener != null;
            Vector3 listener = occlude ? _listener.position : Vector3.zero;
            float deltaTime = Time.deltaTime;
            if (occlude)
            {
                _occlusion.BeginFrame();
            }

            for (int slot = 0; slot < _sources.Length; slot++)
            {
                if (!_allocator.IsBusy(slot))
                {
                    continue;
                }

                AudioSource source = _sources[slot];
                if (_attached[slot])
                {
                    Transform follow = _followed[slot];
                    if (follow == null)
                    {
                        StopSlot(slot);
                        continue;
                    }

                    source.transform.position = follow.position;
                }

                if (!source.isPlaying)
                {
                    StopSlot(slot);
                    continue;
                }

                if (occlude)
                {
                    _occlusion.Follow(slot, source.transform.position, listener, source.spatialBlend, deltaTime);
                }
            }
        }

        private void BuildPool()
        {
            int count = Mathf.Max(1, _config.VoiceCount);
            _sources = new AudioSource[count];
            _followed = new Transform[count];
            _attached = new bool[count];
            var filters = new AudioLowPassFilter[count];

            for (int slot = 0; slot < count; slot++)
            {
                var holder = new GameObject("Voice " + slot.ToString("00"));
                holder.transform.SetParent(transform, false);

                var source = holder.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.rolloffMode = _config.Rolloff;
                source.dopplerLevel = _config.DopplerLevel;
                source.outputAudioMixerGroup = _config.GroupFor(AudioChannel.Sfx);
                _sources[slot] = source;

                // One filter per voice: what a sound is heard through depends on where that sound is.
                filters[slot] = holder.AddComponent<AudioLowPassFilter>();
            }

            _allocator = new VoiceAllocator(count);
            _occlusion = new VoiceOcclusion(filters, _config);
        }

        private AudioHandle PlayInternal(AudioCue cue, Vector3 position, Transform follow, float volumeScale)
        {
            if (cue == null || _allocator == null)
            {
                return AudioHandle.None;
            }

            AudioCueSample sample = cue.Sample(Random.value, Random.value, Random.value);
            if (!sample.HasClip)
            {
                return AudioHandle.None;
            }

            if (!_allocator.TryAcquire(cue.Priority, Time.unscaledTime, out AudioHandle handle))
            {
                return AudioHandle.None;
            }

            int slot = handle.Slot;
            AudioSource source = _sources[slot];
            // The slot may have been taken from a quieter voice that is still running.
            source.Stop();

            source.clip = sample.Clip;
            source.volume = sample.Volume * Mathf.Clamp01(volumeScale);
            source.pitch = sample.Pitch;
            source.loop = cue.Loop;
            source.priority = cue.Priority;
            source.spatialBlend = cue.SpatialBlend;
            source.minDistance = cue.MinDistance;
            source.maxDistance = cue.MaxDistance;
            // A sound in the player's head is not standing in the room, so it takes none of the room's reverb.
            source.bypassReverbZones = cue.SpatialBlend <= 0f;
            source.outputAudioMixerGroup = _config.GroupFor(cue.Channel);
            source.transform.position = position;

            _followed[slot] = follow;
            _attached[slot] = follow != null;

            if (_listener != null)
            {
                _occlusion.Prime(slot, position, _listener.position, cue.SpatialBlend);
            }

            source.Play();

            if (cue.Hints)
            {
                Hinted?.Invoke(position);
            }

            return handle;
        }

        private void StopSlot(int slot)
        {
            AudioSource source = _sources[slot];
            // A scene being torn down takes the voices with it, while a loop is still holding one.
            if (source != null)
            {
                source.Stop();
                source.clip = null;
            }

            _followed[slot] = null;
            _attached[slot] = false;
            _occlusion.Release(slot);
            _allocator.ReleaseSlot(slot);
        }

        private void SetParameter(string parameter, float linear)
        {
            AudioMixer mixer = _config == null ? null : _config.Mixer;
            if (mixer != null)
            {
                mixer.SetFloat(parameter, MixerVolume.ToDecibels(linear));
            }
        }

        private float GetParameter(string parameter)
        {
            AudioMixer mixer = _config == null ? null : _config.Mixer;
            if (mixer != null && mixer.GetFloat(parameter, out float decibels))
            {
                return MixerVolume.ToLinear(decibels);
            }

            return 1f;
        }
    }
}
