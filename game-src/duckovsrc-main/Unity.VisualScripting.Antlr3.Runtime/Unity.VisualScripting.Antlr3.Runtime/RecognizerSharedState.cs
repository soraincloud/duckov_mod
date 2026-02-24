using System.Collections;

namespace Unity.VisualScripting.Antlr3.Runtime;

public class RecognizerSharedState
{
	public BitSet[] following = new BitSet[100];

	public int followingStackPointer = -1;

	public bool errorRecovery;

	public int lastErrorIndex = -1;

	public bool failed;

	public int syntaxErrors;

	public int backtracking;

	public IDictionary[] ruleMemo;

	public IToken token;

	public int tokenStartCharIndex = -1;

	public int tokenStartLine;

	public int tokenStartCharPositionInLine;

	public int channel;

	public int type;

	public string text;
}
