using System;
using System.IO;
using UnityEngine;

namespace MCPrerequisite;

internal static class ModAssets
{
    internal static Sprite? TryLoadSprite(string? modPath, string relativePath, string textureName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(modPath))
            {
                return null;
            }

            var assetPath = Path.Combine(modPath, relativePath);
            return TryLoadSpriteFromPngFile(assetPath, textureName);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MCPrerequisite] Failed to load sprite '{relativePath}': {e.Message}");
            return null;
        }
    }

    internal static Sprite? TryLoadSpriteFromPngFile(string path, string textureName)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var bytes = File.ReadAllBytes(path);
        if (bytes.Length == 0)
        {
            return null;
        }

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
        {
            name = textureName,
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        if (!texture.LoadImage(bytes, false))
        {
            UnityEngine.Object.Destroy(texture);
            return null;
        }

        var rect = new Rect(0f, 0f, texture.width, texture.height);
        var pivot = new Vector2(0.5f, 0.5f);
        var sprite = Sprite.Create(texture, rect, pivot, 100f);
        sprite.name = textureName + "_Sprite";
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }
}