using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Properties.Internal;

internal class ReflectedPropertyBagProvider
{
	private readonly MethodInfo m_CreatePropertyMethod;

	private readonly MethodInfo m_CreatePropertyBagMethod;

	private readonly MethodInfo m_CreateIndexedCollectionPropertyBagMethod;

	private readonly MethodInfo m_CreateSetPropertyBagMethod;

	private readonly MethodInfo m_CreateKeyValueCollectionPropertyBagMethod;

	private readonly MethodInfo m_CreateKeyValuePairPropertyBagMethod;

	private readonly MethodInfo m_CreateArrayPropertyBagMethod;

	private readonly MethodInfo m_CreateListPropertyBagMethod;

	private readonly MethodInfo m_CreateHashSetPropertyBagMethod;

	private readonly MethodInfo m_CreateDictionaryPropertyBagMethod;

	public ReflectedPropertyBagProvider()
	{
		m_CreatePropertyMethod = typeof(ReflectedPropertyBagProvider).GetMethod("CreateProperty", BindingFlags.Instance | BindingFlags.NonPublic);
		m_CreatePropertyBagMethod = typeof(ReflectedPropertyBagProvider).GetMethods(BindingFlags.Instance | BindingFlags.Public).First((MethodInfo x) => x.Name == "CreatePropertyBag" && x.IsGenericMethod);
		m_CreateIndexedCollectionPropertyBagMethod = typeof(ReflectedPropertyBagProvider).GetMethod("CreateIndexedCollectionPropertyBag", BindingFlags.Instance | BindingFlags.NonPublic);
		m_CreateSetPropertyBagMethod = typeof(ReflectedPropertyBagProvider).GetMethod("CreateSetPropertyBag", BindingFlags.Instance | BindingFlags.NonPublic);
		m_CreateKeyValueCollectionPropertyBagMethod = typeof(ReflectedPropertyBagProvider).GetMethod("CreateKeyValueCollectionPropertyBag", BindingFlags.Instance | BindingFlags.NonPublic);
		m_CreateKeyValuePairPropertyBagMethod = typeof(ReflectedPropertyBagProvider).GetMethod("CreateKeyValuePairPropertyBag", BindingFlags.Instance | BindingFlags.NonPublic);
		m_CreateArrayPropertyBagMethod = typeof(ReflectedPropertyBagProvider).GetMethod("CreateArrayPropertyBag", BindingFlags.Instance | BindingFlags.NonPublic);
		m_CreateListPropertyBagMethod = typeof(ReflectedPropertyBagProvider).GetMethod("CreateListPropertyBag", BindingFlags.Instance | BindingFlags.NonPublic);
		m_CreateHashSetPropertyBagMethod = typeof(ReflectedPropertyBagProvider).GetMethod("CreateHashSetPropertyBag", BindingFlags.Instance | BindingFlags.NonPublic);
		m_CreateDictionaryPropertyBagMethod = typeof(ReflectedPropertyBagProvider).GetMethod("CreateDictionaryPropertyBag", BindingFlags.Instance | BindingFlags.NonPublic);
	}

	public IPropertyBag CreatePropertyBag(Type type)
	{
		if (type.IsGenericTypeDefinition)
		{
			return null;
		}
		return (IPropertyBag)m_CreatePropertyBagMethod.MakeGenericMethod(type).Invoke(this, null);
	}

