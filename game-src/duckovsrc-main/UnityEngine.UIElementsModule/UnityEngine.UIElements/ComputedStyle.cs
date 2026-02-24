#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using UnityEngine.UIElements.StyleSheets;
using UnityEngine.Yoga;

namespace UnityEngine.UIElements;

internal struct ComputedStyle
{
	public StyleDataRef<InheritedData> inheritedData;

	public StyleDataRef<LayoutData> layoutData;

	public StyleDataRef<RareData> rareData;

	public StyleDataRef<TransformData> transformData;

	public StyleDataRef<TransitionData> transitionData;

	public StyleDataRef<VisualData> visualData;

	public YogaNode yogaNode;

	public Dictionary<string, StylePropertyValue> customProperties;

	public long matchingRulesHash;

	public float dpiScaling;

	public ComputedTransitionProperty[] computedTransitions;

	public int customPropertiesCount => customProperties?.Count ?? 0;

	public bool hasTransition
	{
		get
		{
			ComputedTransitionProperty[] array = computedTransitions;
			return array != null && array.Length != 0;
		}
	}

	public Align alignContent => layoutData.Read().alignContent;

	public Align alignItems => layoutData.Read().alignItems;

	public Align alignSelf => layoutData.Read().alignSelf;

	public Color backgroundColor => visualData.Read().backgroundColor;

	public Background backgroundImage => visualData.Read().backgroundImage;

	public BackgroundPosition backgroundPositionX => visualData.Read().backgroundPositionX;

	public BackgroundPosition backgroundPositionY => visualData.Read().backgroundPositionY;

	public BackgroundRepeat backgroundRepeat => visualData.Read().backgroundRepeat;

	public BackgroundSize backgroundSize => visualData.Read().backgroundSize;

	public Color borderBottomColor => visualData.Read().borderBottomColor;

	public Length borderBottomLeftRadius => visualData.Read().borderBottomLeftRadius;

	public Length borderBottomRightRadius => visualData.Read().borderBottomRightRadius;

	public float borderBottomWidth => layoutData.Read().borderBottomWidth;

	public Color borderLeftColor => visualData.Read().borderLeftColor;

	public float borderLeftWidth => layoutData.Read().borderLeftWidth;

	public Color borderRightColor => visualData.Read().borderRightColor;

	public float borderRightWidth => layoutData.Read().borderRightWidth;

	public Color borderTopColor => visualData.Read().borderTopColor;

	public Length borderTopLeftRadius => visualData.Read().borderTopLeftRadius;

	public Length borderTopRightRadius => visualData.Read().borderTopRightRadius;

	public float borderTopWidth => layoutData.Read().borderTopWidth;

	public Length bottom => layoutData.Read().bottom;

	public Color color => inheritedData.Read().color;

	public Cursor cursor => rareData.Read().cursor;

	public DisplayStyle display => layoutData.Read().display;

	public Length flexBasis => layoutData.Read().flexBasis;

	public FlexDirection flexDirection => layoutData.Read().flexDirection;

	public float flexGrow => layoutData.Read().flexGrow;

	public float flexShrink => layoutData.Read().flexShrink;

	public Wrap flexWrap => layoutData.Read().flexWrap;

	public Length fontSize => inheritedData.Read().fontSize;

	public Length height => layoutData.Read().height;

	public Justify justifyContent => layoutData.Read().justifyContent;

	public Length left => layoutData.Read().left;

	public Length letterSpacing => inheritedData.Read().letterSpacing;

	public Length marginBottom => layoutData.Read().marginBottom;

	public Length marginLeft => layoutData.Read().marginLeft;

	public Length marginRight => layoutData.Read().marginRight;

	public Length marginTop => layoutData.Read().marginTop;

	public Length maxHeight => layoutData.Read().maxHeight;

	public Length maxWidth => layoutData.Read().maxWidth;

	public Length minHeight => layoutData.Read().minHeight;

	public Length minWidth => layoutData.Read().minWidth;

	public float opacity => visualData.Read().opacity;

	public OverflowInternal overflow => visualData.Read().overflow;

	public Length paddingBottom => layoutData.Read().paddingBottom;

	public Length paddingLeft => layoutData.Read().paddingLeft;

	public Length paddingRight => layoutData.Read().paddingRight;

	public Length paddingTop => layoutData.Read().paddingTop;

	public Position position => layoutData.Read().position;

	public Length right => layoutData.Read().right;

	public Rotate rotate => transformData.Read().rotate;

	public Scale scale => transformData.Read().scale;

	public TextOverflow textOverflow => rareData.Read().textOverflow;

	public TextShadow textShadow => inheritedData.Read().textShadow;

	public Length top => layoutData.Read().top;

	public TransformOrigin transformOrigin => transformData.Read().transformOrigin;

	public List<TimeValue> transitionDelay => transitionData.Read().transitionDelay;

	public List<TimeValue> transitionDuration => transitionData.Read().transitionDuration;

	public List<StylePropertyName> transitionProperty => transitionData.Read().transitionProperty;

	public List<EasingFunction> transitionTimingFunction => transitionData.Read().transitionTimingFunction;

	public Translate translate => transformData.Read().translate;

	public Color unityBackgroundImageTintColor => rareData.Read().unityBackgroundImageTintColor;

	public Font unityFont => inheritedData.Read().unityFont;

	public FontDefinition unityFontDefinition => inheritedData.Read().unityFontDefinition;

	public FontStyle unityFontStyleAndWeight => inheritedData.Read().unityFontStyleAndWeight;

	public OverflowClipBox unityOverflowClipBox => rareData.Read().unityOverflowClipBox;

	public Length unityParagraphSpacing => inheritedData.Read().unityParagraphSpacing;

	public int unitySliceBottom => rareData.Read().unitySliceBottom;

	public int unitySliceLeft => rareData.Read().unitySliceLeft;

	public int unitySliceRight => rareData.Read().unitySliceRight;

	public float unitySliceScale => rareData.Read().unitySliceScale;

	public int unitySliceTop => rareData.Read().unitySliceTop;

	public TextAnchor unityTextAlign => inheritedData.Read().unityTextAlign;

	public Color unityTextOutlineColor => inheritedData.Read().unityTextOutlineColor;

	public float unityTextOutlineWidth => inheritedData.Read().unityTextOutlineWidth;

	public TextOverflowPosition unityTextOverflowPosition => rareData.Read().unityTextOverflowPosition;

	public Visibility visibility => inheritedData.Read().visibility;

	public WhiteSpace whiteSpace => inheritedData.Read().whiteSpace;

	public Length width => layoutData.Read().width;

	public Length wordSpacing => inheritedData.Read().wordSpacing;

	public static ComputedStyle Create()
	{
		return InitialStyle.Acquire();
	}

	public void FinalizeApply(ref ComputedStyle parentStyle)
	{
		if (yogaNode == null)
		{
			yogaNode = new YogaNode();
		}
		if (fontSize.unit == LengthUnit.Percent)
		{
			float value = parentStyle.fontSize.value;
			float value2 = value * fontSize.value / 100f;
			inheritedData.Write().fontSize = new Length(value2);
		}
		SyncWithLayout(yogaNode);
	}

	public void SyncWithLayout(YogaNode targetNode)
	{
		targetNode.Flex = float.NaN;
		targetNode.FlexGrow = flexGrow;
		targetNode.FlexShrink = flexShrink;
		targetNode.FlexBasis = flexBasis.ToYogaValue();
		targetNode.Left = left.ToYogaValue();
		targetNode.Top = top.ToYogaValue();
		targetNode.Right = right.ToYogaValue();
		targetNode.Bottom = bottom.ToYogaValue();
		targetNode.MarginLeft = marginLeft.ToYogaValue();
		targetNode.MarginTop = marginTop.ToYogaValue();
		targetNode.MarginRight = marginRight.ToYogaValue();
		targetNode.MarginBottom = marginBottom.ToYogaValue();
		targetNode.PaddingLeft = paddingLeft.ToYogaValue();
		targetNode.PaddingTop = paddingTop.ToYogaValue();
		targetNode.PaddingRight = paddingRight.ToYogaValue();
		targetNode.PaddingBottom = paddingBottom.ToYogaValue();
		targetNode.BorderLeftWidth = borderLeftWidth;
		targetNode.BorderTopWidth = borderTopWidth;
		targetNode.BorderRightWidth = borderRightWidth;
		targetNode.BorderBottomWidth = borderBottomWidth;
		targetNode.Width = width.ToYogaValue();
		targetNode.Height = height.ToYogaValue();
		targetNode.PositionType = (YogaPositionType)position;
		targetNode.Overflow = (YogaOverflow)overflow;
		targetNode.AlignSelf = (YogaAlign)alignSelf;
		targetNode.MaxWidth = maxWidth.ToYogaValue();
		targetNode.MaxHeight = maxHeight.ToYogaValue();
		targetNode.MinWidth = minWidth.ToYogaValue();
		targetNode.MinHeight = minHeight.ToYogaValue();
		targetNode.FlexDirection = (YogaFlexDirection)flexDirection;
		targetNode.AlignContent = (YogaAlign)alignContent;
		targetNode.AlignItems = (YogaAlign)alignItems;
		targetNode.JustifyContent = (YogaJustify)justifyContent;
		targetNode.Wrap = (YogaWrap)flexWrap;
		targetNode.Display = (YogaDisplay)display;
	}

	private bool ApplyGlobalKeyword(StylePropertyReader reader, ref ComputedStyle parentStyle)
	{
		StyleValueHandle handle = reader.GetValue(0).handle;
		if (handle.valueType == StyleValueType.Keyword)
		{
			switch ((StyleValueKeyword)handle.valueIndex)
			{
			case StyleValueKeyword.Initial:
				ApplyInitialValue(reader);
				return true;
			case StyleValueKeyword.Unset:
				ApplyUnsetValue(reader, ref parentStyle);
				return true;
			}
		}
		return false;
	}

	private bool ApplyGlobalKeyword(StylePropertyId id, StyleKeyword keyword, ref ComputedStyle parentStyle)
	{
		if (keyword == StyleKeyword.Initial)
		{
			ApplyInitialValue(id);
			return true;
		}
		return false;
	}

	private void RemoveCustomStyleProperty(StylePropertyReader reader)
	{
		string name = reader.property.name;
		if (customProperties != null && customProperties.ContainsKey(name))
		{
			customProperties.Remove(name);
		}
	}

	private void ApplyCustomStyleProperty(StylePropertyReader reader)
	{
		dpiScaling = reader.dpiScaling;
		if (customProperties == null)
		{
			customProperties = new Dictionary<string, StylePropertyValue>();
		}
		StyleProperty property = reader.property;
		StylePropertyValue value = reader.GetValue(0);
		customProperties[property.name] = value;
	}

	private void ApplyAllPropertyInitial()
	{
		CopyFrom(ref InitialStyle.Get());
	}

	private void ResetComputedTransitions()
	{
		computedTransitions = null;
	}

	public static bool StartAnimationInlineTextShadow(VisualElement element, ref ComputedStyle computedStyle, StyleTextShadow textShadow, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		TextShadow to = ((textShadow.keyword == StyleKeyword.Initial) ? InitialStyle.textShadow : textShadow.value);
		return element.styleAnimation.Start(StylePropertyId.TextShadow, computedStyle.inheritedData.Read().textShadow, to, durationMs, delayMs, easingCurve);
	}

	public static bool StartAnimationInlineRotate(VisualElement element, ref ComputedStyle computedStyle, StyleRotate rotate, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		Rotate to = ((rotate.keyword == StyleKeyword.Initial) ? InitialStyle.rotate : rotate.value);
		bool flag = element.styleAnimation.Start(StylePropertyId.Rotate, computedStyle.transformData.Read().rotate, to, durationMs, delayMs, easingCurve);
		if (flag && (element.usageHints & UsageHints.DynamicTransform) == 0)
		{
			element.usageHints |= UsageHints.DynamicTransform;
		}
		return flag;
	}

	public static bool StartAnimationInlineTranslate(VisualElement element, ref ComputedStyle computedStyle, StyleTranslate translate, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		Translate to = ((translate.keyword == StyleKeyword.Initial) ? InitialStyle.translate : translate.value);
		bool flag = element.styleAnimation.Start(StylePropertyId.Translate, computedStyle.transformData.Read().translate, to, durationMs, delayMs, easingCurve);
		if (flag && (element.usageHints & UsageHints.DynamicTransform) == 0)
		{
			element.usageHints |= UsageHints.DynamicTransform;
		}
		return flag;
	}

	public static bool StartAnimationInlineScale(VisualElement element, ref ComputedStyle computedStyle, StyleScale scale, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		Scale to = ((scale.keyword == StyleKeyword.Initial) ? InitialStyle.scale : scale.value);
		bool flag = element.styleAnimation.Start(StylePropertyId.Scale, computedStyle.transformData.Read().scale, to, durationMs, delayMs, easingCurve);
		if (flag && (element.usageHints & UsageHints.DynamicTransform) == 0)
		{
			element.usageHints |= UsageHints.DynamicTransform;
		}
		return flag;
	}

	public static bool StartAnimationInlineTransformOrigin(VisualElement element, ref ComputedStyle computedStyle, StyleTransformOrigin transformOrigin, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		TransformOrigin to = ((transformOrigin.keyword == StyleKeyword.Initial) ? InitialStyle.transformOrigin : transformOrigin.value);
		bool flag = element.styleAnimation.Start(StylePropertyId.TransformOrigin, computedStyle.transformData.Read().transformOrigin, to, durationMs, delayMs, easingCurve);
		if (flag && (element.usageHints & UsageHints.DynamicTransform) == 0)
		{
			element.usageHints |= UsageHints.DynamicTransform;
		}
		return flag;
	}

	public static bool StartAnimationInlineBackgroundSize(VisualElement element, ref ComputedStyle computedStyle, StyleBackgroundSize backgroundSize, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		BackgroundSize to = ((backgroundSize.keyword == StyleKeyword.Initial) ? InitialStyle.backgroundSize : backgroundSize.value);
		return element.styleAnimation.Start(StylePropertyId.BackgroundSize, computedStyle.visualData.Read().backgroundSize, to, durationMs, delayMs, easingCurve);
	}

	public static ComputedStyle Create(ref ComputedStyle parentStyle)
	{
		ref ComputedStyle reference = ref InitialStyle.Get();
		ComputedStyle result = new ComputedStyle
		{
			dpiScaling = 1f
		};
		result.inheritedData = parentStyle.inheritedData.Acquire();
		result.layoutData = reference.layoutData.Acquire();
		result.rareData = reference.rareData.Acquire();
		result.transformData = reference.transformData.Acquire();
		result.transitionData = reference.transitionData.Acquire();
		result.visualData = reference.visualData.Acquire();
		return result;
	}

	public static ComputedStyle CreateInitial()
	{
		ComputedStyle result = new ComputedStyle
		{
			dpiScaling = 1f
		};
		result.inheritedData = StyleDataRef<InheritedData>.Create();
		result.layoutData = StyleDataRef<LayoutData>.Create();
		result.rareData = StyleDataRef<RareData>.Create();
		result.transformData = StyleDataRef<TransformData>.Create();
		result.transitionData = StyleDataRef<TransitionData>.Create();
		result.visualData = StyleDataRef<VisualData>.Create();
		return result;
	}

	public ComputedStyle Acquire()
	{
		inheritedData.Acquire();
		layoutData.Acquire();
		rareData.Acquire();
		transformData.Acquire();
		transitionData.Acquire();
		visualData.Acquire();
		return this;
	}

	public void Release()
	{
		inheritedData.Release();
		layoutData.Release();
		rareData.Release();
		transformData.Release();
		transitionData.Release();
		visualData.Release();
	}

	public void CopyFrom(ref ComputedStyle other)
	{
		inheritedData.CopyFrom(other.inheritedData);
		layoutData.CopyFrom(other.layoutData);
		rareData.CopyFrom(other.rareData);
		transformData.CopyFrom(other.transformData);
		transitionData.CopyFrom(other.transitionData);
		visualData.CopyFrom(other.visualData);
		yogaNode = other.yogaNode;
		customProperties = other.customProperties;
		matchingRulesHash = other.matchingRulesHash;
		dpiScaling = other.dpiScaling;
		computedTransitions = other.computedTransitions;
	}

