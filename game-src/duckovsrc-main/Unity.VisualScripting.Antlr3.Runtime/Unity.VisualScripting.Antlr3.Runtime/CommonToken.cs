using System;

namespace Unity.VisualScripting.Antlr3.Runtime;

[Serializable]
public class CommonToken : IToken
{
	protected internal int type;

	protected internal int line;

	protected internal int charPositionInLine = -1;

	protected internal int channel;

	[NonSerialized]
	protected internal ICharStream input;

	protected internal string text;

	protected internal int index = -1;

	protected internal int start;

	protected internal int stop;

	public virtual int Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
		}
	}

	public virtual int Line
	{
		get
		{
			return line;
		}
		set
		{
			line = value;
		}
	}

	public virtual int CharPositionInLine
	{
		get
		{
			return charPositionInLine;
		}
		set
		{
			charPositionInLine = value;
		}
	}

	public virtual int Channel
	{
		get
		{
			return channel;
		}
		set
		{
			channel = value;
		}
	}

	public virtual int StartIndex
	{
		get
		{
			return start;
		}
		set
		{
			start = value;
		}
	}

	public virtual int StopIndex
	{
		get
		{
			return stop;
		}
		set
		{
			stop = value;
		}
	}

	public virtual int TokenIndex
	{
		get
		{
			return index;
		}
		set
		{
			index = value;
		}
	}

	public virtual ICharStream InputStream
	{
		get
		{
			return input;
		}
		set
		{
			input = value;
		}
	}

	public virtual string Text
	{
		get
		{
			if (text != null)
			{
				return text;
			}
			if (input == null)
			{
				return null;
			}
			text = input.Substring(start, stop);
			return text;
		}
		set
		{
			text = value;
		}
	}

	public CommonToken(int type)
	{
		this.type = type;
	}

	public CommonToken(ICharStream input, int type, int channel, int start, int stop)
	{
		this.input = input;
		this.type = type;
		this.channel = channel;
		this.start = start;
		this.stop = stop;
	}

	public CommonToken(int type, string text)
	{
		this.type = type;
		channel = 0;
		this.text = text;
	}

	public CommonToken(IToken oldToken)
	{
		text = oldToken.Text;
		type = oldToken.Type;
		line = oldToken.Line;
		index = oldToken.TokenIndex;
		charPositionInLine = oldToken.CharPositionInLine;
		channel = oldToken.Channel;
		if (oldToken is CommonToken)
		{
			start = ((CommonToken)oldToken).start;
			stop = ((CommonToken)oldToken).stop;
		}
	}

	public override string ToString()
	{
		string text = "";
		if (channel > 0)
		{
			text = ",channel=" + channel;
		}
		string text2 = Text;
		if (text2 != null)
		{
			text2 = text2.Replace("\n", "\\\\n");
			text2 = text2.Replace("\r", "\\\\r");
			text2 = text2.Replace("\t", "\\\\t");
		}
		else
		{
			text2 = "<no text>";
		}
		return "[@" + TokenIndex + "," + start + ":" + stop + "='" + text2 + "',<" + type + ">" + text + "," + line + ":" + CharPositionInLine + "]";
	}
}
