using System.Collections;
using System.Collections.Generic;

namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

public class TreeWizard
{
	public interface ContextVisitor
	{
		void Visit(object t, object parent, int childIndex, IDictionary labels);
	}

	public abstract class Visitor : ContextVisitor
	{
		public void Visit(object t, object parent, int childIndex, IDictionary labels)
		{
			Visit(t);
		}

		public abstract void Visit(object t);
	}

	private sealed class RecordAllElementsVisitor : Visitor
	{
		private IList list;

		public RecordAllElementsVisitor(IList list)
		{
			this.list = list;
		}

		public override void Visit(object t)
		{
			list.Add(t);
		}
	}

	private sealed class PatternMatchingContextVisitor : ContextVisitor
	{
		private TreeWizard owner;

		private TreePattern pattern;

		private IList list;

		public PatternMatchingContextVisitor(TreeWizard owner, TreePattern pattern, IList list)
		{
			this.owner = owner;
			this.pattern = pattern;
			this.list = list;
		}

		public void Visit(object t, object parent, int childIndex, IDictionary labels)
		{
			if (owner._Parse(t, pattern, null))
			{
				list.Add(t);
			}
		}
	}

	public class TreePattern : CommonTree
	{
		public string label;

		public bool hasTextArg;

		public TreePattern(IToken payload)
			: base(payload)
		{
		}

		public override string ToString()
		{
			if (label != null)
			{
				return "%" + label + ":" + base.ToString();
			}
			return base.ToString();
		}
	}

	private sealed class InvokeVisitorOnPatternMatchContextVisitor : ContextVisitor
	{
		private TreeWizard owner;

		private TreePattern pattern;

		private ContextVisitor visitor;

		private Hashtable labels = new Hashtable();

		public InvokeVisitorOnPatternMatchContextVisitor(TreeWizard owner, TreePattern pattern, ContextVisitor visitor)
		{
			this.owner = owner;
			this.pattern = pattern;
			this.visitor = visitor;
		}

		public void Visit(object t, object parent, int childIndex, IDictionary unusedlabels)
		{
			labels.Clear();
			if (owner._Parse(t, pattern, labels))
			{
				visitor.Visit(t, parent, childIndex, labels);
			}
		}
	}

	public class WildcardTreePattern : TreePattern
	{
		public WildcardTreePattern(IToken payload)
			: base(payload)
		{
		}
	}

	public class TreePatternTreeAdaptor : CommonTreeAdaptor
	{
		public override object Create(IToken payload)
		{
			return new TreePattern(payload);
		}
	}

	protected ITreeAdaptor adaptor;

	protected IDictionary tokenNameToTypeMap;

	public TreeWizard(ITreeAdaptor adaptor)
	{
		this.adaptor = adaptor;
	}

	public TreeWizard(ITreeAdaptor adaptor, IDictionary tokenNameToTypeMap)
	{
		this.adaptor = adaptor;
		this.tokenNameToTypeMap = tokenNameToTypeMap;
	}

	public TreeWizard(ITreeAdaptor adaptor, string[] tokenNames)
	{
		this.adaptor = adaptor;
		tokenNameToTypeMap = ComputeTokenTypes(tokenNames);
	}

	public TreeWizard(string[] tokenNames)
		: this(null, tokenNames)
	{
	}

	public IDictionary ComputeTokenTypes(string[] tokenNames)
	{
		IDictionary dictionary = new Hashtable();
		if (tokenNames == null)
		{
			return dictionary;
		}
		for (int i = Token.MIN_TOKEN_TYPE; i < tokenNames.Length; i++)
		{
			string key = tokenNames[i];
			dictionary.Add(key, i);
		}
		return dictionary;
	}

	public int GetTokenType(string tokenName)
	{
		if (tokenNameToTypeMap == null)
		{
			return 0;
		}
		object obj = tokenNameToTypeMap[tokenName];
		if (obj != null)
		{
			return (int)obj;
		}
		return 0;
	}

	public IDictionary Index(object t)
	{
		IDictionary dictionary = new Hashtable();
		_Index(t, dictionary);
		return dictionary;
	}

	protected void _Index(object t, IDictionary m)
	{
		if (t != null)
		{
			int nodeType = adaptor.GetNodeType(t);
			IList list = m[nodeType] as IList;
			if (list == null)
			{
				list = new List<object>();
				m[nodeType] = list;
			}
			list.Add(t);
			int childCount = adaptor.GetChildCount(t);
			for (int i = 0; i < childCount; i++)
			{
				object child = adaptor.GetChild(t, i);
				_Index(child, m);
			}
		}
	}

	public IList Find(object t, int ttype)
	{
		IList list = new List<object>();
		Visit(t, ttype, new RecordAllElementsVisitor(list));
		return list;
	}

