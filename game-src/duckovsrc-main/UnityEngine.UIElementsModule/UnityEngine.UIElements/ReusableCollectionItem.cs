using System;
using UnityEngine.UIElements.Experimental;

namespace UnityEngine.UIElements;

internal class ReusableCollectionItem
{
	public const int UndefinedIndex = -1;

	protected EventCallback<GeometryChangedEvent> m_GeometryChangedEventCallback;

	public virtual VisualElement rootElement => bindableElement;

	public VisualElement bindableElement { get; protected set; }

	public ValueAnimation<StyleValues> animator { get; set; }

	public int index { get; set; }

	public int id { get; set; }

	internal bool isDragGhost { get; private set; }

	public event Action<ReusableCollectionItem> onGeometryChanged;

	internal event Action<ReusableCollectionItem> onDestroy;

	public ReusableCollectionItem()
	{
		index = (id = -1);
		m_GeometryChangedEventCallback = OnGeometryChanged;
	}

	public virtual void Init(VisualElement item)
	{
		bindableElement = item;
	}

	public virtual void PreAttachElement()
	{
		rootElement.AddToClassList(BaseVerticalCollectionView.itemUssClassName);
		rootElement.RegisterCallback(m_GeometryChangedEventCallback);
	}

	public virtual void DetachElement()
	{
		rootElement.RemoveFromClassList(BaseVerticalCollectionView.itemUssClassName);
		rootElement.UnregisterCallback(m_GeometryChangedEventCallback);
		rootElement?.RemoveFromHierarchy();
		SetSelected(selected: false);
		SetDragGhost(dragGhost: false);
		int num = (id = -1);
		index = num;
	}

	public virtual void DestroyElement()
	{
		this.onDestroy?.Invoke(this);
	}

	public virtual void SetSelected(bool selected)
	{
		if (selected)
		{
			rootElement.AddToClassList(BaseVerticalCollectionView.itemSelectedVariantUssClassName);
			rootElement.pseudoStates |= PseudoStates.Checked;
		}
		else
		{
			rootElement.RemoveFromClassList(BaseVerticalCollectionView.itemSelectedVariantUssClassName);
			rootElement.pseudoStates &= ~PseudoStates.Checked;
		}
	}

	public virtual void SetDragGhost(bool dragGhost)
	{
		isDragGhost = dragGhost;
		rootElement.style.maxHeight = ((!isDragGhost) ? StyleKeyword.Initial : StyleKeyword.Undefined);
		bindableElement.style.display = (isDragGhost ? DisplayStyle.None : DisplayStyle.Flex);
	}

	protected void OnGeometryChanged(GeometryChangedEvent evt)
	{
		this.onGeometryChanged?.Invoke(this);
	}
}
