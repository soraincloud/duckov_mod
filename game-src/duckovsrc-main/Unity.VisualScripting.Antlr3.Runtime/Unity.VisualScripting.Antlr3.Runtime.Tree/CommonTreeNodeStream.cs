using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting.Antlr3.Runtime.Collections;

namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

public class CommonTreeNodeStream : ITreeNodeStream, IIntStream, IEnumerable
{
	protected sealed class CommonTreeNodeStreamEnumerator : IEnumerator
	{
		private CommonTreeNodeStream _nodeStream;

		private int _index;

		private object _currentItem;

		public object Current
		{
			get
			{
				if (_currentItem == null)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return _currentItem;
			}
		}

		internal CommonTreeNodeStreamEnumerator()
		{
		}

		internal CommonTreeNodeStreamEnumerator(CommonTreeNodeStream nodeStream)
		{
			_nodeStream = nodeStream;
			Reset();
		}

		public void Reset()
		{
			_index = 0;
			_currentItem = null;
		}

		public bool MoveNext()
		{
			if (_index >= _nodeStream.nodes.Count)
			{
				int index = _index;
				_index++;
				if (index < _nodeStream.nodes.Count)
				{
					_currentItem = _nodeStream.nodes[index];
				}
				_currentItem = _nodeStream.eof;
				return true;
			}
			_currentItem = null;
			return false;
		}
	}

	public const int DEFAULT_INITIAL_BUFFER_SIZE = 100;

	public const int INITIAL_CALL_STACK_SIZE = 10;

	protected object down;

	protected object up;

	protected object eof;

	protected IList nodes;

	protected internal object root;

	protected ITokenStream tokens;

	private ITreeAdaptor adaptor;

	protected bool uniqueNavigationNodes;

	protected int p = -1;

	protected int lastMarker;

	protected StackList calls;

	public virtual object CurrentSymbol => LT(1);

	public virtual object TreeSource => root;

	public virtual string SourceName => TokenStream.SourceName;

	public virtual ITokenStream TokenStream
	{
		get
		{
			return tokens;
		}
		set
		{
			tokens = value;
		}
	}

	public ITreeAdaptor TreeAdaptor
	{
		get
		{
			return adaptor;
		}
		set
		{
			adaptor = value;
		}
	}

	public bool HasUniqueNavigationNodes
	{
		get
		{
			return uniqueNavigationNodes;
		}
		set
		{
			uniqueNavigationNodes = value;
		}
	}

	public virtual int Count
	{
		get
		{
			if (p == -1)
			{
				FillBuffer();
			}
			return nodes.Count;
		}
	}

	public IEnumerator GetEnumerator()
	{
		if (p == -1)
		{
			FillBuffer();
		}
		return new CommonTreeNodeStreamEnumerator(this);
	}

	public CommonTreeNodeStream(object tree)
		: this(new CommonTreeAdaptor(), tree)
	{
	}

	public CommonTreeNodeStream(ITreeAdaptor adaptor, object tree)
		: this(adaptor, tree, 100)
	{
	}

	public CommonTreeNodeStream(ITreeAdaptor adaptor, object tree, int initialBufferSize)
	{
		root = tree;
		this.adaptor = adaptor;
		nodes = new List<object>(initialBufferSize);
		down = adaptor.Create(2, "DOWN");
		up = adaptor.Create(3, "UP");
		eof = adaptor.Create(Token.EOF, "EOF");
	}

	protected void FillBuffer()
	{
		FillBuffer(root);
		p = 0;
	}

	public void FillBuffer(object t)
	{
		bool flag = adaptor.IsNil(t);
		if (!flag)
		{
			nodes.Add(t);
		}
		int childCount = adaptor.GetChildCount(t);
		if (!flag && childCount > 0)
		{
			AddNavigationNode(2);
		}
		for (int i = 0; i < childCount; i++)
		{
			object child = adaptor.GetChild(t, i);
			FillBuffer(child);
		}
		if (!flag && childCount > 0)
		{
			AddNavigationNode(3);
		}
	}

	protected int GetNodeIndex(object node)
	{
		if (p == -1)
		{
			FillBuffer();
		}
		for (int i = 0; i < nodes.Count; i++)
		{
			object obj = nodes[i];
			if (obj == node)
			{
				return i;
			}
		}
		return -1;
	}

	protected void AddNavigationNode(int ttype)
	{
		object obj = null;
		obj = ((ttype == 2) ? ((!HasUniqueNavigationNodes) ? down : adaptor.Create(2, "DOWN")) : ((!HasUniqueNavigationNodes) ? up : adaptor.Create(3, "UP")));
		nodes.Add(obj);
	}

