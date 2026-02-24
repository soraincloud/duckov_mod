using System.Text;

namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

public class TreePatternLexer
{
	public const int EOF = -1;

	public const int BEGIN = 1;

	public const int END = 2;

	public const int ID = 3;

	public const int ARG = 4;

	public const int PERCENT = 5;

	public const int COLON = 6;

	public const int DOT = 7;

	protected string pattern;

	protected int p = -1;

	protected int c;

	protected int n;

	public StringBuilder sval = new StringBuilder();

	public bool error;

	public TreePatternLexer(string pattern)
	{
		this.pattern = pattern;
		n = pattern.Length;
		Consume();
	}

	public int NextToken()
	{
		sval.Length = 0;
		while (c != -1)
		{
			if (c == 32 || c == 10 || c == 13 || c == 9)
			{
				Consume();
				continue;
			}
			if ((c >= 97 && c <= 122) || (c >= 65 && c <= 90) || c == 95)
			{
				sval.Append((char)c);
				Consume();
				while ((c >= 97 && c <= 122) || (c >= 65 && c <= 90) || (c >= 48 && c <= 57) || c == 95)
				{
					sval.Append((char)c);
					Consume();
				}
				return 3;
			}
			if (c == 40)
			{
				Consume();
				return 1;
			}
			if (c == 41)
			{
				Consume();
				return 2;
			}
			if (c == 37)
			{
				Consume();
				return 5;
			}
			if (c == 58)
			{
				Consume();
				return 6;
			}
			if (c == 46)
			{
				Consume();
				return 7;
			}
			if (c == 91)
			{
				Consume();
				while (c != 93)
				{
					if (c == 92)
					{
						Consume();
						if (c != 93)
						{
							sval.Append('\\');
						}
						sval.Append((char)c);
					}
					else
					{
						sval.Append((char)c);
					}
					Consume();
				}
				Consume();
				return 4;
			}
			Consume();
			error = true;
			return -1;
		}
		return -1;
	}

	protected void Consume()
	{
		p++;
		if (p >= n)
		{
			c = -1;
		}
		else
		{
			c = pattern[p];
		}
	}
}
