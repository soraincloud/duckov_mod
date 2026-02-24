using System.Collections.Generic;
using UnityEngine.TextCore.Text;

namespace UnityEngine.UIElements.StyleSheets;

internal class StylePropertyReader
{
	internal delegate int GetCursorIdFunction(StyleSheet sheet, StyleValueHandle handle);

	internal static GetCursorIdFunction getCursorIdFunc;

	private List<StylePropertyValue> m_Values = new List<StylePropertyValue>();

	private List<int> m_ValueCount = new List<int>();

	private StyleVariableResolver m_Resolver = new StyleVariableResolver();

	private StyleSheet m_Sheet;

	private StyleProperty[] m_Properties;

	private StylePropertyId[] m_PropertyIds;

	private int m_CurrentValueIndex;

	private int m_CurrentPropertyIndex;

	public StyleProperty property { get; private set; }

	public StylePropertyId propertyId { get; private set; }

	public int valueCount { get; private set; }

	public float dpiScaling { get; private set; }

	public void SetContext(StyleSheet sheet, StyleComplexSelector selector, StyleVariableContext varContext, float dpiScaling = 1f)
	{
		m_Sheet = sheet;
		m_Properties = selector.rule.properties;
		m_PropertyIds = StyleSheetCache.GetPropertyIds(sheet, selector.ruleIndex);
		m_Resolver.variableContext = varContext;
		this.dpiScaling = dpiScaling;
		LoadProperties();
	}

	public void SetInlineContext(StyleSheet sheet, StyleProperty[] properties, StylePropertyId[] propertyIds, float dpiScaling = 1f)
	{
		m_Sheet = sheet;
		m_Properties = properties;
		m_PropertyIds = propertyIds;
		this.dpiScaling = dpiScaling;
		LoadProperties();
	}

	public StylePropertyId MoveNextProperty()
	{
		m_CurrentPropertyIndex++;
		m_CurrentValueIndex += valueCount;
		SetCurrentProperty();
		return propertyId;
	}

	public StylePropertyValue GetValue(int index)
	{
		return m_Values[m_CurrentValueIndex + index];
	}

	public StyleValueType GetValueType(int index)
	{
		return m_Values[m_CurrentValueIndex + index].handle.valueType;
	}

	public bool IsValueType(int index, StyleValueType type)
	{
		return m_Values[m_CurrentValueIndex + index].handle.valueType == type;
	}

	public bool IsKeyword(int index, StyleValueKeyword keyword)
	{
		StylePropertyValue stylePropertyValue = m_Values[m_CurrentValueIndex + index];
		return stylePropertyValue.handle.valueType == StyleValueType.Keyword && stylePropertyValue.handle.valueIndex == (int)keyword;
	}

	public string ReadAsString(int index)
	{
		StylePropertyValue stylePropertyValue = m_Values[m_CurrentValueIndex + index];
		return stylePropertyValue.sheet.ReadAsString(stylePropertyValue.handle);
	}

	public Length ReadLength(int index)
	{
		StylePropertyValue stylePropertyValue = m_Values[m_CurrentValueIndex + index];
		if (stylePropertyValue.handle.valueType == StyleValueType.Keyword)
		{
			return (StyleValueKeyword)stylePropertyValue.handle.valueIndex switch
			{
				StyleValueKeyword.Auto => Length.Auto(), 
				StyleValueKeyword.None => Length.None(), 
				_ => default(Length), 
			};
		}
		return stylePropertyValue.sheet.ReadDimension(stylePropertyValue.handle).ToLength();
	}

	public TimeValue ReadTimeValue(int index)
	{
		StylePropertyValue stylePropertyValue = m_Values[m_CurrentValueIndex + index];
		return stylePropertyValue.sheet.ReadDimension(stylePropertyValue.handle).ToTime();
	}

	public Translate ReadTranslate(int index)
	{
		StylePropertyValue val = m_Values[m_CurrentValueIndex + index];
		StylePropertyValue val2 = ((valueCount > 1) ? m_Values[m_CurrentValueIndex + index + 1] : default(StylePropertyValue));
		StylePropertyValue val3 = ((valueCount > 2) ? m_Values[m_CurrentValueIndex + index + 2] : default(StylePropertyValue));
		return ReadTranslate(valueCount, val, val2, val3);
	}

