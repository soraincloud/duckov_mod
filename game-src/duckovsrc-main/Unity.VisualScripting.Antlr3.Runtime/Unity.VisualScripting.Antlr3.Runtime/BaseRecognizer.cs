using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Unity.VisualScripting.Antlr3.Runtime;

public abstract class BaseRecognizer
{
	public const int MEMO_RULE_FAILED = -2;

	public const int MEMO_RULE_UNKNOWN = -1;

	public const int INITIAL_FOLLOW_STACK_SIZE = 100;

	public const int DEFAULT_TOKEN_CHANNEL = 0;

	public const int HIDDEN = 99;

	public static readonly string NEXT_TOKEN_RULE_NAME = "nextToken";

	protected internal RecognizerSharedState state;

	public abstract IIntStream Input { get; }

	public int BacktrackingLevel
	{
		get
		{
			return state.backtracking;
		}
		set
		{
			state.backtracking = value;
		}
	}

	public int NumberOfSyntaxErrors => state.syntaxErrors;

	public virtual string GrammarFileName => null;

	public abstract string SourceName { get; }

	public virtual string[] TokenNames => null;

	public BaseRecognizer()
	{
		state = new RecognizerSharedState();
	}

	public BaseRecognizer(RecognizerSharedState state)
	{
		if (state == null)
		{
			state = new RecognizerSharedState();
		}
		this.state = state;
	}

	public virtual void BeginBacktrack(int level)
	{
	}

	public virtual void EndBacktrack(int level, bool successful)
	{
	}

	public bool Failed()
	{
		return state.failed;
	}

	public virtual void Reset()
	{
		if (state != null)
		{
			state.followingStackPointer = -1;
			state.errorRecovery = false;
			state.lastErrorIndex = -1;
			state.failed = false;
			state.syntaxErrors = 0;
			state.backtracking = 0;
			int num = 0;
			while (state.ruleMemo != null && num < state.ruleMemo.Length)
			{
				state.ruleMemo[num] = null;
				num++;
			}
		}
	}

	public virtual object Match(IIntStream input, int ttype, BitSet follow)
	{
		object currentInputSymbol = GetCurrentInputSymbol(input);
		if (input.LA(1) == ttype)
		{
			input.Consume();
			state.errorRecovery = false;
			state.failed = false;
			return currentInputSymbol;
		}
		if (state.backtracking > 0)
		{
			state.failed = true;
			return currentInputSymbol;
		}
		return RecoverFromMismatchedToken(input, ttype, follow);
	}

	public virtual void MatchAny(IIntStream input)
	{
		state.errorRecovery = false;
		state.failed = false;
		input.Consume();
	}

	public bool MismatchIsUnwantedToken(IIntStream input, int ttype)
	{
		return input.LA(2) == ttype;
	}

	public bool MismatchIsMissingToken(IIntStream input, BitSet follow)
	{
		if (follow == null)
		{
			return false;
		}
		if (follow.Member(1))
		{
			BitSet a = ComputeContextSensitiveRuleFOLLOW();
			follow = follow.Or(a);
			if (state.followingStackPointer >= 0)
			{
				follow.Remove(1);
			}
		}
		if (follow.Member(input.LA(1)) || follow.Member(1))
		{
			return true;
		}
		return false;
	}

	public virtual void ReportError(RecognitionException e)
	{
		if (!state.errorRecovery)
		{
			state.syntaxErrors++;
			state.errorRecovery = true;
			DisplayRecognitionError(TokenNames, e);
		}
	}

	public virtual void DisplayRecognitionError(string[] tokenNames, RecognitionException e)
	{
		string errorHeader = GetErrorHeader(e);
		string errorMessage = GetErrorMessage(e, tokenNames);
		EmitErrorMessage(errorHeader + " " + errorMessage);
	}

