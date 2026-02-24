using System;
using System.Collections.Generic;
using Unity.Properties.Internal;

namespace Unity.Properties;

public abstract class Property<TContainer, TValue> : IProperty<TContainer>, IProperty, IPropertyAccept<TContainer>, IAttributes
{
	private List<Attribute> m_Attributes;

	List<Attribute> IAttributes.Attributes
	{
		get
		{
			return m_Attributes;
		}
		set
		{
			m_Attributes = value;
		}
	}

	public abstract string Name { get; }

	public abstract bool IsReadOnly { get; }

	public Type DeclaredValueType()
	{
		return typeof(TValue);
	}

	public void Accept(IPropertyVisitor visitor, ref TContainer container)
	{
		visitor.Visit(this, ref container);
	}

	object IProperty<TContainer>.GetValue(ref TContainer container)
	{
		return GetValue(ref container);
	}

	void IProperty<TContainer>.SetValue(ref TContainer container, object value)
	{
		SetValue(ref container, TypeConversion.Convert<object, TValue>(ref value));
	}

	public abstract TValue GetValue(ref TContainer container);

	public abstract void SetValue(ref TContainer container, TValue value);

	protected void AddAttribute(Attribute attribute)
	{
		((IAttributes)this).AddAttribute(attribute);
	}

	protected void AddAttributes(IEnumerable<Attribute> attributes)
	{
		((IAttributes)this).AddAttributes(attributes);
	}

	void IAttributes.AddAttribute(Attribute attribute)
	{
		if (attribute != null && !(attribute.GetType() == typeof(CreatePropertyAttribute)))
		{
			if (m_Attributes == null)
			{
				m_Attributes = new List<Attribute>();
			}
			m_Attributes.Add(attribute);
		}
	}

	void IAttributes.AddAttributes(IEnumerable<Attribute> attributes)
	{
		if (m_Attributes == null)
		{
			m_Attributes = new List<Attribute>();
		}
		foreach (Attribute attribute in attributes)
		{
			if (attribute != null && !(attribute.GetType() == typeof(CreatePropertyAttribute)))
			{
				m_Attributes.Add(attribute);
			}
		}
	}

	public bool HasAttribute<TAttribute>() where TAttribute : Attribute
	{
		for (int i = 0; i < m_Attributes?.Count; i++)
		{
			if (m_Attributes[i] is TAttribute)
			{
				return true;
			}
		}
		return false;
	}

	public TAttribute GetAttribute<TAttribute>() where TAttribute : Attribute
	{
		for (int i = 0; i < m_Attributes?.Count; i++)
		{
			if (m_Attributes[i] is TAttribute result)
			{
				return result;
			}
		}
		return null;
	}

	public IEnumerable<TAttribute> GetAttributes<TAttribute>() where TAttribute : Attribute
	{
		for (int i = 0; i < m_Attributes?.Count; i++)
		{
			Attribute attribute = m_Attributes[i];
			if (attribute is TAttribute typed)
			{
				yield return typed;
			}
		}
	}

	public IEnumerable<Attribute> GetAttributes()
	{
		for (int i = 0; i < m_Attributes?.Count; i++)
		{
			yield return m_Attributes[i];
		}
	}

	AttributesScope IAttributes.CreateAttributesScope(IAttributes attributes)
	{
		return new AttributesScope(this, attributes?.Attributes);
	}
}