	public TransformOrigin ReadTransformOrigin(int index)
	{
		StylePropertyValue val = m_Values[m_CurrentValueIndex + index];
		StylePropertyValue val2 = ((valueCount > 1) ? m_Values[m_CurrentValueIndex + index + 1] : default(StylePropertyValue));
		StylePropertyValue zVvalue = ((valueCount > 2) ? m_Values[m_CurrentValueIndex + index + 2] : default(StylePropertyValue));
		return ReadTransformOrigin(valueCount, val, val2, zVvalue);
	}

	public Rotate ReadRotate(int index)
	{
		StylePropertyValue val = m_Values[m_CurrentValueIndex + index];
		StylePropertyValue val2 = ((valueCount > 1) ? m_Values[m_CurrentValueIndex + index + 1] : default(StylePropertyValue));
		StylePropertyValue val3 = ((valueCount > 2) ? m_Values[m_CurrentValueIndex + index + 2] : default(StylePropertyValue));
		StylePropertyValue val4 = ((valueCount > 3) ? m_Values[m_CurrentValueIndex + index + 3] : default(StylePropertyValue));
		return ReadRotate(valueCount, val, val2, val3, val4);
	}

	public Scale ReadScale(int index)
	{
		StylePropertyValue val = m_Values[m_CurrentValueIndex + index];
		StylePropertyValue val2 = ((valueCount > 1) ? m_Values[m_CurrentValueIndex + index + 1] : default(StylePropertyValue));
		StylePropertyValue val3 = ((valueCount > 2) ? m_Values[m_CurrentValueIndex + index + 2] : default(StylePropertyValue));
		return ReadScale(valueCount, val, val2, val3);
	}

	public float ReadFloat(int index)
	{
		StylePropertyValue stylePropertyValue = m_Values[m_CurrentValueIndex + index];
		return stylePropertyValue.sheet.ReadFloat(stylePropertyValue.handle);
	}

	public int ReadInt(int index)
	{
		StylePropertyValue stylePropertyValue = m_Values[m_CurrentValueIndex + index];
		return (int)stylePropertyValue.sheet.ReadFloat(stylePropertyValue.handle);
	}

	public Color ReadColor(int index)
	{
		StylePropertyValue stylePropertyValue = m_Values[m_CurrentValueIndex + index];
		Color color = Color.clear;
		if (stylePropertyValue.handle.valueType == StyleValueType.Enum)
		{
			string text = stylePropertyValue.sheet.ReadAsString(stylePropertyValue.handle);
			StyleSheetColor.TryGetColor(text.ToLowerInvariant(), out color);
		}
		else
		{
			color = stylePropertyValue.sheet.ReadColor(stylePropertyValue.handle);
		}
		return color;
	}

	public int ReadEnum(StyleEnumType enumType, int index)
	{
		string text = null;
		StylePropertyValue stylePropertyValue = m_Values[m_CurrentValueIndex + index];
		StyleValueHandle handle = stylePropertyValue.handle;
		if (handle.valueType == StyleValueType.Keyword)
		{
			StyleValueKeyword svk = stylePropertyValue.sheet.ReadKeyword(handle);
			text = svk.ToUssString();
		}
		else
		{
			text = stylePropertyValue.sheet.ReadEnum(handle);
		}
		StylePropertyUtil.TryGetEnumIntValue(enumType, text, out var intValue);
		return intValue;
	}

	public FontDefinition ReadFontDefinition(int index)
	{
		FontAsset fontAsset = null;
		Font font = null;
		StylePropertyValue stylePropertyValue = m_Values[m_CurrentValueIndex + index];
		switch (stylePropertyValue.handle.valueType)
		{
		case StyleValueType.ResourcePath:
		{
			string text = stylePropertyValue.sheet.ReadResourcePath(stylePropertyValue.handle);
			if (!string.IsNullOrEmpty(text))
			{
				font = Panel.LoadResource(text, typeof(Font), dpiScaling) as Font;
				if (font == null)
				{
					fontAsset = Panel.LoadResource(text, typeof(FontAsset), dpiScaling) as FontAsset;
				}
			}
			if (fontAsset == null && font == null)
			{
				Debug.LogWarning($"Font not found for path: {text}");
			}
			break;
		}
		case StyleValueType.AssetReference:
			font = stylePropertyValue.sheet.ReadAssetReference(stylePropertyValue.handle) as Font;
			if (font == null)
			{
				fontAsset = stylePropertyValue.sheet.ReadAssetReference(stylePropertyValue.handle) as FontAsset;
			}
			break;
		case StyleValueType.Keyword:
			if (stylePropertyValue.handle.valueIndex != 6)
			{
				Debug.LogWarning("Invalid keyword for font " + (StyleValueKeyword)stylePropertyValue.handle.valueIndex/*cast due to .constrained prefix*/);
			}
			break;
		default:
			Debug.LogWarning("Invalid value for font " + stylePropertyValue.handle.valueType);
			break;
		}
		if (font != null)
		{
			return FontDefinition.FromFont(font);
		}
		if (fontAsset != null)
		{
			return FontDefinition.FromSDFFont(fontAsset);
		}
		return default(FontDefinition);
	}

