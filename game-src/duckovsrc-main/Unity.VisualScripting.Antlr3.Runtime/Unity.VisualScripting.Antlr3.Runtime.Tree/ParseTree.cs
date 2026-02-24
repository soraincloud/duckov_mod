using System.Collections;
using System.Text;

namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

public class ParseTree : BaseTree
{
	public object payload;

	public IList hiddenTokens;

	public override int Type => 0;

	public override string Text => ToString();

	public override int TokenStartIndex
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public override int TokenStopIndex
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public ParseTree(object label)
	{
		payload = label;
	}

	public override ITree DupNode()
	{
		return null;
	}

	public override string ToString()
	{
		if (payload is IToken)
		{
			IToken token = (IToken)payload;
			if (token.Type == Token.EOF)
			{
				return "<EOF>";
			}
			return token.Text;
		}
		return payload.ToString();
	}

	public string ToStringWithHiddenTokens()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (hiddenTokens != null)
		{
			for (int i = 0; i < hiddenTokens.Count; i++)
			{
				IToken token = (IToken)hiddenTokens[i];
				stringBuilder.Append(token.Text);
			}
		}
		string text = ToString();
		if (text != "<EOF>")
		{
			stringBuilder.Append(text);
		}
		return stringBuilder.ToString();
	}

	public string ToInputString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		_ToStringLeaves(stringBuilder);
		return stringBuilder.ToString();
	}

	public void _ToStringLeaves(StringBuilder buf)
	{
		if (payload is IToken)
		{
			buf.Append(ToStringWithHiddenTokens());
			return;
		}
		int num = 0;
		while (children != null && num < children.Count)
		{
			ParseTree parseTree = (ParseTree)children[num];
			parseTree._ToStringLeaves(buf);
			num++;
		}
	}
}
