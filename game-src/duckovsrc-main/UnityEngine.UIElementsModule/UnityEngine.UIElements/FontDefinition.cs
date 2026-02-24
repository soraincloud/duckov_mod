using System;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;

namespace UnityEngine.UIElements;

public struct FontDefinition : IEquatable<FontDefinition>
{
	private Font m_Font;

	private FontAsset m_FontAsset;

	public Font font
	{
		get
		{
			return m_Font;
		}
		set
		{
			if (value != null && fontAsset != null)
			{
				throw new InvalidOperationException("Cannot set both Font and FontAsset on FontDefinition");
			}
			m_Font = value;
		}
	}

	public FontAsset fontAsset
	{
		get
		{
			return m_FontAsset;
		}
		set
		{
			if (value != null && font != null)
			{
				throw new InvalidOperationException("Cannot set both Font and FontAsset on FontDefinition");
			}
			m_FontAsset = value;
		}
	}

	internal static IEnumerable<Type> allowedAssetTypes
	{
		get
		{
			yield return typeof(Font);
			yield return typeof(FontAsset);
		}
	}

	public static FontDefinition FromFont(Font f)
	{
		return new FontDefinition
		{
			m_Font = f
		};
	}

	public static FontDefinition FromSDFFont(FontAsset f)
	{
		return new FontDefinition
		{
			m_FontAsset = f
		};
	}

	internal static FontDefinition FromObject(object obj)
	{
		Font font = obj as Font;
		if (font != null)
		{
			return FromFont(font);
		}
		FontAsset fontAsset = obj as FontAsset;
		if (fontAsset != null)
		{
			return FromSDFFont(fontAsset);
		}
		return default(FontDefinition);
	}

	internal bool IsEmpty()
	{
		return m_Font == null && m_FontAsset == null;
	}

	public override string ToString()
	{
		if (font != null)
		{
			return $"{font}";
		}
		return $"{fontAsset}";
	}

	public bool Equals(FontDefinition other)
	{
		return object.Equals(m_Font, other.m_Font) && object.Equals(m_FontAsset, other.m_FontAsset);
	}

	public override bool Equals(object obj)
	{
		return obj is FontDefinition other && Equals(other);
	}

	public override int GetHashCode()
	{
		return (((m_Font != null) ? m_Font.GetHashCode() : 0) * 397) ^ ((m_FontAsset != null) ? m_FontAsset.GetHashCode() : 0);
	}

	public static bool operator ==(FontDefinition left, FontDefinition right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(FontDefinition left, FontDefinition right)
	{
		return !left.Equals(right);
	}
}
