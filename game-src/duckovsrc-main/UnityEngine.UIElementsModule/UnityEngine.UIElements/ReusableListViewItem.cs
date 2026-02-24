namespace UnityEngine.UIElements;

internal class ReusableListViewItem : ReusableCollectionItem
{
	private VisualElement m_Container;

	private VisualElement m_DragHandle;

	private VisualElement m_ItemContainer;

	public override VisualElement rootElement => m_Container ?? base.bindableElement;

	public void Init(VisualElement item, bool usesAnimatedDragger)
	{
		base.Init(item);
		VisualElement root = new VisualElement
		{
			name = BaseListView.reorderableItemUssClassName
		};
		UpdateHierarchy(root, base.bindableElement, usesAnimatedDragger);
	}

	protected void UpdateHierarchy(VisualElement root, VisualElement item, bool usesAnimatedDragger)
	{
		if (usesAnimatedDragger)
		{
			if (m_Container == null)
			{
				m_Container = root;
				m_Container.AddToClassList(BaseListView.reorderableItemUssClassName);
				m_DragHandle = new VisualElement
				{
					name = BaseListView.reorderableItemHandleUssClassName
				};
				m_DragHandle.AddToClassList(BaseListView.reorderableItemHandleUssClassName);
				VisualElement visualElement = new VisualElement
				{
					name = BaseListView.reorderableItemHandleBarUssClassName
				};
				visualElement.AddToClassList(BaseListView.reorderableItemHandleBarUssClassName);
				m_DragHandle.Add(visualElement);
				VisualElement visualElement2 = new VisualElement
				{
					name = BaseListView.reorderableItemHandleBarUssClassName
				};
				visualElement2.AddToClassList(BaseListView.reorderableItemHandleBarUssClassName);
				m_DragHandle.Add(visualElement2);
				m_ItemContainer = new VisualElement
				{
					name = BaseListView.reorderableItemContainerUssClassName
				};
				m_ItemContainer.AddToClassList(BaseListView.reorderableItemContainerUssClassName);
				m_ItemContainer.Add(item);
				m_Container.Add(m_DragHandle);
				m_Container.Add(m_ItemContainer);
			}
		}
		else if (m_Container != null)
		{
			m_Container.RemoveFromHierarchy();
			m_Container = null;
		}
	}

	public void UpdateDragHandle(bool needsDragHandle)
	{
		if (needsDragHandle)
		{
			if (m_DragHandle.parent == null)
			{
				rootElement.Insert(0, m_DragHandle);
				rootElement.AddToClassList(BaseListView.reorderableItemUssClassName);
			}
		}
		else if (m_DragHandle?.parent != null)
		{
			m_DragHandle.RemoveFromHierarchy();
			rootElement.RemoveFromClassList(BaseListView.reorderableItemUssClassName);
		}
	}

	public override void PreAttachElement()
	{
		base.PreAttachElement();
		rootElement.AddToClassList(BaseListView.itemUssClassName);
	}

	public override void DetachElement()
	{
		base.DetachElement();
		rootElement.RemoveFromClassList(BaseListView.itemUssClassName);
	}

	public override void SetDragGhost(bool dragGhost)
	{
		base.SetDragGhost(dragGhost);
		if (m_DragHandle != null)
		{
			m_DragHandle.EnableInClassList("unity-hidden", base.isDragGhost);
		}
	}
}
