using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

public class GenericDropdownMenu : IGenericMenu
{
	internal class MenuItem
	{
		public string name;

		public VisualElement element;

		public Action action;

		public Action<object> actionUserData;
	}

	public static readonly string ussClassName = "unity-base-dropdown";

	public static readonly string itemUssClassName = ussClassName + "__item";

	public static readonly string labelUssClassName = ussClassName + "__label";

	public static readonly string containerInnerUssClassName = ussClassName + "__container-inner";

	public static readonly string containerOuterUssClassName = ussClassName + "__container-outer";

	public static readonly string checkmarkUssClassName = ussClassName + "__checkmark";

	public static readonly string separatorUssClassName = ussClassName + "__separator";

	private List<MenuItem> m_Items = new List<MenuItem>();

	private VisualElement m_MenuContainer;

	private VisualElement m_OuterContainer;

	private ScrollView m_ScrollView;

	private VisualElement m_PanelRootVisualContainer;

	private VisualElement m_TargetElement;

	private Rect m_DesiredRect;

	private KeyboardNavigationManipulator m_NavigationManipulator;

	private Vector2 m_MousePosition;

	internal List<MenuItem> items => m_Items;

	internal VisualElement menuContainer => m_MenuContainer;

	internal VisualElement outerContainer => m_OuterContainer;

	internal ScrollView scrollView => m_ScrollView;

	internal bool isSingleSelectionDropdown { get; set; }

	internal bool closeOnParentResize { get; set; }

	public VisualElement contentContainer => m_ScrollView.contentContainer;

	public GenericDropdownMenu()
	{
		m_MenuContainer = new VisualElement();
		m_MenuContainer.AddToClassList(ussClassName);
		m_OuterContainer = new VisualElement();
		m_OuterContainer.AddToClassList(containerOuterUssClassName);
		m_MenuContainer.Add(m_OuterContainer);
		m_ScrollView = new ScrollView();
		m_ScrollView.AddToClassList(containerInnerUssClassName);
		m_ScrollView.pickingMode = PickingMode.Position;
		m_ScrollView.contentContainer.focusable = true;
		m_ScrollView.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;
		m_OuterContainer.hierarchy.Add(m_ScrollView);
		m_MenuContainer.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
		m_MenuContainer.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
		isSingleSelectionDropdown = true;
		closeOnParentResize = true;
	}

	private void OnAttachToPanel(AttachToPanelEvent evt)
	{
		if (evt.destinationPanel != null)
		{
			contentContainer.AddManipulator(m_NavigationManipulator = new KeyboardNavigationManipulator(Apply));
			m_MenuContainer.RegisterCallback<PointerDownEvent>(OnPointerDown);
			m_MenuContainer.RegisterCallback<PointerMoveEvent>(OnPointerMove);
			m_MenuContainer.RegisterCallback<PointerUpEvent>(OnPointerUp);
			evt.destinationPanel.visualTree.RegisterCallback<GeometryChangedEvent>(OnParentResized);
			m_ScrollView.RegisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
			m_ScrollView.RegisterCallback<FocusOutEvent>(OnFocusOut);
		}
	}

	private void OnDetachFromPanel(DetachFromPanelEvent evt)
	{
		if (evt.originPanel != null)
		{
			contentContainer.RemoveManipulator(m_NavigationManipulator);
			m_MenuContainer.UnregisterCallback<PointerDownEvent>(OnPointerDown);
			m_MenuContainer.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
			m_MenuContainer.UnregisterCallback<PointerUpEvent>(OnPointerUp);
			evt.originPanel.visualTree.UnregisterCallback<GeometryChangedEvent>(OnParentResized);
			m_ScrollView.UnregisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
			m_ScrollView.UnregisterCallback<FocusOutEvent>(OnFocusOut);
		}
	}

