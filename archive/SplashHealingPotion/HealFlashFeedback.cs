using UnityEngine;
using UnityEngine.UI;

namespace SplashHealingPotion;

internal sealed class HealFlashFeedback : MonoBehaviour
{
    private const float Duration = 0.24f;
    private const float SegmentLength = 14f;
    private const float Thickness = 4f;
    private const float SegmentOffset = 20f;

    private static HealFlashFeedback? _instance;
    private static Sprite? _segmentSprite;

    private Canvas? _canvas;
    private RectTransform? _flashRoot;
    private CanvasGroup? _canvasGroup;
    private readonly Image?[] _mainSegments = new Image[4];
    private float _timer;

    internal static void EnsureExists()
    {
        if (_instance != null)
        {
            return;
        }

        var go = new GameObject("SplashHealingPotion_HealFlashFeedback");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<HealFlashFeedback>();
    }

    internal static void Trigger()
    {
        EnsureExists();
        _instance?.ShowFlash();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureUi();
    }

    private void Update()
    {
        if (_timer <= 0f)
        {
            if (_canvasGroup != null && _canvasGroup.alpha > 0f)
            {
                _canvasGroup.alpha = 0f;
            }

            return;
        }

        EnsureUi();

        _timer = Mathf.Max(0f, _timer - Time.unscaledDeltaTime);

        if (_flashRoot == null || _canvasGroup == null)
        {
            return;
        }

        _flashRoot.position = GetAimScreenPoint();
        _canvasGroup.alpha = 1f;
    }

    private void ShowFlash()
    {
        EnsureUi();

        if (_flashRoot == null || _canvasGroup == null)
        {
            return;
        }

        _timer = Duration;
        _flashRoot.position = GetAimScreenPoint();
        _flashRoot.localScale = Vector3.one;
        _canvasGroup.alpha = 1f;
        UpdateColors(1f);
    }

    private void EnsureUi()
    {
        if (_canvas != null && _flashRoot != null && _canvasGroup != null)
        {
            return;
        }

        _canvas = GetComponent<Canvas>();
        if (_canvas == null)
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 32760;
            gameObject.AddComponent<GraphicRaycaster>();
        }

        if (GetComponent<CanvasScaler>() == null)
        {
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (_flashRoot == null)
        {
            var root = new GameObject("FlashRoot", typeof(RectTransform), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            _flashRoot = (RectTransform)root.transform;
            _flashRoot.anchorMin = new Vector2(0.5f, 0.5f);
            _flashRoot.anchorMax = new Vector2(0.5f, 0.5f);
            _flashRoot.pivot = new Vector2(0.5f, 0.5f);
            _flashRoot.sizeDelta = new Vector2(92f, 92f);
            _canvasGroup = root.GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;

            CreateSegments("Main", _flashRoot, SegmentLength, Thickness, _mainSegments);
        }
    }

    private static void CreateSegments(string prefix, RectTransform parent, float width, float height, Image?[] target)
    {
        var directions = new[]
        {
            new Vector2(1f, 1f),
            new Vector2(-1f, -1f),
            new Vector2(-1f, 1f),
            new Vector2(1f, -1f)
        };

        for (var i = 0; i < directions.Length; i++)
        {
            var dir = directions[i].normalized;
            var rotation = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            target[i] = CreateSlash($"{prefix}Slash{i}", parent, width, height, rotation, dir * SegmentOffset);
        }
    }

    private static Image CreateSlash(string name, Transform parent, float width, float height, float rotation, Vector2 anchoredPosition)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var rect = (RectTransform)go.transform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = anchoredPosition;
        rect.localRotation = Quaternion.Euler(0f, 0f, rotation);

        var image = go.GetComponent<Image>();
        image.sprite = GetSegmentSprite();
        image.type = Image.Type.Simple;
        image.raycastTarget = false;
        image.color = Color.white;
        return image;
    }

    private void UpdateColors(float alpha)
    {
        var coreColor = new Color(0.62f, 1f, 0.72f, alpha);

        SetImageColors(_mainSegments, coreColor);
    }

    private static void SetImageColors(Image?[] images, Color color)
    {
        for (var i = 0; i < images.Length; i++)
        {
            if (images[i] != null)
            {
                images[i]!.color = color;
            }
        }
    }

    private static Vector3 GetAimScreenPoint()
    {
        var inputManager = LevelManager.Instance?.InputManager;
        if (inputManager != null)
        {
            var point = inputManager.AimScreenPoint;
            if (point.sqrMagnitude > 0f)
            {
                return point;
            }
        }

        return new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
    }

    private static Sprite GetSegmentSprite()
    {
        if (_segmentSprite != null)
        {
            return _segmentSprite;
        }

        const int width = 32;
        const int height = 8;
        var radius = height / 2f - 0.5f;

        var texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false)
        {
            name = "SplashHealingPotion_HealFlashSegment",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        var clear = new Color(1f, 1f, 1f, 0f);
        var fill = Color.white;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var pixelCenter = new Vector2(x + 0.5f, y + 0.5f);
                var leftCenter = new Vector2(radius, height * 0.5f);
                var rightCenter = new Vector2(width - radius, height * 0.5f);

                var insideCenterRect = pixelCenter.x >= leftCenter.x && pixelCenter.x <= rightCenter.x;
                var insideLeftCap = Vector2.Distance(pixelCenter, leftCenter) <= radius;
                var insideRightCap = Vector2.Distance(pixelCenter, rightCenter) <= radius;

                texture.SetPixel(x, y, insideCenterRect || insideLeftCap || insideRightCap ? fill : clear);
            }
        }

        texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);

        _segmentSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
        return _segmentSprite;
    }
}