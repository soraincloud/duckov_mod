#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements.StyleSheets;

internal class StyleSheetBuilder
{
	public struct ComplexSelectorScope : IDisposable
	{
		private StyleSheetBuilder m_Builder;

		public ComplexSelectorScope(StyleSheetBuilder builder)
		{
			m_Builder = builder;
		}

		public void Dispose()
		{
			m_Builder.EndComplexSelector();
		}
	}

	private enum BuilderState
	{
		Init,
		Rule,
		ComplexSelector,
		Property
	}

	private BuilderState m_BuilderState;

	private List<float> m_Floats = new List<float>();

	private List<Dimension> m_Dimensions = new List<Dimension>();

	private List<Color> m_Colors = new List<Color>();

	private List<string> m_Strings = new List<string>();

	private List<StyleRule> m_Rules = new List<StyleRule>();

	private List<Object> m_Assets = new List<Object>();

	private List<ScalableImage> m_ScalableImages = new List<ScalableImage>();

	private List<StyleComplexSelector> m_ComplexSelectors = new List<StyleComplexSelector>();

	private List<StyleProperty> m_CurrentProperties = new List<StyleProperty>();

	private List<StyleValueHandle> m_CurrentValues = new List<StyleValueHandle>();

	private StyleComplexSelector m_CurrentComplexSelector;

	private List<StyleSelector> m_CurrentSelectors = new List<StyleSelector>();

	private StyleProperty m_CurrentProperty;

	private StyleRule m_CurrentRule;

	private List<StyleSheet.ImportStruct> m_Imports = new List<StyleSheet.ImportStruct>();

	public StyleProperty currentProperty => m_CurrentProperty;

	public StyleRule BeginRule(int ruleLine)
	{
		Log("Beginning rule");
		Debug.Assert(m_BuilderState == BuilderState.Init);
		m_BuilderState = BuilderState.Rule;
		m_CurrentRule = new StyleRule
		{
			line = ruleLine
		};
		return m_CurrentRule;
	}

	public ComplexSelectorScope BeginComplexSelector(int specificity)
	{
		Log("Begin complex selector with specificity " + specificity);
		Debug.Assert(m_BuilderState == BuilderState.Rule);
		m_BuilderState = BuilderState.ComplexSelector;
		m_CurrentComplexSelector = new StyleComplexSelector();
		m_CurrentComplexSelector.specificity = specificity;
		m_CurrentComplexSelector.ruleIndex = m_Rules.Count;
		return new ComplexSelectorScope(this);
	}

	public void AddSimpleSelector(StyleSelectorPart[] parts, StyleSelectorRelationship previousRelationsip)
	{
		Debug.Assert(m_BuilderState == BuilderState.ComplexSelector);
		StyleSelector styleSelector = new StyleSelector();
		styleSelector.parts = parts;
		styleSelector.previousRelationship = previousRelationsip;
		Log("Add simple selector " + styleSelector);
		m_CurrentSelectors.Add(styleSelector);
	}

	public void EndComplexSelector()
	{
		Log("Ending complex selector");
		Debug.Assert(m_BuilderState == BuilderState.ComplexSelector);
		m_BuilderState = BuilderState.Rule;
		if (m_CurrentSelectors.Count > 0)
		{
			m_CurrentComplexSelector.selectors = m_CurrentSelectors.ToArray();
			m_ComplexSelectors.Add(m_CurrentComplexSelector);
			m_CurrentSelectors.Clear();
		}
		m_CurrentComplexSelector = null;
	}

	public StyleProperty BeginProperty(string name, int line = -1)
	{
		Log("Begin property named " + name);
		Debug.Assert(m_BuilderState == BuilderState.Rule);
		m_BuilderState = BuilderState.Property;
		m_CurrentProperty = new StyleProperty
		{
			name = name,
			line = line
		};
		m_CurrentProperties.Add(m_CurrentProperty);
		return m_CurrentProperty;
	}

