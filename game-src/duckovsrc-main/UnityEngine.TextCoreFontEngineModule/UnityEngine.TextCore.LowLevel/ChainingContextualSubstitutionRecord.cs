using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.TextCore.LowLevel;

[Serializable]
[UsedByNativeCode]
internal struct ChainingContextualSubstitutionRecord
{
	[SerializeField]
	[NativeName("backtrackGlyphSequences")]
	private GlyphIDSequence[] m_BacktrackGlyphSequences;

	[SerializeField]
	[NativeName("inputGlyphSequences")]
	private GlyphIDSequence[] m_InputGlyphSequences;

	[NativeName("lookaheadGlyphSequences")]
	[SerializeField]
	private GlyphIDSequence[] m_LookaheadGlyphSequences;

	[SerializeField]
	[NativeName("sequenceLookupRecords")]
	private SequenceLookupRecord[] m_SequenceLookupRecords;

	public GlyphIDSequence[] backtrackGlyphSequences
	{
		get
		{
			return m_BacktrackGlyphSequences;
		}
		set
		{
			m_BacktrackGlyphSequences = value;
		}
	}

	public GlyphIDSequence[] inputGlyphSequences
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

	public GlyphIDSequence[] lookaheadGlyphSequences
	{
		get
		{
			return m_LookaheadGlyphSequences;
		}
		set
		{
			m_LookaheadGlyphSequences = value;
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