	public Font ReadFont(int index)
	{
		Font font = null;
		StylePropertyValue stylePropertyValue = m_Values[m_CurrentValueIndex + index];
		switch (stylePropertyValue.handle.valueType)
		{
		case StyleValueType.ResourcePath:
		{
			string text = stylePropertyValue.sheet.ReadResourcePath(stylePropertyValue.handle);
			if (!string.IsNullOrEmpty(text))
			{
				font = Panel.LoadResource(text, typeof(Font), dpiScaling) as Font;
			}
			if (font == null)
			{
				Debug.LogWarning($"Font not found for path: {text}");
			}
			break;
		}
		case StyleValueType.AssetReference:
			font = stylePropertyValue.sheet.ReadAssetReference(stylePropertyValue.handle) as Font;
			break;
		case StyleValueType.Keyword:
			if (stylePropertyValue.handle.valueIndex != 6)
			{
				Debug.LogWarning("Invalid keyword for font " + (StyleValueKeyword)stylePropertyValue.handle.valueIndex/*cast due to .constrained prefix*/);
			}
			break;
		default:
			Debug.LogWarning("Invalid value for font " + stylePropertyValue.handle.valueType);
			break;
		}
		return font;
	}

	public Background ReadBackground(int index)
	{
		ImageSource source = default(ImageSource);
		StylePropertyValue propertyValue = m_Values[m_CurrentValueIndex + index];
		if (propertyValue.handle.valueType == StyleValueType.Keyword)
		{
			if (propertyValue.handle.valueIndex != 6)
			{
				Debug.LogWarning("Invalid keyword for image source " + (StyleValueKeyword)propertyValue.handle.valueIndex/*cast due to .constrained prefix*/);
			}
		}
		else if (TryGetImageSourceFromValue(propertyValue, dpiScaling, out source))
		{
		}
		if (source.texture != null)
		{
			return Background.FromTexture2D(source.texture);
		}
		if (source.sprite != null)
		{
			return Background.FromSprite(source.sprite);
		}
		if (source.vectorImage != null)
		{
			return Background.FromVectorImage(source.vectorImage);
		}
		if (source.renderTexture != null)
		{
			return Background.FromRenderTexture(source.renderTexture);
		}
		return default(Background);
	}

	public Cursor ReadCursor(int index)
	{
		float x = 0f;
		float y = 0f;
		int defaultCursorId = 0;
		Texture2D texture = null;
		StyleValueType valueType = GetValueType(index);
		if (valueType == StyleValueType.ResourcePath || valueType == StyleValueType.AssetReference || valueType == StyleValueType.ScalableImage || valueType == StyleValueType.MissingAssetReference)
		{
			if (valueCount < 1)
			{
				Debug.LogWarning($"USS 'cursor' has invalid value at {index}.");
			}
			else
			{
				ImageSource source = default(ImageSource);
				StylePropertyValue value = GetValue(index);
				if (TryGetImageSourceFromValue(value, dpiScaling, out source))
				{
					texture = source.texture;
					if (valueCount >= 3)
					{
						StylePropertyValue value2 = GetValue(index + 1);
						StylePropertyValue value3 = GetValue(index + 2);
						if (value2.handle.valueType != StyleValueType.Float || value3.handle.valueType != StyleValueType.Float)
						{
							Debug.LogWarning("USS 'cursor' property requires two integers for the hot spot value.");
						}
						else
						{
							x = value2.sheet.ReadFloat(value2.handle);
							y = value3.sheet.ReadFloat(value3.handle);
						}
					}
				}
			}
		}
		else if (getCursorIdFunc != null)
		{
			StylePropertyValue value4 = GetValue(index);
			defaultCursorId = getCursorIdFunc(value4.sheet, value4.handle);
		}
		return new Cursor
		{
			texture = texture,
			hotspot = new Vector2(x, y),
			defaultCursorId = defaultCursorId
		};
	}

