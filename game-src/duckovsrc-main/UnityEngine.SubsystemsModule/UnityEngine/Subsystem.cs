namespace UnityEngine;

public abstract class Subsystem : ISubsystem
{
	internal ISubsystemDescriptor m_SubsystemDescriptor;

	public abstract bool running { get; }

	public abstract void Start();

	public abstract void Stop();

	public void Destroy()
	{
		if (SubsystemManager.RemoveDeprecatedSubsystem(this))
		{
			OnDestroy();
		}
	}

	protected abstract void OnDestroy();
}
public abstract class Subsystem<TSubsystemDescriptor> : Subsystem where TSubsystemDescriptor : ISubsystemDescriptor
{
	public TSubsystemDescriptor SubsystemDescriptor => (TSubsystemDescriptor)m_SubsystemDescriptor;
}
