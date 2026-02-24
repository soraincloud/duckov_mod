using System;
using System.Reflection;
using UnityEngine;

namespace Unity.Properties;

public static class TypeTraits
{
	public static bool IsContainer(Type type)
	{
		if (null == type)
		{
			throw new ArgumentNullException("type");
		}
		return !type.IsPrimitive && !type.IsPointer && !type.IsEnum && !(type == typeof(string));
	}
}
public static class TypeTraits<T>
{
	public static bool IsValueType { get; }

	public static bool IsPrimitive { get; }

	public static bool IsInterface { get; }

	public static bool IsAbstract { get; }

	public static bool IsArray { get; }

	public static bool IsMultidimensionalArray { get; }

	public static bool IsEnum { get; }

	public static bool IsEnumFlags { get; }

	public static bool IsNullable { get; }

	public static bool IsObject { get; }

	public static bool IsString { get; }

	public static bool IsContainer { get; }

	public static bool CanBeNull { get; }

	public static bool IsPrimitiveOrString { get; }

	public static bool IsAbstractOrInterface { get; }

	public static bool IsUnityObject { get; }

	public static bool IsLazyLoadReference { get; }

	static TypeTraits()
	{
		Type typeFromHandle = typeof(T);
		IsValueType = typeFromHandle.IsValueType;
		IsPrimitive = typeFromHandle.IsPrimitive;
		IsInterface = typeFromHandle.IsInterface;
		IsAbstract = typeFromHandle.IsAbstract;
		IsArray = typeFromHandle.IsArray;
		IsEnum = typeFromHandle.IsEnum;
		IsEnumFlags = IsEnum && typeFromHandle.GetCustomAttribute<FlagsAttribute>() != null;
		IsNullable = Nullable.GetUnderlyingType(typeof(T)) != null;
		IsMultidimensionalArray = IsArray && typeof(T).GetArrayRank() != 1;
		IsObject = typeFromHandle == typeof(object);
		IsString = typeFromHandle == typeof(string);
		IsContainer = TypeTraits.IsContainer(typeFromHandle);
		CanBeNull = !IsValueType;
		IsPrimitiveOrString = IsPrimitive || IsString;
		IsAbstractOrInterface = IsAbstract || IsInterface;
		CanBeNull |= IsNullable;
		IsLazyLoadReference = typeFromHandle.IsGenericType && typeFromHandle.GetGenericTypeDefinition() == typeof(LazyLoadReference<>);
		IsUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(typeFromHandle);
	}
}
