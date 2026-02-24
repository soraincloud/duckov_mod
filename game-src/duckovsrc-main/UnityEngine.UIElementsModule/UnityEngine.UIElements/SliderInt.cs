using System;

namespace UnityEngine.UIElements;

public class SliderInt : BaseSlider<int>
{
	public new class UxmlFactory : UxmlFactory<SliderInt, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseSlider<int>.UxmlTraits
	{
		private UxmlIntAttributeDescription m_LowValue = new UxmlIntAttributeDescription
		{
			name = "low-value"
		};

		private UxmlIntAttributeDescription m_HighValue = new UxmlIntAttributeDescription
		{
			name = "high-value",
			defaultValue = 10
		};

		private UxmlIntAttributeDescription m_PageSize = new UxmlIntAttributeDescription
		{
			name = "page-size",
			defaultValue = 0
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
			SliderInt sliderInt = (SliderInt)ve;
			sliderInt.lowValue = m_LowValue.GetValueFromBag(bag, cc);
			sliderInt.highValue = m_HighValue.GetValueFromBag(bag, cc);
			sliderInt.direction = m_Direction.GetValueFromBag(bag, cc);
			sliderInt.pageSize = m_PageSize.GetValueFromBag(bag, cc);
			sliderInt.showInputField = m_ShowInputField.GetValueFromBag(bag, cc);
			sliderInt.inverted = m_Inverted.GetValueFromBag(bag, cc);
			base.Init(ve, bag, cc);
		}
	}

	internal const int kDefaultHighValue = 10;

	public new static readonly string ussClassName = "unity-slider-int";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	public override float pageSize
	{
		get
		{
			return base.pageSize;
		}
		set
		{
			base.pageSize = Mathf.RoundToInt(value);
		}
	}

	public SliderInt()
		: this(null)
	{
	}

	public SliderInt(int start, int end, SliderDirection direction = SliderDirection.Horizontal, float pageSize = 0f)
		: this(null, start, end, direction, pageSize)
	{
	}

	public SliderInt(string label, int start = 0, int end = 10, SliderDirection direction = SliderDirection.Horizontal, float pageSize = 0f)
		: base(label, start, end, direction, pageSize)
	{
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
	}

	public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, int startValue)
	{
		double num = NumericFieldDraggerUtility.CalculateIntDragSensitivity(startValue, base.lowValue, base.highValue);
		float acceleration = NumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
		long num2 = value;
		num2 += (long)Math.Round((double)NumericFieldDraggerUtility.NiceDelta(delta, acceleration) * num);
		value = (int)num2;
	}

	internal override int SliderLerpUnclamped(int a, int b, float interpolant)
	{
		return Mathf.RoundToInt(Mathf.LerpUnclamped(a, b, interpolant));
	}

	internal override float SliderNormalizeValue(int currentValue, int lowerValue, int higherValue)
	{
		return ((float)currentValue - (float)lowerValue) / ((float)higherValue - (float)lowerValue);
	}

	internal override int SliderRange()
	{
		return Math.Abs(base.highValue - base.lowValue);
	}

	internal override int ParseStringToValue(string previousValue, string newValue)
	{
		int num;
		ExpressionEvaluator.Expression expression;
		bool flag = UINumericFieldsUtils.TryConvertStringToInt(newValue, previousValue, out num, out expression);
		expressionEvaluated?.Invoke(expression);
		return flag ? num : 0;
	}

	internal override void ComputeValueAndDirectionFromClick(float sliderLength, float dragElementLength, float dragElementPos, float dragElementLastPos)
	{
		if (Mathf.Approximately(pageSize, 0f))
		{
			base.ComputeValueAndDirectionFromClick(sliderLength, dragElementLength, dragElementPos, dragElementLastPos);
			return;
		}
		float f = sliderLength - dragElementLength;
		if (!(Mathf.Abs(f) < 1E-30f))
		{
			int num = (int)pageSize;
			if ((base.lowValue > base.highValue && !base.inverted) || (base.lowValue < base.highValue && base.inverted) || (base.direction == SliderDirection.Vertical && !base.inverted))
			{
				num = -num;
			}
			bool flag = dragElementLastPos < dragElementPos;
			bool flag2 = dragElementLastPos > dragElementPos + dragElementLength;
			bool flag3 = (base.inverted ? flag2 : flag);
			bool flag4 = (base.inverted ? flag : flag2);
			if (flag3 && base.clampedDragger.dragDirection != ClampedDragger<int>.DragDirection.LowToHigh)
			{
				base.clampedDragger.dragDirection = ClampedDragger<int>.DragDirection.HighToLow;
				value -= num;
			}
			else if (flag4 && base.clampedDragger.dragDirection != ClampedDragger<int>.DragDirection.HighToLow)
			{
				base.clampedDragger.dragDirection = ClampedDragger<int>.DragDirection.LowToHigh;
				value += num;
			}
		}
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
		float num = BaseSlider<int>.GetClosestPowerOfTen(Mathf.Abs((float)(base.highValue - base.lowValue) * 0.01f));
		if (num < 1f)
		{
			num = 1f;
		}
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
		value = Mathf.RoundToInt(BaseSlider<int>.RoundToMultipleOf((float)value + num * 0.5001f, Mathf.Abs(num)));
	}
}
