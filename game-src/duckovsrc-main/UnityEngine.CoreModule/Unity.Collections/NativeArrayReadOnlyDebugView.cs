namespace Unity.Collections;

internal sealed class NativeArrayReadOnlyDebugView<T> where T : struct
{
	private NativeArray<T>.ReadOnly m_Array;

	public T[] Items
	{
		get
		{
			if (!m_Array.IsCreated)
			{
				return null;
			}
			return m_Array.ToArray();
		}
	}

	public NativeArrayReadOnlyDebugView(NativeArray<T>.ReadOnly array)
	{
		m_Array = array;
	}
}
