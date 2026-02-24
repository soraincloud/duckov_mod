using System;

namespace Unity.VisualScripting.Antlr3.Runtime;

[Serializable]
public class MismatchedNotSetException : MismatchedSetException
{
	public MismatchedNotSetException()
	{
	}

	public MismatchedNotSetException(BitSet expecting, IIntStream input)
		: base(expecting, input)
	{
	}

	public override string ToString()
	{
		return string.Concat("MismatchedNotSetException(", UnexpectedType, "!=", expecting, ")");
	}
}
