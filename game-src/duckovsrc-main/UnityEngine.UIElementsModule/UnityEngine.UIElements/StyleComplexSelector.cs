using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UIElements;

[Serializable]
internal class StyleComplexSelector : ISerializationCallbackReceiver
{
	private struct PseudoStateData
	{
		public readonly PseudoStates state;

		public readonly bool negate;

		public PseudoStateData(PseudoStates state, bool negate)
		{
			this.state = state;
			this.negate = negate;
		}
	}

	[NonSerialized]
	public Hashes ancestorHashes;

	[SerializeField]
	private int m_Specificity;

	[NonSerialized]
	private bool m_isSimple;

	[SerializeField]
	private StyleSelector[] m_Selectors;

	[SerializeField]
	internal int ruleIndex;

	[NonSerialized]
	internal StyleComplexSelector nextInTable;

	[NonSerialized]
	internal int orderInStyleSheet;

	private static Dictionary<string, PseudoStateData> s_PseudoStates;

	private static List<StyleSelectorPart> m_HashList = new List<StyleSelectorPart>();

	public int specificity
	{
		get
		{
			return m_Specificity;
		}
		internal set
		{
			m_Specificity = value;
		}
	}

	public StyleRule rule { get; internal set; }

	public bool isSimple => m_isSimple;

	public StyleSelector[] selectors
	{
		get
		{
			return m_Selectors;
		}
		internal set
		{
			m_Selectors = value;
			m_isSimple = m_Selectors.Length == 1;
		}
	}

	public void OnBeforeSerialize()
	{
	}

	public virtual void OnAfterDeserialize()
	{
		m_isSimple = m_Selectors.Length == 1;
	}

	internal void CachePseudoStateMasks()
	{
		if (s_PseudoStates == null)
		{
			s_PseudoStates = new Dictionary<string, PseudoStateData>();
			s_PseudoStates["active"] = new PseudoStateData(PseudoStates.Active, negate: false);
			s_PseudoStates["hover"] = new PseudoStateData(PseudoStates.Hover, negate: false);
			s_PseudoStates["checked"] = new PseudoStateData(PseudoStates.Checked, negate: false);
			s_PseudoStates["selected"] = new PseudoStateData(PseudoStates.Checked, negate: false);
			s_PseudoStates["disabled"] = new PseudoStateData(PseudoStates.Disabled, negate: false);
			s_PseudoStates["focus"] = new PseudoStateData(PseudoStates.Focus, negate: false);
			s_PseudoStates["root"] = new PseudoStateData(PseudoStates.Root, negate: false);
			s_PseudoStates["inactive"] = new PseudoStateData(PseudoStates.Active, negate: true);
			s_PseudoStates["enabled"] = new PseudoStateData(PseudoStates.Disabled, negate: true);
		}
		int i = 0;
		for (int num = selectors.Length; i < num; i++)
		{
			StyleSelector styleSelector = selectors[i];
			StyleSelectorPart[] parts = styleSelector.parts;
			PseudoStates pseudoStates = (PseudoStates)0;
			PseudoStates pseudoStates2 = (PseudoStates)0;
			for (int j = 0; j < styleSelector.parts.Length; j++)
			{
				if (styleSelector.parts[j].type != StyleSelectorType.PseudoClass)
				{
					continue;
				}
				if (s_PseudoStates.TryGetValue(parts[j].value, out var value))
				{
					if (!value.negate)
					{
						pseudoStates |= value.state;
					}
					else
					{
						pseudoStates2 |= value.state;
					}
				}
				else
				{
					Debug.LogWarningFormat("Unknown pseudo class \"{0}\"", parts[j].value);
				}
			}
			styleSelector.pseudoStateMask = (int)pseudoStates;
			styleSelector.negatedPseudoStateMask = (int)pseudoStates2;
		}
	}

	public override string ToString()
	{
		return string.Format("[{0}]", string.Join(", ", m_Selectors.Select((StyleSelector x) => x.ToString()).ToArray()));
	}

	private static int StyleSelectorPartCompare(StyleSelectorPart x, StyleSelectorPart y)
	{
		if (y.type < x.type)
		{
			return -1;
		}
		if (y.type > x.type)
		{
			return 1;
		}
		return y.value.CompareTo(x.value);
	}

	internal unsafe void CalculateHashes()
	{
		if (isSimple)
		{
			return;
		}
		for (int num = selectors.Length - 2; num > -1; num--)
		{
			m_HashList.AddRange(selectors[num].parts);
		}
		m_HashList.RemoveAll((StyleSelectorPart p) => p.type != StyleSelectorType.Class && p.type != StyleSelectorType.ID && p.type != StyleSelectorType.Type);
		m_HashList.Sort(StyleSelectorPartCompare);
		bool flag = true;
		StyleSelectorType styleSelectorType = StyleSelectorType.Unknown;
		string text = "";
		int num2 = 0;
		int num3 = Math.Min(4, m_HashList.Count);
		for (int num4 = 0; num4 < num3; num4++)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				for (; num2 < m_HashList.Count && m_HashList[num2].type == styleSelectorType && m_HashList[num2].value == text; num2++)
				{
				}
				if (num2 == m_HashList.Count)
				{
					break;
				}
			}
			styleSelectorType = m_HashList[num2].type;
			text = m_HashList[num2].value;
			ancestorHashes.hashes[num4] = text.GetHashCode() * styleSelectorType switch
			{
				StyleSelectorType.ID => 17, 
				StyleSelectorType.Class => 19, 
				_ => 13, 
			};
		}
		m_HashList.Clear();
	}
}