	public TextShadow ReadTextShadow(int index)
	{
		float x = 0f;
		float y = 0f;
		float blurRadius = 0f;
		Color color = Color.clear;
		if (valueCount >= 2)
		{
			int num = index;
			StyleValueType valueType = GetValueType(num);
			bool flag = false;
			if (valueType == StyleValueType.Color || valueType == StyleValueType.Enum)
			{
				color = ReadColor(num++);
				flag = true;
			}
			if (num + 1 < valueCount)
			{
				valueType = GetValueType(num);
				StyleValueType valueType2 = GetValueType(num + 1);
				if ((valueType == StyleValueType.Dimension || valueType == StyleValueType.Float) && (valueType2 == StyleValueType.Dimension || valueType2 == StyleValueType.Float))
				{
					StylePropertyValue value = GetValue(num++);
					StylePropertyValue value2 = GetValue(num++);
					x = value.sheet.ReadDimension(value.handle).value;
					y = value2.sheet.ReadDimension(value2.handle).value;
				}
			}
			if (num < valueCount)
			{
				valueType = GetValueType(num);
				if (valueType == StyleValueType.Dimension || valueType == StyleValueType.Float)
				{
					StylePropertyValue value3 = GetValue(num++);
					blurRadius = value3.sheet.ReadDimension(value3.handle).value;
				}
				else if ((valueType == StyleValueType.Color || valueType == StyleValueType.Enum) && !flag)
				{
					color = ReadColor(num);
				}
			}
			if (num < valueCount)
			{
				valueType = GetValueType(num);
				if ((valueType == StyleValueType.Color || valueType == StyleValueType.Enum) && !flag)
				{
					color = ReadColor(num);
				}
			}
		}
		return new TextShadow
		{
			offset = new Vector2(x, y),
			blurRadius = blurRadius,
			color = color
		};
	}

	public BackgroundPosition ReadBackgroundPositionX(int index)
	{
		return ReadBackgroundPosition(index, BackgroundPositionKeyword.Left);
	}

	public BackgroundPosition ReadBackgroundPositionY(int index)
	{
		return ReadBackgroundPosition(index, BackgroundPositionKeyword.Top);
	}

	private BackgroundPosition ReadBackgroundPosition(int index, BackgroundPositionKeyword keyword)
	{
		StylePropertyValue val = m_Values[m_CurrentValueIndex + index];
		StylePropertyValue val2 = ((valueCount > 1) ? m_Values[m_CurrentValueIndex + index + 1] : default(StylePropertyValue));
		return ReadBackgroundPosition(valueCount, val, val2, keyword);
	}

	public BackgroundRepeat ReadBackgroundRepeat(int index)
	{
		StylePropertyValue val = m_Values[m_CurrentValueIndex + index];
		StylePropertyValue val2 = ((valueCount > 1) ? m_Values[m_CurrentValueIndex + index + 1] : default(StylePropertyValue));
		return ReadBackgroundRepeat(valueCount, val, val2);
	}

	public BackgroundSize ReadBackgroundSize(int index)
	{
		StylePropertyValue val = m_Values[m_CurrentValueIndex + index];
		StylePropertyValue val2 = ((valueCount > 1) ? m_Values[m_CurrentValueIndex + index + 1] : default(StylePropertyValue));
		return ReadBackgroundSize(valueCount, val, val2);
	}

	public void ReadListEasingFunction(List<EasingFunction> list, int index)
	{
		list.Clear();
		do
		{
			StylePropertyValue stylePropertyValue = m_Values[m_CurrentValueIndex + index];
			StyleValueHandle handle = stylePropertyValue.handle;
			if (handle.valueType == StyleValueType.Enum)
			{
				string value = stylePropertyValue.sheet.ReadEnum(handle);
				StylePropertyUtil.TryGetEnumIntValue(StyleEnumType.EasingMode, value, out var intValue);
				list.Add(new EasingFunction((EasingMode)intValue));
				index++;
			}
			if (index < valueCount && m_Values[m_CurrentValueIndex + index].handle.valueType == StyleValueType.CommaSeparator)
			{
				index++;
			}
		}
		while (index < valueCount);
	}

