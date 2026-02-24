using System;
using System.Collections.Generic;
using Unity.Properties.Internal;

namespace Unity.Properties;

public static class PropertyBag
{
	public static void AcceptWithSpecializedVisitor<TContainer>(IPropertyBag<TContainer> properties, IPropertyBagVisitor visitor, ref TContainer container)
	{
		if (properties == null)
		{
			throw new ArgumentNullException("properties");
		}
		if (!(properties is IDictionaryPropertyBagAccept<TContainer> dictionaryPropertyBagAccept) || !(visitor is IDictionaryPropertyBagVisitor visitor2))
		{
			if (!(properties is IListPropertyBagAccept<TContainer> listPropertyBagAccept) || !(visitor is IListPropertyBagVisitor visitor3))
			{
				if (!(properties is ISetPropertyBagAccept<TContainer> setPropertyBagAccept) || !(visitor is ISetPropertyBagVisitor visitor4))
				{
					if (properties is ICollectionPropertyBagAccept<TContainer> collectionPropertyBagAccept && visitor is ICollectionPropertyBagVisitor visitor5)
					{
						collectionPropertyBagAccept.Accept(visitor5, ref container);
					}
					else
					{
						properties.Accept(visitor, ref container);
					}
				}
				else
				{
					setPropertyBagAccept.Accept(visitor4, ref container);
				}
			}
			else
			{
				listPropertyBagAccept.Accept(visitor3, ref container);
			}
		}
		else
		{
			dictionaryPropertyBagAccept.Accept(visitor2, ref container);
		}
	}

	public static void Register<TContainer>(PropertyBag<TContainer> propertyBag)
	{
		PropertyBagStore.AddPropertyBag(propertyBag);
	}

	public static void RegisterArray<TElement>()
	{
		if (PropertyBagStore.TypedStore<IPropertyBag<TElement[]>>.PropertyBag == null)
		{
			PropertyBagStore.AddPropertyBag(new ArrayPropertyBag<TElement>());
		}
	}

	public static void RegisterArray<TContainer, TElement>()
	{
		RegisterArray<TElement>();
	}

	public static void RegisterList<TElement>()
	{
		if (PropertyBagStore.TypedStore<IPropertyBag<TElement[]>>.PropertyBag == null)
		{
			PropertyBagStore.AddPropertyBag(new ListPropertyBag<TElement>());
		}
	}

	public static void RegisterList<TContainer, TElement>()
	{
		RegisterList<TElement>();
	}

	public static void RegisterHashSet<TElement>()
	{
		if (PropertyBagStore.TypedStore<IPropertyBag<HashSet<TElement>>>.PropertyBag == null)
		{
			PropertyBagStore.AddPropertyBag(new HashSetPropertyBag<TElement>());
		}
	}

	public static void RegisterHashSet<TContainer, TElement>()
	{
		RegisterHashSet<TElement>();
	}

	public static void RegisterDictionary<TKey, TValue>()
	{
		if (PropertyBagStore.TypedStore<IPropertyBag<Dictionary<TKey, TValue>>>.PropertyBag == null)
		{
			PropertyBagStore.AddPropertyBag(new DictionaryPropertyBag<TKey, TValue>());
		}
	}

	public static void RegisterDictionary<TContainer, TKey, TValue>()
	{
		RegisterDictionary<TKey, TValue>();
	}

	public static void RegisterIList<TList, TElement>() where TList : IList<TElement>
	{
		if (PropertyBagStore.TypedStore<IPropertyBag<TList>>.PropertyBag == null)
		{
			PropertyBagStore.AddPropertyBag(new IndexedCollectionPropertyBag<TList, TElement>());
		}
	}

	public static void RegisterIList<TContainer, TList, TElement>() where TList : IList<TElement>
	{
		RegisterIList<TList, TElement>();
	}

	public static void RegisterISet<TSet, TElement>() where TSet : ISet<TElement>
	{
		if (PropertyBagStore.TypedStore<IPropertyBag<TSet>>.PropertyBag == null)
		{
			PropertyBagStore.AddPropertyBag(new SetPropertyBagBase<TSet, TElement>());
		}
	}

	public static void RegisterISet<TContainer, TSet, TElement>() where TSet : ISet<TElement>
	{
		RegisterISet<TSet, TElement>();
	}

