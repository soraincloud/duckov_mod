namespace Unity.VisualScripting.Antlr3.Runtime;

public class UnwantedTokenException : MismatchedTokenException
{
	public IToken UnexpectedToken => token;

	public UnwantedTokenException()
	{
	}

	public UnwantedTokenException(int expecting, IIntStream input)
		: base(expecting, input)
	{
	}

	public override string ToString()
	{
		string text = ", expected " + base.Expecting;
		if (base.Expecting == 0)
		{
			text = "";
		}
		if (token == null)
		{
			return "UnwantedTokenException(found=" + text + ")";
		}
		return "UnwantedTokenException(found=" + token.Text + text + ")";
	}
}
