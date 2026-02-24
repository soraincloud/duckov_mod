using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Properties.Internal;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Scripting;

namespace Unity.Properties;

public static class TypeUtility
{
	private interface ITypeConstructor
	{
		bool CanBeInstantiated { get; }

		object Instantiate();
	}

	private interface ITypeConstructor<T> : ITypeConstructor
	{
		new T Instantiate();

		void SetExplicitConstructor(Func<T> constructor);
	}

	private class TypeConstructor<T> : ITypeConstructor<T>, ITypeConstructor
	{
		private Func<T> m_ExplicitConstructor;

		private Func<T> m_ImplicitConstructor;

		private IConstructor<T> m_OverrideConstructor;

		bool ITypeConstructor.CanBeInstantiated
		{
			get
			{
				if (m_ExplicitConstructor != null)
				{
					return true;
				}
				if (m_OverrideConstructor != null)
				{
					if (m_OverrideConstructor.InstantiationKind == InstantiationKind.NotInstantiatable)
					{
						return false;
					}
					if (m_OverrideConstructor.InstantiationKind == InstantiationKind.PropertyBagOverride)
					{
						return true;
					}
				}
				return m_ImplicitConstructor != null;
			}
		}

		public TypeConstructor()
		{
			m_OverrideConstructor = PropertyBagStore.GetPropertyBag<T>() as IConstructor<T>;
			SetImplicitConstructor();
		}

		private void SetImplicitConstructor()
		{
			Type typeFromHandle = typeof(T);
			if (typeFromHandle.IsValueType)
			{
				m_ImplicitConstructor = CreateValueTypeInstance;
			}
			else if (!typeFromHandle.IsAbstract)
			{
				if (typeof(ScriptableObject).IsAssignableFrom(typeFromHandle))
				{
					m_ImplicitConstructor = CreateScriptableObjectInstance;
				}
				else if (null != typeFromHandle.GetConstructor(Array.Empty<Type>()))
				{
					m_ImplicitConstructor = CreateClassInstance;
				}
			}
		}

		private static T CreateValueTypeInstance()
		{
			return default(T);
		}

		private static T CreateScriptableObjectInstance()
		{
			return (T)(object)ScriptableObject.CreateInstance(typeof(T));
		}

		private static T CreateClassInstance()
		{
			return Activator.CreateInstance<T>();
		}

		public void SetExplicitConstructor(Func<T> constructor)
		{
			m_ExplicitConstructor = constructor;
		}

		T ITypeConstructor<T>.Instantiate()
		{
			if (m_ExplicitConstructor != null)
			{
				return m_ExplicitConstructor();
			}
			if (m_OverrideConstructor != null)
			{
				if (m_OverrideConstructor.InstantiationKind == InstantiationKind.NotInstantiatable)
				{
					throw new InvalidOperationException("The type '" + typeof(T).Name + "' is not constructable.");
				}
				if (m_OverrideConstructor.InstantiationKind == InstantiationKind.PropertyBagOverride)
				{
					return m_OverrideConstructor.Instantiate();
				}
			}
			if (m_ImplicitConstructor != null)
			{
				return m_ImplicitConstructor();
			}
			throw new InvalidOperationException("The type '" + typeof(T).Name + "' is not constructable.");
		}

		object ITypeConstructor.Instantiate()
		{
			return ((ITypeConstructor<T>)this).Instantiate();
		}
	}

	private class NonConstructable : ITypeConstructor
	{
		bool ITypeConstructor.CanBeInstantiated => false;

		public object Instantiate()
		{
			throw new InvalidOperationException("The type is not instantiatable.");
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct Cache<T>
	{
		public static ITypeConstructor<T> TypeConstructor;
	}

	private class TypeConstructorVisitor : ITypeVisitor
	{
		public ITypeConstructor TypeConstructor;

		public void Visit<TContainer>()
		{
			TypeConstructor = CreateTypeConstructor<TContainer>();
		}
	}

	private static readonly ConcurrentDictionary<Type, ITypeConstructor> s_TypeConstructors;

	private static readonly MethodInfo s_CreateTypeConstructor;

	private static readonly ConcurrentDictionary<Type, string> s_CachedResolvedName;

	private static readonly ObjectPool<StringBuilder> s_Builders;

	private static readonly object syncedPoolObject;

	static TypeUtility()
	{
		s_TypeConstructors = new ConcurrentDictionary<Type, ITypeConstructor>();
		syncedPoolObject = new object();
		s_CachedResolvedName = new ConcurrentDictionary<Type, string>();
		s_Builders = new ObjectPool<StringBuilder>(() => new StringBuilder(), null, delegate(StringBuilder sb)
		{
			sb.Clear();
		});
		SetExplicitInstantiationMethod(() => string.Empty);
		MethodInfo[] methods = typeof(TypeUtility).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			if (!(methodInfo.Name != "CreateTypeConstructor") && methodInfo.IsGenericMethod)
			{
				s_CreateTypeConstructor = methodInfo;
				break;
			}
		}
		if (null == s_CreateTypeConstructor)
		{
			throw new InvalidProgramException();
		}
	}

