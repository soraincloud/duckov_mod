using System;

namespace Unity.VisualScripting.Antlr3.Runtime;

[Serializable]
public class MissingTokenException : MismatchedTokenException
{
	private object inserted;

	public int MissingType => base.Expecting;

	public object Inserted
	{
		get
		{
			return inserted;
		}
		set
		{
			inserted = value;
		}
	}

	public MissingTokenException()
	{
	}

	public MissingTokenException(int expecting, IIntStream input, object inserted)
		: base(expecting, input)
	{
		this.inserted = inserted;
	}

	public override string ToString()
	{
		if (inserted != null && token != null)
		{
			return string.Concat("MissingTokenException(inserted ", inserted, " at ", token.Text, ")");
		}
		if (token != null)
		{
			return "MissingTokenException(at " + token.Text + ")";
		}
		return "MissingTokenException";
	}
}
