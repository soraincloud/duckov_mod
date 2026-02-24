using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public class RectField : BaseCompositeField<Rect, FloatField, float>
{
	public new class UxmlFactory : UxmlFactory<RectField, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseField<Rect>.UxmlTraits
	{
		private UxmlFloatAttributeDescription m_XValue = new UxmlFloatAttributeDescription
		{
			name = "x"
		};

		private UxmlFloatAttributeDescription m_YValue = new UxmlFloatAttributeDescription
		{
			name = "y"
		};

		private UxmlFloatAttributeDescription m_WValue = new UxmlFloatAttributeDescription
		{
			name = "w"
		};

		private UxmlFloatAttributeDescription m_HValue = new UxmlFloatAttributeDescription
		{
			name = "h"
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			RectField rectField = (RectField)ve;
			rectField.SetValueWithoutNotify(new Rect(m_XValue.GetValueFromBag(bag, cc), m_YValue.GetValueFromBag(bag, cc), m_WValue.GetValueFromBag(bag, cc), m_HValue.GetValueFromBag(bag, cc)));
		}
	}

	public new static readonly string ussClassName = "unity-rect-field";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	internal override FieldDescription[] DescribeFields()
	{
		return new FieldDescription[4]
		{
			new FieldDescription("X", "unity-x-input", (Rect r) => r.x, delegate(ref Rect r, float v)
			{
				r.x = v;
			}),
			new FieldDescription("Y", "unity-y-input", (Rect r) => r.y, delegate(ref Rect r, float v)
			{
				r.y = v;
			}),
			new FieldDescription("W", "unity-width-input", (Rect r) => r.width, delegate(ref Rect r, float v)
			{
				r.width = v;
			}),
			new FieldDescription("H", "unity-height-input", (Rect r) => r.height, delegate(ref Rect r, float v)
			{
				r.height = v;
			})
		};
	}

	public RectField()
		: this(null)
	{
	}

	public RectField(string label)
		: base(label, 2)
	{
		AddToClassList(ussClassName);
		AddToClassList(BaseCompositeField<Rect, FloatField, float>.twoLinesVariantUssClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
	}
}
