using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class AncestorFilter
{
	private CountingBloomFilter m_CountingBloomFilter;

	private Stack<int> m_HashStack = new Stack<int>(100);

	private void AddHash(int hash)
	{
		m_HashStack.Push(hash);
		m_CountingBloomFilter.InsertHash((uint)hash);
	}

	public unsafe bool IsCandidate(StyleComplexSelector complexSel)
	{
		for (int i = 0; i < 4; i++)
		{
			if (complexSel.ancestorHashes.hashes[i] == 0)
			{
				return true;
			}
			if (!m_CountingBloomFilter.ContainsHash((uint)complexSel.ancestorHashes.hashes[i]))
			{
				return false;
			}
		}
		return true;
	}

	public void PushElement(VisualElement element)
	{
		int count = m_HashStack.Count;
		AddHash(element.typeName.GetHashCode() * 13);
		if (!string.IsNullOrEmpty(element.name))
		{
			AddHash(element.name.GetHashCode() * 17);
		}
		foreach (string @class in element.classList)
		{
			AddHash(@class.GetHashCode() * 19);
		}
		m_HashStack.Push(m_HashStack.Count - count);
	}

	public void PopElement()
	{
		int num = m_HashStack.Peek();
		m_HashStack.Pop();
		while (num > 0)
		{
			int hash = m_HashStack.Peek();
			m_CountingBloomFilter.RemoveHash((uint)hash);
			m_HashStack.Pop();
			num--;
		}
	}
}
