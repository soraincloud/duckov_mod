using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public class RectIntField : BaseCompositeField<RectInt, IntegerField, int>
{
	public new class UxmlFactory : UxmlFactory<RectIntField, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseField<RectInt>.UxmlTraits
	{
		private UxmlIntAttributeDescription m_XValue = new UxmlIntAttributeDescription
		{
			name = "x"
		};

		private UxmlIntAttributeDescription m_YValue = new UxmlIntAttributeDescription
		{
			name = "y"
		};

		private UxmlIntAttributeDescription m_WValue = new UxmlIntAttributeDescription
		{
			name = "w"
		};

		private UxmlIntAttributeDescription m_HValue = new UxmlIntAttributeDescription
		{
			name = "h"
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			RectIntField rectIntField = (RectIntField)ve;
			rectIntField.SetValueWithoutNotify(new RectInt(m_XValue.GetValueFromBag(bag, cc), m_YValue.GetValueFromBag(bag, cc), m_WValue.GetValueFromBag(bag, cc), m_HValue.GetValueFromBag(bag, cc)));
		}
	}

	public new static readonly string ussClassName = "unity-rect-int-field";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	internal override FieldDescription[] DescribeFields()
	{
		return new FieldDescription[4]
		{
			new FieldDescription("X", "unity-x-input", (RectInt r) => r.x, delegate(ref RectInt r, int v)
			{
				r.x = v;
			}),
			new FieldDescription("Y", "unity-y-input", (RectInt r) => r.y, delegate(ref RectInt r, int v)
			{
				r.y = v;
			}),
			new FieldDescription("W", "unity-width-input", (RectInt r) => r.width, delegate(ref RectInt r, int v)
			{
				r.width = v;
			}),
			new FieldDescription("H", "unity-height-input", (RectInt r) => r.height, delegate(ref RectInt r, int v)
			{
				r.height = v;
			})
		};
	}

	public RectIntField()
		: this(null)
	{
	}

	public RectIntField(string label)
		: base(label, 2)
	{
		AddToClassList(ussClassName);
		AddToClassList(BaseCompositeField<RectInt, IntegerField, int>.twoLinesVariantUssClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
	}
}
