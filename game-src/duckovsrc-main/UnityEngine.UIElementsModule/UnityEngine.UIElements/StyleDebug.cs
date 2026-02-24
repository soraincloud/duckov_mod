#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements.StyleSheets;

namespace UnityEngine.UIElements;

internal static class StyleDebug
{
	internal const int UnitySpecificity = -1;

	internal const int UndefinedSpecificity = 0;

	internal const int InheritedSpecificity = 2147483646;

	internal const int InlineSpecificity = int.MaxValue;

	public static object GetComputedStyleValue(in ComputedStyle computedStyle, StylePropertyId id)
	{
		switch (id)
		{
		case StylePropertyId.AlignContent:
			return computedStyle.alignContent;
		case StylePropertyId.AlignItems:
			return computedStyle.alignItems;
		case StylePropertyId.AlignSelf:
			return computedStyle.alignSelf;
		case StylePropertyId.BackgroundColor:
			return computedStyle.backgroundColor;
		case StylePropertyId.BackgroundImage:
			return computedStyle.backgroundImage;
		case StylePropertyId.BackgroundPositionX:
			return computedStyle.backgroundPositionX;
		case StylePropertyId.BackgroundPositionY:
			return computedStyle.backgroundPositionY;
		case StylePropertyId.BackgroundRepeat:
			return computedStyle.backgroundRepeat;
		case StylePropertyId.BackgroundSize:
			return computedStyle.backgroundSize;
		case StylePropertyId.BorderBottomColor:
			return computedStyle.borderBottomColor;
		case StylePropertyId.BorderBottomLeftRadius:
			return computedStyle.borderBottomLeftRadius;
		case StylePropertyId.BorderBottomRightRadius:
			return computedStyle.borderBottomRightRadius;
		case StylePropertyId.BorderBottomWidth:
			return computedStyle.borderBottomWidth;
		case StylePropertyId.BorderLeftColor:
			return computedStyle.borderLeftColor;
		case StylePropertyId.BorderLeftWidth:
			return computedStyle.borderLeftWidth;
		case StylePropertyId.BorderRightColor:
			return computedStyle.borderRightColor;
		case StylePropertyId.BorderRightWidth:
			return computedStyle.borderRightWidth;
		case StylePropertyId.BorderTopColor:
			return computedStyle.borderTopColor;
		case StylePropertyId.BorderTopLeftRadius:
			return computedStyle.borderTopLeftRadius;
		case StylePropertyId.BorderTopRightRadius:
			return computedStyle.borderTopRightRadius;
		case StylePropertyId.BorderTopWidth:
			return computedStyle.borderTopWidth;
		case StylePropertyId.Bottom:
			return computedStyle.bottom;
		case StylePropertyId.Color:
			return computedStyle.color;
		case StylePropertyId.Cursor:
			return computedStyle.cursor;
		case StylePropertyId.Display:
			return computedStyle.display;
		case StylePropertyId.FlexBasis:
			return computedStyle.flexBasis;
		case StylePropertyId.FlexDirection:
			return computedStyle.flexDirection;
		case StylePropertyId.FlexGrow:
			return computedStyle.flexGrow;
		case StylePropertyId.FlexShrink:
			return computedStyle.flexShrink;
		case StylePropertyId.FlexWrap:
			return computedStyle.flexWrap;
		case StylePropertyId.FontSize:
			return computedStyle.fontSize;
		case StylePropertyId.Height:
			return computedStyle.height;
		case StylePropertyId.JustifyContent:
			return computedStyle.justifyContent;
		case StylePropertyId.Left:
			return computedStyle.left;
		case StylePropertyId.LetterSpacing:
			return computedStyle.letterSpacing;
		case StylePropertyId.MarginBottom:
			return computedStyle.marginBottom;
		case StylePropertyId.MarginLeft:
			return computedStyle.marginLeft;
		case StylePropertyId.MarginRight:
			return computedStyle.marginRight;
		case StylePropertyId.MarginTop:
			return computedStyle.marginTop;
		case StylePropertyId.MaxHeight:
			return computedStyle.maxHeight;
		case StylePropertyId.MaxWidth:
			return computedStyle.maxWidth;
		case StylePropertyId.MinHeight:
			return computedStyle.minHeight;
		case StylePropertyId.MinWidth:
			return computedStyle.minWidth;
		case StylePropertyId.Opacity:
			return computedStyle.opacity;
		case StylePropertyId.Overflow:
			return computedStyle.overflow;
		case StylePropertyId.PaddingBottom:
			return computedStyle.paddingBottom;
		case StylePropertyId.PaddingLeft:
			return computedStyle.paddingLeft;
		case StylePropertyId.PaddingRight:
			return computedStyle.paddingRight;
		case StylePropertyId.PaddingTop:
			return computedStyle.paddingTop;
		case StylePropertyId.Position:
			return computedStyle.position;
		case StylePropertyId.Right:
			return computedStyle.right;
		case StylePropertyId.Rotate:
			return computedStyle.rotate;
		case StylePropertyId.Scale:
			return computedStyle.scale;
		case StylePropertyId.TextOverflow:
			return computedStyle.textOverflow;
		case StylePropertyId.TextShadow:
			return computedStyle.textShadow;
		case StylePropertyId.Top:
			return computedStyle.top;
		case StylePropertyId.TransformOrigin:
			return computedStyle.transformOrigin;
		case StylePropertyId.TransitionDelay:
			return computedStyle.transitionDelay;
		case StylePropertyId.TransitionDuration:
			return computedStyle.transitionDuration;
		case StylePropertyId.TransitionProperty:
			return computedStyle.transitionProperty;
		case StylePropertyId.TransitionTimingFunction:
			return computedStyle.transitionTimingFunction;
		case StylePropertyId.Translate:
			return computedStyle.translate;
		case StylePropertyId.UnityBackgroundImageTintColor:
			return computedStyle.unityBackgroundImageTintColor;
		case StylePropertyId.UnityFont:
			return computedStyle.unityFont;
		case StylePropertyId.UnityFontDefinition:
			return computedStyle.unityFontDefinition;
		case StylePropertyId.UnityFontStyleAndWeight:
			return computedStyle.unityFontStyleAndWeight;
		case StylePropertyId.UnityOverflowClipBox:
			return computedStyle.unityOverflowClipBox;
		case StylePropertyId.UnityParagraphSpacing:
			return computedStyle.unityParagraphSpacing;
		case StylePropertyId.UnitySliceBottom:
			return computedStyle.unitySliceBottom;
		case StylePropertyId.UnitySliceLeft:
			return computedStyle.unitySliceLeft;
		case StylePropertyId.UnitySliceRight:
			return computedStyle.unitySliceRight;
		case StylePropertyId.UnitySliceScale:
			return computedStyle.unitySliceScale;
		case StylePropertyId.UnitySliceTop:
			return computedStyle.unitySliceTop;
		case StylePropertyId.UnityTextAlign:
			return computedStyle.unityTextAlign;
		case StylePropertyId.UnityTextOutlineColor:
			return computedStyle.unityTextOutlineColor;
		case StylePropertyId.UnityTextOutlineWidth:
			return computedStyle.unityTextOutlineWidth;
		case StylePropertyId.UnityTextOverflowPosition:
			return computedStyle.unityTextOverflowPosition;
		case StylePropertyId.Visibility:
			return computedStyle.visibility;
		case StylePropertyId.WhiteSpace:
			return computedStyle.whiteSpace;
		case StylePropertyId.Width:
			return computedStyle.width;
		case StylePropertyId.WordSpacing:
			return computedStyle.wordSpacing;
		default:
			Debug.LogAssertion($"Cannot get computed style value for property id {id}");
			return null;
		}
	}

