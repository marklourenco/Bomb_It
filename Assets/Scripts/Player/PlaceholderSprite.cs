using UnityEngine;

namespace BombIt.Presentation
{
    public static class PlaceholderSprite
    {
        public static Sprite CreateSolid(int pixelSize)
        {
            return CreateSolid(pixelSize, Color.white);
        }
        public static Sprite CreateSolid(int pixelSize, Color color)
        {
            var tex = new Texture2D(pixelSize, pixelSize)
            {
                filterMode = FilterMode.Point
            };
            var pixels = new Color[pixelSize * pixelSize];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, pixelSize, pixelSize), new Vector2(0.5f, 0.5f), pixelSize);
        }
    }
}