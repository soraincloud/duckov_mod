namespace Unity.Loading;

public struct ContentFileUnloadHandle
{
	internal ContentFile Id;

	public bool IsCompleted => ContentLoadInterface.ContentFile_IsUnloadComplete(Id);

	public bool WaitForCompletion(int timeoutMs)
	{
		return ContentLoadInterface.WaitForUnloadCompletion(Id, timeoutMs);
	}
}
