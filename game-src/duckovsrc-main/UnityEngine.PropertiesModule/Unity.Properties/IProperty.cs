using System;
using System.Collections.Generic;

namespace Unity.Properties;

public interface IProperty
{
	string Name { get; }

	bool IsReadOnly { get; }

	Type DeclaredValueType();

	bool HasAttribute<TAttribute>() where TAttribute : Attribute;

	TAttribute GetAttribute<TAttribute>() where TAttribute : Attribute;

	IEnumerable<TAttribute> GetAttributes<TAttribute>() where TAttribute : Attribute;

	IEnumerable<Attribute> GetAttributes();
}
public interface IProperty<TContainer> : IProperty, IPropertyAccept<TContainer>
{
	object GetValue(ref TContainer container);

	void SetValue(ref TContainer container, object value);
}
