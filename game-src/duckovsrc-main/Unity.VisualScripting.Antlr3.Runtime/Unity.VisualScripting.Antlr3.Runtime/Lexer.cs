using System;

namespace Unity.VisualScripting.Antlr3.Runtime;

public abstract class Lexer : BaseRecognizer, ITokenSource
{
	private const int TOKEN_dot_EOF = -1;

	protected internal ICharStream input;

	public virtual ICharStream CharStream
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

	public virtual int Line => input.Line;

	public virtual int CharPositionInLine => input.CharPositionInLine;

	public virtual int CharIndex => input.Index();

	public virtual string Text
	{
		get
		{
			if (state.text != null)
			{
				return state.text;
			}
			return input.Substring(state.tokenStartCharIndex, CharIndex - 1);
		}
		set
		{
			state.text = value;
		}
	}

	public Lexer()
	{
	}

	public Lexer(ICharStream input)
	{
		this.input = input;
	}

	public Lexer(ICharStream input, RecognizerSharedState state)
		: base(state)
	{
		this.input = input;
	}

	public override void Reset()
	{
		base.Reset();
		if (input != null)
		{
			input.Seek(0);
		}
		if (state != null)
		{
			state.token = null;
			state.type = 0;
			state.channel = 0;
			state.tokenStartCharIndex = -1;
			state.tokenStartCharPositionInLine = -1;
			state.tokenStartLine = -1;
			state.text = null;
		}
	}

	public virtual IToken NextToken()
	{
		while (true)
		{
			state.token = null;
			state.channel = 0;
			state.tokenStartCharIndex = input.Index();
			state.tokenStartCharPositionInLine = input.CharPositionInLine;
			state.tokenStartLine = input.Line;
			state.text = null;
			if (input.LA(1) == -1)
			{
				break;
			}
			try
			{
				mTokens();
				if (state.token == null)
				{
					Emit();
					goto IL_00ae;
				}
				if (state.token != Token.SKIP_TOKEN)
				{
					goto IL_00ae;
				}
				goto end_IL_007b;
				IL_00ae:
				return state.token;
				end_IL_007b:;
			}
			catch (NoViableAltException ex)
			{
				ReportError(ex);
				Recover(ex);
			}
			catch (RecognitionException e)
			{
				ReportError(e);
			}
		}
		return Token.EOF_TOKEN;
	}

	public void Skip()
	{
		state.token = Token.SKIP_TOKEN;
	}

	public abstract void mTokens();

	public virtual void Emit(IToken token)
	{
		state.token = token;
	}

	public virtual IToken Emit()
	{
		IToken token = new CommonToken(input, state.type, state.channel, state.tokenStartCharIndex, CharIndex - 1);
		token.Line = state.tokenStartLine;
		token.Text = state.text;
		token.CharPositionInLine = state.tokenStartCharPositionInLine;
		Emit(token);
		return token;
	}

	public virtual void Match(string s)
	{
		int num = 0;
		while (num < s.Length)
		{
			if (input.LA(1) != s[num])
			{
				if (state.backtracking > 0)
				{
					state.failed = true;
					break;
				}
				MismatchedTokenException ex = new MismatchedTokenException(s[num], input);
				Recover(ex);
				throw ex;
			}
			num++;
			input.Consume();
			state.failed = false;
		}
	}

	public virtual void MatchAny()
	{
		input.Consume();
	}

	public virtual void Match(int c)
	{
		if (input.LA(1) != c)
		{
			if (state.backtracking <= 0)
			{
				MismatchedTokenException ex = new MismatchedTokenException(c, input);
				Recover(ex);
				throw ex;
			}
			state.failed = true;
		}
		else
		{
			input.Consume();
			state.failed = false;
		}
	}

	public virtual void MatchRange(int a, int b)
	{
		if (input.LA(1) < a || input.LA(1) > b)
		{
			if (state.backtracking <= 0)
			{
				MismatchedRangeException ex = new MismatchedRangeException(a, b, input);
				Recover(ex);
				throw ex;
			}
			state.failed = true;
		}
		else
		{
			input.Consume();
			state.failed = false;
		}
	}

	public virtual void Recover(RecognitionException re)
	{
		input.Consume();
	}

	public override void ReportError(RecognitionException e)
	{
		DisplayRecognitionError(TokenNames, e);
	}

	public override string GetErrorMessage(RecognitionException e, string[] tokenNames)
	{
		string text = null;
		if (e is MismatchedTokenException)
		{
			MismatchedTokenException ex = (MismatchedTokenException)e;
			return "mismatched character " + GetCharErrorDisplay(e.Char) + " expecting " + GetCharErrorDisplay(ex.Expecting);
		}
		if (e is NoViableAltException)
		{
			NoViableAltException ex2 = (NoViableAltException)e;
			return "no viable alternative at character " + GetCharErrorDisplay(ex2.Char);
		}
		if (e is EarlyExitException)
		{
			EarlyExitException ex3 = (EarlyExitException)e;
			return "required (...)+ loop did not match anything at character " + GetCharErrorDisplay(ex3.Char);
		}
		if (e is MismatchedNotSetException)
		{
			MismatchedSetException ex4 = (MismatchedSetException)e;
			return "mismatched character " + GetCharErrorDisplay(ex4.Char) + " expecting set " + ex4.expecting;
		}
		if (e is MismatchedSetException)
		{
			MismatchedSetException ex5 = (MismatchedSetException)e;
			return "mismatched character " + GetCharErrorDisplay(ex5.Char) + " expecting set " + ex5.expecting;
		}
		if (e is MismatchedRangeException)
		{
			MismatchedRangeException ex6 = (MismatchedRangeException)e;
			return "mismatched character " + GetCharErrorDisplay(ex6.Char) + " expecting set " + GetCharErrorDisplay(ex6.A) + ".." + GetCharErrorDisplay(ex6.B);
		}
		return base.GetErrorMessage(e, tokenNames);
	}

	public string GetCharErrorDisplay(int c)
	{
		return "'" + c switch
		{
			-1 => "<EOF>", 
			10 => "\\n", 
			9 => "\\t", 
			13 => "\\r", 
			_ => Convert.ToString((char)c), 
		} + "'";
	}

	public virtual void TraceIn(string ruleName, int ruleIndex)
	{
		string inputSymbol = (char)input.LT(1) + " line=" + Line + ":" + CharPositionInLine;
		base.TraceIn(ruleName, ruleIndex, inputSymbol);
	}

	public virtual void TraceOut(string ruleName, int ruleIndex)
	{
		string inputSymbol = (char)input.LT(1) + " line=" + Line + ":" + CharPositionInLine;
		base.TraceOut(ruleName, ruleIndex, inputSymbol);
	}
}
