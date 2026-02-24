using System;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public class EnumField : BaseField<Enum>
{
	public new class UxmlFactory : UxmlFactory<EnumField, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseField<Enum>.UxmlTraits
	{
		private UxmlTypeAttributeDescription<Enum> m_Type = EnumFieldHelpers.type;

		private UxmlStringAttributeDescription m_Value = EnumFieldHelpers.value;

		private UxmlBoolAttributeDescription m_IncludeObsoleteValues = EnumFieldHelpers.includeObsoleteValues;

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			if (EnumFieldHelpers.ExtractValue(bag, cc, out var resEnumType, out var resEnumValue, out var resIncludeObsoleteValues))
			{
				EnumField enumField = (EnumField)ve;
				enumField.Init(resEnumValue, resIncludeObsoleteValues);
			}
			else if (null != resEnumType)
			{
				EnumField enumField2 = (EnumField)ve;
				enumField2.m_EnumType = resEnumType;
				if (enumField2.m_EnumType != null)
				{
					enumField2.PopulateDataFromType(enumField2.m_EnumType);
				}
				enumField2.value = null;
			}
			else
			{
				EnumField enumField3 = (EnumField)ve;
				enumField3.m_EnumType = null;
				enumField3.value = null;
			}
		}
	}

	private Type m_EnumType;

	private bool m_IncludeObsoleteValues;

	private TextElement m_TextElement;

	private VisualElement m_ArrowElement;

	private EnumData m_EnumData;

	internal Func<IGenericMenu> createMenuCallback;

	public new static readonly string ussClassName = "unity-enum-field";

	public static readonly string textUssClassName = ussClassName + "__text";

	public static readonly string arrowUssClassName = ussClassName + "__arrow";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	internal Type type => m_EnumType;

	internal bool includeObsoleteValues => m_IncludeObsoleteValues;

	public string text => m_TextElement.text;

	private void Initialize(Enum defaultValue)
	{
		m_TextElement = new TextElement();
		m_TextElement.AddToClassList(textUssClassName);
		m_TextElement.pickingMode = PickingMode.Ignore;
		base.visualInput.Add(m_TextElement);
		m_ArrowElement = new VisualElement();
		m_ArrowElement.AddToClassList(arrowUssClassName);
		m_ArrowElement.pickingMode = PickingMode.Ignore;
		base.visualInput.Add(m_ArrowElement);
		if (defaultValue != null)
		{
			Init(defaultValue);
		}
	}

	public EnumField()
		: this(null, null)
	{
	}

	public EnumField(Enum defaultValue)
		: this(null, defaultValue)
	{
	}

	public EnumField(string label, Enum defaultValue = null)
		: base(label, (VisualElement)null)
	{
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
		Initialize(defaultValue);
		RegisterCallback<PointerDownEvent>(OnPointerDownEvent);
		RegisterCallback<PointerMoveEvent>(OnPointerMoveEvent);
		RegisterCallback(delegate(MouseDownEvent e)
		{
			if (e.button == 0)
			{
				e.StopPropagation();
			}
		});
		RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
	}

	public void Init(Enum defaultValue)
	{
		Init(defaultValue, includeObsoleteValues: false);
	}

	public void Init(Enum defaultValue, bool includeObsoleteValues)
	{
		if (defaultValue == null)
		{
			throw new ArgumentNullException("defaultValue");
		}
		m_IncludeObsoleteValues = includeObsoleteValues;
		PopulateDataFromType(defaultValue.GetType());
		if (!object.Equals(base.rawValue, defaultValue))
		{
			SetValueWithoutNotify(defaultValue);
		}
		else
		{
			UpdateValueLabel(defaultValue);
		}
	}

	internal void PopulateDataFromType(Type enumType)
	{
		m_EnumType = enumType;
		m_EnumData = EnumDataUtility.GetCachedEnumData(m_EnumType, includeObsoleteValues ? EnumDataUtility.CachedType.IncludeObsoleteExceptErrors : EnumDataUtility.CachedType.ExcludeObsolete);
	}

	public override void SetValueWithoutNotify(Enum newValue)
	{
		if (!object.Equals(base.rawValue, newValue))
		{
			base.SetValueWithoutNotify(newValue);
			if (!(m_EnumType == null))
			{
				UpdateValueLabel(newValue);
			}
		}
	}

	private void UpdateValueLabel(Enum value)
	{
		int num = Array.IndexOf(m_EnumData.values, value);
		if ((num >= 0) & (num < m_EnumData.values.Length))
		{
			m_TextElement.text = m_EnumData.displayNames[num];
		}
		else
		{
			m_TextElement.text = string.Empty;
		}
	}

	private void OnPointerDownEvent(PointerDownEvent evt)
	{
		ProcessPointerDown(evt);
	}

	private void OnPointerMoveEvent(PointerMoveEvent evt)
	{
		if (evt.button == 0 && (evt.pressedButtons & 1) != 0)
		{
			ProcessPointerDown(evt);
		}
	}

	private bool ContainsPointer(int pointerId)
	{
		VisualElement topElementUnderPointer = base.elementPanel.GetTopElementUnderPointer(pointerId);
		return this == topElementUnderPointer || base.visualInput == topElementUnderPointer;
	}

	private void ProcessPointerDown<T>(PointerEventBase<T> evt) where T : PointerEventBase<T>, new()
	{
		if (evt.button == 0 && ContainsPointer(evt.pointerId))
		{
			ShowMenu();
			evt.StopPropagation();
		}
	}

	private void OnNavigationSubmit(NavigationSubmitEvent evt)
	{
		ShowMenu();
		evt.StopPropagation();
	}

	internal void ShowMenu()
	{
		if (m_EnumType == null)
		{
			return;
		}
		IGenericMenu genericMenu = ((createMenuCallback != null) ? createMenuCallback() : base.elementPanel.CreateMenu());
		int num = Array.IndexOf(m_EnumData.values, value);
		for (int i = 0; i < m_EnumData.values.Length; i++)
		{
			bool isChecked = num == i;
			genericMenu.AddItem(m_EnumData.displayNames[i], isChecked, delegate(object contentView)
			{
				ChangeValueFromMenu(contentView);
			}, m_EnumData.values[i]);
		}
		genericMenu.DropDown(base.visualInput.worldBound, this, anchored: true);
	}

	private void ChangeValueFromMenu(object menuItem)
	{
		value = menuItem as Enum;
	}

	protected override void UpdateMixedValueContent()
	{
		if (base.showMixedValue)
		{
			m_TextElement.text = BaseField<Enum>.mixedValueString;
		}
		else
		{
			UpdateValueLabel(value);
		}
		m_TextElement.EnableInClassList(labelUssClassName, base.showMixedValue);
		m_TextElement.EnableInClassList(BaseField<Enum>.mixedValueLabelUssClassName, base.showMixedValue);
	}
}
