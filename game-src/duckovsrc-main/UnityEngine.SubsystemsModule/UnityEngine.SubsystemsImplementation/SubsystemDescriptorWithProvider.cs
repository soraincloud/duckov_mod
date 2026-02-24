using System;

namespace UnityEngine.SubsystemsImplementation;

public abstract class SubsystemDescriptorWithProvider : ISubsystemDescriptor
{
	public string id { get; set; }

	protected internal Type providerType { get; set; }

	protected internal Type subsystemTypeOverride { get; set; }

	internal abstract ISubsystem CreateImpl();

	ISubsystem ISubsystemDescriptor.Create()
	{
		return CreateImpl();
	}

	internal abstract void ThrowIfInvalid();
}
public class SubsystemDescriptorWithProvider<TSubsystem, TProvider> : SubsystemDescriptorWithProvider where TSubsystem : SubsystemWithProvider, new() where TProvider : SubsystemProvider<TSubsystem>
{
	internal override ISubsystem CreateImpl()
	{
		return Create();
	}

	public TSubsystem Create()
	{
		if (SubsystemManager.FindStandaloneSubsystemByDescriptor(this) is TSubsystem result)
		{
			return result;
		}
		TProvider val = CreateProvider();
		if (val == null)
		{
			return null;
		}
		TSubsystem val2 = ((base.subsystemTypeOverride != null) ? ((TSubsystem)Activator.CreateInstance(base.subsystemTypeOverride)) : new TSubsystem());
		val2.Initialize(this, val);
		SubsystemManager.AddStandaloneSubsystem(val2);
		return val2;
	}

	internal sealed override void ThrowIfInvalid()
	{
		if (base.providerType == null)
		{
			throw new InvalidOperationException("Invalid descriptor - must supply a valid providerType field!");
		}
		if (!base.providerType.IsSubclassOf(typeof(TProvider)))
		{
			throw new InvalidOperationException($"Can't create provider - providerType '{base.providerType.ToString()}' is not a subclass of '{typeof(TProvider).ToString()}'!");
		}
		if (base.subsystemTypeOverride != null && !base.subsystemTypeOverride.IsSubclassOf(typeof(TSubsystem)))
		{
			throw new InvalidOperationException($"Can't create provider - subsystemTypeOverride '{base.subsystemTypeOverride.ToString()}' is not a subclass of '{typeof(TSubsystem).ToString()}'!");
		}
	}

	internal TProvider CreateProvider()
	{
		TProvider val = (TProvider)Activator.CreateInstance(base.providerType);
		return val.TryInitialize() ? val : null;
	}
}
