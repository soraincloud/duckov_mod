using System;

namespace UnityEngine.UIElements;

internal struct InheritedData : IStyleDataGroup<InheritedData>, IEquatable<InheritedData>
{
	public Color color;

	public Length fontSize;

	public Length letterSpacing;

	public TextShadow textShadow;

	public Font unityFont;

	public FontDefinition unityFontDefinition;

	public FontStyle unityFontStyleAndWeight;

	public Length unityParagraphSpacing;

	public TextAnchor unityTextAlign;

	public Color unityTextOutlineColor;

	public float unityTextOutlineWidth;

	public Visibility visibility;

	public WhiteSpace whiteSpace;

	public Length wordSpacing;

	public InheritedData Copy()
	{
		return this;
	}

	public void CopyFrom(ref InheritedData other)
	{
		this = other;
	}

	public static bool operator ==(InheritedData lhs, InheritedData rhs)
	{
		return lhs.color == rhs.color && lhs.fontSize == rhs.fontSize && lhs.letterSpacing == rhs.letterSpacing && lhs.textShadow == rhs.textShadow && lhs.unityFont == rhs.unityFont && lhs.unityFontDefinition == rhs.unityFontDefinition && lhs.unityFontStyleAndWeight == rhs.unityFontStyleAndWeight && lhs.unityParagraphSpacing == rhs.unityParagraphSpacing && lhs.unityTextAlign == rhs.unityTextAlign && lhs.unityTextOutlineColor == rhs.unityTextOutlineColor && lhs.unityTextOutlineWidth == rhs.unityTextOutlineWidth && lhs.visibility == rhs.visibility && lhs.whiteSpace == rhs.whiteSpace && lhs.wordSpacing == rhs.wordSpacing;
	}

	public static bool operator !=(InheritedData lhs, InheritedData rhs)
	{
		return !(lhs == rhs);
	}

	public bool Equals(InheritedData other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return obj is InheritedData && Equals((InheritedData)obj);
	}

	public override int GetHashCode()
	{
		int hashCode = color.GetHashCode();
		hashCode = (hashCode * 397) ^ fontSize.GetHashCode();
		hashCode = (hashCode * 397) ^ letterSpacing.GetHashCode();
		hashCode = (hashCode * 397) ^ textShadow.GetHashCode();
		hashCode = (hashCode * 397) ^ ((!(unityFont == null)) ? unityFont.GetHashCode() : 0);
		hashCode = (hashCode * 397) ^ unityFontDefinition.GetHashCode();
		hashCode = (hashCode * 397) ^ (int)unityFontStyleAndWeight;
		hashCode = (hashCode * 397) ^ unityParagraphSpacing.GetHashCode();
		hashCode = (hashCode * 397) ^ (int)unityTextAlign;
		hashCode = (hashCode * 397) ^ unityTextOutlineColor.GetHashCode();
		hashCode = (hashCode * 397) ^ unityTextOutlineWidth.GetHashCode();
		hashCode = (hashCode * 397) ^ (int)visibility;
		hashCode = (hashCode * 397) ^ (int)whiteSpace;
		return (hashCode * 397) ^ wordSpacing.GetHashCode();
	}
}