	public void ApplyProperties(StylePropertyReader reader, ref ComputedStyle parentStyle)
	{
		StylePropertyId stylePropertyId = reader.propertyId;
		while (reader.property != null)
		{
			if (!ApplyGlobalKeyword(reader, ref parentStyle))
			{
				switch (stylePropertyId)
				{
				case StylePropertyId.AlignContent:
					layoutData.Write().alignContent = (Align)reader.ReadEnum(StyleEnumType.Align, 0);
					break;
				case StylePropertyId.AlignItems:
					layoutData.Write().alignItems = (Align)reader.ReadEnum(StyleEnumType.Align, 0);
					break;
				case StylePropertyId.AlignSelf:
					layoutData.Write().alignSelf = (Align)reader.ReadEnum(StyleEnumType.Align, 0);
					break;
				case StylePropertyId.BackgroundColor:
					visualData.Write().backgroundColor = reader.ReadColor(0);
					break;
				case StylePropertyId.BackgroundImage:
					visualData.Write().backgroundImage = reader.ReadBackground(0);
					break;
				case StylePropertyId.BackgroundPosition:
					ShorthandApplicator.ApplyBackgroundPosition(reader, ref this);
					break;
				case StylePropertyId.BackgroundPositionX:
					visualData.Write().backgroundPositionX = reader.ReadBackgroundPositionX(0);
					break;
				case StylePropertyId.BackgroundPositionY:
					visualData.Write().backgroundPositionY = reader.ReadBackgroundPositionY(0);
					break;
				case StylePropertyId.BackgroundRepeat:
					visualData.Write().backgroundRepeat = reader.ReadBackgroundRepeat(0);
					break;
				case StylePropertyId.BackgroundSize:
					visualData.Write().backgroundSize = reader.ReadBackgroundSize(0);
					break;
				case StylePropertyId.BorderBottomColor:
					visualData.Write().borderBottomColor = reader.ReadColor(0);
					break;
				case StylePropertyId.BorderBottomLeftRadius:
					visualData.Write().borderBottomLeftRadius = reader.ReadLength(0);
					break;
				case StylePropertyId.BorderBottomRightRadius:
					visualData.Write().borderBottomRightRadius = reader.ReadLength(0);
					break;
				case StylePropertyId.BorderBottomWidth:
					layoutData.Write().borderBottomWidth = reader.ReadFloat(0);
					break;
				case StylePropertyId.BorderColor:
					ShorthandApplicator.ApplyBorderColor(reader, ref this);
					break;
				case StylePropertyId.BorderLeftColor:
					visualData.Write().borderLeftColor = reader.ReadColor(0);
					break;
				case StylePropertyId.BorderLeftWidth:
					layoutData.Write().borderLeftWidth = reader.ReadFloat(0);
					break;
				case StylePropertyId.BorderRadius:
					ShorthandApplicator.ApplyBorderRadius(reader, ref this);
					break;
				case StylePropertyId.BorderRightColor:
					visualData.Write().borderRightColor = reader.ReadColor(0);
					break;
				case StylePropertyId.BorderRightWidth:
					layoutData.Write().borderRightWidth = reader.ReadFloat(0);
					break;
				case StylePropertyId.BorderTopColor:
					visualData.Write().borderTopColor = reader.ReadColor(0);
					break;
				case StylePropertyId.BorderTopLeftRadius:
					visualData.Write().borderTopLeftRadius = reader.ReadLength(0);
					break;
				case StylePropertyId.BorderTopRightRadius:
					visualData.Write().borderTopRightRadius = reader.ReadLength(0);
					break;
				case StylePropertyId.BorderTopWidth:
					layoutData.Write().borderTopWidth = reader.ReadFloat(0);
					break;
				case StylePropertyId.BorderWidth:
					ShorthandApplicator.ApplyBorderWidth(reader, ref this);
					break;
				case StylePropertyId.Bottom:
					layoutData.Write().bottom = reader.ReadLength(0);
					break;
				case StylePropertyId.Color:
					inheritedData.Write().color = reader.ReadColor(0);
					break;
				case StylePropertyId.Cursor:
					rareData.Write().cursor = reader.ReadCursor(0);
					break;
				case StylePropertyId.Display:
					layoutData.Write().display = (DisplayStyle)reader.ReadEnum(StyleEnumType.DisplayStyle, 0);
					break;
				case StylePropertyId.Flex:
					ShorthandApplicator.ApplyFlex(reader, ref this);
					break;
				case StylePropertyId.FlexBasis:
					layoutData.Write().flexBasis = reader.ReadLength(0);
					break;
				case StylePropertyId.FlexDirection:
					layoutData.Write().flexDirection = (FlexDirection)reader.ReadEnum(StyleEnumType.FlexDirection, 0);
					break;
				case StylePropertyId.FlexGrow:
					layoutData.Write().flexGrow = reader.ReadFloat(0);
					break;
				case StylePropertyId.FlexShrink:
					layoutData.Write().flexShrink = reader.ReadFloat(0);
					break;
				case StylePropertyId.FlexWrap:
					layoutData.Write().flexWrap = (Wrap)reader.ReadEnum(StyleEnumType.Wrap, 0);
					break;
				case StylePropertyId.FontSize:
					inheritedData.Write().fontSize = reader.ReadLength(0);
					break;
				case StylePropertyId.Height:
					layoutData.Write().height = reader.ReadLength(0);
					break;
				case StylePropertyId.JustifyContent:
					layoutData.Write().justifyContent = (Justify)reader.ReadEnum(StyleEnumType.Justify, 0);
					break;
				case StylePropertyId.Left:
					layoutData.Write().left = reader.ReadLength(0);
					break;
				case StylePropertyId.LetterSpacing:
					inheritedData.Write().letterSpacing = reader.ReadLength(0);
					break;
				case StylePropertyId.Margin:
					ShorthandApplicator.ApplyMargin(reader, ref this);
					break;
				case StylePropertyId.MarginBottom:
					layoutData.Write().marginBottom = reader.ReadLength(0);
					break;
				case StylePropertyId.MarginLeft:
					layoutData.Write().marginLeft = reader.ReadLength(0);
					break;
				case StylePropertyId.MarginRight:
					layoutData.Write().marginRight = reader.ReadLength(0);
					break;
				case StylePropertyId.MarginTop:
					layoutData.Write().marginTop = reader.ReadLength(0);
					break;
				case StylePropertyId.MaxHeight:
					layoutData.Write().maxHeight = reader.ReadLength(0);
					break;
				case StylePropertyId.MaxWidth:
					layoutData.Write().maxWidth = reader.ReadLength(0);
					break;
				case StylePropertyId.MinHeight:
					layoutData.Write().minHeight = reader.ReadLength(0);
					break;
				case StylePropertyId.MinWidth:
					layoutData.Write().minWidth = reader.ReadLength(0);
					break;
				case StylePropertyId.Opacity:
					visualData.Write().opacity = reader.ReadFloat(0);
					break;
				case StylePropertyId.Overflow:
					visualData.Write().overflow = (OverflowInternal)reader.ReadEnum(StyleEnumType.OverflowInternal, 0);
					break;
				case StylePropertyId.Padding:
					ShorthandApplicator.ApplyPadding(reader, ref this);
					break;
				case StylePropertyId.PaddingBottom:
					layoutData.Write().paddingBottom = reader.ReadLength(0);
					break;
				case StylePropertyId.PaddingLeft:
					layoutData.Write().paddingLeft = reader.ReadLength(0);
					break;
				case StylePropertyId.PaddingRight:
					layoutData.Write().paddingRight = reader.ReadLength(0);
					break;
				case StylePropertyId.PaddingTop:
					layoutData.Write().paddingTop = reader.ReadLength(0);
					break;
				case StylePropertyId.Position:
					layoutData.Write().position = (Position)reader.ReadEnum(StyleEnumType.Position, 0);
					break;
				case StylePropertyId.Right:
					layoutData.Write().right = reader.ReadLength(0);
					break;
				case StylePropertyId.Rotate:
					transformData.Write().rotate = reader.ReadRotate(0);
					break;
				case StylePropertyId.Scale:
					transformData.Write().scale = reader.ReadScale(0);
					break;
				case StylePropertyId.TextOverflow:
					rareData.Write().textOverflow = (TextOverflow)reader.ReadEnum(StyleEnumType.TextOverflow, 0);
					break;
				case StylePropertyId.TextShadow:
					inheritedData.Write().textShadow = reader.ReadTextShadow(0);
					break;
				case StylePropertyId.Top:
					layoutData.Write().top = reader.ReadLength(0);
					break;
				case StylePropertyId.TransformOrigin:
					transformData.Write().transformOrigin = reader.ReadTransformOrigin(0);
					break;
				case StylePropertyId.Transition:
					ShorthandApplicator.ApplyTransition(reader, ref this);
					break;
				case StylePropertyId.TransitionDelay:
					reader.ReadListTimeValue(transitionData.Write().transitionDelay, 0);
					ResetComputedTransitions();
					break;
				case StylePropertyId.TransitionDuration:
					reader.ReadListTimeValue(transitionData.Write().transitionDuration, 0);
					ResetComputedTransitions();
					break;
				case StylePropertyId.TransitionProperty:
					reader.ReadListStylePropertyName(transitionData.Write().transitionProperty, 0);
					ResetComputedTransitions();
					break;
				case StylePropertyId.TransitionTimingFunction:
					reader.ReadListEasingFunction(transitionData.Write().transitionTimingFunction, 0);
					ResetComputedTransitions();
					break;
				case StylePropertyId.Translate:
					transformData.Write().translate = reader.ReadTranslate(0);
					break;
				case StylePropertyId.UnityBackgroundImageTintColor:
					rareData.Write().unityBackgroundImageTintColor = reader.ReadColor(0);
					break;
				case StylePropertyId.UnityBackgroundScaleMode:
					ShorthandApplicator.ApplyUnityBackgroundScaleMode(reader, ref this);
					break;
				case StylePropertyId.UnityFont:
					inheritedData.Write().unityFont = reader.ReadFont(0);
					break;
				case StylePropertyId.UnityFontDefinition:
					inheritedData.Write().unityFontDefinition = reader.ReadFontDefinition(0);
					break;
				case StylePropertyId.UnityFontStyleAndWeight:
					inheritedData.Write().unityFontStyleAndWeight = (FontStyle)reader.ReadEnum(StyleEnumType.FontStyle, 0);
					break;
				case StylePropertyId.UnityOverflowClipBox:
					rareData.Write().unityOverflowClipBox = (OverflowClipBox)reader.ReadEnum(StyleEnumType.OverflowClipBox, 0);
					break;
				case StylePropertyId.UnityParagraphSpacing:
					inheritedData.Write().unityParagraphSpacing = reader.ReadLength(0);
					break;
				case StylePropertyId.UnitySliceBottom:
					rareData.Write().unitySliceBottom = reader.ReadInt(0);
					break;
				case StylePropertyId.UnitySliceLeft:
					rareData.Write().unitySliceLeft = reader.ReadInt(0);
					break;
				case StylePropertyId.UnitySliceRight:
					rareData.Write().unitySliceRight = reader.ReadInt(0);
					break;
				case StylePropertyId.UnitySliceScale:
					rareData.Write().unitySliceScale = reader.ReadFloat(0);
					break;
				case StylePropertyId.UnitySliceTop:
					rareData.Write().unitySliceTop = reader.ReadInt(0);
					break;
				case StylePropertyId.UnityTextAlign:
					inheritedData.Write().unityTextAlign = (TextAnchor)reader.ReadEnum(StyleEnumType.TextAnchor, 0);
					break;
				case StylePropertyId.UnityTextOutline:
					ShorthandApplicator.ApplyUnityTextOutline(reader, ref this);
					break;
				case StylePropertyId.UnityTextOutlineColor:
					inheritedData.Write().unityTextOutlineColor = reader.ReadColor(0);
					break;
				case StylePropertyId.UnityTextOutlineWidth:
					inheritedData.Write().unityTextOutlineWidth = reader.ReadFloat(0);
					break;
				case StylePropertyId.UnityTextOverflowPosition:
					rareData.Write().unityTextOverflowPosition = (TextOverflowPosition)reader.ReadEnum(StyleEnumType.TextOverflowPosition, 0);
					break;
				case StylePropertyId.Visibility:
					inheritedData.Write().visibility = (Visibility)reader.ReadEnum(StyleEnumType.Visibility, 0);
					break;
				case StylePropertyId.WhiteSpace:
					inheritedData.Write().whiteSpace = (WhiteSpace)reader.ReadEnum(StyleEnumType.WhiteSpace, 0);
					break;
				case StylePropertyId.Width:
					layoutData.Write().width = reader.ReadLength(0);
					break;
				case StylePropertyId.WordSpacing:
					inheritedData.Write().wordSpacing = reader.ReadLength(0);
					break;
				case StylePropertyId.Custom:
					ApplyCustomStyleProperty(reader);
					break;
				default:
					Debug.LogAssertion($"Unknown property id {stylePropertyId}");
					break;
				case StylePropertyId.Unknown:
				case StylePropertyId.All:
					break;
				}
			}
			stylePropertyId = reader.MoveNextProperty();
		}
	}

	public void ApplyStyleValue(StyleValue sv, ref ComputedStyle parentStyle)
	{
		if (ApplyGlobalKeyword(sv.id, sv.keyword, ref parentStyle))
		{
			return;
		}
		switch (sv.id)
		{
		case StylePropertyId.AlignContent:
			layoutData.Write().alignContent = (Align)sv.number;
			if (sv.keyword == StyleKeyword.Auto)
			{
				layoutData.Write().alignContent = Align.Auto;
			}
			break;
		case StylePropertyId.AlignItems:
			layoutData.Write().alignItems = (Align)sv.number;
			if (sv.keyword == StyleKeyword.Auto)
			{
				layoutData.Write().alignItems = Align.Auto;
			}
			break;
		case StylePropertyId.AlignSelf:
			layoutData.Write().alignSelf = (Align)sv.number;
			if (sv.keyword == StyleKeyword.Auto)
			{
				layoutData.Write().alignSelf = Align.Auto;
			}
			break;
		case StylePropertyId.BackgroundColor:
			visualData.Write().backgroundColor = sv.color;
			break;
		case StylePropertyId.BackgroundImage:
			visualData.Write().backgroundImage = (sv.resource.IsAllocated ? Background.FromObject(sv.resource.Target) : default(Background));
			break;
		case StylePropertyId.BackgroundPositionX:
			visualData.Write().backgroundPositionX = sv.position;
			break;
		case StylePropertyId.BackgroundPositionY:
			visualData.Write().backgroundPositionY = sv.position;
			break;
		case StylePropertyId.BackgroundRepeat:
			visualData.Write().backgroundRepeat = sv.repeat;
			break;
		case StylePropertyId.BorderBottomColor:
			visualData.Write().borderBottomColor = sv.color;
			break;
		case StylePropertyId.BorderBottomLeftRadius:
			visualData.Write().borderBottomLeftRadius = sv.length;
			break;
		case StylePropertyId.BorderBottomRightRadius:
			visualData.Write().borderBottomRightRadius = sv.length;
			break;
		case StylePropertyId.BorderBottomWidth:
			layoutData.Write().borderBottomWidth = sv.number;
			break;
		case StylePropertyId.BorderLeftColor:
			visualData.Write().borderLeftColor = sv.color;
			break;
		case StylePropertyId.BorderLeftWidth:
			layoutData.Write().borderLeftWidth = sv.number;
			break;
		case StylePropertyId.BorderRightColor:
			visualData.Write().borderRightColor = sv.color;
			break;
		case StylePropertyId.BorderRightWidth:
			layoutData.Write().borderRightWidth = sv.number;
			break;
		case StylePropertyId.BorderTopColor:
			visualData.Write().borderTopColor = sv.color;
			break;
		case StylePropertyId.BorderTopLeftRadius:
			visualData.Write().borderTopLeftRadius = sv.length;
			break;
		case StylePropertyId.BorderTopRightRadius:
			visualData.Write().borderTopRightRadius = sv.length;
			break;
		case StylePropertyId.BorderTopWidth:
			layoutData.Write().borderTopWidth = sv.number;
			break;
		case StylePropertyId.Bottom:
			layoutData.Write().bottom = sv.length;
			break;
		case StylePropertyId.Color:
			inheritedData.Write().color = sv.color;
			break;
		case StylePropertyId.Display:
			layoutData.Write().display = (DisplayStyle)sv.number;
			if (sv.keyword == StyleKeyword.None)
			{
				layoutData.Write().display = DisplayStyle.None;
			}
			break;
		case StylePropertyId.FlexBasis:
			layoutData.Write().flexBasis = sv.length;
			break;
		case StylePropertyId.FlexDirection:
			layoutData.Write().flexDirection = (FlexDirection)sv.number;
			break;
		case StylePropertyId.FlexGrow:
			layoutData.Write().flexGrow = sv.number;
			break;
		case StylePropertyId.FlexShrink:
			layoutData.Write().flexShrink = sv.number;
			break;
		case StylePropertyId.FlexWrap:
			layoutData.Write().flexWrap = (Wrap)sv.number;
			break;
		case StylePropertyId.FontSize:
			inheritedData.Write().fontSize = sv.length;
			break;
		case StylePropertyId.Height:
			layoutData.Write().height = sv.length;
			break;
		case StylePropertyId.JustifyContent:
			layoutData.Write().justifyContent = (Justify)sv.number;
			break;
		case StylePropertyId.Left:
			layoutData.Write().left = sv.length;
			break;
		case StylePropertyId.LetterSpacing:
			inheritedData.Write().letterSpacing = sv.length;
			break;
		case StylePropertyId.MarginBottom:
			layoutData.Write().marginBottom = sv.length;
			break;
		case StylePropertyId.MarginLeft:
			layoutData.Write().marginLeft = sv.length;
			break;
		case StylePropertyId.MarginRight:
			layoutData.Write().marginRight = sv.length;
			break;
		case StylePropertyId.MarginTop:
			layoutData.Write().marginTop = sv.length;
			break;
		case StylePropertyId.MaxHeight:
			layoutData.Write().maxHeight = sv.length;
			break;
		case StylePropertyId.MaxWidth:
			layoutData.Write().maxWidth = sv.length;
			break;
		case StylePropertyId.MinHeight:
			layoutData.Write().minHeight = sv.length;
			break;
		case StylePropertyId.MinWidth:
			layoutData.Write().minWidth = sv.length;
			break;
		case StylePropertyId.Opacity:
			visualData.Write().opacity = sv.number;
			break;
		case StylePropertyId.Overflow:
			visualData.Write().overflow = (OverflowInternal)sv.number;
			break;
		case StylePropertyId.PaddingBottom:
			layoutData.Write().paddingBottom = sv.length;
			break;
		case StylePropertyId.PaddingLeft:
			layoutData.Write().paddingLeft = sv.length;
			break;
		case StylePropertyId.PaddingRight:
			layoutData.Write().paddingRight = sv.length;
			break;
		case StylePropertyId.PaddingTop:
			layoutData.Write().paddingTop = sv.length;
			break;
		case StylePropertyId.Position:
			layoutData.Write().position = (Position)sv.number;
			break;
		case StylePropertyId.Right:
			layoutData.Write().right = sv.length;
			break;
		case StylePropertyId.TextOverflow:
			rareData.Write().textOverflow = (TextOverflow)sv.number;
			break;
		case StylePropertyId.Top:
			layoutData.Write().top = sv.length;
			break;
		case StylePropertyId.UnityBackgroundImageTintColor:
			rareData.Write().unityBackgroundImageTintColor = sv.color;
			break;
		case StylePropertyId.UnityFont:
			inheritedData.Write().unityFont = (sv.resource.IsAllocated ? (sv.resource.Target as Font) : null);
			break;
		case StylePropertyId.UnityFontDefinition:
			inheritedData.Write().unityFontDefinition = (sv.resource.IsAllocated ? FontDefinition.FromObject(sv.resource.Target) : default(FontDefinition));
			break;
		case StylePropertyId.UnityFontStyleAndWeight:
			inheritedData.Write().unityFontStyleAndWeight = (FontStyle)sv.number;
			break;
		case StylePropertyId.UnityOverflowClipBox:
			rareData.Write().unityOverflowClipBox = (OverflowClipBox)sv.number;
			break;
		case StylePropertyId.UnityParagraphSpacing:
			inheritedData.Write().unityParagraphSpacing = sv.length;
			break;
		case StylePropertyId.UnitySliceBottom:
			rareData.Write().unitySliceBottom = (int)sv.number;
			break;
		case StylePropertyId.UnitySliceLeft:
			rareData.Write().unitySliceLeft = (int)sv.number;
			break;
		case StylePropertyId.UnitySliceRight:
			rareData.Write().unitySliceRight = (int)sv.number;
			break;
		case StylePropertyId.UnitySliceScale:
			rareData.Write().unitySliceScale = sv.number;
			break;
		case StylePropertyId.UnitySliceTop:
			rareData.Write().unitySliceTop = (int)sv.number;
			break;
		case StylePropertyId.UnityTextAlign:
			inheritedData.Write().unityTextAlign = (TextAnchor)sv.number;
			break;
		case StylePropertyId.UnityTextOutlineColor:
			inheritedData.Write().unityTextOutlineColor = sv.color;
			break;
		case StylePropertyId.UnityTextOutlineWidth:
			inheritedData.Write().unityTextOutlineWidth = sv.number;
			break;
		case StylePropertyId.UnityTextOverflowPosition:
			rareData.Write().unityTextOverflowPosition = (TextOverflowPosition)sv.number;
			break;
		case StylePropertyId.Visibility:
			inheritedData.Write().visibility = (Visibility)sv.number;
			break;
		case StylePropertyId.WhiteSpace:
			inheritedData.Write().whiteSpace = (WhiteSpace)sv.number;
			break;
		case StylePropertyId.Width:
			layoutData.Write().width = sv.length;
			break;
		case StylePropertyId.WordSpacing:
			inheritedData.Write().wordSpacing = sv.length;
			break;
		default:
			Debug.LogAssertion($"Unexpected property id {sv.id}");
			break;
		}
	}

	public void ApplyStyleValueManaged(StyleValueManaged sv, ref ComputedStyle parentStyle)
	{
		if (ApplyGlobalKeyword(sv.id, sv.keyword, ref parentStyle))
		{
			return;
		}
		switch (sv.id)
		{
		case StylePropertyId.TransitionDelay:
			if (sv.value == null)
			{
				transitionData.Write().transitionDelay.CopyFrom(InitialStyle.transitionDelay);
			}
			else
			{
				transitionData.Write().transitionDelay = sv.value as List<TimeValue>;
			}
			ResetComputedTransitions();
			break;
		case StylePropertyId.TransitionDuration:
			if (sv.value == null)
			{
				transitionData.Write().transitionDuration.CopyFrom(InitialStyle.transitionDuration);
			}
			else
			{
				transitionData.Write().transitionDuration = sv.value as List<TimeValue>;
			}
			ResetComputedTransitions();
			break;
		case StylePropertyId.TransitionProperty:
			if (sv.value == null)
			{
				transitionData.Write().transitionProperty.CopyFrom(InitialStyle.transitionProperty);
			}
			else
			{
				transitionData.Write().transitionProperty = sv.value as List<StylePropertyName>;
			}
			ResetComputedTransitions();
			break;
		case StylePropertyId.TransitionTimingFunction:
			if (sv.value == null)
			{
				transitionData.Write().transitionTimingFunction.CopyFrom(InitialStyle.transitionTimingFunction);
			}
			else
			{
				transitionData.Write().transitionTimingFunction = sv.value as List<EasingFunction>;
			}
			ResetComputedTransitions();
			break;
		default:
			Debug.LogAssertion($"Unexpected property id {sv.id}");
			break;
		}
	}

	public void ApplyStyleCursor(Cursor cursor)
	{
		rareData.Write().cursor = cursor;
	}

	public void ApplyStyleTextShadow(TextShadow st)
	{
		inheritedData.Write().textShadow = st;
	}

