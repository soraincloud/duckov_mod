using System;

namespace UnityEngine.UIElements.UIR;

internal class BasicNodePool<T> : LinkedPool<BasicNode<T>>
{
	private static void Reset(BasicNode<T> node)
	{
		node.next = null;
		node.data = default(T);
	}

	private static BasicNode<T> Create()
	{
		return new BasicNode<T>();
	}

	public BasicNodePool()
		: base((Func<BasicNode<T>>)Create, (Action<BasicNode<T>>)Reset, 10000)
	{
	}
}
