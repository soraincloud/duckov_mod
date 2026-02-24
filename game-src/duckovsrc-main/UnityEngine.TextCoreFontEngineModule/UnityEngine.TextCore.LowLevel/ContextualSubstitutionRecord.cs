using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.TextCore.LowLevel;

[Serializable]
[UsedByNativeCode]
internal struct ContextualSubstitutionRecord
{
	[SerializeField]
	[NativeName("inputGlyphSequences")]
	private GlyphIDSequence[] m_InputGlyphSequences;

	[SerializeField]
	[NativeName("sequenceLookupRecords")]
	private SequenceLookupRecord[] m_SequenceLookupRecords;

	public GlyphIDSequence[] inputSequences
	{
		get
		{
			return m_InputGlyphSequences;
		}
		set
		{
			m_InputGlyphSequences = value;
		}
	}

	public SequenceLookupRecord[] sequenceLookupRecords
	{
		get
		{
			return m_SequenceLookupRecords;
		}
		set
		{
			m_SequenceLookupRecords = value;
		}
	}
}
