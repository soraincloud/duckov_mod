using System;
using UnityEngine.SceneManagement;

namespace Unity.Loading;

public struct ContentSceneFile
{
	internal ulong Id;

	public Scene Scene
	{
		get
		{
			ThrowIfInvalidHandle();
			return ContentLoadInterface.ContentSceneFile_GetScene(this);
		}
	}

	public SceneLoadingStatus Status
	{
		get
		{
			ThrowIfInvalidHandle();
			return ContentLoadInterface.ContentSceneFile_GetStatus(this);
		}
	}

	public bool IsValid => ContentLoadInterface.ContentSceneFile_IsHandleValid(this);

	public void IntegrateAtEndOfFrame()
	{
		ThrowIfInvalidHandle();
		ContentLoadInterface.ContentSceneFile_IntegrateAtEndOfFrame(this);
	}

	public bool UnloadAtEndOfFrame()
	{
		ThrowIfInvalidHandle();
		return ContentLoadInterface.ContentSceneFile_UnloadAtEndOfFrame(this);
	}

	public bool WaitForLoadCompletion(int timeoutMs)
	{
		ThrowIfInvalidHandle();
		return ContentLoadInterface.ContentSceneFile_WaitForCompletion(this, timeoutMs);
	}

	private void ThrowIfInvalidHandle()
	{
		if (!IsValid)
		{
			throw new Exception("The ContentSceneFile operation cannot be performed because the handle is invalid. Did you already unload it?");
		}
	}
}
