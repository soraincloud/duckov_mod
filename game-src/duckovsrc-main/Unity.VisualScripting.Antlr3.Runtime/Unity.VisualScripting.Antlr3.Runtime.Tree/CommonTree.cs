using System;

namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

[Serializable]
public class CommonTree : BaseTree
{
	public int startIndex = -1;

	public int stopIndex = -1;

	protected IToken token;

	public CommonTree parent;

	public int childIndex = -1;

	public virtual IToken Token => token;

	public override bool IsNil => token == null;

	public override int Type
	{
		get
		{
			if (token == null)
			{
				return 0;
			}
			return token.Type;
		}
	}

	public override string Text
	{
		get
		{
			if (token == null)
			{
				return null;
			}
			return token.Text;
		}
	}

	public override int Line
	{
		get
		{
			if (token == null || token.Line == 0)
			{
				if (ChildCount > 0)
				{
					return GetChild(0).Line;
				}
				return 0;
			}
			return token.Line;
		}
	}

	public override int CharPositionInLine
	{
		get
		{
			if (token == null || token.CharPositionInLine == -1)
			{
				if (ChildCount > 0)
				{
					return GetChild(0).CharPositionInLine;
				}
				return 0;
			}
			return token.CharPositionInLine;
		}
	}

	public override int TokenStartIndex
	{
		get
		{
			if (startIndex == -1 && token != null)
			{
				return token.TokenIndex;
			}
			return startIndex;
		}
		set
		{
			startIndex = value;
		}
	}

	public override int TokenStopIndex
	{
		get
		{
			if (stopIndex == -1 && token != null)
			{
				return token.TokenIndex;
			}
			return stopIndex;
		}
		set
		{
			stopIndex = value;
		}
	}

	public override int ChildIndex
	{
		get
		{
			return childIndex;
		}
		set
		{
			childIndex = value;
		}
	}

	public override ITree Parent
	{
		get
		{
			return parent;
		}
		set
		{
			parent = (CommonTree)value;
		}
	}

	public CommonTree()
	{
	}

	public CommonTree(CommonTree node)
		: base(node)
	{
		token = node.token;
		startIndex = node.startIndex;
		stopIndex = node.stopIndex;
	}

	public CommonTree(IToken t)
	{
		token = t;
	}

	public void SetUnknownTokenBoundaries()
	{
		if (children == null)
		{
			if (startIndex < 0 || stopIndex < 0)
			{
				startIndex = (stopIndex = token.TokenIndex);
			}
			return;
		}
		for (int i = 0; i < children.Count; i++)
		{
			((CommonTree)children[i]).SetUnknownTokenBoundaries();
		}
		if ((startIndex < 0 || stopIndex < 0) && children.Count > 0)
		{
			CommonTree commonTree = (CommonTree)children[0];
			CommonTree commonTree2 = (CommonTree)children[children.Count - 1];
			startIndex = commonTree.TokenStartIndex;
			stopIndex = commonTree2.TokenStopIndex;
		}
	}

	public override ITree DupNode()
	{
		return new CommonTree(this);
	}

	public override string ToString()
	{
		if (IsNil)
		{
			return "nil";
		}
		if (Type == 0)
		{
			return "<errornode>";
		}
		if (token == null)
		{
			return null;
		}
		return token.Text;
	}
}
