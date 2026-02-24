using System;

namespace Unity.VisualScripting.Antlr3.Runtime;

[Serializable]
public class MismatchedTokenException : RecognitionException
{
	private int expecting;

	public int Expecting
	{
		get
		{
			return expecting;
		}
		set
		{
			expecting = value;
		}
	}

	public MismatchedTokenException()
	{
	}

	public MismatchedTokenException(int expecting, IIntStream input)
		: base(input)
	{
		this.expecting = expecting;
	}

	public override string ToString()
	{
		return "MismatchedTokenException(" + UnexpectedType + "!=" + expecting + ")";
	}
}