	public void ReadListTimeValue(List<TimeValue> list, int index)
	{
		list.Clear();
		do
		{
			StylePropertyValue stylePropertyValue = m_Values[m_CurrentValueIndex + index];
			TimeValue item = stylePropertyValue.sheet.ReadDimension(stylePropertyValue.handle).ToTime();
			list.Add(item);
			index++;
			if (index < valueCount && m_Values[m_CurrentValueIndex + index].handle.valueType == StyleValueType.CommaSeparator)
			{
				index++;
			}
		}
		while (index < valueCount);
	}

	public void ReadListStylePropertyName(List<StylePropertyName> list, int index)
	{
		list.Clear();
		do
		{
			StylePropertyValue stylePropertyValue = m_Values[m_CurrentValueIndex + index];
			string name = stylePropertyValue.sheet.ReadAsString(stylePropertyValue.handle);
			list.Add(new StylePropertyName(name));
			index++;
			if (index < valueCount && m_Values[m_CurrentValueIndex + index].handle.valueType == StyleValueType.CommaSeparator)
			{
				index++;
			}
		}
		while (index < valueCount);
	}

	public void ReadListString(List<string> list, int index)
	{
		list.Clear();
		do
		{
			StylePropertyValue stylePropertyValue = m_Values[m_CurrentValueIndex + index];
			string item = stylePropertyValue.sheet.ReadAsString(stylePropertyValue.handle);
			list.Add(item);
			index++;
			if (index < valueCount && m_Values[m_CurrentValueIndex + index].handle.valueType == StyleValueType.CommaSeparator)
			{
				index++;
			}
		}
		while (index < valueCount);
	}

	private void LoadProperties()
	{
		m_CurrentPropertyIndex = 0;
		m_CurrentValueIndex = 0;
		m_Values.Clear();
		m_ValueCount.Clear();
		StyleProperty[] properties = m_Properties;
		foreach (StyleProperty styleProperty in properties)
		{
			int num = 0;
			bool flag = true;
			if (styleProperty.requireVariableResolve)
			{
				m_Resolver.Init(styleProperty, m_Sheet, styleProperty.values);
				for (int j = 0; j < styleProperty.values.Length && flag; j++)
				{
					StyleValueHandle handle = styleProperty.values[j];
					if (handle.IsVarFunction())
					{
						flag = m_Resolver.ResolveVarFunction(ref j);
					}
					else
					{
						m_Resolver.AddValue(handle);
					}
				}
				if (flag && m_Resolver.ValidateResolvedValues())
				{
					m_Values.AddRange(m_Resolver.resolvedValues);
					num += m_Resolver.resolvedValues.Count;
				}
				else
				{
					StyleValueHandle handle2 = new StyleValueHandle
					{
						valueType = StyleValueType.Keyword,
						valueIndex = 3
					};
					m_Values.Add(new StylePropertyValue
					{
						sheet = m_Sheet,
						handle = handle2
					});
					num++;
				}
			}
			else
			{
				num = styleProperty.values.Length;
				for (int k = 0; k < num; k++)
				{
					m_Values.Add(new StylePropertyValue
					{
						sheet = m_Sheet,
						handle = styleProperty.values[k]
					});
				}
			}
			m_ValueCount.Add(num);
		}
		SetCurrentProperty();
	}

	private void SetCurrentProperty()
	{
		if (m_CurrentPropertyIndex < m_PropertyIds.Length)
		{
			property = m_Properties[m_CurrentPropertyIndex];
			propertyId = m_PropertyIds[m_CurrentPropertyIndex];
			valueCount = m_ValueCount[m_CurrentPropertyIndex];
		}
		else
		{
			property = null;
			propertyId = StylePropertyId.Unknown;
			valueCount = 0;
		}
	}

