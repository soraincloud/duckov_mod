using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.TextCore.LowLevel;

[Serializable]
[UsedByNativeCode]
internal struct MarkToMarkAdjustmentRecord
{
	[SerializeField]
	[NativeName("baseMarkGlyphID")]
	private uint m_BaseMarkGlyphID;

	[SerializeField]
	[NativeName("baseMarkAnchor")]
	private GlyphAnchorPoint m_BaseMarkGlyphAnchorPoint;

	[NativeName("combiningMarkGlyphID")]
	[SerializeField]
	private uint m_CombiningMarkGlyphID;

	[NativeName("combiningMarkPositionAdjustment")]
	[SerializeField]
	private MarkPositionAdjustment m_CombiningMarkPositionAdjustment;

	public uint baseMarkGlyphID
	{
		get
		{
			return m_BaseMarkGlyphID;
		}
		set
		{
			m_BaseMarkGlyphID = value;
		}
	}

	public GlyphAnchorPoint baseMarkGlyphAnchorPoint
	{
		get
		{
			return m_BaseMarkGlyphAnchorPoint;
		}
		set
		{
			m_BaseMarkGlyphAnchorPoint = value;
		}
	}

	public uint combiningMarkGlyphID
	{
		get
		{
			return m_CombiningMarkGlyphID;
		}
		set
		{
			m_CombiningMarkGlyphID = value;
		}
	}

	public MarkPositionAdjustment combiningMarkPositionAdjustment
	{
		get
		{
			return m_CombiningMarkPositionAdjustment;
		}
		set
		{
			m_CombiningMarkPositionAdjustment = value;
		}
	}
}
