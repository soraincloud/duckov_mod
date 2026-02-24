using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Parameter)]
public sealed class MustDisposeResourceAttribute : Attribute
{
	public bool Value { get; }

	public MustDisposeResourceAttribute()
	{
		Value = true;
	}

	public MustDisposeResourceAttribute(bool value)
	{
		Value = value;
	}
}
