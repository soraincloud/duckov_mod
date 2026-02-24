namespace UnityEngine.UIElements;

public class GroupBox : BindableElement, IGroupBox
{
	public new class UxmlFactory : UxmlFactory<GroupBox, UxmlTraits>
	{
	}

	public new class UxmlTraits : BindableElement.UxmlTraits
	{
		private UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription
		{
			name = "text"
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			((GroupBox)ve).text = m_Text.GetValueFromBag(bag, cc);
		}
	}

	public static readonly string ussClassName = "unity-group-box";

	public static readonly string labelUssClassName = ussClassName + "__label";

	private Label m_TitleLabel;

	internal Label titleLabel => m_TitleLabel;

	public string text
	{
		get
		{
			return m_TitleLabel?.text;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				if (m_TitleLabel == null)
				{
					m_TitleLabel = new Label(value);
					m_TitleLabel.AddToClassList(labelUssClassName);
					Insert(0, m_TitleLabel);
				}
				m_TitleLabel.text = value;
			}
			else if (m_TitleLabel != null)
			{
				m_TitleLabel.RemoveFromHierarchy();
				m_TitleLabel = null;
			}
		}
	}

	public GroupBox()
		: this(null)
	{
	}

	public GroupBox(string text)
	{
		AddToClassList(ussClassName);
		this.text = text;
	}

	void IGroupBox.OnOptionAdded(IGroupBoxOption option)
	{
	}

	void IGroupBox.OnOptionRemoved(IGroupBoxOption option)
	{
	}
}
