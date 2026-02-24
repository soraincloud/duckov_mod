using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.TextCore.LowLevel;

[Serializable]
[UsedByNativeCode]
internal struct GlyphIDSequence
{
	[SerializeField]
	[NativeName("glyphIDs")]
	private uint[] m_GlyphIDs;

	public uint[] glyphIDs
	{
		get
		{
			return m_GlyphIDs;
		}
		set
		{
			m_GlyphIDs = value;
		}
	}
}
