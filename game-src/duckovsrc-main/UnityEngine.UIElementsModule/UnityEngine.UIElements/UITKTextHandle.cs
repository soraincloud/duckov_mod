using System;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements.Experimental;

namespace UnityEngine.UIElements;

internal class UITKTextHandle : TextHandle
{
	private TextElement m_TextElement;

	internal bool isOverridingCursor = false;

	internal int currentLinkIDHash = -1;

	internal bool hasLinkTag = false;

	internal bool hasATag = false;

	internal static readonly float k_MinPadding = 6f;

	public Vector2 MeasuredSizes { get; set; }

	public Vector2 RoundedSizes { get; set; }

	public UITKTextHandle(TextElement te)
	{
		m_TextElement = te;
	}

	public float ComputeTextWidth(string textToMeasure, bool wordWrap, float width, float height)
	{
		ConvertUssToTextGenerationSettings(TextHandle.s_LayoutSettings);
		TextHandle.s_LayoutSettings.text = textToMeasure;
		TextHandle.s_LayoutSettings.screenRect = new Rect(0f, 0f, width, height);
		TextHandle.s_LayoutSettings.wordWrap = wordWrap;
		return ComputeTextWidth(TextHandle.s_LayoutSettings);
	}

	public float ComputeTextHeight(string textToMeasure, float width, float height)
	{
		ConvertUssToTextGenerationSettings(TextHandle.s_LayoutSettings);
		TextHandle.s_LayoutSettings.text = textToMeasure;
		TextHandle.s_LayoutSettings.screenRect = new Rect(0f, 0f, width, height);
		return ComputeTextHeight(TextHandle.s_LayoutSettings);
	}

	public TextInfo Update()
	{
		ConvertUssToTextGenerationSettings(textGenerationSettings);
		Vector2 vector = m_TextElement.contentRect.size;
		if (Mathf.Abs(vector.x - RoundedSizes.x) < 0.01f && Mathf.Abs(vector.y - RoundedSizes.y) < 0.01f)
		{
			vector = MeasuredSizes;
		}
		else
		{
			RoundedSizes = vector;
			MeasuredSizes = vector;
		}
		textGenerationSettings.screenRect = new Rect(Vector2.zero, vector);
		Update(textGenerationSettings);
		HandleATag();
		HandleLinkTag();
		return base.textInfo;
	}

	private void ATagOnPointerUp(PointerUpEvent pue)
	{
		Vector3 position = pue.localPosition - new Vector3(m_TextElement.contentRect.min.x, m_TextElement.contentRect.min.y);
		int num = FindIntersectingLink(position);
		if (num < 0)
		{
			return;
		}
		LinkInfo linkInfo = base.textInfo.linkInfo[num];
		if (linkInfo.hashCode == 2535353 && linkInfo.linkId != null && linkInfo.linkIdLength > 0)
		{
			string linkId = linkInfo.GetLinkId();
			if (Uri.IsWellFormedUriString(linkId, UriKind.Absolute))
			{
				Application.OpenURL(linkId);
			}
		}
	}

	private void ATagOnPointerOver(PointerOverEvent _)
	{
		isOverridingCursor = false;
	}

	private void ATagOnPointerMove(PointerMoveEvent pme)
	{
		Vector3 position = pme.localPosition - new Vector3(m_TextElement.contentRect.min.x, m_TextElement.contentRect.min.y);
		int num = FindIntersectingLink(position);
		ICursorManager cursorManager = (m_TextElement.panel as BaseVisualElementPanel)?.cursorManager;
		if (num >= 0)
		{
			LinkInfo linkInfo = base.textInfo.linkInfo[num];
			if (linkInfo.hashCode == 2535353)
			{
				if (!isOverridingCursor)
				{
					isOverridingCursor = true;
					cursorManager?.SetCursor(new Cursor
					{
						defaultCursorId = 4
					});
				}
				return;
			}
		}
		if (isOverridingCursor)
		{
			cursorManager?.SetCursor(m_TextElement.computedStyle.cursor);
			isOverridingCursor = false;
		}
	}

	private void ATagOnPointerOut(PointerOutEvent _)
	{
		isOverridingCursor = false;
	}

	internal void LinkTagOnPointerDown(PointerDownEvent pde)
	{
		Vector3 position = pde.localPosition - new Vector3(m_TextElement.contentRect.min.x, m_TextElement.contentRect.min.y);
		int num = FindIntersectingLink(position);
		if (num < 0)
		{
			return;
		}
		LinkInfo linkInfo = base.textInfo.linkInfo[num];
		if (linkInfo.hashCode == 2535353 || linkInfo.linkId == null || linkInfo.linkIdLength <= 0)
		{
			return;
		}
		using PointerDownLinkTagEvent pointerDownLinkTagEvent = PointerDownLinkTagEvent.GetPooled(pde, linkInfo.GetLinkId(), linkInfo.GetLinkText(base.textInfo));
		pointerDownLinkTagEvent.target = m_TextElement;
		m_TextElement.SendEvent(pointerDownLinkTagEvent);
	}

