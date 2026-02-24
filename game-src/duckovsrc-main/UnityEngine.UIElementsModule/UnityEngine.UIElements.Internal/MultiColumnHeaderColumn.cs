using System;

namespace UnityEngine.UIElements.Internal;

internal class MultiColumnHeaderColumn : VisualElement
{
	public static readonly string ussClassName = MultiColumnCollectionHeader.ussClassName + "__column";

	public static readonly string sortableUssClassName = ussClassName + "--sortable";

	public static readonly string sortedAscendingUssClassName = ussClassName + "--sorted-ascending";

	public static readonly string sortedDescendingUssClassName = ussClassName + "--sorted-descending";

	public static readonly string movingUssClassName = ussClassName + "--moving";

	public static readonly string contentContainerUssClassName = ussClassName + "__content-container";

	public static readonly string contentUssClassName = ussClassName + "__content";

	public static readonly string defaultContentUssClassName = ussClassName + "__default-content";

	public static readonly string hasIconUssClassName = contentUssClassName + "--has-icon";

	public static readonly string hasTitleUssClassName = contentUssClassName + "--has-title";

	public static readonly string titleUssClassName = ussClassName + "__title";

	public static readonly string iconElementName = "unity-multi-column-header-column-icon";

	public static readonly string titleElementName = "unity-multi-column-header-column-title";

	private static readonly string s_BoundVEPropertyName = "__bound";

	private static readonly string s_BindingCallbackVEPropertyName = "__binding-callback";

	private static readonly string s_UnbindingCallbackVEPropertyName = "__unbinding-callback";

	private static readonly string s_DestroyCallbackVEPropertyName = "__destroy-callback";

	private VisualElement m_ContentContainer;

	private VisualElement m_Content;

	private MultiColumnHeaderColumnSortIndicator m_SortIndicatorContainer;

	private IVisualElementScheduledItem m_ScheduledHeaderTemplateUpdate;

	public Clickable clickable { get; private set; }

	public ColumnMover mover { get; private set; }

	public string sortOrderLabel
	{
		get
		{
			return m_SortIndicatorContainer.sortOrderLabel;
		}
		set
		{
			m_SortIndicatorContainer.sortOrderLabel = value;
		}
	}

	public Column column { get; private set; }

	internal Label title => content?.Q<Label>(titleElementName);

	public VisualElement content
	{
		get
		{
			return m_Content;
		}
		set
		{
			if (m_Content != null)
			{
				if (m_Content.parent == m_ContentContainer)
				{
					m_Content.RemoveFromHierarchy();
				}
				DestroyHeaderContent();
				m_Content = null;
			}
			m_Content = value;
			if (m_Content != null)
			{
				m_Content.AddToClassList(contentUssClassName);
				m_ContentContainer.Add(m_Content);
			}
		}
	}

	private bool isContentBound
	{
		get
		{
			return m_Content != null && (bool)m_Content.GetProperty(s_BoundVEPropertyName);
		}
		set
		{
			m_Content?.SetProperty(s_BoundVEPropertyName, value);
		}
	}

	public MultiColumnHeaderColumn()
		: this(new Column())
	{
	}

	public MultiColumnHeaderColumn(Column column)
	{
		this.column = column;
		this.column.changed += OnColumnChanged;
		this.column.resized += OnColumnResized;
		AddToClassList(ussClassName);
		base.style.marginLeft = 0f;
		base.style.marginTop = 0f;
		base.style.marginRight = 0f;
		base.style.marginBottom = 0f;
		base.style.paddingLeft = 0f;
		base.style.paddingTop = 0f;
		base.style.paddingRight = 0f;
		base.style.paddingBottom = 0f;
		Add(m_SortIndicatorContainer = new MultiColumnHeaderColumnSortIndicator());
		m_ContentContainer = new VisualElement();
		m_ContentContainer.style.flexGrow = 1f;
		m_ContentContainer.style.flexShrink = 1f;
		m_ContentContainer.AddToClassList(contentContainerUssClassName);
		Add(m_ContentContainer);
		UpdateHeaderTemplate();
		UpdateGeometryFromColumn();
		InitManipulators();
	}

	private void OnColumnChanged(Column c, ColumnDataType role)
	{
		if (column == c)
		{
			if (role == ColumnDataType.HeaderTemplate)
			{
				m_ScheduledHeaderTemplateUpdate?.Pause();
				m_ScheduledHeaderTemplateUpdate = base.schedule.Execute(UpdateHeaderTemplate);
			}
			else
			{
				UpdateDataFromColumn();
			}
		}
	}

	private void OnColumnResized(Column c)
	{
		UpdateGeometryFromColumn();
	}

	private void InitManipulators()
	{
		ColumnMover manipulator = (mover = new ColumnMover());
		this.AddManipulator(manipulator);
		mover.movingChanged += OnMoverChanged;
		Clickable manipulator2 = (this.clickable = new Clickable((Action)null));
		this.AddManipulator(manipulator2);
		this.clickable.activators.Add(new ManipulatorActivationFilter
		{
			button = MouseButton.LeftMouse,
			modifiers = EventModifiers.Shift
		});
		EventModifiers modifiers = EventModifiers.Control;
		RuntimePlatform platform = Application.platform;
		if (platform == RuntimePlatform.OSXEditor || platform == RuntimePlatform.OSXPlayer)
		{
			modifiers = EventModifiers.Command;
		}
		this.clickable.activators.Add(new ManipulatorActivationFilter
		{
			button = MouseButton.LeftMouse,
			modifiers = modifiers
		});
	}

