using System;

namespace Unity.VisualScripting.Antlr3.Runtime;

[Serializable]
public class MismatchedRangeException : RecognitionException
{
	private int a;

	private int b;

	public int A
	{
		get
		{
			return a;
		}
		set
		{
			a = value;
		}
	}

	public int B
	{
		get
		{
			return b;
		}
		set
		{
			b = value;
		}
	}

	public MismatchedRangeException()
	{
	}

	public MismatchedRangeException(int a, int b, IIntStream input)
		: base(input)
	{
		this.a = a;
		this.b = b;
	}

	public override string ToString()
	{
		return "MismatchedNotSetException(" + UnexpectedType + " not in [" + a + "," + b + "])";
	}
}
