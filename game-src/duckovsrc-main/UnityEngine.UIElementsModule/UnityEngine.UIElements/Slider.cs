using System;

namespace UnityEngine.UIElements;

public class Slider : BaseSlider<float>
{
	public new class UxmlFactory : UxmlFactory<Slider, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseSlider<float>.UxmlTraits
	{
		private UxmlFloatAttributeDescription m_LowValue = new UxmlFloatAttributeDescription
		{
			name = "low-value"
		};

		private UxmlFloatAttributeDescription m_HighValue = new UxmlFloatAttributeDescription
		{
			name = "high-value",
			defaultValue = 10f
		};

		private UxmlFloatAttributeDescription m_PageSize = new UxmlFloatAttributeDescription
		{
			name = "page-size",
			defaultValue = 0f
		};

		private UxmlBoolAttributeDescription m_ShowInputField = new UxmlBoolAttributeDescription
		{
			name = "show-input-field",
			defaultValue = false
		};

		private UxmlEnumAttributeDescription<SliderDirection> m_Direction = new UxmlEnumAttributeDescription<SliderDirection>
		{
			name = "direction",
			defaultValue = SliderDirection.Horizontal
		};

		private UxmlBoolAttributeDescription m_Inverted = new UxmlBoolAttributeDescription
		{
			name = "inverted",
			defaultValue = false
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			Slider slider = (Slider)ve;
			slider.lowValue = m_LowValue.GetValueFromBag(bag, cc);
			slider.highValue = m_HighValue.GetValueFromBag(bag, cc);
			slider.direction = m_Direction.GetValueFromBag(bag, cc);
			slider.pageSize = m_PageSize.GetValueFromBag(bag, cc);
			slider.showInputField = m_ShowInputField.GetValueFromBag(bag, cc);
			slider.inverted = m_Inverted.GetValueFromBag(bag, cc);
			base.Init(ve, bag, cc);
		}
	}

	internal const float kDefaultHighValue = 10f;

	public new static readonly string ussClassName = "unity-slider";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	public Slider()
		: this(null)
	{
	}

	public Slider(float start, float end, SliderDirection direction = SliderDirection.Horizontal, float pageSize = 0f)
		: this(null, start, end, direction, pageSize)
	{
	}

	public Slider(string label, float start = 0f, float end = 10f, SliderDirection direction = SliderDirection.Horizontal, float pageSize = 0f)
		: base(label, start, end, direction, pageSize)
	{
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
	}

	public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, float startValue)
	{
		double num = NumericFieldDraggerUtility.CalculateFloatDragSensitivity(startValue, base.lowValue, base.highValue);
		float acceleration = NumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
		double num2 = value;
		num2 += (double)NumericFieldDraggerUtility.NiceDelta(delta, acceleration) * num;
		value = (float)num2;
	}

	internal override float SliderLerpUnclamped(float a, float b, float interpolant)
	{
		float num = Mathf.LerpUnclamped(a, b, interpolant);
		float num2 = Mathf.Abs((base.highValue - base.lowValue) / (base.dragContainer.resolvedStyle.width - base.dragElement.resolvedStyle.width));
		int digits = ((num2 == 0f) ? Mathf.Clamp((int)(5.0 - (double)Mathf.Log10(Mathf.Abs(num2))), 0, 15) : Mathf.Clamp(-Mathf.FloorToInt(Mathf.Log10(Mathf.Abs(num2))), 0, 15));
		return (float)Math.Round(num, digits, MidpointRounding.AwayFromZero);
	}

	internal override float SliderNormalizeValue(float currentValue, float lowerValue, float higherValue)
	{
		return (currentValue - lowerValue) / (higherValue - lowerValue);
	}

	internal override float SliderRange()
	{
		return Math.Abs(base.highValue - base.lowValue);
	}

	internal override float ParseStringToValue(string previousValue, string newValue)
	{
		float num;
		ExpressionEvaluator.Expression expression;
		bool flag = UINumericFieldsUtils.TryConvertStringToFloat(newValue, previousValue, out num, out expression);
		expressionEvaluated?.Invoke(expression);
		return flag ? num : 0f;
	}

	internal override void ComputeValueFromKey(SliderKey sliderKey, bool isShift)
	{
		switch (sliderKey)
		{
		case SliderKey.None:
			return;
		case SliderKey.Lowest:
			value = base.lowValue;
			return;
		case SliderKey.Highest:
			value = base.highValue;
			return;
		}
		bool flag = sliderKey == SliderKey.LowerPage || sliderKey == SliderKey.HigherPage;
		float num = BaseSlider<float>.GetClosestPowerOfTen(Mathf.Abs((base.highValue - base.lowValue) * 0.01f));
		if (flag)
		{
			num *= pageSize;
		}
		else if (isShift)
		{
			num *= 10f;
		}
		if (sliderKey == SliderKey.Lower || sliderKey == SliderKey.LowerPage)
		{
			num = 0f - num;
		}
		value = BaseSlider<float>.RoundToMultipleOf(value + num * 0.5001f, Mathf.Abs(num));
	}
}
