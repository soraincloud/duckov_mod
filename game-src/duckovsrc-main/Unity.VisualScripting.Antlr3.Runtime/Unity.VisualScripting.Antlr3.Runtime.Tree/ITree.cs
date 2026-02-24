using System.Collections;

namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

public interface ITree
{
	int ChildCount { get; }

	ITree Parent { get; set; }

	int ChildIndex { get; set; }

	bool IsNil { get; }

	int Type { get; }

	string Text { get; }

	int Line { get; }

	int CharPositionInLine { get; }

	int TokenStartIndex { get; set; }

	int TokenStopIndex { get; set; }

	bool HasAncestor(int ttype);

	ITree GetAncestor(int ttype);

	IList GetAncestors();

	void FreshenParentAndChildIndexes();

	ITree GetChild(int i);

	void AddChild(ITree t);

	void SetChild(int i, ITree t);

	object DeleteChild(int i);

	void ReplaceChildren(int startChildIndex, int stopChildIndex, object t);

	ITree DupNode();

	string ToStringTree();

	new string ToString();
}
