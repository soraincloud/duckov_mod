using System;
using System.Collections;

namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

public abstract class BaseTreeAdaptor : ITreeAdaptor
{
	protected IDictionary treeToUniqueIDMap;

	protected int uniqueNodeID = 1;

	public virtual object GetNilNode()
	{
		return Create(null);
	}

	public virtual object ErrorNode(ITokenStream input, IToken start, IToken stop, RecognitionException e)
	{
		return new CommonErrorNode(input, start, stop, e);
	}

	public virtual bool IsNil(object tree)
	{
		return ((ITree)tree).IsNil;
	}

	public virtual object DupTree(object tree)
	{
		return DupTree(tree, null);
	}

	public virtual object DupTree(object t, object parent)
	{
		if (t == null)
		{
			return null;
		}
		object obj = DupNode(t);
		SetChildIndex(obj, GetChildIndex(t));
		SetParent(obj, parent);
		int childCount = GetChildCount(t);
		for (int i = 0; i < childCount; i++)
		{
			object child = GetChild(t, i);
			object child2 = DupTree(child, t);
			AddChild(obj, child2);
		}
		return obj;
	}

	public virtual void AddChild(object t, object child)
	{
		if (t != null && child != null)
		{
			((ITree)t).AddChild((ITree)child);
		}
	}

	public virtual object BecomeRoot(object newRoot, object oldRoot)
	{
		ITree tree = (ITree)newRoot;
		ITree t = (ITree)oldRoot;
		if (oldRoot == null)
		{
			return newRoot;
		}
		if (tree.IsNil)
		{
			int childCount = tree.ChildCount;
			if (childCount == 1)
			{
				tree = tree.GetChild(0);
			}
			else if (childCount > 1)
			{
				throw new SystemException("more than one node as root (TODO: make exception hierarchy)");
			}
		}
		tree.AddChild(t);
		return tree;
	}

	public virtual object RulePostProcessing(object root)
	{
		ITree tree = (ITree)root;
		if (tree != null && tree.IsNil)
		{
			if (tree.ChildCount == 0)
			{
				tree = null;
			}
			else if (tree.ChildCount == 1)
			{
				tree = tree.GetChild(0);
				tree.Parent = null;
				tree.ChildIndex = -1;
			}
		}
		return tree;
	}

	public virtual object BecomeRoot(IToken newRoot, object oldRoot)
	{
		return BecomeRoot(Create(newRoot), oldRoot);
	}

	public virtual object Create(int tokenType, IToken fromToken)
	{
		fromToken = CreateToken(fromToken);
		fromToken.Type = tokenType;
		return (ITree)Create(fromToken);
	}

	public virtual object Create(int tokenType, IToken fromToken, string text)
	{
		fromToken = CreateToken(fromToken);
		fromToken.Type = tokenType;
		fromToken.Text = text;
		return (ITree)Create(fromToken);
	}

	public virtual object Create(int tokenType, string text)
	{
		IToken param = CreateToken(tokenType, text);
		return (ITree)Create(param);
	}

	public virtual int GetNodeType(object t)
	{
		return ((ITree)t).Type;
	}

	public virtual void SetNodeType(object t, int type)
	{
		throw new NotImplementedException("don't know enough about Tree node");
	}

	public virtual string GetNodeText(object t)
	{
		return ((ITree)t).Text;
	}

	public virtual void SetNodeText(object t, string text)
	{
		throw new NotImplementedException("don't know enough about Tree node");
	}

	public virtual object GetChild(object t, int i)
	{
		return ((ITree)t).GetChild(i);
	}

	public virtual void SetChild(object t, int i, object child)
	{
		((ITree)t).SetChild(i, (ITree)child);
	}

	public virtual object DeleteChild(object t, int i)
	{
		return ((ITree)t).DeleteChild(i);
	}

	public virtual int GetChildCount(object t)
	{
		return ((ITree)t).ChildCount;
	}

	public abstract object DupNode(object param1);

	public abstract object Create(IToken param1);

	public abstract void SetTokenBoundaries(object param1, IToken param2, IToken param3);

	public abstract int GetTokenStartIndex(object t);

	public abstract int GetTokenStopIndex(object t);

	public abstract IToken GetToken(object treeNode);

	public int GetUniqueID(object node)
	{
		if (treeToUniqueIDMap == null)
		{
			treeToUniqueIDMap = new Hashtable();
		}
		object obj = treeToUniqueIDMap[node];
		if (obj != null)
		{
			return (int)obj;
		}
		int num = uniqueNodeID;
		treeToUniqueIDMap[node] = num;
		uniqueNodeID++;
		return num;
	}

	public abstract IToken CreateToken(int tokenType, string text);

	public abstract IToken CreateToken(IToken fromToken);

	public abstract object GetParent(object t);

	public abstract void SetParent(object t, object parent);

	public abstract int GetChildIndex(object t);

	public abstract void SetChildIndex(object t, int index);

	public abstract void ReplaceChildren(object parent, int startChildIndex, int stopChildIndex, object t);
}
