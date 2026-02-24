using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public class Vector3Field : BaseCompositeField<Vector3, FloatField, float>
{
	public new class UxmlFactory : UxmlFactory<Vector3Field, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseField<Vector3>.UxmlTraits
	{
		private UxmlFloatAttributeDescription m_XValue = new UxmlFloatAttributeDescription
		{
			name = "x"
		};

		private UxmlFloatAttributeDescription m_YValue = new UxmlFloatAttributeDescription
		{
			name = "y"
		};

		private UxmlFloatAttributeDescription m_ZValue = new UxmlFloatAttributeDescription
		{
			name = "z"
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			Vector3Field vector3Field = (Vector3Field)ve;
			vector3Field.SetValueWithoutNotify(new Vector3(m_XValue.GetValueFromBag(bag, cc), m_YValue.GetValueFromBag(bag, cc), m_ZValue.GetValueFromBag(bag, cc)));
		}
	}

	public new static readonly string ussClassName = "unity-vector3-field";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	internal override FieldDescription[] DescribeFields()
	{
		return new FieldDescription[3]
		{
			new FieldDescription("X", "unity-x-input", (Vector3 r) => r.x, delegate(ref Vector3 r, float v)
			{
				r.x = v;
			}),
			new FieldDescription("Y", "unity-y-input", (Vector3 r) => r.y, delegate(ref Vector3 r, float v)
			{
				r.y = v;
			}),
			new FieldDescription("Z", "unity-z-input", (Vector3 r) => r.z, delegate(ref Vector3 r, float v)
			{
				r.z = v;
			})
		};
	}

	public Vector3Field()
		: this(null)
	{
	}

	public Vector3Field(string label)
		: base(label, 3)
	{
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
	}
}
