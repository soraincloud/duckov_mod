#define UNITY_ASSERTIONS
using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class StyleVariableContext
{
	public static readonly StyleVariableContext none = new StyleVariableContext();

	private int m_VariableHash;

	private List<StyleVariable> m_Variables;

	private List<int> m_SortedHash;

	private List<int> m_UnsortedHash;

	public List<StyleVariable> variables => m_Variables;

	public void Add(StyleVariable sv)
	{
		int hash = sv.GetHashCode();
		int num = m_SortedHash.BinarySearch(hash);
		if (num >= 0)
		{
			int num2 = m_Variables.Count - 1;
			if (m_UnsortedHash[num2] == hash)
			{
				return;
			}
			for (num2--; num2 >= 0; num2--)
			{
				if (m_UnsortedHash[num2] == hash)
				{
					m_VariableHash ^= ComputeOrderSensitiveHash(num2);
					m_Variables.RemoveAt(num2);
					m_UnsortedHash.RemoveAt(num2);
					break;
				}
			}
		}
		else
		{
			m_SortedHash.Insert(~num, hash);
		}
		m_VariableHash ^= ComputeOrderSensitiveHash(m_Variables.Count);
		m_Variables.Add(sv);
		m_UnsortedHash.Add(hash);
		int ComputeOrderSensitiveHash(int index)
		{
			return (index + 1) * hash;
		}
	}

	public void AddInitialRange(StyleVariableContext other)
	{
		if (other.m_Variables.Count > 0)
		{
			Debug.Assert(m_Variables.Count == 0);
			m_VariableHash = other.m_VariableHash;
			m_Variables.AddRange(other.m_Variables);
			m_SortedHash.AddRange(other.m_SortedHash);
			m_UnsortedHash.AddRange(other.m_UnsortedHash);
		}
	}

	public void Clear()
	{
		if (m_Variables.Count > 0)
		{
			m_Variables.Clear();
			m_VariableHash = 0;
			m_SortedHash.Clear();
			m_UnsortedHash.Clear();
		}
	}

	public StyleVariableContext()
	{
		m_Variables = new List<StyleVariable>();
		m_VariableHash = 0;
		m_SortedHash = new List<int>();
		m_UnsortedHash = new List<int>();
	}

	public StyleVariableContext(StyleVariableContext other)
	{
		m_Variables = new List<StyleVariable>(other.m_Variables);
		m_VariableHash = other.m_VariableHash;
		m_SortedHash = new List<int>(other.m_SortedHash);
		m_UnsortedHash = new List<int>(other.m_UnsortedHash);
	}

	public bool TryFindVariable(string name, out StyleVariable v)
	{
		for (int num = m_Variables.Count - 1; num >= 0; num--)
		{
			if (m_Variables[num].name == name)
			{
				v = m_Variables[num];
				return true;
			}
		}
		v = default(StyleVariable);
		return false;
	}

	public int GetVariableHash()
	{
		return m_VariableHash;
	}
}
