namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

public class TreeRuleReturnScope : RuleReturnScope
{
	private object start;

	public override object Start
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
}
