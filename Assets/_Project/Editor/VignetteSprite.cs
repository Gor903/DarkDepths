using System.IO;
using UnityEditor;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Paints the one sprite the UI needs: white in the middle, solid at the corners. Tinted black it is
    // the edge darkening docs/04 asks for, which a plain quad over the screen could never be.
    public static class VignetteSprite
    {
        private const int Size = 256;
        private const float ClearRadius = 0.45f;

        public static Sprite Create(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            var pixels = new Color32[Size * Size];
            float corner = Mathf.Sqrt(2f);

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float nx = (x + 0.5f) / Size * 2f - 1f;
                    float ny = (y + 0.5f) / Size * 2f - 1f;
                    float radius = Mathf.Sqrt(nx * nx + ny * ny) / corner;
                    float alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(ClearRadius, 1f, radius));

                    pixels[y * Size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Configure(path);

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void Configure(string path)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }
    }
}
