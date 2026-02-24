using System;
using System.Collections.Generic;
using UnityEngine.UIElements.StyleSheets;

namespace UnityEngine.UIElements;

internal class StyleMatchingContext
{
	private List<StyleSheet> m_StyleSheetStack;

	public StyleVariableContext variableContext;

	public VisualElement currentElement;

	public Action<VisualElement, MatchResultInfo> processResult;

	public AncestorFilter ancestorFilter = new AncestorFilter();

	public int styleSheetCount => m_StyleSheetStack.Count;

	public StyleMatchingContext(Action<VisualElement, MatchResultInfo> processResult)
	{
		m_StyleSheetStack = new List<StyleSheet>();
		variableContext = StyleVariableContext.none;
		currentElement = null;
		this.processResult = processResult;
	}

	public void AddStyleSheet(StyleSheet sheet)
	{
		if (!(sheet == null))
		{
			m_StyleSheetStack.Add(sheet);
		}
	}

	public void RemoveStyleSheetRange(int index, int count)
	{
		m_StyleSheetStack.RemoveRange(index, count);
	}

	public StyleSheet GetStyleSheetAt(int index)
	{
		return m_StyleSheetStack[index];
	}
}
