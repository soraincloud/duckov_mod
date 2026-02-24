#define UNITY_ASSERTIONS
using System.Collections.Generic;
using UnityEngine.Pool;

namespace UnityEngine.UIElements;

internal static class EventDispatchUtilities
{
	public static void PropagateEvent(EventBase evt)
	{
		if (!(evt.target is VisualElement visualElement))
		{
			return;
		}
		Debug.Assert(!evt.dispatch, "Event is being dispatched recursively.");
		evt.dispatch = true;
		if (!evt.bubblesOrTricklesDown)
		{
			if (visualElement.HasEventCallbacksOrDefaultActionAtTarget(evt.eventCategory))
			{
				visualElement.HandleEventAtTargetPhase(evt);
			}
		}
		else if (visualElement.HasParentEventCallbacksOrDefaultActionAtTarget(evt.eventCategory))
		{
			HandleEventAcrossPropagationPath(evt);
		}
		evt.dispatch = false;
	}

	private static void HandleEventAcrossPropagationPath(EventBase evt)
	{
		VisualElement visualElement = (VisualElement)evt.leafTarget;
		PropagationPaths propagationPaths = (evt.path = PropagationPaths.Build(visualElement, evt));
		EventDebugger.LogPropagationPaths(evt, propagationPaths);
		IPanel panel = visualElement.panel;
		if (evt.tricklesDown)
		{
			evt.propagationPhase = PropagationPhase.TrickleDown;
			int num = propagationPaths.trickleDownPath.Count - 1;
			while (num >= 0 && !evt.isPropagationStopped)
			{
				VisualElement visualElement2 = propagationPaths.trickleDownPath[num];
				if (!evt.Skip(visualElement2) && visualElement2.panel == panel)
				{
					evt.currentTarget = visualElement2;
					evt.currentTarget.HandleEvent(evt);
				}
				num--;
			}
		}
		evt.propagationPhase = PropagationPhase.AtTarget;
		foreach (VisualElement targetElement in propagationPaths.targetElements)
		{
			if (!evt.Skip(targetElement) && targetElement.panel == panel)
			{
				evt.target = targetElement;
				evt.currentTarget = evt.target;
				evt.currentTarget.HandleEvent(evt);
			}
		}
		evt.propagationPhase = PropagationPhase.DefaultActionAtTarget;
		foreach (VisualElement targetElement2 in propagationPaths.targetElements)
		{
			if (!evt.Skip(targetElement2) && targetElement2.panel == panel)
			{
				evt.target = targetElement2;
				evt.currentTarget = evt.target;
				evt.currentTarget.HandleEvent(evt);
			}
		}
		evt.target = evt.leafTarget;
		if (evt.bubbles)
		{
			evt.propagationPhase = PropagationPhase.BubbleUp;
			foreach (VisualElement item in propagationPaths.bubbleUpPath)
			{
				if (!evt.Skip(item) && item.panel == panel)
				{
					evt.currentTarget = item;
					evt.currentTarget.HandleEvent(evt);
				}
			}
		}
		evt.propagationPhase = PropagationPhase.None;
		evt.currentTarget = null;
	}

	internal static void PropagateToIMGUIContainer(VisualElement root, EventBase evt)
	{
		if (evt.imguiEvent == null || root.elementPanel.contextType == ContextType.Player)
		{
			return;
		}
		if (root.isIMGUIContainer)
		{
			IMGUIContainer iMGUIContainer = root as IMGUIContainer;
			if (evt.Skip(iMGUIContainer))
			{
				return;
			}
			bool flag = (evt.target as Focusable)?.focusable ?? false;
			if (iMGUIContainer.SendEventToIMGUI(evt, !flag))
			{
				evt.StopPropagation();
				evt.PreventDefault();
			}
			if (evt.imguiEvent.rawType == EventType.Used)
			{
				Debug.Assert(evt.isPropagationStopped);
			}
		}
		if (root.imguiContainerDescendantCount <= 0)
		{
			return;
		}
		List<VisualElement> value;
		using (CollectionPool<List<VisualElement>, VisualElement>.Get(out value))
		{
			value.AddRange(root.hierarchy.children);
			foreach (VisualElement item in value)
			{
				if (item.hierarchy.parent == root)
				{
					PropagateToIMGUIContainer(item, evt);
					if (evt.isPropagationStopped)
					{
						break;
					}
				}
			}
		}
	}

	public static void ExecuteDefaultAction(EventBase evt)
	{
		if (evt.target is VisualElement visualElement && visualElement.HasDefaultAction(evt.eventCategory))
		{
			evt.dispatch = true;
			evt.currentTarget = evt.target;
			evt.propagationPhase = PropagationPhase.DefaultAction;
			evt.currentTarget.HandleEvent(evt);
			evt.propagationPhase = PropagationPhase.None;
			evt.currentTarget = null;
			evt.dispatch = false;
		}
	}
}
