using System;
using UnityEngine;

namespace GoldenApple;

internal static class GoldenAppleEnchantedIcon
{
    private const float FramesPerSecond = 14f;
    private const float ScrollSpeed = 0.45f;
    private const float StripeWidth = 0.2f;
    private const float StripeFeather = 0.08f;
    private const float StripeSkew = 0.28f;
    private const float BlendStrength = 0.5f;
    private static readonly Color EnchantTint = new(0.78f, 0.32f, 0.96f, 1f);

    private static Texture2D? _animatedTexture;
    private static Sprite? _animatedSprite;
    private static Color32[]? _basePixels;
    private static Color32[]? _workingPixels;
    private static GoldenAppleEnchantedIconHost? _host;
    private static float _nextFrameTime;

    internal static Sprite Create(Sprite sourceSprite)
    {
        if (sourceSprite == null)
        {
            throw new ArgumentNullException(nameof(sourceSprite));
        }

        Shutdown();

        var sourceTexture = sourceSprite.texture;
        _basePixels = sourceTexture.GetPixels32();
        _workingPixels = new Color32[_basePixels.Length];

        _animatedTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false)
        {
            name = "GoldenApple_EnchantedIcon",
            filterMode = sourceTexture.filterMode,
            wrapMode = TextureWrapMode.Clamp
        };

        Array.Copy(_basePixels, _workingPixels, _basePixels.Length);
        _animatedTexture.SetPixels32(_workingPixels);
        _animatedTexture.Apply(false, false);

        var pivot = new Vector2(
            sourceSprite.pivot.x / sourceSprite.rect.width,
            sourceSprite.pivot.y / sourceSprite.rect.height);
        _animatedSprite = Sprite.Create(_animatedTexture, sourceSprite.rect, pivot, sourceSprite.pixelsPerUnit);
        _animatedSprite.name = sourceSprite.name + "_Enchanted";

        EnsureHost();
        UpdateFrame(force: true);
        return _animatedSprite;
    }

    internal static void Shutdown()
    {
        if (_host != null)
        {
            try
            {
                UnityEngine.Object.Destroy(_host.gameObject);
            }
            catch
            {
                // ignore
            }

            _host = null;
        }

        if (_animatedSprite != null)
        {
            try
            {
                UnityEngine.Object.Destroy(_animatedSprite);
            }
            catch
            {
                // ignore
            }

            _animatedSprite = null;
        }

        if (_animatedTexture != null)
        {
            try
            {
                UnityEngine.Object.Destroy(_animatedTexture);
            }
            catch
            {
                // ignore
            }

            _animatedTexture = null;
        }

        _basePixels = null;
        _workingPixels = null;
        _nextFrameTime = 0f;
    }

    internal static void Tick()
    {
        UpdateFrame(force: false);
    }

    private static void EnsureHost()
    {
        if (_host != null)
        {
            return;
        }

        var hostObject = new GameObject("GoldenApple_EnchantedIconHost");
        UnityEngine.Object.DontDestroyOnLoad(hostObject);
        _host = hostObject.AddComponent<GoldenAppleEnchantedIconHost>();
    }

    private static void UpdateFrame(bool force)
    {
        if (_animatedTexture == null || _basePixels == null || _workingPixels == null)
        {
            return;
        }

        var now = Time.unscaledTime;
        if (!force && now < _nextFrameTime)
        {
            return;
        }

        _nextFrameTime = now + 1f / FramesPerSecond;

        var width = _animatedTexture.width;
        var height = _animatedTexture.height;

        for (var y = 0; y < height; y++)
        {
            var normalizedY = height <= 1 ? 0f : y / (float)(height - 1);
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                var source = _basePixels[index];
                if (source.a <= 0)
                {
                    _workingPixels[index] = source;
                    continue;
                }

                var sourceBrightness = (source.r + source.g + source.b) / 765f;
                if (sourceBrightness < 0.16f)
                {
                    _workingPixels[index] = source;
                    continue;
                }

                var normalizedX = width <= 1 ? 0f : x / (float)(width - 1);
                var stripe = Mathf.Repeat(normalizedY - now * ScrollSpeed + normalizedX * StripeSkew, 1f);
                var band = EvaluateStripe(stripe);
                var secondaryBand = EvaluateStripe(Mathf.Repeat(stripe + 0.42f, 1f)) * 0.55f;
                var intensity = Mathf.Clamp01((band + secondaryBand) * sourceBrightness);
                _workingPixels[index] = Blend(source, intensity);
            }
        }

        _animatedTexture.SetPixels32(_workingPixels);
        _animatedTexture.Apply(false, false);
    }

    private static float EvaluateStripe(float value)
    {
        var fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, StripeFeather, value));
        var fadeOut = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(StripeWidth - StripeFeather, StripeWidth, value));
        return Mathf.Clamp01(fadeIn * fadeOut);
    }

    private static Color32 Blend(Color32 source, float intensity)
    {
        if (intensity <= 0f)
        {
            return source;
        }

        var sourceColor = (Color)source;
        var tinted = Color.Lerp(sourceColor, EnchantTint, BlendStrength * intensity);
        tinted.r = Mathf.Clamp01(tinted.r + intensity * 0.1f);
        tinted.g = Mathf.Clamp01(tinted.g + intensity * 0.03f);
        tinted.b = Mathf.Clamp01(tinted.b + intensity * 0.12f);
        tinted.a = sourceColor.a;
        return (Color32)tinted;
    }
}