	private void OnMoverChanged(ColumnMover mv)
	{
		if (mover.moving)
		{
			AddToClassList(movingUssClassName);
		}
		else
		{
			RemoveFromClassList(movingUssClassName);
		}
	}

	private void UpdateDataFromColumn()
	{
		if (column != null)
		{
			base.name = column.name;
			UnbindHeaderContent();
			BindHeaderContent();
		}
	}

	private void BindHeaderContent()
	{
		if (!isContentBound)
		{
			if (content.GetProperty(s_BindingCallbackVEPropertyName) is Action<VisualElement> action)
			{
				action(content);
			}
			isContentBound = true;
		}
	}

	private void UnbindHeaderContent()
	{
		if (isContentBound)
		{
			if (content.GetProperty(s_UnbindingCallbackVEPropertyName) is Action<VisualElement> action)
			{
				action(content);
			}
			isContentBound = false;
		}
	}

	private void DestroyHeaderContent()
	{
		UnbindHeaderContent();
		Action<VisualElement> action = content.GetProperty(s_DestroyCallbackVEPropertyName) as Action<VisualElement>;
		content.SetProperty(s_BindingCallbackVEPropertyName, null);
		content.SetProperty(s_UnbindingCallbackVEPropertyName, null);
		content.SetProperty(s_DestroyCallbackVEPropertyName, null);
		content.SetProperty(s_BoundVEPropertyName, null);
		action?.Invoke(content);
	}

	private VisualElement CreateDefaultHeaderContent()
	{
		VisualElement visualElement = new VisualElement
		{
			pickingMode = PickingMode.Ignore
		};
		visualElement.AddToClassList(defaultContentUssClassName);
		MultiColumnHeaderColumnIcon child = new MultiColumnHeaderColumnIcon
		{
			name = iconElementName,
			pickingMode = PickingMode.Ignore
		};
		Label label = new Label
		{
			name = titleElementName,
			pickingMode = PickingMode.Ignore
		};
		label.AddToClassList(titleUssClassName);
		visualElement.Add(child);
		visualElement.Add(label);
		return visualElement;
	}

	private void DefaultBindHeaderContent(VisualElement ve)
	{
		Label label = ve.Q<Label>(titleElementName);
		MultiColumnHeaderColumnIcon multiColumnHeaderColumnIcon = ve.Q<MultiColumnHeaderColumnIcon>();
		ve.RemoveFromClassList(hasTitleUssClassName);
		if (label != null)
		{
			label.text = column.title;
		}
		if (!string.IsNullOrEmpty(column.title))
		{
			ve.AddToClassList(hasTitleUssClassName);
		}
		if (multiColumnHeaderColumnIcon != null)
		{
			if (column.icon.texture != null || column.icon.sprite != null || column.icon.vectorImage != null)
			{
				multiColumnHeaderColumnIcon.isImageInline = true;
				multiColumnHeaderColumnIcon.image = column.icon.texture;
				multiColumnHeaderColumnIcon.sprite = column.icon.sprite;
				multiColumnHeaderColumnIcon.vectorImage = column.icon.vectorImage;
			}
			else if (multiColumnHeaderColumnIcon.isImageInline)
			{
				multiColumnHeaderColumnIcon.image = null;
				multiColumnHeaderColumnIcon.sprite = null;
				multiColumnHeaderColumnIcon.vectorImage = null;
			}
			multiColumnHeaderColumnIcon.UpdateClassList();
		}
	}

	private void UpdateHeaderTemplate()
	{
		if (column != null)
		{
			Func<VisualElement> func = column.makeHeader;
			Action<VisualElement> value = column.bindHeader;
			Action<VisualElement> value2 = column.unbindHeader;
			Action<VisualElement> value3 = column.destroyHeader;
			if (func == null)
			{
				func = CreateDefaultHeaderContent;
				value = DefaultBindHeaderContent;
				value2 = null;
				value3 = null;
			}
			content = func();
			content.SetProperty(s_BindingCallbackVEPropertyName, value);
			content.SetProperty(s_UnbindingCallbackVEPropertyName, value2);
			content.SetProperty(s_DestroyCallbackVEPropertyName, value3);
			isContentBound = false;
			m_ScheduledHeaderTemplateUpdate = null;
			UpdateDataFromColumn();
		}
	}

	private void UpdateGeometryFromColumn()
	{
		if (!float.IsNaN(column.desiredWidth))
		{
			base.style.width = column.desiredWidth;
		}
	}

	public void Dispose()
	{
		mover.movingChanged -= OnMoverChanged;
		column.changed -= OnColumnChanged;
		column.resized -= OnColumnResized;
		this.RemoveManipulator(mover);
		this.RemoveManipulator(clickable);
		mover = null;
		column = null;
		content = null;
	}
}
