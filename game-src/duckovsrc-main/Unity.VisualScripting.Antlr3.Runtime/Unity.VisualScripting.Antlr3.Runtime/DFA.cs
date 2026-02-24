namespace Unity.VisualScripting.Antlr3.Runtime;

public abstract class DFA
{
	public delegate int SpecialStateTransitionHandler(DFA dfa, int s, IIntStream input);

	public const bool debug = false;

	protected short[] eot;

	protected short[] eof;

	protected char[] min;

	protected char[] max;

	protected short[] accept;

	protected short[] special;

	protected short[][] transition;

	protected int decisionNumber;

	public SpecialStateTransitionHandler specialStateTransitionHandler;

	protected BaseRecognizer recognizer;

	public virtual string Description => "n/a";

	public int Predict(IIntStream input)
	{
		int marker = input.Mark();
		int num = 0;
		try
		{
			char c;
			while (true)
			{
				int num2 = special[num];
				if (num2 >= 0)
				{
					num = specialStateTransitionHandler(this, num2, input);
					if (num == -1)
					{
						NoViableAlt(num, input);
						return 0;
					}
					input.Consume();
					continue;
				}
				if (accept[num] >= 1)
				{
					return accept[num];
				}
				c = (char)input.LA(1);
				if (c >= min[num] && c <= max[num])
				{
					int num3 = transition[num][c - min[num]];
					if (num3 < 0)
					{
						if (eot[num] < 0)
						{
							NoViableAlt(num, input);
							return 0;
						}
						num = eot[num];
						input.Consume();
					}
					else
					{
						num = num3;
						input.Consume();
					}
				}
				else
				{
					if (eot[num] < 0)
					{
						break;
					}
					num = eot[num];
					input.Consume();
				}
			}
			if (c == (ushort)Token.EOF && eof[num] >= 0)
			{
				return accept[eof[num]];
			}
			NoViableAlt(num, input);
			return 0;
		}
		finally
		{
			input.Rewind(marker);
		}
	}

	protected void NoViableAlt(int s, IIntStream input)
	{
		if (recognizer.state.backtracking > 0)
		{
			recognizer.state.failed = true;
			return;
		}
		NoViableAltException ex = new NoViableAltException(Description, decisionNumber, s, input);
		Error(ex);
		throw ex;
	}

	public virtual void Error(NoViableAltException nvae)
	{
	}

	public virtual int SpecialStateTransition(int s, IIntStream input)
	{
		return -1;
	}

	public static short[] UnpackEncodedString(string encodedString)
	{
		int num = 0;
		for (int i = 0; i < encodedString.Length; i += 2)
		{
			num += encodedString[i];
		}
		short[] array = new short[num];
		int num2 = 0;
		for (int j = 0; j < encodedString.Length; j += 2)
		{
			char c = encodedString[j];
			char c2 = encodedString[j + 1];
			for (int k = 1; k <= c; k++)
			{
				array[num2++] = (short)c2;
			}
		}
		return array;
	}

	public static short[][] UnpackEncodedStringArray(string[] encodedStrings)
	{
		short[][] array = new short[encodedStrings.Length][];
		for (int i = 0; i < encodedStrings.Length; i++)
		{
			array[i] = UnpackEncodedString(encodedStrings[i]);
		}
		return array;
	}

	public static char[] UnpackEncodedStringToUnsignedChars(string encodedString)
	{
		int num = 0;
		for (int i = 0; i < encodedString.Length; i += 2)
		{
			num += encodedString[i];
		}
		char[] array = new char[num];
		int num2 = 0;
		for (int j = 0; j < encodedString.Length; j += 2)
		{
			char c = encodedString[j];
			char c2 = encodedString[j + 1];
			for (int k = 1; k <= c; k++)
			{
				array[num2++] = c2;
			}
		}
		return array;
	}

	public int SpecialTransition(int state, int symbol)
	{
		return 0;
	}
}