	public static Type GetComputedStyleType(StylePropertyId id)
	{
		switch (id)
		{
		case StylePropertyId.AlignContent:
			return typeof(Align);
		case StylePropertyId.AlignItems:
			return typeof(Align);
		case StylePropertyId.AlignSelf:
			return typeof(Align);
		case StylePropertyId.BackgroundColor:
			return typeof(Color);
		case StylePropertyId.BackgroundImage:
			return typeof(Background);
		case StylePropertyId.BackgroundPositionX:
			return typeof(BackgroundPosition);
		case StylePropertyId.BackgroundPositionY:
			return typeof(BackgroundPosition);
		case StylePropertyId.BackgroundRepeat:
			return typeof(BackgroundRepeat);
		case StylePropertyId.BackgroundSize:
			return typeof(BackgroundSize);
		case StylePropertyId.BorderBottomColor:
			return typeof(Color);
		case StylePropertyId.BorderBottomLeftRadius:
			return typeof(Length);
		case StylePropertyId.BorderBottomRightRadius:
			return typeof(Length);
		case StylePropertyId.BorderBottomWidth:
			return typeof(float);
		case StylePropertyId.BorderLeftColor:
			return typeof(Color);
		case StylePropertyId.BorderLeftWidth:
			return typeof(float);
		case StylePropertyId.BorderRightColor:
			return typeof(Color);
		case StylePropertyId.BorderRightWidth:
			return typeof(float);
		case StylePropertyId.BorderTopColor:
			return typeof(Color);
		case StylePropertyId.BorderTopLeftRadius:
			return typeof(Length);
		case StylePropertyId.BorderTopRightRadius:
			return typeof(Length);
		case StylePropertyId.BorderTopWidth:
			return typeof(float);
		case StylePropertyId.Bottom:
			return typeof(Length);
		case StylePropertyId.Color:
			return typeof(Color);
		case StylePropertyId.Cursor:
			return typeof(Cursor);
		case StylePropertyId.Display:
			return typeof(DisplayStyle);
		case StylePropertyId.FlexBasis:
			return typeof(Length);
		case StylePropertyId.FlexDirection:
			return typeof(FlexDirection);
		case StylePropertyId.FlexGrow:
			return typeof(float);
		case StylePropertyId.FlexShrink:
			return typeof(float);
		case StylePropertyId.FlexWrap:
			return typeof(Wrap);
		case StylePropertyId.FontSize:
			return typeof(Length);
		case StylePropertyId.Height:
			return typeof(Length);
		case StylePropertyId.JustifyContent:
			return typeof(Justify);
		case StylePropertyId.Left:
			return typeof(Length);
		case StylePropertyId.LetterSpacing:
			return typeof(Length);
		case StylePropertyId.MarginBottom:
			return typeof(Length);
		case StylePropertyId.MarginLeft:
			return typeof(Length);
		case StylePropertyId.MarginRight:
			return typeof(Length);
		case StylePropertyId.MarginTop:
			return typeof(Length);
		case StylePropertyId.MaxHeight:
			return typeof(Length);
		case StylePropertyId.MaxWidth:
			return typeof(Length);
		case StylePropertyId.MinHeight:
			return typeof(Length);
		case StylePropertyId.MinWidth:
			return typeof(Length);
		case StylePropertyId.Opacity:
			return typeof(float);
		case StylePropertyId.Overflow:
			return typeof(OverflowInternal);
		case StylePropertyId.PaddingBottom:
			return typeof(Length);
		case StylePropertyId.PaddingLeft:
			return typeof(Length);
		case StylePropertyId.PaddingRight:
			return typeof(Length);
		case StylePropertyId.PaddingTop:
			return typeof(Length);
		case StylePropertyId.Position:
			return typeof(Position);
		case StylePropertyId.Right:
			return typeof(Length);
		case StylePropertyId.Rotate:
			return typeof(Rotate);
		case StylePropertyId.Scale:
			return typeof(Scale);
		case StylePropertyId.TextOverflow:
			return typeof(TextOverflow);
		case StylePropertyId.TextShadow:
			return typeof(TextShadow);
		case StylePropertyId.Top:
			return typeof(Length);
		case StylePropertyId.TransformOrigin:
			return typeof(TransformOrigin);
		case StylePropertyId.TransitionDelay:
			return typeof(List<TimeValue>);
		case StylePropertyId.TransitionDuration:
			return typeof(List<TimeValue>);
		case StylePropertyId.TransitionProperty:
			return typeof(List<StylePropertyName>);
		case StylePropertyId.TransitionTimingFunction:
			return typeof(List<EasingFunction>);
		case StylePropertyId.Translate:
			return typeof(Translate);
		case StylePropertyId.UnityBackgroundImageTintColor:
			return typeof(Color);
		case StylePropertyId.UnityFont:
			return typeof(Font);
		case StylePropertyId.UnityFontDefinition:
			return typeof(FontDefinition);
		case StylePropertyId.UnityFontStyleAndWeight:
			return typeof(FontStyle);
		case StylePropertyId.UnityOverflowClipBox:
			return typeof(OverflowClipBox);
		case StylePropertyId.UnityParagraphSpacing:
			return typeof(Length);
		case StylePropertyId.UnitySliceBottom:
			return typeof(int);
		case StylePropertyId.UnitySliceLeft:
			return typeof(int);
		case StylePropertyId.UnitySliceRight:
			return typeof(int);
		case StylePropertyId.UnitySliceScale:
			return typeof(float);
		case StylePropertyId.UnitySliceTop:
			return typeof(int);
		case StylePropertyId.UnityTextAlign:
			return typeof(TextAnchor);
		case StylePropertyId.UnityTextOutlineColor:
			return typeof(Color);
		case StylePropertyId.UnityTextOutlineWidth:
			return typeof(float);
		case StylePropertyId.UnityTextOverflowPosition:
			return typeof(TextOverflowPosition);
		case StylePropertyId.Visibility:
			return typeof(Visibility);
		case StylePropertyId.WhiteSpace:
			return typeof(WhiteSpace);
		case StylePropertyId.Width:
			return typeof(Length);
		case StylePropertyId.WordSpacing:
			return typeof(Length);
		default:
			Debug.LogAssertion($"Cannot get computed style type for property id {id}");
			return null;
		}
	}

	public static Type GetShorthandStyleType(StylePropertyId id)
	{
		switch (id)
		{
		case StylePropertyId.BackgroundPosition:
			return typeof(BackgroundPosition);
		case StylePropertyId.BorderColor:
			return typeof(Color);
		case StylePropertyId.BorderRadius:
			return typeof(Length);
		case StylePropertyId.BorderWidth:
			return typeof(float);
		case StylePropertyId.Margin:
			return typeof(Length);
		case StylePropertyId.Padding:
			return typeof(Length);
		default:
			Debug.LogAssertion($"Cannot get shorthand style type for property id {id}");
			return null;
		}
	}

