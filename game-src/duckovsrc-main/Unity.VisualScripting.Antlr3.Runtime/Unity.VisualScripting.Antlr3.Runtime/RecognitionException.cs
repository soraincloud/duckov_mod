using System;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

namespace Unity.VisualScripting.Antlr3.Runtime;

[Serializable]
public class RecognitionException : Exception
{
	[NonSerialized]
	protected IIntStream input;

	protected int index;

	protected IToken token;

	protected object node;

	protected int c;

	protected int line;

	protected int charPositionInLine;

	public bool approximateLineInfo;

	public IIntStream Input
	{
		get
		{
			return input;
		}
		set
		{
			input = value;
		}
	}

	public int Index
	{
		get
		{
			return index;
		}
		set
		{
			index = value;
		}
	}

	public IToken Token
	{
		get
		{
			return token;
		}
		set
		{
			token = value;
		}
	}

	public object Node
	{
		get
		{
			return node;
		}
		set
		{
			node = value;
		}
	}

	public int Char
	{
		get
		{
			return c;
		}
		set
		{
			c = value;
		}
	}

	public int CharPositionInLine
	{
		get
		{
			return charPositionInLine;
		}
		set
		{
			charPositionInLine = value;
		}
	}

	public int Line
	{
		get
		{
			return line;
		}
		set
		{
			line = value;
		}
	}

	public virtual int UnexpectedType
	{
		get
		{
			if (input is ITokenStream)
			{
				return token.Type;
			}
			if (input is ITreeNodeStream)
			{
				ITreeNodeStream treeNodeStream = (ITreeNodeStream)input;
				ITreeAdaptor treeAdaptor = treeNodeStream.TreeAdaptor;
				return treeAdaptor.GetNodeType(node);
			}
			return c;
		}
	}

	public RecognitionException()
		: this(null, null, null)
	{
	}

	public RecognitionException(string message)
		: this(message, null, null)
	{
	}

	public RecognitionException(string message, Exception inner)
		: this(message, inner, null)
	{
	}

	public RecognitionException(IIntStream input)
		: this(null, null, input)
	{
	}

	public RecognitionException(string message, IIntStream input)
		: this(message, null, input)
	{
	}

	public RecognitionException(string message, Exception inner, IIntStream input)
		: base(message, inner)
	{
		this.input = input;
		index = input.Index();
		if (input is ITokenStream)
		{
			token = ((ITokenStream)input).LT(1);
			line = token.Line;
			charPositionInLine = token.CharPositionInLine;
		}
		if (input is ITreeNodeStream)
		{
			ExtractInformationFromTreeNodeStream(input);
		}
		else if (input is ICharStream)
		{
			c = input.LA(1);
			line = ((ICharStream)input).Line;
			charPositionInLine = ((ICharStream)input).CharPositionInLine;
		}
		else
		{
			c = input.LA(1);
		}
	}

	protected void ExtractInformationFromTreeNodeStream(IIntStream input)
	{
		ITreeNodeStream treeNodeStream = (ITreeNodeStream)input;
		node = treeNodeStream.LT(1);
		ITreeAdaptor treeAdaptor = treeNodeStream.TreeAdaptor;
		IToken token = treeAdaptor.GetToken(node);
		if (token != null)
		{
			this.token = token;
			if (token.Line <= 0)
			{
				int num = -1;
				for (object obj = treeNodeStream.LT(num); obj != null; obj = treeNodeStream.LT(num))
				{
					IToken token2 = treeAdaptor.GetToken(obj);
					if (token2 != null && token2.Line > 0)
					{
						line = token2.Line;
						charPositionInLine = token2.CharPositionInLine;
						approximateLineInfo = true;
						break;
					}
					num--;
				}
			}
			else
			{
				line = token.Line;
				charPositionInLine = token.CharPositionInLine;
			}
		}
		else if (node is ITree)
		{
			line = ((ITree)node).Line;
			charPositionInLine = ((ITree)node).CharPositionInLine;
			if (node is CommonTree)
			{
				this.token = ((CommonTree)node).Token;
			}
		}
		else
		{
			int nodeType = treeAdaptor.GetNodeType(node);
			string nodeText = treeAdaptor.GetNodeText(node);
			this.token = new CommonToken(nodeType, nodeText);
		}
	}
}
