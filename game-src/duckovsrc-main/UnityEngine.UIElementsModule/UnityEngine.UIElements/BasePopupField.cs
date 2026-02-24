using System;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public abstract class BasePopupField<TValueType, TValueChoice> : BaseField<TValueType>
{
	private class PopupTextElement : TextElement
	{
		protected internal override Vector2 DoMeasure(float desiredWidth, MeasureMode widthMode, float desiredHeight, MeasureMode heightMode)
		{
			string text = this.text;
			if (string.IsNullOrEmpty(text))
			{
				text = " ";
			}
			return MeasureTextSize(text, desiredWidth, widthMode, desiredHeight, heightMode);
		}
	}

	internal List<TValueChoice> m_Choices;

	private TextElement m_TextElement;

	private VisualElement m_ArrowElement;

	internal Func<TValueChoice, string> m_FormatSelectedValueCallback;

	internal Func<TValueChoice, string> m_FormatListItemCallback;

	internal Func<IGenericMenu> createMenuCallback;

	public new static readonly string ussClassName = "unity-base-popup-field";

	public static readonly string textUssClassName = ussClassName + "__text";

	public static readonly string arrowUssClassName = ussClassName + "__arrow";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	protected TextElement textElement => m_TextElement;

	public virtual List<TValueChoice> choices
	{
		get
		{
			return m_Choices;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_Choices = value;
			SetValueWithoutNotify(base.rawValue);
		}
	}

	public string text => m_TextElement.text;

	internal abstract string GetValueToDisplay();

	internal abstract string GetListItemToDisplay(TValueType item);

	internal abstract void AddMenuItems(IGenericMenu menu);

	public override void SetValueWithoutNotify(TValueType newValue)
	{
		base.SetValueWithoutNotify(newValue);
		((INotifyValueChanged<string>)m_TextElement).SetValueWithoutNotify(GetValueToDisplay());
	}

	internal BasePopupField()
		: this((string)null)
	{
	}

	internal BasePopupField(string label)
		: base(label, (VisualElement)null)
	{
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		m_TextElement = new PopupTextElement
		{
			pickingMode = PickingMode.Ignore
		};
		m_TextElement.AddToClassList(textUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
		base.visualInput.Add(m_TextElement);
		m_ArrowElement = new VisualElement();
		m_ArrowElement.AddToClassList(arrowUssClassName);
		m_ArrowElement.pickingMode = PickingMode.Ignore;
		base.visualInput.Add(m_ArrowElement);
		choices = new List<TValueChoice>();
		RegisterCallback<PointerDownEvent>(OnPointerDownEvent);
		RegisterCallback<PointerUpEvent>(OnPointerUpEvent);
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

	private void OnPointerDownEvent(PointerDownEvent evt)
	{
		ProcessPointerDown(evt);
	}

	private void OnPointerUpEvent(PointerUpEvent evt)
	{
		if (evt.button == 0 && ContainsPointer(evt.pointerId))
		{
			evt.StopPropagation();
		}
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
			base.schedule.Execute(ShowMenu);
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
		IGenericMenu genericMenu = ((createMenuCallback != null) ? createMenuCallback() : base.elementPanel.CreateMenu());
		AddMenuItems(genericMenu);
		genericMenu.DropDown(base.visualInput.worldBound, this, anchored: true);
	}

	protected override void UpdateMixedValueContent()
	{
		if (base.showMixedValue)
		{
			((INotifyValueChanged<string>)m_TextElement).SetValueWithoutNotify(BaseField<TValueType>.mixedValueString);
		}
		textElement.EnableInClassList(BaseField<TValueType>.mixedValueLabelUssClassName, base.showMixedValue);
	}
}