	public static object GetInlineStyleValue(IStyle style, StylePropertyId id)
	{
		switch (id)
		{
		case StylePropertyId.AlignContent:
			return style.alignContent;
		case StylePropertyId.AlignItems:
			return style.alignItems;
		case StylePropertyId.AlignSelf:
			return style.alignSelf;
		case StylePropertyId.BackgroundColor:
			return style.backgroundColor;
		case StylePropertyId.BackgroundImage:
			return style.backgroundImage;
		case StylePropertyId.BackgroundPositionX:
			return style.backgroundPositionX;
		case StylePropertyId.BackgroundPositionY:
			return style.backgroundPositionY;
		case StylePropertyId.BackgroundRepeat:
			return style.backgroundRepeat;
		case StylePropertyId.BackgroundSize:
			return style.backgroundSize;
		case StylePropertyId.BorderBottomColor:
			return style.borderBottomColor;
		case StylePropertyId.BorderBottomLeftRadius:
			return style.borderBottomLeftRadius;
		case StylePropertyId.BorderBottomRightRadius:
			return style.borderBottomRightRadius;
		case StylePropertyId.BorderBottomWidth:
			return style.borderBottomWidth;
		case StylePropertyId.BorderLeftColor:
			return style.borderLeftColor;
		case StylePropertyId.BorderLeftWidth:
			return style.borderLeftWidth;
		case StylePropertyId.BorderRightColor:
			return style.borderRightColor;
		case StylePropertyId.BorderRightWidth:
			return style.borderRightWidth;
		case StylePropertyId.BorderTopColor:
			return style.borderTopColor;
		case StylePropertyId.BorderTopLeftRadius:
			return style.borderTopLeftRadius;
		case StylePropertyId.BorderTopRightRadius:
			return style.borderTopRightRadius;
		case StylePropertyId.BorderTopWidth:
			return style.borderTopWidth;
		case StylePropertyId.Bottom:
			return style.bottom;
		case StylePropertyId.Color:
			return style.color;
		case StylePropertyId.Cursor:
			return style.cursor;
		case StylePropertyId.Display:
			return style.display;
		case StylePropertyId.FlexBasis:
			return style.flexBasis;
		case StylePropertyId.FlexDirection:
			return style.flexDirection;
		case StylePropertyId.FlexGrow:
			return style.flexGrow;
		case StylePropertyId.FlexShrink:
			return style.flexShrink;
		case StylePropertyId.FlexWrap:
			return style.flexWrap;
		case StylePropertyId.FontSize:
			return style.fontSize;
		case StylePropertyId.Height:
			return style.height;
		case StylePropertyId.JustifyContent:
			return style.justifyContent;
		case StylePropertyId.Left:
			return style.left;
		case StylePropertyId.LetterSpacing:
			return style.letterSpacing;
		case StylePropertyId.MarginBottom:
			return style.marginBottom;
		case StylePropertyId.MarginLeft:
			return style.marginLeft;
		case StylePropertyId.MarginRight:
			return style.marginRight;
		case StylePropertyId.MarginTop:
			return style.marginTop;
		case StylePropertyId.MaxHeight:
			return style.maxHeight;
		case StylePropertyId.MaxWidth:
			return style.maxWidth;
		case StylePropertyId.MinHeight:
			return style.minHeight;
		case StylePropertyId.MinWidth:
			return style.minWidth;
		case StylePropertyId.Opacity:
			return style.opacity;
		case StylePropertyId.Overflow:
			return style.overflow;
		case StylePropertyId.PaddingBottom:
			return style.paddingBottom;
		case StylePropertyId.PaddingLeft:
			return style.paddingLeft;
		case StylePropertyId.PaddingRight:
			return style.paddingRight;
		case StylePropertyId.PaddingTop:
			return style.paddingTop;
		case StylePropertyId.Position:
			return style.position;
		case StylePropertyId.Right:
			return style.right;
		case StylePropertyId.Rotate:
			return style.rotate;
		case StylePropertyId.Scale:
			return style.scale;
		case StylePropertyId.TextOverflow:
			return style.textOverflow;
		case StylePropertyId.TextShadow:
			return style.textShadow;
		case StylePropertyId.Top:
			return style.top;
		case StylePropertyId.TransformOrigin:
			return style.transformOrigin;
		case StylePropertyId.TransitionDelay:
			return style.transitionDelay;
		case StylePropertyId.TransitionDuration:
			return style.transitionDuration;
		case StylePropertyId.TransitionProperty:
			return style.transitionProperty;
		case StylePropertyId.TransitionTimingFunction:
			return style.transitionTimingFunction;
		case StylePropertyId.Translate:
			return style.translate;
		case StylePropertyId.UnityBackgroundImageTintColor:
			return style.unityBackgroundImageTintColor;
		case StylePropertyId.UnityFont:
			return style.unityFont;
		case StylePropertyId.UnityFontDefinition:
			return style.unityFontDefinition;
		case StylePropertyId.UnityFontStyleAndWeight:
			return style.unityFontStyleAndWeight;
		case StylePropertyId.UnityOverflowClipBox:
			return style.unityOverflowClipBox;
		case StylePropertyId.UnityParagraphSpacing:
			return style.unityParagraphSpacing;
		case StylePropertyId.UnitySliceBottom:
			return style.unitySliceBottom;
		case StylePropertyId.UnitySliceLeft:
			return style.unitySliceLeft;
		case StylePropertyId.UnitySliceRight:
			return style.unitySliceRight;
		case StylePropertyId.UnitySliceScale:
			return style.unitySliceScale;
		case StylePropertyId.UnitySliceTop:
			return style.unitySliceTop;
		case StylePropertyId.UnityTextAlign:
			return style.unityTextAlign;
		case StylePropertyId.UnityTextOutlineColor:
			return style.unityTextOutlineColor;
		case StylePropertyId.UnityTextOutlineWidth:
			return style.unityTextOutlineWidth;
		case StylePropertyId.UnityTextOverflowPosition:
			return style.unityTextOverflowPosition;
		case StylePropertyId.Visibility:
			return style.visibility;
		case StylePropertyId.WhiteSpace:
			return style.whiteSpace;
		case StylePropertyId.Width:
			return style.width;
		case StylePropertyId.WordSpacing:
			return style.wordSpacing;
		default:
			Debug.LogAssertion($"Cannot get inline style value for property id {id}");
			return null;
		}
	}

	public static void SetInlineStyleValue(IStyle style, StylePropertyId id, object value)
	{
		switch (id)
		{
		case StylePropertyId.AlignContent:
			style.alignContent = (StyleEnum<Align>)value;
			break;
		case StylePropertyId.AlignItems:
			style.alignItems = (StyleEnum<Align>)value;
			break;
		case StylePropertyId.AlignSelf:
			style.alignSelf = (StyleEnum<Align>)value;
			break;
		case StylePropertyId.BackgroundColor:
			style.backgroundColor = (StyleColor)value;
			break;
		case StylePropertyId.BackgroundImage:
			style.backgroundImage = (StyleBackground)value;
			break;
		case StylePropertyId.BackgroundPositionX:
			style.backgroundPositionX = (StyleBackgroundPosition)value;
			break;
		case StylePropertyId.BackgroundPositionY:
			style.backgroundPositionY = (StyleBackgroundPosition)value;
			break;
		case StylePropertyId.BackgroundRepeat:
			style.backgroundRepeat = (StyleBackgroundRepeat)value;
			break;
		case StylePropertyId.BackgroundSize:
			style.backgroundSize = (StyleBackgroundSize)value;
			break;
		case StylePropertyId.BorderBottomColor:
			style.borderBottomColor = (StyleColor)value;
			break;
		case StylePropertyId.BorderBottomLeftRadius:
			style.borderBottomLeftRadius = (StyleLength)value;
			break;
		case StylePropertyId.BorderBottomRightRadius:
			style.borderBottomRightRadius = (StyleLength)value;
			break;
		case StylePropertyId.BorderBottomWidth:
			style.borderBottomWidth = (StyleFloat)value;
			break;
		case StylePropertyId.BorderLeftColor:
			style.borderLeftColor = (StyleColor)value;
			break;
		case StylePropertyId.BorderLeftWidth:
			style.borderLeftWidth = (StyleFloat)value;
			break;
		case StylePropertyId.BorderRightColor:
			style.borderRightColor = (StyleColor)value;
			break;
		case StylePropertyId.BorderRightWidth:
			style.borderRightWidth = (StyleFloat)value;
			break;
		case StylePropertyId.BorderTopColor:
			style.borderTopColor = (StyleColor)value;
			break;
		case StylePropertyId.BorderTopLeftRadius:
			style.borderTopLeftRadius = (StyleLength)value;
			break;
		case StylePropertyId.BorderTopRightRadius:
			style.borderTopRightRadius = (StyleLength)value;
			break;
		case StylePropertyId.BorderTopWidth:
			style.borderTopWidth = (StyleFloat)value;
			break;
		case StylePropertyId.Bottom:
			style.bottom = (StyleLength)value;
			break;
		case StylePropertyId.Color:
			style.color = (StyleColor)value;
			break;
		case StylePropertyId.Cursor:
			style.cursor = (StyleCursor)value;
			break;
		case StylePropertyId.Display:
			style.display = (StyleEnum<DisplayStyle>)value;
			break;
		case StylePropertyId.FlexBasis:
			style.flexBasis = (StyleLength)value;
			break;
		case StylePropertyId.FlexDirection:
			style.flexDirection = (StyleEnum<FlexDirection>)value;
			break;
		case StylePropertyId.FlexGrow:
			style.flexGrow = (StyleFloat)value;
			break;
		case StylePropertyId.FlexShrink:
			style.flexShrink = (StyleFloat)value;
			break;
		case StylePropertyId.FlexWrap:
			style.flexWrap = (StyleEnum<Wrap>)value;
			break;
		case StylePropertyId.FontSize:
			style.fontSize = (StyleLength)value;
			break;
		case StylePropertyId.Height:
			style.height = (StyleLength)value;
			break;
		case StylePropertyId.JustifyContent:
			style.justifyContent = (StyleEnum<Justify>)value;
			break;
		case StylePropertyId.Left:
			style.left = (StyleLength)value;
			break;
		case StylePropertyId.LetterSpacing:
			style.letterSpacing = (StyleLength)value;
			break;
		case StylePropertyId.MarginBottom:
			style.marginBottom = (StyleLength)value;
			break;
		case StylePropertyId.MarginLeft:
			style.marginLeft = (StyleLength)value;
			break;
		case StylePropertyId.MarginRight:
			style.marginRight = (StyleLength)value;
			break;
		case StylePropertyId.MarginTop:
			style.marginTop = (StyleLength)value;
			break;
		case StylePropertyId.MaxHeight:
			style.maxHeight = (StyleLength)value;
			break;
		case StylePropertyId.MaxWidth:
			style.maxWidth = (StyleLength)value;
			break;
		case StylePropertyId.MinHeight:
			style.minHeight = (StyleLength)value;
			break;
		case StylePropertyId.MinWidth:
			style.minWidth = (StyleLength)value;
			break;
		case StylePropertyId.Opacity:
			style.opacity = (StyleFloat)value;
			break;
		case StylePropertyId.Overflow:
			style.overflow = (StyleEnum<Overflow>)value;
			break;
		case StylePropertyId.PaddingBottom:
			style.paddingBottom = (StyleLength)value;
			break;
		case StylePropertyId.PaddingLeft:
			style.paddingLeft = (StyleLength)value;
			break;
		case StylePropertyId.PaddingRight:
			style.paddingRight = (StyleLength)value;
			break;
		case StylePropertyId.PaddingTop:
			style.paddingTop = (StyleLength)value;
			break;
		case StylePropertyId.Position:
			style.position = (StyleEnum<Position>)value;
			break;
		case StylePropertyId.Right:
			style.right = (StyleLength)value;
			break;
		case StylePropertyId.Rotate:
			style.rotate = (StyleRotate)value;
			break;
		case StylePropertyId.Scale:
			style.scale = (StyleScale)value;
			break;
		case StylePropertyId.TextOverflow:
			style.textOverflow = (StyleEnum<TextOverflow>)value;
			break;
		case StylePropertyId.TextShadow:
			style.textShadow = (StyleTextShadow)value;
			break;
		case StylePropertyId.Top:
			style.top = (StyleLength)value;
			break;
		case StylePropertyId.TransformOrigin:
			style.transformOrigin = (StyleTransformOrigin)value;
			break;
		case StylePropertyId.TransitionDelay:
			style.transitionDelay = (StyleList<TimeValue>)value;
			break;
		case StylePropertyId.TransitionDuration:
			style.transitionDuration = (StyleList<TimeValue>)value;
			break;
		case StylePropertyId.TransitionProperty:
			style.transitionProperty = (StyleList<StylePropertyName>)value;
			break;
		case StylePropertyId.TransitionTimingFunction:
			style.transitionTimingFunction = (StyleList<EasingFunction>)value;
			break;
		case StylePropertyId.Translate:
			style.translate = (StyleTranslate)value;
			break;
		case StylePropertyId.UnityBackgroundImageTintColor:
			style.unityBackgroundImageTintColor = (StyleColor)value;
			break;
		case StylePropertyId.UnityFont:
			style.unityFont = (StyleFont)value;
			break;
		case StylePropertyId.UnityFontDefinition:
			style.unityFontDefinition = (StyleFontDefinition)value;
			break;
		case StylePropertyId.UnityFontStyleAndWeight:
			style.unityFontStyleAndWeight = (StyleEnum<FontStyle>)value;
			break;
		case StylePropertyId.UnityOverflowClipBox:
			style.unityOverflowClipBox = (StyleEnum<OverflowClipBox>)value;
			break;
		case StylePropertyId.UnityParagraphSpacing:
			style.unityParagraphSpacing = (StyleLength)value;
			break;
		case StylePropertyId.UnitySliceBottom:
			style.unitySliceBottom = (StyleInt)value;
			break;
		case StylePropertyId.UnitySliceLeft:
			style.unitySliceLeft = (StyleInt)value;
			break;
		case StylePropertyId.UnitySliceRight:
			style.unitySliceRight = (StyleInt)value;
			break;
		case StylePropertyId.UnitySliceScale:
			style.unitySliceScale = (StyleFloat)value;
			break;
		case StylePropertyId.UnitySliceTop:
			style.unitySliceTop = (StyleInt)value;
			break;
		case StylePropertyId.UnityTextAlign:
			style.unityTextAlign = (StyleEnum<TextAnchor>)value;
			break;
		case StylePropertyId.UnityTextOutlineColor:
			style.unityTextOutlineColor = (StyleColor)value;
			break;
		case StylePropertyId.UnityTextOutlineWidth:
			style.unityTextOutlineWidth = (StyleFloat)value;
			break;
		case StylePropertyId.UnityTextOverflowPosition:
			style.unityTextOverflowPosition = (StyleEnum<TextOverflowPosition>)value;
			break;
		case StylePropertyId.Visibility:
			style.visibility = (StyleEnum<Visibility>)value;
			break;
		case StylePropertyId.WhiteSpace:
			style.whiteSpace = (StyleEnum<WhiteSpace>)value;
			break;
		case StylePropertyId.Width:
			style.width = (StyleLength)value;
			break;
		case StylePropertyId.WordSpacing:
			style.wordSpacing = (StyleLength)value;
			break;
		default:
			Debug.LogAssertion($"Cannot set inline style value for property id {id}");
			break;
		}
	}

