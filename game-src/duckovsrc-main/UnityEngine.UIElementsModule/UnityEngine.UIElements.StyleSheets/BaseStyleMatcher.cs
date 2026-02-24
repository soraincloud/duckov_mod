#define UNITY_ASSERTIONS
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UIElements.StyleSheets.Syntax;

namespace UnityEngine.UIElements.StyleSheets;

internal abstract class BaseStyleMatcher
{
	private struct MatchContext
	{
		public int valueIndex;

		public int matchedVariableCount;
	}

	protected static readonly Regex s_CustomIdentRegex = new Regex("^-?[_a-z][_a-z0-9-]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private Stack<MatchContext> m_ContextStack = new Stack<MatchContext>();

	private MatchContext m_CurrentContext;

	public abstract int valueCount { get; }

	public abstract bool isCurrentVariable { get; }

	public abstract bool isCurrentComma { get; }

	public bool hasCurrent => m_CurrentContext.valueIndex < valueCount;

	public int currentIndex
	{
		get
		{
			return m_CurrentContext.valueIndex;
		}
		set
		{
			m_CurrentContext.valueIndex = value;
		}
	}

	public int matchedVariableCount
	{
		get
		{
			return m_CurrentContext.matchedVariableCount;
		}
		set
		{
			m_CurrentContext.matchedVariableCount = value;
		}
	}

	protected abstract bool MatchKeyword(string keyword);

	protected abstract bool MatchNumber();

	protected abstract bool MatchInteger();

	protected abstract bool MatchLength();

	protected abstract bool MatchPercentage();

	protected abstract bool MatchColor();

	protected abstract bool MatchResource();

	protected abstract bool MatchUrl();

	protected abstract bool MatchTime();

	protected abstract bool MatchAngle();

	protected abstract bool MatchCustomIdent();

	protected void Initialize()
	{
		m_CurrentContext = default(MatchContext);
		m_ContextStack.Clear();
	}

	public void MoveNext()
	{
		if (currentIndex + 1 <= valueCount)
		{
			currentIndex++;
		}
	}

	public void SaveContext()
	{
		m_ContextStack.Push(m_CurrentContext);
	}

	public void RestoreContext()
	{
		m_CurrentContext = m_ContextStack.Pop();
	}

	public void DropContext()
	{
		m_ContextStack.Pop();
	}

	protected bool Match(Expression exp)
	{
		bool flag = true;
		if (exp.multiplier.type == ExpressionMultiplierType.None)
		{
			return MatchExpression(exp);
		}
		Debug.Assert(exp.multiplier.type != ExpressionMultiplierType.GroupAtLeastOne, "'!' multiplier in syntax expression is not supported");
		return MatchExpressionWithMultiplier(exp);
	}

	private bool MatchExpression(Expression exp)
	{
		bool flag = false;
		if (exp.type == ExpressionType.Combinator)
		{
			flag = MatchCombinator(exp);
		}
		else
		{
			if (isCurrentVariable)
			{
				flag = true;
				matchedVariableCount++;
			}
			else if (exp.type == ExpressionType.Data)
			{
				flag = MatchDataType(exp);
			}
			else if (exp.type == ExpressionType.Keyword)
			{
				flag = MatchKeyword(exp.keyword);
			}
			if (flag)
			{
				MoveNext();
			}
		}
		if (!flag && !hasCurrent && matchedVariableCount > 0)
		{
			flag = true;
		}
		return flag;
	}

	private bool MatchExpressionWithMultiplier(Expression exp)
	{
		bool flag = exp.multiplier.type == ExpressionMultiplierType.OneOrMoreComma;
		bool flag2 = true;
		int min = exp.multiplier.min;
		int max = exp.multiplier.max;
		int num = 0;
		int num2 = 0;
		while (flag2 && hasCurrent && num2 < max)
		{
			flag2 = MatchExpression(exp);
			if (flag2)
			{
				num++;
				if (flag)
				{
					if (!isCurrentComma)
					{
						break;
					}
					MoveNext();
				}
			}
			num2++;
		}
		flag2 = num >= min && num <= max;
		if (!flag2 && num <= max && matchedVariableCount > 0)
		{
			flag2 = true;
		}
		return flag2;
	}

	private bool MatchGroup(Expression exp)
	{
		Debug.Assert(exp.subExpressions.Length == 1, "Group has invalid number of sub expressions");
		Expression exp2 = exp.subExpressions[0];
		return Match(exp2);
	}

	private bool MatchCombinator(Expression exp)
	{
		SaveContext();
		bool flag = false;
		switch (exp.combinator)
		{
		case ExpressionCombinator.Or:
			flag = MatchOr(exp);
			break;
		case ExpressionCombinator.OrOr:
			flag = MatchOrOr(exp);
			break;
		case ExpressionCombinator.AndAnd:
			flag = MatchAndAnd(exp);
			break;
		case ExpressionCombinator.Juxtaposition:
			flag = MatchJuxtaposition(exp);
			break;
		case ExpressionCombinator.Group:
			flag = MatchGroup(exp);
			break;
		}
		if (flag)
		{
			DropContext();
		}
		else
		{
			RestoreContext();
		}
		return flag;
	}

	private bool MatchOr(Expression exp)
	{
		MatchContext currentContext = default(MatchContext);
		int num = 0;
		for (int i = 0; i < exp.subExpressions.Length; i++)
		{
			SaveContext();
			int num2 = currentIndex;
			bool flag = Match(exp.subExpressions[i]);
			int num3 = currentIndex - num2;
			if (flag && num3 > num)
			{
				num = num3;
				currentContext = m_CurrentContext;
			}
			RestoreContext();
		}
		if (num > 0)
		{
			m_CurrentContext = currentContext;
			return true;
		}
		return false;
	}

	private bool MatchOrOr(Expression exp)
	{
		int num = MatchMany(exp);
		return num > 0;
	}

	private bool MatchAndAnd(Expression exp)
	{
		int num = MatchMany(exp);
		int num2 = exp.subExpressions.Length;
		return num == num2;
	}

	private unsafe int MatchMany(Expression exp)
	{
		MatchContext currentContext = default(MatchContext);
		int num = 0;
		int num2 = -1;
		int num3 = exp.subExpressions.Length;
		int* ptr = stackalloc int[num3];
		do
		{
			SaveContext();
			num2++;
			for (int i = 0; i < num3; i++)
			{
				int num4 = ((num2 > 0) ? ((num2 + i) % num3) : i);
				ptr[i] = num4;
			}
			int num5 = MatchManyByOrder(exp, ptr);
			if (num5 > num)
			{
				num = num5;
				currentContext = m_CurrentContext;
			}
			RestoreContext();
		}
		while (num < num3 && num2 < num3);
		if (num > 0)
		{
			m_CurrentContext = currentContext;
		}
		return num;
	}

	private unsafe int MatchManyByOrder(Expression exp, int* matchOrder)
	{
		int num = exp.subExpressions.Length;
		int* ptr = stackalloc int[num];
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		while (num4 < num && num2 + num3 < num)
		{
			int num5 = matchOrder[num4];
			bool flag = false;
			for (int i = 0; i < num2; i++)
			{
				if (ptr[i] == num5)
				{
					flag = true;
					break;
				}
			}
			bool flag2 = false;
			if (!flag)
			{
				flag2 = Match(exp.subExpressions[num5]);
			}
			if (flag2)
			{
				if (num3 == matchedVariableCount)
				{
					ptr[num2] = num5;
					num2++;
				}
				else
				{
					num3 = matchedVariableCount;
				}
				num4 = 0;
			}
			else
			{
				num4++;
			}
		}
		return num2 + num3;
	}

	private bool MatchJuxtaposition(Expression exp)
	{
		bool flag = true;
		int num = 0;
		while (flag && num < exp.subExpressions.Length)
		{
			flag = Match(exp.subExpressions[num]);
			num++;
		}
		return flag;
	}

	private bool MatchDataType(Expression exp)
	{
		bool result = false;
		if (hasCurrent)
		{
			switch (exp.dataType)
			{
			case DataType.Number:
				result = MatchNumber();
				break;
			case DataType.Integer:
				result = MatchInteger();
				break;
			case DataType.Length:
				result = MatchLength();
				break;
			case DataType.Percentage:
				result = MatchPercentage();
				break;
			case DataType.Color:
				result = MatchColor();
				break;
			case DataType.Resource:
				result = MatchResource();
				break;
			case DataType.Url:
				result = MatchUrl();
				break;
			case DataType.Time:
				result = MatchTime();
				break;
			case DataType.Angle:
				result = MatchAngle();
				break;
			case DataType.CustomIdent:
				result = MatchCustomIdent();
				break;
			}
		}
		return result;
	}
}
