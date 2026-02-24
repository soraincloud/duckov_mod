using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Unity.Properties.Internal;

internal static class PropertyBagStore
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct TypedStore<TContainer>
	{
		public static IPropertyBag<TContainer> PropertyBag;
	}

	private static readonly ConcurrentDictionary<Type, IPropertyBag> s_PropertyBags;

	private static readonly List<Type> s_RegisteredTypes;

	private static ReflectedPropertyBagProvider s_PropertyBagProvider;

	internal static bool HasProvider => s_PropertyBagProvider != null;

	internal static List<Type> AllTypes => s_RegisteredTypes;

	internal static event Action<Type, IPropertyBag> NewTypeRegistered;

	static PropertyBagStore()
	{
		s_PropertyBags = new ConcurrentDictionary<Type, IPropertyBag>();
		s_RegisteredTypes = new List<Type>();
		s_PropertyBagProvider = null;
		s_PropertyBagProvider = new ReflectedPropertyBagProvider();
		DefaultPropertyBagInitializer.Initialize();
	}

	internal static void AddPropertyBag<TContainer>(IPropertyBag<TContainer> propertyBag)
	{
		if (!TypeTraits<TContainer>.IsContainer)
		{
			throw new Exception($"PropertyBagStore Type=[{typeof(TContainer)}] is not a valid container type. Type can not be primitive, enum or string.");
		}
		if (TypeTraits<TContainer>.IsAbstractOrInterface)
		{
			throw new Exception($"PropertyBagStore Type=[{typeof(TContainer)}] is not a valid container type. Type can not be abstract or interface.");
		}
		if (TypedStore<TContainer>.PropertyBag != null)
		{
			IPropertyBag<TContainer> propertyBag2 = TypedStore<TContainer>.PropertyBag;
			if (propertyBag2.GetType().Assembly == typeof(TContainer).Assembly || (propertyBag.GetType().GetCustomAttributes<CompilerGeneratedAttribute>().Any() && propertyBag.GetType().Assembly != typeof(TContainer).Assembly))
			{
				return;
			}
		}
		TypedStore<TContainer>.PropertyBag = propertyBag;
		if (!s_PropertyBags.ContainsKey(typeof(TContainer)))
		{
			s_RegisteredTypes.Add(typeof(TContainer));
		}
		s_PropertyBags[typeof(TContainer)] = propertyBag;
		PropertyBagStore.NewTypeRegistered?.Invoke(typeof(TContainer), propertyBag);
	}

	internal static IPropertyBag<TContainer> GetPropertyBag<TContainer>()
	{
		if (TypedStore<TContainer>.PropertyBag != null)
		{
			return TypedStore<TContainer>.PropertyBag;
		}
		IPropertyBag propertyBag = GetPropertyBag(typeof(TContainer));
		if (propertyBag == null)
		{
			return null;
		}
		if (!(propertyBag is IPropertyBag<TContainer> result))
		{
			throw new InvalidOperationException("PropertyBag type container type mismatch.");
		}
		return result;
	}

	internal static IPropertyBag GetPropertyBag(Type type)
	{
		if (s_PropertyBags.TryGetValue(type, out var value))
		{
			return value;
		}
		if (!TypeTraits.IsContainer(type))
		{
			return null;
		}
		if (type.IsArray && type.GetArrayRank() != 1)
		{
			return null;
		}
		if (type.IsInterface || type.IsAbstract)
		{
			return null;
		}
		if (type == typeof(object))
		{
			return null;
		}
		if (s_PropertyBagProvider != null)
		{
			value = s_PropertyBagProvider.CreatePropertyBag(type);
			if (value != null)
			{
				(value as IPropertyBagRegister)?.Register();
				return value;
			}
			s_PropertyBags.TryAdd(type, null);
		}
		return null;
	}

	internal static bool Exists<TContainer>()
	{
		return TypedStore<TContainer>.PropertyBag != null;
	}

	internal static bool Exists(Type type)
	{
		return s_PropertyBags.ContainsKey(type);
	}

	internal static bool Exists<TContainer>(ref TContainer value)
	{
		if (!TypeTraits<TContainer>.CanBeNull)
		{
			return GetPropertyBag<TContainer>() != null;
		}
		if (EqualityComparer<TContainer>.Default.Equals(value, default(TContainer)))
		{
			return false;
		}
		return GetPropertyBag(value.GetType()) != null;
	}

	internal static bool TryGetPropertyBagForValue<TValue>(ref TValue value, out IPropertyBag propertyBag)
	{
		if (!TypeTraits<TValue>.IsContainer)
		{
			propertyBag = null;
			return false;
		}
		if (TypeTraits<TValue>.CanBeNull && EqualityComparer<TValue>.Default.Equals(value, default(TValue)))
		{
			propertyBag = GetPropertyBag<TValue>();
			return propertyBag != null;
		}
		if (TypeTraits<TValue>.IsValueType)
		{
			propertyBag = GetPropertyBag<TValue>();
			return propertyBag != null;
		}
		propertyBag = GetPropertyBag(value.GetType());
		return propertyBag != null;
	}
}