	public virtual string GetErrorMessage(RecognitionException e, string[] tokenNames)
	{
		string result = e.Message;
		if (e is UnwantedTokenException)
		{
			UnwantedTokenException ex = (UnwantedTokenException)e;
			string text = "<unknown>";
			result = string.Concat(str3: (ex.Expecting != Token.EOF) ? tokenNames[ex.Expecting] : "EOF", str0: "extraneous input ", str1: GetTokenErrorDisplay(ex.UnexpectedToken), str2: " expecting ");
		}
		else if (e is MissingTokenException)
		{
			MissingTokenException ex2 = (MissingTokenException)e;
			string text2 = "<unknown>";
			text2 = ((ex2.Expecting != Token.EOF) ? tokenNames[ex2.Expecting] : "EOF");
			result = "missing " + text2 + " at " + GetTokenErrorDisplay(e.Token);
		}
		else if (e is MismatchedTokenException)
		{
			MismatchedTokenException ex3 = (MismatchedTokenException)e;
			string text3 = "<unknown>";
			result = string.Concat(str3: (ex3.Expecting != Token.EOF) ? tokenNames[ex3.Expecting] : "EOF", str0: "mismatched input ", str1: GetTokenErrorDisplay(e.Token), str2: " expecting ");
		}
		else if (e is MismatchedTreeNodeException)
		{
			MismatchedTreeNodeException ex4 = (MismatchedTreeNodeException)e;
			string text4 = "<unknown>";
			text4 = ((ex4.expecting != Token.EOF) ? tokenNames[ex4.expecting] : "EOF");
			result = string.Concat("mismatched tree node: ", (ex4.Node != null && ex4.Node.ToString() != null) ? ex4.Node : string.Empty, " expecting ", text4);
		}
		else if (e is NoViableAltException)
		{
			result = "no viable alternative at input " + GetTokenErrorDisplay(e.Token);
		}
		else if (e is EarlyExitException)
		{
			result = "required (...)+ loop did not match anything at input " + GetTokenErrorDisplay(e.Token);
		}
		else if (e is MismatchedSetException)
		{
			MismatchedSetException ex5 = (MismatchedSetException)e;
			result = "mismatched input " + GetTokenErrorDisplay(e.Token) + " expecting set " + ex5.expecting;
		}
		else if (e is MismatchedNotSetException)
		{
			MismatchedNotSetException ex6 = (MismatchedNotSetException)e;
			result = "mismatched input " + GetTokenErrorDisplay(e.Token) + " expecting set " + ex6.expecting;
		}
		else if (e is FailedPredicateException)
		{
			FailedPredicateException ex7 = (FailedPredicateException)e;
			result = "rule " + ex7.ruleName + " failed predicate: {" + ex7.predicateText + "}?";
		}
		return result;
	}

	public virtual string GetErrorHeader(RecognitionException e)
	{
		return "line " + e.Line + ":" + e.CharPositionInLine;
	}

	public virtual string GetTokenErrorDisplay(IToken t)
	{
		string text = t.Text;
		if (text == null)
		{
			text = ((t.Type != Token.EOF) ? ("<" + t.Type + ">") : "<EOF>");
		}
		text = text.Replace("\n", "\\\\n");
		text = text.Replace("\r", "\\\\r");
		text = text.Replace("\t", "\\\\t");
		return "'" + text + "'";
	}

	public virtual void EmitErrorMessage(string msg)
	{
		Console.Error.WriteLine(msg);
	}

	public virtual void Recover(IIntStream input, RecognitionException re)
	{
		if (state.lastErrorIndex == input.Index())
		{
			input.Consume();
		}
		state.lastErrorIndex = input.Index();
		BitSet set = ComputeErrorRecoverySet();
		BeginResync();
		ConsumeUntil(input, set);
		EndResync();
	}

	public virtual void BeginResync()
	{
	}

	public virtual void EndResync()
	{
	}

	protected internal virtual object RecoverFromMismatchedToken(IIntStream input, int ttype, BitSet follow)
	{
		RecognitionException e = null;
		if (MismatchIsUnwantedToken(input, ttype))
		{
			e = new UnwantedTokenException(ttype, input);
			BeginResync();
			input.Consume();
			EndResync();
			ReportError(e);
			object currentInputSymbol = GetCurrentInputSymbol(input);
			input.Consume();
			return currentInputSymbol;
		}
		if (MismatchIsMissingToken(input, follow))
		{
			object missingSymbol = GetMissingSymbol(input, e, ttype, follow);
			e = new MissingTokenException(ttype, input, missingSymbol);
			ReportError(e);
			return missingSymbol;
		}
		e = new MismatchedTokenException(ttype, input);
		throw e;
	}

	public virtual object RecoverFromMismatchedSet(IIntStream input, RecognitionException e, BitSet follow)
	{
		if (MismatchIsMissingToken(input, follow))
		{
			ReportError(e);
			return GetMissingSymbol(input, e, 0, follow);
		}
		throw e;
	}

	public virtual void ConsumeUntil(IIntStream input, int tokenType)
	{
		int num = input.LA(1);
		while (num != Token.EOF && num != tokenType)
		{
			input.Consume();
			num = input.LA(1);
		}
	}

	public virtual void ConsumeUntil(IIntStream input, BitSet set)
	{
		int num = input.LA(1);
		while (num != Token.EOF && !set.Member(num))
		{
			input.Consume();
			num = input.LA(1);
		}
	}

	public virtual IList GetRuleInvocationStack()
	{
		string fullName = GetType().FullName;
		return GetRuleInvocationStack(new Exception(), fullName);
	}

