using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.VisualScripting.Antlr3.Runtime;

public class ANTLRStringStream : ICharStream, IIntStream
{
	protected internal char[] data;

	protected int n;

	protected internal int p;

	protected internal int line = 1;

	protected internal int charPositionInLine;

	protected internal int markDepth;

	protected internal IList markers;

	protected int lastMarker;

	protected string name;

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

	public virtual int Count => n;

	public virtual string SourceName
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	protected ANTLRStringStream()
	{
	}

	public ANTLRStringStream(string input)
	{
		data = input.ToCharArray();
		n = input.Length;
	}

	public ANTLRStringStream(char[] data, int numberOfActualCharsInArray)
	{
		this.data = data;
		n = numberOfActualCharsInArray;
	}

	public virtual void Reset()
	{
		p = 0;
		line = 1;
		charPositionInLine = 0;
		markDepth = 0;
	}

	public virtual void Consume()
	{
		if (p < n)
		{
			charPositionInLine++;
			if (data[p] == '\n')
			{
				line++;
				charPositionInLine = 0;
			}
			p++;
		}
	}

	public virtual int LA(int i)
	{
		if (i == 0)
		{
			return 0;
		}
		if (i < 0)
		{
			i++;
			if (p + i - 1 < 0)
			{
				return -1;
			}
		}
		if (p + i - 1 >= n)
		{
			return -1;
		}
		return data[p + i - 1];
	}

	public virtual int LT(int i)
	{
		return LA(i);
	}

	public virtual int Index()
	{
		return p;
	}

	[Obsolete("Please use property Count instead.")]
	public virtual int Size()
	{
		return Count;
	}

	public virtual int Mark()
	{
		if (markers == null)
		{
			markers = new List<object>();
			markers.Add(null);
		}
		markDepth++;
		CharStreamState charStreamState = null;
		if (markDepth >= markers.Count)
		{
			charStreamState = new CharStreamState();
			markers.Add(charStreamState);
		}
		else
		{
			charStreamState = (CharStreamState)markers[markDepth];
		}
		charStreamState.p = p;
		charStreamState.line = line;
		charStreamState.charPositionInLine = charPositionInLine;
		lastMarker = markDepth;
		return markDepth;
	}

	public virtual void Rewind(int m)
	{
		CharStreamState charStreamState = (CharStreamState)markers[m];
		Seek(charStreamState.p);
		line = charStreamState.line;
		charPositionInLine = charStreamState.charPositionInLine;
		Release(m);
	}

	public virtual void Rewind()
	{
		Rewind(lastMarker);
	}

	public virtual void Release(int marker)
	{
		markDepth = marker;
		markDepth--;
	}

	public virtual void Seek(int index)
	{
		if (index <= p)
		{
			p = index;
			return;
		}
		while (p < index)
		{
			Consume();
		}
	}

	public virtual string Substring(int start, int stop)
	{
		return new string(data, start, stop - start + 1);
	}
}