	public void AddImport(StyleSheet.ImportStruct importStruct)
	{
		m_Imports.Add(importStruct);
	}

	public void AddValue(float value)
	{
		RegisterValue(m_Floats, StyleValueType.Float, value);
	}

	public void AddValue(Dimension value)
	{
		RegisterValue(m_Dimensions, StyleValueType.Dimension, value);
	}

	public void AddValue(StyleValueKeyword keyword)
	{
		m_CurrentValues.Add(new StyleValueHandle((int)keyword, StyleValueType.Keyword));
	}

	public void AddValue(StyleValueFunction function)
	{
		m_CurrentValues.Add(new StyleValueHandle((int)function, StyleValueType.Function));
	}

	public void AddCommaSeparator()
	{
		m_CurrentValues.Add(new StyleValueHandle(0, StyleValueType.CommaSeparator));
	}

	public void AddValue(string value, StyleValueType type)
	{
		if (type == StyleValueType.Variable)
		{
			RegisterVariable(value);
		}
		else
		{
			RegisterValue(m_Strings, type, value);
		}
	}

	public void AddValue(Color value)
	{
		RegisterValue(m_Colors, StyleValueType.Color, value);
	}

	public void AddValue(Object value)
	{
		RegisterValue(m_Assets, StyleValueType.AssetReference, value);
	}

	public void AddValue(ScalableImage value)
	{
		RegisterValue(m_ScalableImages, StyleValueType.ScalableImage, value);
	}

	public void EndProperty()
	{
		Log("Ending property");
		Debug.Assert(m_BuilderState == BuilderState.Property);
		m_BuilderState = BuilderState.Rule;
		m_CurrentProperty.values = m_CurrentValues.ToArray();
		m_CurrentProperty = null;
		m_CurrentValues.Clear();
	}

	public int EndRule()
	{
		Log("Ending rule");
		Debug.Assert(m_BuilderState == BuilderState.Rule);
		m_BuilderState = BuilderState.Init;
		m_CurrentRule.properties = m_CurrentProperties.ToArray();
		m_Rules.Add(m_CurrentRule);
		m_CurrentRule = null;
		m_CurrentProperties.Clear();
		return m_Rules.Count - 1;
	}

	public void BuildTo(StyleSheet writeTo)
	{
		Debug.Assert(m_BuilderState == BuilderState.Init);
		writeTo.floats = m_Floats.ToArray();
		writeTo.dimensions = m_Dimensions.ToArray();
		writeTo.colors = m_Colors.ToArray();
		writeTo.strings = m_Strings.ToArray();
		writeTo.rules = m_Rules.ToArray();
		writeTo.assets = m_Assets.ToArray();
		writeTo.scalableImages = m_ScalableImages.ToArray();
		writeTo.complexSelectors = m_ComplexSelectors.ToArray();
		writeTo.imports = m_Imports.ToArray();
		if (writeTo.imports.Length != 0)
		{
			writeTo.FlattenImportedStyleSheetsRecursive();
		}
	}

	private void RegisterVariable(string value)
	{
		Log("Add variable : " + value);
		Debug.Assert(m_BuilderState == BuilderState.Property);
		int num = m_Strings.IndexOf(value);
		if (num < 0)
		{
			m_Strings.Add(value);
			num = m_Strings.Count - 1;
		}
		m_CurrentValues.Add(new StyleValueHandle(num, StyleValueType.Variable));
	}

	private void RegisterValue<T>(List<T> list, StyleValueType type, T value)
	{
		string text = type.ToString();
		T val = value;
		Log("Add value of type " + text + " : " + val);
		Debug.Assert(m_BuilderState == BuilderState.Property);
		list.Add(value);
		m_CurrentValues.Add(new StyleValueHandle(list.Count - 1, type));
	}

	private static void Log(string msg)
	{
	}
}
