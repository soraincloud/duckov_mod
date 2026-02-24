using System;

namespace Unity.VisualScripting.Antlr3.Runtime;

[Serializable]
public class ClassicToken : IToken
{
	protected internal string text;

	protected internal int type;

	protected internal int line;

	protected internal int charPositionInLine;

	protected internal int channel;

	protected internal int index;

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

	public virtual string Text
	{
		get
		{
			return text;
		}
		set
		{
			text = value;
		}
	}

	public virtual ICharStream InputStream
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public ClassicToken(int type)
	{
		this.type = type;
	}

	public ClassicToken(IToken oldToken)
	{
		text = oldToken.Text;
		type = oldToken.Type;
		line = oldToken.Line;
		charPositionInLine = oldToken.CharPositionInLine;
		channel = oldToken.Channel;
	}

	public ClassicToken(int type, string text)
	{
		this.type = type;
		this.text = text;
	}

	public ClassicToken(int type, string text, int channel)
	{
		this.type = type;
		this.text = text;
		this.channel = channel;
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
		return "[@" + TokenIndex + ",'" + text2 + "',<" + type + ">" + text + "," + line + ":" + CharPositionInLine + "]";
	}
}