	public static void SetInlineKeyword(IStyle style, StylePropertyId id, StyleKeyword keyword)
	{
		switch (id)
		{
		case StylePropertyId.AlignContent:
			style.alignContent = keyword;
			break;
		case StylePropertyId.AlignItems:
			style.alignItems = keyword;
			break;
		case StylePropertyId.AlignSelf:
			style.alignSelf = keyword;
			break;
		case StylePropertyId.BackgroundColor:
			style.backgroundColor = keyword;
			break;
		case StylePropertyId.BackgroundImage:
			style.backgroundImage = keyword;
			break;
		case StylePropertyId.BackgroundPositionX:
			style.backgroundPositionX = keyword;
			break;
		case StylePropertyId.BackgroundPositionY:
			style.backgroundPositionY = keyword;
			break;
		case StylePropertyId.BackgroundRepeat:
			style.backgroundRepeat = keyword;
			break;
		case StylePropertyId.BackgroundSize:
			style.backgroundSize = keyword;
			break;
		case StylePropertyId.BorderBottomColor:
			style.borderBottomColor = keyword;
			break;
		case StylePropertyId.BorderBottomLeftRadius:
			style.borderBottomLeftRadius = keyword;
			break;
		case StylePropertyId.BorderBottomRightRadius:
			style.borderBottomRightRadius = keyword;
			break;
		case StylePropertyId.BorderBottomWidth:
			style.borderBottomWidth = keyword;
			break;
		case StylePropertyId.BorderLeftColor:
			style.borderLeftColor = keyword;
			break;
		case StylePropertyId.BorderLeftWidth:
			style.borderLeftWidth = keyword;
			break;
		case StylePropertyId.BorderRightColor:
			style.borderRightColor = keyword;
			break;
		case StylePropertyId.BorderRightWidth:
			style.borderRightWidth = keyword;
			break;
		case StylePropertyId.BorderTopColor:
			style.borderTopColor = keyword;
			break;
		case StylePropertyId.BorderTopLeftRadius:
			style.borderTopLeftRadius = keyword;
			break;
		case StylePropertyId.BorderTopRightRadius:
			style.borderTopRightRadius = keyword;
			break;
		case StylePropertyId.BorderTopWidth:
			style.borderTopWidth = keyword;
			break;
		case StylePropertyId.Bottom:
			style.bottom = keyword;
			break;
		case StylePropertyId.Color:
			style.color = keyword;
			break;
		case StylePropertyId.Cursor:
			style.cursor = keyword;
			break;
		case StylePropertyId.Display:
			style.display = keyword;
			break;
		case StylePropertyId.FlexBasis:
			style.flexBasis = keyword;
			break;
		case StylePropertyId.FlexDirection:
			style.flexDirection = keyword;
			break;
		case StylePropertyId.FlexGrow:
			style.flexGrow = keyword;
			break;
		case StylePropertyId.FlexShrink:
			style.flexShrink = keyword;
			break;
		case StylePropertyId.FlexWrap:
			style.flexWrap = keyword;
			break;
		case StylePropertyId.FontSize:
			style.fontSize = keyword;
			break;
		case StylePropertyId.Height:
			style.height = keyword;
			break;
		case StylePropertyId.JustifyContent:
			style.justifyContent = keyword;
			break;
		case StylePropertyId.Left:
			style.left = keyword;
			break;
		case StylePropertyId.LetterSpacing:
			style.letterSpacing = keyword;
			break;
		case StylePropertyId.MarginBottom:
			style.marginBottom = keyword;
			break;
		case StylePropertyId.MarginLeft:
			style.marginLeft = keyword;
			break;
		case StylePropertyId.MarginRight:
			style.marginRight = keyword;
			break;
		case StylePropertyId.MarginTop:
			style.marginTop = keyword;
			break;
		case StylePropertyId.MaxHeight:
			style.maxHeight = keyword;
			break;
		case StylePropertyId.MaxWidth:
			style.maxWidth = keyword;
			break;
		case StylePropertyId.MinHeight:
			style.minHeight = keyword;
			break;
		case StylePropertyId.MinWidth:
			style.minWidth = keyword;
			break;
		case StylePropertyId.Opacity:
			style.opacity = keyword;
			break;
		case StylePropertyId.Overflow:
			style.overflow = keyword;
			break;
		case StylePropertyId.PaddingBottom:
			style.paddingBottom = keyword;
			break;
		case StylePropertyId.PaddingLeft:
			style.paddingLeft = keyword;
			break;
		case StylePropertyId.PaddingRight:
			style.paddingRight = keyword;
			break;
		case StylePropertyId.PaddingTop:
			style.paddingTop = keyword;
			break;
		case StylePropertyId.Position:
			style.position = keyword;
			break;
		case StylePropertyId.Right:
			style.right = keyword;
			break;
		case StylePropertyId.Rotate:
			style.rotate = keyword;
			break;
		case StylePropertyId.Scale:
			style.scale = keyword;
			break;
		case StylePropertyId.TextOverflow:
			style.textOverflow = keyword;
			break;
		case StylePropertyId.TextShadow:
			style.textShadow = keyword;
			break;
		case StylePropertyId.Top:
			style.top = keyword;
			break;
		case StylePropertyId.TransformOrigin:
			style.transformOrigin = keyword;
			break;
		case StylePropertyId.TransitionDelay:
			style.transitionDelay = keyword;
			break;
		case StylePropertyId.TransitionDuration:
			style.transitionDuration = keyword;
			break;
		case StylePropertyId.TransitionProperty:
			style.transitionProperty = keyword;
			break;
		case StylePropertyId.TransitionTimingFunction:
			style.transitionTimingFunction = keyword;
			break;
		case StylePropertyId.Translate:
			style.translate = keyword;
			break;
		case StylePropertyId.UnityBackgroundImageTintColor:
			style.unityBackgroundImageTintColor = keyword;
			break;
		case StylePropertyId.UnityFont:
			style.unityFont = keyword;
			break;
		case StylePropertyId.UnityFontDefinition:
			style.unityFontDefinition = keyword;
			break;
		case StylePropertyId.UnityFontStyleAndWeight:
			style.unityFontStyleAndWeight = keyword;
			break;
		case StylePropertyId.UnityOverflowClipBox:
			style.unityOverflowClipBox = keyword;
			break;
		case StylePropertyId.UnityParagraphSpacing:
			style.unityParagraphSpacing = keyword;
			break;
		case StylePropertyId.UnitySliceBottom:
			style.unitySliceBottom = keyword;
			break;
		case StylePropertyId.UnitySliceLeft:
			style.unitySliceLeft = keyword;
			break;
		case StylePropertyId.UnitySliceRight:
			style.unitySliceRight = keyword;
			break;
		case StylePropertyId.UnitySliceScale:
			style.unitySliceScale = keyword;
			break;
		case StylePropertyId.UnitySliceTop:
			style.unitySliceTop = keyword;
			break;
		case StylePropertyId.UnityTextAlign:
			style.unityTextAlign = keyword;
			break;
		case StylePropertyId.UnityTextOutlineColor:
			style.unityTextOutlineColor = keyword;
			break;
		case StylePropertyId.UnityTextOutlineWidth:
			style.unityTextOutlineWidth = keyword;
			break;
		case StylePropertyId.UnityTextOverflowPosition:
			style.unityTextOverflowPosition = keyword;
			break;
		case StylePropertyId.Visibility:
			style.visibility = keyword;
			break;
		case StylePropertyId.WhiteSpace:
			style.whiteSpace = keyword;
			break;
		case StylePropertyId.Width:
			style.width = keyword;
			break;
		case StylePropertyId.WordSpacing:
			style.wordSpacing = keyword;
			break;
		default:
			Debug.LogAssertion($"Cannot set inline keyword value for property id {id}");
			break;
		}
	}