	public IList Find(object t, string pattern)
	{
		IList list = new List<object>();
		TreePatternLexer tokenizer = new TreePatternLexer(pattern);
		TreePatternParser treePatternParser = new TreePatternParser(tokenizer, this, new TreePatternTreeAdaptor());
		TreePattern treePattern = (TreePattern)treePatternParser.Pattern();
		if (treePattern == null || treePattern.IsNil || (object)treePattern.GetType() == typeof(WildcardTreePattern))
		{
			return null;
		}
		int type = treePattern.Type;
		Visit(t, type, new PatternMatchingContextVisitor(this, treePattern, list));
		return list;
	}

	public object FindFirst(object t, int ttype)
	{
		return null;
	}

	public object FindFirst(object t, string pattern)
	{
		return null;
	}

	public void Visit(object t, int ttype, ContextVisitor visitor)
	{
		_Visit(t, null, 0, ttype, visitor);
	}

	protected void _Visit(object t, object parent, int childIndex, int ttype, ContextVisitor visitor)
	{
		if (t != null)
		{
			if (adaptor.GetNodeType(t) == ttype)
			{
				visitor.Visit(t, parent, childIndex, null);
			}
			int childCount = adaptor.GetChildCount(t);
			for (int i = 0; i < childCount; i++)
			{
				object child = adaptor.GetChild(t, i);
				_Visit(child, t, i, ttype, visitor);
			}
		}
	}

	public void Visit(object t, string pattern, ContextVisitor visitor)
	{
		TreePatternLexer tokenizer = new TreePatternLexer(pattern);
		TreePatternParser treePatternParser = new TreePatternParser(tokenizer, this, new TreePatternTreeAdaptor());
		TreePattern treePattern = (TreePattern)treePatternParser.Pattern();
		if (treePattern != null && !treePattern.IsNil && (object)treePattern.GetType() != typeof(WildcardTreePattern))
		{
			int type = treePattern.Type;
			Visit(t, type, new InvokeVisitorOnPatternMatchContextVisitor(this, treePattern, visitor));
		}
	}

	public bool Parse(object t, string pattern, IDictionary labels)
	{
		TreePatternLexer tokenizer = new TreePatternLexer(pattern);
		TreePatternParser treePatternParser = new TreePatternParser(tokenizer, this, new TreePatternTreeAdaptor());
		TreePattern t2 = (TreePattern)treePatternParser.Pattern();
		return _Parse(t, t2, labels);
	}

	public bool Parse(object t, string pattern)
	{
		return Parse(t, pattern, null);
	}

	protected bool _Parse(object t1, TreePattern t2, IDictionary labels)
	{
		if (t1 == null || t2 == null)
		{
			return false;
		}
		if ((object)t2.GetType() != typeof(WildcardTreePattern))
		{
			if (adaptor.GetNodeType(t1) != t2.Type)
			{
				return false;
			}
			if (t2.hasTextArg && !adaptor.GetNodeText(t1).Equals(t2.Text))
			{
				return false;
			}
		}
		if (t2.label != null && labels != null)
		{
			labels[t2.label] = t1;
		}
		int childCount = adaptor.GetChildCount(t1);
		int childCount2 = t2.ChildCount;
		if (childCount != childCount2)
		{
			return false;
		}
		for (int i = 0; i < childCount; i++)
		{
			object child = adaptor.GetChild(t1, i);
			TreePattern t3 = (TreePattern)t2.GetChild(i);
			if (!_Parse(child, t3, labels))
			{
				return false;
			}
		}
		return true;
	}

	public object Create(string pattern)
	{
		TreePatternLexer tokenizer = new TreePatternLexer(pattern);
		TreePatternParser treePatternParser = new TreePatternParser(tokenizer, this, adaptor);
		return treePatternParser.Pattern();
	}

	public static bool Equals(object t1, object t2, ITreeAdaptor adaptor)
	{
		return _Equals(t1, t2, adaptor);
	}

	public new bool Equals(object t1, object t2)
	{
		return _Equals(t1, t2, adaptor);
	}

	protected static bool _Equals(object t1, object t2, ITreeAdaptor adaptor)
	{
		if (t1 == null || t2 == null)
		{
			return false;
		}
		if (adaptor.GetNodeType(t1) != adaptor.GetNodeType(t2))
		{
			return false;
		}
		if (!adaptor.GetNodeText(t1).Equals(adaptor.GetNodeText(t2)))
		{
			return false;
		}
		int childCount = adaptor.GetChildCount(t1);
		int childCount2 = adaptor.GetChildCount(t2);
		if (childCount != childCount2)
		{
			return false;
		}
		for (int i = 0; i < childCount; i++)
		{
			object child = adaptor.GetChild(t1, i);
			object child2 = adaptor.GetChild(t2, i);
			if (!_Equals(child, child2, adaptor))
			{
				return false;
			}
		}
		return true;
	}
}
