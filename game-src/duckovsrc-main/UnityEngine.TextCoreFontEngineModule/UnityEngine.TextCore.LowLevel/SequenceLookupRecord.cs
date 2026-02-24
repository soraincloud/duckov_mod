using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.TextCore.LowLevel;

[Serializable]
[UsedByNativeCode]
internal struct SequenceLookupRecord
{
	[NativeName("glyphSequenceIndex")]
	[SerializeField]
	private uint m_GlyphSequenceIndex;

	[SerializeField]
	[NativeName("lookupListIndex")]
	private uint m_LookupListIndex;

	public uint glyphSequenceIndex
	{
		get
		{
			return m_GlyphSequenceIndex;
		}
		set
		{
			m_GlyphSequenceIndex = value;
		}
	}

	public uint lookupListIndex
	{
		get
		{
			return m_LookupListIndex;
		}
		set
		{
			m_LookupListIndex = value;
		}
	}
}
