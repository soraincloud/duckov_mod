using System;
using System.Collections.Generic;
using Unity.Properties.Internal;

namespace Unity.Properties;

public abstract class PathVisitor : IPropertyBagVisitor, IPropertyVisitor
{
	private readonly struct PropertyScope : IDisposable
	{
		private readonly PathVisitor m_Visitor;

		private readonly IProperty m_Property;

		public PropertyScope(PathVisitor visitor, IProperty property)
		{
			m_Visitor = visitor;
			m_Property = m_Visitor.Property;
			m_Visitor.Property = property;
		}

		public void Dispose()
		{
			m_Visitor.Property = m_Property;
		}
	}

	private int m_PathIndex;

	public PropertyPath Path { get; set; }

	private IProperty Property { get; set; }

	public bool ReadonlyVisit { get; set; }

	public VisitReturnCode ReturnCode { get; protected set; }

	public virtual void Reset()
	{
		m_PathIndex = 0;
		Path = default(PropertyPath);
		ReturnCode = VisitReturnCode.Ok;
		ReadonlyVisit = false;
	}

	void IPropertyBagVisitor.Visit<TContainer>(IPropertyBag<TContainer> properties, ref TContainer container)
	{
		PropertyPathPart propertyPathPart = Path[m_PathIndex++];
		IProperty<TContainer> property;
		switch (propertyPathPart.Kind)
		{
		case PropertyPathPartKind.Name:
			if (properties is INamedProperties<TContainer> namedProperties && namedProperties.TryGetProperty(ref container, propertyPathPart.Name, out property))
			{
				property.Accept(this, ref container);
			}
			else
			{
				ReturnCode = VisitReturnCode.InvalidPath;
			}
			break;
		case PropertyPathPartKind.Index:
			if (properties is IIndexedProperties<TContainer> indexedProperties && indexedProperties.TryGetProperty(ref container, propertyPathPart.Index, out property))
			{
				using ((property as IAttributes).CreateAttributesScope(Property as IAttributes))
				{
					property.Accept(this, ref container);
					break;
				}
			}
			ReturnCode = VisitReturnCode.InvalidPath;
			break;
		case PropertyPathPartKind.Key:
			if (properties is IKeyedProperties<TContainer, object> keyedProperties && keyedProperties.TryGetProperty(ref container, propertyPathPart.Key, out property))
			{
				using ((property as IAttributes).CreateAttributesScope(Property as IAttributes))
				{
					property.Accept(this, ref container);
					break;
				}
			}
			ReturnCode = VisitReturnCode.InvalidPath;
			break;
		default:
			ReturnCode = VisitReturnCode.InvalidPath;
			break;
		}
	}

	void IPropertyVisitor.Visit<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container)
	{
		TValue value = property.GetValue(ref container);
		IPropertyBag propertyBag;
		if (m_PathIndex >= Path.Length)
		{
			VisitPath(property, ref container, ref value);
		}
		else if (PropertyBag.TryGetPropertyBagForValue(ref value, out propertyBag))
		{
			if (TypeTraits<TValue>.CanBeNull && EqualityComparer<TValue>.Default.Equals(value, default(TValue)))
			{
				ReturnCode = VisitReturnCode.InvalidPath;
				return;
			}
			using (new PropertyScope(this, property))
			{
				PropertyContainer.Accept(this, ref value);
			}
			if (!property.IsReadOnly && !ReadonlyVisit)
			{
				property.SetValue(ref container, value);
			}
		}
		else
		{
			ReturnCode = VisitReturnCode.InvalidPath;
		}
	}

	protected virtual void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
	{
	}
}