	public static List<StyleKeyword> GetValidKeyword(StylePropertyId id)
	{
		switch (id)
		{
		case StylePropertyId.AlignContent:
			return new List<StyleKeyword> { StyleKeyword.Auto };
		case StylePropertyId.AlignItems:
			return new List<StyleKeyword> { StyleKeyword.Auto };
		case StylePropertyId.AlignSelf:
			return new List<StyleKeyword> { StyleKeyword.Auto };
		case StylePropertyId.BackgroundColor:
			return new List<StyleKeyword>();
		case StylePropertyId.BackgroundImage:
			return new List<StyleKeyword> { StyleKeyword.None };
		case StylePropertyId.BackgroundPositionX:
			return new List<StyleKeyword>();
		case StylePropertyId.BackgroundPositionY:
			return new List<StyleKeyword>();
		case StylePropertyId.BackgroundRepeat:
			return new List<StyleKeyword>();
		case StylePropertyId.BackgroundSize:
			return new List<StyleKeyword> { StyleKeyword.Auto };
		case StylePropertyId.BorderBottomColor:
			return new List<StyleKeyword>();
		case StylePropertyId.BorderBottomLeftRadius:
			return new List<StyleKeyword>();
		case StylePropertyId.BorderBottomRightRadius:
			return new List<StyleKeyword>();
		case StylePropertyId.BorderBottomWidth:
			return new List<StyleKeyword>();
		case StylePropertyId.BorderLeftColor:
			return new List<StyleKeyword>();
		case StylePropertyId.BorderLeftWidth:
			return new List<StyleKeyword>();
		case StylePropertyId.BorderRightColor:
			return new List<StyleKeyword>();
		case StylePropertyId.BorderRightWidth:
			return new List<StyleKeyword>();
		case StylePropertyId.BorderTopColor:
			return new List<StyleKeyword>();
		case StylePropertyId.BorderTopLeftRadius:
			return new List<StyleKeyword>();
		case StylePropertyId.BorderTopRightRadius:
			return new List<StyleKeyword>();
		case StylePropertyId.BorderTopWidth:
			return new List<StyleKeyword>();
		case StylePropertyId.Bottom:
			return new List<StyleKeyword> { StyleKeyword.Auto };
		case StylePropertyId.Color:
			return new List<StyleKeyword>();
		case StylePropertyId.Cursor:
			return new List<StyleKeyword>();
		case StylePropertyId.Display:
			return new List<StyleKeyword> { StyleKeyword.None };
		case StylePropertyId.FlexBasis:
			return new List<StyleKeyword>();
		case StylePropertyId.FlexDirection:
			return new List<StyleKeyword>();
		case StylePropertyId.FlexGrow:
			return new List<StyleKeyword>();
		case StylePropertyId.FlexShrink:
			return new List<StyleKeyword>();
		case StylePropertyId.FlexWrap:
			return new List<StyleKeyword>();
		case StylePropertyId.FontSize:
			return new List<StyleKeyword>();
		case StylePropertyId.Height:
			return new List<StyleKeyword> { StyleKeyword.Auto };
		case StylePropertyId.JustifyContent:
			return new List<StyleKeyword>();
		case StylePropertyId.Left:
			return new List<StyleKeyword> { StyleKeyword.Auto };
		case StylePropertyId.LetterSpacing:
			return new List<StyleKeyword>();
		case StylePropertyId.MarginBottom:
			return new List<StyleKeyword> { StyleKeyword.Auto };
		case StylePropertyId.MarginLeft:
			return new List<StyleKeyword> { StyleKeyword.Auto };
		case StylePropertyId.MarginRight:
			return new List<StyleKeyword> { StyleKeyword.Auto };
		case StylePropertyId.MarginTop:
			return new List<StyleKeyword> { StyleKeyword.Auto };
		case StylePropertyId.MaxHeight:
			return new List<StyleKeyword> { StyleKeyword.None };
		case StylePropertyId.MaxWidth:
			return new List<StyleKeyword> { StyleKeyword.None };
		case StylePropertyId.MinHeight:
			return new List<StyleKeyword> { StyleKeyword.Auto };
		case StylePropertyId.MinWidth:
			return new List<StyleKeyword> { StyleKeyword.Auto };
		case StylePropertyId.Opacity:
			return new List<StyleKeyword>();
		case StylePropertyId.Overflow:
			return new List<StyleKeyword>();
		case StylePropertyId.PaddingBottom:
			return new List<StyleKeyword>();
		case StylePropertyId.PaddingLeft:
			return new List<StyleKeyword>();
		case StylePropertyId.PaddingRight:
			return new List<StyleKeyword>();
		case StylePropertyId.PaddingTop:
			return new List<StyleKeyword>();
		case StylePropertyId.Position:
			return new List<StyleKeyword>();
		case StylePropertyId.Right:
			return new List<StyleKeyword> { StyleKeyword.Auto };
		case StylePropertyId.Rotate:
			return new List<StyleKeyword> { StyleKeyword.None };
		case StylePropertyId.Scale:
			return new List<StyleKeyword> { StyleKeyword.None };
		case StylePropertyId.TextOverflow:
			return new List<StyleKeyword>();
		case StylePropertyId.TextShadow:
			return new List<StyleKeyword>();
		case StylePropertyId.Top:
			return new List<StyleKeyword> { StyleKeyword.Auto };
		case StylePropertyId.TransformOrigin:
			return new List<StyleKeyword>();
		case StylePropertyId.TransitionDelay:
			return new List<StyleKeyword>();
		case StylePropertyId.TransitionDuration:
			return new List<StyleKeyword>();
		case StylePropertyId.TransitionProperty:
			return new List<StyleKeyword> { StyleKeyword.None };
		case StylePropertyId.TransitionTimingFunction:
			return new List<StyleKeyword>();
		case StylePropertyId.Translate:
			return new List<StyleKeyword> { StyleKeyword.None };
		case StylePropertyId.UnityBackgroundImageTintColor:
			return new List<StyleKeyword>();
		case StylePropertyId.UnityFont:
			return new List<StyleKeyword>();
		case StylePropertyId.UnityFontDefinition:
			return new List<StyleKeyword>();
		case StylePropertyId.UnityFontStyleAndWeight:
			return new List<StyleKeyword>();
		case StylePropertyId.UnityOverflowClipBox:
			return new List<StyleKeyword>();
		case StylePropertyId.UnityParagraphSpacing:
			return new List<StyleKeyword>();
		case StylePropertyId.UnitySliceBottom:
			return new List<StyleKeyword>();
		case StylePropertyId.UnitySliceLeft:
			return new List<StyleKeyword>();
		case StylePropertyId.UnitySliceRight:
			return new List<StyleKeyword>();
		case StylePropertyId.UnitySliceScale:
			return new List<StyleKeyword>();
		case StylePropertyId.UnitySliceTop:
			return new List<StyleKeyword>();
		case StylePropertyId.UnityTextAlign:
			return new List<StyleKeyword>();
		case StylePropertyId.UnityTextOutlineColor:
			return new List<StyleKeyword>();
		case StylePropertyId.UnityTextOutlineWidth:
			return new List<StyleKeyword>();
		case StylePropertyId.UnityTextOverflowPosition:
			return new List<StyleKeyword>();
		case StylePropertyId.Visibility:
			return new List<StyleKeyword>();
		case StylePropertyId.WhiteSpace:
			return new List<StyleKeyword>();
		case StylePropertyId.Width:
			return new List<StyleKeyword> { StyleKeyword.Auto };
		case StylePropertyId.WordSpacing:
			return new List<StyleKeyword>();
		default:
			Debug.LogAssertion($"Cannot get valid keyword value for property id {id}");
			return null;
		}
	}

