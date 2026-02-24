using System;
using System.Collections.Generic;
using Unity.Properties.Internal;

namespace Unity.Properties;

public readonly struct AttributesScope : IDisposable
{
	private readonly IAttributes m_Target;

	private readonly List<Attribute> m_Previous;

	public AttributesScope(IProperty target, IProperty source)
	{
		m_Target = target as IAttributes;
		m_Previous = (target as IAttributes)?.Attributes;
		if (m_Target != null)
		{
			m_Target.Attributes = (source as IAttributes)?.Attributes;
		}
	}

	internal AttributesScope(IAttributes target, List<Attribute> attributes)
	{
		m_Target = target;
		m_Previous = target.Attributes;
		target.Attributes = attributes;
	}

	public void Dispose()
	{
		if (m_Target != null)
		{
			m_Target.Attributes = m_Previous;
		}
	}
}