	internal void LinkTagOnPointerUp(PointerUpEvent pue)
	{
		Vector3 position = pue.localPosition - new Vector3(m_TextElement.contentRect.min.x, m_TextElement.contentRect.min.y);
		int num = FindIntersectingLink(position);
		if (num < 0)
		{
			return;
		}
		LinkInfo linkInfo = base.textInfo.linkInfo[num];
		if (linkInfo.hashCode == 2535353 || linkInfo.linkId == null || linkInfo.linkIdLength <= 0)
		{
			return;
		}
		using PointerUpLinkTagEvent pointerUpLinkTagEvent = PointerUpLinkTagEvent.GetPooled(pue, linkInfo.GetLinkId(), linkInfo.GetLinkText(base.textInfo));
		pointerUpLinkTagEvent.target = m_TextElement;
		m_TextElement.SendEvent(pointerUpLinkTagEvent);
	}

	internal void LinkTagOnPointerMove(PointerMoveEvent pme)
	{
		Vector3 position = pme.localPosition - new Vector3(m_TextElement.contentRect.min.x, m_TextElement.contentRect.min.y);
		int num = FindIntersectingLink(position);
		if (num >= 0)
		{
			LinkInfo linkInfo = base.textInfo.linkInfo[num];
			if (linkInfo.hashCode != 2535353)
			{
				if (currentLinkIDHash == -1)
				{
					currentLinkIDHash = linkInfo.hashCode;
					using PointerOverLinkTagEvent pointerOverLinkTagEvent = PointerOverLinkTagEvent.GetPooled(pme, linkInfo.GetLinkId(), linkInfo.GetLinkText(base.textInfo));
					pointerOverLinkTagEvent.target = m_TextElement;
					m_TextElement.SendEvent(pointerOverLinkTagEvent);
					return;
				}
				if (currentLinkIDHash == linkInfo.hashCode)
				{
					using (PointerMoveLinkTagEvent pointerMoveLinkTagEvent = PointerMoveLinkTagEvent.GetPooled(pme, linkInfo.GetLinkId(), linkInfo.GetLinkText(base.textInfo)))
					{
						pointerMoveLinkTagEvent.target = m_TextElement;
						m_TextElement.SendEvent(pointerMoveLinkTagEvent);
						return;
					}
				}
			}
		}
		if (currentLinkIDHash != -1)
		{
			currentLinkIDHash = -1;
			using PointerOutLinkTagEvent pointerOutLinkTagEvent = PointerOutLinkTagEvent.GetPooled(pme, string.Empty);
			pointerOutLinkTagEvent.target = m_TextElement;
			m_TextElement.SendEvent(pointerOutLinkTagEvent);
		}
	}

	private void LinkTagOnPointerOut(PointerOutEvent poe)
	{
		if (currentLinkIDHash != -1)
		{
			using (PointerOutLinkTagEvent pointerOutLinkTagEvent = PointerOutLinkTagEvent.GetPooled(poe, string.Empty))
			{
				pointerOutLinkTagEvent.target = m_TextElement;
				m_TextElement.SendEvent(pointerOutLinkTagEvent);
			}
			currentLinkIDHash = -1;
		}
	}

	private void HandleLinkTag()
	{
		for (int i = 0; i < base.textInfo.linkCount; i++)
		{
			LinkInfo linkInfo = base.textInfo.linkInfo[i];
			if (linkInfo.hashCode != 2535353)
			{
				m_TextElement.RegisterCallback<PointerDownEvent>(LinkTagOnPointerDown, TrickleDown.TrickleDown);
				m_TextElement.RegisterCallback<PointerUpEvent>(LinkTagOnPointerUp, TrickleDown.TrickleDown);
				m_TextElement.RegisterCallback<PointerMoveEvent>(LinkTagOnPointerMove, TrickleDown.TrickleDown);
				m_TextElement.RegisterCallback<PointerOutEvent>(LinkTagOnPointerOut, TrickleDown.TrickleDown);
				hasLinkTag = true;
				return;
			}
		}
		if (hasLinkTag)
		{
			hasLinkTag = false;
			m_TextElement.UnregisterCallback<PointerDownEvent>(LinkTagOnPointerDown, TrickleDown.TrickleDown);
			m_TextElement.UnregisterCallback<PointerUpEvent>(LinkTagOnPointerUp, TrickleDown.TrickleDown);
			m_TextElement.UnregisterCallback<PointerMoveEvent>(LinkTagOnPointerMove, TrickleDown.TrickleDown);
			m_TextElement.UnregisterCallback<PointerOutEvent>(LinkTagOnPointerOut, TrickleDown.TrickleDown);
		}
	}

