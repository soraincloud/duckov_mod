namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

public interface ITreeNodeStream : IIntStream
{
	object TreeSource { get; }

	ITokenStream TokenStream { get; }

	ITreeAdaptor TreeAdaptor { get; }

	bool HasUniqueNavigationNodes { set; }

	object Get(int i);

	object LT(int k);

	string ToString(object start, object stop);

	void ReplaceChildren(object parent, int startChildIndex, int stopChildIndex, object t);
}
