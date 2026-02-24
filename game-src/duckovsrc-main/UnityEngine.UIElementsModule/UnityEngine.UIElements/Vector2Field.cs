using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public class Vector2Field : BaseCompositeField<Vector2, FloatField, float>
{
	public new class UxmlFactory : UxmlFactory<Vector2Field, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseField<Vector2>.UxmlTraits
	{
		private UxmlFloatAttributeDescription m_XValue = new UxmlFloatAttributeDescription
		{
			name = "x"
		};

		private UxmlFloatAttributeDescription m_YValue = new UxmlFloatAttributeDescription
		{
			name = "y"
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			Vector2Field vector2Field = (Vector2Field)ve;
			vector2Field.SetValueWithoutNotify(new Vector2(m_XValue.GetValueFromBag(bag, cc), m_YValue.GetValueFromBag(bag, cc)));
		}
	}

	public new static readonly string ussClassName = "unity-vector2-field";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	internal override FieldDescription[] DescribeFields()
	{
		return new FieldDescription[2]
		{
			new FieldDescription("X", "unity-x-input", (Vector2 r) => r.x, delegate(ref Vector2 r, float v)
			{
				r.x = v;
			}),
			new FieldDescription("Y", "unity-y-input", (Vector2 r) => r.y, delegate(ref Vector2 r, float v)
			{
				r.y = v;
			})
		};
	}

	public Vector2Field()
		: this(null)
	{
	}

	public Vector2Field(string label)
		: base(label, 2)
	{
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
	}
}
