using System;

namespace UnityEngine.UIElements.UIR;

internal struct TextCoreSettings : IEquatable<TextCoreSettings>
{
	public Color faceColor;

	public Color outlineColor;

	public float outlineWidth;

	public Color underlayColor;

	public Vector2 underlayOffset;

	public float underlaySoftness;

	public override bool Equals(object obj)
	{
		return obj is TextCoreSettings && Equals((TextCoreSettings)obj);
	}

	public bool Equals(TextCoreSettings other)
	{
		return other.faceColor == faceColor && other.outlineColor == outlineColor && other.outlineWidth == outlineWidth && other.underlayColor == underlayColor && other.underlayOffset == underlayOffset && other.underlaySoftness == underlaySoftness;
	}

	public override int GetHashCode()
	{
		int num = 75905159;
		num = num * -1521134295 + faceColor.GetHashCode();
		num = num * -1521134295 + outlineColor.GetHashCode();
		num = num * -1521134295 + outlineWidth.GetHashCode();
		num = num * -1521134295 + underlayColor.GetHashCode();
		num = num * -1521134295 + underlayOffset.x.GetHashCode();
		num = num * -1521134295 + underlayOffset.y.GetHashCode();
		return num * -1521134295 + underlaySoftness.GetHashCode();
	}
}
