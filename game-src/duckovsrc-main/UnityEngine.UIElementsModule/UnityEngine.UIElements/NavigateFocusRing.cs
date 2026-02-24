namespace UnityEngine.UIElements;

internal class NavigateFocusRing : IFocusRing
{
	public class ChangeDirection : FocusChangeDirection
	{
		public ChangeDirection(int i)
			: base(i)
		{
		}
	}

	private struct FocusableHierarchyTraversal
	{
		public VisualElement currentFocusable;

		public Rect validRect;

		public bool firstPass;

		public ChangeDirection direction;

		private bool ValidateHierarchyTraversal(VisualElement v)
		{
			return IsActive(v) && v.worldBoundingBox.Overlaps(validRect);
		}

		private bool ValidateElement(VisualElement v)
		{
			return IsNavigable(v) && v.worldBound.Overlaps(validRect);
		}

		private int Order(VisualElement a, VisualElement b)
		{
			Rect worldBound = a.worldBound;
			Rect worldBound2 = b.worldBound;
			int num = StrictOrder(worldBound, worldBound2);
			return (num != 0) ? num : TieBreaker(worldBound, worldBound2);
		}

		private int StrictOrder(VisualElement a, VisualElement b)
		{
			return StrictOrder(a.worldBound, b.worldBound);
		}

		private int StrictOrder(Rect ra, Rect rb)
		{
			float num = 0f;
			if (direction == Up)
			{
				num = rb.yMax - ra.yMax;
			}
			else if (direction == Down)
			{
				num = ra.yMin - rb.yMin;
			}
			else if (direction == Left)
			{
				num = rb.xMax - ra.xMax;
			}
			else if (direction == Right)
			{
				num = ra.xMin - rb.xMin;
			}
			if (!Mathf.Approximately(num, 0f))
			{
				return (num > 0f) ? 1 : (-1);
			}
			return 0;
		}

		private int TieBreaker(Rect ra, Rect rb)
		{
			Rect worldBound = currentFocusable.worldBound;
			float num = (ra.min - worldBound.min).sqrMagnitude - (rb.min - worldBound.min).sqrMagnitude;
			if (!Mathf.Approximately(num, 0f))
			{
				return (num > 0f) ? 1 : (-1);
			}
			return 0;
		}

		public VisualElement GetBestOverall(VisualElement candidate, VisualElement bestSoFar = null)
		{
			if (!ValidateHierarchyTraversal(candidate))
			{
				return bestSoFar;
			}
			if (ValidateElement(candidate))
			{
				if ((!firstPass || StrictOrder(candidate, currentFocusable) > 0) && (bestSoFar == null || Order(bestSoFar, candidate) > 0))
				{
					bestSoFar = candidate;
				}
				return bestSoFar;
			}
			int childCount = candidate.hierarchy.childCount;
			for (int i = 0; i < childCount; i++)
			{
				VisualElement candidate2 = candidate.hierarchy[i];
				bestSoFar = GetBestOverall(candidate2, bestSoFar);
			}
			return bestSoFar;
		}
	}

	public static readonly ChangeDirection Left = new ChangeDirection(1);

	public static readonly ChangeDirection Right = new ChangeDirection(2);

	public static readonly ChangeDirection Up = new ChangeDirection(3);

	public static readonly ChangeDirection Down = new ChangeDirection(4);

	public static readonly FocusChangeDirection Next = VisualElementFocusChangeDirection.right;

	public static readonly FocusChangeDirection Previous = VisualElementFocusChangeDirection.left;

	private readonly VisualElement m_Root;

	private readonly VisualElementFocusRing m_Ring;

	private FocusController focusController => m_Root.focusController;

	public NavigateFocusRing(VisualElement root)
	{
		m_Root = root;
		m_Ring = new VisualElementFocusRing(root);
	}

	public FocusChangeDirection GetFocusChangeDirection(Focusable currentFocusable, EventBase e)
	{
		if (e.eventTypeId == EventBase<PointerDownEvent>.TypeId() && focusController.GetFocusableParentForPointerEvent(e.target as Focusable, out var effectiveTarget))
		{
			return VisualElementFocusChangeTarget.GetPooled(effectiveTarget);
		}
		if (e.eventTypeId == EventBase<NavigationMoveEvent>.TypeId())
		{
			switch (((NavigationMoveEvent)e).direction)
			{
			case NavigationMoveEvent.Direction.Left:
				return Left;
			case NavigationMoveEvent.Direction.Up:
				return Up;
			case NavigationMoveEvent.Direction.Right:
				return Right;
			case NavigationMoveEvent.Direction.Down:
				return Down;
			case NavigationMoveEvent.Direction.Next:
				return Next;
			case NavigationMoveEvent.Direction.Previous:
				return Previous;
			}
		}
		return FocusChangeDirection.none;
	}

	public virtual Focusable GetNextFocusable(Focusable currentFocusable, FocusChangeDirection direction)
	{
		if (direction == Up || direction == Down || direction == Right || direction == Left)
		{
			return GetNextFocusable2D(currentFocusable, (ChangeDirection)direction);
		}
		return m_Ring.GetNextFocusable(currentFocusable, direction);
	}

	private Focusable GetNextFocusable2D(Focusable currentFocusable, ChangeDirection direction)
	{
		VisualElement visualElement = currentFocusable as VisualElement;
		if (visualElement == null)
		{
			visualElement = m_Root;
		}
		Rect worldBoundingBox = m_Root.worldBoundingBox;
		Rect rect = new Rect(worldBoundingBox.position - Vector2.one, worldBoundingBox.size + Vector2.one * 2f);
		Rect worldBound = visualElement.worldBound;
		Rect validRect = new Rect(worldBound.position - Vector2.one, worldBound.size + Vector2.one * 2f);
		if (direction == Up)
		{
			validRect.yMin = rect.yMin;
		}
		else if (direction == Down)
		{
			validRect.yMax = rect.yMax;
		}
		else if (direction == Left)
		{
			validRect.xMin = rect.xMin;
		}
		else if (direction == Right)
		{
			validRect.xMax = rect.xMax;
		}
		Focusable bestOverall = new FocusableHierarchyTraversal
		{
			currentFocusable = visualElement,
			direction = direction,
			validRect = validRect,
			firstPass = true
		}.GetBestOverall(m_Root);
		if (bestOverall != null)
		{
			return bestOverall;
		}
		validRect = new Rect(worldBound.position - Vector2.one, worldBound.size + Vector2.one * 2f);
		if (direction == Down)
		{
			validRect.yMin = rect.yMin;
		}
		else if (direction == Up)
		{
			validRect.yMax = rect.yMax;
		}
		else if (direction == Right)
		{
			validRect.xMin = rect.xMin;
		}
		else if (direction == Left)
		{
			validRect.xMax = rect.xMax;
		}
		bestOverall = new FocusableHierarchyTraversal
		{
			currentFocusable = visualElement,
			direction = direction,
			validRect = validRect,
			firstPass = false
		}.GetBestOverall(m_Root);
		if (bestOverall != null)
		{
			return bestOverall;
		}
		return currentFocusable;
	}

	private static bool IsActive(VisualElement v)
	{
		return v.resolvedStyle.display != DisplayStyle.None && v.enabledInHierarchy;
	}

	private static bool IsNavigable(Focusable focusable)
	{
		return focusable.canGrabFocus && focusable.tabIndex >= 0 && !focusable.delegatesFocus && !focusable.excludeFromFocusRing;
	}
}