	private void HandleATag()
	{
		for (int i = 0; i < base.textInfo.linkCount; i++)
		{
			LinkInfo linkInfo = base.textInfo.linkInfo[i];
			if (linkInfo.hashCode == 2535353)
			{
				m_TextElement.RegisterCallback<PointerUpEvent>(ATagOnPointerUp, TrickleDown.TrickleDown);
				if (m_TextElement.panel.contextType == ContextType.Editor)
				{
					m_TextElement.RegisterCallback<PointerMoveEvent>(ATagOnPointerMove, TrickleDown.TrickleDown);
					m_TextElement.RegisterCallback<PointerOverEvent>(ATagOnPointerOver, TrickleDown.TrickleDown);
					m_TextElement.RegisterCallback<PointerOutEvent>(ATagOnPointerOut, TrickleDown.TrickleDown);
				}
				hasATag = true;
				return;
			}
		}
		if (hasATag)
		{
			hasATag = false;
			m_TextElement.UnregisterCallback<PointerUpEvent>(ATagOnPointerUp, TrickleDown.TrickleDown);
			if (m_TextElement.panel.contextType == ContextType.Editor)
			{
				m_TextElement.UnregisterCallback<PointerMoveEvent>(ATagOnPointerMove, TrickleDown.TrickleDown);
				m_TextElement.UnregisterCallback<PointerOverEvent>(ATagOnPointerOver, TrickleDown.TrickleDown);
				m_TextElement.UnregisterCallback<PointerOutEvent>(ATagOnPointerOut, TrickleDown.TrickleDown);
			}
		}
	}

	private TextOverflowMode GetTextOverflowMode()
	{
		ComputedStyle computedStyle = m_TextElement.computedStyle;
		if (computedStyle.textOverflow == TextOverflow.Clip)
		{
			return TextOverflowMode.Masking;
		}
		if (computedStyle.textOverflow != TextOverflow.Ellipsis)
		{
			return TextOverflowMode.Overflow;
		}
		if (!TextLibraryCanElide())
		{
			return TextOverflowMode.Masking;
		}
		if (computedStyle.overflow == OverflowInternal.Hidden)
		{
			return TextOverflowMode.Ellipsis;
		}
		return TextOverflowMode.Overflow;
	}

	internal void ConvertUssToTextGenerationSettings(UnityEngine.TextCore.Text.TextGenerationSettings tgs)
	{
		ComputedStyle computedStyle = m_TextElement.computedStyle;
		tgs.textSettings = TextUtilities.GetTextSettingsFrom(m_TextElement);
		if (!(tgs.textSettings == null))
		{
			tgs.fontAsset = TextUtilities.GetFontAsset(m_TextElement);
			if (!(tgs.fontAsset == null))
			{
				tgs.material = tgs.fontAsset.material;
				tgs.screenRect = new Rect(0f, 0f, m_TextElement.contentRect.width, m_TextElement.contentRect.height);
				tgs.extraPadding = GetTextEffectPadding(tgs.fontAsset);
				tgs.text = ((m_TextElement.isElided && !TextLibraryCanElide()) ? m_TextElement.elidedText : m_TextElement.renderedText);
				tgs.fontSize = ((computedStyle.fontSize.value > 0f) ? computedStyle.fontSize.value : ((float)tgs.fontAsset.faceInfo.pointSize));
				tgs.fontStyle = TextGeneratorUtilities.LegacyStyleToNewStyle(computedStyle.unityFontStyleAndWeight);
				tgs.textAlignment = TextGeneratorUtilities.LegacyAlignmentToNewAlignment(computedStyle.unityTextAlign);
				tgs.wordWrap = computedStyle.whiteSpace == WhiteSpace.Normal;
				tgs.wordWrappingRatio = 0.4f;
				tgs.richText = m_TextElement.enableRichText;
				tgs.overflowMode = GetTextOverflowMode();
				tgs.characterSpacing = computedStyle.letterSpacing.value;
				tgs.wordSpacing = computedStyle.wordSpacing.value;
				tgs.paragraphSpacing = computedStyle.unityParagraphSpacing.value;
				tgs.color = computedStyle.color;
				tgs.shouldConvertToLinearSpace = false;
				tgs.isRightToLeft = m_TextElement.localLanguageDirection == LanguageDirection.RTL;
				tgs.parseControlCharacters = m_TextElement.parseEscapeSequences;
				tgs.inverseYAxis = true;
			}
		}
	}

	internal bool TextLibraryCanElide()
	{
		return m_TextElement.computedStyle.unityTextOverflowPosition == TextOverflowPosition.End;
	}

	internal float GetTextEffectPadding(FontAsset fontAsset)
	{
		ComputedStyle computedStyle = m_TextElement.computedStyle;
		float num = computedStyle.unityTextOutlineWidth / 2f;
		float num2 = Mathf.Abs(computedStyle.textShadow.offset.x);
		float num3 = Mathf.Abs(computedStyle.textShadow.offset.y);
		float num4 = Mathf.Abs(computedStyle.textShadow.blurRadius);
		if (num <= 0f && num2 <= 0f && num3 <= 0f && num4 <= 0f)
		{
			return k_MinPadding;
		}
		float a = Mathf.Max(num2 + num4, num);
		float b = Mathf.Max(num3 + num4, num);
		float num5 = Mathf.Max(a, b) + k_MinPadding;
		float num6 = TextUtilities.ConvertPixelUnitsToTextCoreRelativeUnits(m_TextElement, fontAsset);
		int num7 = fontAsset.atlasPadding + 1;
		return Mathf.Min(num5 * num6 * (float)num7, num7);
	}
}
