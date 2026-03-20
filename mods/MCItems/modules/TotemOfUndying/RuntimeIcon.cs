using UnityEngine;

namespace TotemOfUndying;

internal static class RuntimeIcon
{
    public static Sprite CreateTotemIcon()
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.name = "TotemOfUndying_Icon";
        tex.filterMode = FilterMode.Point;

        var bg = new Color32(20, 28, 20, 255);
        var body = new Color32(236, 190, 90, 255);
        var glow = new Color32(120, 235, 95, 255);
        var dark = new Color32(100, 76, 42, 255);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, bg);
            }
        }

        for (int y = 14; y <= 49; y++)
        {
            for (int x = 21; x <= 42; x++)
            {
                if (x == 21 || x == 42 || y == 14 || y == 49)
                {
                    tex.SetPixel(x, y, dark);
                }
                else
                {
                    tex.SetPixel(x, y, body);
                }
            }
        }

        for (int y = 27; y <= 36; y++)
        {
            for (int x = 17; x <= 46; x++)
            {
                if (x == 17 || x == 46 || y == 27 || y == 36)
                {
                    tex.SetPixel(x, y, dark);
                }
                else
                {
                    tex.SetPixel(x, y, glow);
                }
            }
        }

        tex.SetPixel(27, 20, dark);
        tex.SetPixel(36, 20, dark);
        tex.SetPixel(27, 43, dark);
        tex.SetPixel(36, 43, dark);

        tex.Apply();
        var rect = new Rect(0, 0, size, size);
        return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 100f);
    }
}
