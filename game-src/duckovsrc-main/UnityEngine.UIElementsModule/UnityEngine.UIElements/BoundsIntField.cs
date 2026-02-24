using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public class BoundsIntField : BaseField<BoundsInt>
{
	public new class UxmlFactory : UxmlFactory<BoundsIntField, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseField<BoundsInt>.UxmlTraits
	{
		private UxmlIntAttributeDescription m_PositionXValue = new UxmlIntAttributeDescription
		{
			name = "px"
		};

		private UxmlIntAttributeDescription m_PositionYValue = new UxmlIntAttributeDescription
		{
			name = "py"
		};

		private UxmlIntAttributeDescription m_PositionZValue = new UxmlIntAttributeDescription
		{
			name = "pz"
		};

		private UxmlIntAttributeDescription m_SizeXValue = new UxmlIntAttributeDescription
		{
			name = "sx"
		};

		private UxmlIntAttributeDescription m_SizeYValue = new UxmlIntAttributeDescription
		{
			name = "sy"
		};

		private UxmlIntAttributeDescription m_SizeZValue = new UxmlIntAttributeDescription
		{
			name = "sz"
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			BoundsIntField boundsIntField = (BoundsIntField)ve;
			boundsIntField.SetValueWithoutNotify(new BoundsInt(new Vector3Int(m_PositionXValue.GetValueFromBag(bag, cc), m_PositionYValue.GetValueFromBag(bag, cc), m_PositionZValue.GetValueFromBag(bag, cc)), new Vector3Int(m_SizeXValue.GetValueFromBag(bag, cc), m_SizeYValue.GetValueFromBag(bag, cc), m_SizeZValue.GetValueFromBag(bag, cc))));
		}
	}

	private Vector3IntField m_PositionField;

	private Vector3IntField m_SizeField;

	public new static readonly string ussClassName = "unity-bounds-int-field";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	public static readonly string positionUssClassName = ussClassName + "__position-field";

	public static readonly string sizeUssClassName = ussClassName + "__size-field";

	public BoundsIntField()
		: this(null)
	{
	}

	public BoundsIntField(string label)
		: base(label, (VisualElement)null)
	{
		base.delegatesFocus = false;
		base.visualInput.focusable = false;
		AddToClassList(ussClassName);
		base.visualInput.AddToClassList(inputUssClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		m_PositionField = new Vector3IntField("Position");
		m_PositionField.name = "unity-m_Position-input";
		m_PositionField.delegatesFocus = true;
		m_PositionField.AddToClassList(positionUssClassName);
		m_PositionField.RegisterValueChangedCallback(delegate(ChangeEvent<Vector3Int> e)
		{
			BoundsInt boundsInt = value;
			boundsInt.position = e.newValue;
			value = boundsInt;
		});
		base.visualInput.hierarchy.Add(m_PositionField);
		m_SizeField = new Vector3IntField("Size");
		m_SizeField.name = "unity-m_Size-input";
		m_SizeField.delegatesFocus = true;
		m_SizeField.AddToClassList(sizeUssClassName);
		m_SizeField.RegisterValueChangedCallback(delegate(ChangeEvent<Vector3Int> e)
		{
			BoundsInt boundsInt = value;
			boundsInt.size = e.newValue;
			value = boundsInt;
		});
		base.visualInput.hierarchy.Add(m_SizeField);
	}

	public override void SetValueWithoutNotify(BoundsInt newValue)
	{
		base.SetValueWithoutNotify(newValue);
		m_PositionField.SetValueWithoutNotify(base.rawValue.position);
		m_SizeField.SetValueWithoutNotify(base.rawValue.size);
	}

	protected override void UpdateMixedValueContent()
	{
		m_PositionField.showMixedValue = base.showMixedValue;
		m_SizeField.showMixedValue = base.showMixedValue;
	}
}
