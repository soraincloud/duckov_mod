using System.Collections.Generic;
using UnityEngine.UIElements.StyleSheets;

namespace UnityEngine.UIElements;

internal class VisualTreeStyleUpdaterTraversal : HierarchyTraversal
{
	private StyleVariableContext m_ProcessVarContext = new StyleVariableContext();

	private HashSet<VisualElement> m_UpdateList = new HashSet<VisualElement>();

	private HashSet<VisualElement> m_ParentList = new HashSet<VisualElement>();

	private List<SelectorMatchRecord> m_TempMatchResults = new List<SelectorMatchRecord>();

	private StyleMatchingContext m_StyleMatchingContext = new StyleMatchingContext(OnProcessMatchResult);

	private StylePropertyReader m_StylePropertyReader = new StylePropertyReader();

	private readonly List<StylePropertyId> m_AnimatedProperties = new List<StylePropertyId>();

	private float currentPixelsPerPoint { get; set; } = 1f;

	public StyleMatchingContext styleMatchingContext => m_StyleMatchingContext;

	public void PrepareTraversal(float pixelsPerPoint)
	{
		currentPixelsPerPoint = pixelsPerPoint;
	}

	public void AddChangedElement(VisualElement ve, VersionChangeType versionChangeType)
	{
		m_UpdateList.Add(ve);
		if ((versionChangeType & VersionChangeType.StyleSheet) == VersionChangeType.StyleSheet)
		{
			PropagateToChildren(ve);
		}
		PropagateToParents(ve);
	}

	public void Clear()
	{
		m_UpdateList.Clear();
		m_ParentList.Clear();
		m_TempMatchResults.Clear();
	}

	private void PropagateToChildren(VisualElement ve)
	{
		int childCount = ve.hierarchy.childCount;
		for (int i = 0; i < childCount; i++)
		{
			VisualElement visualElement = ve.hierarchy[i];
			if (m_UpdateList.Add(visualElement))
			{
				PropagateToChildren(visualElement);
			}
		}
	}

	private void PropagateToParents(VisualElement ve)
	{
		VisualElement parent = ve.hierarchy.parent;
		while (parent != null && m_ParentList.Add(parent))
		{
			parent = parent.hierarchy.parent;
		}
	}

	private static void OnProcessMatchResult(VisualElement current, MatchResultInfo info)
	{
		current.triggerPseudoMask |= info.triggerPseudoMask;
		current.dependencyPseudoMask |= info.dependencyPseudoMask;
	}

	public override void TraverseRecursive(VisualElement element, int depth)
	{
		if (ShouldSkipElement(element))
		{
			return;
		}
		bool flag = m_UpdateList.Contains(element);
		if (flag)
		{
			element.triggerPseudoMask = (PseudoStates)0;
			element.dependencyPseudoMask = (PseudoStates)0;
		}
		int styleSheetCount = m_StyleMatchingContext.styleSheetCount;
		if (element.styleSheetList != null)
		{
			for (int i = 0; i < element.styleSheetList.Count; i++)
			{
				StyleSheet styleSheet = element.styleSheetList[i];
				if (styleSheet.flattenedRecursiveImports != null)
				{
					for (int j = 0; j < styleSheet.flattenedRecursiveImports.Count; j++)
					{
						m_StyleMatchingContext.AddStyleSheet(styleSheet.flattenedRecursiveImports[j]);
					}
				}
				m_StyleMatchingContext.AddStyleSheet(styleSheet);
			}
		}
		StyleVariableContext variableContext = m_StyleMatchingContext.variableContext;
		int customPropertiesCount = element.computedStyle.customPropertiesCount;
		if (flag)
		{
			m_StyleMatchingContext.currentElement = element;
			StyleSelectorHelper.FindMatches(m_StyleMatchingContext, m_TempMatchResults, styleSheetCount - 1);
			ComputedStyle computedStyle = ProcessMatchedRules(element, m_TempMatchResults);
			computedStyle.Acquire();
			if (element.hasInlineStyle)
			{
				element.inlineStyleAccess.ApplyInlineStyles(ref computedStyle);
			}
			ComputedTransitionUtils.UpdateComputedTransitions(ref computedStyle);
			if (element.hasRunningAnimations && !ComputedTransitionUtils.SameTransitionProperty(ref element.computedStyle, ref computedStyle))
			{
				CancelAnimationsWithNoTransitionProperty(element, ref computedStyle);
			}
			if (computedStyle.hasTransition && element.styleInitialized)
			{
				ProcessTransitions(element, ref element.computedStyle, ref computedStyle);
				element.SetComputedStyle(ref computedStyle);
				ForceUpdateTransitions(element);
			}
			else
			{
				element.SetComputedStyle(ref computedStyle);
			}
			computedStyle.Release();
			element.styleInitialized = true;
			element.inheritedStylesHash = element.computedStyle.inheritedData.GetHashCode();
			m_StyleMatchingContext.currentElement = null;
			m_TempMatchResults.Clear();
		}
		else
		{
			m_StyleMatchingContext.variableContext = element.variableContext;
		}
		if (flag && (customPropertiesCount > 0 || element.computedStyle.customPropertiesCount > 0) && element.HasEventCallbacksOrDefaultActions(EventBase<CustomStyleResolvedEvent>.EventCategory))
		{
			using CustomStyleResolvedEvent customStyleResolvedEvent = EventBase<CustomStyleResolvedEvent>.GetPooled();
			customStyleResolvedEvent.target = element;
			element.HandleEventAtTargetAndDefaultPhase(customStyleResolvedEvent);
		}
		m_StyleMatchingContext.ancestorFilter.PushElement(element);
		Recurse(element, depth);
		m_StyleMatchingContext.ancestorFilter.PopElement();
		m_StyleMatchingContext.variableContext = variableContext;
		if (m_StyleMatchingContext.styleSheetCount > styleSheetCount)
		{
			m_StyleMatchingContext.RemoveStyleSheetRange(styleSheetCount, m_StyleMatchingContext.styleSheetCount - styleSheetCount);
		}
	}

