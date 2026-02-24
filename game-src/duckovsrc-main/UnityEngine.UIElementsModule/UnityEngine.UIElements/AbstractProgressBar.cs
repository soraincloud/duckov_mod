using System.Collections.Generic;

namespace UnityEngine.UIElements;

public abstract class AbstractProgressBar : BindableElement, INotifyValueChanged<float>
{
	public new class UxmlTraits : BindableElement.UxmlTraits
	{
		private UxmlFloatAttributeDescription m_LowValue = new UxmlFloatAttributeDescription
		{
			name = "low-value",
			defaultValue = 0f
		};

		private UxmlFloatAttributeDescription m_HighValue = new UxmlFloatAttributeDescription
		{
			name = "high-value",
			defaultValue = 100f
		};

		private UxmlFloatAttributeDescription m_Value = new UxmlFloatAttributeDescription
		{
			name = "value",
			defaultValue = 0f
		};

		private UxmlStringAttributeDescription m_Title = new UxmlStringAttributeDescription
		{
			name = "title",
			defaultValue = string.Empty
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			AbstractProgressBar abstractProgressBar = ve as AbstractProgressBar;
			abstractProgressBar.lowValue = m_LowValue.GetValueFromBag(bag, cc);
			abstractProgressBar.highValue = m_HighValue.GetValueFromBag(bag, cc);
			string valueFromBag = m_Title.GetValueFromBag(bag, cc);
			abstractProgressBar.title = (string.IsNullOrEmpty(valueFromBag) ? string.Empty : valueFromBag);
			abstractProgressBar.value = m_Value.GetValueFromBag(bag, cc);
		}
	}

	public static readonly string ussClassName = "unity-progress-bar";

	public static readonly string containerUssClassName = ussClassName + "__container";

	public static readonly string titleUssClassName = ussClassName + "__title";

	public static readonly string titleContainerUssClassName = ussClassName + "__title-container";

	public static readonly string progressUssClassName = ussClassName + "__progress";

	public static readonly string backgroundUssClassName = ussClassName + "__background";

	private readonly VisualElement m_Background;

	private readonly VisualElement m_Progress;

	private readonly Label m_Title;

	private float m_LowValue;

	private float m_HighValue = 100f;

	private float m_Value;

	private const float k_MinVisibleProgress = 1f;

	public string title
	{
		get
		{
			return m_Title.text;
		}
		set
		{
			m_Title.text = value;
		}
	}

	public float lowValue
	{
		get
		{
			return m_LowValue;
		}
		set
		{
			m_LowValue = value;
			SetProgress(m_Value);
		}
	}

	public float highValue
	{
		get
		{
			return m_HighValue;
		}
		set
		{
			m_HighValue = value;
			SetProgress(m_Value);
		}
	}

	public virtual float value
	{
		get
		{
			return m_Value;
		}
		set
		{
			if (EqualityComparer<float>.Default.Equals(m_Value, value))
			{
				return;
			}
			if (base.panel != null)
			{
				using (ChangeEvent<float> changeEvent = ChangeEvent<float>.GetPooled(m_Value, value))
				{
					changeEvent.target = this;
					SetValueWithoutNotify(value);
					SendEvent(changeEvent);
					return;
				}
			}
			SetValueWithoutNotify(value);
		}
	}

	public AbstractProgressBar()
	{
		AddToClassList(ussClassName);
		VisualElement visualElement = new VisualElement
		{
			name = ussClassName
		};
		m_Background = new VisualElement();
		m_Background.AddToClassList(backgroundUssClassName);
		visualElement.Add(m_Background);
		m_Progress = new VisualElement();
		m_Progress.AddToClassList(progressUssClassName);
		m_Background.Add(m_Progress);
		VisualElement visualElement2 = new VisualElement();
		visualElement2.AddToClassList(titleContainerUssClassName);
		m_Background.Add(visualElement2);
		m_Title = new Label();
		m_Title.AddToClassList(titleUssClassName);
		visualElement2.Add(m_Title);
		visualElement.AddToClassList(containerUssClassName);
		base.hierarchy.Add(visualElement);
		RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
	}

	private void OnGeometryChanged(GeometryChangedEvent e)
	{
		SetProgress(value);
	}

	public void SetValueWithoutNotify(float newValue)
	{
		m_Value = newValue;
		SetProgress(value);
	}

	private void SetProgress(float p)
	{
		float width = ((p < lowValue) ? lowValue : ((!(p > highValue)) ? p : highValue));
		width = CalculateProgressWidth(width);
		if (width >= 0f)
		{
			m_Progress.style.right = width;
		}
	}

	private float CalculateProgressWidth(float width)
	{
		if (m_Background == null || m_Progress == null)
		{
			return 0f;
		}
		if (float.IsNaN(m_Background.layout.width))
		{
			return 0f;
		}
		float num = m_Background.layout.width - 2f;
		return num - Mathf.Max(num * width / highValue, 1f);
	}
}
