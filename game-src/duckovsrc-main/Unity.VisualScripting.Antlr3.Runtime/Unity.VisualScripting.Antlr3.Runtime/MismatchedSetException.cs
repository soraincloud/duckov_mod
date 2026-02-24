using System;

namespace Unity.VisualScripting.Antlr3.Runtime;

[Serializable]
public class MismatchedSetException : RecognitionException
{
	public BitSet expecting;

	public MismatchedSetException()
	{
	}

	public MismatchedSetException(BitSet expecting, IIntStream input)
		: base(input)
	{
		this.expecting = expecting;
	}

	public override string ToString()
	{
		return string.Concat("MismatchedSetException(", UnexpectedType, "!=", expecting, ")");
	}
}
