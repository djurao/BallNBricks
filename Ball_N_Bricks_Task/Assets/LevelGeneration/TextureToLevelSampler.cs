using UnityEngine;

namespace LevelGeneration
{
    public static class TextureToLevelSampler
    {
        // Creates Color[w,h] where each cell is the color of the corresponding pixel.
        // Assumes texture is a Texture2D (readable).
        public static Color[,] CreateGridFromTexture(Texture2D texture)
        {
            if (texture == null) return new Color[0, 0];
            var w = texture.width;
            var h = texture.height;
            var grid = new Color[w, h];

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    grid[x, y] = texture.GetPixel(x, y);
                }
            }
            return grid;
        }
    }
}