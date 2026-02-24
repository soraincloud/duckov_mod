using System;

namespace UnityEngine.TextCore.Text;

[Serializable]
public class Character : TextElement
{
	public Character()
	{
		m_ElementType = TextElementType.Character;
		base.scale = 1f;
	}

	public Character(uint unicode, Glyph glyph)
	{
		m_ElementType = TextElementType.Character;
		base.unicode = unicode;
		base.textAsset = null;
		base.glyph = glyph;
		base.glyphIndex = glyph.index;
		base.scale = 1f;
	}

	public Character(uint unicode, FontAsset fontAsset, Glyph glyph)
	{
		m_ElementType = TextElementType.Character;
		base.unicode = unicode;
		base.textAsset = fontAsset;
		base.glyph = glyph;
		base.glyphIndex = glyph.index;
		base.scale = 1f;
	}

	internal Character(uint unicode, uint glyphIndex)
	{
		m_ElementType = TextElementType.Character;
		base.unicode = unicode;
		base.textAsset = null;
		base.glyph = null;
		base.glyphIndex = glyphIndex;
		base.scale = 1f;
	}
}