	public static TransformOrigin ReadTransformOrigin(int valCount, StylePropertyValue val1, StylePropertyValue val2, StylePropertyValue zVvalue)
	{
		Length x = Length.Percent(50f);
		Length y = Length.Percent(50f);
		float z = 0f;
		switch (valCount)
		{
		case 1:
		{
			bool isVertical;
			bool isHorizontal;
			Length length = ReadTransformOriginEnum(val1, out isVertical, out isHorizontal);
			if (isHorizontal)
			{
				x = length;
			}
			else
			{
				y = length;
			}
			break;
		}
		case 2:
		{
			bool isVertical2;
			bool isHorizontal2;
			Length length2 = ReadTransformOriginEnum(val1, out isVertical2, out isHorizontal2);
			bool isVertical3;
			bool isHorizontal3;
			Length length3 = ReadTransformOriginEnum(val2, out isVertical3, out isHorizontal3);
			if (!isHorizontal2 || !isVertical3)
			{
				if (isHorizontal3 && isVertical2)
				{
					x = length3;
					y = length2;
				}
			}
			else
			{
				x = length2;
				y = length3;
			}
			break;
		}
		case 3:
			if (zVvalue.handle.valueType == StyleValueType.Dimension || zVvalue.handle.valueType == StyleValueType.Float)
			{
				z = zVvalue.sheet.ReadDimension(zVvalue.handle).value;
			}
			goto case 2;
		}
		return new TransformOrigin(x, y, z);
	}

	private static Length ReadTransformOriginEnum(StylePropertyValue value, out bool isVertical, out bool isHorizontal)
	{
		if (value.handle.valueType == StyleValueType.Enum)
		{
			switch ((TransformOriginOffset)ReadEnum(StyleEnumType.TransformOriginOffset, value))
			{
			case TransformOriginOffset.Left:
				isVertical = false;
				isHorizontal = true;
				return Length.Percent(0f);
			case TransformOriginOffset.Top:
				isVertical = true;
				isHorizontal = false;
				return Length.Percent(0f);
			case TransformOriginOffset.Center:
				isVertical = true;
				isHorizontal = true;
				return Length.Percent(50f);
			case TransformOriginOffset.Right:
				isVertical = false;
				isHorizontal = true;
				return Length.Percent(100f);
			case TransformOriginOffset.Bottom:
				isVertical = true;
				isHorizontal = false;
				return Length.Percent(100f);
			}
		}
		else if (value.handle.valueType == StyleValueType.Dimension || value.handle.valueType == StyleValueType.Float)
		{
			isVertical = true;
			isHorizontal = true;
			return value.sheet.ReadDimension(value.handle).ToLength();
		}
		isVertical = false;
		isHorizontal = false;
		return Length.Percent(50f);
	}

	public static Translate ReadTranslate(int valCount, StylePropertyValue val1, StylePropertyValue val2, StylePropertyValue val3)
	{
		if (val1.handle.valueType == StyleValueType.Keyword && val1.handle.valueIndex == 6)
		{
			return Translate.None();
		}
		Length x = 0f;
		Length y = 0f;
		float z = 0f;
		switch (valCount)
		{
		case 1:
			if (val1.handle.valueType == StyleValueType.Dimension || val1.handle.valueType == StyleValueType.Float)
			{
				x = val1.sheet.ReadDimension(val1.handle).ToLength();
				y = val1.sheet.ReadDimension(val1.handle).ToLength();
			}
			break;
		case 2:
			if (val1.handle.valueType == StyleValueType.Dimension || val1.handle.valueType == StyleValueType.Float)
			{
				x = val1.sheet.ReadDimension(val1.handle).ToLength();
			}
			if (val2.handle.valueType == StyleValueType.Dimension || val2.handle.valueType == StyleValueType.Float)
			{
				y = val2.sheet.ReadDimension(val2.handle).ToLength();
			}
			break;
		case 3:
			if (val3.handle.valueType == StyleValueType.Dimension || val3.handle.valueType == StyleValueType.Float)
			{
				Dimension dimension = val3.sheet.ReadDimension(val3.handle);
				if (dimension.unit != Dimension.Unit.Pixel && dimension.unit != Dimension.Unit.Unitless)
				{
					z = dimension.value;
				}
			}
			goto case 2;
		}
		return new Translate(x, y, z);
	}

	public static Scale ReadScale(int valCount, StylePropertyValue val1, StylePropertyValue val2, StylePropertyValue val3)
	{
		if (val1.handle.valueType == StyleValueType.Keyword && val1.handle.valueIndex == 6)
		{
			return Scale.None();
		}
		Vector3 one = Vector3.one;
		switch (valCount)
		{
		case 1:
			if (val1.handle.valueType == StyleValueType.Dimension || val1.handle.valueType == StyleValueType.Float)
			{
				one.x = val1.sheet.ReadFloat(val1.handle);
				one.y = one.x;
			}
			break;
		case 2:
			if (val1.handle.valueType == StyleValueType.Dimension || val1.handle.valueType == StyleValueType.Float)
			{
				one.x = val1.sheet.ReadFloat(val1.handle);
			}
			if (val2.handle.valueType == StyleValueType.Dimension || val2.handle.valueType == StyleValueType.Float)
			{
				one.y = val2.sheet.ReadFloat(val2.handle);
			}
			break;
		case 3:
			if (val3.handle.valueType == StyleValueType.Dimension || val3.handle.valueType == StyleValueType.Float)
			{
				one.z = val3.sheet.ReadFloat(val3.handle);
			}
			goto case 2;
		}
		return new Scale(one);
	}

