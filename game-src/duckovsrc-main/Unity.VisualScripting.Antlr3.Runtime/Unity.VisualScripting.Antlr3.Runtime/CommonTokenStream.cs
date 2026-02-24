using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting.Antlr3.Runtime.Collections;

namespace Unity.VisualScripting.Antlr3.Runtime;

public class CommonTokenStream : ITokenStream, IIntStream
{
	protected ITokenSource tokenSource;

	protected IList tokens;

	protected IDictionary channelOverrideMap;

	protected HashList discardSet;

	protected int channel;

	protected bool discardOffChannelTokens;

	protected int lastMarker;

	protected int p = -1;

	public virtual ITokenSource TokenSource
	{
		get
		{
			return tokenSource;
		}
		set
		{
			tokenSource = value;
			tokens.Clear();
			p = -1;
			channel = 0;
		}
	}

	public virtual string SourceName => TokenSource.SourceName;

	public virtual int Count => tokens.Count;

	public CommonTokenStream()
	{
		channel = 0;
		tokens = new List<object>(500);
	}

	public CommonTokenStream(ITokenSource tokenSource)
		: this()
	{
		this.tokenSource = tokenSource;
	}

	public CommonTokenStream(ITokenSource tokenSource, int channel)
		: this(tokenSource)
	{
		this.channel = channel;
	}

	public virtual IToken LT(int k)
	{
		if (p == -1)
		{
			FillBuffer();
		}
		if (k == 0)
		{
			return null;
		}
		if (k < 0)
		{
			return LB(-k);
		}
		if (p + k - 1 >= tokens.Count)
		{
			return Token.EOF_TOKEN;
		}
		int num = p;
		for (int i = 1; i < k; i++)
		{
			num = SkipOffTokenChannels(num + 1);
		}
		if (num >= tokens.Count)
		{
			return Token.EOF_TOKEN;
		}
		return (IToken)tokens[num];
	}

	public virtual IToken Get(int i)
	{
		return (IToken)tokens[i];
	}

	public virtual string ToString(int start, int stop)
	{
		if (start < 0 || stop < 0)
		{
			return null;
		}
		if (p == -1)
		{
			FillBuffer();
		}
		if (stop >= tokens.Count)
		{
			stop = tokens.Count - 1;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = start; i <= stop; i++)
		{
			IToken token = (IToken)tokens[i];
			stringBuilder.Append(token.Text);
		}
		return stringBuilder.ToString();
	}

	public virtual string ToString(IToken start, IToken stop)
	{
		if (start != null && stop != null)
		{
			return ToString(start.TokenIndex, stop.TokenIndex);
		}
		return null;
	}

	public virtual void Consume()
	{
		if (p < tokens.Count)
		{
			p++;
			p = SkipOffTokenChannels(p);
		}
	}

	public virtual int LA(int i)
	{
		return LT(i).Type;
	}

	public virtual int Mark()
	{
		if (p == -1)
		{
			FillBuffer();
		}
		lastMarker = Index();
		return lastMarker;
	}

	public virtual int Index()
	{
		return p;
	}

	public virtual void Rewind(int marker)
	{
		Seek(marker);
	}

	public virtual void Rewind()
	{
		Seek(lastMarker);
	}

	public virtual void Reset()
	{
		p = 0;
		lastMarker = 0;
	}

	public virtual void Release(int marker)
	{
	}

	public virtual void Seek(int index)
	{
		p = index;
	}

	[Obsolete("Please use the property Count instead.")]
	public virtual int Size()
	{
		return Count;
	}

	protected virtual void FillBuffer()
	{
		int num = 0;
		IToken token = tokenSource.NextToken();
		while (token != null && token.Type != -1)
		{
			bool flag = false;
			if (channelOverrideMap != null)
			{
				object obj = channelOverrideMap[token.Type];
				if (obj != null)
				{
					token.Channel = (int)obj;
				}
			}
			if (discardSet != null && discardSet.Contains(token.Type.ToString()))
			{
				flag = true;
			}
			else if (discardOffChannelTokens && token.Channel != channel)
			{
				flag = true;
			}
			if (!flag)
			{
				token.TokenIndex = num;
				tokens.Add(token);
				num++;
			}
			token = tokenSource.NextToken();
		}
		p = 0;
		p = SkipOffTokenChannels(p);
	}

	protected virtual int SkipOffTokenChannels(int i)
	{
		int count = tokens.Count;
		while (i < count && ((IToken)tokens[i]).Channel != channel)
		{
			i++;
		}
		return i;
	}

	protected virtual int SkipOffTokenChannelsReverse(int i)
	{
		while (i >= 0 && ((IToken)tokens[i]).Channel != channel)
		{
			i--;
		}
		return i;
	}

	public virtual void SetTokenTypeChannel(int ttype, int channel)
	{
		if (channelOverrideMap == null)
		{
			channelOverrideMap = new Hashtable();
		}
		channelOverrideMap[ttype] = channel;
	}

	public virtual void DiscardTokenType(int ttype)
	{
		if (discardSet == null)
		{
			discardSet = new HashList();
		}
		discardSet.Add(ttype.ToString(), ttype);
	}

	public virtual void DiscardOffChannelTokens(bool discardOffChannelTokens)
	{
		this.discardOffChannelTokens = discardOffChannelTokens;
	}

	public virtual IList GetTokens()
	{
		if (p == -1)
		{
			FillBuffer();
		}
		return tokens;
	}

	public virtual IList GetTokens(int start, int stop)
	{
		return GetTokens(start, stop, (BitSet)null);
	}

	public virtual IList GetTokens(int start, int stop, BitSet types)
	{
		if (p == -1)
		{
			FillBuffer();
		}
		if (stop >= tokens.Count)
		{
			stop = tokens.Count - 1;
		}
		if (start < 0)
		{
			start = 0;
		}
		if (start > stop)
		{
			return null;
		}
		IList list = new List<object>();
		for (int i = start; i <= stop; i++)
		{
			IToken token = (IToken)tokens[i];
			if (types == null || types.Member(token.Type))
			{
				list.Add(token);
			}
		}
		if (list.Count == 0)
		{
			list = null;
		}
		return list;
	}

	public virtual IList GetTokens(int start, int stop, IList types)
	{
		return GetTokens(start, stop, new BitSet(types));
	}

	public virtual IList GetTokens(int start, int stop, int ttype)
	{
		return GetTokens(start, stop, BitSet.Of(ttype));
	}

	protected virtual IToken LB(int k)
	{
		if (p == -1)
		{
			FillBuffer();
		}
		if (k == 0)
		{
			return null;
		}
		if (p - k < 0)
		{
			return null;
		}
		int num = p;
		for (int i = 1; i <= k; i++)
		{
			num = SkipOffTokenChannelsReverse(num - 1);
		}
		if (num < 0)
		{
			return null;
		}
		return (IToken)tokens[num];
	}

	public override string ToString()
	{
		if (p == -1)
		{
			FillBuffer();
		}
		return ToString(0, tokens.Count - 1);
	}
}