	public void ApplyFromComputedStyle(StylePropertyId id, ref ComputedStyle other)
	{
		switch (id)
		{
		case StylePropertyId.AlignContent:
			layoutData.Write().alignContent = other.layoutData.Read().alignContent;
			break;
		case StylePropertyId.AlignItems:
			layoutData.Write().alignItems = other.layoutData.Read().alignItems;
			break;
		case StylePropertyId.AlignSelf:
			layoutData.Write().alignSelf = other.layoutData.Read().alignSelf;
			break;
		case StylePropertyId.BackgroundColor:
			visualData.Write().backgroundColor = other.visualData.Read().backgroundColor;
			break;
		case StylePropertyId.BackgroundImage:
			visualData.Write().backgroundImage = other.visualData.Read().backgroundImage;
			break;
		case StylePropertyId.BackgroundPositionX:
			visualData.Write().backgroundPositionX = other.visualData.Read().backgroundPositionX;
			break;
		case StylePropertyId.BackgroundPositionY:
			visualData.Write().backgroundPositionY = other.visualData.Read().backgroundPositionY;
			break;
		case StylePropertyId.BackgroundRepeat:
			visualData.Write().backgroundRepeat = other.visualData.Read().backgroundRepeat;
			break;
		case StylePropertyId.BackgroundSize:
			visualData.Write().backgroundSize = other.visualData.Read().backgroundSize;
			break;
		case StylePropertyId.BorderBottomColor:
			visualData.Write().borderBottomColor = other.visualData.Read().borderBottomColor;
			break;
		case StylePropertyId.BorderBottomLeftRadius:
			visualData.Write().borderBottomLeftRadius = other.visualData.Read().borderBottomLeftRadius;
			break;
		case StylePropertyId.BorderBottomRightRadius:
			visualData.Write().borderBottomRightRadius = other.visualData.Read().borderBottomRightRadius;
			break;
		case StylePropertyId.BorderBottomWidth:
			layoutData.Write().borderBottomWidth = other.layoutData.Read().borderBottomWidth;
			break;
		case StylePropertyId.BorderLeftColor:
			visualData.Write().borderLeftColor = other.visualData.Read().borderLeftColor;
			break;
		case StylePropertyId.BorderLeftWidth:
			layoutData.Write().borderLeftWidth = other.layoutData.Read().borderLeftWidth;
			break;
		case StylePropertyId.BorderRightColor:
			visualData.Write().borderRightColor = other.visualData.Read().borderRightColor;
			break;
		case StylePropertyId.BorderRightWidth:
			layoutData.Write().borderRightWidth = other.layoutData.Read().borderRightWidth;
			break;
		case StylePropertyId.BorderTopColor:
			visualData.Write().borderTopColor = other.visualData.Read().borderTopColor;
			break;
		case StylePropertyId.BorderTopLeftRadius:
			visualData.Write().borderTopLeftRadius = other.visualData.Read().borderTopLeftRadius;
			break;
		case StylePropertyId.BorderTopRightRadius:
			visualData.Write().borderTopRightRadius = other.visualData.Read().borderTopRightRadius;
			break;
		case StylePropertyId.BorderTopWidth:
			layoutData.Write().borderTopWidth = other.layoutData.Read().borderTopWidth;
			break;
		case StylePropertyId.Bottom:
			layoutData.Write().bottom = other.layoutData.Read().bottom;
			break;
		case StylePropertyId.Color:
			inheritedData.Write().color = other.inheritedData.Read().color;
			break;
		case StylePropertyId.Cursor:
			rareData.Write().cursor = other.rareData.Read().cursor;
			break;
		case StylePropertyId.Display:
			layoutData.Write().display = other.layoutData.Read().display;
			break;
		case StylePropertyId.FlexBasis:
			layoutData.Write().flexBasis = other.layoutData.Read().flexBasis;
			break;
		case StylePropertyId.FlexDirection:
			layoutData.Write().flexDirection = other.layoutData.Read().flexDirection;
			break;
		case StylePropertyId.FlexGrow:
			layoutData.Write().flexGrow = other.layoutData.Read().flexGrow;
			break;
		case StylePropertyId.FlexShrink:
			layoutData.Write().flexShrink = other.layoutData.Read().flexShrink;
			break;
		case StylePropertyId.FlexWrap:
			layoutData.Write().flexWrap = other.layoutData.Read().flexWrap;
			break;
		case StylePropertyId.FontSize:
			inheritedData.Write().fontSize = other.inheritedData.Read().fontSize;
			break;
		case StylePropertyId.Height:
			layoutData.Write().height = other.layoutData.Read().height;
			break;
		case StylePropertyId.JustifyContent:
			layoutData.Write().justifyContent = other.layoutData.Read().justifyContent;
			break;
		case StylePropertyId.Left:
			layoutData.Write().left = other.layoutData.Read().left;
			break;
		case StylePropertyId.LetterSpacing:
			inheritedData.Write().letterSpacing = other.inheritedData.Read().letterSpacing;
			break;
		case StylePropertyId.MarginBottom:
			layoutData.Write().marginBottom = other.layoutData.Read().marginBottom;
			break;
		case StylePropertyId.MarginLeft:
			layoutData.Write().marginLeft = other.layoutData.Read().marginLeft;
			break;
		case StylePropertyId.MarginRight:
			layoutData.Write().marginRight = other.layoutData.Read().marginRight;
			break;
		case StylePropertyId.MarginTop:
			layoutData.Write().marginTop = other.layoutData.Read().marginTop;
			break;
		case StylePropertyId.MaxHeight:
			layoutData.Write().maxHeight = other.layoutData.Read().maxHeight;
			break;
		case StylePropertyId.MaxWidth:
			layoutData.Write().maxWidth = other.layoutData.Read().maxWidth;
			break;
		case StylePropertyId.MinHeight:
			layoutData.Write().minHeight = other.layoutData.Read().minHeight;
			break;
		case StylePropertyId.MinWidth:
			layoutData.Write().minWidth = other.layoutData.Read().minWidth;
			break;
		case StylePropertyId.Opacity:
			visualData.Write().opacity = other.visualData.Read().opacity;
			break;
		case StylePropertyId.Overflow:
			visualData.Write().overflow = other.visualData.Read().overflow;
			break;
		case StylePropertyId.PaddingBottom:
			layoutData.Write().paddingBottom = other.layoutData.Read().paddingBottom;
			break;
		case StylePropertyId.PaddingLeft:
			layoutData.Write().paddingLeft = other.layoutData.Read().paddingLeft;
			break;
		case StylePropertyId.PaddingRight:
			layoutData.Write().paddingRight = other.layoutData.Read().paddingRight;
			break;
		case StylePropertyId.PaddingTop:
			layoutData.Write().paddingTop = other.layoutData.Read().paddingTop;
			break;
		case StylePropertyId.Position:
			layoutData.Write().position = other.layoutData.Read().position;
			break;
		case StylePropertyId.Right:
			layoutData.Write().right = other.layoutData.Read().right;
			break;
		case StylePropertyId.Rotate:
			transformData.Write().rotate = other.transformData.Read().rotate;
			break;
		case StylePropertyId.Scale:
			transformData.Write().scale = other.transformData.Read().scale;
			break;
		case StylePropertyId.TextOverflow:
			rareData.Write().textOverflow = other.rareData.Read().textOverflow;
			break;
		case StylePropertyId.TextShadow:
			inheritedData.Write().textShadow = other.inheritedData.Read().textShadow;
			break;
		case StylePropertyId.Top:
			layoutData.Write().top = other.layoutData.Read().top;
			break;
		case StylePropertyId.TransformOrigin:
			transformData.Write().transformOrigin = other.transformData.Read().transformOrigin;
			break;
		case StylePropertyId.TransitionDelay:
			transitionData.Write().transitionDelay.CopyFrom(other.transitionData.Read().transitionDelay);
			ResetComputedTransitions();
			break;
		case StylePropertyId.TransitionDuration:
			transitionData.Write().transitionDuration.CopyFrom(other.transitionData.Read().transitionDuration);
			ResetComputedTransitions();
			break;
		case StylePropertyId.TransitionProperty:
			transitionData.Write().transitionProperty.CopyFrom(other.transitionData.Read().transitionProperty);
			ResetComputedTransitions();
			break;
		case StylePropertyId.TransitionTimingFunction:
			transitionData.Write().transitionTimingFunction.CopyFrom(other.transitionData.Read().transitionTimingFunction);
			ResetComputedTransitions();
			break;
		case StylePropertyId.Translate:
			transformData.Write().translate = other.transformData.Read().translate;
			break;
		case StylePropertyId.UnityBackgroundImageTintColor:
			rareData.Write().unityBackgroundImageTintColor = other.rareData.Read().unityBackgroundImageTintColor;
			break;
		case StylePropertyId.UnityFont:
			inheritedData.Write().unityFont = other.inheritedData.Read().unityFont;
			break;
		case StylePropertyId.UnityFontDefinition:
			inheritedData.Write().unityFontDefinition = other.inheritedData.Read().unityFontDefinition;
			break;
		case StylePropertyId.UnityFontStyleAndWeight:
			inheritedData.Write().unityFontStyleAndWeight = other.inheritedData.Read().unityFontStyleAndWeight;
			break;
		case StylePropertyId.UnityOverflowClipBox:
			rareData.Write().unityOverflowClipBox = other.rareData.Read().unityOverflowClipBox;
			break;
		case StylePropertyId.UnityParagraphSpacing:
			inheritedData.Write().unityParagraphSpacing = other.inheritedData.Read().unityParagraphSpacing;
			break;
		case StylePropertyId.UnitySliceBottom:
			rareData.Write().unitySliceBottom = other.rareData.Read().unitySliceBottom;
			break;
		case StylePropertyId.UnitySliceLeft:
			rareData.Write().unitySliceLeft = other.rareData.Read().unitySliceLeft;
			break;
		case StylePropertyId.UnitySliceRight:
			rareData.Write().unitySliceRight = other.rareData.Read().unitySliceRight;
			break;
		case StylePropertyId.UnitySliceScale:
			rareData.Write().unitySliceScale = other.rareData.Read().unitySliceScale;
			break;
		case StylePropertyId.UnitySliceTop:
			rareData.Write().unitySliceTop = other.rareData.Read().unitySliceTop;
			break;
		case StylePropertyId.UnityTextAlign:
			inheritedData.Write().unityTextAlign = other.inheritedData.Read().unityTextAlign;
			break;
		case StylePropertyId.UnityTextOutlineColor:
			inheritedData.Write().unityTextOutlineColor = other.inheritedData.Read().unityTextOutlineColor;
			break;
		case StylePropertyId.UnityTextOutlineWidth:
			inheritedData.Write().unityTextOutlineWidth = other.inheritedData.Read().unityTextOutlineWidth;
			break;
		case StylePropertyId.UnityTextOverflowPosition:
			rareData.Write().unityTextOverflowPosition = other.rareData.Read().unityTextOverflowPosition;
			break;
		case StylePropertyId.Visibility:
			inheritedData.Write().visibility = other.inheritedData.Read().visibility;
			break;
		case StylePropertyId.WhiteSpace:
			inheritedData.Write().whiteSpace = other.inheritedData.Read().whiteSpace;
			break;
		case StylePropertyId.Width:
			layoutData.Write().width = other.layoutData.Read().width;
			break;
		case StylePropertyId.WordSpacing:
			inheritedData.Write().wordSpacing = other.inheritedData.Read().wordSpacing;
			break;
		default:
			Debug.LogAssertion($"Unexpected property id {id}");
			break;
		}
	}

	public void ApplyPropertyAnimation(VisualElement ve, StylePropertyId id, Length newValue)
	{
		switch (id)
		{
		case StylePropertyId.BorderBottomLeftRadius:
			visualData.Write().borderBottomLeftRadius = newValue;
			ve.IncrementVersion(VersionChangeType.BorderRadius | VersionChangeType.Repaint);
			break;
		case StylePropertyId.BorderBottomRightRadius:
			visualData.Write().borderBottomRightRadius = newValue;
			ve.IncrementVersion(VersionChangeType.BorderRadius | VersionChangeType.Repaint);
			break;
		case StylePropertyId.BorderTopLeftRadius:
			visualData.Write().borderTopLeftRadius = newValue;
			ve.IncrementVersion(VersionChangeType.BorderRadius | VersionChangeType.Repaint);
			break;
		case StylePropertyId.BorderTopRightRadius:
			visualData.Write().borderTopRightRadius = newValue;
			ve.IncrementVersion(VersionChangeType.BorderRadius | VersionChangeType.Repaint);
			break;
		case StylePropertyId.Bottom:
			layoutData.Write().bottom = newValue;
			ve.yogaNode.Bottom = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.FlexBasis:
			layoutData.Write().flexBasis = newValue;
			ve.yogaNode.FlexBasis = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.FontSize:
			inheritedData.Write().fontSize = newValue;
			ve.IncrementVersion(VersionChangeType.Layout | VersionChangeType.StyleSheet | VersionChangeType.Repaint);
			break;
		case StylePropertyId.Height:
			layoutData.Write().height = newValue;
			ve.yogaNode.Height = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.Left:
			layoutData.Write().left = newValue;
			ve.yogaNode.Left = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.LetterSpacing:
			inheritedData.Write().letterSpacing = newValue;
			ve.IncrementVersion(VersionChangeType.Layout | VersionChangeType.StyleSheet | VersionChangeType.Repaint);
			break;
		case StylePropertyId.MarginBottom:
			layoutData.Write().marginBottom = newValue;
			ve.yogaNode.MarginBottom = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.MarginLeft:
			layoutData.Write().marginLeft = newValue;
			ve.yogaNode.MarginLeft = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.MarginRight:
			layoutData.Write().marginRight = newValue;
			ve.yogaNode.MarginRight = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.MarginTop:
			layoutData.Write().marginTop = newValue;
			ve.yogaNode.MarginTop = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.MaxHeight:
			layoutData.Write().maxHeight = newValue;
			ve.yogaNode.MaxHeight = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.MaxWidth:
			layoutData.Write().maxWidth = newValue;
			ve.yogaNode.MaxWidth = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.MinHeight:
			layoutData.Write().minHeight = newValue;
			ve.yogaNode.MinHeight = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.MinWidth:
			layoutData.Write().minWidth = newValue;
			ve.yogaNode.MinWidth = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.PaddingBottom:
			layoutData.Write().paddingBottom = newValue;
			ve.yogaNode.PaddingBottom = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.PaddingLeft:
			layoutData.Write().paddingLeft = newValue;
			ve.yogaNode.PaddingLeft = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.PaddingRight:
			layoutData.Write().paddingRight = newValue;
			ve.yogaNode.PaddingRight = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.PaddingTop:
			layoutData.Write().paddingTop = newValue;
			ve.yogaNode.PaddingTop = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.Right:
			layoutData.Write().right = newValue;
			ve.yogaNode.Right = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.Top:
			layoutData.Write().top = newValue;
			ve.yogaNode.Top = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.UnityParagraphSpacing:
			inheritedData.Write().unityParagraphSpacing = newValue;
			ve.IncrementVersion(VersionChangeType.Layout | VersionChangeType.StyleSheet | VersionChangeType.Repaint);
			break;
		case StylePropertyId.Width:
			layoutData.Write().width = newValue;
			ve.yogaNode.Width = newValue.ToYogaValue();
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.WordSpacing:
			inheritedData.Write().wordSpacing = newValue;
			ve.IncrementVersion(VersionChangeType.Layout | VersionChangeType.StyleSheet | VersionChangeType.Repaint);
			break;
		default:
			throw new ArgumentException("Invalid animation property id. Can't apply value of type 'Length' to property '" + id.ToString() + "'. Please make sure that this property is animatable.", "id");
		}
	}

	public void ApplyPropertyAnimation(VisualElement ve, StylePropertyId id, float newValue)
	{
		switch (id)
		{
		case StylePropertyId.BorderBottomWidth:
			layoutData.Write().borderBottomWidth = newValue;
			ve.yogaNode.BorderBottomWidth = newValue;
			ve.IncrementVersion(VersionChangeType.Layout | VersionChangeType.BorderWidth | VersionChangeType.Repaint);
			break;
		case StylePropertyId.BorderLeftWidth:
			layoutData.Write().borderLeftWidth = newValue;
			ve.yogaNode.BorderLeftWidth = newValue;
			ve.IncrementVersion(VersionChangeType.Layout | VersionChangeType.BorderWidth | VersionChangeType.Repaint);
			break;
		case StylePropertyId.BorderRightWidth:
			layoutData.Write().borderRightWidth = newValue;
			ve.yogaNode.BorderRightWidth = newValue;
			ve.IncrementVersion(VersionChangeType.Layout | VersionChangeType.BorderWidth | VersionChangeType.Repaint);
			break;
		case StylePropertyId.BorderTopWidth:
			layoutData.Write().borderTopWidth = newValue;
			ve.yogaNode.BorderTopWidth = newValue;
			ve.IncrementVersion(VersionChangeType.Layout | VersionChangeType.BorderWidth | VersionChangeType.Repaint);
			break;
		case StylePropertyId.FlexGrow:
			layoutData.Write().flexGrow = newValue;
			ve.yogaNode.FlexGrow = newValue;
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.FlexShrink:
			layoutData.Write().flexShrink = newValue;
			ve.yogaNode.FlexShrink = newValue;
			ve.IncrementVersion(VersionChangeType.Layout);
			break;
		case StylePropertyId.Opacity:
			visualData.Write().opacity = newValue;
			ve.IncrementVersion(VersionChangeType.Opacity);
			break;
		case StylePropertyId.UnitySliceScale:
			rareData.Write().unitySliceScale = newValue;
			ve.IncrementVersion(VersionChangeType.Layout | VersionChangeType.Repaint);
			break;
		case StylePropertyId.UnityTextOutlineWidth:
			inheritedData.Write().unityTextOutlineWidth = newValue;
			ve.IncrementVersion(VersionChangeType.Layout | VersionChangeType.StyleSheet | VersionChangeType.Repaint);
			break;
		default:
			throw new ArgumentException("Invalid animation property id. Can't apply value of type 'float' to property '" + id.ToString() + "'. Please make sure that this property is animatable.", "id");
		}
	}

	public void ApplyPropertyAnimation(VisualElement ve, StylePropertyId id, int newValue)
	{
		switch (id)
		{
		case StylePropertyId.AlignContent:
			if (layoutData.Read().alignContent != (Align)newValue)
			{
				layoutData.Write().alignContent = (Align)newValue;
				ve.yogaNode.AlignContent = (YogaAlign)newValue;
				ve.IncrementVersion(VersionChangeType.Layout);
			}
			break;
		case StylePropertyId.AlignItems:
			if (layoutData.Read().alignItems != (Align)newValue)
			{
				layoutData.Write().alignItems = (Align)newValue;
				ve.yogaNode.AlignItems = (YogaAlign)newValue;
				ve.IncrementVersion(VersionChangeType.Layout);
			}
			break;
		case StylePropertyId.AlignSelf:
			if (layoutData.Read().alignSelf != (Align)newValue)
			{
				layoutData.Write().alignSelf = (Align)newValue;
				ve.yogaNode.AlignSelf = (YogaAlign)newValue;
				ve.IncrementVersion(VersionChangeType.Layout);
			}
			break;
		case StylePropertyId.Display:
			if (layoutData.Read().display != (DisplayStyle)newValue)
			{
				layoutData.Write().display = (DisplayStyle)newValue;
				ve.yogaNode.Display = (YogaDisplay)newValue;
				ve.IncrementVersion(VersionChangeType.Layout | VersionChangeType.Repaint);
			}
			break;
		case StylePropertyId.FlexDirection:
			if (layoutData.Read().flexDirection != (FlexDirection)newValue)
			{
				layoutData.Write().flexDirection = (FlexDirection)newValue;
				ve.yogaNode.FlexDirection = (YogaFlexDirection)newValue;
				ve.IncrementVersion(VersionChangeType.Layout);
			}
			break;
		case StylePropertyId.FlexWrap:
			if (layoutData.Read().flexWrap != (Wrap)newValue)
			{
				layoutData.Write().flexWrap = (Wrap)newValue;
				ve.yogaNode.Wrap = (YogaWrap)newValue;
				ve.IncrementVersion(VersionChangeType.Layout);
			}
			break;
		case StylePropertyId.JustifyContent:
			if (layoutData.Read().justifyContent != (Justify)newValue)
			{
				layoutData.Write().justifyContent = (Justify)newValue;
				ve.yogaNode.JustifyContent = (YogaJustify)newValue;
				ve.IncrementVersion(VersionChangeType.Layout);
			}
			break;
		case StylePropertyId.Overflow:
			if (visualData.Read().overflow != (OverflowInternal)newValue)
			{
				visualData.Write().overflow = (OverflowInternal)newValue;
				ve.yogaNode.Overflow = (YogaOverflow)newValue;
				ve.IncrementVersion(VersionChangeType.Layout | VersionChangeType.Overflow);
			}
			break;
		case StylePropertyId.Position:
			if (layoutData.Read().position != (Position)newValue)
			{
				layoutData.Write().position = (Position)newValue;
				ve.yogaNode.PositionType = (YogaPositionType)newValue;
				ve.IncrementVersion(VersionChangeType.Layout);
			}
			break;
		case StylePropertyId.TextOverflow:
			if (rareData.Read().textOverflow != (TextOverflow)newValue)
			{
				rareData.Write().textOverflow = (TextOverflow)newValue;
				ve.IncrementVersion(VersionChangeType.Layout | VersionChangeType.Repaint);
			}
			break;
		case StylePropertyId.UnityFontStyleAndWeight:
			if (inheritedData.Read().unityFontStyleAndWeight != (FontStyle)newValue)
			{
				inheritedData.Write().unityFontStyleAndWeight = (FontStyle)newValue;
				ve.IncrementVersion(VersionChangeType.Layout | VersionChangeType.StyleSheet | VersionChangeType.Repaint);
			}
			break;
		case StylePropertyId.UnityOverflowClipBox:
			if (rareData.Read().unityOverflowClipBox != (OverflowClipBox)newValue)
			{
				rareData.Write().unityOverflowClipBox = (OverflowClipBox)newValue;
				ve.IncrementVersion(VersionChangeType.Repaint);
			}
			break;
		case StylePropertyId.UnitySliceBottom:
			rareData.Write().unitySliceBottom = newValue;
			ve.IncrementVersion(VersionChangeType.Repaint);
			break;
		case StylePropertyId.UnitySliceLeft:
			rareData.Write().unitySliceLeft = newValue;
			ve.IncrementVersion(VersionChangeType.Repaint);
			break;
		case StylePropertyId.UnitySliceRight:
			rareData.Write().unitySliceRight = newValue;
			ve.IncrementVersion(VersionChangeType.Repaint);
			break;
		case StylePropertyId.UnitySliceTop:
			rareData.Write().unitySliceTop = newValue;
			ve.IncrementVersion(VersionChangeType.Repaint);
			break;
		case StylePropertyId.UnityTextAlign:
			if (inheritedData.Read().unityTextAlign != (TextAnchor)newValue)
			{
				inheritedData.Write().unityTextAlign = (TextAnchor)newValue;
				ve.IncrementVersion(VersionChangeType.StyleSheet | VersionChangeType.Repaint);
			}
			break;
		case StylePropertyId.UnityTextOverflowPosition:
			if (rareData.Read().unityTextOverflowPosition != (TextOverflowPosition)newValue)
			{
				rareData.Write().unityTextOverflowPosition = (TextOverflowPosition)newValue;
				ve.IncrementVersion(VersionChangeType.Repaint);
			}
			break;
		case StylePropertyId.Visibility:
			if (inheritedData.Read().visibility != (Visibility)newValue)
			{
				inheritedData.Write().visibility = (Visibility)newValue;
				ve.IncrementVersion(VersionChangeType.StyleSheet | VersionChangeType.Repaint | VersionChangeType.Picking);
			}
			break;
		case StylePropertyId.WhiteSpace:
			if (inheritedData.Read().whiteSpace != (WhiteSpace)newValue)
			{
				inheritedData.Write().whiteSpace = (WhiteSpace)newValue;
				ve.IncrementVersion(VersionChangeType.Layout | VersionChangeType.StyleSheet);
			}
			break;
		default:
			throw new ArgumentException("Invalid animation property id. Can't apply value of type 'int' to property '" + id.ToString() + "'. Please make sure that this property is animatable.", "id");
		}
	}

