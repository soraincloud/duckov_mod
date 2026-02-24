using System;
using System.Globalization;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public class LongField : TextValueField<long>
{
	public new class UxmlFactory : UxmlFactory<LongField, UxmlTraits>
	{
	}

	public new class UxmlTraits : TextValueFieldTraits<long, UxmlLongAttributeDescription>
	{
	}

	private class LongInput : TextValueInput
	{
		private LongField parentLongField => (LongField)base.parent;

		protected override string allowedCharacters => UINumericFieldsUtils.k_AllowedCharactersForInt;

		internal LongInput()
		{
			base.formatString = UINumericFieldsUtils.k_IntFieldFormatString;
		}

		public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, long startValue)
		{
			double num = NumericFieldDraggerUtility.CalculateIntDragSensitivity(startValue);
			float acceleration = NumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
			long value = StringToValue(base.text);
			long niceDelta = (long)Math.Round((double)NumericFieldDraggerUtility.NiceDelta(delta, acceleration) * num);
			value = ClampMinMaxLongValue(niceDelta, value);
			if (parentLongField.isDelayed)
			{
				base.text = ValueToString(value);
			}
			else
			{
				parentLongField.value = value;
			}
		}

		private long ClampMinMaxLongValue(long niceDelta, long value)
		{
			long num = Math.Abs(niceDelta);
			if (niceDelta > 0)
			{
				if (value > 0 && num > long.MaxValue - value)
				{
					return long.MaxValue;
				}
				return value + niceDelta;
			}
			if (value < 0 && value < long.MinValue + num)
			{
				return long.MinValue;
			}
			return value - num;
		}

		protected override string ValueToString(long v)
		{
			return v.ToString(base.formatString);
		}

		protected override long StringToValue(string str)
		{
			return parentLongField.StringToValue(str);
		}
	}

	public new static readonly string ussClassName = "unity-long-field";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	private LongInput longInput => (LongInput)base.textInputBase;

	protected override string ValueToString(long v)
	{
		return v.ToString(base.formatString, CultureInfo.InvariantCulture.NumberFormat);
	}

	protected override long StringToValue(string str)
	{
		long num;
		ExpressionEvaluator.Expression expression;
		bool flag = UINumericFieldsUtils.TryConvertStringToLong(str, base.textInputBase.originalText, out num, out expression);
		expressionEvaluated?.Invoke(expression);
		return flag ? num : base.rawValue;
	}

	public LongField()
		: this(null)
	{
	}

	public LongField(int maxLength)
		: this(null, maxLength)
	{
	}

	public LongField(string label, int maxLength = 1000)
		: base(label, maxLength, (TextValueInput)new LongInput())
	{
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
		AddLabelDragger<long>();
	}

	internal override bool CanTryParse(string textString)
	{
		long result;
		return long.TryParse(textString, out result);
	}

	public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, long startValue)
	{
		longInput.ApplyInputDeviceDelta(delta, speed, startValue);
	}
}
