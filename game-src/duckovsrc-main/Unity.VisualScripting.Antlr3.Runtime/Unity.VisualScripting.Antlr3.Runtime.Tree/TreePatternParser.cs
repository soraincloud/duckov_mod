using System;

namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

public class TreePatternParser
{
	protected TreePatternLexer tokenizer;

	protected int ttype;

	protected TreeWizard wizard;

	protected ITreeAdaptor adaptor;

	public TreePatternParser(TreePatternLexer tokenizer, TreeWizard wizard, ITreeAdaptor adaptor)
	{
		this.tokenizer = tokenizer;
		this.wizard = wizard;
		this.adaptor = adaptor;
		ttype = tokenizer.NextToken();
	}

	public object Pattern()
	{
		if (ttype == 1)
		{
			return ParseTree();
		}
		if (ttype == 3)
		{
			object result = ParseNode();
			if (ttype == -1)
			{
				return result;
			}
			return null;
		}
		return null;
	}

	public object ParseTree()
	{
		if (ttype != 1)
		{
			Console.Out.WriteLine("no BEGIN");
			return null;
		}
		ttype = tokenizer.NextToken();
		object obj = ParseNode();
		if (obj == null)
		{
			return null;
		}
		while (ttype == 1 || ttype == 3 || ttype == 5 || ttype == 7)
		{
			if (ttype == 1)
			{
				object child = ParseTree();
				adaptor.AddChild(obj, child);
				continue;
			}
			object obj2 = ParseNode();
			if (obj2 == null)
			{
				return null;
			}
			adaptor.AddChild(obj, obj2);
		}
		if (ttype != 2)
		{
			Console.Out.WriteLine("no END");
			return null;
		}
		ttype = tokenizer.NextToken();
		return obj;
	}

	public object ParseNode()
	{
		string text = null;
		if (ttype == 5)
		{
			ttype = tokenizer.NextToken();
			if (ttype != 3)
			{
				return null;
			}
			text = tokenizer.sval.ToString();
			ttype = tokenizer.NextToken();
			if (ttype != 6)
			{
				return null;
			}
			ttype = tokenizer.NextToken();
		}
		if (ttype == 7)
		{
			ttype = tokenizer.NextToken();
			IToken payload = new CommonToken(0, ".");
			TreeWizard.TreePattern treePattern = new TreeWizard.WildcardTreePattern(payload);
			if (text != null)
			{
				treePattern.label = text;
			}
			return treePattern;
		}
		if (ttype != 3)
		{
			return null;
		}
		string text2 = tokenizer.sval.ToString();
		ttype = tokenizer.NextToken();
		if (text2.Equals("nil"))
		{
			return adaptor.GetNilNode();
		}
		string text3 = text2;
		string text4 = null;
		if (ttype == 4)
		{
			text4 = tokenizer.sval.ToString();
			text3 = text4;
			ttype = tokenizer.NextToken();
		}
		int tokenType = wizard.GetTokenType(text2);
		if (tokenType == 0)
		{
			return null;
		}
		object obj = adaptor.Create(tokenType, text3);
		if (text != null && (object)obj.GetType() == typeof(TreeWizard.TreePattern))
		{
			((TreeWizard.TreePattern)obj).label = text;
		}
		if (text4 != null && (object)obj.GetType() == typeof(TreeWizard.TreePattern))
		{
			((TreeWizard.TreePattern)obj).hasTextArg = true;
		}
		return obj;
	}
}
