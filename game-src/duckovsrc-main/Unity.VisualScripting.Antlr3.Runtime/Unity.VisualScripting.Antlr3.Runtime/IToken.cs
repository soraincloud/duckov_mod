namespace Unity.VisualScripting.Antlr3.Runtime;

public interface IToken
{
	int Type { get; set; }

	int Line { get; set; }

	int CharPositionInLine { get; set; }

	int Channel { get; set; }

	int TokenIndex { get; set; }

	string Text { get; set; }
}
