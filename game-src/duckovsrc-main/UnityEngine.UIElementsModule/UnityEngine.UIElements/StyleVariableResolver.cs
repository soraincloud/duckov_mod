#define UNITY_ASSERTIONS
using System.Collections.Generic;
using UnityEngine.UIElements.StyleSheets;
using UnityEngine.UIElements.StyleSheets.Syntax;

namespace UnityEngine.UIElements;

internal class StyleVariableResolver
{
	private enum Result
	{
		Valid,
		Invalid,
		NotFound
	}

	private struct ResolveContext
	{
		public StyleSheet sheet;

		public StyleValueHandle[] handles;
	}

	internal const int kMaxResolves = 100;

	private static StyleSyntaxParser s_SyntaxParser = new StyleSyntaxParser();

	private StylePropertyValueMatcher m_Matcher = new StylePropertyValueMatcher();

	private List<StylePropertyValue> m_ResolvedValues = new List<StylePropertyValue>();

	private Stack<string> m_ResolvedVarStack = new Stack<string>();

	private StyleProperty m_Property;

	private Stack<ResolveContext> m_ContextStack = new Stack<ResolveContext>();

	private ResolveContext m_CurrentContext;

	private StyleSheet currentSheet => m_CurrentContext.sheet;

	private StyleValueHandle[] currentHandles => m_CurrentContext.handles;

	public List<StylePropertyValue> resolvedValues => m_ResolvedValues;

	public StyleVariableContext variableContext { get; set; }

	public void Init(StyleProperty property, StyleSheet sheet, StyleValueHandle[] handles)
	{
		m_ResolvedValues.Clear();
		m_ContextStack.Clear();
		m_Property = property;
		PushContext(sheet, handles);
	}

	private void PushContext(StyleSheet sheet, StyleValueHandle[] handles)
	{
		m_CurrentContext = new ResolveContext
		{
			sheet = sheet,
			handles = handles
		};
		m_ContextStack.Push(m_CurrentContext);
	}

	private void PopContext()
	{
		m_ContextStack.Pop();
		m_CurrentContext = m_ContextStack.Peek();
	}

	public void AddValue(StyleValueHandle handle)
	{
		m_ResolvedValues.Add(new StylePropertyValue
		{
			sheet = currentSheet,
			handle = handle
		});
	}

	public bool ResolveVarFunction(ref int index)
	{
		m_ResolvedVarStack.Clear();
		ParseVarFunction(currentSheet, currentHandles, ref index, out var argCount, out var variableName);
		Result result = ResolveVarFunction(ref index, argCount, variableName);
		return result == Result.Valid;
	}

	private Result ResolveVarFunction(ref int index, int argc, string varName)
	{
		Result result = ResolveVariable(varName);
		if (result == Result.NotFound && argc > 1)
		{
			StyleValueHandle styleValueHandle = currentHandles[++index];
			Debug.Assert(styleValueHandle.valueType == StyleValueType.CommaSeparator, $"Unexpected value type {styleValueHandle.valueType} in var function");
			if (styleValueHandle.valueType == StyleValueType.CommaSeparator && index + 1 < currentHandles.Length)
			{
				index++;
				result = ResolveFallback(ref index);
			}
		}
		return result;
	}

	public bool ValidateResolvedValues()
	{
		if (m_Property.isCustomProperty)
		{
			return true;
		}
		if (!StylePropertyCache.TryGetSyntax(m_Property.name, out var syntax))
		{
			Debug.LogAssertion("Unknown style property " + m_Property.name);
			return false;
		}
		Expression exp = s_SyntaxParser.Parse(syntax);
		return m_Matcher.Match(exp, m_ResolvedValues).success;
	}

	private Result ResolveVariable(string variableName)
	{
		if (!variableContext.TryFindVariable(variableName, out var v))
		{
			return Result.NotFound;
		}
		if (m_ResolvedVarStack.Contains(v.name))
		{
			return Result.NotFound;
		}
		m_ResolvedVarStack.Push(v.name);
		Result result = Result.Valid;
		for (int i = 0; i < v.handles.Length; i++)
		{
			if (result != Result.Valid)
			{
				break;
			}
			if (m_ResolvedValues.Count + 1 > 100)
			{
				return Result.Invalid;
			}
			StyleValueHandle handle = v.handles[i];
			if (handle.IsVarFunction())
			{
				PushContext(v.sheet, v.handles);
				ParseVarFunction(v.sheet, v.handles, ref i, out var argCount, out var variableName2);
				result = ResolveVarFunction(ref i, argCount, variableName2);
				PopContext();
			}
			else
			{
				m_ResolvedValues.Add(new StylePropertyValue
				{
					sheet = v.sheet,
					handle = handle
				});
			}
		}
		m_ResolvedVarStack.Pop();
		return result;
	}

	private Result ResolveFallback(ref int index)
	{
		Result result = Result.Valid;
		while (index < currentHandles.Length && result == Result.Valid)
		{
			StyleValueHandle handle = currentHandles[index];
			if (handle.IsVarFunction())
			{
				ParseVarFunction(currentSheet, currentHandles, ref index, out var argCount, out var variableName);
				result = ResolveVariable(variableName);
				if (result == Result.NotFound && argCount > 1)
				{
					handle = currentHandles[++index];
					Debug.Assert(handle.valueType == StyleValueType.CommaSeparator, $"Unexpected value type {handle.valueType} in var function");
					if (handle.valueType == StyleValueType.CommaSeparator && index + 1 < currentHandles.Length)
					{
						index++;
						result = ResolveFallback(ref index);
					}
				}
			}
			else
			{
				m_ResolvedValues.Add(new StylePropertyValue
				{
					sheet = currentSheet,
					handle = handle
				});
			}
			index++;
		}
		return result;
	}

	private static void ParseVarFunction(StyleSheet sheet, StyleValueHandle[] handles, ref int index, out int argCount, out string variableName)
	{
		argCount = (int)sheet.ReadFloat(handles[++index]);
		variableName = sheet.ReadVariable(handles[++index]);
	}
}
