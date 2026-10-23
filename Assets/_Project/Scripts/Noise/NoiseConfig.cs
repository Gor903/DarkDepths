using System;
using UnityEngine;

namespace DarkDepths.Noise
{
    // Every number the noise model of docs/04 runs on. Starting values are the ones in the document;
    // balance is tuned here and nowhere else.
    [CreateAssetMenu(menuName = "DarkDepths/Noise Config", fileName = "NoiseConfig")]
    public sealed class NoiseConfig : ScriptableObject
    {
        [Serializable]
        private struct LoudnessEntry
        {
            public NoiseSound Sound;
            [Tooltip("Metres of reach at a hearing multiplier of one.")]
            public float Metres;
        }

        [Serializable]
        private struct SurfaceEntry
        {
            public SurfaceType Surface;
            public float Multiplier;
        }

        [Header("Loudness (m)")]
        [SerializeField]
        private LoudnessEntry[] _loudness =
        {
            new LoudnessEntry { Sound = NoiseSound.CrouchStep, Metres = 3f },
            new LoudnessEntry { Sound = NoiseSound.WalkStep, Metres = 8f },
            new LoudnessEntry { Sound = NoiseSound.RunStep, Metres = 15f },
            new LoudnessEntry { Sound = NoiseSound.FlashlightClick, Metres = 2f },
            new LoudnessEntry { Sound = NoiseSound.DoorOpen, Metres = 5f },
            new LoudnessEntry { Sound = NoiseSound.DoorCreak, Metres = 10f },
            new LoudnessEntry { Sound = NoiseSound.ObjectFall, Metres = 12f },
            new LoudnessEntry { Sound = NoiseSound.ThrowImpact, Metres = 12f },
            new LoudnessEntry { Sound = NoiseSound.BatonHit, Metres = 6f },
            new LoudnessEntry { Sound = NoiseSound.Generator, Metres = 20f }
        };

        [Header("Surfaces")]
        [SerializeField]
        private SurfaceEntry[] _surfaces =
        {
            new SurfaceEntry { Surface = SurfaceType.Carpet, Multiplier = 0.6f },
            new SurfaceEntry { Surface = SurfaceType.Concrete, Multiplier = 1f },
            new SurfaceEntry { Surface = SurfaceType.Wood, Multiplier = 1.1f },
            new SurfaceEntry { Surface = SurfaceType.Metal, Multiplier = 1.3f },
            new SurfaceEntry { Surface = SurfaceType.Glass, Multiplier = 1.6f }
        };

        [Tooltip("Used for a surface the table does not list.")]
        [SerializeField] private float _defaultSurfaceMultiplier = 1f;

        [Header("Walls")]
        [Tooltip("What is left of the reach with a wall between source and listener; the MVP rule of docs/04.")]
        [SerializeField, Range(0f, 1f)] private float _wallMultiplier = 0.5f;
        [SerializeField] private LayerMask _wallMask;

        [Header("Surface probe")]
        [Tooltip("The probe starts this far above the sound and looks down.")]
        [SerializeField] private float _probeHeight = 0.5f;
        [SerializeField] private float _probeDistance = 1.2f;
        [SerializeField] private LayerMask _probeMask;
        [SerializeField] private SurfaceType _defaultSurface = SurfaceType.Concrete;

        [Header("Debug")]
        [Tooltip("Seconds a noise keeps its gizmo after it is heard; without it balance cannot be read.")]
        [SerializeField, Min(0f)] private float _gizmoSeconds = 1f;
        [SerializeField]
        private Color[] _gizmoColors =
        {
            new Color(0.6f, 0.85f, 1f),
            new Color(1f, 0.8f, 0.35f),
            new Color(1f, 0.55f, 0.3f),
            new Color(1f, 0.35f, 0.35f),
            new Color(0.6f, 1f, 0.6f),
            new Color(0.85f, 0.7f, 1f)
        };

        public float WallMultiplier => _wallMultiplier;

        public LayerMask WallMask => _wallMask;

        public float ProbeHeight => _probeHeight;

        public float ProbeDistance => _probeDistance;

        public LayerMask ProbeMask => _probeMask;

        public SurfaceType DefaultSurface => _defaultSurface;

        public float GizmoSeconds => _gizmoSeconds;

        // A sound the table does not list is silent rather than loud by accident.
        public float LoudnessOf(NoiseSound sound)
        {
            for (int i = 0; i < _loudness.Length; i++)
            {
                if (_loudness[i].Sound == sound)
                {
                    return Mathf.Max(0f, _loudness[i].Metres);
                }
            }

            return 0f;
        }

        public float MultiplierFor(SurfaceType surface)
        {
            for (int i = 0; i < _surfaces.Length; i++)
            {
                if (_surfaces[i].Surface == surface)
                {
                    return Mathf.Max(0f, _surfaces[i].Multiplier);
                }
            }

            return _defaultSurfaceMultiplier;
        }

        public Color ColorFor(NoiseType type)
        {
            int index = (int)type;
            return index >= 0 && index < _gizmoColors.Length ? _gizmoColors[index] : Color.white;
        }
    }
}