	public IPropertyBag<TContainer> CreatePropertyBag<TContainer>()
	{
		if (!TypeTraits<TContainer>.IsContainer || TypeTraits<TContainer>.IsObject)
		{
			throw new InvalidOperationException("Invalid container type.");
		}
		if (typeof(TContainer).IsArray)
		{
			if (typeof(TContainer).GetArrayRank() != 1)
			{
				throw new InvalidOperationException("Properties does not support multidimensional arrays.");
			}
			return (IPropertyBag<TContainer>)m_CreateArrayPropertyBagMethod.MakeGenericMethod(typeof(TContainer).GetElementType()).Invoke(this, new object[0]);
		}
		if (typeof(TContainer).IsGenericType && typeof(TContainer).GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
		{
			return (IPropertyBag<TContainer>)m_CreateListPropertyBagMethod.MakeGenericMethod(typeof(TContainer).GetGenericArguments().First()).Invoke(this, new object[0]);
		}
		if (typeof(TContainer).IsGenericType && typeof(TContainer).GetGenericTypeDefinition().IsAssignableFrom(typeof(HashSet<>)))
		{
			return (IPropertyBag<TContainer>)m_CreateHashSetPropertyBagMethod.MakeGenericMethod(typeof(TContainer).GetGenericArguments().First()).Invoke(this, new object[0]);
		}
		if (typeof(TContainer).IsGenericType && typeof(TContainer).GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<, >)))
		{
			return (IPropertyBag<TContainer>)m_CreateDictionaryPropertyBagMethod.MakeGenericMethod(typeof(TContainer).GetGenericArguments().First(), typeof(TContainer).GetGenericArguments().ElementAt(1)).Invoke(this, new object[0]);
		}
		if (typeof(TContainer).IsGenericType && typeof(TContainer).GetGenericTypeDefinition().IsAssignableFrom(typeof(IList<>)))
		{
			return (IPropertyBag<TContainer>)m_CreateIndexedCollectionPropertyBagMethod.MakeGenericMethod(typeof(TContainer), typeof(TContainer).GetGenericArguments().First()).Invoke(this, new object[0]);
		}
		if (typeof(TContainer).IsGenericType && typeof(TContainer).GetGenericTypeDefinition().IsAssignableFrom(typeof(ISet<>)))
		{
			return (IPropertyBag<TContainer>)m_CreateSetPropertyBagMethod.MakeGenericMethod(typeof(TContainer), typeof(TContainer).GetGenericArguments().First()).Invoke(this, new object[0]);
		}
		if (typeof(TContainer).IsGenericType && typeof(TContainer).GetGenericTypeDefinition().IsAssignableFrom(typeof(IDictionary<, >)))
		{
			return (IPropertyBag<TContainer>)m_CreateKeyValueCollectionPropertyBagMethod.MakeGenericMethod(typeof(TContainer), typeof(TContainer).GetGenericArguments().First(), typeof(TContainer).GetGenericArguments().ElementAt(1)).Invoke(this, new object[0]);
		}
		if (typeof(TContainer).IsGenericType && typeof(TContainer).GetGenericTypeDefinition().IsAssignableFrom(typeof(KeyValuePair<, >)))
		{
			Type[] array = typeof(TContainer).GetGenericArguments().ToArray();
			return (IPropertyBag<TContainer>)m_CreateKeyValuePairPropertyBagMethod.MakeGenericMethod(array[0], array[1]).Invoke(this, new object[0]);
		}
		ReflectedPropertyBag<TContainer> reflectedPropertyBag = new ReflectedPropertyBag<TContainer>();
		foreach (MemberInfo propertyMember in GetPropertyMembers(typeof(TContainer)))
		{
			MemberInfo memberInfo = propertyMember;
			MemberInfo memberInfo2 = memberInfo;
			IMemberInfo memberInfo3;
			if (!(memberInfo2 is FieldInfo fieldInfo))
			{
				if (!(memberInfo2 is PropertyInfo propertyInfo))
				{
					throw new InvalidOperationException();
				}
				memberInfo3 = new PropertyMember(propertyInfo);
			}
			else
			{
				memberInfo3 = new FieldMember(fieldInfo);
			}
			m_CreatePropertyMethod.MakeGenericMethod(typeof(TContainer), memberInfo3.ValueType).Invoke(this, new object[2] { memberInfo3, reflectedPropertyBag });
		}
		return reflectedPropertyBag;
	}

	[Preserve]
	private void CreateProperty<TContainer, TValue>(IMemberInfo member, ReflectedPropertyBag<TContainer> propertyBag)
	{
		if (!typeof(TValue).IsPointer)
		{
			propertyBag.AddProperty(new ReflectedMemberProperty<TContainer, TValue>(member, member.Name));
		}
	}

	[Preserve]
	private IPropertyBag<TList> CreateIndexedCollectionPropertyBag<TList, TElement>() where TList : IList<TElement>
	{
		return new IndexedCollectionPropertyBag<TList, TElement>();
	}

	[Preserve]
	private IPropertyBag<TSet> CreateSetPropertyBag<TSet, TValue>() where TSet : ISet<TValue>
	{
		return new SetPropertyBagBase<TSet, TValue>();
	}

	[Preserve]
	private IPropertyBag<TDictionary> CreateKeyValueCollectionPropertyBag<TDictionary, TKey, TValue>() where TDictionary : IDictionary<TKey, TValue>
	{
		return new KeyValueCollectionPropertyBag<TDictionary, TKey, TValue>();
	}

	[Preserve]
	private IPropertyBag<KeyValuePair<TKey, TValue>> CreateKeyValuePairPropertyBag<TKey, TValue>()
	{
		return new KeyValuePairPropertyBag<TKey, TValue>();
	}

	[Preserve]
	private IPropertyBag<TElement[]> CreateArrayPropertyBag<TElement>()
	{
		return new ArrayPropertyBag<TElement>();
	}

	[Preserve]
	private IPropertyBag<List<TElement>> CreateListPropertyBag<TElement>()
	{
		return new ListPropertyBag<TElement>();
	}

	[Preserve]
	private IPropertyBag<HashSet<TElement>> CreateHashSetPropertyBag<TElement>()
	{
		return new HashSetPropertyBag<TElement>();
	}

	[Preserve]
	private IPropertyBag<Dictionary<TKey, TValue>> CreateDictionaryPropertyBag<TKey, TValue>()
	{
		return new DictionaryPropertyBag<TKey, TValue>();
	}

	private static IEnumerable<MemberInfo> GetPropertyMembers(Type type)
	{
		do
		{
			IOrderedEnumerable<MemberInfo> members = from x in type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				orderby x.MetadataToken
				select x;
			foreach (MemberInfo member in members)
			{
				if ((member.MemberType != MemberTypes.Field && member.MemberType != MemberTypes.Property) || member.DeclaringType != type || !IsValidMember(member))
				{
					continue;
				}
				bool hasDontCreatePropertyAttribute = member.GetCustomAttribute<DontCreatePropertyAttribute>() != null;
				bool hasCreatePropertyAttribute = member.GetCustomAttribute<CreatePropertyAttribute>() != null;
				bool hasNonSerializedAttribute = member.GetCustomAttribute<NonSerializedAttribute>() != null;
				bool hasSerializedFieldAttribute = member.GetCustomAttribute<SerializeField>() != null;
				if (hasDontCreatePropertyAttribute)
				{
					continue;
				}
				if (hasCreatePropertyAttribute)
				{
					yield return member;
				}
				else if (!hasNonSerializedAttribute)
				{
					if (hasSerializedFieldAttribute)
					{
						yield return member;
					}
					else if (member is FieldInfo field && field.IsPublic)
					{
						yield return member;
					}
				}
			}
			type = type.BaseType;
		}
		while (type != null && type != typeof(object));
	}

	private static bool IsValidMember(MemberInfo memberInfo)
	{
		if (!(memberInfo is FieldInfo fieldInfo))
		{
			if (memberInfo is PropertyInfo propertyInfo)
			{
				return null != propertyInfo.GetMethod && !propertyInfo.GetMethod.IsStatic && IsValidPropertyType(propertyInfo.PropertyType);
			}
			return false;
		}
		return !fieldInfo.IsStatic && IsValidPropertyType(fieldInfo.FieldType);
	}

	private static bool IsValidPropertyType(Type type)
	{
		if (type.IsPointer)
		{
			return false;
		}
		return !type.IsGenericType || type.GetGenericArguments().All(IsValidPropertyType);
	}
}
