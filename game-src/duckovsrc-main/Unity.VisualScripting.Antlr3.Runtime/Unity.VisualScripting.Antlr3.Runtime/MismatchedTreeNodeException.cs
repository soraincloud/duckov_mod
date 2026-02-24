using System;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

namespace Unity.VisualScripting.Antlr3.Runtime;

[Serializable]
public class MismatchedTreeNodeException : RecognitionException
{
	public int expecting;

	public MismatchedTreeNodeException()
	{
	}

	public MismatchedTreeNodeException(int expecting, ITreeNodeStream input)
		: base(input)
	{
		this.expecting = expecting;
	}

	public override string ToString()
	{
		return "MismatchedTreeNodeException(" + UnexpectedType + "!=" + expecting + ")";
	}
}
