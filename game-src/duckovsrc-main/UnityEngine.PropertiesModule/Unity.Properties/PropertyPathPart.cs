using System;
using System.Runtime.CompilerServices;

namespace Unity.Properties;

public readonly struct PropertyPathPart : IEquatable<PropertyPathPart>
{
	private readonly PropertyPathPartKind m_Kind;

	private readonly string m_Name;

	private readonly int m_Index;

	private readonly object m_Key;

	public bool IsName => Kind == PropertyPathPartKind.Name;

	public bool IsIndex => Kind == PropertyPathPartKind.Index;

	public bool IsKey => Kind == PropertyPathPartKind.Key;

	public PropertyPathPartKind Kind => m_Kind;

	public string Name
	{
		get
		{
			CheckKind(PropertyPathPartKind.Name);
			return m_Name;
		}
	}

	public int Index
	{
		get
		{
			CheckKind(PropertyPathPartKind.Index);
			return m_Index;
		}
	}

	public object Key
	{
		get
		{
			CheckKind(PropertyPathPartKind.Key);
			return m_Key;
		}
	}

	public PropertyPathPart(string name)
	{
		m_Kind = PropertyPathPartKind.Name;
		m_Name = name;
		m_Index = -1;
		m_Key = null;
	}

	public PropertyPathPart(int index)
	{
		m_Kind = PropertyPathPartKind.Index;
		m_Name = string.Empty;
		m_Index = index;
		m_Key = null;
	}

	public PropertyPathPart(object key)
	{
		m_Kind = PropertyPathPartKind.Key;
		m_Name = string.Empty;
		m_Index = -1;
		m_Key = key;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void CheckKind(PropertyPathPartKind type)
	{
		if (type != Kind)
		{
			throw new InvalidOperationException();
		}
	}

	public override string ToString()
	{
		PropertyPathPartKind kind = Kind;
		if (1 == 0)
		{
		}
		string result = kind switch
		{
			PropertyPathPartKind.Name => m_Name, 
			PropertyPathPartKind.Index => "[" + m_Index + "]", 
			PropertyPathPartKind.Key => "[\"" + m_Key?.ToString() + "\"]", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	public bool Equals(PropertyPathPart other)
	{
		return m_Kind == other.m_Kind && m_Name == other.m_Name && m_Index == other.m_Index && object.Equals(m_Key, other.m_Key);
	}

	public override bool Equals(object obj)
	{
		return obj is PropertyPathPart other && Equals(other);
	}

	public override int GetHashCode()
	{
		int kind = (int)m_Kind;
		PropertyPathPartKind kind2 = m_Kind;
		if (1 == 0)
		{
		}
		int result = kind2 switch
		{
			PropertyPathPartKind.Name => (kind * 397) ^ ((m_Name != null) ? m_Name.GetHashCode() : 0), 
			PropertyPathPartKind.Index => (kind * 397) ^ m_Index, 
			PropertyPathPartKind.Key => (kind * 397) ^ ((m_Key != null) ? m_Key.GetHashCode() : 0), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (1 == 0)
		{
		}
		return result;
	}
}
