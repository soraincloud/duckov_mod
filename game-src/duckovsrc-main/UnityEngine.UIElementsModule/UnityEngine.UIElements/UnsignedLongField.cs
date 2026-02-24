using System;
using System.Globalization;

namespace UnityEngine.UIElements;

public class UnsignedLongField : TextValueField<ulong>
{
	public new class UxmlFactory : UxmlFactory<UnsignedLongField, UxmlTraits>
	{
	}

	public new class UxmlTraits : TextValueFieldTraits<ulong, UxmlUnsignedLongAttributeDescription>
	{
	}

	private class UnsignedLongInput : TextValueInput
	{
		private UnsignedLongField parentUnsignedLongField => (UnsignedLongField)base.parent;

		protected override string allowedCharacters => UINumericFieldsUtils.k_AllowedCharactersForInt;

		internal UnsignedLongInput()
		{
			base.formatString = UINumericFieldsUtils.k_IntFieldFormatString;
		}

		public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, ulong startValue)
		{
			double num = NumericFieldDraggerUtility.CalculateIntDragSensitivity(startValue);
			float acceleration = NumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
			ulong value = StringToValue(base.text);
			long niceDelta = (long)Math.Round((double)NumericFieldDraggerUtility.NiceDelta(delta, acceleration) * num);
			value = ClampToMinMaxULongValue(niceDelta, value);
			if (parentUnsignedLongField.isDelayed)
			{
				base.text = ValueToString(value);
			}
			else
			{
				parentUnsignedLongField.value = value;
			}
		}

		private ulong ClampToMinMaxULongValue(long niceDelta, ulong value)
		{
			ulong num = (ulong)Math.Abs(niceDelta);
			if (niceDelta > 0)
			{
				if (num > (ulong)(-1L - (long)value))
				{
					return ulong.MaxValue;
				}
				return value + num;
			}
			if (num > value)
			{
				return 0uL;
			}
			return value - num;
		}

		protected override string ValueToString(ulong v)
		{
			return v.ToString(base.formatString);
		}

		protected override ulong StringToValue(string str)
		{
			return parentUnsignedLongField.StringToValue(str);
		}
	}

	public new static readonly string ussClassName = "unity-unsigned-long-field";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	private UnsignedLongInput unsignedLongInput => (UnsignedLongInput)base.textInputBase;

	protected override string ValueToString(ulong v)
	{
		return v.ToString(base.formatString, CultureInfo.InvariantCulture.NumberFormat);
	}

	protected override ulong StringToValue(string str)
	{
		ulong num;
		ExpressionEvaluator.Expression expression;
		bool flag = UINumericFieldsUtils.TryConvertStringToULong(str, base.textInputBase.originalText, out num, out expression);
		expressionEvaluated?.Invoke(expression);
		return flag ? num : base.rawValue;
	}

	public UnsignedLongField()
		: this(null)
	{
	}

	public UnsignedLongField(int maxLength)
		: this(null, maxLength)
	{
	}

	public UnsignedLongField(string label, int maxLength = 1000)
		: base(label, maxLength, (TextValueInput)new UnsignedLongInput())
	{
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
		AddLabelDragger<ulong>();
	}

	internal override bool CanTryParse(string textString)
	{
		ulong result;
		return ulong.TryParse(textString, out result);
	}

	public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, ulong startValue)
	{
		unsignedLongInput.ApplyInputDeviceDelta(delta, speed, startValue);
	}
}
