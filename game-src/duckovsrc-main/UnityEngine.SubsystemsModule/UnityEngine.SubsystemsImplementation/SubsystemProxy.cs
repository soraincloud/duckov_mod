namespace UnityEngine.SubsystemsImplementation;

public class SubsystemProxy<TSubsystem, TProvider> where TSubsystem : SubsystemWithProvider, new() where TProvider : SubsystemProvider<TSubsystem>
{
	public TProvider provider { get; private set; }

	public bool running
	{
		get
		{
			return provider.running;
		}
		set
		{
			provider.m_Running = value;
		}
	}

	internal SubsystemProxy(TProvider provider)
	{
		this.provider = provider;
	}
}
