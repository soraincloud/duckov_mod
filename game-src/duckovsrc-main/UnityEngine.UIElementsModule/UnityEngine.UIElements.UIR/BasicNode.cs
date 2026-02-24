namespace UnityEngine.UIElements.UIR;

internal class BasicNode<T> : LinkedPoolItem<BasicNode<T>>
{
	public BasicNode<T> next;

	public T data;

	public void InsertFirst(ref BasicNode<T> first)
	{
		if (first == null)
		{
			first = this;
			return;
		}
		next = first.next;
		first.next = this;
	}
}
