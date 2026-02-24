using UnityEngine.Bindings;
using UnityEngine.SceneManagement;

namespace Unity.Loading;

public struct ContentSceneParameters
{
	[NativeName("LoadSceneMode")]
	internal LoadSceneMode m_LoadSceneMode;

	[NativeName("LocalPhysicsMode")]
	internal LocalPhysicsMode m_LocalPhysicsMode;

	[NativeName("AutoIntegrate")]
	internal bool m_AutoIntegrate;

	public LoadSceneMode loadSceneMode
	{
		get
		{
			return m_LoadSceneMode;
		}
		set
		{
			m_LoadSceneMode = value;
		}
	}

	public LocalPhysicsMode localPhysicsMode
	{
		get
		{
			return m_LocalPhysicsMode;
		}
		set
		{
			m_LocalPhysicsMode = value;
		}
	}

	public bool autoIntegrate
	{
		get
		{
			return m_AutoIntegrate;
		}
		set
		{
			m_AutoIntegrate = value;
		}
	}
}