	public static void RegisterIDictionary<TDictionary, TKey, TValue>() where TDictionary : IDictionary<TKey, TValue>
	{
		if (PropertyBagStore.TypedStore<IPropertyBag<TDictionary>>.PropertyBag == null)
		{
			PropertyBagStore.AddPropertyBag(new KeyValueCollectionPropertyBag<TDictionary, TKey, TValue>());
			PropertyBagStore.AddPropertyBag(new KeyValuePairPropertyBag<TKey, TValue>());
		}
	}

	public static void RegisterIDictionary<TContainer, TDictionary, TKey, TValue>() where TDictionary : IDictionary<TKey, TValue>
	{
		RegisterIDictionary<TDictionary, TKey, TValue>();
	}

	public static TContainer CreateInstance<TContainer>()
	{
		IPropertyBag<TContainer> propertyBag = PropertyBagStore.GetPropertyBag<TContainer>();
		if (propertyBag == null)
		{
			throw new MissingPropertyBagException(typeof(TContainer));
		}
		return propertyBag.CreateInstance();
	}

	public static IPropertyBag GetPropertyBag(Type type)
	{
		return PropertyBagStore.GetPropertyBag(type);
	}

	public static IPropertyBag<TContainer> GetPropertyBag<TContainer>()
	{
		return PropertyBagStore.GetPropertyBag<TContainer>();
	}

	public static bool TryGetPropertyBagForValue<TValue>(ref TValue value, out IPropertyBag propertyBag)
	{
		return PropertyBagStore.TryGetPropertyBagForValue(ref value, out propertyBag);
	}

	public static bool Exists<TContainer>()
	{
		return PropertyBagStore.Exists<TContainer>();
	}

	public static bool Exists(Type type)
	{
		return PropertyBagStore.Exists(type);
	}

	public static IEnumerable<Type> GetAllTypesWithAPropertyBag()
	{
		return PropertyBagStore.AllTypes;
	}
}
public abstract class PropertyBag<TContainer> : IPropertyBag<TContainer>, IPropertyBag, IPropertyBagRegister, IConstructor<TContainer>, IConstructor
{
	InstantiationKind IConstructor.InstantiationKind => InstantiationKind;

	protected virtual InstantiationKind InstantiationKind { get; } = InstantiationKind.Activator;

	static PropertyBag()
	{
		if (!TypeTraits.IsContainer(typeof(TContainer)))
		{
			throw new InvalidOperationException($"Failed to create a property bag for Type=[{typeof(TContainer)}]. The type is not a valid container type.");
		}
	}

	void IPropertyBagRegister.Register()
	{
		PropertyBagStore.AddPropertyBag(this);
	}

	public void Accept(ITypeVisitor visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		visitor.Visit<TContainer>();
	}

	void IPropertyBag.Accept(IPropertyBagVisitor visitor, ref object container)
	{
		if (container == null)
		{
			throw new ArgumentNullException("container");
		}
		if (!(container is TContainer container2) || 1 == 0)
		{
			throw new ArgumentException($"The given ContainerType=[{container.GetType()}] does not match the PropertyBagType=[{typeof(TContainer)}]");
		}
		PropertyBag.AcceptWithSpecializedVisitor(this, visitor, ref container2);
		container = container2;
	}

	void IPropertyBag<TContainer>.Accept(IPropertyBagVisitor visitor, ref TContainer container)
	{
		visitor.Visit(this, ref container);
	}

	PropertyCollection<TContainer> IPropertyBag<TContainer>.GetProperties()
	{
		return GetProperties();
	}

	PropertyCollection<TContainer> IPropertyBag<TContainer>.GetProperties(ref TContainer container)
	{
		return GetProperties(ref container);
	}

	TContainer IConstructor<TContainer>.Instantiate()
	{
		return Instantiate();
	}

	public abstract PropertyCollection<TContainer> GetProperties();

	public abstract PropertyCollection<TContainer> GetProperties(ref TContainer container);

	protected virtual TContainer Instantiate()
	{
		return default(TContainer);
	}

	public TContainer CreateInstance()
	{
		return TypeUtility.Instantiate<TContainer>();
	}

	public bool TryCreateInstance(out TContainer instance)
	{
		return TypeUtility.TryInstantiate<TContainer>(out instance);
	}
}
