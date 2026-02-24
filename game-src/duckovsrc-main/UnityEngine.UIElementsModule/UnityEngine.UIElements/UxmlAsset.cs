using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

[Serializable]
internal class UxmlAsset : IUxmlAttributes
{
	[SerializeField]
	private string m_FullTypeName;

	[SerializeField]
	private int m_Id;

	[SerializeField]
	private int m_OrderInDocument;

	[SerializeField]
	private int m_ParentId;

	[SerializeField]
	protected List<string> m_Properties;

	public string fullTypeName
	{
		get
		{
			return m_FullTypeName;
		}
		set
		{
			m_FullTypeName = value;
		}
	}

	public int id
	{
		get
		{
			return m_Id;
		}
		set
		{
			m_Id = value;
		}
	}

	public int orderInDocument
	{
		get
		{
			return m_OrderInDocument;
		}
		set
		{
			m_OrderInDocument = value;
		}
	}

	public int parentId
	{
		get
		{
			return m_ParentId;
		}
		set
		{
			m_ParentId = value;
		}
	}

	public UxmlAsset(string fullTypeName)
	{
		m_FullTypeName = fullTypeName;
	}

	public List<string> GetProperties()
	{
		return m_Properties;
	}

	public bool HasParent()
	{
		return m_ParentId != 0;
	}

	public bool HasAttribute(string attributeName)
	{
		if (m_Properties == null || m_Properties.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < m_Properties.Count; i += 2)
		{
			string text = m_Properties[i];
			if (text == attributeName)
			{
				return true;
			}
		}
		return false;
	}

	public string GetAttributeValue(string attributeName)
	{
		TryGetAttributeValue(attributeName, out var value);
		return value;
	}

	public bool TryGetAttributeValue(string propertyName, out string value)
	{
		if (m_Properties == null)
		{
			value = null;
			return false;
		}
		for (int i = 0; i < m_Properties.Count - 1; i += 2)
		{
			if (m_Properties[i] == propertyName)
			{
				value = m_Properties[i + 1];
				return true;
			}
		}
		value = null;
		return false;
	}

	public void SetAttribute(string name, string value)
	{
		SetOrAddProperty(name, value);
	}

	public void RemoveAttribute(string attributeName)
	{
		if (m_Properties == null || m_Properties.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < m_Properties.Count; i += 2)
		{
			string text = m_Properties[i];
			if (!(text != attributeName))
			{
				m_Properties.RemoveAt(i);
				m_Properties.RemoveAt(i);
				break;
			}
		}
	}

	private void SetOrAddProperty(string propertyName, string propertyValue)
	{
		if (m_Properties == null)
		{
			m_Properties = new List<string>();
		}
		for (int i = 0; i < m_Properties.Count - 1; i += 2)
		{
			if (m_Properties[i] == propertyName)
			{
				m_Properties[i + 1] = propertyValue;
				return;
			}
		}
		m_Properties.Add(propertyName);
		m_Properties.Add(propertyValue);
	}
}
