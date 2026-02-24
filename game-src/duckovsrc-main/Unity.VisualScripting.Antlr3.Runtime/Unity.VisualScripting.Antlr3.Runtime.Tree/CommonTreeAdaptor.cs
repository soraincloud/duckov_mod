namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

public class CommonTreeAdaptor : BaseTreeAdaptor
{
	public override object DupNode(object t)
	{
		if (t == null)
		{
			return null;
		}
		return ((ITree)t).DupNode();
	}

	public override object Create(IToken payload)
	{
		return new CommonTree(payload);
	}

	public override IToken CreateToken(int tokenType, string text)
	{
		return new CommonToken(tokenType, text);
	}

	public override IToken CreateToken(IToken fromToken)
	{
		return new CommonToken(fromToken);
	}

	public override void SetTokenBoundaries(object t, IToken startToken, IToken stopToken)
	{
		if (t != null)
		{
			int tokenStartIndex = 0;
			int tokenStopIndex = 0;
			if (startToken != null)
			{
				tokenStartIndex = startToken.TokenIndex;
			}
			if (stopToken != null)
			{
				tokenStopIndex = stopToken.TokenIndex;
			}
			((ITree)t).TokenStartIndex = tokenStartIndex;
			((ITree)t).TokenStopIndex = tokenStopIndex;
		}
	}

	public override int GetTokenStartIndex(object t)
	{
		if (t == null)
		{
			return -1;
		}
		return ((ITree)t).TokenStartIndex;
	}

	public override int GetTokenStopIndex(object t)
	{
		if (t == null)
		{
			return -1;
		}
		return ((ITree)t).TokenStopIndex;
	}

	public override string GetNodeText(object t)
	{
		if (t == null)
		{
			return null;
		}
		return ((ITree)t).Text;
	}

	public override int GetNodeType(object t)
	{
		if (t == null)
		{
			return 0;
		}
		return ((ITree)t).Type;
	}

	public override IToken GetToken(object treeNode)
	{
		if (treeNode is CommonTree)
		{
			return ((CommonTree)treeNode).Token;
		}
		return null;
	}

	public override object GetChild(object t, int i)
	{
		if (t == null)
		{
			return null;
		}
		return ((ITree)t).GetChild(i);
	}

	public override int GetChildCount(object t)
	{
		if (t == null)
		{
			return 0;
		}
		return ((ITree)t).ChildCount;
	}

	public override object GetParent(object t)
	{
		if (t == null)
		{
			return null;
		}
		return ((ITree)t).Parent;
	}

	public override void SetParent(object t, object parent)
	{
		if (t == null)
		{
			((ITree)t).Parent = (ITree)parent;
		}
	}

	public override int GetChildIndex(object t)
	{
		if (t == null)
		{
			return 0;
		}
		return ((ITree)t).ChildIndex;
	}

	public override void SetChildIndex(object t, int index)
	{
		if (t == null)
		{
			((ITree)t).ChildIndex = index;
		}
	}

	public override void ReplaceChildren(object parent, int startChildIndex, int stopChildIndex, object t)
	{
		if (parent != null)
		{
			((ITree)parent).ReplaceChildren(startChildIndex, stopChildIndex, t);
		}
	}
}
