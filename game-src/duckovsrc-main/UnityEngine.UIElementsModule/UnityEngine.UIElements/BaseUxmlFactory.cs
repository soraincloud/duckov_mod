using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

public abstract class BaseUxmlFactory<TCreatedType, TTraits> where TCreatedType : new() where TTraits : BaseUxmlTraits, new()
{
	internal TTraits m_Traits;

	public virtual string uxmlName => typeof(TCreatedType).Name;

	public virtual string uxmlNamespace => typeof(TCreatedType).Namespace ?? string.Empty;

	public virtual string uxmlQualifiedName => typeof(TCreatedType).FullName;

	public virtual Type uxmlType => typeof(TCreatedType);

	public bool canHaveAnyAttribute => m_Traits.canHaveAnyAttribute;

	public virtual IEnumerable<UxmlAttributeDescription> uxmlAttributesDescription => m_Traits.uxmlAttributesDescription;

	public virtual IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription => m_Traits.uxmlChildElementsDescription;

	public virtual string substituteForTypeName
	{
		get
		{
			if (typeof(TCreatedType) == typeof(VisualElement))
			{
				return string.Empty;
			}
			return typeof(VisualElement).Name;
		}
	}

	public virtual string substituteForTypeNamespace
	{
		get
		{
			if (typeof(TCreatedType) == typeof(VisualElement))
			{
				return string.Empty;
			}
			return typeof(VisualElement).Namespace ?? string.Empty;
		}
	}

	public virtual string substituteForTypeQualifiedName
	{
		get
		{
			if (typeof(TCreatedType) == typeof(VisualElement))
			{
				return string.Empty;
			}
			return typeof(VisualElement).FullName;
		}
	}

	protected BaseUxmlFactory()
	{
		m_Traits = new TTraits();
	}

	public virtual bool AcceptsAttributeBag(IUxmlAttributes bag, CreationContext cc)
	{
		return true;
	}
}
