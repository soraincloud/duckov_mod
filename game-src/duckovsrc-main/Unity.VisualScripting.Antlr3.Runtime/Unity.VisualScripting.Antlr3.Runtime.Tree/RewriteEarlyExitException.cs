using System;

namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

[Serializable]
public class RewriteEarlyExitException : RewriteCardinalityException
{
	public RewriteEarlyExitException()
		: base(null)
	{
	}

	public RewriteEarlyExitException(string elementDescription)
		: base(elementDescription)
	{
	}
}
