using System;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public abstract class BaseCompositeField<TValueType, TField, TFieldValue> : BaseField<TValueType> where TField : TextValueField<TFieldValue>, new()
{
	internal struct FieldDescription
	{
		public delegate void WriteDelegate(ref TValueType val, TFieldValue fieldValue);

		internal readonly string name;

		internal readonly string ussName;

		internal readonly Func<TValueType, TFieldValue> read;

		internal readonly WriteDelegate write;

		public FieldDescription(string name, string ussName, Func<TValueType, TFieldValue> read, WriteDelegate write)
		{
			this.name = name;
			this.ussName = ussName;
			this.read = read;
			this.write = write;
		}
	}

	private List<TField> m_Fields;

	private bool m_ShouldUpdateDisplay;

	private bool m_ForceUpdateDisplay;

	private int m_PropertyIndex;

	public new static readonly string ussClassName = "unity-composite-field";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	public static readonly string spacerUssClassName = ussClassName + "__field-spacer";

	public static readonly string multilineVariantUssClassName = ussClassName + "--multi-line";

	public static readonly string fieldGroupUssClassName = ussClassName + "__field-group";

	public static readonly string fieldUssClassName = ussClassName + "__field";

	public static readonly string firstFieldVariantUssClassName = fieldUssClassName + "--first";

	public static readonly string twoLinesVariantUssClassName = ussClassName + "--two-lines";

	internal List<TField> fields => m_Fields;

	internal int propertyIndex
	{
		get
		{
			return m_PropertyIndex;
		}
		set
		{
			m_PropertyIndex = value;
		}
	}

	internal bool forceUpdateDisplay
	{
		get
		{
			return m_ForceUpdateDisplay;
		}
		set
		{
			m_ForceUpdateDisplay = value;
		}
	}

	private VisualElement GetSpacer()
	{
		VisualElement visualElement = new VisualElement();
		visualElement.AddToClassList(spacerUssClassName);
		visualElement.visible = false;
		visualElement.focusable = false;
		return visualElement;
	}

	internal abstract FieldDescription[] DescribeFields();

	protected BaseCompositeField(string label, int fieldsByLine)
		: base(label, (VisualElement)null)
	{
		base.delegatesFocus = false;
		base.visualInput.focusable = false;
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
		m_ShouldUpdateDisplay = true;
		m_Fields = new List<TField>();
		FieldDescription[] array = DescribeFields();
		int num = 1;
		if (fieldsByLine > 1)
		{
			num = array.Length / fieldsByLine;
		}
		bool flag = false;
		if (num > 1)
		{
			flag = true;
			AddToClassList(multilineVariantUssClassName);
		}
		m_PropertyIndex = 0;
		for (int i = 0; i < num; i++)
		{
			VisualElement visualElement = null;
			if (flag)
			{
				visualElement = new VisualElement();
				visualElement.AddToClassList(fieldGroupUssClassName);
			}
			bool flag2 = true;
			for (int j = i * fieldsByLine; j < i * fieldsByLine + fieldsByLine; j++)
			{
				FieldDescription desc = array[j];
				TField field = new TField
				{
					name = desc.ussName
				};
				field.delegatesFocus = true;
				field.AddToClassList(fieldUssClassName);
				if (flag2)
				{
					field.AddToClassList(firstFieldVariantUssClassName);
					flag2 = false;
				}
				field.label = desc.name;
				field.onValidateValue += delegate(TFieldValue newValue)
				{
					TValueType val = value;
					desc.write(ref val, newValue);
					TValueType arg = ValidatedValue(val);
					return desc.read(arg);
				};
				field.RegisterValueChangedCallback(delegate(ChangeEvent<TFieldValue> e)
				{
					TValueType val = value;
					desc.write(ref val, e.newValue);
					string text = e.newValue.ToString();
					string text2 = ((TField)e.currentTarget).text;
					if (text != text2 || field.CanTryParse(text2))
					{
						m_ShouldUpdateDisplay = false;
					}
					value = val;
					m_ShouldUpdateDisplay = true;
				});
				m_Fields.Add(field);
				if (flag)
				{
					visualElement.Add(field);
				}
				else
				{
					base.visualInput.hierarchy.Add(field);
				}
			}
			if (fieldsByLine < 3)
			{
				int num2 = 3 - fieldsByLine;
				for (int num3 = 0; num3 < num2; num3++)
				{
					if (flag)
					{
						visualElement.Add(GetSpacer());
					}
					else
					{
						base.visualInput.hierarchy.Add(GetSpacer());
					}
				}
			}
			if (flag)
			{
				base.visualInput.hierarchy.Add(visualElement);
			}
		}
		UpdateDisplay();
	}

	private void UpdateDisplay()
	{
		if (m_Fields.Count != 0)
		{
			int num = 0;
			FieldDescription[] array = DescribeFields();
			FieldDescription[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				FieldDescription fieldDescription = array2[i];
				m_Fields[num].SetValueWithoutNotify(fieldDescription.read(base.rawValue));
				num++;
			}
		}
	}

	public override void SetValueWithoutNotify(TValueType newValue)
	{
		bool flag = m_ForceUpdateDisplay || (m_ShouldUpdateDisplay && !EqualityComparer<TValueType>.Default.Equals(base.rawValue, newValue));
		base.SetValueWithoutNotify(newValue);
		if (flag)
		{
			UpdateDisplay();
		}
		m_ForceUpdateDisplay = false;
	}

	internal override void OnViewDataReady()
	{
		m_ForceUpdateDisplay = true;
		base.OnViewDataReady();
	}

	protected override void UpdateMixedValueContent()
	{
		foreach (TField field in m_Fields)
		{
			field.showMixedValue = base.showMixedValue;
		}
	}
}
