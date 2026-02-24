using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class PropagationPaths
{
	[Flags]
	public enum Type
	{
		None = 0,
		TrickleDown = 1,
		BubbleUp = 2
	}

	private static readonly ObjectPool<PropagationPaths> s_Pool = new ObjectPool<PropagationPaths>(() => new PropagationPaths());

	public readonly List<VisualElement> trickleDownPath;

	public readonly List<VisualElement> targetElements;

	public readonly List<VisualElement> bubbleUpPath;

	private const int k_DefaultPropagationDepth = 16;

	private const int k_DefaultTargetCount = 4;

	public PropagationPaths()
	{
		trickleDownPath = new List<VisualElement>(16);
		targetElements = new List<VisualElement>(4);
		bubbleUpPath = new List<VisualElement>(16);
	}

	public PropagationPaths(PropagationPaths paths)
	{
		trickleDownPath = new List<VisualElement>(paths.trickleDownPath);
		targetElements = new List<VisualElement>(paths.targetElements);
		bubbleUpPath = new List<VisualElement>(paths.bubbleUpPath);
	}

	internal static PropagationPaths Copy(PropagationPaths paths)
	{
		PropagationPaths propagationPaths = s_Pool.Get();
		propagationPaths.trickleDownPath.AddRange(paths.trickleDownPath);
		propagationPaths.targetElements.AddRange(paths.targetElements);
		propagationPaths.bubbleUpPath.AddRange(paths.bubbleUpPath);
		return propagationPaths;
	}

	public static PropagationPaths Build(VisualElement elem, EventBase evt)
	{
		PropagationPaths propagationPaths = s_Pool.Get();
		EventCategory eventCategory = evt.eventCategory;
		if (elem.HasEventCallbacksOrDefaultActions(eventCategory))
		{
			propagationPaths.targetElements.Add(elem);
		}
		for (VisualElement nextParentWithEventCallback = elem.nextParentWithEventCallback; nextParentWithEventCallback != null; nextParentWithEventCallback = nextParentWithEventCallback.nextParentWithEventCallback)
		{
			if (nextParentWithEventCallback.isCompositeRoot && !evt.ignoreCompositeRoots)
			{
				if (nextParentWithEventCallback.HasEventCallbacksOrDefaultActions(eventCategory))
				{
					propagationPaths.targetElements.Add(nextParentWithEventCallback);
				}
			}
			else if (nextParentWithEventCallback.HasEventCallbacks(eventCategory))
			{
				if (evt.tricklesDown && nextParentWithEventCallback.HasTrickleDownHandlers())
				{
					propagationPaths.trickleDownPath.Add(nextParentWithEventCallback);
				}
				if (evt.bubbles && nextParentWithEventCallback.HasBubbleUpHandlers())
				{
					propagationPaths.bubbleUpPath.Add(nextParentWithEventCallback);
				}
			}
		}
		return propagationPaths;
	}

	public void Release()
	{
		bubbleUpPath.Clear();
		targetElements.Clear();
		trickleDownPath.Clear();
		s_Pool.Release(this);
	}
}