	public static IList GetRuleInvocationStack(Exception e, string recognizerClassName)
	{
		IList list = new List<object>();
		StackTrace stackTrace = new StackTrace(e);
		int num = 0;
		for (num = stackTrace.FrameCount - 1; num >= 0; num--)
		{
			StackFrame frame = stackTrace.GetFrame(num);
			if (!frame.GetMethod().DeclaringType.FullName.StartsWith("Unity.VisualScripting.Antlr3.Runtime.") && !frame.GetMethod().Name.Equals(NEXT_TOKEN_RULE_NAME) && frame.GetMethod().DeclaringType.FullName.Equals(recognizerClassName))
			{
				list.Add(frame.GetMethod().Name);
			}
		}
		return list;
	}

	public virtual IList ToStrings(IList tokens)
	{
		if (tokens == null)
		{
			return null;
		}
		IList list = new List<object>(tokens.Count);
		for (int i = 0; i < tokens.Count; i++)
		{
			list.Add(((IToken)tokens[i]).Text);
		}
		return list;
	}

	public virtual int GetRuleMemoization(int ruleIndex, int ruleStartIndex)
	{
		if (state.ruleMemo[ruleIndex] == null)
		{
			state.ruleMemo[ruleIndex] = new Hashtable();
		}
		object obj = state.ruleMemo[ruleIndex][ruleStartIndex];
		if (obj == null)
		{
			return -1;
		}
		return (int)obj;
	}

	public virtual bool AlreadyParsedRule(IIntStream input, int ruleIndex)
	{
		int ruleMemoization = GetRuleMemoization(ruleIndex, input.Index());
		switch (ruleMemoization)
		{
		case -1:
			return false;
		case -2:
			state.failed = true;
			break;
		default:
			input.Seek(ruleMemoization + 1);
			break;
		}
		return true;
	}

	public virtual void Memoize(IIntStream input, int ruleIndex, int ruleStartIndex)
	{
		int num = (state.failed ? (-2) : (input.Index() - 1));
		if (state.ruleMemo[ruleIndex] != null)
		{
			state.ruleMemo[ruleIndex][ruleStartIndex] = num;
		}
	}

	public int GetRuleMemoizationCacheSize()
	{
		int num = 0;
		int num2 = 0;
		while (state.ruleMemo != null && num2 < state.ruleMemo.Length)
		{
			IDictionary dictionary = state.ruleMemo[num2];
			if (dictionary != null)
			{
				num += dictionary.Count;
			}
			num2++;
		}
		return num;
	}

	public virtual void TraceIn(string ruleName, int ruleIndex, object inputSymbol)
	{
		Console.Out.Write("enter " + ruleName + " " + inputSymbol);
		if (state.backtracking > 0)
		{
			Console.Out.Write(" backtracking=" + state.backtracking);
		}
		Console.Out.WriteLine();
	}

	public virtual void TraceOut(string ruleName, int ruleIndex, object inputSymbol)
	{
		Console.Out.Write("exit " + ruleName + " " + inputSymbol);
		if (state.backtracking > 0)
		{
			Console.Out.Write(" backtracking=" + state.backtracking);
			if (state.failed)
			{
				Console.Out.WriteLine(" failed" + state.failed);
			}
			else
			{
				Console.Out.WriteLine(" succeeded" + state.failed);
			}
		}
		Console.Out.WriteLine();
	}

	protected internal virtual BitSet ComputeErrorRecoverySet()
	{
		return CombineFollows(exact: false);
	}

	protected internal virtual BitSet ComputeContextSensitiveRuleFOLLOW()
	{
		return CombineFollows(exact: true);
	}

	protected internal virtual BitSet CombineFollows(bool exact)
	{
		int followingStackPointer = state.followingStackPointer;
		BitSet bitSet = new BitSet();
		for (int num = followingStackPointer; num >= 0; num--)
		{
			BitSet bitSet2 = state.following[num];
			bitSet.OrInPlace(bitSet2);
			if (exact)
			{
				if (!bitSet2.Member(1))
				{
					break;
				}
				if (num > 0)
				{
					bitSet.Remove(1);
				}
			}
		}
		return bitSet;
	}

	protected virtual object GetCurrentInputSymbol(IIntStream input)
	{
		return null;
	}

	protected virtual object GetMissingSymbol(IIntStream input, RecognitionException e, int expectedTokenType, BitSet follow)
	{
		return null;
	}

	protected void PushFollow(BitSet fset)
	{
		if (state.followingStackPointer + 1 >= state.following.Length)
		{
			BitSet[] array = new BitSet[state.following.Length * 2];
			Array.Copy(state.following, 0, array, 0, state.following.Length);
			state.following = array;
		}
		state.following[++state.followingStackPointer] = fset;
	}
}
