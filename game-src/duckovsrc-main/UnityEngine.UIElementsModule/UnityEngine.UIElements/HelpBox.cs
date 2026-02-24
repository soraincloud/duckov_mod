namespace UnityEngine.UIElements;

public class HelpBox : VisualElement
{
	public new class UxmlFactory : UxmlFactory<HelpBox, UxmlTraits>
	{
	}

	public new class UxmlTraits : VisualElement.UxmlTraits
	{
		private UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription
		{
			name = "text"
		};

		private UxmlEnumAttributeDescription<HelpBoxMessageType> m_MessageType = new UxmlEnumAttributeDescription<HelpBoxMessageType>
		{
			name = "message-type",
			defaultValue = HelpBoxMessageType.None
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			HelpBox helpBox = ve as HelpBox;
			helpBox.text = m_Text.GetValueFromBag(bag, cc);
			helpBox.messageType = m_MessageType.GetValueFromBag(bag, cc);
		}
	}

	public static readonly string ussClassName = "unity-help-box";

	public static readonly string labelUssClassName = ussClassName + "__label";

	public static readonly string iconUssClassName = ussClassName + "__icon";

	public static readonly string iconInfoUssClassName = iconUssClassName + "--info";

	public static readonly string iconwarningUssClassName = iconUssClassName + "--warning";

	public static readonly string iconErrorUssClassName = iconUssClassName + "--error";

	private HelpBoxMessageType m_HelpBoxMessageType;

	private VisualElement m_Icon;

	private string m_IconClass;

	private Label m_Label;

	public string text
	{
		get
		{
			return m_Label.text;
		}
		set
		{
			m_Label.text = value;
		}
	}

	public HelpBoxMessageType messageType
	{
		get
		{
			return m_HelpBoxMessageType;
		}
		set
		{
			if (value != m_HelpBoxMessageType)
			{
				m_HelpBoxMessageType = value;
				UpdateIcon(value);
			}
		}
	}

	public HelpBox()
		: this(string.Empty, HelpBoxMessageType.None)
	{
	}

	public HelpBox(string text, HelpBoxMessageType messageType)
	{
		AddToClassList(ussClassName);
		m_HelpBoxMessageType = messageType;
		m_Label = new Label(text);
		m_Label.AddToClassList(labelUssClassName);
		Add(m_Label);
		m_Icon = new VisualElement();
		m_Icon.AddToClassList(iconUssClassName);
		UpdateIcon(messageType);
	}

	private string GetIconClass(HelpBoxMessageType messageType)
	{
		return messageType switch
		{
			HelpBoxMessageType.Info => iconInfoUssClassName, 
			HelpBoxMessageType.Warning => iconwarningUssClassName, 
			HelpBoxMessageType.Error => iconErrorUssClassName, 
			_ => null, 
		};
	}

	private void UpdateIcon(HelpBoxMessageType messageType)
	{
		if (!string.IsNullOrEmpty(m_IconClass))
		{
			m_Icon.RemoveFromClassList(m_IconClass);
		}
		m_IconClass = GetIconClass(messageType);
		if (m_IconClass == null)
		{
			m_Icon.RemoveFromHierarchy();
			return;
		}
		m_Icon.AddToClassList(m_IconClass);
		if (m_Icon.parent == null)
		{
			Insert(0, m_Icon);
		}
	}
}
