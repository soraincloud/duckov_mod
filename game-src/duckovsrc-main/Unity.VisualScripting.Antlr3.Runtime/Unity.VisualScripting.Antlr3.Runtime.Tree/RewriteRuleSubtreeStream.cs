using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

public class RewriteRuleSubtreeStream : RewriteRuleElementStream<object>
{
	private delegate object ProcessHandler(object o);

	public RewriteRuleSubtreeStream(ITreeAdaptor adaptor, string elementDescription)
		: base(adaptor, elementDescription)
	{
	}

	public RewriteRuleSubtreeStream(ITreeAdaptor adaptor, string elementDescription, object oneElement)
		: base(adaptor, elementDescription, oneElement)
	{
	}

	public RewriteRuleSubtreeStream(ITreeAdaptor adaptor, string elementDescription, IList<object> elements)
		: base(adaptor, elementDescription, elements)
	{
	}

	[Obsolete("This constructor is for internal use only and might be phased out soon. Use instead the one with IList<T>.")]
	public RewriteRuleSubtreeStream(ITreeAdaptor adaptor, string elementDescription, IList elements)
		: base(adaptor, elementDescription, elements)
	{
	}

	public object NextNode()
	{
		return FetchObject((object o) => adaptor.DupNode(o));
	}

	private object FetchObject(ProcessHandler ph)
	{
		if (RequiresDuplication())
		{
			return ph(_Next());
		}
		return _Next();
	}

	private bool RequiresDuplication()
	{
		int count = base.Count;
		if (!dirty)
		{
			if (cursor >= count)
			{
				return count == 1;
			}
			return false;
		}
		return true;
	}

	public override object NextTree()
	{
		return FetchObject((object o) => Dup(o));
	}

	private object Dup(object el)
	{
		return adaptor.DupTree(el);
	}
}
