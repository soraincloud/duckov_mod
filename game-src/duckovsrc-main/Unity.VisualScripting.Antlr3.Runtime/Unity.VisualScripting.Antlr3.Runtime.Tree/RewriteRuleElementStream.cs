using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

public abstract class RewriteRuleElementStream<T>
{
	protected int cursor;

	protected T singleElement;

	protected IList<T> elements;

	protected bool dirty;

	protected string elementDescription;

	protected ITreeAdaptor adaptor;

	public int Count
	{
		get
		{
			if (singleElement != null)
			{
				return 1;
			}
			if (elements != null)
			{
				return elements.Count;
			}
			return 0;
		}
	}

	public string Description => elementDescription;

	public RewriteRuleElementStream(ITreeAdaptor adaptor, string elementDescription)
	{
		this.elementDescription = elementDescription;
		this.adaptor = adaptor;
	}

	public RewriteRuleElementStream(ITreeAdaptor adaptor, string elementDescription, T oneElement)
		: this(adaptor, elementDescription)
	{
		Add(oneElement);
	}

	public RewriteRuleElementStream(ITreeAdaptor adaptor, string elementDescription, IList<T> elements)
		: this(adaptor, elementDescription)
	{
		singleElement = default(T);
		this.elements = elements;
	}

	[Obsolete("This constructor is for internal use only and might be phased out soon. Use instead the one with IList<T>.")]
	public RewriteRuleElementStream(ITreeAdaptor adaptor, string elementDescription, IList elements)
		: this(adaptor, elementDescription)
	{
		singleElement = default(T);
		this.elements = new List<T>();
		if (elements == null)
		{
			return;
		}
		foreach (T element in elements)
		{
			this.elements.Add(element);
		}
	}

	public void Add(T el)
	{
		if (el != null)
		{
			if (elements != null)
			{
				elements.Add(el);
				return;
			}
			if (singleElement == null)
			{
				singleElement = el;
				return;
			}
			elements = new List<T>(5);
			elements.Add(singleElement);
			singleElement = default(T);
			elements.Add(el);
		}
	}

	public virtual void Reset()
	{
		cursor = 0;
		dirty = true;
	}

	public bool HasNext()
	{
		if (singleElement == null || cursor >= 1)
		{
			if (elements != null)
			{
				return cursor < elements.Count;
			}
			return false;
		}
		return true;
	}

	public virtual object NextTree()
	{
		return _Next();
	}

	protected object _Next()
	{
		int count = Count;
		if (count == 0)
		{
			throw new RewriteEmptyStreamException(elementDescription);
		}
		if (cursor >= count)
		{
			if (count == 1)
			{
				return ToTree(singleElement);
			}
			throw new RewriteCardinalityException(elementDescription);
		}
		if (singleElement != null)
		{
			cursor++;
			return ToTree(singleElement);
		}
		return ToTree(elements[cursor++]);
	}

	protected virtual object ToTree(T el)
	{
		return el;
	}

	[Obsolete("Please use property Count instead.")]
	public int Size()
	{
		return Count;
	}
}
