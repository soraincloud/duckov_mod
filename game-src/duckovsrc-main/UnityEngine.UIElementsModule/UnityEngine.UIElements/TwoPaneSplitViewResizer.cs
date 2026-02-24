using System;

namespace UnityEngine.UIElements;

internal class TwoPaneSplitViewResizer : PointerManipulator
{
	private const float k_DragLineTolerance = 1f;

	private Vector3 m_Start;

	protected bool m_Active;

	private TwoPaneSplitView m_SplitView;

	private int m_Direction;

	private TwoPaneSplitViewOrientation orientation => m_SplitView.orientation;

	private VisualElement fixedPane => m_SplitView.fixedPane;

	private VisualElement flexedPane => m_SplitView.flexedPane;

	private float fixedPaneMinDimension
	{
		get
		{
			if (orientation == TwoPaneSplitViewOrientation.Horizontal)
			{
				return fixedPane.resolvedStyle.minWidth.value;
			}
			return fixedPane.resolvedStyle.minHeight.value;
		}
	}

	private float fixedPaneMargins
	{
		get
		{
			if (orientation == TwoPaneSplitViewOrientation.Horizontal)
			{
				return fixedPane.resolvedStyle.marginLeft + fixedPane.resolvedStyle.marginRight;
			}
			return fixedPane.resolvedStyle.marginTop + fixedPane.resolvedStyle.marginBottom;
		}
	}

	private float flexedPaneMinDimension
	{
		get
		{
			if (orientation == TwoPaneSplitViewOrientation.Horizontal)
			{
				return flexedPane.resolvedStyle.minWidth.value;
			}
			return flexedPane.resolvedStyle.minHeight.value;
		}
	}

	private float flexedPaneMargin
	{
		get
		{
			if (orientation == TwoPaneSplitViewOrientation.Horizontal)
			{
				return flexedPane.resolvedStyle.marginLeft + flexedPane.resolvedStyle.marginRight;
			}
			return flexedPane.resolvedStyle.marginTop + flexedPane.resolvedStyle.marginBottom;
		}
	}

	public TwoPaneSplitViewResizer(TwoPaneSplitView splitView, int dir)
	{
		m_SplitView = splitView;
		m_Direction = dir;
		base.activators.Add(new ManipulatorActivationFilter
		{
			button = MouseButton.LeftMouse
		});
		m_Active = false;
	}

	protected override void RegisterCallbacksOnTarget()
	{
		base.target.RegisterCallback<PointerDownEvent>(OnPointerDown);
		base.target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
		base.target.RegisterCallback<PointerUpEvent>(OnPointerUp);
	}

	protected override void UnregisterCallbacksFromTarget()
	{
		base.target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
		base.target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
		base.target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
	}

	public void ApplyDelta(float delta)
	{
		float num = ((orientation == TwoPaneSplitViewOrientation.Horizontal) ? fixedPane.resolvedStyle.width : fixedPane.resolvedStyle.height);
		float num2 = num + delta;
		float num3 = fixedPaneMinDimension;
		if (m_SplitView.fixedPaneIndex == 1)
		{
			num3 += ((orientation == TwoPaneSplitViewOrientation.Horizontal) ? (base.target.worldBound.width + Math.Abs(m_SplitView.dragLine.resolvedStyle.left)) : (base.target.worldBound.height + Math.Abs(m_SplitView.dragLine.resolvedStyle.top)));
		}
		if (num2 < num && num2 < num3)
		{
			num2 = num3;
		}
		float num4 = ((orientation == TwoPaneSplitViewOrientation.Horizontal) ? m_SplitView.resolvedStyle.width : m_SplitView.resolvedStyle.height);
		num4 -= flexedPaneMinDimension + flexedPaneMargin + fixedPaneMargins;
		if (m_SplitView.fixedPaneIndex == 0)
		{
			num4 -= ((orientation == TwoPaneSplitViewOrientation.Horizontal) ? Math.Abs(base.target.worldBound.width - (m_SplitView.dragLine.resolvedStyle.width - Math.Abs(m_SplitView.dragLine.resolvedStyle.left))) : Math.Abs(base.target.worldBound.height - (m_SplitView.dragLine.resolvedStyle.height - Math.Abs(m_SplitView.dragLine.resolvedStyle.top))));
		}
		if (num2 > num && num2 > num4)
		{
			num2 = num4;
		}
		if (orientation == TwoPaneSplitViewOrientation.Horizontal)
		{
			fixedPane.style.width = num2;
			if (m_SplitView.fixedPaneIndex == 0)
			{
				base.target.style.left = num2 + fixedPaneMargins;
			}
			else
			{
				float num5 = m_SplitView.resolvedStyle.width - num2 - fixedPaneMargins;
				if (num5 >= flexedPaneMinDimension + flexedPaneMargin)
				{
					base.target.style.left = num5;
				}
			}
		}
		else
		{
			fixedPane.style.height = num2;
			if (m_SplitView.fixedPaneIndex == 0)
			{
				base.target.style.top = num2 + fixedPaneMargins;
			}
			else
			{
				float num6 = m_SplitView.resolvedStyle.height - num2 - fixedPaneMargins;
				if (num6 >= flexedPaneMinDimension + flexedPaneMargin)
				{
					base.target.style.top = num6;
				}
			}
		}
		m_SplitView.fixedPaneDimension = num2;
	}

	protected void OnPointerDown(PointerDownEvent e)
	{
		if (m_Active)
		{
			e.StopImmediatePropagation();
		}
		else if (CanStartManipulation(e))
		{
			m_Start = e.localPosition;
			m_Active = true;
			base.target.CapturePointer(e.pointerId);
			e.StopPropagation();
		}
	}

	protected void OnPointerMove(PointerMoveEvent e)
	{
		if (!m_Active || !base.target.HasPointerCapture(e.pointerId))
		{
			return;
		}
		bool flag = ((orientation == TwoPaneSplitViewOrientation.Horizontal) ? (m_SplitView.dragLine.worldBound.x < base.target.worldBound.x) : (m_SplitView.dragLine.worldBound.y < base.target.worldBound.y));
		float num = ((orientation == TwoPaneSplitViewOrientation.Horizontal) ? Math.Abs(base.target.worldBound.x - m_SplitView.dragLine.worldBound.x) : Math.Abs(base.target.worldBound.y - m_SplitView.dragLine.worldBound.y));
		float value = ((orientation == TwoPaneSplitViewOrientation.Horizontal) ? m_SplitView.dragLine.resolvedStyle.left : m_SplitView.dragLine.resolvedStyle.top);
		if (flag && Math.Abs(value) + 1f <= num)
		{
			InterruptPointerMove(e);
			return;
		}
		Vector2 vector = e.localPosition - m_Start;
		float num2 = vector.x;
		if (orientation == TwoPaneSplitViewOrientation.Vertical)
		{
			num2 = vector.y;
		}
		float delta = (float)m_Direction * num2;
		ApplyDelta(delta);
		e.StopPropagation();
	}

	protected void OnPointerUp(PointerUpEvent e)
	{
		if (m_Active && base.target.HasPointerCapture(e.pointerId) && CanStopManipulation(e))
		{
			m_Active = false;
			base.target.ReleasePointer(e.pointerId);
			e.StopPropagation();
		}
	}

	protected void InterruptPointerMove(PointerMoveEvent e)
	{
		if (CanStopManipulation(e))
		{
			m_Active = false;
			base.target.ReleasePointer(e.pointerId);
			e.StopPropagation();
		}
	}
}
