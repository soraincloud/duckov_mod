using System;

namespace Unity.Properties;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GeneratePropertyBagsForTypesQualifiedWithAttribute : Attribute
{
	public Type Type { get; }

	public TypeGenerationOptions Options { get; }

	public GeneratePropertyBagsForTypesQualifiedWithAttribute(Type type, TypeGenerationOptions options = TypeGenerationOptions.Default)
	{
		if (type == null)
		{
			throw new ArgumentException("type is null.");
		}
		if (!type.IsInterface)
		{
			throw new ArgumentException("GeneratePropertyBagsForTypesQualifiedWithAttribute Type must be an interface type.");
		}
		Type = type;
		Options = options;
	}
}
