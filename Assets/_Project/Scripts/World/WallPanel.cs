namespace DarkDepths.World
{
    // One solid rectangle of a wall, measured from the wall's bottom centre.
    public readonly struct WallPanel
    {
        public WallPanel(float centerX, float centerY, float width, float height)
        {
            CenterX = centerX;
            CenterY = centerY;
            Width = width;
            Height = height;
        }

        public float CenterX { get; }
        public float CenterY { get; }
        public float Width { get; }
        public float Height { get; }

        public float Area => Width * Height;
    }
}
