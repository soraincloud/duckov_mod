using System;

namespace UnityEngine.UIElements;

public abstract class Focusable : CallbackEventHandler
{
	private bool m_DelegatesFocus;

	private bool m_ExcludeFromFocusRing;

	public abstract FocusController focusController { get; }

	public bool focusable { get; set; }

	public int tabIndex { get; set; }

	public bool delegatesFocus
	{
		get
		{
			return m_DelegatesFocus;
		}
		set
		{
			m_DelegatesFocus = value;
		}
	}

	internal bool excludeFromFocusRing
	{
		get
		{
			return m_ExcludeFromFocusRing;
		}
		set
		{
			if (!((VisualElement)this).isCompositeRoot)
			{
				throw new InvalidOperationException("excludeFromFocusRing should only be set on composite roots.");
			}
			m_ExcludeFromFocusRing = value;
		}
	}

	public virtual bool canGrabFocus => focusable;

	protected Focusable()
	{
		focusable = true;
		tabIndex = 0;
	}

	public virtual void Focus()
	{
		if (focusController != null)
		{
			if (canGrabFocus)
			{
				Focusable focusDelegate = GetFocusDelegate();
				focusController.SwitchFocus(focusDelegate, this != focusDelegate);
			}
			else
			{
				focusController.SwitchFocus(null);
			}
		}
	}

	public virtual void Blur()
	{
		focusController?.Blur(this);
	}

	internal void BlurImmediately()
	{
		focusController?.Blur(this, bIsFocusDelegated: false, DispatchMode.Immediate);
	}

	private Focusable GetFocusDelegate()
	{
		Focusable focusable = this;
		while (focusable != null && focusable.delegatesFocus)
		{
			focusable = GetFirstFocusableChild(focusable as VisualElement);
		}
		return focusable;
	}

	private static Focusable GetFirstFocusableChild(VisualElement ve)
	{
		int childCount = ve.hierarchy.childCount;
		for (int i = 0; i < childCount; i++)
		{
			VisualElement visualElement = ve.hierarchy[i];
			if (visualElement.canGrabFocus && visualElement.tabIndex >= 0)
			{
				return visualElement;
			}
			bool flag = visualElement.hierarchy.parent != null && visualElement == visualElement.hierarchy.parent.contentContainer;
			if (!visualElement.isCompositeRoot && !flag)
			{
				Focusable firstFocusableChild = GetFirstFocusableChild(visualElement);
				if (firstFocusableChild != null)
				{
					return firstFocusableChild;
				}
			}
		}
		return null;
	}

	[EventInterest(new Type[]
	{
		typeof(PointerDownEvent),
		typeof(NavigationMoveEvent)
	})]
	protected override void ExecuteDefaultAction(EventBase evt)
	{
		base.ExecuteDefaultAction(evt);
		ProcessEvent(evt);
	}

	[EventInterest(new Type[]
	{
		typeof(PointerDownEvent),
		typeof(NavigationMoveEvent)
	})]
	internal override void ExecuteDefaultActionDisabled(EventBase evt)
	{
		base.ExecuteDefaultActionDisabled(evt);
		ProcessEvent(evt);
	}

	private void ProcessEvent(EventBase evt)
	{
		if (evt != null && evt.target == evt.leafTarget)
		{
			focusController?.SwitchFocusOnEvent(evt);
		}
	}
}
