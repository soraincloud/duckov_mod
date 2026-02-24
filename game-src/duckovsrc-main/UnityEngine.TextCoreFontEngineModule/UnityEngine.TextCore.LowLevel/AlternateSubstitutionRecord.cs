using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.TextCore.LowLevel;

[Serializable]
[UsedByNativeCode]
internal struct AlternateSubstitutionRecord
{
	[SerializeField]
	[NativeName("targetGlyphID")]
	private uint m_TargetGlyphID;

	[SerializeField]
	[NativeName("substituteGlyphIDs")]
	private uint[] m_SubstituteGlyphIDs;

	public uint targetGlyphID
	{
		get
		{
			return m_TargetGlyphID;
		}
		set
		{
			m_TargetGlyphID = value;
		}
	}

	public uint[] substituteGlyphIDs
	{
		get
		{
			return m_SubstituteGlyphIDs;
		}
		set
		{
			m_SubstituteGlyphIDs = value;
		}
	}
}
