using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

public class RewriteRuleTokenStream : RewriteRuleElementStream<IToken>
{
	public RewriteRuleTokenStream(ITreeAdaptor adaptor, string elementDescription)
		: base(adaptor, elementDescription)
	{
	}

	public RewriteRuleTokenStream(ITreeAdaptor adaptor, string elementDescription, IToken oneElement)
		: base(adaptor, elementDescription, oneElement)
	{
	}

	public RewriteRuleTokenStream(ITreeAdaptor adaptor, string elementDescription, IList<IToken> elements)
		: base(adaptor, elementDescription, elements)
	{
	}

	[Obsolete("This constructor is for internal use only and might be phased out soon. Use instead the one with IList<T>.")]
	public RewriteRuleTokenStream(ITreeAdaptor adaptor, string elementDescription, IList elements)
		: base(adaptor, elementDescription, elements)
	{
	}

	public object NextNode()
	{
		return adaptor.Create((IToken)_Next());
	}

	public IToken NextToken()
	{
		return (IToken)_Next();
	}

	protected override object ToTree(IToken el)
	{
		return el;
	}
}
