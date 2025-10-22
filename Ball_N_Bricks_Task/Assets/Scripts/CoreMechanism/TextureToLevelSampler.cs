using UnityEngine;

public static class TextureToLevelSampler
{
    public static Color[,] CreateGridFromTexture(Texture2D texture)
    {
        if (texture == null || !texture.isReadable) return new Color[0, 0];

        var w = texture.width;
        var h = texture.height;

        var pixels = texture.GetPixels32();
        var grid = new Color[w, h];

        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var index = y * w + x;
                var c32 = pixels[index];
                grid[x, y] = new Color(c32.r / 255f, c32.g / 255f, c32.b / 255f, c32.a / 255f);
            }
        }
        return grid;
    }
}