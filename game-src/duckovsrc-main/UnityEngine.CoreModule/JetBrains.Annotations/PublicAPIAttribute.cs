using System;

namespace JetBrains.Annotations;

[MeansImplicitUse(ImplicitUseTargetFlags.WithMembers)]
[AttributeUsage(AttributeTargets.All, Inherited = false)]
public sealed class PublicAPIAttribute : Attribute
{
	[CanBeNull]
	public string Comment { get; }

	public PublicAPIAttribute()
	{
	}

	public PublicAPIAttribute([NotNull] string comment)
	{
		Comment = comment;
	}
}
