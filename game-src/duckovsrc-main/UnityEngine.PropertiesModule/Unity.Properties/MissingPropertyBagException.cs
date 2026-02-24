using System;

namespace Unity.Properties;

[Serializable]
public class MissingPropertyBagException : Exception
{
	public Type Type { get; }

	public MissingPropertyBagException(Type type)
		: base(GetMessageForType(type))
	{
		Type = type;
	}

	public MissingPropertyBagException(Type type, Exception inner)
		: base(GetMessageForType(type), inner)
	{
		Type = type;
	}

	private static string GetMessageForType(Type type)
	{
		return "No PropertyBag was found for Type=[" + type.FullName + "]. Please make sure all types are declared ahead of time using [GeneratePropertyBagAttribute], [GeneratePropertyBagsForTypeAttribute] or [GeneratePropertyBagsForTypesQualifiedWithAttribute]";
	}
}
