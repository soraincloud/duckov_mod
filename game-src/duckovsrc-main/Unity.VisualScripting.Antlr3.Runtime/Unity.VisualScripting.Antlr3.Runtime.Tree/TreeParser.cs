using System.Text.RegularExpressions;

namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

public class TreeParser : BaseRecognizer
{
	public const int DOWN = 2;

	public const int UP = 3;

	private static readonly string dotdot = ".*[^.]\\.\\.[^.].*";

	private static readonly string doubleEtc = ".*\\.\\.\\.\\s+\\.\\.\\..*";

	private static readonly string spaces = "\\s+";

	private static readonly Regex dotdotPattern = new Regex(dotdot, RegexOptions.Compiled);

	private static readonly Regex doubleEtcPattern = new Regex(doubleEtc, RegexOptions.Compiled);

	private static readonly Regex spacesPattern = new Regex(spaces, RegexOptions.Compiled);

	protected internal ITreeNodeStream input;

	public virtual ITreeNodeStream TreeNodeStream
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

	public override string SourceName => input.SourceName;

	public override IIntStream Input => input;

	public TreeParser(ITreeNodeStream input)
	{
		TreeNodeStream = input;
	}

	public TreeParser(ITreeNodeStream input, RecognizerSharedState state)
		: base(state)
	{
		TreeNodeStream = input;
	}

	protected override object GetCurrentInputSymbol(IIntStream input)
	{
		return ((ITreeNodeStream)input).LT(1);
	}

	protected override object GetMissingSymbol(IIntStream input, RecognitionException e, int expectedTokenType, BitSet follow)
	{
		string text = "<missing " + TokenNames[expectedTokenType] + ">";
		return new CommonTree(new CommonToken(expectedTokenType, text));
	}

	public override void Reset()
	{
		base.Reset();
		if (input != null)
		{
			input.Seek(0);
		}
	}

	public override void MatchAny(IIntStream ignore)
	{
		state.errorRecovery = false;
		state.failed = false;
		object t = input.LT(1);
		if (input.TreeAdaptor.GetChildCount(t) == 0)
		{
			input.Consume();
			return;
		}
		int num = 0;
		int nodeType = input.TreeAdaptor.GetNodeType(t);
		while (nodeType != Token.EOF && (nodeType != 3 || num != 0))
		{
			input.Consume();
			t = input.LT(1);
			nodeType = input.TreeAdaptor.GetNodeType(t);
			switch (nodeType)
			{
			case 2:
				num++;
				break;
			case 3:
				num--;
				break;
			}
		}
		input.Consume();
	}

	protected internal override object RecoverFromMismatchedToken(IIntStream input, int ttype, BitSet follow)
	{
		throw new MismatchedTreeNodeException(ttype, (ITreeNodeStream)input);
	}

	public override string GetErrorHeader(RecognitionException e)
	{
		return GrammarFileName + ": node from " + (e.approximateLineInfo ? "after " : "") + "line " + e.Line + ":" + e.CharPositionInLine;
	}

	public override string GetErrorMessage(RecognitionException e, string[] tokenNames)
	{
		if (this != null)
		{
			ITreeAdaptor treeAdaptor = ((ITreeNodeStream)e.Input).TreeAdaptor;
			e.Token = treeAdaptor.GetToken(e.Node);
			if (e.Token == null)
			{
				e.Token = new CommonToken(treeAdaptor.GetNodeType(e.Node), treeAdaptor.GetNodeText(e.Node));
			}
		}
		return base.GetErrorMessage(e, tokenNames);
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