	public static object ConvertComputedToInlineStyleValue(StylePropertyId id, object value)
	{
		switch (id)
		{
		case StylePropertyId.AlignContent:
			return (StyleEnum<Align>)(Align)value;
		case StylePropertyId.AlignItems:
			return (StyleEnum<Align>)(Align)value;
		case StylePropertyId.AlignSelf:
			return (StyleEnum<Align>)(Align)value;
		case StylePropertyId.BackgroundColor:
			return (StyleColor)(Color)value;
		case StylePropertyId.BackgroundImage:
			return (StyleBackground)(Background)value;
		case StylePropertyId.BackgroundPositionX:
			return (StyleBackgroundPosition)(BackgroundPosition)value;
		case StylePropertyId.BackgroundPositionY:
			return (StyleBackgroundPosition)(BackgroundPosition)value;
		case StylePropertyId.BackgroundRepeat:
			return (StyleBackgroundRepeat)(BackgroundRepeat)value;
		case StylePropertyId.BackgroundSize:
			return (StyleBackgroundSize)(BackgroundSize)value;
		case StylePropertyId.BorderBottomColor:
			return (StyleColor)(Color)value;
		case StylePropertyId.BorderBottomLeftRadius:
			return (StyleLength)(Length)value;
		case StylePropertyId.BorderBottomRightRadius:
			return (StyleLength)(Length)value;
		case StylePropertyId.BorderBottomWidth:
			return (StyleFloat)(float)value;
		case StylePropertyId.BorderLeftColor:
			return (StyleColor)(Color)value;
		case StylePropertyId.BorderLeftWidth:
			return (StyleFloat)(float)value;
		case StylePropertyId.BorderRightColor:
			return (StyleColor)(Color)value;
		case StylePropertyId.BorderRightWidth:
			return (StyleFloat)(float)value;
		case StylePropertyId.BorderTopColor:
			return (StyleColor)(Color)value;
		case StylePropertyId.BorderTopLeftRadius:
			return (StyleLength)(Length)value;
		case StylePropertyId.BorderTopRightRadius:
			return (StyleLength)(Length)value;
		case StylePropertyId.BorderTopWidth:
			return (StyleFloat)(float)value;
		case StylePropertyId.Bottom:
			return (StyleLength)(Length)value;
		case StylePropertyId.Color:
			return (StyleColor)(Color)value;
		case StylePropertyId.Cursor:
			return (StyleCursor)(Cursor)value;
		case StylePropertyId.Display:
			return (StyleEnum<DisplayStyle>)(DisplayStyle)value;
		case StylePropertyId.FlexBasis:
			return (StyleLength)(Length)value;
		case StylePropertyId.FlexDirection:
			return (StyleEnum<FlexDirection>)(FlexDirection)value;
		case StylePropertyId.FlexGrow:
			return (StyleFloat)(float)value;
		case StylePropertyId.FlexShrink:
			return (StyleFloat)(float)value;
		case StylePropertyId.FlexWrap:
			return (StyleEnum<Wrap>)(Wrap)value;
		case StylePropertyId.FontSize:
			return (StyleLength)(Length)value;
		case StylePropertyId.Height:
			return (StyleLength)(Length)value;
		case StylePropertyId.JustifyContent:
			return (StyleEnum<Justify>)(Justify)value;
		case StylePropertyId.Left:
			return (StyleLength)(Length)value;
		case StylePropertyId.LetterSpacing:
			return (StyleLength)(Length)value;
		case StylePropertyId.MarginBottom:
			return (StyleLength)(Length)value;
		case StylePropertyId.MarginLeft:
			return (StyleLength)(Length)value;
		case StylePropertyId.MarginRight:
			return (StyleLength)(Length)value;
		case StylePropertyId.MarginTop:
			return (StyleLength)(Length)value;
		case StylePropertyId.MaxHeight:
			return (StyleLength)(Length)value;
		case StylePropertyId.MaxWidth:
			return (StyleLength)(Length)value;
		case StylePropertyId.MinHeight:
			return (StyleLength)(Length)value;
		case StylePropertyId.MinWidth:
			return (StyleLength)(Length)value;
		case StylePropertyId.Opacity:
			return (StyleFloat)(float)value;
		case StylePropertyId.Overflow:
			return (StyleEnum<Overflow>)(Overflow)(OverflowInternal)value;
		case StylePropertyId.PaddingBottom:
			return (StyleLength)(Length)value;
		case StylePropertyId.PaddingLeft:
			return (StyleLength)(Length)value;
		case StylePropertyId.PaddingRight:
			return (StyleLength)(Length)value;
		case StylePropertyId.PaddingTop:
			return (StyleLength)(Length)value;
		case StylePropertyId.Position:
			return (StyleEnum<Position>)(Position)value;
		case StylePropertyId.Right:
			return (StyleLength)(Length)value;
		case StylePropertyId.Rotate:
			return (StyleRotate)(Rotate)value;
		case StylePropertyId.Scale:
			return (StyleScale)(Scale)value;
		case StylePropertyId.TextOverflow:
			return (StyleEnum<TextOverflow>)(TextOverflow)value;
		case StylePropertyId.TextShadow:
			return (StyleTextShadow)(TextShadow)value;
		case StylePropertyId.Top:
			return (StyleLength)(Length)value;
		case StylePropertyId.TransformOrigin:
			return (StyleTransformOrigin)(TransformOrigin)value;
		case StylePropertyId.TransitionDelay:
			return (StyleList<TimeValue>)(List<TimeValue>)value;
		case StylePropertyId.TransitionDuration:
			return (StyleList<TimeValue>)(List<TimeValue>)value;
		case StylePropertyId.TransitionProperty:
			return (StyleList<StylePropertyName>)(List<StylePropertyName>)value;
		case StylePropertyId.TransitionTimingFunction:
			return (StyleList<EasingFunction>)(List<EasingFunction>)value;
		case StylePropertyId.Translate:
			return (StyleTranslate)(Translate)value;
		case StylePropertyId.UnityBackgroundImageTintColor:
			return (StyleColor)(Color)value;
		case StylePropertyId.UnityFont:
			return (StyleFont)(Font)value;
		case StylePropertyId.UnityFontDefinition:
			return (StyleFontDefinition)(FontDefinition)value;
		case StylePropertyId.UnityFontStyleAndWeight:
			return (StyleEnum<FontStyle>)(FontStyle)value;
		case StylePropertyId.UnityOverflowClipBox:
			return (StyleEnum<OverflowClipBox>)(OverflowClipBox)value;
		case StylePropertyId.UnityParagraphSpacing:
			return (StyleLength)(Length)value;
		case StylePropertyId.UnitySliceBottom:
			return (StyleInt)(int)value;
		case StylePropertyId.UnitySliceLeft:
			return (StyleInt)(int)value;
		case StylePropertyId.UnitySliceRight:
			return (StyleInt)(int)value;
		case StylePropertyId.UnitySliceScale:
			return (StyleFloat)(float)value;
		case StylePropertyId.UnitySliceTop:
			return (StyleInt)(int)value;
		case StylePropertyId.UnityTextAlign:
			return (StyleEnum<TextAnchor>)(TextAnchor)value;
		case StylePropertyId.UnityTextOutlineColor:
			return (StyleColor)(Color)value;
		case StylePropertyId.UnityTextOutlineWidth:
			return (StyleFloat)(float)value;
		case StylePropertyId.UnityTextOverflowPosition:
			return (StyleEnum<TextOverflowPosition>)(TextOverflowPosition)value;
		case StylePropertyId.Visibility:
			return (StyleEnum<Visibility>)(Visibility)value;
		case StylePropertyId.WhiteSpace:
			return (StyleEnum<WhiteSpace>)(WhiteSpace)value;
		case StylePropertyId.Width:
			return (StyleLength)(Length)value;
		case StylePropertyId.WordSpacing:
			return (StyleLength)(Length)value;
		default:
			Debug.LogAssertion($"Cannot convert computed style value to inline style value for property id {id}");
			return null;
		}
	}

