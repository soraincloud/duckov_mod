using System;
using System.Diagnostics;
using UnityEngine.Bindings;
using UnityEngine.Internal;
using UnityEngine.Scripting;

namespace UnityEngine.TextCore.LowLevel;

[Serializable]
[UsedByNativeCode]
[DebuggerDisplay("First glyphIndex = {m_FirstAdjustmentRecord.m_GlyphIndex},  Second glyphIndex = {m_SecondAdjustmentRecord.m_GlyphIndex}")]
public struct GlyphPairAdjustmentRecord : IEquatable<GlyphPairAdjustmentRecord>
{
	[SerializeField]
	[NativeName("firstAdjustmentRecord")]
	private GlyphAdjustmentRecord m_FirstAdjustmentRecord;

	[SerializeField]
	[NativeName("secondAdjustmentRecord")]
	private GlyphAdjustmentRecord m_SecondAdjustmentRecord;

	[SerializeField]
	private FontFeatureLookupFlags m_FeatureLookupFlags;

	public GlyphAdjustmentRecord firstAdjustmentRecord
	{
		get
		{
			return m_FirstAdjustmentRecord;
		}
		set
		{
			m_FirstAdjustmentRecord = value;
		}
	}

	public GlyphAdjustmentRecord secondAdjustmentRecord
	{
		get
		{
			return m_SecondAdjustmentRecord;
		}
		set
		{
			m_SecondAdjustmentRecord = value;
		}
	}

	public FontFeatureLookupFlags featureLookupFlags
	{
		get
		{
			return m_FeatureLookupFlags;
		}
		set
		{
			m_FeatureLookupFlags = value;
		}
	}

	public GlyphPairAdjustmentRecord(GlyphAdjustmentRecord firstAdjustmentRecord, GlyphAdjustmentRecord secondAdjustmentRecord)
	{
		m_FirstAdjustmentRecord = firstAdjustmentRecord;
		m_SecondAdjustmentRecord = secondAdjustmentRecord;
		m_FeatureLookupFlags = FontFeatureLookupFlags.None;
	}

	[ExcludeFromDocs]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	[ExcludeFromDocs]
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	[ExcludeFromDocs]
	public bool Equals(GlyphPairAdjustmentRecord other)
	{
		return base.Equals((object)other);
	}

	[ExcludeFromDocs]
	public static bool operator ==(GlyphPairAdjustmentRecord lhs, GlyphPairAdjustmentRecord rhs)
	{
		return lhs.m_FirstAdjustmentRecord == rhs.m_FirstAdjustmentRecord && lhs.m_SecondAdjustmentRecord == rhs.m_SecondAdjustmentRecord;
	}

	[ExcludeFromDocs]
	public static bool operator !=(GlyphPairAdjustmentRecord lhs, GlyphPairAdjustmentRecord rhs)
	{
		return !(lhs == rhs);
	}
}
