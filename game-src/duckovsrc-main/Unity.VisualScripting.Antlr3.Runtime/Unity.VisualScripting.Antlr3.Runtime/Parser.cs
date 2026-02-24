namespace Unity.VisualScripting.Antlr3.Runtime;

public class Parser : BaseRecognizer
{
	protected internal ITokenStream input;

	public virtual ITokenStream TokenStream
	{
		get
		{
			return input;
		}
		set
		{
			input = null;
			Reset();
			input = value;
		}
	}

	public override string SourceName => input.SourceName;

	public override IIntStream Input => input;

	public Parser(ITokenStream input)
	{
		TokenStream = input;
	}

	public Parser(ITokenStream input, RecognizerSharedState state)
		: base(state)
	{
		TokenStream = input;
	}

	public override void Reset()
	{
		base.Reset();
		if (input != null)
		{
			input.Seek(0);
		}
	}

	protected override object GetCurrentInputSymbol(IIntStream input)
	{
		return ((ITokenStream)input).LT(1);
	}

	protected override object GetMissingSymbol(IIntStream input, RecognitionException e, int expectedTokenType, BitSet follow)
	{
		string text = null;
		text = ((expectedTokenType != Token.EOF) ? ("<missing " + TokenNames[expectedTokenType] + ">") : "<missing EOF>");
		CommonToken commonToken = new CommonToken(expectedTokenType, text);
		IToken token = ((ITokenStream)input).LT(1);
		if (token.Type == Token.EOF)
		{
			token = ((ITokenStream)input).LT(-1);
		}
		commonToken.line = token.Line;
		commonToken.CharPositionInLine = token.CharPositionInLine;
		commonToken.Channel = 0;
		return commonToken;
	}

	public virtual void TraceIn(string ruleName, int ruleIndex)
	{
		base.TraceIn(ruleName, ruleIndex, input.LT(1));
	}

	public virtual void TraceOut(string ruleName, int ruleIndex)
	{
		base.TraceOut(ruleName, ruleIndex, input.LT(1));
	}
}
