using DarkDepths.Audio;
using DarkDepths.Core;
using UnityEngine;

namespace DarkDepths.World
{
    // The three sounds a door has: the creak out, the creak back, and the latch when the leaf comes to rest.
    [RequireComponent(typeof(Door))]
    [DisallowMultipleComponent]
    public sealed class DoorAudio : MonoBehaviour
    {
        [SerializeField] private AudioCue _openCue;
        [SerializeField] private AudioCue _closeCue;
        [SerializeField] private AudioCue _latchCue;
        [Tooltip("Where the sounds come from; left empty, the door itself, which stands in its frame.")]
        [SerializeField] private Transform _source;

        private Door _door;
        private IAudioService _audio;

        private void Awake()
        {
            _door = GetComponent<Door>();
            if (_source == null)
            {
                _source = transform;
            }
        }

        private void OnEnable()
        {
            _door.Opening += PlayOpen;
            _door.Closing += PlayClose;
            _door.Latched += PlayLatch;
        }

        private void OnDisable()
        {
            _door.Opening -= PlayOpen;
            _door.Closing -= PlayClose;
            _door.Latched -= PlayLatch;
        }

        // Resolved in Start: every service has registered itself by then.
        private void Start()
        {
            _audio = Services.Get<IAudioService>();
            if (_audio == null)
            {
                Debug.LogError("DoorAudio found no audio service; the door stays silent.", this);
                enabled = false;
            }
        }

        private void PlayOpen()
        {
            Play(_openCue);
        }

        private void PlayClose()
        {
            Play(_closeCue);
        }

        private void PlayLatch()
        {
            Play(_latchCue);
        }

        private void Play(AudioCue cue)
        {
            if (_audio != null && cue != null)
            {
                _audio.Play(cue, _source.position);
            }
        }
    }
}
