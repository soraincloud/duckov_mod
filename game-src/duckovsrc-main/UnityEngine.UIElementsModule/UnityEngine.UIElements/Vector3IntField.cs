using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public class Vector3IntField : BaseCompositeField<Vector3Int, IntegerField, int>
{
	public new class UxmlFactory : UxmlFactory<Vector3IntField, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseField<Vector3Int>.UxmlTraits
	{
		private UxmlIntAttributeDescription m_XValue = new UxmlIntAttributeDescription
		{
			name = "x"
		};

		private UxmlIntAttributeDescription m_YValue = new UxmlIntAttributeDescription
		{
			name = "y"
		};

		private UxmlIntAttributeDescription m_ZValue = new UxmlIntAttributeDescription
		{
			name = "z"
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			Vector3IntField vector3IntField = (Vector3IntField)ve;
			vector3IntField.SetValueWithoutNotify(new Vector3Int(m_XValue.GetValueFromBag(bag, cc), m_YValue.GetValueFromBag(bag, cc), m_ZValue.GetValueFromBag(bag, cc)));
		}
	}

	public new static readonly string ussClassName = "unity-vector3-int-field";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	internal override FieldDescription[] DescribeFields()
	{
		return new FieldDescription[3]
		{
			new FieldDescription("X", "unity-x-input", (Vector3Int r) => r.x, delegate(ref Vector3Int r, int v)
			{
				r.x = v;
			}),
			new FieldDescription("Y", "unity-y-input", (Vector3Int r) => r.y, delegate(ref Vector3Int r, int v)
			{
				r.y = v;
			}),
			new FieldDescription("Z", "unity-z-input", (Vector3Int r) => r.z, delegate(ref Vector3Int r, int v)
			{
				r.z = v;
			})
		};
	}

	public Vector3IntField()
		: this(null)
	{
	}

	public Vector3IntField(string label)
		: base(label, 3)
	{
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
	}
}
