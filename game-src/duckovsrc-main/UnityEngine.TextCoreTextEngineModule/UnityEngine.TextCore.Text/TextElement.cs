using System;

namespace UnityEngine.TextCore.Text;

[Serializable]
public abstract class TextElement
{
	[SerializeField]
	protected TextElementType m_ElementType;

	[SerializeField]
	internal uint m_Unicode;

	internal TextAsset m_TextAsset;

	internal Glyph m_Glyph;

	[SerializeField]
	internal uint m_GlyphIndex;

	[SerializeField]
	internal float m_Scale;

	public TextElementType elementType => m_ElementType;

	public uint unicode
	{
		get
		{
			return m_Unicode;
		}
		set
		{
			m_Unicode = value;
		}
	}

	public TextAsset textAsset
	{
		get
		{
			return m_TextAsset;
		}
		set
		{
			m_TextAsset = value;
		}
	}

	public Glyph glyph
	{
		get
		{
			return m_Glyph;
		}
		set
		{
			m_Glyph = value;
		}
	}

	public uint glyphIndex
	{
		get
		{
			return m_GlyphIndex;
		}
		set
		{
			m_GlyphIndex = value;
		}
	}

	public float scale
	{
		get
		{
			return m_Scale;
		}
		set
		{
			m_Scale = value;
		}
	}
}