	public static Rotate ReadRotate(int valCount, StylePropertyValue val1, StylePropertyValue val2, StylePropertyValue val3, StylePropertyValue val4)
	{
		if (val1.handle.valueType == StyleValueType.Keyword && val1.handle.valueIndex == 6)
		{
			return Rotate.None();
		}
		Rotate result = Rotate.Initial();
		if (valCount == 1 && val1.handle.valueType == StyleValueType.Dimension)
		{
			result.angle = ReadAngle(val1);
		}
		return result;
	}

	private static bool TryReadEnum(StyleEnumType enumType, StylePropertyValue value, out int intValue)
	{
		string text = null;
		StyleValueHandle handle = value.handle;
		if (handle.valueType == StyleValueType.Keyword)
		{
			StyleValueKeyword svk = value.sheet.ReadKeyword(handle);
			text = svk.ToUssString();
		}
		else
		{
			text = value.sheet.ReadEnum(handle);
		}
		return StylePropertyUtil.TryGetEnumIntValue(enumType, text, out intValue);
	}

	private static int ReadEnum(StyleEnumType enumType, StylePropertyValue value)
	{
		string text = null;
		StyleValueHandle handle = value.handle;
		if (handle.valueType == StyleValueType.Keyword)
		{
			StyleValueKeyword svk = value.sheet.ReadKeyword(handle);
			text = svk.ToUssString();
		}
		else
		{
			text = value.sheet.ReadEnum(handle);
		}
		StylePropertyUtil.TryGetEnumIntValue(enumType, text, out var intValue);
		return intValue;
	}

	public static Angle ReadAngle(StylePropertyValue value)
	{
		if (value.handle.valueType == StyleValueType.Keyword)
		{
			StyleValueKeyword valueIndex = (StyleValueKeyword)value.handle.valueIndex;
			StyleValueKeyword styleValueKeyword = valueIndex;
			StyleValueKeyword styleValueKeyword2 = styleValueKeyword;
			if (styleValueKeyword2 == StyleValueKeyword.None)
			{
				return Angle.None();
			}
			return default(Angle);
		}
		return value.sheet.ReadDimension(value.handle).ToAngle();
	}

	public static BackgroundPosition ReadBackgroundPosition(int valCount, StylePropertyValue val1, StylePropertyValue val2, BackgroundPositionKeyword keyword)
	{
		switch (valCount)
		{
		case 1:
			if (val1.handle.valueType == StyleValueType.Enum)
			{
				return new BackgroundPosition((BackgroundPositionKeyword)ReadEnum(StyleEnumType.BackgroundPositionKeyword, val1));
			}
			if (val1.handle.valueType == StyleValueType.Dimension || val1.handle.valueType == StyleValueType.Float)
			{
				return new BackgroundPosition(keyword, val1.sheet.ReadDimension(val1.handle).ToLength());
			}
			break;
		case 2:
			if (val1.handle.valueType == StyleValueType.Enum && (val2.handle.valueType == StyleValueType.Dimension || val2.handle.valueType == StyleValueType.Float))
			{
				return new BackgroundPosition((BackgroundPositionKeyword)ReadEnum(StyleEnumType.BackgroundPositionKeyword, val1), val1.sheet.ReadDimension(val2.handle).ToLength());
			}
			break;
		}
		return default(BackgroundPosition);
	}