	public static Type GetInlineStyleType(StylePropertyId id)
	{
		switch (id)
		{
		case StylePropertyId.AlignContent:
			return typeof(StyleEnum<Align>);
		case StylePropertyId.AlignItems:
			return typeof(StyleEnum<Align>);
		case StylePropertyId.AlignSelf:
			return typeof(StyleEnum<Align>);
		case StylePropertyId.BackgroundColor:
			return typeof(StyleColor);
		case StylePropertyId.BackgroundImage:
			return typeof(StyleBackground);
		case StylePropertyId.BackgroundPositionX:
			return typeof(StyleBackgroundPosition);
		case StylePropertyId.BackgroundPositionY:
			return typeof(StyleBackgroundPosition);
		case StylePropertyId.BackgroundRepeat:
			return typeof(StyleBackgroundRepeat);
		case StylePropertyId.BackgroundSize:
			return typeof(StyleBackgroundSize);
		case StylePropertyId.BorderBottomColor:
			return typeof(StyleColor);
		case StylePropertyId.BorderBottomLeftRadius:
			return typeof(StyleLength);
		case StylePropertyId.BorderBottomRightRadius:
			return typeof(StyleLength);
		case StylePropertyId.BorderBottomWidth:
			return typeof(StyleFloat);
		case StylePropertyId.BorderLeftColor:
			return typeof(StyleColor);
		case StylePropertyId.BorderLeftWidth:
			return typeof(StyleFloat);
		case StylePropertyId.BorderRightColor:
			return typeof(StyleColor);
		case StylePropertyId.BorderRightWidth:
			return typeof(StyleFloat);
		case StylePropertyId.BorderTopColor:
			return typeof(StyleColor);
		case StylePropertyId.BorderTopLeftRadius:
			return typeof(StyleLength);
		case StylePropertyId.BorderTopRightRadius:
			return typeof(StyleLength);
		case StylePropertyId.BorderTopWidth:
			return typeof(StyleFloat);
		case StylePropertyId.Bottom:
			return typeof(StyleLength);
		case StylePropertyId.Color:
			return typeof(StyleColor);
		case StylePropertyId.Cursor:
			return typeof(StyleCursor);
		case StylePropertyId.Display:
			return typeof(StyleEnum<DisplayStyle>);
		case StylePropertyId.FlexBasis:
			return typeof(StyleLength);
		case StylePropertyId.FlexDirection:
			return typeof(StyleEnum<FlexDirection>);
		case StylePropertyId.FlexGrow:
			return typeof(StyleFloat);
		case StylePropertyId.FlexShrink:
			return typeof(StyleFloat);
		case StylePropertyId.FlexWrap:
			return typeof(StyleEnum<Wrap>);
		case StylePropertyId.FontSize:
			return typeof(StyleLength);
		case StylePropertyId.Height:
			return typeof(StyleLength);
		case StylePropertyId.JustifyContent:
			return typeof(StyleEnum<Justify>);
		case StylePropertyId.Left:
			return typeof(StyleLength);
		case StylePropertyId.LetterSpacing:
			return typeof(StyleLength);
		case StylePropertyId.MarginBottom:
			return typeof(StyleLength);
		case StylePropertyId.MarginLeft:
			return typeof(StyleLength);
		case StylePropertyId.MarginRight:
			return typeof(StyleLength);
		case StylePropertyId.MarginTop:
			return typeof(StyleLength);
		case StylePropertyId.MaxHeight:
			return typeof(StyleLength);
		case StylePropertyId.MaxWidth:
			return typeof(StyleLength);
		case StylePropertyId.MinHeight:
			return typeof(StyleLength);
		case StylePropertyId.MinWidth:
			return typeof(StyleLength);
		case StylePropertyId.Opacity:
			return typeof(StyleFloat);
		case StylePropertyId.Overflow:
			return typeof(StyleEnum<Overflow>);
		case StylePropertyId.PaddingBottom:
			return typeof(StyleLength);
		case StylePropertyId.PaddingLeft:
			return typeof(StyleLength);
		case StylePropertyId.PaddingRight:
			return typeof(StyleLength);
		case StylePropertyId.PaddingTop:
			return typeof(StyleLength);
		case StylePropertyId.Position:
			return typeof(StyleEnum<Position>);
		case StylePropertyId.Right:
			return typeof(StyleLength);
		case StylePropertyId.Rotate:
			return typeof(StyleRotate);
		case StylePropertyId.Scale:
			return typeof(StyleScale);
		case StylePropertyId.TextOverflow:
			return typeof(StyleEnum<TextOverflow>);
		case StylePropertyId.TextShadow:
			return typeof(StyleTextShadow);
		case StylePropertyId.Top:
			return typeof(StyleLength);
		case StylePropertyId.TransformOrigin:
			return typeof(StyleTransformOrigin);
		case StylePropertyId.TransitionDelay:
			return typeof(StyleList<TimeValue>);
		case StylePropertyId.TransitionDuration:
			return typeof(StyleList<TimeValue>);
		case StylePropertyId.TransitionProperty:
			return typeof(StyleList<StylePropertyName>);
		case StylePropertyId.TransitionTimingFunction:
			return typeof(StyleList<EasingFunction>);
		case StylePropertyId.Translate:
			return typeof(StyleTranslate);
		case StylePropertyId.UnityBackgroundImageTintColor:
			return typeof(StyleColor);
		case StylePropertyId.UnityFont:
			return typeof(StyleFont);
		case StylePropertyId.UnityFontDefinition:
			return typeof(StyleFontDefinition);
		case StylePropertyId.UnityFontStyleAndWeight:
			return typeof(StyleEnum<FontStyle>);
		case StylePropertyId.UnityOverflowClipBox:
			return typeof(StyleEnum<OverflowClipBox>);
		case StylePropertyId.UnityParagraphSpacing:
			return typeof(StyleLength);
		case StylePropertyId.UnitySliceBottom:
			return typeof(StyleInt);
		case StylePropertyId.UnitySliceLeft:
			return typeof(StyleInt);
		case StylePropertyId.UnitySliceRight:
			return typeof(StyleInt);
		case StylePropertyId.UnitySliceScale:
			return typeof(StyleFloat);
		case StylePropertyId.UnitySliceTop:
			return typeof(StyleInt);
		case StylePropertyId.UnityTextAlign:
			return typeof(StyleEnum<TextAnchor>);
		case StylePropertyId.UnityTextOutlineColor:
			return typeof(StyleColor);
		case StylePropertyId.UnityTextOutlineWidth:
			return typeof(StyleFloat);
		case StylePropertyId.UnityTextOverflowPosition:
			return typeof(StyleEnum<TextOverflowPosition>);
		case StylePropertyId.Visibility:
			return typeof(StyleEnum<Visibility>);
		case StylePropertyId.WhiteSpace:
			return typeof(StyleEnum<WhiteSpace>);
		case StylePropertyId.Width:
			return typeof(StyleLength);
		case StylePropertyId.WordSpacing:
			return typeof(StyleLength);
		default:
			Debug.LogAssertion($"Cannot get computed style type for property id {id}");
			return null;
		}
	}

	public static string[] GetLonghandPropertyNames(StylePropertyId id)
	{
		switch (id)
		{
		case StylePropertyId.All:
			return new string[0];
		case StylePropertyId.BackgroundPosition:
			return new string[2] { "background-position-x", "background-position-y" };
		case StylePropertyId.BorderColor:
			return new string[4] { "border-top-color", "border-right-color", "border-bottom-color", "border-left-color" };
		case StylePropertyId.BorderRadius:
			return new string[4] { "border-top-left-radius", "border-top-right-radius", "border-bottom-right-radius", "border-bottom-left-radius" };
		case StylePropertyId.BorderWidth:
			return new string[4] { "border-top-width", "border-right-width", "border-bottom-width", "border-left-width" };
		case StylePropertyId.Flex:
			return new string[3] { "flex-grow", "flex-shrink", "flex-basis" };
		case StylePropertyId.Margin:
			return new string[4] { "margin-top", "margin-right", "margin-bottom", "margin-left" };
		case StylePropertyId.Padding:
			return new string[4] { "padding-top", "padding-right", "padding-bottom", "padding-left" };
		case StylePropertyId.Transition:
			return new string[4] { "transition-delay", "transition-duration", "transition-property", "transition-timing-function" };
		case StylePropertyId.UnityBackgroundScaleMode:
			return new string[4] { "background-position-x", "background-position-y", "background-repeat", "background-size" };
		case StylePropertyId.UnityTextOutline:
			return new string[2] { "-unity-text-outline-color", "-unity-text-outline-width" };
		default:
			Debug.LogAssertion($"Cannot get longhand property names for property id {id}");
			return null;
		}
	}

