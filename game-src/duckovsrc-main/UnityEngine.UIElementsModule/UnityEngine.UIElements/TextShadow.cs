using System;

namespace UnityEngine.UIElements;

public struct TextShadow : IEquatable<TextShadow>
{
	public Vector2 offset;

	public float blurRadius;

	public Color color;

	public override bool Equals(object obj)
	{
		return obj is TextShadow && Equals((TextShadow)obj);
	}

	public bool Equals(TextShadow other)
	{
		return other.offset == offset && other.blurRadius == blurRadius && other.color == color;
	}

	public override int GetHashCode()
	{
		int num = 1500536833;
		num = num * -1521134295 + offset.GetHashCode();
		num = num * -1521134295 + blurRadius.GetHashCode();
		return num * -1521134295 + color.GetHashCode();
	}

	public static bool operator ==(TextShadow style1, TextShadow style2)
	{
		return style1.Equals(style2);
	}

	public static bool operator !=(TextShadow style1, TextShadow style2)
	{
		return !(style1 == style2);
	}

	public override string ToString()
	{
		return $"offset={offset}, blurRadius={blurRadius}, color={color}";
	}

	internal static TextShadow LerpUnclamped(TextShadow a, TextShadow b, float t)
	{
		return new TextShadow
		{
			offset = Vector2.LerpUnclamped(a.offset, b.offset, t),
			blurRadius = Mathf.LerpUnclamped(a.blurRadius, b.blurRadius, t),
			color = Color.LerpUnclamped(a.color, b.color, t)
		};
	}
}