	public static BackgroundRepeat ReadBackgroundRepeat(int valCount, StylePropertyValue val1, StylePropertyValue val2)
	{
		BackgroundRepeat result = default(BackgroundRepeat);
		if (valCount == 1)
		{
			if (TryReadEnum(StyleEnumType.RepeatXY, val1, out var intValue))
			{
				switch (intValue)
				{
				case 0:
					result.x = Repeat.Repeat;
					result.y = Repeat.NoRepeat;
					break;
				case 1:
					result.x = Repeat.NoRepeat;
					result.y = Repeat.Repeat;
					break;
				}
			}
			else
			{
				result.x = (Repeat)ReadEnum(StyleEnumType.Repeat, val1);
				result.y = result.x;
			}
		}
		else
		{
			result.x = (Repeat)ReadEnum(StyleEnumType.Repeat, val1);
			result.y = (Repeat)ReadEnum(StyleEnumType.Repeat, val2);
		}
		return result;
	}

	public static BackgroundSize ReadBackgroundSize(int valCount, StylePropertyValue val1, StylePropertyValue val2)
	{
		BackgroundSize result = default(BackgroundSize);
		switch (valCount)
		{
		case 1:
			if (val1.handle.valueType == StyleValueType.Keyword)
			{
				if (val1.handle.valueIndex == 2)
				{
					result.x = Length.Auto();
					result.y = Length.Auto();
				}
			}
			else if (val1.handle.valueType == StyleValueType.Enum)
			{
				result.sizeType = (BackgroundSizeType)ReadEnum(StyleEnumType.BackgroundSizeType, val1);
			}
			else if (val1.handle.valueType == StyleValueType.Dimension)
			{
				result.x = val1.sheet.ReadDimension(val1.handle).ToLength();
				result.y = Length.Auto();
			}
			break;
		case 2:
			if (val1.handle.valueType == StyleValueType.Keyword)
			{
				if (val1.handle.valueIndex == 2)
				{
					result.x = Length.Auto();
				}
			}
			else if (val1.handle.valueType == StyleValueType.Dimension)
			{
				result.x = val1.sheet.ReadDimension(val1.handle).ToLength();
			}
			if (val2.handle.valueType == StyleValueType.Keyword)
			{
				if (val2.handle.valueIndex == 2)
				{
					result.y = Length.Auto();
				}
			}
			else if (val2.handle.valueType == StyleValueType.Dimension)
			{
				result.y = val2.sheet.ReadDimension(val2.handle).ToLength();
			}
			break;
		}
		return result;
	}

	internal static bool TryGetImageSourceFromValue(StylePropertyValue propertyValue, float dpiScaling, out ImageSource source)
	{
		source = default(ImageSource);
		switch (propertyValue.handle.valueType)
		{
		case StyleValueType.ResourcePath:
		{
			string text = propertyValue.sheet.ReadResourcePath(propertyValue.handle);
			if (!string.IsNullOrEmpty(text))
			{
				source.sprite = Panel.LoadResource(text, typeof(Sprite), dpiScaling) as Sprite;
				if (source.IsNull())
				{
					source.texture = Panel.LoadResource(text, typeof(Texture2D), dpiScaling) as Texture2D;
				}
				if (source.IsNull())
				{
					source.vectorImage = Panel.LoadResource(text, typeof(VectorImage), dpiScaling) as VectorImage;
				}
				if (source.IsNull())
				{
					source.renderTexture = Panel.LoadResource(text, typeof(RenderTexture), dpiScaling) as RenderTexture;
				}
			}
			if (source.IsNull())
			{
				Debug.LogWarning($"Image not found for path: {text}");
				return false;
			}
			break;
		}
		case StyleValueType.AssetReference:
		{
			Object obj = propertyValue.sheet.ReadAssetReference(propertyValue.handle);
			source.texture = obj as Texture2D;
			source.sprite = obj as Sprite;
			source.vectorImage = obj as VectorImage;
			source.renderTexture = obj as RenderTexture;
			if (source.IsNull())
			{
				Debug.LogWarning("Invalid image specified");
				return false;
			}
			break;
		}
		case StyleValueType.MissingAssetReference:
			return false;
		case StyleValueType.ScalableImage:
		{
			ScalableImage scalableImage = propertyValue.sheet.ReadScalableImage(propertyValue.handle);
			if (scalableImage.normalImage == null && scalableImage.highResolutionImage == null)
			{
				Debug.LogWarning("Invalid scalable image specified");
				return false;
			}
			source.texture = scalableImage.normalImage;
			if (!Mathf.Approximately(dpiScaling % 1f, 0f))
			{
				source.texture.filterMode = FilterMode.Bilinear;
			}
			break;
		}
		default:
			Debug.LogWarning("Invalid value for image texture " + propertyValue.handle.valueType);
			return false;
		}
		return true;
	}
}
