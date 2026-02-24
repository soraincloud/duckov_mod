using System;

namespace Unity.VisualScripting.Antlr3.Runtime;

[Serializable]
public class FailedPredicateException : RecognitionException
{
	public string ruleName;

	public string predicateText;

	public FailedPredicateException()
	{
	}

	public FailedPredicateException(IIntStream input, string ruleName, string predicateText)
		: base(input)
	{
		this.ruleName = ruleName;
		this.predicateText = predicateText;
	}

	public override string ToString()
	{
		return "FailedPredicateException(" + ruleName + ",{" + predicateText + "}?)";
	}
}
