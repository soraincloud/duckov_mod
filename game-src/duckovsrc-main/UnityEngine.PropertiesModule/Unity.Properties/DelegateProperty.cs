using System;

namespace Unity.Properties;

public class DelegateProperty<TContainer, TValue> : Property<TContainer, TValue>
{
	private readonly PropertyGetter<TContainer, TValue> m_Getter;

	private readonly PropertySetter<TContainer, TValue> m_Setter;

	public override string Name { get; }

	public override bool IsReadOnly => m_Setter == null;

	public DelegateProperty(string name, PropertyGetter<TContainer, TValue> getter, PropertySetter<TContainer, TValue> setter = null)
	{
		Name = name;
		m_Getter = getter ?? throw new ArgumentException("getter");
		m_Setter = setter;
	}

	public override TValue GetValue(ref TContainer container)
	{
		return m_Getter(ref container);
	}

	public override void SetValue(ref TContainer container, TValue value)
	{
		if (IsReadOnly)
		{
			throw new InvalidOperationException("Property is ReadOnly.");
		}
		m_Setter(ref container, value);
	}
}
