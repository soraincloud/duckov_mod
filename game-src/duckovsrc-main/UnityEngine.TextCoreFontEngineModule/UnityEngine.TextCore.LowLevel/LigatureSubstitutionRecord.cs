using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.TextCore.LowLevel;

[Serializable]
[UsedByNativeCode]
internal struct LigatureSubstitutionRecord
{
	[SerializeField]
	[NativeName("componentGlyphs")]
	private uint[] m_ComponentGlyphIDs;

	[SerializeField]
	[NativeName("ligatureGlyph")]
	private uint m_LigatureGlyphID;

	public uint[] componentGlyphIDs
	{
		get
		{
			return m_ComponentGlyphIDs;
		}
		set
		{
			m_ComponentGlyphIDs = value;
		}
	}

	public uint ligatureGlyphID
	{
		get
		{
			return m_LigatureGlyphID;
		}
		set
		{
			m_LigatureGlyphID = value;
		}
	}
}
