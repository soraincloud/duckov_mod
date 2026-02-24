using System;

namespace UnityEngine.UIElements;

internal class ReusableTreeViewItem : ReusableCollectionItem
{
	private Toggle m_Toggle;

	private VisualElement m_Container;

	private VisualElement m_IndentElement;

	private VisualElement m_BindableContainer;

	private VisualElement m_Checkmark;

	private int m_Depth;

	private float m_IndentWidth;

	private EventCallback<PointerUpEvent> m_PointerUpCallback;

	private EventCallback<ChangeEvent<bool>> m_ToggleValueChangedCallback;

	private EventCallback<GeometryChangedEvent> m_ToggleGeometryChangedCallback;

	public override VisualElement rootElement => m_Container ?? base.bindableElement;

	internal float indentWidth => m_IndentWidth;

	public event Action<PointerUpEvent> onPointerUp;

	public event Action<ChangeEvent<bool>> onToggleValueChanged;

	public ReusableTreeViewItem()
	{
		m_PointerUpCallback = OnPointerUp;
		m_ToggleValueChangedCallback = OnToggleValueChanged;
		m_ToggleGeometryChangedCallback = OnToggleGeometryChanged;
	}

	public override void Init(VisualElement item)
	{
		base.Init(item);
		VisualElement visualElement = new VisualElement
		{
			name = BaseTreeView.itemUssClassName
		};
		visualElement.AddToClassList(BaseTreeView.itemUssClassName);
		InitExpandHierarchy(visualElement, item);
	}

	protected void InitExpandHierarchy(VisualElement root, VisualElement item)
	{
		m_Container = root;
		m_Container.style.flexDirection = FlexDirection.Row;
		VisualElement visualElement = new VisualElement();
		visualElement.name = BaseTreeView.itemIndentUssClassName;
		visualElement.style.flexDirection = FlexDirection.Row;
		m_IndentElement = visualElement;
		m_Container.hierarchy.Add(m_IndentElement);
		m_Toggle = new Toggle
		{
			name = BaseTreeView.itemToggleUssClassName,
			userData = this
		};
		m_Toggle.AddToClassList(Foldout.toggleUssClassName);
		m_Toggle.AddToClassList(BaseTreeView.itemToggleUssClassName);
		m_Toggle.visualInput.AddToClassList(Foldout.inputUssClassName);
		m_Checkmark = m_Toggle.visualInput.Q(null, Toggle.checkmarkUssClassName);
		m_Checkmark.AddToClassList(Foldout.checkmarkUssClassName);
		m_Container.hierarchy.Add(m_Toggle);
		VisualElement visualElement2 = new VisualElement();
		visualElement2.name = BaseTreeView.itemContentContainerUssClassName;
		visualElement2.style.flexGrow = 1f;
		m_BindableContainer = visualElement2;
		m_BindableContainer.AddToClassList(BaseTreeView.itemContentContainerUssClassName);
		m_Container.Add(m_BindableContainer);
		m_BindableContainer.Add(item);
	}

	public override void PreAttachElement()
	{
		base.PreAttachElement();
		rootElement.AddToClassList(BaseTreeView.itemUssClassName);
		m_Container?.RegisterCallback(m_PointerUpCallback);
		m_Toggle?.visualInput.Q(null, Toggle.checkmarkUssClassName).RegisterCallback(m_ToggleGeometryChangedCallback);
		m_Toggle?.RegisterValueChangedCallback(m_ToggleValueChangedCallback);
	}

	public override void DetachElement()
	{
		base.DetachElement();
		rootElement.RemoveFromClassList(BaseTreeView.itemUssClassName);
		m_Container?.UnregisterCallback(m_PointerUpCallback);
		m_Toggle?.visualInput.Q(null, Toggle.checkmarkUssClassName).UnregisterCallback(m_ToggleGeometryChangedCallback);
		m_Toggle?.UnregisterValueChangedCallback(m_ToggleValueChangedCallback);
	}

	public void Indent(int depth)
	{
		if (m_IndentElement != null)
		{
			m_Depth = depth;
			UpdateIndentLayout();
		}
	}

	public void SetExpandedWithoutNotify(bool expanded)
	{
		m_Toggle?.SetValueWithoutNotify(expanded);
	}

	public void SetToggleVisibility(bool visible)
	{
		if (m_Toggle != null)
		{
			m_Toggle.visible = visible;
		}
	}

	private void OnToggleGeometryChanged(GeometryChangedEvent evt)
	{
		float num = m_Checkmark.resolvedStyle.width + m_Checkmark.resolvedStyle.marginLeft + m_Checkmark.resolvedStyle.marginRight;
		if (!(Math.Abs(num - m_IndentWidth) < float.Epsilon))
		{
			m_IndentWidth = num;
			UpdateIndentLayout();
		}
	}

	private void UpdateIndentLayout()
	{
		m_IndentElement.style.width = m_IndentWidth * (float)m_Depth;
		m_IndentElement.EnableInClassList(BaseTreeView.itemIndentUssClassName, m_Depth > 0);
	}

	private void OnPointerUp(PointerUpEvent evt)
	{
		this.onPointerUp?.Invoke(evt);
	}

	private void OnToggleValueChanged(ChangeEvent<bool> evt)
	{
		this.onToggleValueChanged?.Invoke(evt);
	}
}
