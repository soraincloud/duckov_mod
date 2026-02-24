namespace UnityEngine.UIElements.Internal;

internal class ColumnResizer : PointerManipulator
{
	private Vector2 m_Start;

	protected bool m_Active;

	private bool m_Resizing;

	private MultiColumnCollectionHeader m_Header;

	private Column m_Column;

	private VisualElement m_PreviewElement;

	public ColumnLayout columnLayout { get; set; }

	public bool preview { get; set; }

	public ColumnResizer(Column column)
	{
		m_Column = column;
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
		base.target.RegisterCallback<KeyDownEvent>(OnKeyDown);
	}

	protected override void UnregisterCallbacksFromTarget()
	{
		base.target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
		base.target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
		base.target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
		base.target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
	}

	private void OnKeyDown(KeyDownEvent e)
	{
		if (e.keyCode == KeyCode.Escape && m_Resizing && preview)
		{
			EndDragResize(0f, cancelled: true);
		}
	}

	private void OnPointerDown(PointerDownEvent e)
	{
		if (m_Active)
		{
			e.StopImmediatePropagation();
		}
		else
		{
			if (!CanStartManipulation(e))
			{
				return;
			}
			VisualElement visualElement = e.currentTarget as VisualElement;
			m_Header = visualElement.GetFirstAncestorOfType<MultiColumnCollectionHeader>();
			preview = m_Column.collection.resizePreview;
			if (preview)
			{
				if (m_PreviewElement == null)
				{
					m_PreviewElement = new MultiColumnHeaderColumnResizePreview();
				}
				VisualElement visualElement2 = m_Header.GetFirstAncestorOfType<ScrollView>()?.parent ?? m_Header.parent;
				visualElement2.hierarchy.Add(m_PreviewElement);
			}
			columnLayout = m_Header.columnLayout;
			m_Start = visualElement.ChangeCoordinatesTo(m_Header, e.localPosition);
			BeginDragResize(m_Start.x);
			m_Active = true;
			base.target.CaptureMouse();
			e.StopPropagation();
		}
	}

	private void OnPointerMove(PointerMoveEvent e)
	{
		if (m_Active && base.target.HasPointerCapture(e.pointerId))
		{
			VisualElement src = e.currentTarget as VisualElement;
			DragResize(src.ChangeCoordinatesTo(m_Header, e.localPosition).x);
			e.StopPropagation();
		}
	}

	private void OnPointerUp(PointerUpEvent e)
	{
		if (m_Active && base.target.HasPointerCapture(e.pointerId) && CanStopManipulation(e))
		{
			VisualElement src = e.currentTarget as VisualElement;
			EndDragResize(src.ChangeCoordinatesTo(m_Header, e.localPosition).x, cancelled: false);
			m_Active = false;
			base.target.ReleasePointer(e.pointerId);
			e.StopPropagation();
		}
	}

	private void BeginDragResize(float pos)
	{
		m_Resizing = true;
		columnLayout?.BeginDragResize(m_Column, m_Start.x, preview);
		if (preview)
		{
			UpdatePreviewPosition();
		}
	}

	private void DragResize(float pos)
	{
		if (m_Resizing)
		{
			columnLayout?.DragResize(m_Column, pos);
			if (preview)
			{
				UpdatePreviewPosition();
			}
		}
	}

	private void UpdatePreviewPosition()
	{
		m_PreviewElement.style.left = columnLayout.GetDesiredPosition(m_Column) + columnLayout.GetDesiredWidth(m_Column);
	}

	private void EndDragResize(float pos, bool cancelled)
	{
		if (m_Resizing)
		{
			if (preview)
			{
				m_PreviewElement?.RemoveFromHierarchy();
				m_PreviewElement = null;
			}
			columnLayout?.EndDragResize(m_Column, cancelled);
			m_Resizing = false;
		}
	}
}
