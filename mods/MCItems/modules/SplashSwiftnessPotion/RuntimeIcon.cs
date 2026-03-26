using UnityEngine;

namespace SplashSwiftnessPotion;

internal static class RuntimeIcon
{
    public static Sprite CreateSplashSwiftnessPotionIcon()
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "SplashSwiftnessPotion_Icon",
            filterMode = FilterMode.Point
        };

        var bg = new Color32(12, 23, 38, 255);
        var c1 = new Color32(42, 154, 255, 255);
        var c2 = new Color32(171, 235, 255, 255);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, bg);
            }
        }

        var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.24f;
        float radiusSq = radius * radius;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var point = new Vector2(x, y) - center;
                float distSq = point.sqrMagnitude;
                if (distSq > radiusSq)
                {
                    continue;
                }

                float t = Mathf.Clamp01(1f - Mathf.Sqrt(distSq) / radius);
                tex.SetPixel(x, y, Color32.Lerp(c1, c2, t * 0.65f));
            }
        }

        for (int y = 12; y <= 20; y++)
        {
            for (int x = 26; x <= 37; x++)
            {
                tex.SetPixel(x, y, new Color32(225, 247, 255, 255));
            }
        }

        int highlightX = (int)(center.x - radius * 0.35f);
        int highlightY = (int)(center.y + radius * 0.35f);
        for (int y = -3; y <= 3; y++)
        {
            for (int x = -3; x <= 3; x++)
            {
                int px = highlightX + x;
                int py = highlightY + y;
                if (px < 0 || px >= size || py < 0 || py >= size)
                {
                    continue;
                }

                if (x * x + y * y > 9)
                {
                    continue;
                }

                tex.SetPixel(px, py, Color.white);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}