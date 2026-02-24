namespace UnityEngine.UIElements;

internal interface IGroupBox
{
	void OnOptionAdded(IGroupBoxOption option);

	void OnOptionRemoved(IGroupBoxOption option);
}
internal interface IGroupBox<T> : IGroupBox where T : IGroupManager
{
}