	private void ProcessTransitions(VisualElement element, ref ComputedStyle oldStyle, ref ComputedStyle newStyle)
	{
		for (int num = newStyle.computedTransitions.Length - 1; num >= 0; num--)
		{
			ComputedTransitionProperty computedTransitionProperty = newStyle.computedTransitions[num];
			if (!element.hasInlineStyle || !element.inlineStyleAccess.IsValueSet(computedTransitionProperty.id))
			{
				ComputedStyle.StartAnimation(element, computedTransitionProperty.id, ref oldStyle, ref newStyle, computedTransitionProperty.durationMs, computedTransitionProperty.delayMs, computedTransitionProperty.easingCurve);
			}
		}
	}

	private void ForceUpdateTransitions(VisualElement element)
	{
		element.styleAnimation.GetAllAnimations(m_AnimatedProperties);
		if (m_AnimatedProperties.Count <= 0)
		{
			return;
		}
		foreach (StylePropertyId animatedProperty in m_AnimatedProperties)
		{
			element.styleAnimation.UpdateAnimation(animatedProperty);
		}
		m_AnimatedProperties.Clear();
	}

	internal void CancelAnimationsWithNoTransitionProperty(VisualElement element, ref ComputedStyle newStyle)
	{
		element.styleAnimation.GetAllAnimations(m_AnimatedProperties);
		foreach (StylePropertyId animatedProperty in m_AnimatedProperties)
		{
			if (!newStyle.HasTransitionProperty(animatedProperty))
			{
				element.styleAnimation.CancelAnimation(animatedProperty);
			}
		}
		m_AnimatedProperties.Clear();
	}

	protected bool ShouldSkipElement(VisualElement element)
	{
		return !m_ParentList.Contains(element) && !m_UpdateList.Contains(element);
	}

	private ComputedStyle ProcessMatchedRules(VisualElement element, List<SelectorMatchRecord> matchingSelectors)
	{
		matchingSelectors.Sort((SelectorMatchRecord a, SelectorMatchRecord b) => SelectorMatchRecord.Compare(a, b));
		long num = element.fullTypeName.GetHashCode();
		num = (num * 397) ^ currentPixelsPerPoint.GetHashCode();
		int variableHash = m_StyleMatchingContext.variableContext.GetVariableHash();
		int num2 = 0;
		foreach (SelectorMatchRecord matchingSelector in matchingSelectors)
		{
			num2 += matchingSelector.complexSelector.rule.customPropertiesCount;
		}
		if (num2 > 0)
		{
			m_ProcessVarContext.AddInitialRange(m_StyleMatchingContext.variableContext);
		}
		foreach (SelectorMatchRecord matchingSelector2 in matchingSelectors)
		{
			StyleSheet sheet = matchingSelector2.sheet;
			int ruleIndex = matchingSelector2.complexSelector.ruleIndex;
			int specificity = matchingSelector2.complexSelector.specificity;
			num = (num * 397) ^ sheet.contentHash;
			num = (num * 397) ^ ruleIndex;
			num = (num * 397) ^ specificity;
			StyleRule rule = matchingSelector2.complexSelector.rule;
			if (rule.customPropertiesCount > 0)
			{
				ProcessMatchedVariables(matchingSelector2.sheet, rule);
			}
		}
		VisualElement parent = element.hierarchy.parent;
		int num3 = parent?.inheritedStylesHash ?? 0;
		num = (num * 397) ^ num3;
		int num4 = variableHash;
		if (num2 > 0)
		{
			num4 = m_ProcessVarContext.GetVariableHash();
		}
		num = (num * 397) ^ num4;
		if (variableHash != num4)
		{
			if (!StyleCache.TryGetValue(num4, out StyleVariableContext data))
			{
				data = new StyleVariableContext(m_ProcessVarContext);
				StyleCache.SetValue(num4, data);
			}
			m_StyleMatchingContext.variableContext = data;
		}
		element.variableContext = m_StyleMatchingContext.variableContext;
		m_ProcessVarContext.Clear();
		if (!StyleCache.TryGetValue(num, out var data2))
		{
			ref ComputedStyle reference;
			if (parent != null)
			{
				_ = ref parent.computedStyle;
				reference = ref parent.computedStyle;
			}
			else
			{
				reference = ref InitialStyle.Get();
			}
			ref ComputedStyle parentStyle = ref reference;
			data2 = ComputedStyle.Create(ref parentStyle);
			data2.matchingRulesHash = num;
			float scaledPixelsPerPoint = element.scaledPixelsPerPoint;
			foreach (SelectorMatchRecord matchingSelector3 in matchingSelectors)
			{
				m_StylePropertyReader.SetContext(matchingSelector3.sheet, matchingSelector3.complexSelector, m_StyleMatchingContext.variableContext, scaledPixelsPerPoint);
				data2.ApplyProperties(m_StylePropertyReader, ref parentStyle);
			}
			data2.FinalizeApply(ref parentStyle);
			StyleCache.SetValue(num, ref data2);
		}
		return data2;
	}

	private void ProcessMatchedVariables(StyleSheet sheet, StyleRule rule)
	{
		StyleProperty[] properties = rule.properties;
		foreach (StyleProperty styleProperty in properties)
		{
			if (styleProperty.isCustomProperty)
			{
				StyleVariable sv = new StyleVariable(styleProperty.name, sheet, styleProperty.values);
				m_ProcessVarContext.Add(sv);
			}
		}
	}
}
