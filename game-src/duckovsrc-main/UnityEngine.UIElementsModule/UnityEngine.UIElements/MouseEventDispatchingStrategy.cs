#define UNITY_ASSERTIONS
using UnityEngine.Assertions;

namespace UnityEngine.UIElements;

internal class MouseEventDispatchingStrategy : IEventDispatchingStrategy
{
	public bool CanDispatchEvent(EventBase evt)
	{
		return evt is IMouseEvent;
	}

	public void DispatchEvent(EventBase evt, IPanel iPanel)
	{
		if (iPanel != null)
		{
			Assert.IsTrue(iPanel is BaseVisualElementPanel);
			BaseVisualElementPanel panel = (BaseVisualElementPanel)iPanel;
			SetBestTargetForEvent(evt, panel);
			SendEventToTarget(evt, panel);
		}
		evt.stopDispatch = true;
	}

	private static bool SendEventToTarget(EventBase evt, BaseVisualElementPanel panel)
	{
		return SendEventToRegularTarget(evt, panel) || SendEventToIMGUIContainer(evt, panel);
	}

	private static bool SendEventToRegularTarget(EventBase evt, BaseVisualElementPanel panel)
	{
		if (!(evt.target is VisualElement visualElement))
		{
			return false;
		}
		if (visualElement.panel == panel)
		{
			EventDispatchUtilities.PropagateEvent(evt);
		}
		return IsDone(evt);
	}

	private static bool SendEventToIMGUIContainer(EventBase evt, BaseVisualElementPanel panel)
	{
		if (evt.imguiEvent == null)
		{
			return false;
		}
		IMGUIContainer rootIMGUIContainer = panel.rootIMGUIContainer;
		if (rootIMGUIContainer == null)
		{
			return false;
		}
		if (evt.propagateToIMGUI || evt.eventTypeId == EventBase<MouseEnterWindowEvent>.TypeId() || evt.eventTypeId == EventBase<MouseLeaveWindowEvent>.TypeId())
		{
			evt.skipElements.Add(evt.target);
			EventDispatchUtilities.PropagateToIMGUIContainer(panel.visualTree, evt);
		}
		return IsDone(evt);
	}

	private static void SetBestTargetForEvent(EventBase evt, BaseVisualElementPanel panel)
	{
		UpdateElementUnderMouse(evt, panel, out var elementUnderMouse);
		if (evt.target != null)
		{
			evt.propagateToIMGUI = false;
		}
		else if (elementUnderMouse != null)
		{
			evt.propagateToIMGUI = false;
			evt.target = elementUnderMouse;
		}
		else
		{
			evt.target = panel?.visualTree;
		}
	}

	private static void UpdateElementUnderMouse(EventBase evt, BaseVisualElementPanel panel, out VisualElement elementUnderMouse)
	{
		bool flag = (evt as IMouseEventInternal)?.recomputeTopElementUnderMouse ?? true;
		elementUnderMouse = (flag ? panel.RecomputeTopElementUnderPointer(PointerId.mousePointerId, ((IMouseEvent)evt).mousePosition, evt) : panel.GetTopElementUnderPointer(PointerId.mousePointerId));
		if (evt.eventTypeId == EventBase<MouseLeaveWindowEvent>.TypeId() && (evt as MouseLeaveWindowEvent).pressedButtons == 0)
		{
			panel.ClearCachedElementUnderPointer(PointerId.mousePointerId, evt);
		}
	}

	private static bool IsDone(EventBase evt)
	{
		Event imguiEvent = evt.imguiEvent;
		if (imguiEvent != null && imguiEvent.rawType == EventType.Used)
		{
			evt.StopPropagation();
		}
		return evt.isPropagationStopped;
	}
}
