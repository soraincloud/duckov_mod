using System;
using System.IO;
using UnityEngine;

namespace GoldenApple;

internal static class ModAssets
{
    private const string IconFileName = "icon.png";
    private const string GoldenAppleIconRelativePath = "assets/item-icons/GoldenApple.png";

    internal static Sprite? TryLoadIconSprite(string? modPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(modPath))
            {
                return null;
            }

            var itemIconPath = Path.Combine(modPath, GoldenAppleIconRelativePath);
            if (File.Exists(itemIconPath))
            {
                var sprite = TryLoadSpriteFromPngFile(itemIconPath, "GoldenApple_Icon");
                if (sprite != null)
                {
                    return sprite;
                }
            }

            var iconPath = Path.Combine(modPath, IconFileName);
            if (!File.Exists(iconPath))
            {
                return null;
            }

            return TryLoadSpriteFromPngFile(iconPath, "GoldenApple_Icon");
        }
        catch (Exception e)
        {
            ModLog.Warn($"[GoldenApple] Failed to load icon sprite: {e.Message}");
            return null;
        }
    }

    private static Sprite? TryLoadSpriteFromPngFile(string pngPath, string textureName)
    {
        if (string.IsNullOrWhiteSpace(pngPath) || !File.Exists(pngPath))
        {
            return null;
        }

        var pngBytes = File.ReadAllBytes(pngPath);
        if (pngBytes.Length == 0)
        {
            return null;
        }

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.name = textureName;
        texture.filterMode = FilterMode.Point;
        if (!ImageConversion.LoadImage(texture, pngBytes))
        {
            UnityEngine.Object.Destroy(texture);
            return null;
        }

        var rect = new Rect(0f, 0f, texture.width, texture.height);
        return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 50f);
    }
}