	public static bool IsShorthandProperty(StylePropertyId id)
	{
		return id switch
		{
			StylePropertyId.All => true, 
			StylePropertyId.BackgroundPosition => true, 
			StylePropertyId.BorderColor => true, 
			StylePropertyId.BorderRadius => true, 
			StylePropertyId.BorderWidth => true, 
			StylePropertyId.Flex => true, 
			StylePropertyId.Margin => true, 
			StylePropertyId.Padding => true, 
			StylePropertyId.Transition => true, 
			StylePropertyId.UnityBackgroundScaleMode => true, 
			StylePropertyId.UnityTextOutline => true, 
			_ => false, 
		};
	}

	public static bool IsInheritedProperty(StylePropertyId id)
	{
		return id switch
		{
			StylePropertyId.Color => true, 
			StylePropertyId.FontSize => true, 
			StylePropertyId.LetterSpacing => true, 
			StylePropertyId.TextShadow => true, 
			StylePropertyId.UnityFont => true, 
			StylePropertyId.UnityFontDefinition => true, 
			StylePropertyId.UnityFontStyleAndWeight => true, 
			StylePropertyId.UnityParagraphSpacing => true, 
			StylePropertyId.UnityTextAlign => true, 
			StylePropertyId.UnityTextOutlineColor => true, 
			StylePropertyId.UnityTextOutlineWidth => true, 
			StylePropertyId.Visibility => true, 
			StylePropertyId.WhiteSpace => true, 
			StylePropertyId.WordSpacing => true, 
			_ => false, 
		};
	}

	public static StylePropertyId[] GetInheritedProperties()
	{
		return new StylePropertyId[14]
		{
			StylePropertyId.Color,
			StylePropertyId.FontSize,
			StylePropertyId.LetterSpacing,
			StylePropertyId.TextShadow,
			StylePropertyId.UnityFont,
			StylePropertyId.UnityFontDefinition,
			StylePropertyId.UnityFontStyleAndWeight,
			StylePropertyId.UnityParagraphSpacing,
			StylePropertyId.UnityTextAlign,
			StylePropertyId.UnityTextOutlineColor,
			StylePropertyId.UnityTextOutlineWidth,
			StylePropertyId.Visibility,
			StylePropertyId.WhiteSpace,
			StylePropertyId.WordSpacing
		};
	}

	public static bool IsDiscreteTypeProperty(StylePropertyId id)
	{
		return id switch
		{
			StylePropertyId.AlignContent => true, 
			StylePropertyId.AlignItems => true, 
			StylePropertyId.AlignSelf => true, 
			StylePropertyId.BackgroundImage => true, 
			StylePropertyId.BackgroundPositionX => true, 
			StylePropertyId.BackgroundPositionY => true, 
			StylePropertyId.BackgroundRepeat => true, 
			StylePropertyId.Display => true, 
			StylePropertyId.FlexDirection => true, 
			StylePropertyId.FlexWrap => true, 
			StylePropertyId.JustifyContent => true, 
			StylePropertyId.Overflow => true, 
			StylePropertyId.Position => true, 
			StylePropertyId.TextOverflow => true, 
			StylePropertyId.UnityFont => true, 
			StylePropertyId.UnityFontDefinition => true, 
			StylePropertyId.UnityFontStyleAndWeight => true, 
			StylePropertyId.UnityOverflowClipBox => true, 
			StylePropertyId.UnityTextAlign => true, 
			StylePropertyId.UnityTextOverflowPosition => true, 
			StylePropertyId.Visibility => true, 
			StylePropertyId.WhiteSpace => true, 
			_ => false, 
		};
	}

	public static string[] GetStylePropertyNames()
	{
		List<string> list = StylePropertyUtil.s_NameToId.Keys.ToList();
		list.Sort();
		return list.ToArray();
	}

	public static string[] GetLonghandPropertyNames(string shorthandName)
	{
		if (StylePropertyUtil.s_NameToId.TryGetValue(shorthandName, out var value) && IsShorthandProperty(value))
		{
			return GetLonghandPropertyNames(value);
		}
		return null;
	}

	public static StylePropertyId GetStylePropertyIdFromName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return StylePropertyId.Unknown;
		}
		if (StylePropertyUtil.s_NameToId.TryGetValue(name, out var value))
		{
			return value;
		}
		return StylePropertyId.Unknown;
	}

	public static object GetComputedStyleValue(in ComputedStyle computedStyle, string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		if (StylePropertyUtil.s_NameToId.TryGetValue(name, out var value))
		{
			bool valid;
			if (value == StylePropertyId.UnityBackgroundScaleMode)
			{
				return BackgroundPropertyHelper.ResolveUnityBackgroundScaleMode(computedStyle.backgroundPositionX, computedStyle.backgroundPositionY, computedStyle.backgroundRepeat, computedStyle.backgroundSize, out valid);
			}
			return GetComputedStyleValue(in computedStyle, value);
		}
		return null;
	}

	public static object GetInlineStyleValue(IStyle style, string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		if (StylePropertyUtil.s_NameToId.TryGetValue(name, out var value))
		{
			if (value == StylePropertyId.UnityBackgroundScaleMode)
			{
				return style.unityBackgroundScaleMode;
			}
			return GetInlineStyleValue(style, value);
		}
		return null;
	}

	public static void SetInlineStyleValue(IStyle style, string name, object value)
	{
		if (StylePropertyUtil.s_NameToId.TryGetValue(name, out var value2))
		{
			SetInlineStyleValue(style, value2, value);
		}
	}

	public static Type GetInlineStyleType(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		if (StylePropertyUtil.s_NameToId.TryGetValue(name, out var value))
		{
			if (value == StylePropertyId.UnityBackgroundScaleMode)
			{
				return typeof(StyleEnum<ScaleMode>);
			}
			if (!IsShorthandProperty(value))
			{
				return GetInlineStyleType(value);
			}
		}
		return null;
	}

	public static Type GetComputedStyleType(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		if (StylePropertyUtil.s_NameToId.TryGetValue(name, out var value))
		{
			if (value == StylePropertyId.UnityBackgroundScaleMode)
			{
				return typeof(ScaleMode);
			}
			if (!IsShorthandProperty(value))
			{
				return GetComputedStyleType(value);
			}
		}
		return null;
	}

	public static void FindSpecifiedStyles(in ComputedStyle computedStyle, IEnumerable<SelectorMatchRecord> matchRecords, Dictionary<StylePropertyId, int> result)
	{
		result.Clear();
		foreach (SelectorMatchRecord matchRecord in matchRecords)
		{
			int value = matchRecord.complexSelector.specificity;
			if (matchRecord.sheet.isDefaultStyleSheet)
			{
				value = -1;
			}
			StyleProperty[] properties = matchRecord.complexSelector.rule.properties;
			StyleProperty[] array = properties;
			foreach (StyleProperty styleProperty in array)
			{
				if (!StylePropertyUtil.s_NameToId.TryGetValue(styleProperty.name, out var value2))
				{
					continue;
				}
				if (IsShorthandProperty(value2))
				{
					string[] longhandPropertyNames = GetLonghandPropertyNames(value2);
					string[] array2 = longhandPropertyNames;
					foreach (string name in array2)
					{
						StylePropertyId stylePropertyIdFromName = GetStylePropertyIdFromName(name);
						result[stylePropertyIdFromName] = value;
					}
				}
				else
				{
					result[value2] = value;
				}
			}
		}
		StylePropertyId[] inheritedProperties = GetInheritedProperties();
		StylePropertyId[] array3 = inheritedProperties;
		foreach (StylePropertyId stylePropertyId in array3)
		{
			if (!result.ContainsKey(stylePropertyId))
			{
				object computedStyleValue = GetComputedStyleValue(in computedStyle, stylePropertyId);
				object computedStyleValue2 = GetComputedStyleValue(in InitialStyle.Get(), stylePropertyId);
				if (computedStyleValue != null && !computedStyleValue.Equals(computedStyleValue2))
				{
					result[stylePropertyId] = 2147483646;
				}
			}
		}
	}
}
