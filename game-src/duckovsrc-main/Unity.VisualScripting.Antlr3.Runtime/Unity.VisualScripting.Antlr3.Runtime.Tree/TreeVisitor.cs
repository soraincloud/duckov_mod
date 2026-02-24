namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

public class TreeVisitor
{
	protected ITreeAdaptor adaptor;

	public TreeVisitor(ITreeAdaptor adaptor)
	{
		this.adaptor = adaptor;
	}

	public TreeVisitor()
		: this(new CommonTreeAdaptor())
	{
	}

	public object Visit(object t, ITreeVisitorAction action)
	{
		bool flag = adaptor.IsNil(t);
		if (action != null && !flag)
		{
			t = action.Pre(t);
		}
		int childCount = adaptor.GetChildCount(t);
		for (int i = 0; i < childCount; i++)
		{
			object child = adaptor.GetChild(t, i);
			object obj = Visit(child, action);
			object child2 = adaptor.GetChild(t, i);
			if (obj != child2)
			{
				adaptor.SetChild(t, i, obj);
			}
		}
		if (action != null && !flag)
		{
			t = action.Post(t);
		}
		return t;
	}
}
