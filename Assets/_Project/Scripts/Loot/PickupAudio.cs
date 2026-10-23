using DarkDepths.Audio;
using DarkDepths.Core;
using UnityEngine;

namespace DarkDepths.Loot
{
    // The sound of taking something off a shelf. The voice lives in the pool, so it outlives the object.
    [RequireComponent(typeof(Pickup))]
    [DisallowMultipleComponent]
    public sealed class PickupAudio : MonoBehaviour
    {
        [SerializeField] private AudioCue _cue;

        private Pickup _pickup;
        private IAudioService _audio;

        private void Awake()
        {
            _pickup = GetComponent<Pickup>();
        }

        private void OnEnable()
        {
            _pickup.Taken += Play;
        }

        private void OnDisable()
        {
            _pickup.Taken -= Play;
        }

        // Resolved in Start: every service has registered itself by then.
        private void Start()
        {
            _audio = Services.Get<IAudioService>();
            if (_audio == null)
            {
                Debug.LogError("PickupAudio found no audio service; taking the item stays silent.", this);
            }
        }

        private void Play()
        {
            if (_audio != null && _cue != null)
            {
                _audio.Play(_cue, transform.position);
            }
        }
    }
}