	private void Hide(bool giveFocusBack = false)
	{
		m_MenuContainer.RemoveFromHierarchy();
		if (m_TargetElement != null)
		{
			m_TargetElement.pseudoStates ^= PseudoStates.Active;
			if (giveFocusBack && m_TargetElement.canGrabFocus)
			{
				m_TargetElement.Focus();
			}
		}
		m_TargetElement = null;
	}

	private void Apply(KeyboardNavigationOperation op, EventBase sourceEvent)
	{
		if (Apply(op))
		{
			sourceEvent.StopPropagation();
			sourceEvent.PreventDefault();
		}
	}

	private bool Apply(KeyboardNavigationOperation op)
	{
		int selectedIndex = GetSelectedIndex();
		switch (op)
		{
		case KeyboardNavigationOperation.Cancel:
			Hide(giveFocusBack: true);
			return true;
		case KeyboardNavigationOperation.Submit:
		{
			MenuItem menuItem = ((selectedIndex != -1) ? m_Items[selectedIndex] : null);
			if (selectedIndex >= 0 && menuItem.element.enabledSelf)
			{
				menuItem.action?.Invoke();
				menuItem.actionUserData?.Invoke(menuItem.element.userData);
			}
			Hide(giveFocusBack: true);
			return true;
		}
		case KeyboardNavigationOperation.Previous:
			UpdateSelectionUp((selectedIndex < 0) ? (m_Items.Count - 1) : (selectedIndex - 1));
			return true;
		case KeyboardNavigationOperation.Next:
			UpdateSelectionDown(selectedIndex + 1);
			return true;
		case KeyboardNavigationOperation.PageUp:
		case KeyboardNavigationOperation.Begin:
			UpdateSelectionDown(0);
			return true;
		case KeyboardNavigationOperation.PageDown:
		case KeyboardNavigationOperation.End:
			UpdateSelectionUp(m_Items.Count - 1);
			return true;
		default:
			return false;
		}
		void UpdateSelectionDown(int newIndex)
		{
			while (newIndex < m_Items.Count)
			{
				if (m_Items[newIndex].element.enabledSelf)
				{
					ChangeSelectedIndex(newIndex, selectedIndex);
					break;
				}
				newIndex++;
			}
		}
		void UpdateSelectionUp(int newIndex)
		{
			while (newIndex >= 0)
			{
				if (m_Items[newIndex].element.enabledSelf)
				{
					ChangeSelectedIndex(newIndex, selectedIndex);
					break;
				}
				newIndex--;
			}
		}
	}

	private void OnPointerDown(PointerDownEvent evt)
	{
		m_MousePosition = m_ScrollView.WorldToLocal(evt.position);
		UpdateSelection(evt.target as VisualElement);
		if (evt.pointerId != PointerId.mousePointerId)
		{
			m_MenuContainer.panel.PreventCompatibilityMouseEvents(evt.pointerId);
		}
		evt.StopPropagation();
	}

	private void OnPointerMove(PointerMoveEvent evt)
	{
		m_MousePosition = m_ScrollView.WorldToLocal(evt.position);
		UpdateSelection(evt.target as VisualElement);
		if (evt.pointerId != PointerId.mousePointerId)
		{
			m_MenuContainer.panel.PreventCompatibilityMouseEvents(evt.pointerId);
		}
		evt.StopPropagation();
	}

	private void OnPointerUp(PointerUpEvent evt)
	{
		int selectedIndex = GetSelectedIndex();
		if (selectedIndex != -1)
		{
			MenuItem menuItem = m_Items[selectedIndex];
			menuItem.action?.Invoke();
			menuItem.actionUserData?.Invoke(menuItem.element.userData);
			if (isSingleSelectionDropdown)
			{
				Hide(giveFocusBack: true);
			}
		}
		if (evt.pointerId != PointerId.mousePointerId)
		{
			m_MenuContainer.panel.PreventCompatibilityMouseEvents(evt.pointerId);
		}
		evt.StopPropagation();
	}

