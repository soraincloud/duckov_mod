using System;
using UnityEngine;

namespace Unity.Loading;

public struct ContentFile
{
	internal ulong Id;

	public bool IsValid => ContentLoadInterface.ContentFile_IsHandleValid(this);

	public LoadingStatus LoadingStatus
	{
		get
		{
			ThrowIfInvalidHandle();
			return ContentLoadInterface.ContentFile_GetLoadingStatus(this);
		}
	}

	public static ContentFile GlobalTableDependency => new ContentFile
	{
		Id = 1uL
	};

	public ContentFileUnloadHandle UnloadAsync()
	{
		ThrowIfInvalidHandle();
		ContentLoadInterface.ContentFile_UnloadAsync(this);
		return new ContentFileUnloadHandle
		{
			Id = this
		};
	}

	public UnityEngine.Object[] GetObjects()
	{
		ThrowIfNotComplete();
		return ContentLoadInterface.ContentFile_GetObjects(this);
	}

	public UnityEngine.Object GetObject(ulong localIdentifierInFile)
	{
		ThrowIfNotComplete();
		return ContentLoadInterface.ContentFile_GetObject(this, localIdentifierInFile);
	}

	private void ThrowIfInvalidHandle()
	{
		if (!IsValid)
		{
			throw new Exception("The ContentFile operation cannot be performed because the handle is invalid. Did you already unload it?");
		}
	}

	private void ThrowIfNotComplete()
	{
		switch (LoadingStatus)
		{
		case LoadingStatus.Failed:
			throw new Exception("Cannot use a failed ContentFile operation.");
		case LoadingStatus.InProgress:
			throw new Exception("This ContentFile functionality is not supported while loading is in progress");
		}
	}

	public bool WaitForCompletion(int timeoutMs)
	{
		ThrowIfInvalidHandle();
		return ContentLoadInterface.WaitForLoadCompletion(this, timeoutMs);
	}
}