	public object Get(int i)
	{
		if (p == -1)
		{
			FillBuffer();
		}
		return nodes[i];
	}

	public object LT(int k)
	{
		if (p == -1)
		{
			FillBuffer();
		}
		if (k == 0)
		{
			return null;
		}
		if (k < 0)
		{
			return LB(-k);
		}
		if (p + k - 1 >= nodes.Count)
		{
			return eof;
		}
		return nodes[p + k - 1];
	}

	protected object LB(int k)
	{
		if (k == 0)
		{
			return null;
		}
		if (p - k < 0)
		{
			return null;
		}
		return nodes[p - k];
	}

	public void Push(int index)
	{
		if (calls == null)
		{
			calls = new StackList();
		}
		calls.Push(p);
		Seek(index);
	}

	public int Pop()
	{
		int num = (int)calls.Pop();
		Seek(num);
		return num;
	}

	public void Reset()
	{
		p = -1;
		lastMarker = 0;
		if (calls != null)
		{
			calls.Clear();
		}
	}

	public void ReplaceChildren(object parent, int startChildIndex, int stopChildIndex, object t)
	{
		if (parent != null)
		{
			adaptor.ReplaceChildren(parent, startChildIndex, stopChildIndex, t);
		}
	}

	public virtual void Consume()
	{
		if (p == -1)
		{
			FillBuffer();
		}
		p++;
	}

	public virtual int LA(int i)
	{
		return adaptor.GetNodeType(LT(i));
	}

	public virtual int Mark()
	{
		if (p == -1)
		{
			FillBuffer();
		}
		lastMarker = Index();
		return lastMarker;
	}

	public virtual void Release(int marker)
	{
	}

	public virtual void Rewind(int marker)
	{
		Seek(marker);
	}

	public void Rewind()
	{
		Seek(lastMarker);
	}

	public virtual void Seek(int index)
	{
		if (p == -1)
		{
			FillBuffer();
		}
		p = index;
	}

	public virtual int Index()
	{
		return p;
	}

	[Obsolete("Please use property Count instead.")]
	public virtual int Size()
	{
		return Count;
	}

	public override string ToString()
	{
		if (p == -1)
		{
			FillBuffer();
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < nodes.Count; i++)
		{
			object t = nodes[i];
			stringBuilder.Append(" ");
			stringBuilder.Append(adaptor.GetNodeType(t));
		}
		return stringBuilder.ToString();
	}

	public string ToTokenString(int start, int stop)
	{
		if (p == -1)
		{
			FillBuffer();
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = start; i < nodes.Count && i <= stop; i++)
		{
			object treeNode = nodes[i];
			stringBuilder.Append(" ");
			stringBuilder.Append(adaptor.GetToken(treeNode));
		}
		return stringBuilder.ToString();
	}

	public virtual string ToString(object start, object stop)
	{
		Console.Out.WriteLine("ToString");
		if (start == null || stop == null)
		{
			return null;
		}
		if (p == -1)
		{
			FillBuffer();
		}
		if (start is CommonTree)
		{
			Console.Out.Write(string.Concat("ToString: ", ((CommonTree)start).Token, ", "));
		}
		else
		{
			Console.Out.WriteLine(start);
		}
		if (stop is CommonTree)
		{
			Console.Out.WriteLine(((CommonTree)stop).Token);
		}
		else
		{
			Console.Out.WriteLine(stop);
		}
		if (tokens != null)
		{
			int tokenStartIndex = adaptor.GetTokenStartIndex(start);
			int num = adaptor.GetTokenStopIndex(stop);
			if (adaptor.GetNodeType(stop) == 3)
			{
				num = adaptor.GetTokenStopIndex(start);
			}
			else if (adaptor.GetNodeType(stop) == Token.EOF)
			{
				num = Count - 2;
			}
			return tokens.ToString(tokenStartIndex, num);
		}
		object obj = null;
		int i;
		for (i = 0; i < nodes.Count; i++)
		{
			obj = nodes[i];
			if (obj == start)
			{
				break;
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		string text;
		for (obj = nodes[i]; obj != stop; obj = nodes[i])
		{
			text = adaptor.GetNodeText(obj);
			if (text == null)
			{
				text = " " + adaptor.GetNodeType(obj);
			}
			stringBuilder.Append(text);
			i++;
		}
		text = adaptor.GetNodeText(stop);
		if (text == null)
		{
			text = " " + adaptor.GetNodeType(stop);
		}
		stringBuilder.Append(text);
		return stringBuilder.ToString();
	}
}
