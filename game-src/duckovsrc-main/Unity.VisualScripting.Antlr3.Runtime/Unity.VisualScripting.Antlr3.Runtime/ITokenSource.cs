namespace Unity.VisualScripting.Antlr3.Runtime;

public interface ITokenSource
{
	string SourceName { get; }

	IToken NextToken();
}