	private void OnFocusOut(FocusOutEvent evt)
	{
		if (!m_ScrollView.ContainsPoint(m_MousePosition))
		{
			Hide();
		}
		else
		{
			m_MenuContainer.schedule.Execute(contentContainer.Focus);
		}
	}

	private void OnParentResized(GeometryChangedEvent evt)
	{
		if (closeOnParentResize)
		{
			Hide(giveFocusBack: true);
		}
	}

	private void UpdateSelection(VisualElement target)
	{
		if (!m_ScrollView.ContainsPoint(m_MousePosition))
		{
			int selectedIndex = GetSelectedIndex();
			if (selectedIndex >= 0)
			{
				m_Items[selectedIndex].element.pseudoStates &= ~PseudoStates.Hover;
			}
		}
		else if (target != null && (target.pseudoStates & PseudoStates.Hover) != PseudoStates.Hover)
		{
			int selectedIndex2 = GetSelectedIndex();
			if (selectedIndex2 >= 0)
			{
				m_Items[selectedIndex2].element.pseudoStates &= ~PseudoStates.Hover;
			}
			target.pseudoStates |= PseudoStates.Hover;
		}
	}

	private void ChangeSelectedIndex(int newIndex, int previousIndex)
	{
		if (previousIndex >= 0 && previousIndex < m_Items.Count)
		{
			m_Items[previousIndex].element.pseudoStates &= ~PseudoStates.Hover;
		}
		if (newIndex >= 0 && newIndex < m_Items.Count)
		{
			m_Items[newIndex].element.pseudoStates |= PseudoStates.Hover;
			m_ScrollView.ScrollTo(m_Items[newIndex].element);
		}
	}

	private int GetSelectedIndex()
	{
		for (int i = 0; i < m_Items.Count; i++)
		{
			if ((m_Items[i].element.pseudoStates & PseudoStates.Hover) == PseudoStates.Hover)
			{
				return i;
			}
		}
		return -1;
	}

	public void AddItem(string itemName, bool isChecked, Action action)
	{
		MenuItem menuItem = AddItem(itemName, isChecked, isEnabled: true);
		if (menuItem != null)
		{
			menuItem.action = action;
		}
	}

	public void AddItem(string itemName, bool isChecked, Action<object> action, object data)
	{
		MenuItem menuItem = AddItem(itemName, isChecked, isEnabled: true, data);
		if (menuItem != null)
		{
			menuItem.actionUserData = action;
		}
	}

	public void AddDisabledItem(string itemName, bool isChecked)
	{
		AddItem(itemName, isChecked, isEnabled: false);
	}

	public void AddSeparator(string path)
	{
		VisualElement visualElement = new VisualElement();
		visualElement.AddToClassList(separatorUssClassName);
		visualElement.pickingMode = PickingMode.Ignore;
		m_ScrollView.Add(visualElement);
	}

	private MenuItem AddItem(string itemName, bool isChecked, bool isEnabled, object data = null)
	{
		if (string.IsNullOrEmpty(itemName) || itemName.EndsWith("/"))
		{
			AddSeparator(itemName);
			return null;
		}
		for (int i = 0; i < m_Items.Count; i++)
		{
			if (itemName == m_Items[i].name)
			{
				return null;
			}
		}
		VisualElement visualElement = new VisualElement();
		visualElement.AddToClassList(itemUssClassName);
		visualElement.SetEnabled(isEnabled);
		visualElement.userData = data;
		VisualElement visualElement2 = new VisualElement();
		visualElement2.AddToClassList(checkmarkUssClassName);
		visualElement2.pickingMode = PickingMode.Ignore;
		visualElement.Add(visualElement2);
		if (isChecked)
		{
			visualElement.pseudoStates |= PseudoStates.Checked;
		}
		Label label = new Label(itemName);
		label.AddToClassList(labelUssClassName);
		label.pickingMode = PickingMode.Ignore;
		visualElement.Add(label);
		m_ScrollView.Add(visualElement);
		MenuItem menuItem = new MenuItem
		{
			name = itemName,
			element = visualElement
		};
		m_Items.Add(menuItem);
		return menuItem;
	}

