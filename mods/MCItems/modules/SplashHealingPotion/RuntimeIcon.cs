using UnityEngine;

namespace SplashHealingPotion;

internal static class RuntimeIcon
{
    public static Sprite CreateSplashHealingPotionIcon()
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.name = "SplashHealingPotion_Icon";
        tex.filterMode = FilterMode.Point;

        var bg = new Color32(34, 14, 24, 255);
        var c1 = new Color32(214, 88, 164, 255);
        var c2 = new Color32(255, 196, 223, 255);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, bg);
            }
        }

        // 简单画一个“药水瓶”主体 + 高光
        var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float r = size * 0.24f;
        float r2 = r * r;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2(x, y) - center;
                float d2 = p.sqrMagnitude;
                if (d2 > r2) continue;

                float t = Mathf.Clamp01(1f - Mathf.Sqrt(d2) / r);
                var col = Color32.Lerp(c1, c2, t * 0.55f);
                tex.SetPixel(x, y, col);
            }
        }

        for (int y = 12; y <= 20; y++)
        {
            for (int x = 26; x <= 37; x++)
            {
                tex.SetPixel(x, y, new Color32(245, 230, 238, 255));
            }
        }

        // 高光点
        int hx = (int)(center.x - r * 0.35f);
        int hy = (int)(center.y + r * 0.35f);
        for (int y = -3; y <= 3; y++)
        {
            for (int x = -3; x <= 3; x++)
            {
                int px = hx + x;
                int py = hy + y;
                if (px < 0 || px >= size || py < 0 || py >= size) continue;
                float d = (x * x + y * y);
                if (d > 9) continue;
                tex.SetPixel(px, py, new Color32(255, 255, 255, 255));
            }
        }

        tex.Apply();
        var rect = new Rect(0, 0, size, size);
        return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 100f);
    }
}
