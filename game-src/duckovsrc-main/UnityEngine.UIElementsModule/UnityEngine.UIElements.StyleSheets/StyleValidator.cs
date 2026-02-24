#define UNITY_ASSERTIONS
using UnityEngine.UIElements.StyleSheets.Syntax;

namespace UnityEngine.UIElements.StyleSheets;

internal class StyleValidator
{
	private StyleSyntaxParser m_SyntaxParser;

	private StyleMatcher m_StyleMatcher;

	public StyleValidator()
	{
		m_SyntaxParser = new StyleSyntaxParser();
		m_StyleMatcher = new StyleMatcher();
	}

	public StyleValidationResult ValidateProperty(string name, string value)
	{
		StyleValidationResult result = new StyleValidationResult
		{
			status = StyleValidationStatus.Ok
		};
		if (name.StartsWith("--"))
		{
			return result;
		}
		if (!StylePropertyCache.TryGetSyntax(name, out var syntax))
		{
			string text = StylePropertyCache.FindClosestPropertyName(name);
			result.status = StyleValidationStatus.Error;
			result.message = "Unknown property '" + name + "'";
			if (!string.IsNullOrEmpty(text))
			{
				result.message = result.message + " (did you mean '" + text + "'?)";
			}
			return result;
		}
		Expression expression = m_SyntaxParser.Parse(syntax);
		if (expression == null)
		{
			result.status = StyleValidationStatus.Error;
			result.message = "Invalid '" + name + "' property syntax '" + syntax + "'";
			return result;
		}
		MatchResult matchResult = m_StyleMatcher.Match(expression, value);
		if (!matchResult.success)
		{
			result.errorValue = matchResult.errorValue;
			switch (matchResult.errorCode)
			{
			case MatchResultErrorCode.Syntax:
			{
				result.status = StyleValidationStatus.Error;
				if (IsUnitMissing(syntax, value, out var unitHint))
				{
					result.hint = "Property expects a unit. Did you forget to add " + unitHint + "?";
				}
				else if (IsUnsupportedColor(syntax))
				{
					result.hint = "Unsupported color '" + value + "'.";
				}
				result.message = "Expected (" + syntax + ") but found '" + matchResult.errorValue + "'";
				break;
			}
			case MatchResultErrorCode.EmptyValue:
				result.status = StyleValidationStatus.Error;
				result.message = "Expected (" + syntax + ") but found empty value";
				break;
			case MatchResultErrorCode.ExpectedEndOfValue:
				result.status = StyleValidationStatus.Warning;
				result.message = "Expected end of value but found '" + matchResult.errorValue + "'";
				break;
			default:
				Debug.LogAssertion($"Unexpected error code '{matchResult.errorCode}'");
				break;
			}
		}
		return result;
	}

	private bool IsUnitMissing(string propertySyntax, string propertyValue, out string unitHint)
	{
		unitHint = null;
		if (!float.TryParse(propertyValue, out var _))
		{
			return false;
		}
		if (propertySyntax.Contains("<length>") || propertySyntax.Contains("<length-percentage>"))
		{
			unitHint = "px or %";
		}
		else if (propertySyntax.Contains("<time>"))
		{
			unitHint = "s or ms";
		}
		return !string.IsNullOrEmpty(unitHint);
	}

	private bool IsUnsupportedColor(string propertySyntax)
	{
		return propertySyntax.StartsWith("<color>");
	}
}
