using UnityEngine;

namespace DarkDepths.Enemies
{
    // The numbers a silhouette enemy is cut and animated from; the look lives here, not in the generator (docs/02).
    [CreateAssetMenu(menuName = "DarkDepths/Silhouette Config", fileName = "SilhouetteConfig")]
    public sealed class SilhouetteConfig : ScriptableObject
    {
        // A clip whose keys share a time stops being a curve, so every duration keeps a floor.
        private const float MinDuration = 0.01f;

        [Header("Legs (m)")]
        [SerializeField] private float _thighLength = 0.42f;
        [SerializeField] private float _shinLength = 0.42f;
        [SerializeField] private float _legThickness = 0.11f;

        [Header("Torso (m)")]
        [SerializeField] private float _pelvisHeight = 0.18f;
        [SerializeField] private float _waistWidth = 0.3f;
        [SerializeField] private float _waistDepth = 0.18f;
        [SerializeField] private float _spineLength = 0.26f;
        [SerializeField] private float _chestHeight = 0.4f;
        [SerializeField] private float _chestWidth = 0.42f;
        [SerializeField] private float _chestDepth = 0.22f;

        [Header("Head (m)")]
        [SerializeField] private float _neckLength = 0.12f;
        [Tooltip("Narrow and tall: a head that is not quite human is what the player reads first.")]
        [SerializeField] private float _headWidth = 0.18f;
        [SerializeField] private float _headHeight = 0.26f;
        [SerializeField] private float _headDepth = 0.2f;

        [Header("Arms (m)")]
        [SerializeField] private float _upperArmLength = 0.34f;
        [Tooltip("Longer than the upper arm; the hands hang far below where they should.")]
        [SerializeField] private float _forearmLength = 0.42f;
        [SerializeField] private float _armThickness = 0.09f;
        [Tooltip("How far the shoulder sits below the top of the chest.")]
        [SerializeField] private float _shoulderDrop = 0.06f;

        [Header("Posture (deg)")]
        [Tooltip("Forward bend of the spine; the neck and head undo it so the face still aims forward.")]
        [SerializeField] private float _hunchAngle = 14f;
        [SerializeField] private float _headCockAngle = 8f;
        [SerializeField] private float _armRestAngle = 7f;
        [SerializeField] private float _elbowRestAngle = -16f;
        [Tooltip("How far the arms hang clear of the chest.")]
        [SerializeField] private float _armSpreadAngle = 5f;

        [Header("Walk")]
        [SerializeField] private float _strideDuration = 1.1f;
        [SerializeField] private float _strideAngle = 26f;
        [SerializeField] private float _kneeAngle = 34f;
        [SerializeField] private float _armSwingAngle = 16f;
        [Tooltip("How much of the stride the dragging leg loses; this is the limp.")]
        [SerializeField] private float _limpAngle = 9f;
        [SerializeField] private float _swayAngle = 4f;
        [SerializeField] private float _headScanAngle = 7f;
        [SerializeField] private float _bobHeight = 0.035f;

        [Header("Idle")]
        [Tooltip("Seconds per breath; idle stays near still, so the first real move is the one that reads.")]
        [SerializeField] private float _idleDuration = 4.5f;
        [SerializeField] private float _idleSwayAngle = 1.5f;
        [SerializeField] private float _idleHeadDriftAngle = 3f;

        [Header("Head turn")]
        [SerializeField] private float _headTurnAngle = 72f;
        [Tooltip("Seconds to snap the head round; short enough to read as a reaction, not a look.")]
        [SerializeField] private float _headTurnTime = 0.12f;
        [SerializeField] private float _headTurnHold = 0.9f;
        [SerializeField] private float _headTurnReturnTime = 0.5f;
        [SerializeField] private float _headTurnChestAngle = 10f;

        [Header("Clips")]
        [Tooltip("Keys baked per cycle; the curves are smoothed between them.")]
        [SerializeField, Range(4, 64)] private int _cycleSamples = 16;
        [SerializeField] private float _clipFrameRate = 30f;
        [SerializeField] private float _blendTime = 0.15f;

        public float StrideDuration => Mathf.Max(MinDuration, _strideDuration);
        public float IdleDuration => Mathf.Max(MinDuration, _idleDuration);
        public float IdleSwayAngle => _idleSwayAngle;
        public float IdleHeadDriftAngle => _idleHeadDriftAngle;

        public float HeadTurnAngle => _headTurnAngle;
        public float HeadTurnTime => Mathf.Max(MinDuration, _headTurnTime);
        public float HeadTurnHold => Mathf.Max(0f, _headTurnHold);
        public float HeadTurnReturnTime => Mathf.Max(MinDuration, _headTurnReturnTime);
        public float HeadTurnChestAngle => _headTurnChestAngle;

        public int CycleSamples => _cycleSamples;
        public float ClipFrameRate => Mathf.Max(1f, _clipFrameRate);
        public float BlendTime => Mathf.Max(0f, _blendTime);

        public SilhouetteProportions CreateProportions()
        {
            return new SilhouetteProportions
            {
                ThighLength = _thighLength,
                ShinLength = _shinLength,
                LegThickness = _legThickness,
                PelvisHeight = _pelvisHeight,
                WaistWidth = _waistWidth,
                WaistDepth = _waistDepth,
                SpineLength = _spineLength,
                ChestHeight = _chestHeight,
                ChestWidth = _chestWidth,
                ChestDepth = _chestDepth,
                NeckLength = _neckLength,
                HeadWidth = _headWidth,
                HeadHeight = _headHeight,
                HeadDepth = _headDepth,
                UpperArmLength = _upperArmLength,
                ForearmLength = _forearmLength,
                ArmThickness = _armThickness,
                ShoulderDrop = _shoulderDrop,
                HunchAngle = _hunchAngle,
                HeadCockAngle = _headCockAngle,
                ArmRestAngle = _armRestAngle,
                ElbowRestAngle = _elbowRestAngle,
                ArmSpreadAngle = _armSpreadAngle
            };
        }

        public SilhouetteGait CreateGait()
        {
            return new SilhouetteGait(_strideAngle, _kneeAngle, _armSwingAngle, _limpAngle, _swayAngle,
                _headScanAngle, _bobHeight);
        }
    }
}
