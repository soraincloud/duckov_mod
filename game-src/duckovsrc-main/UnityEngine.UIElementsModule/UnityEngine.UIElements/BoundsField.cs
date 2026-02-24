using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public class BoundsField : BaseField<Bounds>
{
	public new class UxmlFactory : UxmlFactory<BoundsField, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseField<Bounds>.UxmlTraits
	{
		private UxmlFloatAttributeDescription m_CenterXValue = new UxmlFloatAttributeDescription
		{
			name = "cx"
		};

		private UxmlFloatAttributeDescription m_CenterYValue = new UxmlFloatAttributeDescription
		{
			name = "cy"
		};

		private UxmlFloatAttributeDescription m_CenterZValue = new UxmlFloatAttributeDescription
		{
			name = "cz"
		};

		private UxmlFloatAttributeDescription m_ExtentsXValue = new UxmlFloatAttributeDescription
		{
			name = "ex"
		};

		private UxmlFloatAttributeDescription m_ExtentsYValue = new UxmlFloatAttributeDescription
		{
			name = "ey"
		};

		private UxmlFloatAttributeDescription m_ExtentsZValue = new UxmlFloatAttributeDescription
		{
			name = "ez"
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			BoundsField boundsField = (BoundsField)ve;
			boundsField.SetValueWithoutNotify(new Bounds(new Vector3(m_CenterXValue.GetValueFromBag(bag, cc), m_CenterYValue.GetValueFromBag(bag, cc), m_CenterZValue.GetValueFromBag(bag, cc)), new Vector3(m_ExtentsXValue.GetValueFromBag(bag, cc), m_ExtentsYValue.GetValueFromBag(bag, cc), m_ExtentsZValue.GetValueFromBag(bag, cc))));
		}
	}

	public new static readonly string ussClassName = "unity-bounds-field";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	public static readonly string centerFieldUssClassName = ussClassName + "__center-field";

	public static readonly string extentsFieldUssClassName = ussClassName + "__extents-field";

	private Vector3Field m_CenterField;

	private Vector3Field m_ExtentsField;

	public BoundsField()
		: this(null)
	{
	}

	public BoundsField(string label)
		: base(label, (VisualElement)null)
	{
		base.delegatesFocus = false;
		base.visualInput.focusable = false;
		AddToClassList(ussClassName);
		base.visualInput.AddToClassList(inputUssClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		m_CenterField = new Vector3Field("Center");
		m_CenterField.name = "unity-m_Center-input";
		m_CenterField.delegatesFocus = true;
		m_CenterField.AddToClassList(centerFieldUssClassName);
		m_CenterField.RegisterValueChangedCallback(delegate(ChangeEvent<Vector3> e)
		{
			Bounds bounds = value;
			bounds.center = e.newValue;
			value = bounds;
		});
		base.visualInput.hierarchy.Add(m_CenterField);
		m_ExtentsField = new Vector3Field("Extents");
		m_ExtentsField.name = "unity-m_Extent-input";
		m_ExtentsField.delegatesFocus = true;
		m_ExtentsField.AddToClassList(extentsFieldUssClassName);
		m_ExtentsField.RegisterValueChangedCallback(delegate(ChangeEvent<Vector3> e)
		{
			Bounds bounds = value;
			bounds.extents = e.newValue;
			value = bounds;
		});
		base.visualInput.hierarchy.Add(m_ExtentsField);
	}

	public override void SetValueWithoutNotify(Bounds newValue)
	{
		base.SetValueWithoutNotify(newValue);
		m_CenterField.SetValueWithoutNotify(base.rawValue.center);
		m_ExtentsField.SetValueWithoutNotify(base.rawValue.extents);
	}

	protected override void UpdateMixedValueContent()
	{
		m_CenterField.showMixedValue = base.showMixedValue;
		m_ExtentsField.showMixedValue = base.showMixedValue;
	}
}
