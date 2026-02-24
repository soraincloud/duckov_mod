using System;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

namespace Unity.VisualScripting.Antlr3.Runtime;

[Serializable]
public class CommonErrorNode : CommonTree
{
	public IIntStream input;

	public IToken start;

	public IToken stop;

	[NonSerialized]
	public RecognitionException trappedException;

	public override bool IsNil => false;

	public override int Type => 0;

	public override string Text
	{
		get
		{
			string text = null;
			if (start != null)
			{
				int tokenIndex = start.TokenIndex;
				int num = stop.TokenIndex;
				if (stop.Type == Unity.VisualScripting.Antlr3.Runtime.Token.EOF)
				{
					num = ((ITokenStream)input).Count;
				}
				return ((ITokenStream)input).ToString(tokenIndex, num);
			}
			if (start is ITree)
			{
				return ((ITreeNodeStream)input).ToString(start, stop);
			}
			return "<unknown>";
		}
	}

	public CommonErrorNode(ITokenStream input, IToken start, IToken stop, RecognitionException e)
	{
		if (stop == null || (stop.TokenIndex < start.TokenIndex && stop.Type != Unity.VisualScripting.Antlr3.Runtime.Token.EOF))
		{
			stop = start;
		}
		this.input = input;
		this.start = start;
		this.stop = stop;
		trappedException = e;
	}

	public override string ToString()
	{
		if (trappedException is MissingTokenException)
		{
			return "<missing type: " + ((MissingTokenException)trappedException).MissingType + ">";
		}
		if (trappedException is UnwantedTokenException)
		{
			return string.Concat("<extraneous: ", ((UnwantedTokenException)trappedException).UnexpectedToken, ", resync=", Text, ">");
		}
		if (trappedException is MismatchedTokenException)
		{
			return string.Concat("<mismatched token: ", trappedException.Token, ", resync=", Text, ">");
		}
		if (trappedException is NoViableAltException)
		{
			return string.Concat("<unexpected: ", trappedException.Token, ", resync=", Text, ">");
		}
		return "<error: " + Text + ">";
	}
}
