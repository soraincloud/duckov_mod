using System.Collections.Generic;

namespace UnityEngine.UIElements;

public class FocusController
{
	private struct FocusedElement
	{
		public VisualElement m_SubTreeRoot;

		public Focusable m_FocusedElement;
	}

	private TextElement m_SelectedTextElement;

	private List<FocusedElement> m_FocusedElements = new List<FocusedElement>();

	private Focusable m_LastFocusedElement;

	internal Focusable m_LastPendingFocusedElement;

	private int m_PendingFocusCount = 0;

	private IFocusRing focusRing { get; }

	internal TextElement selectedTextElement
	{
		get
		{
			return m_SelectedTextElement;
		}
		set
		{
			if (m_SelectedTextElement != value)
			{
				m_SelectedTextElement?.selection.SelectNone();
				m_SelectedTextElement = value;
			}
		}
	}

	public Focusable focusedElement
	{
		get
		{
			Focusable retargetedFocusedElement = GetRetargetedFocusedElement(null);
			return IsLocalElement(retargetedFocusedElement) ? retargetedFocusedElement : null;
		}
	}

	internal int imguiKeyboardControl { get; set; }

	public FocusController(IFocusRing focusRing)
	{
		this.focusRing = focusRing;
		imguiKeyboardControl = 0;
	}

	internal bool IsFocused(Focusable f)
	{
		if (!IsLocalElement(f))
		{
			return false;
		}
		foreach (FocusedElement focusedElement in m_FocusedElements)
		{
			if (focusedElement.m_FocusedElement == f)
			{
				return true;
			}
		}
		return false;
	}

	internal Focusable GetRetargetedFocusedElement(VisualElement retargetAgainst)
	{
		VisualElement visualElement = retargetAgainst?.hierarchy.parent;
		if (visualElement == null)
		{
			if (m_FocusedElements.Count > 0)
			{
				return m_FocusedElements[m_FocusedElements.Count - 1].m_FocusedElement;
			}
		}
		else
		{
			while (!visualElement.isCompositeRoot && visualElement.hierarchy.parent != null)
			{
				visualElement = visualElement.hierarchy.parent;
			}
			foreach (FocusedElement focusedElement in m_FocusedElements)
			{
				if (focusedElement.m_SubTreeRoot == visualElement)
				{
					return focusedElement.m_FocusedElement;
				}
			}
		}
		return null;
	}

	internal Focusable GetLeafFocusedElement()
	{
		if (m_FocusedElements.Count > 0)
		{
			Focusable focusable = m_FocusedElements[0].m_FocusedElement;
			return IsLocalElement(focusable) ? focusable : null;
		}
		return null;
	}

	private bool IsLocalElement(Focusable f)
	{
		return f?.focusController == this;
	}

	internal void ClearPendingFocusEvents()
	{
		m_PendingFocusCount = 0;
		m_LastPendingFocusedElement = null;
	}

	internal bool IsPendingFocus(Focusable f)
	{
		for (VisualElement visualElement = m_LastPendingFocusedElement as VisualElement; visualElement != null; visualElement = visualElement.hierarchy.parent)
		{
			if (f == visualElement)
			{
				return true;
			}
		}
		return false;
	}

	internal void SetFocusToLastFocusedElement()
	{
		if (m_LastFocusedElement != null && !(m_LastFocusedElement is IMGUIContainer))
		{
			m_LastFocusedElement.Focus();
		}
	}

	internal void BlurLastFocusedElement()
	{
		selectedTextElement = null;
		if (m_LastFocusedElement != null && !(m_LastFocusedElement is IMGUIContainer))
		{
			Focusable lastFocusedElement = m_LastFocusedElement;
			m_LastFocusedElement.Blur();
			m_LastFocusedElement = lastFocusedElement;
		}
	}

	internal void DoFocusChange(Focusable f)
	{
		m_FocusedElements.Clear();
		for (VisualElement visualElement = f as VisualElement; visualElement != null; visualElement = visualElement.hierarchy.parent)
		{
			if (visualElement.hierarchy.parent == null || visualElement.isCompositeRoot)
			{
				m_FocusedElements.Add(new FocusedElement
				{
					m_SubTreeRoot = visualElement,
					m_FocusedElement = f
				});
				f = visualElement;
			}
		}
	}

	internal void ProcessPendingFocusChange(Focusable f)
	{
		m_PendingFocusCount--;
		if (m_PendingFocusCount == 0)
		{
			m_LastPendingFocusedElement = null;
		}
		DoFocusChange(f);
	}

	internal Focusable FocusNextInDirection(FocusChangeDirection direction)
	{
		Focusable nextFocusable = focusRing.GetNextFocusable(GetLeafFocusedElement(), direction);
		direction.ApplyTo(this, nextFocusable);
		return nextFocusable;
	}

	private void AboutToReleaseFocus(Focusable focusable, Focusable willGiveFocusTo, FocusChangeDirection direction, DispatchMode dispatchMode)
	{
		using FocusOutEvent e = FocusEventBase<FocusOutEvent>.GetPooled(focusable, willGiveFocusTo, direction, this);
		focusable.SendEvent(e, dispatchMode);
	}

