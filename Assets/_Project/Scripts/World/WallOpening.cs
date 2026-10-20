using System.Collections.Generic;
using UnityEngine;

namespace DarkDepths.World
{
    // Cuts a doorway out of a wall segment: what is left are the panels beside the opening and the one above it.
    public sealed class WallOpening
    {
        private const float Epsilon = 1e-4f;

        private readonly WallPanel[] _panels;

        public WallOpening(float wallWidth, float wallHeight, float openingWidth, float openingHeight)
        {
            Width = Mathf.Max(0f, wallWidth);
            Height = Mathf.Max(0f, wallHeight);
            // An opening wider or taller than the wall would leave panels with negative size.
            OpeningWidth = Mathf.Clamp(openingWidth, 0f, Width);
            OpeningHeight = Mathf.Clamp(openingHeight, 0f, Height);

            var panels = new List<WallPanel>(3);

            float sideWidth = (Width - OpeningWidth) * 0.5f;
            if (sideWidth > Epsilon)
            {
                float offset = (OpeningWidth + sideWidth) * 0.5f;
                panels.Add(new WallPanel(-offset, Height * 0.5f, sideWidth, Height));
                panels.Add(new WallPanel(offset, Height * 0.5f, sideWidth, Height));
            }

            float headerHeight = Height - OpeningHeight;
            if (headerHeight > Epsilon && OpeningWidth > Epsilon)
            {
                panels.Add(new WallPanel(0f, OpeningHeight + headerHeight * 0.5f, OpeningWidth, headerHeight));
            }

            _panels = panels.ToArray();
        }

        public float Width { get; }
        public float Height { get; }
        public float OpeningWidth { get; }
        public float OpeningHeight { get; }

        public int Count => _panels.Length;

        public WallPanel this[int index] => _panels[index];

        public float SolidArea
        {
            get
            {
                float area = 0f;
                for (int i = 0; i < _panels.Length; i++)
                {
                    area += _panels[i].Area;
                }

                return area;
            }
        }
    }
}
