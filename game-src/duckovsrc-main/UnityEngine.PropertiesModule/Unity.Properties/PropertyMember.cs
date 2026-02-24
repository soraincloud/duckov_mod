using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Properties.Internal;

namespace Unity.Properties;

internal readonly struct PropertyMember : IMemberInfo
{
	internal readonly PropertyInfo m_PropertyInfo;

	public string Name { get; }

	public bool IsReadOnly => !m_PropertyInfo.CanWrite;

	public Type ValueType => m_PropertyInfo.PropertyType;

	public PropertyMember(PropertyInfo propertyInfo)
	{
		m_PropertyInfo = propertyInfo;
		Name = ReflectionUtilities.SanitizeMemberName(m_PropertyInfo);
	}

	public object GetValue(object obj)
	{
		return m_PropertyInfo.GetValue(obj);
	}

	public void SetValue(object obj, object value)
	{
		m_PropertyInfo.SetValue(obj, value);
	}

	public IEnumerable<Attribute> GetCustomAttributes()
	{
		return m_PropertyInfo.GetCustomAttributes();
	}
}