	private void ReleaseFocus(Focusable focusable, Focusable willGiveFocusTo, FocusChangeDirection direction, DispatchMode dispatchMode)
	{
		using BlurEvent e = FocusEventBase<BlurEvent>.GetPooled(focusable, willGiveFocusTo, direction, this);
		focusable.SendEvent(e, dispatchMode);
	}

	private void AboutToGrabFocus(Focusable focusable, Focusable willTakeFocusFrom, FocusChangeDirection direction, DispatchMode dispatchMode)
	{
		using FocusInEvent e = FocusEventBase<FocusInEvent>.GetPooled(focusable, willTakeFocusFrom, direction, this);
		focusable.SendEvent(e, dispatchMode);
	}

	private void GrabFocus(Focusable focusable, Focusable willTakeFocusFrom, FocusChangeDirection direction, bool bIsFocusDelegated, DispatchMode dispatchMode)
	{
		using FocusEvent e = FocusEventBase<FocusEvent>.GetPooled(focusable, willTakeFocusFrom, direction, this, bIsFocusDelegated);
		focusable.SendEvent(e, dispatchMode);
	}

	internal void Blur(Focusable focusable, bool bIsFocusDelegated = false, DispatchMode dispatchMode = DispatchMode.Default)
	{
		if ((m_PendingFocusCount > 0) ? IsPendingFocus(focusable) : IsFocused(focusable))
		{
			SwitchFocus(null, bIsFocusDelegated, dispatchMode);
		}
	}

	internal void SwitchFocus(Focusable newFocusedElement, bool bIsFocusDelegated = false, DispatchMode dispatchMode = DispatchMode.Default)
	{
		SwitchFocus(newFocusedElement, FocusChangeDirection.unspecified, bIsFocusDelegated, dispatchMode);
	}

	internal void SwitchFocus(Focusable newFocusedElement, FocusChangeDirection direction, bool bIsFocusDelegated = false, DispatchMode dispatchMode = DispatchMode.Default)
	{
		m_LastFocusedElement = newFocusedElement;
		Focusable focusable = ((m_PendingFocusCount > 0) ? m_LastPendingFocusedElement : GetLeafFocusedElement());
		if (focusable == newFocusedElement)
		{
			return;
		}
		if (newFocusedElement == null || !newFocusedElement.canGrabFocus)
		{
			if (focusable != null)
			{
				m_LastPendingFocusedElement = null;
				m_PendingFocusCount++;
				AboutToReleaseFocus(focusable, null, direction, dispatchMode);
				ReleaseFocus(focusable, null, direction, dispatchMode);
			}
		}
		else if (newFocusedElement != focusable)
		{
			Focusable willGiveFocusTo = (newFocusedElement as VisualElement)?.RetargetElement(focusable as VisualElement) ?? newFocusedElement;
			Focusable willTakeFocusFrom = (focusable as VisualElement)?.RetargetElement(newFocusedElement as VisualElement) ?? focusable;
			m_LastPendingFocusedElement = newFocusedElement;
			m_PendingFocusCount++;
			if (focusable != null)
			{
				AboutToReleaseFocus(focusable, willGiveFocusTo, direction, dispatchMode);
			}
			AboutToGrabFocus(newFocusedElement, willTakeFocusFrom, direction, dispatchMode);
			if (focusable != null)
			{
				ReleaseFocus(focusable, willGiveFocusTo, direction, dispatchMode);
			}
			GrabFocus(newFocusedElement, willTakeFocusFrom, direction, bIsFocusDelegated, dispatchMode);
		}
	}

	internal void SwitchFocusOnEvent(EventBase e)
	{
		if (e.processedByFocusController)
		{
			return;
		}
		using FocusChangeDirection focusChangeDirection = focusRing.GetFocusChangeDirection(GetLeafFocusedElement(), e);
		if (focusChangeDirection != FocusChangeDirection.none)
		{
			FocusNextInDirection(focusChangeDirection);
			e.processedByFocusController = true;
		}
	}

	internal void ReevaluateFocus()
	{
		if (focusedElement is VisualElement visualElement && (!visualElement.isHierarchyDisplayed || !visualElement.visible))
		{
			visualElement.Blur();
		}
	}

	internal bool GetFocusableParentForPointerEvent(Focusable target, out Focusable effectiveTarget)
	{
		if (target == null || !target.focusable)
		{
			effectiveTarget = target;
			return target != null;
		}
		effectiveTarget = target;
		while (effectiveTarget is VisualElement visualElement && (!visualElement.enabledInHierarchy || !visualElement.focusable) && visualElement.hierarchy.parent != null)
		{
			effectiveTarget = visualElement.hierarchy.parent;
		}
		return !IsFocused(effectiveTarget);
	}

	internal void SyncIMGUIFocus(int imguiKeyboardControlID, Focusable imguiContainerHavingKeyboardControl, bool forceSwitch)
	{
		imguiKeyboardControl = imguiKeyboardControlID;
		if (forceSwitch || imguiKeyboardControl != 0)
		{
			SwitchFocus(imguiContainerHavingKeyboardControl, FocusChangeDirection.unspecified);
		}
		else
		{
			SwitchFocus(null, FocusChangeDirection.unspecified);
		}
	}
}