	public void ApplyPropertyAnimation(VisualElement ve, StylePropertyId id, BackgroundPosition newValue)
	{
		switch (id)
		{
		case StylePropertyId.BackgroundPositionX:
			if (visualData.Read().backgroundPositionX != newValue)
			{
				visualData.Write().backgroundPositionX = newValue;
				ve.IncrementVersion(VersionChangeType.Repaint);
			}
			break;
		case StylePropertyId.BackgroundPositionY:
			if (visualData.Read().backgroundPositionY != newValue)
			{
				visualData.Write().backgroundPositionY = newValue;
				ve.IncrementVersion(VersionChangeType.Repaint);
			}
			break;
		default:
			throw new ArgumentException("Invalid animation property id. Can't apply value of type 'BackgroundPosition' to property '" + id.ToString() + "'. Please make sure that this property is animatable.", "id");
		}
	}

	public void ApplyPropertyAnimation(VisualElement ve, StylePropertyId id, BackgroundRepeat newValue)
	{
		StylePropertyId stylePropertyId = id;
		StylePropertyId stylePropertyId2 = stylePropertyId;
		if (stylePropertyId2 == StylePropertyId.BackgroundRepeat)
		{
			if (visualData.Read().backgroundRepeat != newValue)
			{
				visualData.Write().backgroundRepeat = newValue;
				ve.IncrementVersion(VersionChangeType.Repaint);
			}
			return;
		}
		throw new ArgumentException("Invalid animation property id. Can't apply value of type 'BackgroundRepeat' to property '" + id.ToString() + "'. Please make sure that this property is animatable.", "id");
	}

	public void ApplyPropertyAnimation(VisualElement ve, StylePropertyId id, BackgroundSize newValue)
	{
		StylePropertyId stylePropertyId = id;
		StylePropertyId stylePropertyId2 = stylePropertyId;
		if (stylePropertyId2 == StylePropertyId.BackgroundSize)
		{
			visualData.Write().backgroundSize = newValue;
			ve.IncrementVersion(VersionChangeType.Repaint);
			return;
		}
		throw new ArgumentException("Invalid animation property id. Can't apply value of type 'BackgroundSize' to property '" + id.ToString() + "'. Please make sure that this property is animatable.", "id");
	}

	public void ApplyPropertyAnimation(VisualElement ve, StylePropertyId id, Color newValue)
	{
		switch (id)
		{
		case StylePropertyId.BackgroundColor:
			visualData.Write().backgroundColor = newValue;
			ve.IncrementVersion(VersionChangeType.Color);
			break;
		case StylePropertyId.BorderBottomColor:
			visualData.Write().borderBottomColor = newValue;
			ve.IncrementVersion(VersionChangeType.Color);
			break;
		case StylePropertyId.BorderLeftColor:
			visualData.Write().borderLeftColor = newValue;
			ve.IncrementVersion(VersionChangeType.Color);
			break;
		case StylePropertyId.BorderRightColor:
			visualData.Write().borderRightColor = newValue;
			ve.IncrementVersion(VersionChangeType.Color);
			break;
		case StylePropertyId.BorderTopColor:
			visualData.Write().borderTopColor = newValue;
			ve.IncrementVersion(VersionChangeType.Color);
			break;
		case StylePropertyId.Color:
			inheritedData.Write().color = newValue;
			ve.IncrementVersion(VersionChangeType.StyleSheet | VersionChangeType.Color);
			break;
		case StylePropertyId.UnityBackgroundImageTintColor:
			rareData.Write().unityBackgroundImageTintColor = newValue;
			ve.IncrementVersion(VersionChangeType.Color);
			break;
		case StylePropertyId.UnityTextOutlineColor:
			inheritedData.Write().unityTextOutlineColor = newValue;
			ve.IncrementVersion(VersionChangeType.StyleSheet | VersionChangeType.Repaint);
			break;
		default:
			throw new ArgumentException("Invalid animation property id. Can't apply value of type 'Color' to property '" + id.ToString() + "'. Please make sure that this property is animatable.", "id");
		}
	}

	public void ApplyPropertyAnimation(VisualElement ve, StylePropertyId id, Background newValue)
	{
		StylePropertyId stylePropertyId = id;
		StylePropertyId stylePropertyId2 = stylePropertyId;
		if (stylePropertyId2 == StylePropertyId.BackgroundImage)
		{
			if (visualData.Read().backgroundImage != newValue)
			{
				visualData.Write().backgroundImage = newValue;
				ve.IncrementVersion(VersionChangeType.Repaint);
			}
			return;
		}
		throw new ArgumentException("Invalid animation property id. Can't apply value of type 'Background' to property '" + id.ToString() + "'. Please make sure that this property is animatable.", "id");
	}

	public void ApplyPropertyAnimation(VisualElement ve, StylePropertyId id, Font newValue)
	{
		StylePropertyId stylePropertyId = id;
		StylePropertyId stylePropertyId2 = stylePropertyId;
		if (stylePropertyId2 == StylePropertyId.UnityFont)
		{
			if (inheritedData.Read().unityFont != newValue)
			{
				inheritedData.Write().unityFont = newValue;
				ve.IncrementVersion(VersionChangeType.Layout | VersionChangeType.StyleSheet | VersionChangeType.Repaint);
			}
			return;
		}
		throw new ArgumentException("Invalid animation property id. Can't apply value of type 'Font' to property '" + id.ToString() + "'. Please make sure that this property is animatable.", "id");
	}

	public void ApplyPropertyAnimation(VisualElement ve, StylePropertyId id, FontDefinition newValue)
	{
		StylePropertyId stylePropertyId = id;
		StylePropertyId stylePropertyId2 = stylePropertyId;
		if (stylePropertyId2 == StylePropertyId.UnityFontDefinition)
		{
			if (inheritedData.Read().unityFontDefinition != newValue)
			{
				inheritedData.Write().unityFontDefinition = newValue;
				ve.IncrementVersion(VersionChangeType.Layout | VersionChangeType.StyleSheet | VersionChangeType.Repaint);
			}
			return;
		}
		throw new ArgumentException("Invalid animation property id. Can't apply value of type 'FontDefinition' to property '" + id.ToString() + "'. Please make sure that this property is animatable.", "id");
	}

	public void ApplyPropertyAnimation(VisualElement ve, StylePropertyId id, TextShadow newValue)
	{
		StylePropertyId stylePropertyId = id;
		StylePropertyId stylePropertyId2 = stylePropertyId;
		if (stylePropertyId2 == StylePropertyId.TextShadow)
		{
			inheritedData.Write().textShadow = newValue;
			ve.IncrementVersion(VersionChangeType.StyleSheet | VersionChangeType.Repaint);
			return;
		}
		throw new ArgumentException("Invalid animation property id. Can't apply value of type 'TextShadow' to property '" + id.ToString() + "'. Please make sure that this property is animatable.", "id");
	}

	public void ApplyPropertyAnimation(VisualElement ve, StylePropertyId id, Translate newValue)
	{
		StylePropertyId stylePropertyId = id;
		StylePropertyId stylePropertyId2 = stylePropertyId;
		if (stylePropertyId2 == StylePropertyId.Translate)
		{
			transformData.Write().translate = newValue;
			ve.IncrementVersion(VersionChangeType.Transform);
			return;
		}
		throw new ArgumentException("Invalid animation property id. Can't apply value of type 'Translate' to property '" + id.ToString() + "'. Please make sure that this property is animatable.", "id");
	}

	public void ApplyPropertyAnimation(VisualElement ve, StylePropertyId id, TransformOrigin newValue)
	{
		StylePropertyId stylePropertyId = id;
		StylePropertyId stylePropertyId2 = stylePropertyId;
		if (stylePropertyId2 == StylePropertyId.TransformOrigin)
		{
			transformData.Write().transformOrigin = newValue;
			ve.IncrementVersion(VersionChangeType.Transform);
			return;
		}
		throw new ArgumentException("Invalid animation property id. Can't apply value of type 'TransformOrigin' to property '" + id.ToString() + "'. Please make sure that this property is animatable.", "id");
	}

	public void ApplyPropertyAnimation(VisualElement ve, StylePropertyId id, Rotate newValue)
	{
		StylePropertyId stylePropertyId = id;
		StylePropertyId stylePropertyId2 = stylePropertyId;
		if (stylePropertyId2 == StylePropertyId.Rotate)
		{
			transformData.Write().rotate = newValue;
			ve.IncrementVersion(VersionChangeType.Transform);
			return;
		}
		throw new ArgumentException("Invalid animation property id. Can't apply value of type 'Rotate' to property '" + id.ToString() + "'. Please make sure that this property is animatable.", "id");
	}

	public void ApplyPropertyAnimation(VisualElement ve, StylePropertyId id, Scale newValue)
	{
		StylePropertyId stylePropertyId = id;
		StylePropertyId stylePropertyId2 = stylePropertyId;
		if (stylePropertyId2 == StylePropertyId.Scale)
		{
			transformData.Write().scale = newValue;
			ve.IncrementVersion(VersionChangeType.Transform);
			return;
		}
		throw new ArgumentException("Invalid animation property id. Can't apply value of type 'Scale' to property '" + id.ToString() + "'. Please make sure that this property is animatable.", "id");
	}

