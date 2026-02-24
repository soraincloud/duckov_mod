using System;

namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

[Serializable]
public class RewriteEmptyStreamException : RewriteCardinalityException
{
	public RewriteEmptyStreamException(string elementDescription)
		: base(elementDescription)
	{
	}
}
