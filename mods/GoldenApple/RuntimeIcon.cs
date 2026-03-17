using UnityEngine;

namespace GoldenApple;

internal static class RuntimeIcon
{
    public static Sprite CreateGoldenAppleIcon()
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.name = "GoldenApple_Icon";
        tex.filterMode = FilterMode.Point;

        var bg = new Color32(36, 22, 8, 255);
        var outline = new Color32(117, 73, 17, 255);
        var gold = new Color32(244, 196, 57, 255);
        var lightGold = new Color32(255, 231, 127, 255);
        var leaf = new Color32(114, 175, 72, 255);
        var stem = new Color32(132, 89, 39, 255);

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, bg);
            }
        }

        for (var y = 16; y <= 49; y++)
        {
            for (var x = 14; x <= 49; x++)
            {
                var nx = (x - 31.5f) / 18f;
                var ny = (y - 33f) / 17f;
                var appleShape = nx * nx + ny * ny;
                var topDip = Mathf.Abs(x - 31.5f) < 6f && y > 16 && y < 26;
                if (appleShape > 1.05f)
                {
                    continue;
                }

                var color = gold;
                if (topDip)
                {
                    color = bg;
                }
                else if (x < 25 || y > 41)
                {
                    color = lightGold;
                }

                tex.SetPixel(x, y, color);
            }
        }

        for (var y = 16; y <= 49; y++)
        {
            for (var x = 14; x <= 49; x++)
            {
                var current = tex.GetPixel(x, y);
                if (current.Equals(bg))
                {
                    continue;
                }

                var isEdge = tex.GetPixel(x - 1, y).Equals(bg)
                    || tex.GetPixel(x + 1, y).Equals(bg)
                    || tex.GetPixel(x, y - 1).Equals(bg)
                    || tex.GetPixel(x, y + 1).Equals(bg);
                if (isEdge)
                {
                    tex.SetPixel(x, y, outline);
                }
            }
        }

        for (var y = 10; y <= 18; y++)
        {
            tex.SetPixel(31, y, stem);
            tex.SetPixel(32, y, stem);
        }

        for (var y = 8; y <= 16; y++)
        {
            for (var x = 34; x <= 44; x++)
            {
                var dx = (x - 39f) / 6f;
                var dy = (y - 12f) / 4f;
                if (dx * dx + dy * dy <= 1f)
                {
                    tex.SetPixel(x, y, leaf);
                }
            }
        }

        for (var y = 22; y <= 30; y++)
        {
            for (var x = 20; x <= 27; x++)
            {
                if ((x + y) % 2 == 0)
                {
                    tex.SetPixel(x, y, new Color32(255, 248, 196, 255));
                }
            }
        }

        tex.Apply();
        var rect = new Rect(0, 0, size, size);
        return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 100f);
    }
}