	public static bool StartAnimation(VisualElement element, StylePropertyId id, ref ComputedStyle oldStyle, ref ComputedStyle newStyle, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		switch (id)
		{
		case StylePropertyId.AlignContent:
			return element.styleAnimation.StartEnum(StylePropertyId.AlignContent, (int)oldStyle.layoutData.Read().alignContent, (int)newStyle.layoutData.Read().alignContent, durationMs, delayMs, easingCurve);
		case StylePropertyId.AlignItems:
			return element.styleAnimation.StartEnum(StylePropertyId.AlignItems, (int)oldStyle.layoutData.Read().alignItems, (int)newStyle.layoutData.Read().alignItems, durationMs, delayMs, easingCurve);
		case StylePropertyId.AlignSelf:
			return element.styleAnimation.StartEnum(StylePropertyId.AlignSelf, (int)oldStyle.layoutData.Read().alignSelf, (int)newStyle.layoutData.Read().alignSelf, durationMs, delayMs, easingCurve);
		case StylePropertyId.All:
			return StartAnimationAllProperty(element, ref oldStyle, ref newStyle, durationMs, delayMs, easingCurve);
		case StylePropertyId.BackgroundColor:
		{
			bool flag13 = element.styleAnimation.Start(StylePropertyId.BackgroundColor, oldStyle.visualData.Read().backgroundColor, newStyle.visualData.Read().backgroundColor, durationMs, delayMs, easingCurve);
			if (flag13 && (element.usageHints & UsageHints.DynamicColor) == 0)
			{
				element.usageHints |= UsageHints.DynamicColor;
			}
			return flag13;
		}
		case StylePropertyId.BackgroundImage:
			return element.styleAnimation.Start(StylePropertyId.BackgroundImage, oldStyle.visualData.Read().backgroundImage, newStyle.visualData.Read().backgroundImage, durationMs, delayMs, easingCurve);
		case StylePropertyId.BackgroundPosition:
		{
			bool flag11 = false;
			flag11 |= element.styleAnimation.Start(StylePropertyId.BackgroundPositionX, oldStyle.visualData.Read().backgroundPositionX, newStyle.visualData.Read().backgroundPositionX, durationMs, delayMs, easingCurve);
			return flag11 | element.styleAnimation.Start(StylePropertyId.BackgroundPositionY, oldStyle.visualData.Read().backgroundPositionY, newStyle.visualData.Read().backgroundPositionY, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.BackgroundPositionX:
			return element.styleAnimation.Start(StylePropertyId.BackgroundPositionX, oldStyle.visualData.Read().backgroundPositionX, newStyle.visualData.Read().backgroundPositionX, durationMs, delayMs, easingCurve);
		case StylePropertyId.BackgroundPositionY:
			return element.styleAnimation.Start(StylePropertyId.BackgroundPositionY, oldStyle.visualData.Read().backgroundPositionY, newStyle.visualData.Read().backgroundPositionY, durationMs, delayMs, easingCurve);
		case StylePropertyId.BackgroundRepeat:
			return element.styleAnimation.Start(StylePropertyId.BackgroundRepeat, oldStyle.visualData.Read().backgroundRepeat, newStyle.visualData.Read().backgroundRepeat, durationMs, delayMs, easingCurve);
		case StylePropertyId.BackgroundSize:
			return element.styleAnimation.Start(StylePropertyId.BackgroundSize, oldStyle.visualData.Read().backgroundSize, newStyle.visualData.Read().backgroundSize, durationMs, delayMs, easingCurve);
		case StylePropertyId.BorderBottomColor:
		{
			bool flag20 = element.styleAnimation.Start(StylePropertyId.BorderBottomColor, oldStyle.visualData.Read().borderBottomColor, newStyle.visualData.Read().borderBottomColor, durationMs, delayMs, easingCurve);
			if (flag20 && (element.usageHints & UsageHints.DynamicColor) == 0)
			{
				element.usageHints |= UsageHints.DynamicColor;
			}
			return flag20;
		}
		case StylePropertyId.BorderBottomLeftRadius:
			return element.styleAnimation.Start(StylePropertyId.BorderBottomLeftRadius, oldStyle.visualData.Read().borderBottomLeftRadius, newStyle.visualData.Read().borderBottomLeftRadius, durationMs, delayMs, easingCurve);
		case StylePropertyId.BorderBottomRightRadius:
			return element.styleAnimation.Start(StylePropertyId.BorderBottomRightRadius, oldStyle.visualData.Read().borderBottomRightRadius, newStyle.visualData.Read().borderBottomRightRadius, durationMs, delayMs, easingCurve);
		case StylePropertyId.BorderBottomWidth:
			return element.styleAnimation.Start(StylePropertyId.BorderBottomWidth, oldStyle.layoutData.Read().borderBottomWidth, newStyle.layoutData.Read().borderBottomWidth, durationMs, delayMs, easingCurve);
		case StylePropertyId.BorderColor:
		{
			bool flag10 = false;
			flag10 |= element.styleAnimation.Start(StylePropertyId.BorderTopColor, oldStyle.visualData.Read().borderTopColor, newStyle.visualData.Read().borderTopColor, durationMs, delayMs, easingCurve);
			flag10 |= element.styleAnimation.Start(StylePropertyId.BorderRightColor, oldStyle.visualData.Read().borderRightColor, newStyle.visualData.Read().borderRightColor, durationMs, delayMs, easingCurve);
			flag10 |= element.styleAnimation.Start(StylePropertyId.BorderBottomColor, oldStyle.visualData.Read().borderBottomColor, newStyle.visualData.Read().borderBottomColor, durationMs, delayMs, easingCurve);
			flag10 |= element.styleAnimation.Start(StylePropertyId.BorderLeftColor, oldStyle.visualData.Read().borderLeftColor, newStyle.visualData.Read().borderLeftColor, durationMs, delayMs, easingCurve);
			if (flag10 && (element.usageHints & UsageHints.DynamicColor) == 0)
			{
				element.usageHints |= UsageHints.DynamicColor;
			}
			return flag10;
		}
		case StylePropertyId.BorderLeftColor:
		{
			bool flag8 = element.styleAnimation.Start(StylePropertyId.BorderLeftColor, oldStyle.visualData.Read().borderLeftColor, newStyle.visualData.Read().borderLeftColor, durationMs, delayMs, easingCurve);
			if (flag8 && (element.usageHints & UsageHints.DynamicColor) == 0)
			{
				element.usageHints |= UsageHints.DynamicColor;
			}
			return flag8;
		}
		case StylePropertyId.BorderLeftWidth:
			return element.styleAnimation.Start(StylePropertyId.BorderLeftWidth, oldStyle.layoutData.Read().borderLeftWidth, newStyle.layoutData.Read().borderLeftWidth, durationMs, delayMs, easingCurve);
		case StylePropertyId.BorderRadius:
		{
			bool flag6 = false;
			flag6 |= element.styleAnimation.Start(StylePropertyId.BorderTopLeftRadius, oldStyle.visualData.Read().borderTopLeftRadius, newStyle.visualData.Read().borderTopLeftRadius, durationMs, delayMs, easingCurve);
			flag6 |= element.styleAnimation.Start(StylePropertyId.BorderTopRightRadius, oldStyle.visualData.Read().borderTopRightRadius, newStyle.visualData.Read().borderTopRightRadius, durationMs, delayMs, easingCurve);
			flag6 |= element.styleAnimation.Start(StylePropertyId.BorderBottomRightRadius, oldStyle.visualData.Read().borderBottomRightRadius, newStyle.visualData.Read().borderBottomRightRadius, durationMs, delayMs, easingCurve);
			return flag6 | element.styleAnimation.Start(StylePropertyId.BorderBottomLeftRadius, oldStyle.visualData.Read().borderBottomLeftRadius, newStyle.visualData.Read().borderBottomLeftRadius, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.BorderRightColor:
		{
			bool flag4 = element.styleAnimation.Start(StylePropertyId.BorderRightColor, oldStyle.visualData.Read().borderRightColor, newStyle.visualData.Read().borderRightColor, durationMs, delayMs, easingCurve);
			if (flag4 && (element.usageHints & UsageHints.DynamicColor) == 0)
			{
				element.usageHints |= UsageHints.DynamicColor;
			}
			return flag4;
		}
		case StylePropertyId.BorderRightWidth:
			return element.styleAnimation.Start(StylePropertyId.BorderRightWidth, oldStyle.layoutData.Read().borderRightWidth, newStyle.layoutData.Read().borderRightWidth, durationMs, delayMs, easingCurve);
		case StylePropertyId.BorderTopColor:
		{
			bool flag19 = element.styleAnimation.Start(StylePropertyId.BorderTopColor, oldStyle.visualData.Read().borderTopColor, newStyle.visualData.Read().borderTopColor, durationMs, delayMs, easingCurve);
			if (flag19 && (element.usageHints & UsageHints.DynamicColor) == 0)
			{
				element.usageHints |= UsageHints.DynamicColor;
			}
			return flag19;
		}
		case StylePropertyId.BorderTopLeftRadius:
			return element.styleAnimation.Start(StylePropertyId.BorderTopLeftRadius, oldStyle.visualData.Read().borderTopLeftRadius, newStyle.visualData.Read().borderTopLeftRadius, durationMs, delayMs, easingCurve);
		case StylePropertyId.BorderTopRightRadius:
			return element.styleAnimation.Start(StylePropertyId.BorderTopRightRadius, oldStyle.visualData.Read().borderTopRightRadius, newStyle.visualData.Read().borderTopRightRadius, durationMs, delayMs, easingCurve);
		case StylePropertyId.BorderTopWidth:
			return element.styleAnimation.Start(StylePropertyId.BorderTopWidth, oldStyle.layoutData.Read().borderTopWidth, newStyle.layoutData.Read().borderTopWidth, durationMs, delayMs, easingCurve);
		case StylePropertyId.BorderWidth:
		{
			bool flag18 = false;
			flag18 |= element.styleAnimation.Start(StylePropertyId.BorderTopWidth, oldStyle.layoutData.Read().borderTopWidth, newStyle.layoutData.Read().borderTopWidth, durationMs, delayMs, easingCurve);
			flag18 |= element.styleAnimation.Start(StylePropertyId.BorderRightWidth, oldStyle.layoutData.Read().borderRightWidth, newStyle.layoutData.Read().borderRightWidth, durationMs, delayMs, easingCurve);
			flag18 |= element.styleAnimation.Start(StylePropertyId.BorderBottomWidth, oldStyle.layoutData.Read().borderBottomWidth, newStyle.layoutData.Read().borderBottomWidth, durationMs, delayMs, easingCurve);
			return flag18 | element.styleAnimation.Start(StylePropertyId.BorderLeftWidth, oldStyle.layoutData.Read().borderLeftWidth, newStyle.layoutData.Read().borderLeftWidth, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.Bottom:
			return element.styleAnimation.Start(StylePropertyId.Bottom, oldStyle.layoutData.Read().bottom, newStyle.layoutData.Read().bottom, durationMs, delayMs, easingCurve);
		case StylePropertyId.Color:
		{
			bool flag17 = element.styleAnimation.Start(StylePropertyId.Color, oldStyle.inheritedData.Read().color, newStyle.inheritedData.Read().color, durationMs, delayMs, easingCurve);
			if (flag17 && (element.usageHints & UsageHints.DynamicColor) == 0)
			{
				element.usageHints |= UsageHints.DynamicColor;
			}
			return flag17;
		}
		case StylePropertyId.Display:
			return element.styleAnimation.StartEnum(StylePropertyId.Display, (int)oldStyle.layoutData.Read().display, (int)newStyle.layoutData.Read().display, durationMs, delayMs, easingCurve);
		case StylePropertyId.Flex:
		{
			bool flag16 = false;
			flag16 |= element.styleAnimation.Start(StylePropertyId.FlexGrow, oldStyle.layoutData.Read().flexGrow, newStyle.layoutData.Read().flexGrow, durationMs, delayMs, easingCurve);
			flag16 |= element.styleAnimation.Start(StylePropertyId.FlexShrink, oldStyle.layoutData.Read().flexShrink, newStyle.layoutData.Read().flexShrink, durationMs, delayMs, easingCurve);
			return flag16 | element.styleAnimation.Start(StylePropertyId.FlexBasis, oldStyle.layoutData.Read().flexBasis, newStyle.layoutData.Read().flexBasis, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.FlexBasis:
			return element.styleAnimation.Start(StylePropertyId.FlexBasis, oldStyle.layoutData.Read().flexBasis, newStyle.layoutData.Read().flexBasis, durationMs, delayMs, easingCurve);
		case StylePropertyId.FlexDirection:
			return element.styleAnimation.StartEnum(StylePropertyId.FlexDirection, (int)oldStyle.layoutData.Read().flexDirection, (int)newStyle.layoutData.Read().flexDirection, durationMs, delayMs, easingCurve);
		case StylePropertyId.FlexGrow:
			return element.styleAnimation.Start(StylePropertyId.FlexGrow, oldStyle.layoutData.Read().flexGrow, newStyle.layoutData.Read().flexGrow, durationMs, delayMs, easingCurve);
		case StylePropertyId.FlexShrink:
			return element.styleAnimation.Start(StylePropertyId.FlexShrink, oldStyle.layoutData.Read().flexShrink, newStyle.layoutData.Read().flexShrink, durationMs, delayMs, easingCurve);
		case StylePropertyId.FlexWrap:
			return element.styleAnimation.StartEnum(StylePropertyId.FlexWrap, (int)oldStyle.layoutData.Read().flexWrap, (int)newStyle.layoutData.Read().flexWrap, durationMs, delayMs, easingCurve);
		case StylePropertyId.FontSize:
			return element.styleAnimation.Start(StylePropertyId.FontSize, oldStyle.inheritedData.Read().fontSize, newStyle.inheritedData.Read().fontSize, durationMs, delayMs, easingCurve);
		case StylePropertyId.Height:
			return element.styleAnimation.Start(StylePropertyId.Height, oldStyle.layoutData.Read().height, newStyle.layoutData.Read().height, durationMs, delayMs, easingCurve);
		case StylePropertyId.JustifyContent:
			return element.styleAnimation.StartEnum(StylePropertyId.JustifyContent, (int)oldStyle.layoutData.Read().justifyContent, (int)newStyle.layoutData.Read().justifyContent, durationMs, delayMs, easingCurve);
		case StylePropertyId.Left:
			return element.styleAnimation.Start(StylePropertyId.Left, oldStyle.layoutData.Read().left, newStyle.layoutData.Read().left, durationMs, delayMs, easingCurve);
		case StylePropertyId.LetterSpacing:
			return element.styleAnimation.Start(StylePropertyId.LetterSpacing, oldStyle.inheritedData.Read().letterSpacing, newStyle.inheritedData.Read().letterSpacing, durationMs, delayMs, easingCurve);
		case StylePropertyId.Margin:
		{
			bool flag15 = false;
			flag15 |= element.styleAnimation.Start(StylePropertyId.MarginTop, oldStyle.layoutData.Read().marginTop, newStyle.layoutData.Read().marginTop, durationMs, delayMs, easingCurve);
			flag15 |= element.styleAnimation.Start(StylePropertyId.MarginRight, oldStyle.layoutData.Read().marginRight, newStyle.layoutData.Read().marginRight, durationMs, delayMs, easingCurve);
			flag15 |= element.styleAnimation.Start(StylePropertyId.MarginBottom, oldStyle.layoutData.Read().marginBottom, newStyle.layoutData.Read().marginBottom, durationMs, delayMs, easingCurve);
			return flag15 | element.styleAnimation.Start(StylePropertyId.MarginLeft, oldStyle.layoutData.Read().marginLeft, newStyle.layoutData.Read().marginLeft, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.MarginBottom:
			return element.styleAnimation.Start(StylePropertyId.MarginBottom, oldStyle.layoutData.Read().marginBottom, newStyle.layoutData.Read().marginBottom, durationMs, delayMs, easingCurve);
		case StylePropertyId.MarginLeft:
			return element.styleAnimation.Start(StylePropertyId.MarginLeft, oldStyle.layoutData.Read().marginLeft, newStyle.layoutData.Read().marginLeft, durationMs, delayMs, easingCurve);
		case StylePropertyId.MarginRight:
			return element.styleAnimation.Start(StylePropertyId.MarginRight, oldStyle.layoutData.Read().marginRight, newStyle.layoutData.Read().marginRight, durationMs, delayMs, easingCurve);
		case StylePropertyId.MarginTop:
			return element.styleAnimation.Start(StylePropertyId.MarginTop, oldStyle.layoutData.Read().marginTop, newStyle.layoutData.Read().marginTop, durationMs, delayMs, easingCurve);
		case StylePropertyId.MaxHeight:
			return element.styleAnimation.Start(StylePropertyId.MaxHeight, oldStyle.layoutData.Read().maxHeight, newStyle.layoutData.Read().maxHeight, durationMs, delayMs, easingCurve);
		case StylePropertyId.MaxWidth:
			return element.styleAnimation.Start(StylePropertyId.MaxWidth, oldStyle.layoutData.Read().maxWidth, newStyle.layoutData.Read().maxWidth, durationMs, delayMs, easingCurve);
		case StylePropertyId.MinHeight:
			return element.styleAnimation.Start(StylePropertyId.MinHeight, oldStyle.layoutData.Read().minHeight, newStyle.layoutData.Read().minHeight, durationMs, delayMs, easingCurve);
		case StylePropertyId.MinWidth:
			return element.styleAnimation.Start(StylePropertyId.MinWidth, oldStyle.layoutData.Read().minWidth, newStyle.layoutData.Read().minWidth, durationMs, delayMs, easingCurve);
		case StylePropertyId.Opacity:
			return element.styleAnimation.Start(StylePropertyId.Opacity, oldStyle.visualData.Read().opacity, newStyle.visualData.Read().opacity, durationMs, delayMs, easingCurve);
		case StylePropertyId.Overflow:
			return element.styleAnimation.StartEnum(StylePropertyId.Overflow, (int)oldStyle.visualData.Read().overflow, (int)newStyle.visualData.Read().overflow, durationMs, delayMs, easingCurve);
		case StylePropertyId.Padding:
		{
			bool flag14 = false;
			flag14 |= element.styleAnimation.Start(StylePropertyId.PaddingTop, oldStyle.layoutData.Read().paddingTop, newStyle.layoutData.Read().paddingTop, durationMs, delayMs, easingCurve);
			flag14 |= element.styleAnimation.Start(StylePropertyId.PaddingRight, oldStyle.layoutData.Read().paddingRight, newStyle.layoutData.Read().paddingRight, durationMs, delayMs, easingCurve);
			flag14 |= element.styleAnimation.Start(StylePropertyId.PaddingBottom, oldStyle.layoutData.Read().paddingBottom, newStyle.layoutData.Read().paddingBottom, durationMs, delayMs, easingCurve);
			return flag14 | element.styleAnimation.Start(StylePropertyId.PaddingLeft, oldStyle.layoutData.Read().paddingLeft, newStyle.layoutData.Read().paddingLeft, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.PaddingBottom:
			return element.styleAnimation.Start(StylePropertyId.PaddingBottom, oldStyle.layoutData.Read().paddingBottom, newStyle.layoutData.Read().paddingBottom, durationMs, delayMs, easingCurve);
		case StylePropertyId.PaddingLeft:
			return element.styleAnimation.Start(StylePropertyId.PaddingLeft, oldStyle.layoutData.Read().paddingLeft, newStyle.layoutData.Read().paddingLeft, durationMs, delayMs, easingCurve);
		case StylePropertyId.PaddingRight:
			return element.styleAnimation.Start(StylePropertyId.PaddingRight, oldStyle.layoutData.Read().paddingRight, newStyle.layoutData.Read().paddingRight, durationMs, delayMs, easingCurve);
		case StylePropertyId.PaddingTop:
			return element.styleAnimation.Start(StylePropertyId.PaddingTop, oldStyle.layoutData.Read().paddingTop, newStyle.layoutData.Read().paddingTop, durationMs, delayMs, easingCurve);
		case StylePropertyId.Position:
			return element.styleAnimation.StartEnum(StylePropertyId.Position, (int)oldStyle.layoutData.Read().position, (int)newStyle.layoutData.Read().position, durationMs, delayMs, easingCurve);
		case StylePropertyId.Right:
			return element.styleAnimation.Start(StylePropertyId.Right, oldStyle.layoutData.Read().right, newStyle.layoutData.Read().right, durationMs, delayMs, easingCurve);
		case StylePropertyId.Rotate:
		{
			bool flag12 = element.styleAnimation.Start(StylePropertyId.Rotate, oldStyle.transformData.Read().rotate, newStyle.transformData.Read().rotate, durationMs, delayMs, easingCurve);
			if (flag12 && (element.usageHints & UsageHints.DynamicTransform) == 0)
			{
				element.usageHints |= UsageHints.DynamicTransform;
			}
			return flag12;
		}
		case StylePropertyId.Scale:
		{
			bool flag9 = element.styleAnimation.Start(StylePropertyId.Scale, oldStyle.transformData.Read().scale, newStyle.transformData.Read().scale, durationMs, delayMs, easingCurve);
			if (flag9 && (element.usageHints & UsageHints.DynamicTransform) == 0)
			{
				element.usageHints |= UsageHints.DynamicTransform;
			}
			return flag9;
		}
		case StylePropertyId.TextOverflow:
			return element.styleAnimation.StartEnum(StylePropertyId.TextOverflow, (int)oldStyle.rareData.Read().textOverflow, (int)newStyle.rareData.Read().textOverflow, durationMs, delayMs, easingCurve);
		case StylePropertyId.TextShadow:
			return element.styleAnimation.Start(StylePropertyId.TextShadow, oldStyle.inheritedData.Read().textShadow, newStyle.inheritedData.Read().textShadow, durationMs, delayMs, easingCurve);
		case StylePropertyId.Top:
			return element.styleAnimation.Start(StylePropertyId.Top, oldStyle.layoutData.Read().top, newStyle.layoutData.Read().top, durationMs, delayMs, easingCurve);
		case StylePropertyId.TransformOrigin:
		{
			bool flag7 = element.styleAnimation.Start(StylePropertyId.TransformOrigin, oldStyle.transformData.Read().transformOrigin, newStyle.transformData.Read().transformOrigin, durationMs, delayMs, easingCurve);
			if (flag7 && (element.usageHints & UsageHints.DynamicTransform) == 0)
			{
				element.usageHints |= UsageHints.DynamicTransform;
			}
			return flag7;
		}
		case StylePropertyId.Translate:
		{
			bool flag5 = element.styleAnimation.Start(StylePropertyId.Translate, oldStyle.transformData.Read().translate, newStyle.transformData.Read().translate, durationMs, delayMs, easingCurve);
			if (flag5 && (element.usageHints & UsageHints.DynamicTransform) == 0)
			{
				element.usageHints |= UsageHints.DynamicTransform;
			}
			return flag5;
		}
		case StylePropertyId.UnityBackgroundImageTintColor:
		{
			bool flag3 = element.styleAnimation.Start(StylePropertyId.UnityBackgroundImageTintColor, oldStyle.rareData.Read().unityBackgroundImageTintColor, newStyle.rareData.Read().unityBackgroundImageTintColor, durationMs, delayMs, easingCurve);
			if (flag3 && (element.usageHints & UsageHints.DynamicColor) == 0)
			{
				element.usageHints |= UsageHints.DynamicColor;
			}
			return flag3;
		}
		case StylePropertyId.UnityBackgroundScaleMode:
		{
			bool flag2 = false;
			flag2 |= element.styleAnimation.Start(StylePropertyId.BackgroundPositionX, oldStyle.visualData.Read().backgroundPositionX, newStyle.visualData.Read().backgroundPositionX, durationMs, delayMs, easingCurve);
			flag2 |= element.styleAnimation.Start(StylePropertyId.BackgroundPositionY, oldStyle.visualData.Read().backgroundPositionY, newStyle.visualData.Read().backgroundPositionY, durationMs, delayMs, easingCurve);
			flag2 |= element.styleAnimation.Start(StylePropertyId.BackgroundRepeat, oldStyle.visualData.Read().backgroundRepeat, newStyle.visualData.Read().backgroundRepeat, durationMs, delayMs, easingCurve);
			return flag2 | element.styleAnimation.Start(StylePropertyId.BackgroundSize, oldStyle.visualData.Read().backgroundSize, newStyle.visualData.Read().backgroundSize, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.UnityFont:
			return element.styleAnimation.Start(StylePropertyId.UnityFont, oldStyle.inheritedData.Read().unityFont, newStyle.inheritedData.Read().unityFont, durationMs, delayMs, easingCurve);
		case StylePropertyId.UnityFontDefinition:
			return element.styleAnimation.Start(StylePropertyId.UnityFontDefinition, oldStyle.inheritedData.Read().unityFontDefinition, newStyle.inheritedData.Read().unityFontDefinition, durationMs, delayMs, easingCurve);
		case StylePropertyId.UnityFontStyleAndWeight:
			return element.styleAnimation.StartEnum(StylePropertyId.UnityFontStyleAndWeight, (int)oldStyle.inheritedData.Read().unityFontStyleAndWeight, (int)newStyle.inheritedData.Read().unityFontStyleAndWeight, durationMs, delayMs, easingCurve);
		case StylePropertyId.UnityOverflowClipBox:
			return element.styleAnimation.StartEnum(StylePropertyId.UnityOverflowClipBox, (int)oldStyle.rareData.Read().unityOverflowClipBox, (int)newStyle.rareData.Read().unityOverflowClipBox, durationMs, delayMs, easingCurve);
		case StylePropertyId.UnityParagraphSpacing:
			return element.styleAnimation.Start(StylePropertyId.UnityParagraphSpacing, oldStyle.inheritedData.Read().unityParagraphSpacing, newStyle.inheritedData.Read().unityParagraphSpacing, durationMs, delayMs, easingCurve);
		case StylePropertyId.UnitySliceBottom:
			return element.styleAnimation.Start(StylePropertyId.UnitySliceBottom, oldStyle.rareData.Read().unitySliceBottom, newStyle.rareData.Read().unitySliceBottom, durationMs, delayMs, easingCurve);
		case StylePropertyId.UnitySliceLeft:
			return element.styleAnimation.Start(StylePropertyId.UnitySliceLeft, oldStyle.rareData.Read().unitySliceLeft, newStyle.rareData.Read().unitySliceLeft, durationMs, delayMs, easingCurve);
		case StylePropertyId.UnitySliceRight:
			return element.styleAnimation.Start(StylePropertyId.UnitySliceRight, oldStyle.rareData.Read().unitySliceRight, newStyle.rareData.Read().unitySliceRight, durationMs, delayMs, easingCurve);
		case StylePropertyId.UnitySliceScale:
			return element.styleAnimation.Start(StylePropertyId.UnitySliceScale, oldStyle.rareData.Read().unitySliceScale, newStyle.rareData.Read().unitySliceScale, durationMs, delayMs, easingCurve);
		case StylePropertyId.UnitySliceTop:
			return element.styleAnimation.Start(StylePropertyId.UnitySliceTop, oldStyle.rareData.Read().unitySliceTop, newStyle.rareData.Read().unitySliceTop, durationMs, delayMs, easingCurve);
		case StylePropertyId.UnityTextAlign:
			return element.styleAnimation.StartEnum(StylePropertyId.UnityTextAlign, (int)oldStyle.inheritedData.Read().unityTextAlign, (int)newStyle.inheritedData.Read().unityTextAlign, durationMs, delayMs, easingCurve);
		case StylePropertyId.UnityTextOutline:
		{
			bool flag = false;
			flag |= element.styleAnimation.Start(StylePropertyId.UnityTextOutlineColor, oldStyle.inheritedData.Read().unityTextOutlineColor, newStyle.inheritedData.Read().unityTextOutlineColor, durationMs, delayMs, easingCurve);
			return flag | element.styleAnimation.Start(StylePropertyId.UnityTextOutlineWidth, oldStyle.inheritedData.Read().unityTextOutlineWidth, newStyle.inheritedData.Read().unityTextOutlineWidth, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.UnityTextOutlineColor:
			return element.styleAnimation.Start(StylePropertyId.UnityTextOutlineColor, oldStyle.inheritedData.Read().unityTextOutlineColor, newStyle.inheritedData.Read().unityTextOutlineColor, durationMs, delayMs, easingCurve);
		case StylePropertyId.UnityTextOutlineWidth:
			return element.styleAnimation.Start(StylePropertyId.UnityTextOutlineWidth, oldStyle.inheritedData.Read().unityTextOutlineWidth, newStyle.inheritedData.Read().unityTextOutlineWidth, durationMs, delayMs, easingCurve);
		case StylePropertyId.UnityTextOverflowPosition:
			return element.styleAnimation.StartEnum(StylePropertyId.UnityTextOverflowPosition, (int)oldStyle.rareData.Read().unityTextOverflowPosition, (int)newStyle.rareData.Read().unityTextOverflowPosition, durationMs, delayMs, easingCurve);
		case StylePropertyId.Visibility:
			return element.styleAnimation.StartEnum(StylePropertyId.Visibility, (int)oldStyle.inheritedData.Read().visibility, (int)newStyle.inheritedData.Read().visibility, durationMs, delayMs, easingCurve);
		case StylePropertyId.WhiteSpace:
			return element.styleAnimation.StartEnum(StylePropertyId.WhiteSpace, (int)oldStyle.inheritedData.Read().whiteSpace, (int)newStyle.inheritedData.Read().whiteSpace, durationMs, delayMs, easingCurve);
		case StylePropertyId.Width:
			return element.styleAnimation.Start(StylePropertyId.Width, oldStyle.layoutData.Read().width, newStyle.layoutData.Read().width, durationMs, delayMs, easingCurve);
		case StylePropertyId.WordSpacing:
			return element.styleAnimation.Start(StylePropertyId.WordSpacing, oldStyle.inheritedData.Read().wordSpacing, newStyle.inheritedData.Read().wordSpacing, durationMs, delayMs, easingCurve);
		default:
			return false;
		}
	}

	public static bool StartAnimationAllProperty(VisualElement element, ref ComputedStyle oldStyle, ref ComputedStyle newStyle, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		bool flag = false;
		UsageHints usageHints = UsageHints.None;
		if (!oldStyle.inheritedData.Equals(newStyle.inheritedData))
		{
			ref readonly InheritedData reference = ref oldStyle.inheritedData.Read();
			ref readonly InheritedData reference2 = ref newStyle.inheritedData.Read();
			if (reference.color != reference2.color)
			{
				bool flag2 = element.styleAnimation.Start(StylePropertyId.Color, reference.color, reference2.color, durationMs, delayMs, easingCurve);
				if (flag2)
				{
					usageHints |= UsageHints.DynamicColor;
				}
				flag = flag || flag2;
			}
			if (reference.fontSize != reference2.fontSize)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.FontSize, reference.fontSize, reference2.fontSize, durationMs, delayMs, easingCurve);
			}
			if (reference.letterSpacing != reference2.letterSpacing)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.LetterSpacing, reference.letterSpacing, reference2.letterSpacing, durationMs, delayMs, easingCurve);
			}
			if (reference.textShadow != reference2.textShadow)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.TextShadow, reference.textShadow, reference2.textShadow, durationMs, delayMs, easingCurve);
			}
			if (reference.unityFont != reference2.unityFont)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.UnityFont, reference.unityFont, reference2.unityFont, durationMs, delayMs, easingCurve);
			}
			if (reference.unityFontDefinition != reference2.unityFontDefinition)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.UnityFontDefinition, reference.unityFontDefinition, reference2.unityFontDefinition, durationMs, delayMs, easingCurve);
			}
			if (reference.unityFontStyleAndWeight != reference2.unityFontStyleAndWeight)
			{
				flag |= element.styleAnimation.StartEnum(StylePropertyId.UnityFontStyleAndWeight, (int)reference.unityFontStyleAndWeight, (int)reference2.unityFontStyleAndWeight, durationMs, delayMs, easingCurve);
			}
			if (reference.unityParagraphSpacing != reference2.unityParagraphSpacing)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.UnityParagraphSpacing, reference.unityParagraphSpacing, reference2.unityParagraphSpacing, durationMs, delayMs, easingCurve);
			}
			if (reference.unityTextAlign != reference2.unityTextAlign)
			{
				flag |= element.styleAnimation.StartEnum(StylePropertyId.UnityTextAlign, (int)reference.unityTextAlign, (int)reference2.unityTextAlign, durationMs, delayMs, easingCurve);
			}
			if (reference.unityTextOutlineColor != reference2.unityTextOutlineColor)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.UnityTextOutlineColor, reference.unityTextOutlineColor, reference2.unityTextOutlineColor, durationMs, delayMs, easingCurve);
			}
			if (reference.unityTextOutlineWidth != reference2.unityTextOutlineWidth)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.UnityTextOutlineWidth, reference.unityTextOutlineWidth, reference2.unityTextOutlineWidth, durationMs, delayMs, easingCurve);
			}
			if (reference.visibility != reference2.visibility)
			{
				flag |= element.styleAnimation.StartEnum(StylePropertyId.Visibility, (int)reference.visibility, (int)reference2.visibility, durationMs, delayMs, easingCurve);
			}
			if (reference.whiteSpace != reference2.whiteSpace)
			{
				flag |= element.styleAnimation.StartEnum(StylePropertyId.WhiteSpace, (int)reference.whiteSpace, (int)reference2.whiteSpace, durationMs, delayMs, easingCurve);
			}
			if (reference.wordSpacing != reference2.wordSpacing)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.WordSpacing, reference.wordSpacing, reference2.wordSpacing, durationMs, delayMs, easingCurve);
			}
		}
		if (!oldStyle.layoutData.Equals(newStyle.layoutData))
		{
			ref readonly LayoutData reference3 = ref oldStyle.layoutData.Read();
			ref readonly LayoutData reference4 = ref newStyle.layoutData.Read();
			if (reference3.alignContent != reference4.alignContent)
			{
				flag |= element.styleAnimation.StartEnum(StylePropertyId.AlignContent, (int)reference3.alignContent, (int)reference4.alignContent, durationMs, delayMs, easingCurve);
			}
			if (reference3.alignItems != reference4.alignItems)
			{
				flag |= element.styleAnimation.StartEnum(StylePropertyId.AlignItems, (int)reference3.alignItems, (int)reference4.alignItems, durationMs, delayMs, easingCurve);
			}
			if (reference3.alignSelf != reference4.alignSelf)
			{
				flag |= element.styleAnimation.StartEnum(StylePropertyId.AlignSelf, (int)reference3.alignSelf, (int)reference4.alignSelf, durationMs, delayMs, easingCurve);
			}
			if (reference3.borderBottomWidth != reference4.borderBottomWidth)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.BorderBottomWidth, reference3.borderBottomWidth, reference4.borderBottomWidth, durationMs, delayMs, easingCurve);
			}
			if (reference3.borderLeftWidth != reference4.borderLeftWidth)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.BorderLeftWidth, reference3.borderLeftWidth, reference4.borderLeftWidth, durationMs, delayMs, easingCurve);
			}
			if (reference3.borderRightWidth != reference4.borderRightWidth)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.BorderRightWidth, reference3.borderRightWidth, reference4.borderRightWidth, durationMs, delayMs, easingCurve);
			}
			if (reference3.borderTopWidth != reference4.borderTopWidth)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.BorderTopWidth, reference3.borderTopWidth, reference4.borderTopWidth, durationMs, delayMs, easingCurve);
			}
			if (reference3.bottom != reference4.bottom)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.Bottom, reference3.bottom, reference4.bottom, durationMs, delayMs, easingCurve);
			}
			if (reference3.display != reference4.display)
			{
				flag |= element.styleAnimation.StartEnum(StylePropertyId.Display, (int)reference3.display, (int)reference4.display, durationMs, delayMs, easingCurve);
			}
			if (reference3.flexBasis != reference4.flexBasis)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.FlexBasis, reference3.flexBasis, reference4.flexBasis, durationMs, delayMs, easingCurve);
			}
			if (reference3.flexDirection != reference4.flexDirection)
			{
				flag |= element.styleAnimation.StartEnum(StylePropertyId.FlexDirection, (int)reference3.flexDirection, (int)reference4.flexDirection, durationMs, delayMs, easingCurve);
			}
			if (reference3.flexGrow != reference4.flexGrow)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.FlexGrow, reference3.flexGrow, reference4.flexGrow, durationMs, delayMs, easingCurve);
			}
			if (reference3.flexShrink != reference4.flexShrink)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.FlexShrink, reference3.flexShrink, reference4.flexShrink, durationMs, delayMs, easingCurve);
			}
			if (reference3.flexWrap != reference4.flexWrap)
			{
				flag |= element.styleAnimation.StartEnum(StylePropertyId.FlexWrap, (int)reference3.flexWrap, (int)reference4.flexWrap, durationMs, delayMs, easingCurve);
			}
			if (reference3.height != reference4.height)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.Height, reference3.height, reference4.height, durationMs, delayMs, easingCurve);
			}
			if (reference3.justifyContent != reference4.justifyContent)
			{
				flag |= element.styleAnimation.StartEnum(StylePropertyId.JustifyContent, (int)reference3.justifyContent, (int)reference4.justifyContent, durationMs, delayMs, easingCurve);
			}
			if (reference3.left != reference4.left)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.Left, reference3.left, reference4.left, durationMs, delayMs, easingCurve);
			}
			if (reference3.marginBottom != reference4.marginBottom)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.MarginBottom, reference3.marginBottom, reference4.marginBottom, durationMs, delayMs, easingCurve);
			}
			if (reference3.marginLeft != reference4.marginLeft)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.MarginLeft, reference3.marginLeft, reference4.marginLeft, durationMs, delayMs, easingCurve);
			}
			if (reference3.marginRight != reference4.marginRight)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.MarginRight, reference3.marginRight, reference4.marginRight, durationMs, delayMs, easingCurve);
			}
			if (reference3.marginTop != reference4.marginTop)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.MarginTop, reference3.marginTop, reference4.marginTop, durationMs, delayMs, easingCurve);
			}
			if (reference3.maxHeight != reference4.maxHeight)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.MaxHeight, reference3.maxHeight, reference4.maxHeight, durationMs, delayMs, easingCurve);
			}
			if (reference3.maxWidth != reference4.maxWidth)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.MaxWidth, reference3.maxWidth, reference4.maxWidth, durationMs, delayMs, easingCurve);
			}
			if (reference3.minHeight != reference4.minHeight)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.MinHeight, reference3.minHeight, reference4.minHeight, durationMs, delayMs, easingCurve);
			}
			if (reference3.minWidth != reference4.minWidth)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.MinWidth, reference3.minWidth, reference4.minWidth, durationMs, delayMs, easingCurve);
			}
			if (reference3.paddingBottom != reference4.paddingBottom)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.PaddingBottom, reference3.paddingBottom, reference4.paddingBottom, durationMs, delayMs, easingCurve);
			}
			if (reference3.paddingLeft != reference4.paddingLeft)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.PaddingLeft, reference3.paddingLeft, reference4.paddingLeft, durationMs, delayMs, easingCurve);
			}
			if (reference3.paddingRight != reference4.paddingRight)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.PaddingRight, reference3.paddingRight, reference4.paddingRight, durationMs, delayMs, easingCurve);
			}
			if (reference3.paddingTop != reference4.paddingTop)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.PaddingTop, reference3.paddingTop, reference4.paddingTop, durationMs, delayMs, easingCurve);
			}
			if (reference3.position != reference4.position)
			{
				flag |= element.styleAnimation.StartEnum(StylePropertyId.Position, (int)reference3.position, (int)reference4.position, durationMs, delayMs, easingCurve);
			}
			if (reference3.right != reference4.right)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.Right, reference3.right, reference4.right, durationMs, delayMs, easingCurve);
			}
			if (reference3.top != reference4.top)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.Top, reference3.top, reference4.top, durationMs, delayMs, easingCurve);
			}
			if (reference3.width != reference4.width)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.Width, reference3.width, reference4.width, durationMs, delayMs, easingCurve);
			}
		}
		if (!oldStyle.rareData.Equals(newStyle.rareData))
		{
			ref readonly RareData reference5 = ref oldStyle.rareData.Read();
			ref readonly RareData reference6 = ref newStyle.rareData.Read();
			if (reference5.textOverflow != reference6.textOverflow)
			{
				flag |= element.styleAnimation.StartEnum(StylePropertyId.TextOverflow, (int)reference5.textOverflow, (int)reference6.textOverflow, durationMs, delayMs, easingCurve);
			}
			if (reference5.unityBackgroundImageTintColor != reference6.unityBackgroundImageTintColor)
			{
				bool flag3 = element.styleAnimation.Start(StylePropertyId.UnityBackgroundImageTintColor, reference5.unityBackgroundImageTintColor, reference6.unityBackgroundImageTintColor, durationMs, delayMs, easingCurve);
				if (flag3)
				{
					usageHints |= UsageHints.DynamicColor;
				}
				flag = flag || flag3;
			}
			if (reference5.unityOverflowClipBox != reference6.unityOverflowClipBox)
			{
				flag |= element.styleAnimation.StartEnum(StylePropertyId.UnityOverflowClipBox, (int)reference5.unityOverflowClipBox, (int)reference6.unityOverflowClipBox, durationMs, delayMs, easingCurve);
			}
			if (reference5.unitySliceBottom != reference6.unitySliceBottom)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.UnitySliceBottom, reference5.unitySliceBottom, reference6.unitySliceBottom, durationMs, delayMs, easingCurve);
			}
			if (reference5.unitySliceLeft != reference6.unitySliceLeft)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.UnitySliceLeft, reference5.unitySliceLeft, reference6.unitySliceLeft, durationMs, delayMs, easingCurve);
			}
			if (reference5.unitySliceRight != reference6.unitySliceRight)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.UnitySliceRight, reference5.unitySliceRight, reference6.unitySliceRight, durationMs, delayMs, easingCurve);
			}
			if (reference5.unitySliceScale != reference6.unitySliceScale)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.UnitySliceScale, reference5.unitySliceScale, reference6.unitySliceScale, durationMs, delayMs, easingCurve);
			}
			if (reference5.unitySliceTop != reference6.unitySliceTop)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.UnitySliceTop, reference5.unitySliceTop, reference6.unitySliceTop, durationMs, delayMs, easingCurve);
			}
			if (reference5.unityTextOverflowPosition != reference6.unityTextOverflowPosition)
			{
				flag |= element.styleAnimation.StartEnum(StylePropertyId.UnityTextOverflowPosition, (int)reference5.unityTextOverflowPosition, (int)reference6.unityTextOverflowPosition, durationMs, delayMs, easingCurve);
			}
		}
		if (!oldStyle.transformData.Equals(newStyle.transformData))
		{
			ref readonly TransformData reference7 = ref oldStyle.transformData.Read();
			ref readonly TransformData reference8 = ref newStyle.transformData.Read();
			if (reference7.rotate != reference8.rotate)
			{
				bool flag4 = element.styleAnimation.Start(StylePropertyId.Rotate, reference7.rotate, reference8.rotate, durationMs, delayMs, easingCurve);
				if (flag4)
				{
					usageHints |= UsageHints.DynamicTransform;
				}
				flag = flag || flag4;
			}
			if (reference7.scale != reference8.scale)
			{
				bool flag5 = element.styleAnimation.Start(StylePropertyId.Scale, reference7.scale, reference8.scale, durationMs, delayMs, easingCurve);
				if (flag5)
				{
					usageHints |= UsageHints.DynamicTransform;
				}
				flag = flag || flag5;
			}
			if (reference7.transformOrigin != reference8.transformOrigin)
			{
				bool flag6 = element.styleAnimation.Start(StylePropertyId.TransformOrigin, reference7.transformOrigin, reference8.transformOrigin, durationMs, delayMs, easingCurve);
				if (flag6)
				{
					usageHints |= UsageHints.DynamicTransform;
				}
				flag = flag || flag6;
			}
			if (reference7.translate != reference8.translate)
			{
				bool flag7 = element.styleAnimation.Start(StylePropertyId.Translate, reference7.translate, reference8.translate, durationMs, delayMs, easingCurve);
				if (flag7)
				{
					usageHints |= UsageHints.DynamicTransform;
				}
				flag = flag || flag7;
			}
		}
		if (!oldStyle.visualData.Equals(newStyle.visualData))
		{
			ref readonly VisualData reference9 = ref oldStyle.visualData.Read();
			ref readonly VisualData reference10 = ref newStyle.visualData.Read();
			if (reference9.backgroundColor != reference10.backgroundColor)
			{
				bool flag8 = element.styleAnimation.Start(StylePropertyId.BackgroundColor, reference9.backgroundColor, reference10.backgroundColor, durationMs, delayMs, easingCurve);
				if (flag8)
				{
					usageHints |= UsageHints.DynamicColor;
				}
				flag = flag || flag8;
			}
			if (reference9.backgroundImage != reference10.backgroundImage)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.BackgroundImage, reference9.backgroundImage, reference10.backgroundImage, durationMs, delayMs, easingCurve);
			}
			if (reference9.backgroundPositionX != reference10.backgroundPositionX)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.BackgroundPositionX, reference9.backgroundPositionX, reference10.backgroundPositionX, durationMs, delayMs, easingCurve);
			}
			if (reference9.backgroundPositionY != reference10.backgroundPositionY)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.BackgroundPositionY, reference9.backgroundPositionY, reference10.backgroundPositionY, durationMs, delayMs, easingCurve);
			}
			if (reference9.backgroundRepeat != reference10.backgroundRepeat)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.BackgroundRepeat, reference9.backgroundRepeat, reference10.backgroundRepeat, durationMs, delayMs, easingCurve);
			}
			if (reference9.backgroundSize != reference10.backgroundSize)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.BackgroundSize, reference9.backgroundSize, reference10.backgroundSize, durationMs, delayMs, easingCurve);
			}
			if (reference9.borderBottomColor != reference10.borderBottomColor)
			{
				bool flag9 = element.styleAnimation.Start(StylePropertyId.BorderBottomColor, reference9.borderBottomColor, reference10.borderBottomColor, durationMs, delayMs, easingCurve);
				if (flag9)
				{
					usageHints |= UsageHints.DynamicColor;
				}
				flag = flag || flag9;
			}
			if (reference9.borderBottomLeftRadius != reference10.borderBottomLeftRadius)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.BorderBottomLeftRadius, reference9.borderBottomLeftRadius, reference10.borderBottomLeftRadius, durationMs, delayMs, easingCurve);
			}
			if (reference9.borderBottomRightRadius != reference10.borderBottomRightRadius)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.BorderBottomRightRadius, reference9.borderBottomRightRadius, reference10.borderBottomRightRadius, durationMs, delayMs, easingCurve);
			}
			if (reference9.borderLeftColor != reference10.borderLeftColor)
			{
				bool flag10 = element.styleAnimation.Start(StylePropertyId.BorderLeftColor, reference9.borderLeftColor, reference10.borderLeftColor, durationMs, delayMs, easingCurve);
				if (flag10)
				{
					usageHints |= UsageHints.DynamicColor;
				}
				flag = flag || flag10;
			}
			if (reference9.borderRightColor != reference10.borderRightColor)
			{
				bool flag11 = element.styleAnimation.Start(StylePropertyId.BorderRightColor, reference9.borderRightColor, reference10.borderRightColor, durationMs, delayMs, easingCurve);
				if (flag11)
				{
					usageHints |= UsageHints.DynamicColor;
				}
				flag = flag || flag11;
			}
			if (reference9.borderTopColor != reference10.borderTopColor)
			{
				bool flag12 = element.styleAnimation.Start(StylePropertyId.BorderTopColor, reference9.borderTopColor, reference10.borderTopColor, durationMs, delayMs, easingCurve);
				if (flag12)
				{
					usageHints |= UsageHints.DynamicColor;
				}
				flag = flag || flag12;
			}
			if (reference9.borderTopLeftRadius != reference10.borderTopLeftRadius)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.BorderTopLeftRadius, reference9.borderTopLeftRadius, reference10.borderTopLeftRadius, durationMs, delayMs, easingCurve);
			}
			if (reference9.borderTopRightRadius != reference10.borderTopRightRadius)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.BorderTopRightRadius, reference9.borderTopRightRadius, reference10.borderTopRightRadius, durationMs, delayMs, easingCurve);
			}
			if (reference9.opacity != reference10.opacity)
			{
				flag |= element.styleAnimation.Start(StylePropertyId.Opacity, reference9.opacity, reference10.opacity, durationMs, delayMs, easingCurve);
			}
			if (reference9.overflow != reference10.overflow)
			{
				flag |= element.styleAnimation.StartEnum(StylePropertyId.Overflow, (int)reference9.overflow, (int)reference10.overflow, durationMs, delayMs, easingCurve);
			}
		}
		if (usageHints != UsageHints.None)
		{
			element.usageHints |= usageHints;
		}
		return flag;
	}

	public static bool StartAnimationInline(VisualElement element, StylePropertyId id, ref ComputedStyle computedStyle, StyleValue sv, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		switch (id)
		{
		case StylePropertyId.AlignContent:
		{
			Align to31 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.alignContent : ((Align)sv.number));
			if (sv.keyword == StyleKeyword.Auto)
			{
				to31 = Align.Auto;
			}
			return element.styleAnimation.StartEnum(StylePropertyId.AlignContent, (int)computedStyle.layoutData.Read().alignContent, (int)to31, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.AlignItems:
		{
			Align to60 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.alignItems : ((Align)sv.number));
			if (sv.keyword == StyleKeyword.Auto)
			{
				to60 = Align.Auto;
			}
			return element.styleAnimation.StartEnum(StylePropertyId.AlignItems, (int)computedStyle.layoutData.Read().alignItems, (int)to60, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.AlignSelf:
		{
			Align to70 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.alignSelf : ((Align)sv.number));
			if (sv.keyword == StyleKeyword.Auto)
			{
				to70 = Align.Auto;
			}
			return element.styleAnimation.StartEnum(StylePropertyId.AlignSelf, (int)computedStyle.layoutData.Read().alignSelf, (int)to70, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.BackgroundColor:
		{
			Color to54 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.backgroundColor : sv.color);
			bool flag6 = element.styleAnimation.Start(StylePropertyId.BackgroundColor, computedStyle.visualData.Read().backgroundColor, to54, durationMs, delayMs, easingCurve);
			if (flag6 && (element.usageHints & UsageHints.DynamicColor) == 0)
			{
				element.usageHints |= UsageHints.DynamicColor;
			}
			return flag6;
		}
		case StylePropertyId.BackgroundImage:
		{
			Background to68 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.backgroundImage : (sv.resource.IsAllocated ? Background.FromObject(sv.resource.Target) : default(Background)));
			return element.styleAnimation.Start(StylePropertyId.BackgroundImage, computedStyle.visualData.Read().backgroundImage, to68, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.BackgroundPositionX:
		{
			BackgroundPosition to24 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.backgroundPositionX : sv.position);
			return element.styleAnimation.Start(StylePropertyId.BackgroundPositionX, computedStyle.visualData.Read().backgroundPositionX, to24, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.BackgroundPositionY:
		{
			BackgroundPosition to10 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.backgroundPositionY : sv.position);
			return element.styleAnimation.Start(StylePropertyId.BackgroundPositionY, computedStyle.visualData.Read().backgroundPositionY, to10, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.BackgroundRepeat:
		{
			BackgroundRepeat to16 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.backgroundRepeat : sv.repeat);
			return element.styleAnimation.Start(StylePropertyId.BackgroundRepeat, computedStyle.visualData.Read().backgroundRepeat, to16, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.BorderBottomColor:
		{
			Color to46 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.borderBottomColor : sv.color);
			bool flag5 = element.styleAnimation.Start(StylePropertyId.BorderBottomColor, computedStyle.visualData.Read().borderBottomColor, to46, durationMs, delayMs, easingCurve);
			if (flag5 && (element.usageHints & UsageHints.DynamicColor) == 0)
			{
				element.usageHints |= UsageHints.DynamicColor;
			}
			return flag5;
		}
		case StylePropertyId.BorderBottomLeftRadius:
		{
			Length to6 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.borderBottomLeftRadius : sv.length);
			return element.styleAnimation.Start(StylePropertyId.BorderBottomLeftRadius, computedStyle.visualData.Read().borderBottomLeftRadius, to6, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.BorderBottomRightRadius:
		{
			Length to35 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.borderBottomRightRadius : sv.length);
			return element.styleAnimation.Start(StylePropertyId.BorderBottomRightRadius, computedStyle.visualData.Read().borderBottomRightRadius, to35, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.BorderBottomWidth:
		{
			float to36 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.borderBottomWidth : sv.number);
			return element.styleAnimation.Start(StylePropertyId.BorderBottomWidth, computedStyle.layoutData.Read().borderBottomWidth, to36, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.BorderLeftColor:
		{
			Color to56 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.borderLeftColor : sv.color);
			bool flag7 = element.styleAnimation.Start(StylePropertyId.BorderLeftColor, computedStyle.visualData.Read().borderLeftColor, to56, durationMs, delayMs, easingCurve);
			if (flag7 && (element.usageHints & UsageHints.DynamicColor) == 0)
			{
				element.usageHints |= UsageHints.DynamicColor;
			}
			return flag7;
		}
		case StylePropertyId.BorderLeftWidth:
		{
			float to30 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.borderLeftWidth : sv.number);
			return element.styleAnimation.Start(StylePropertyId.BorderLeftWidth, computedStyle.layoutData.Read().borderLeftWidth, to30, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.BorderRightColor:
		{
			Color to2 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.borderRightColor : sv.color);
			bool flag = element.styleAnimation.Start(StylePropertyId.BorderRightColor, computedStyle.visualData.Read().borderRightColor, to2, durationMs, delayMs, easingCurve);
			if (flag && (element.usageHints & UsageHints.DynamicColor) == 0)
			{
				element.usageHints |= UsageHints.DynamicColor;
			}
			return flag;
		}
		case StylePropertyId.BorderRightWidth:
		{
			float to52 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.borderRightWidth : sv.number);
			return element.styleAnimation.Start(StylePropertyId.BorderRightWidth, computedStyle.layoutData.Read().borderRightWidth, to52, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.BorderTopColor:
		{
			Color to28 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.borderTopColor : sv.color);
			bool flag4 = element.styleAnimation.Start(StylePropertyId.BorderTopColor, computedStyle.visualData.Read().borderTopColor, to28, durationMs, delayMs, easingCurve);
			if (flag4 && (element.usageHints & UsageHints.DynamicColor) == 0)
			{
				element.usageHints |= UsageHints.DynamicColor;
			}
			return flag4;
		}
		case StylePropertyId.BorderTopLeftRadius:
		{
			Length to22 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.borderTopLeftRadius : sv.length);
			return element.styleAnimation.Start(StylePropertyId.BorderTopLeftRadius, computedStyle.visualData.Read().borderTopLeftRadius, to22, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.BorderTopRightRadius:
		{
			Length to63 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.borderTopRightRadius : sv.length);
			return element.styleAnimation.Start(StylePropertyId.BorderTopRightRadius, computedStyle.visualData.Read().borderTopRightRadius, to63, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.BorderTopWidth:
		{
			float to58 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.borderTopWidth : sv.number);
			return element.styleAnimation.Start(StylePropertyId.BorderTopWidth, computedStyle.layoutData.Read().borderTopWidth, to58, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.Bottom:
		{
			Length to40 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.bottom : sv.length);
			return element.styleAnimation.Start(StylePropertyId.Bottom, computedStyle.layoutData.Read().bottom, to40, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.Color:
		{
			Color to15 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.color : sv.color);
			bool flag3 = element.styleAnimation.Start(StylePropertyId.Color, computedStyle.inheritedData.Read().color, to15, durationMs, delayMs, easingCurve);
			if (flag3 && (element.usageHints & UsageHints.DynamicColor) == 0)
			{
				element.usageHints |= UsageHints.DynamicColor;
			}
			return flag3;
		}
		case StylePropertyId.Display:
		{
			DisplayStyle to3 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.display : ((DisplayStyle)sv.number));
			if (sv.keyword == StyleKeyword.None)
			{
				to3 = DisplayStyle.None;
			}
			return element.styleAnimation.StartEnum(StylePropertyId.Display, (int)computedStyle.layoutData.Read().display, (int)to3, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.FlexBasis:
		{
			Length to69 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.flexBasis : sv.length);
			return element.styleAnimation.Start(StylePropertyId.FlexBasis, computedStyle.layoutData.Read().flexBasis, to69, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.FlexDirection:
		{
			FlexDirection to51 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.flexDirection : ((FlexDirection)sv.number));
			return element.styleAnimation.StartEnum(StylePropertyId.FlexDirection, (int)computedStyle.layoutData.Read().flexDirection, (int)to51, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.FlexGrow:
		{
			float to42 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.flexGrow : sv.number);
			return element.styleAnimation.Start(StylePropertyId.FlexGrow, computedStyle.layoutData.Read().flexGrow, to42, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.FlexShrink:
		{
			float to43 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.flexShrink : sv.number);
			return element.styleAnimation.Start(StylePropertyId.FlexShrink, computedStyle.layoutData.Read().flexShrink, to43, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.FlexWrap:
		{
			Wrap to27 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.flexWrap : ((Wrap)sv.number));
			return element.styleAnimation.StartEnum(StylePropertyId.FlexWrap, (int)computedStyle.layoutData.Read().flexWrap, (int)to27, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.FontSize:
		{
			Length to14 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.fontSize : sv.length);
			return element.styleAnimation.Start(StylePropertyId.FontSize, computedStyle.inheritedData.Read().fontSize, to14, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.Height:
		{
			Length to11 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.height : sv.length);
			return element.styleAnimation.Start(StylePropertyId.Height, computedStyle.layoutData.Read().height, to11, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.JustifyContent:
		{
			Justify to67 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.justifyContent : ((Justify)sv.number));
			return element.styleAnimation.StartEnum(StylePropertyId.JustifyContent, (int)computedStyle.layoutData.Read().justifyContent, (int)to67, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.Left:
		{
			Length to62 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.left : sv.length);
			return element.styleAnimation.Start(StylePropertyId.Left, computedStyle.layoutData.Read().left, to62, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.LetterSpacing:
		{
			Length to59 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.letterSpacing : sv.length);
			return element.styleAnimation.Start(StylePropertyId.LetterSpacing, computedStyle.inheritedData.Read().letterSpacing, to59, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.MarginBottom:
		{
			Length to47 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.marginBottom : sv.length);
			return element.styleAnimation.Start(StylePropertyId.MarginBottom, computedStyle.layoutData.Read().marginBottom, to47, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.MarginLeft:
		{
			Length to44 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.marginLeft : sv.length);
			return element.styleAnimation.Start(StylePropertyId.MarginLeft, computedStyle.layoutData.Read().marginLeft, to44, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.MarginRight:
		{
			Length to38 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.marginRight : sv.length);
			return element.styleAnimation.Start(StylePropertyId.MarginRight, computedStyle.layoutData.Read().marginRight, to38, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.MarginTop:
		{
			Length to26 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.marginTop : sv.length);
			return element.styleAnimation.Start(StylePropertyId.MarginTop, computedStyle.layoutData.Read().marginTop, to26, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.MaxHeight:
		{
			Length to19 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.maxHeight : sv.length);
			return element.styleAnimation.Start(StylePropertyId.MaxHeight, computedStyle.layoutData.Read().maxHeight, to19, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.MaxWidth:
		{
			Length to20 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.maxWidth : sv.length);
			return element.styleAnimation.Start(StylePropertyId.MaxWidth, computedStyle.layoutData.Read().maxWidth, to20, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.MinHeight:
		{
			Length to8 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.minHeight : sv.length);
			return element.styleAnimation.Start(StylePropertyId.MinHeight, computedStyle.layoutData.Read().minHeight, to8, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.MinWidth:
		{
			Length to7 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.minWidth : sv.length);
			return element.styleAnimation.Start(StylePropertyId.MinWidth, computedStyle.layoutData.Read().minWidth, to7, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.Opacity:
		{
			float to66 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.opacity : sv.number);
			return element.styleAnimation.Start(StylePropertyId.Opacity, computedStyle.visualData.Read().opacity, to66, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.Overflow:
		{
			OverflowInternal to64 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.overflow : ((OverflowInternal)sv.number));
			return element.styleAnimation.StartEnum(StylePropertyId.Overflow, (int)computedStyle.visualData.Read().overflow, (int)to64, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.PaddingBottom:
		{
			Length to55 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.paddingBottom : sv.length);
			return element.styleAnimation.Start(StylePropertyId.PaddingBottom, computedStyle.layoutData.Read().paddingBottom, to55, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.PaddingLeft:
		{
			Length to50 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.paddingLeft : sv.length);
			return element.styleAnimation.Start(StylePropertyId.PaddingLeft, computedStyle.layoutData.Read().paddingLeft, to50, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.PaddingRight:
		{
			Length to48 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.paddingRight : sv.length);
			return element.styleAnimation.Start(StylePropertyId.PaddingRight, computedStyle.layoutData.Read().paddingRight, to48, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.PaddingTop:
		{
			Length to39 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.paddingTop : sv.length);
			return element.styleAnimation.Start(StylePropertyId.PaddingTop, computedStyle.layoutData.Read().paddingTop, to39, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.Position:
		{
			Position to34 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.position : ((Position)sv.number));
			return element.styleAnimation.StartEnum(StylePropertyId.Position, (int)computedStyle.layoutData.Read().position, (int)to34, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.Right:
		{
			Length to32 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.right : sv.length);
			return element.styleAnimation.Start(StylePropertyId.Right, computedStyle.layoutData.Read().right, to32, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.TextOverflow:
		{
			TextOverflow to23 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.textOverflow : ((TextOverflow)sv.number));
			return element.styleAnimation.StartEnum(StylePropertyId.TextOverflow, (int)computedStyle.rareData.Read().textOverflow, (int)to23, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.Top:
		{
			Length to18 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.top : sv.length);
			return element.styleAnimation.Start(StylePropertyId.Top, computedStyle.layoutData.Read().top, to18, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.UnityBackgroundImageTintColor:
		{
			Color to12 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.unityBackgroundImageTintColor : sv.color);
			bool flag2 = element.styleAnimation.Start(StylePropertyId.UnityBackgroundImageTintColor, computedStyle.rareData.Read().unityBackgroundImageTintColor, to12, durationMs, delayMs, easingCurve);
			if (flag2 && (element.usageHints & UsageHints.DynamicColor) == 0)
			{
				element.usageHints |= UsageHints.DynamicColor;
			}
			return flag2;
		}
		case StylePropertyId.UnityFont:
		{
			Font to4 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.unityFont : (sv.resource.IsAllocated ? (sv.resource.Target as Font) : null));
			return element.styleAnimation.Start(StylePropertyId.UnityFont, computedStyle.inheritedData.Read().unityFont, to4, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.UnityFontDefinition:
		{
			FontDefinition to65 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.unityFontDefinition : (sv.resource.IsAllocated ? FontDefinition.FromObject(sv.resource.Target) : default(FontDefinition)));
			return element.styleAnimation.Start(StylePropertyId.UnityFontDefinition, computedStyle.inheritedData.Read().unityFontDefinition, to65, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.UnityFontStyleAndWeight:
		{
			FontStyle to61 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.unityFontStyleAndWeight : ((FontStyle)sv.number));
			return element.styleAnimation.StartEnum(StylePropertyId.UnityFontStyleAndWeight, (int)computedStyle.inheritedData.Read().unityFontStyleAndWeight, (int)to61, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.UnityOverflowClipBox:
		{
			OverflowClipBox to57 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.unityOverflowClipBox : ((OverflowClipBox)sv.number));
			return element.styleAnimation.StartEnum(StylePropertyId.UnityOverflowClipBox, (int)computedStyle.rareData.Read().unityOverflowClipBox, (int)to57, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.UnityParagraphSpacing:
		{
			Length to53 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.unityParagraphSpacing : sv.length);
			return element.styleAnimation.Start(StylePropertyId.UnityParagraphSpacing, computedStyle.inheritedData.Read().unityParagraphSpacing, to53, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.UnitySliceBottom:
		{
			int to49 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.unitySliceBottom : ((int)sv.number));
			return element.styleAnimation.Start(StylePropertyId.UnitySliceBottom, computedStyle.rareData.Read().unitySliceBottom, to49, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.UnitySliceLeft:
		{
			int to45 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.unitySliceLeft : ((int)sv.number));
			return element.styleAnimation.Start(StylePropertyId.UnitySliceLeft, computedStyle.rareData.Read().unitySliceLeft, to45, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.UnitySliceRight:
		{
			int to41 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.unitySliceRight : ((int)sv.number));
			return element.styleAnimation.Start(StylePropertyId.UnitySliceRight, computedStyle.rareData.Read().unitySliceRight, to41, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.UnitySliceScale:
		{
			float to37 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.unitySliceScale : sv.number);
			return element.styleAnimation.Start(StylePropertyId.UnitySliceScale, computedStyle.rareData.Read().unitySliceScale, to37, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.UnitySliceTop:
		{
			int to33 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.unitySliceTop : ((int)sv.number));
			return element.styleAnimation.Start(StylePropertyId.UnitySliceTop, computedStyle.rareData.Read().unitySliceTop, to33, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.UnityTextAlign:
		{
			TextAnchor to29 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.unityTextAlign : ((TextAnchor)sv.number));
			return element.styleAnimation.StartEnum(StylePropertyId.UnityTextAlign, (int)computedStyle.inheritedData.Read().unityTextAlign, (int)to29, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.UnityTextOutlineColor:
		{
			Color to25 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.unityTextOutlineColor : sv.color);
			return element.styleAnimation.Start(StylePropertyId.UnityTextOutlineColor, computedStyle.inheritedData.Read().unityTextOutlineColor, to25, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.UnityTextOutlineWidth:
		{
			float to21 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.unityTextOutlineWidth : sv.number);
			return element.styleAnimation.Start(StylePropertyId.UnityTextOutlineWidth, computedStyle.inheritedData.Read().unityTextOutlineWidth, to21, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.UnityTextOverflowPosition:
		{
			TextOverflowPosition to17 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.unityTextOverflowPosition : ((TextOverflowPosition)sv.number));
			return element.styleAnimation.StartEnum(StylePropertyId.UnityTextOverflowPosition, (int)computedStyle.rareData.Read().unityTextOverflowPosition, (int)to17, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.Visibility:
		{
			Visibility to13 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.visibility : ((Visibility)sv.number));
			return element.styleAnimation.StartEnum(StylePropertyId.Visibility, (int)computedStyle.inheritedData.Read().visibility, (int)to13, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.WhiteSpace:
		{
			WhiteSpace to9 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.whiteSpace : ((WhiteSpace)sv.number));
			return element.styleAnimation.StartEnum(StylePropertyId.WhiteSpace, (int)computedStyle.inheritedData.Read().whiteSpace, (int)to9, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.Width:
		{
			Length to5 = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.width : sv.length);
			return element.styleAnimation.Start(StylePropertyId.Width, computedStyle.layoutData.Read().width, to5, durationMs, delayMs, easingCurve);
		}
		case StylePropertyId.WordSpacing:
		{
			Length to = ((sv.keyword == StyleKeyword.Initial) ? InitialStyle.wordSpacing : sv.length);
			return element.styleAnimation.Start(StylePropertyId.WordSpacing, computedStyle.inheritedData.Read().wordSpacing, to, durationMs, delayMs, easingCurve);
		}
		default:
			return false;
		}
	}

	public void ApplyStyleTransformOrigin(TransformOrigin st)
	{
		transformData.Write().transformOrigin = st;
	}

	public void ApplyStyleTranslate(Translate translateValue)
	{
		transformData.Write().translate = translateValue;
	}

	public void ApplyStyleRotate(Rotate rotateValue)
	{
		transformData.Write().rotate = rotateValue;
	}

	public void ApplyStyleScale(Scale scaleValue)
	{
		transformData.Write().scale = scaleValue;
	}

	public void ApplyStyleBackgroundSize(BackgroundSize backgroundSizeValue)
	{
		visualData.Write().backgroundSize = backgroundSizeValue;
	}

	public void ApplyInitialValue(StylePropertyReader reader)
	{
		switch (reader.propertyId)
		{
		case StylePropertyId.Custom:
			RemoveCustomStyleProperty(reader);
			break;
		case StylePropertyId.All:
			ApplyAllPropertyInitial();
			break;
		default:
			ApplyInitialValue(reader.propertyId);
			break;
		}
	}

	public void ApplyInitialValue(StylePropertyId id)
	{
		switch (id)
		{
		case StylePropertyId.AlignContent:
			layoutData.Write().alignContent = InitialStyle.alignContent;
			break;
		case StylePropertyId.AlignItems:
			layoutData.Write().alignItems = InitialStyle.alignItems;
			break;
		case StylePropertyId.AlignSelf:
			layoutData.Write().alignSelf = InitialStyle.alignSelf;
			break;
		case StylePropertyId.All:
			break;
		case StylePropertyId.BackgroundColor:
			visualData.Write().backgroundColor = InitialStyle.backgroundColor;
			break;
		case StylePropertyId.BackgroundImage:
			visualData.Write().backgroundImage = InitialStyle.backgroundImage;
			break;
		case StylePropertyId.BackgroundPosition:
			visualData.Write().backgroundPositionX = InitialStyle.backgroundPositionX;
			visualData.Write().backgroundPositionY = InitialStyle.backgroundPositionY;
			break;
		case StylePropertyId.BackgroundPositionX:
			visualData.Write().backgroundPositionX = InitialStyle.backgroundPositionX;
			break;
		case StylePropertyId.BackgroundPositionY:
			visualData.Write().backgroundPositionY = InitialStyle.backgroundPositionY;
			break;
		case StylePropertyId.BackgroundRepeat:
			visualData.Write().backgroundRepeat = InitialStyle.backgroundRepeat;
			break;
		case StylePropertyId.BackgroundSize:
			visualData.Write().backgroundSize = InitialStyle.backgroundSize;
			break;
		case StylePropertyId.BorderBottomColor:
			visualData.Write().borderBottomColor = InitialStyle.borderBottomColor;
			break;
		case StylePropertyId.BorderBottomLeftRadius:
			visualData.Write().borderBottomLeftRadius = InitialStyle.borderBottomLeftRadius;
			break;
		case StylePropertyId.BorderBottomRightRadius:
			visualData.Write().borderBottomRightRadius = InitialStyle.borderBottomRightRadius;
			break;
		case StylePropertyId.BorderBottomWidth:
			layoutData.Write().borderBottomWidth = InitialStyle.borderBottomWidth;
			break;
		case StylePropertyId.BorderColor:
			visualData.Write().borderTopColor = InitialStyle.borderTopColor;
			visualData.Write().borderRightColor = InitialStyle.borderRightColor;
			visualData.Write().borderBottomColor = InitialStyle.borderBottomColor;
			visualData.Write().borderLeftColor = InitialStyle.borderLeftColor;
			break;
		case StylePropertyId.BorderLeftColor:
			visualData.Write().borderLeftColor = InitialStyle.borderLeftColor;
			break;
		case StylePropertyId.BorderLeftWidth:
			layoutData.Write().borderLeftWidth = InitialStyle.borderLeftWidth;
			break;
		case StylePropertyId.BorderRadius:
			visualData.Write().borderTopLeftRadius = InitialStyle.borderTopLeftRadius;
			visualData.Write().borderTopRightRadius = InitialStyle.borderTopRightRadius;
			visualData.Write().borderBottomRightRadius = InitialStyle.borderBottomRightRadius;
			visualData.Write().borderBottomLeftRadius = InitialStyle.borderBottomLeftRadius;
			break;
		case StylePropertyId.BorderRightColor:
			visualData.Write().borderRightColor = InitialStyle.borderRightColor;
			break;
		case StylePropertyId.BorderRightWidth:
			layoutData.Write().borderRightWidth = InitialStyle.borderRightWidth;
			break;
		case StylePropertyId.BorderTopColor:
			visualData.Write().borderTopColor = InitialStyle.borderTopColor;
			break;
		case StylePropertyId.BorderTopLeftRadius:
			visualData.Write().borderTopLeftRadius = InitialStyle.borderTopLeftRadius;
			break;
		case StylePropertyId.BorderTopRightRadius:
			visualData.Write().borderTopRightRadius = InitialStyle.borderTopRightRadius;
			break;
		case StylePropertyId.BorderTopWidth:
			layoutData.Write().borderTopWidth = InitialStyle.borderTopWidth;
			break;
		case StylePropertyId.BorderWidth:
			layoutData.Write().borderTopWidth = InitialStyle.borderTopWidth;
			layoutData.Write().borderRightWidth = InitialStyle.borderRightWidth;
			layoutData.Write().borderBottomWidth = InitialStyle.borderBottomWidth;
			layoutData.Write().borderLeftWidth = InitialStyle.borderLeftWidth;
			break;
		case StylePropertyId.Bottom:
			layoutData.Write().bottom = InitialStyle.bottom;
			break;
		case StylePropertyId.Color:
			inheritedData.Write().color = InitialStyle.color;
			break;
		case StylePropertyId.Cursor:
			rareData.Write().cursor = InitialStyle.cursor;
			break;
		case StylePropertyId.Display:
			layoutData.Write().display = InitialStyle.display;
			break;
		case StylePropertyId.Flex:
			layoutData.Write().flexGrow = InitialStyle.flexGrow;
			layoutData.Write().flexShrink = InitialStyle.flexShrink;
			layoutData.Write().flexBasis = InitialStyle.flexBasis;
			break;
		case StylePropertyId.FlexBasis:
			layoutData.Write().flexBasis = InitialStyle.flexBasis;
			break;
		case StylePropertyId.FlexDirection:
			layoutData.Write().flexDirection = InitialStyle.flexDirection;
			break;
		case StylePropertyId.FlexGrow:
			layoutData.Write().flexGrow = InitialStyle.flexGrow;
			break;
		case StylePropertyId.FlexShrink:
			layoutData.Write().flexShrink = InitialStyle.flexShrink;
			break;
		case StylePropertyId.FlexWrap:
			layoutData.Write().flexWrap = InitialStyle.flexWrap;
			break;
		case StylePropertyId.FontSize:
			inheritedData.Write().fontSize = InitialStyle.fontSize;
			break;
		case StylePropertyId.Height:
			layoutData.Write().height = InitialStyle.height;
			break;
		case StylePropertyId.JustifyContent:
			layoutData.Write().justifyContent = InitialStyle.justifyContent;
			break;
		case StylePropertyId.Left:
			layoutData.Write().left = InitialStyle.left;
			break;
		case StylePropertyId.LetterSpacing:
			inheritedData.Write().letterSpacing = InitialStyle.letterSpacing;
			break;
		case StylePropertyId.Margin:
			layoutData.Write().marginTop = InitialStyle.marginTop;
			layoutData.Write().marginRight = InitialStyle.marginRight;
			layoutData.Write().marginBottom = InitialStyle.marginBottom;
			layoutData.Write().marginLeft = InitialStyle.marginLeft;
			break;
		case StylePropertyId.MarginBottom:
			layoutData.Write().marginBottom = InitialStyle.marginBottom;
			break;
		case StylePropertyId.MarginLeft:
			layoutData.Write().marginLeft = InitialStyle.marginLeft;
			break;
		case StylePropertyId.MarginRight:
			layoutData.Write().marginRight = InitialStyle.marginRight;
			break;
		case StylePropertyId.MarginTop:
			layoutData.Write().marginTop = InitialStyle.marginTop;
			break;
		case StylePropertyId.MaxHeight:
			layoutData.Write().maxHeight = InitialStyle.maxHeight;
			break;
		case StylePropertyId.MaxWidth:
			layoutData.Write().maxWidth = InitialStyle.maxWidth;
			break;
		case StylePropertyId.MinHeight:
			layoutData.Write().minHeight = InitialStyle.minHeight;
			break;
		case StylePropertyId.MinWidth:
			layoutData.Write().minWidth = InitialStyle.minWidth;
			break;
		case StylePropertyId.Opacity:
			visualData.Write().opacity = InitialStyle.opacity;
			break;
		case StylePropertyId.Overflow:
			visualData.Write().overflow = InitialStyle.overflow;
			break;
		case StylePropertyId.Padding:
			layoutData.Write().paddingTop = InitialStyle.paddingTop;
			layoutData.Write().paddingRight = InitialStyle.paddingRight;
			layoutData.Write().paddingBottom = InitialStyle.paddingBottom;
			layoutData.Write().paddingLeft = InitialStyle.paddingLeft;
			break;
		case StylePropertyId.PaddingBottom:
			layoutData.Write().paddingBottom = InitialStyle.paddingBottom;
			break;
		case StylePropertyId.PaddingLeft:
			layoutData.Write().paddingLeft = InitialStyle.paddingLeft;
			break;
		case StylePropertyId.PaddingRight:
			layoutData.Write().paddingRight = InitialStyle.paddingRight;
			break;
		case StylePropertyId.PaddingTop:
			layoutData.Write().paddingTop = InitialStyle.paddingTop;
			break;
		case StylePropertyId.Position:
			layoutData.Write().position = InitialStyle.position;
			break;
		case StylePropertyId.Right:
			layoutData.Write().right = InitialStyle.right;
			break;
		case StylePropertyId.Rotate:
			transformData.Write().rotate = InitialStyle.rotate;
			break;
		case StylePropertyId.Scale:
			transformData.Write().scale = InitialStyle.scale;
			break;
		case StylePropertyId.TextOverflow:
			rareData.Write().textOverflow = InitialStyle.textOverflow;
			break;
		case StylePropertyId.TextShadow:
			inheritedData.Write().textShadow = InitialStyle.textShadow;
			break;
		case StylePropertyId.Top:
			layoutData.Write().top = InitialStyle.top;
			break;
		case StylePropertyId.TransformOrigin:
			transformData.Write().transformOrigin = InitialStyle.transformOrigin;
			break;
		case StylePropertyId.Transition:
			transitionData.Write().transitionDelay.CopyFrom(InitialStyle.transitionDelay);
			transitionData.Write().transitionDuration.CopyFrom(InitialStyle.transitionDuration);
			transitionData.Write().transitionProperty.CopyFrom(InitialStyle.transitionProperty);
			transitionData.Write().transitionTimingFunction.CopyFrom(InitialStyle.transitionTimingFunction);
			ResetComputedTransitions();
			break;
		case StylePropertyId.TransitionDelay:
			transitionData.Write().transitionDelay.CopyFrom(InitialStyle.transitionDelay);
			ResetComputedTransitions();
			break;
		case StylePropertyId.TransitionDuration:
			transitionData.Write().transitionDuration.CopyFrom(InitialStyle.transitionDuration);
			ResetComputedTransitions();
			break;
		case StylePropertyId.TransitionProperty:
			transitionData.Write().transitionProperty.CopyFrom(InitialStyle.transitionProperty);
			ResetComputedTransitions();
			break;
		case StylePropertyId.TransitionTimingFunction:
			transitionData.Write().transitionTimingFunction.CopyFrom(InitialStyle.transitionTimingFunction);
			ResetComputedTransitions();
			break;
		case StylePropertyId.Translate:
			transformData.Write().translate = InitialStyle.translate;
			break;
		case StylePropertyId.UnityBackgroundImageTintColor:
			rareData.Write().unityBackgroundImageTintColor = InitialStyle.unityBackgroundImageTintColor;
			break;
		case StylePropertyId.UnityBackgroundScaleMode:
			visualData.Write().backgroundPositionX = InitialStyle.backgroundPositionX;
			visualData.Write().backgroundPositionY = InitialStyle.backgroundPositionY;
			visualData.Write().backgroundRepeat = InitialStyle.backgroundRepeat;
			visualData.Write().backgroundSize = InitialStyle.backgroundSize;
			break;
		case StylePropertyId.UnityFont:
			inheritedData.Write().unityFont = InitialStyle.unityFont;
			break;
		case StylePropertyId.UnityFontDefinition:
			inheritedData.Write().unityFontDefinition = InitialStyle.unityFontDefinition;
			break;
		case StylePropertyId.UnityFontStyleAndWeight:
			inheritedData.Write().unityFontStyleAndWeight = InitialStyle.unityFontStyleAndWeight;
			break;
		case StylePropertyId.UnityOverflowClipBox:
			rareData.Write().unityOverflowClipBox = InitialStyle.unityOverflowClipBox;
			break;
		case StylePropertyId.UnityParagraphSpacing:
			inheritedData.Write().unityParagraphSpacing = InitialStyle.unityParagraphSpacing;
			break;
		case StylePropertyId.UnitySliceBottom:
			rareData.Write().unitySliceBottom = InitialStyle.unitySliceBottom;
			break;
		case StylePropertyId.UnitySliceLeft:
			rareData.Write().unitySliceLeft = InitialStyle.unitySliceLeft;
			break;
		case StylePropertyId.UnitySliceRight:
			rareData.Write().unitySliceRight = InitialStyle.unitySliceRight;
			break;
		case StylePropertyId.UnitySliceScale:
			rareData.Write().unitySliceScale = InitialStyle.unitySliceScale;
			break;
		case StylePropertyId.UnitySliceTop:
			rareData.Write().unitySliceTop = InitialStyle.unitySliceTop;
			break;
		case StylePropertyId.UnityTextAlign:
			inheritedData.Write().unityTextAlign = InitialStyle.unityTextAlign;
			break;
		case StylePropertyId.UnityTextOutline:
			inheritedData.Write().unityTextOutlineColor = InitialStyle.unityTextOutlineColor;
			inheritedData.Write().unityTextOutlineWidth = InitialStyle.unityTextOutlineWidth;
			break;
		case StylePropertyId.UnityTextOutlineColor:
			inheritedData.Write().unityTextOutlineColor = InitialStyle.unityTextOutlineColor;
			break;
		case StylePropertyId.UnityTextOutlineWidth:
			inheritedData.Write().unityTextOutlineWidth = InitialStyle.unityTextOutlineWidth;
			break;
		case StylePropertyId.UnityTextOverflowPosition:
			rareData.Write().unityTextOverflowPosition = InitialStyle.unityTextOverflowPosition;
			break;
		case StylePropertyId.Visibility:
			inheritedData.Write().visibility = InitialStyle.visibility;
			break;
		case StylePropertyId.WhiteSpace:
			inheritedData.Write().whiteSpace = InitialStyle.whiteSpace;
			break;
		case StylePropertyId.Width:
			layoutData.Write().width = InitialStyle.width;
			break;
		case StylePropertyId.WordSpacing:
			inheritedData.Write().wordSpacing = InitialStyle.wordSpacing;
			break;
		default:
			Debug.LogAssertion($"Unexpected property id {id}");
			break;
		}
	}

	public void ApplyUnsetValue(StylePropertyReader reader, ref ComputedStyle parentStyle)
	{
		StylePropertyId propertyId = reader.propertyId;
		StylePropertyId stylePropertyId = propertyId;
		if (stylePropertyId == StylePropertyId.Custom)
		{
			RemoveCustomStyleProperty(reader);
		}
		else
		{
			ApplyUnsetValue(reader.propertyId, ref parentStyle);
		}
	}

	public void ApplyUnsetValue(StylePropertyId id, ref ComputedStyle parentStyle)
	{
		switch (id)
		{
		case StylePropertyId.Color:
			inheritedData.Write().color = parentStyle.color;
			break;
		case StylePropertyId.FontSize:
			inheritedData.Write().fontSize = parentStyle.fontSize;
			break;
		case StylePropertyId.LetterSpacing:
			inheritedData.Write().letterSpacing = parentStyle.letterSpacing;
			break;
		case StylePropertyId.TextShadow:
			inheritedData.Write().textShadow = parentStyle.textShadow;
			break;
		case StylePropertyId.UnityFont:
			inheritedData.Write().unityFont = parentStyle.unityFont;
			break;
		case StylePropertyId.UnityFontDefinition:
			inheritedData.Write().unityFontDefinition = parentStyle.unityFontDefinition;
			break;
		case StylePropertyId.UnityFontStyleAndWeight:
			inheritedData.Write().unityFontStyleAndWeight = parentStyle.unityFontStyleAndWeight;
			break;
		case StylePropertyId.UnityParagraphSpacing:
			inheritedData.Write().unityParagraphSpacing = parentStyle.unityParagraphSpacing;
			break;
		case StylePropertyId.UnityTextAlign:
			inheritedData.Write().unityTextAlign = parentStyle.unityTextAlign;
			break;
		case StylePropertyId.UnityTextOutlineColor:
			inheritedData.Write().unityTextOutlineColor = parentStyle.unityTextOutlineColor;
			break;
		case StylePropertyId.UnityTextOutlineWidth:
			inheritedData.Write().unityTextOutlineWidth = parentStyle.unityTextOutlineWidth;
			break;
		case StylePropertyId.Visibility:
			inheritedData.Write().visibility = parentStyle.visibility;
			break;
		case StylePropertyId.WhiteSpace:
			inheritedData.Write().whiteSpace = parentStyle.whiteSpace;
			break;
		case StylePropertyId.WordSpacing:
			inheritedData.Write().wordSpacing = parentStyle.wordSpacing;
			break;
		default:
			ApplyInitialValue(id);
			break;
		}
	}

	public static VersionChangeType CompareChanges(ref ComputedStyle x, ref ComputedStyle y)
	{
		VersionChangeType versionChangeType = VersionChangeType.Styles;
		if (!x.layoutData.ReferenceEquals(y.layoutData))
		{
			if (x.flexGrow != y.flexGrow || x.flexShrink != y.flexShrink || x.flexWrap != y.flexWrap || x.flexDirection != y.flexDirection || x.justifyContent != y.justifyContent || x.bottom != y.bottom || x.left != y.left || x.right != y.right || x.top != y.top || x.height != y.height || x.width != y.width || x.paddingBottom != y.paddingBottom || x.paddingLeft != y.paddingLeft || x.paddingRight != y.paddingRight || x.paddingTop != y.paddingTop || x.marginBottom != y.marginBottom || x.marginLeft != y.marginLeft || x.marginRight != y.marginRight || x.marginTop != y.marginTop || x.position != y.position || x.alignContent != y.alignContent || x.alignItems != y.alignItems || x.alignSelf != y.alignSelf || x.flexBasis != y.flexBasis || x.maxHeight != y.maxHeight || x.maxWidth != y.maxWidth || x.minHeight != y.minHeight || x.minWidth != y.minWidth)
			{
				versionChangeType |= VersionChangeType.Layout;
			}
			if (x.borderBottomWidth != y.borderBottomWidth || x.borderLeftWidth != y.borderLeftWidth || x.borderRightWidth != y.borderRightWidth || x.borderTopWidth != y.borderTopWidth)
			{
				versionChangeType |= VersionChangeType.Layout | VersionChangeType.BorderWidth | VersionChangeType.Repaint;
			}
			if (x.display != y.display)
			{
				versionChangeType |= VersionChangeType.Layout | VersionChangeType.Repaint;
			}
		}
		if (!x.inheritedData.ReferenceEquals(y.inheritedData))
		{
			if (x.color != y.color)
			{
				versionChangeType |= VersionChangeType.Color;
			}
			if ((versionChangeType & (VersionChangeType.Layout | VersionChangeType.Repaint)) == 0 && (x.unityFont != y.unityFont || x.fontSize != y.fontSize || x.unityFontDefinition != y.unityFontDefinition || x.unityFontStyleAndWeight != y.unityFontStyleAndWeight || x.unityTextOutlineWidth != y.unityTextOutlineWidth || x.letterSpacing != y.letterSpacing || x.wordSpacing != y.wordSpacing || x.unityParagraphSpacing != y.unityParagraphSpacing))
			{
				versionChangeType |= VersionChangeType.Layout | VersionChangeType.Repaint;
			}
			if ((versionChangeType & VersionChangeType.Repaint) == 0 && (x.textShadow != y.textShadow || x.unityTextAlign != y.unityTextAlign || x.unityTextOutlineColor != y.unityTextOutlineColor))
			{
				versionChangeType |= VersionChangeType.Repaint;
			}
			if (x.visibility != y.visibility)
			{
				versionChangeType |= VersionChangeType.Repaint | VersionChangeType.Picking;
			}
			if (x.whiteSpace != y.whiteSpace)
			{
				versionChangeType |= VersionChangeType.Layout;
			}
		}
		if (!x.transformData.ReferenceEquals(y.transformData) && (x.scale != y.scale || x.rotate != y.rotate || x.translate != y.translate || x.transformOrigin != y.transformOrigin))
		{
			versionChangeType |= VersionChangeType.Transform;
		}
		if (!x.transitionData.ReferenceEquals(y.transitionData) && !ComputedTransitionUtils.SameTransitionProperty(ref x, ref y))
		{
			versionChangeType |= VersionChangeType.TransitionProperty;
		}
		if (!x.visualData.ReferenceEquals(y.visualData))
		{
			if ((versionChangeType & VersionChangeType.Color) == 0 && (x.backgroundColor != y.backgroundColor || x.borderBottomColor != y.borderBottomColor || x.borderLeftColor != y.borderLeftColor || x.borderRightColor != y.borderRightColor || x.borderTopColor != y.borderTopColor))
			{
				versionChangeType |= VersionChangeType.Color;
			}
			if ((versionChangeType & VersionChangeType.Repaint) == 0 && (x.backgroundImage != y.backgroundImage || x.backgroundPositionX != y.backgroundPositionX || x.backgroundPositionY != y.backgroundPositionY || x.backgroundRepeat != y.backgroundRepeat || x.backgroundSize != y.backgroundSize))
			{
				versionChangeType |= VersionChangeType.Repaint;
			}
			if (x.borderBottomLeftRadius != y.borderBottomLeftRadius || x.borderBottomRightRadius != y.borderBottomRightRadius || x.borderTopLeftRadius != y.borderTopLeftRadius || x.borderTopRightRadius != y.borderTopRightRadius)
			{
				versionChangeType |= VersionChangeType.BorderRadius | VersionChangeType.Repaint;
			}
			if (x.opacity != y.opacity)
			{
				versionChangeType |= VersionChangeType.Opacity;
			}
			if (x.overflow != y.overflow)
			{
				versionChangeType |= VersionChangeType.Layout | VersionChangeType.Overflow;
			}
		}
		if (!x.rareData.ReferenceEquals(y.rareData))
		{
			if (x.textOverflow != y.textOverflow || x.unitySliceScale != y.unitySliceScale)
			{
				versionChangeType |= VersionChangeType.Layout | VersionChangeType.Repaint;
			}
			if (x.unityBackgroundImageTintColor != y.unityBackgroundImageTintColor)
			{
				versionChangeType |= VersionChangeType.Color;
			}
			if ((versionChangeType & VersionChangeType.Repaint) == 0 && (x.unityOverflowClipBox != y.unityOverflowClipBox || x.unitySliceBottom != y.unitySliceBottom || x.unitySliceLeft != y.unitySliceLeft || x.unitySliceRight != y.unitySliceRight || x.unitySliceTop != y.unitySliceTop || x.unityTextOverflowPosition != y.unityTextOverflowPosition))
			{
				versionChangeType |= VersionChangeType.Repaint;
			}
		}
		return versionChangeType;
	}
}
