namespace Unity.VisualScripting.Antlr3.Runtime;

public static class Token
{
	public const int EOR_TOKEN_TYPE = 1;

	public const int DOWN = 2;

	public const int UP = 3;

	public const int INVALID_TOKEN_TYPE = 0;

	public const int DEFAULT_CHANNEL = 0;

	public const int HIDDEN_CHANNEL = 99;

	public static readonly int MIN_TOKEN_TYPE = 4;

	public static readonly int EOF = -1;

	public static readonly IToken EOF_TOKEN = new CommonToken(EOF);

	public static readonly IToken INVALID_TOKEN = new CommonToken(0);

	public static readonly IToken SKIP_TOKEN = new CommonToken(0);
}
