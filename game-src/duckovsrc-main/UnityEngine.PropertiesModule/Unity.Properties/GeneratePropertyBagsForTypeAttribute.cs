using System;

namespace Unity.Properties;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GeneratePropertyBagsForTypeAttribute : Attribute
{
	public Type Type { get; }

	public GeneratePropertyBagsForTypeAttribute(Type type)
	{
		if (!TypeTraits.IsContainer(type))
		{
			throw new ArgumentException(type.Name + " is not a valid container type.");
		}
		Type = type;
	}
}
