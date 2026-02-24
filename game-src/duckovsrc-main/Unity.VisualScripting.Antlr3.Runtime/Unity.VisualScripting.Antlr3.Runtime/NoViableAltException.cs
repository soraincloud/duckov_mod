using System;

namespace Unity.VisualScripting.Antlr3.Runtime;

[Serializable]
public class NoViableAltException : RecognitionException
{
	public string grammarDecisionDescription;

	public int decisionNumber;

	public int stateNumber;

	public NoViableAltException()
	{
	}

	public NoViableAltException(string grammarDecisionDescription, int decisionNumber, int stateNumber, IIntStream input)
		: base(input)
	{
		this.grammarDecisionDescription = grammarDecisionDescription;
		this.decisionNumber = decisionNumber;
		this.stateNumber = stateNumber;
	}

	public override string ToString()
	{
		if (input is ICharStream)
		{
			return "NoViableAltException('" + (char)UnexpectedType + "'@[" + grammarDecisionDescription + "])";
		}
		return "NoViableAltException(" + UnexpectedType + "@[" + grammarDecisionDescription + "])";
	}
}
