using UnityEngine.TextCore.Text;
using UnityEngine.UIElements.UIR;

namespace UnityEngine.UIElements;

internal static class TextUtilities
{
	internal static Vector2 MeasureVisualElementTextSize(TextElement te, string textToMeasure, float width, VisualElement.MeasureMode widthMode, float height, VisualElement.MeasureMode heightMode)
	{
		float x = float.NaN;
		float y = float.NaN;
		if (textToMeasure == null || !IsFontAssigned(te))
		{
			return new Vector2(x, y);
		}
		float scaledPixelsPerPoint = te.scaledPixelsPerPoint;
		if (scaledPixelsPerPoint <= 0f)
		{
			return Vector2.zero;
		}
		if (widthMode == VisualElement.MeasureMode.Exactly)
		{
			x = width;
		}
		else
		{
			x = te.uitkTextHandle.ComputeTextWidth(textToMeasure, wordWrap: false, width, height);
			if (widthMode == VisualElement.MeasureMode.AtMost)
			{
				x = Mathf.Min(x, width);
			}
		}
		if (heightMode == VisualElement.MeasureMode.Exactly)
		{
			y = height;
		}
		else
		{
			y = te.uitkTextHandle.ComputeTextHeight(textToMeasure, width, height);
			if (heightMode == VisualElement.MeasureMode.AtMost)
			{
				y = Mathf.Min(y, height);
			}
		}
		float x2 = AlignmentUtils.CeilToPixelGrid(x, scaledPixelsPerPoint, 0f);
		float y2 = AlignmentUtils.CeilToPixelGrid(y, scaledPixelsPerPoint, 0f);
		Vector2 vector = new Vector2(x2, y2);
		te.uitkTextHandle.MeasuredSizes = new Vector2(x, y);
		te.uitkTextHandle.RoundedSizes = vector;
		return vector;
	}

	internal static FontAsset GetFontAsset(VisualElement ve)
	{
		if (ve.computedStyle.unityFontDefinition.fontAsset != null)
		{
			return ve.computedStyle.unityFontDefinition.fontAsset;
		}
		PanelTextSettings textSettingsFrom = GetTextSettingsFrom(ve);
		if (ve.computedStyle.unityFontDefinition.font != null)
		{
			return textSettingsFrom.GetCachedFontAsset(ve.computedStyle.unityFontDefinition.font);
		}
		if (ve.computedStyle.unityFont != null)
		{
			return textSettingsFrom.GetCachedFontAsset(ve.computedStyle.unityFont);
		}
		return null;
	}

	internal static Font GetFont(VisualElement ve)
	{
		ComputedStyle computedStyle = ve.computedStyle;
		if (computedStyle.unityFontDefinition.font != null)
		{
			return computedStyle.unityFontDefinition.font;
		}
		if (computedStyle.unityFont != null)
		{
			return computedStyle.unityFont;
		}
		return computedStyle.unityFontDefinition.fontAsset?.sourceFontFile;
	}

	internal static bool IsFontAssigned(VisualElement ve)
	{
		return ve.computedStyle.unityFont != null || !ve.computedStyle.unityFontDefinition.IsEmpty();
	}

	internal static PanelTextSettings GetTextSettingsFrom(VisualElement ve)
	{
		if (ve.panel is RuntimePanel runtimePanel)
		{
			return runtimePanel.panelSettings.textSettings ?? PanelTextSettings.defaultPanelTextSettings;
		}
		return PanelTextSettings.defaultPanelTextSettings;
	}

	internal static float ConvertPixelUnitsToTextCoreRelativeUnits(VisualElement ve, FontAsset fontAsset)
	{
		float num = 1f / (float)fontAsset.atlasPadding;
		float num2 = (float)fontAsset.faceInfo.pointSize / ve.computedStyle.fontSize.value;
		return num * num2;
	}

	internal static TextCoreSettings GetTextCoreSettingsForElement(VisualElement ve)
	{
		FontAsset fontAsset = GetFontAsset(ve);
		if (fontAsset == null)
		{
			return default(TextCoreSettings);
		}
		IResolvedStyle resolvedStyle = ve.resolvedStyle;
		ComputedStyle computedStyle = ve.computedStyle;
		float num = ConvertPixelUnitsToTextCoreRelativeUnits(ve, fontAsset);
		float num2 = Mathf.Clamp(resolvedStyle.unityTextOutlineWidth * num, 0f, 1f);
		float underlaySoftness = Mathf.Clamp(computedStyle.textShadow.blurRadius * num, 0f, 1f);
		float x = ((computedStyle.textShadow.offset.x < 0f) ? Mathf.Max(computedStyle.textShadow.offset.x * num, -1f) : Mathf.Min(computedStyle.textShadow.offset.x * num, 1f));
		float y = ((computedStyle.textShadow.offset.y < 0f) ? Mathf.Max(computedStyle.textShadow.offset.y * num, -1f) : Mathf.Min(computedStyle.textShadow.offset.y * num, 1f));
		Vector2 underlayOffset = new Vector2(x, y);
		Color color = resolvedStyle.color;
		Color unityTextOutlineColor = resolvedStyle.unityTextOutlineColor;
		if (num2 < 1E-30f)
		{
			unityTextOutlineColor.a = 0f;
		}
		return new TextCoreSettings
		{
			faceColor = color,
			outlineColor = unityTextOutlineColor,
			outlineWidth = num2,
			underlayColor = computedStyle.textShadow.color,
			underlayOffset = underlayOffset,
			underlaySoftness = underlaySoftness
		};
	}
}
