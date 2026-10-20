using UnityEngine;

namespace DarkDepths.World
{
    // The measurements every modular piece is cut to; rooms are assembled on this grid (docs/06).
    [CreateAssetMenu(menuName = "DarkDepths/Module Config", fileName = "ModuleConfig")]
    public sealed class ModuleConfig : ScriptableObject
    {
        [Header("Grid (m)")]
        [Tooltip("Edge of one floor tile and the width of one wall segment; everything snaps to it.")]
        [SerializeField] private float _cellSize = 2f;
        [SerializeField] private float _wallHeight = 3f;
        [SerializeField] private float _wallThickness = 0.2f;
        [Tooltip("Thickness of floor and ceiling slabs; the walkable surface stays at y = 0.")]
        [SerializeField] private float _deckThickness = 0.2f;

        [Header("Doorway (m)")]
        [SerializeField] private float _doorWidth = 1.2f;
        [SerializeField] private float _doorHeight = 2.2f;
        [SerializeField] private float _doorLeafThickness = 0.08f;

        [Header("Furniture (m)")]
        [SerializeField] private float _shelfWidth = 1.6f;
        [SerializeField] private float _shelfHeight = 2f;
        [SerializeField] private float _shelfDepth = 0.4f;
        [SerializeField] private float _tableWidth = 1.4f;
        [SerializeField] private float _tableHeight = 0.75f;
        [SerializeField] private float _tableDepth = 0.8f;
        [SerializeField] private float _crateSize = 0.8f;
        [Tooltip("Thickness of boards and legs; the one number that keeps the furniture looking like a set.")]
        [SerializeField] private float _boardThickness = 0.08f;

        public float CellSize => _cellSize;
        public float WallHeight => _wallHeight;
        public float WallThickness => _wallThickness;
        public float DeckThickness => _deckThickness;

        public float DoorWidth => _doorWidth;
        public float DoorHeight => _doorHeight;
        public float DoorLeafThickness => _doorLeafThickness;

        public float ShelfWidth => _shelfWidth;
        public float ShelfHeight => _shelfHeight;
        public float ShelfDepth => _shelfDepth;
        public float TableWidth => _tableWidth;
        public float TableHeight => _tableHeight;
        public float TableDepth => _tableDepth;
        public float CrateSize => _crateSize;
        public float BoardThickness => _boardThickness;

        // A wall sits with its inner face on the room boundary, so the interior keeps its full size.
        public float WallOffset => _wallThickness * 0.5f;
    }
}
