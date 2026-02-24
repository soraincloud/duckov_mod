using System;

namespace Unity.VisualScripting.Antlr3.Runtime;

public class RuleReturnScope
{
	public virtual object Start
	{
		get
		{
			return null;
		}
		set
		{
			throw new NotSupportedException("Setter has not been defined for this property.");
		}
	}

	public virtual object Stop
	{
		get
		{
			return null;
		}
		set
		{
			throw new NotSupportedException("Setter has not been defined for this property.");
		}
	}

	public virtual object Tree
	{
		get
		{
			return null;
		}
		set
		{
			throw new NotSupportedException("Setter has not been defined for this property.");
		}
	}

	public virtual object Template => null;
}