	internal void UpdateItem(string itemName, bool isChecked)
	{
		MenuItem menuItem = m_Items.Find((MenuItem x) => x.name == itemName);
		if (menuItem != null)
		{
			if (isChecked)
			{
				menuItem.element.pseudoStates |= PseudoStates.Checked;
			}
			else
			{
				menuItem.element.pseudoStates &= ~PseudoStates.Checked;
			}
		}
	}

	public void DropDown(Rect position, VisualElement targetElement = null, bool anchored = false)
	{
		if (targetElement == null)
		{
			Debug.LogError("VisualElement Generic Menu needs a target to find a root to attach to.");
			return;
		}
		m_TargetElement = targetElement;
		m_TargetElement.RegisterCallback<DetachFromPanelEvent>(OnTargetElementDetachFromPanel);
		m_PanelRootVisualContainer = m_TargetElement.GetRootVisualContainer();
		if (m_PanelRootVisualContainer == null)
		{
			Debug.LogError("Could not find rootVisualContainer...");
			return;
		}
		m_PanelRootVisualContainer.Add(m_MenuContainer);
		m_MenuContainer.style.left = m_PanelRootVisualContainer.layout.x;
		m_MenuContainer.style.top = m_PanelRootVisualContainer.layout.y;
		m_MenuContainer.style.width = m_PanelRootVisualContainer.layout.width;
		m_MenuContainer.style.height = m_PanelRootVisualContainer.layout.height;
		m_MenuContainer.style.fontSize = m_TargetElement.computedStyle.fontSize;
		m_MenuContainer.style.unityFont = m_TargetElement.computedStyle.unityFont;
		m_MenuContainer.style.unityFontDefinition = m_TargetElement.computedStyle.unityFontDefinition;
		Rect rect = m_PanelRootVisualContainer.WorldToLocal(position);
		m_OuterContainer.style.left = rect.x - m_PanelRootVisualContainer.layout.x;
		m_OuterContainer.style.top = rect.y + position.height - m_PanelRootVisualContainer.layout.y;
		m_DesiredRect = (anchored ? position : Rect.zero);
		m_MenuContainer.schedule.Execute(contentContainer.Focus);
		EnsureVisibilityInParent();
		if (targetElement != null)
		{
			targetElement.pseudoStates |= PseudoStates.Active;
		}
	}

	private void OnTargetElementDetachFromPanel(DetachFromPanelEvent evt)
	{
		Hide();
	}

	private void OnContainerGeometryChanged(GeometryChangedEvent evt)
	{
		EnsureVisibilityInParent();
	}

	private void EnsureVisibilityInParent()
	{
		if (m_PanelRootVisualContainer != null && !float.IsNaN(m_OuterContainer.layout.width) && !float.IsNaN(m_OuterContainer.layout.height))
		{
			if (m_DesiredRect == Rect.zero)
			{
				float num = Mathf.Min(m_OuterContainer.layout.x, m_PanelRootVisualContainer.layout.width - m_OuterContainer.layout.width);
				float num2 = Mathf.Min(m_OuterContainer.layout.y, Mathf.Max(0f, m_PanelRootVisualContainer.layout.height - m_OuterContainer.layout.height));
				m_OuterContainer.style.left = num;
				m_OuterContainer.style.top = num2;
			}
			m_OuterContainer.style.height = Mathf.Min(m_MenuContainer.layout.height - m_MenuContainer.layout.y - m_OuterContainer.layout.y, m_ScrollView.layout.height + m_OuterContainer.resolvedStyle.borderBottomWidth + m_OuterContainer.resolvedStyle.borderTopWidth);
			if (m_DesiredRect != Rect.zero)
			{
				m_OuterContainer.style.width = m_DesiredRect.width;
			}
		}
	}
}
