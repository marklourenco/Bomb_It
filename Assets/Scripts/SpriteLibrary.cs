using BombIt.Simulation;
using System.Collections.Generic;
using UnityEngine;

namespace BombIt.Presentation
{
    public static class SpriteLibrary
    {
        private const int placeholderPixelSize = 32;
        private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        private const string grassSheetPath = "Sprites/TX Tileset Grass";
        private const string wallSheetPath = "Sprites/TX Tileset Wall";
        private const string propsSheetPath = "Sprites/TX Props";
        private static readonly string[] floorSpriteNames =
        {
            "TX Tileset Grass 15",
            "TX Tileset Grass Flower 1",
            "TX Tileset Grass Flower 8"
        };
        public static Sprite GetFloor(int x, int y)
        {
            int hash = x * 92821 + y * 68917;
            hash = (hash ^ (hash >> 13)) * 0x45d9f3b;
            hash = hash ^ (hash >> 16);
            int variantIndex = Mathf.Abs(hash) % floorSpriteNames.Length;
            return LoadFromSheet(grassSheetPath, floorSpriteNames[variantIndex], new Color(0.85f, 0.85f, 0.85f));
        }
        public static Sprite GetWall()
        {
            return LoadFromSheet(wallSheetPath, "TX Tileset Wall_41", new Color(0.25f, 0.25f, 0.25f));
        }
        public static Sprite GetBox()
        {
            return LoadFromSheet(propsSheetPath, "TX Props Crate", new Color(0.55f, 0.35f, 0.15f));
        }
        public static Sprite GetBomb()
        {
            return LoadOrPlaceholder("Sprites/bomb", new Color(0.1f, 0.1f, 0.1f));
        }
        public static Sprite GetExplosion()
        {
            return LoadOrPlaceholder("Sprites/explosion", new Color(1.0f, 0.5f, 0.1f));
        }
        public static Sprite GetUpgrade(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Range:
                    return LoadOrPlaceholder("Sprites/upgrade_range", new Color(0.9f, 0.2f, 0.2f));
                case UpgradeType.BombCount:
                    return LoadOrPlaceholder("Sprites/upgrade_bomb_count", new Color(0.2f, 0.6f, 0.9f));
                case UpgradeType.Speed:
                    return LoadOrPlaceholder("Sprites/upgrade_speed", new Color(0.9f, 0.85f, 0.2f));
                default:
                    return LoadOrPlaceholder("Sprites/upgrade_unknown", Color.white);
            }
        }
        public static Sprite GetPlayer(int playerId)
        {
            return LoadOrPlaceholder($"Sprites/player_{playerId}", GetFallbackPlayerColor(playerId));
        }
        public static Color GetFallbackPlayerColor(int playerId)
        {
            switch (playerId)
            {
                case 0:
                    return new Color(0.2f, 0.5f, 0.95f);
                case 1:
                    return new Color(0.9f, 0.2f, 0.2f);
                case 2:
                    return new Color(0.2f, 0.85f, 0.35f);
                case 3:
                    return new Color(0.95f, 0.85f, 0.15f);
                default:
                    return Color.white;
            }
        }
        private static Sprite LoadOrPlaceholder(string resourcePath, Color fallbackColor)
        {
            Sprite cached;
            if (cache.TryGetValue(resourcePath, out cached))
            {
                return cached;
            }
            var loaded = Resources.Load<Sprite>(resourcePath);
            var result = loaded != null ? loaded : PlaceholderSprite.CreateSolid(placeholderPixelSize, fallbackColor);
            cache[resourcePath] = result;
            return result;
        }
        private static Sprite LoadFromSheet(string sheetResourcePath, string spriteName, Color fallbackColor)
        {
            string cacheKey = sheetResourcePath + "::" + spriteName;
            Sprite cached;
            if (cache.TryGetValue(cacheKey, out cached))
            {
                return cached;
            }
            var sheetSprites = Resources.LoadAll<Sprite>(sheetResourcePath);
            Sprite found = null;
            foreach (var sprite in sheetSprites)
            {
                if (sprite.name == spriteName)
                {
                    found = sprite;
                    break;
                }
            }
            var result = found != null ? found : PlaceholderSprite.CreateSolid(placeholderPixelSize, fallbackColor);
            cache[cacheKey] = result;
            return result;
        }
    }
}