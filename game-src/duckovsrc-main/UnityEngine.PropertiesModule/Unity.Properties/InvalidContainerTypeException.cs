using System;

namespace Unity.Properties;

[Serializable]
public class InvalidContainerTypeException : Exception
{
	public Type Type { get; }

	public InvalidContainerTypeException(Type type)
		: base(GetMessageForType(type))
	{
		Type = type;
	}

	public InvalidContainerTypeException(Type type, Exception inner)
		: base(GetMessageForType(type), inner)
	{
		Type = type;
	}

	private static string GetMessageForType(Type type)
	{
		return "Invalid container Type=[" + type.Name + "." + type.Name + "]";
	}
}
