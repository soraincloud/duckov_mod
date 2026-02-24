using System.Collections.Generic;

namespace Unity.VisualScripting.Antlr3.Runtime.Collections;

public class StackList : List<object>
{
	public void Push(object item)
	{
		Add(item);
	}

	public object Pop()
	{
		object result = base[base.Count - 1];
		RemoveAt(base.Count - 1);
		return result;
	}

	public object Peek()
	{
		return base[base.Count - 1];
	}
}