	public static string GetTypeDisplayName(Type type)
	{
		if (s_CachedResolvedName.TryGetValue(type, out var value))
		{
			return value;
		}
		int argIndex = 0;
		value = GetTypeDisplayName(type, type.GetGenericArguments(), ref argIndex);
		s_CachedResolvedName[type] = value;
		return value;
	}

	private static string GetTypeDisplayName(Type type, IReadOnlyList<Type> args, ref int argIndex)
	{
		if (type == typeof(int))
		{
			return "int";
		}
		if (type == typeof(uint))
		{
			return "uint";
		}
		if (type == typeof(short))
		{
			return "short";
		}
		if (type == typeof(ushort))
		{
			return "ushort";
		}
		if (type == typeof(byte))
		{
			return "byte";
		}
		if (type == typeof(char))
		{
			return "char";
		}
		if (type == typeof(bool))
		{
			return "bool";
		}
		if (type == typeof(long))
		{
			return "long";
		}
		if (type == typeof(ulong))
		{
			return "ulong";
		}
		if (type == typeof(float))
		{
			return "float";
		}
		if (type == typeof(double))
		{
			return "double";
		}
		if (type == typeof(string))
		{
			return "string";
		}
		string text = type.Name;
		if (type.IsGenericParameter)
		{
			return text;
		}
		if (type.IsNested)
		{
			text = GetTypeDisplayName(type.DeclaringType, args, ref argIndex) + "." + text;
		}
		if (!type.IsGenericType)
		{
			return text;
		}
		int num = text.IndexOf('`');
		int num2 = type.GetGenericArguments().Length;
		if (num > -1)
		{
			num2 = int.Parse(text.Substring(num + 1));
			text = text.Remove(num);
		}
		StringBuilder stringBuilder = null;
		lock (syncedPoolObject)
		{
			stringBuilder = s_Builders.Get();
		}
		try
		{
			int num3 = 0;
			while (num3 < num2 && argIndex < args.Count)
			{
				if (num3 != 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(GetTypeDisplayName(args[argIndex]));
				num3++;
				argIndex++;
			}
			if (stringBuilder.Length > 0)
			{
				text = $"{text}<{stringBuilder}>";
			}
		}
		finally
		{
			lock (syncedPoolObject)
			{
				s_Builders.Release(stringBuilder);
			}
		}
		return text;
	}

	public static Type GetRootType(this Type type)
	{
		if (type.IsInterface)
		{
			return null;
		}
		Type type2 = (type.IsValueType ? typeof(ValueType) : typeof(object));
		while (type2 != type.BaseType)
		{
			type = type.BaseType;
		}
		return type;
	}

	[Preserve]
	private static ITypeConstructor CreateTypeConstructor(Type type)
	{
		IPropertyBag propertyBag = PropertyBagStore.GetPropertyBag(type);
		if (propertyBag != null)
		{
			TypeConstructorVisitor typeConstructorVisitor = new TypeConstructorVisitor();
			propertyBag.Accept(typeConstructorVisitor);
			return typeConstructorVisitor.TypeConstructor;
		}
		if (type.ContainsGenericParameters)
		{
			NonConstructable nonConstructable = new NonConstructable();
			s_TypeConstructors[type] = nonConstructable;
			return nonConstructable;
		}
		return s_CreateTypeConstructor.MakeGenericMethod(type).Invoke(null, null) as ITypeConstructor;
	}

	private static ITypeConstructor<T> CreateTypeConstructor<T>()
	{
		TypeConstructor<T> typeConstructor = (TypeConstructor<T>)(Cache<T>.TypeConstructor = new TypeConstructor<T>());
		s_TypeConstructors[typeof(T)] = typeConstructor;
		return typeConstructor;
	}

	private static ITypeConstructor GetTypeConstructor(Type type)
	{
		ITypeConstructor value;
		return s_TypeConstructors.TryGetValue(type, out value) ? value : CreateTypeConstructor(type);
	}

	private static ITypeConstructor<T> GetTypeConstructor<T>()
	{
		return (Cache<T>.TypeConstructor != null) ? Cache<T>.TypeConstructor : CreateTypeConstructor<T>();
	}

	public static bool CanBeInstantiated(Type type)
	{
		return GetTypeConstructor(type).CanBeInstantiated;
	}

	public static bool CanBeInstantiated<T>()
	{
		return GetTypeConstructor<T>().CanBeInstantiated;
	}

	public static void SetExplicitInstantiationMethod<T>(Func<T> constructor)
	{
		GetTypeConstructor<T>().SetExplicitConstructor(constructor);
	}

	public static T Instantiate<T>()
	{
		ITypeConstructor<T> typeConstructor = GetTypeConstructor<T>();
		CheckCanBeInstantiated(typeConstructor);
		return typeConstructor.Instantiate();
	}

	public static bool TryInstantiate<T>(out T instance)
	{
		ITypeConstructor<T> typeConstructor = GetTypeConstructor<T>();
		if (typeConstructor.CanBeInstantiated)
		{
			instance = typeConstructor.Instantiate();
			return true;
		}
		instance = default(T);
		return false;
	}

	public static T Instantiate<T>(Type derivedType)
	{
		ITypeConstructor typeConstructor = GetTypeConstructor(derivedType);
		CheckIsAssignableFrom(typeof(T), derivedType);
		CheckCanBeInstantiated(typeConstructor, derivedType);
		return (T)typeConstructor.Instantiate();
	}

	public static bool TryInstantiate<T>(Type derivedType, out T value)
	{
		if (!typeof(T).IsAssignableFrom(derivedType))
		{
			value = default(T);
			value = default(T);
			return false;
		}
		ITypeConstructor typeConstructor = GetTypeConstructor(derivedType);
		if (!typeConstructor.CanBeInstantiated)
		{
			value = default(T);
			return false;
		}
		value = (T)typeConstructor.Instantiate();
		return true;
	}

	public static TArray InstantiateArray<TArray>(int count = 0)
	{
		if (count < 0)
		{
			throw new ArgumentException(string.Format("{0}: Cannot construct an array with {1}={2}", "TypeUtility", "count", count));
		}
		IPropertyBag<TArray> propertyBag = PropertyBagStore.GetPropertyBag<TArray>();
		if (propertyBag is IConstructorWithCount<TArray> constructorWithCount)
		{
			return constructorWithCount.InstantiateWithCount(count);
		}
		Type typeFromHandle = typeof(TArray);
		if (!typeFromHandle.IsArray)
		{
			throw new ArgumentException("TypeUtility: Cannot construct an array, since " + typeof(TArray).Name + " is not an array type.");
		}
		Type elementType = typeFromHandle.GetElementType();
		if (null == elementType)
		{
			throw new ArgumentException("TypeUtility: Cannot construct an array, since " + typeof(TArray).Name + ".GetElementType() returned null.");
		}
		return (TArray)(object)Array.CreateInstance(elementType, count);
	}

	public static bool TryInstantiateArray<TArray>(int count, out TArray instance)
	{
		if (count < 0)
		{
			instance = default(TArray);
			return false;
		}
		IPropertyBag<TArray> propertyBag = PropertyBagStore.GetPropertyBag<TArray>();
		if (propertyBag is IConstructorWithCount<TArray> constructorWithCount)
		{
			try
			{
				instance = constructorWithCount.InstantiateWithCount(count);
				return true;
			}
			catch
			{
			}
		}
		Type typeFromHandle = typeof(TArray);
		if (!typeFromHandle.IsArray)
		{
			instance = default(TArray);
			return false;
		}
		Type elementType = typeFromHandle.GetElementType();
		if (null == elementType)
		{
			instance = default(TArray);
			return false;
		}
		instance = (TArray)(object)Array.CreateInstance(elementType, count);
		return true;
	}

	public static TArray InstantiateArray<TArray>(Type derivedType, int count = 0)
	{
		if (count < 0)
		{
			throw new ArgumentException(string.Format("{0}: Cannot instantiate an array with {1}={2}", "TypeUtility", "count", count));
		}
		IPropertyBag propertyBag = PropertyBagStore.GetPropertyBag(derivedType);
		if (propertyBag is IConstructorWithCount<TArray> constructorWithCount)
		{
			return constructorWithCount.InstantiateWithCount(count);
		}
		Type typeFromHandle = typeof(TArray);
		if (!typeFromHandle.IsArray)
		{
			throw new ArgumentException("TypeUtility: Cannot instantiate an array, since " + typeof(TArray).Name + " is not an array type.");
		}
		Type elementType = typeFromHandle.GetElementType();
		if (null == elementType)
		{
			throw new ArgumentException("TypeUtility: Cannot instantiate an array, since " + typeof(TArray).Name + ".GetElementType() returned null.");
		}
		return (TArray)(object)Array.CreateInstance(elementType, count);
	}

	private static void CheckIsAssignableFrom(Type type, Type derivedType)
	{
		if (!type.IsAssignableFrom(derivedType))
		{
			throw new ArgumentException("Could not create instance of type `" + derivedType.Name + "` and convert to `" + type.Name + "`: The given type is not assignable to target type.");
		}
	}

	private static void CheckCanBeInstantiated<T>(ITypeConstructor<T> constructor)
	{
		if (!constructor.CanBeInstantiated)
		{
			throw new InvalidOperationException("Type `" + typeof(T).Name + "` could not be instantiated. A parameter-less constructor or an explicit construction method is required.");
		}
	}

	private static void CheckCanBeInstantiated(ITypeConstructor constructor, Type type)
	{
		if (!constructor.CanBeInstantiated)
		{
			throw new InvalidOperationException("Type `" + type.Name + "` could not be instantiated. A parameter-less constructor or an explicit construction method is required.");
		}
	}
}
