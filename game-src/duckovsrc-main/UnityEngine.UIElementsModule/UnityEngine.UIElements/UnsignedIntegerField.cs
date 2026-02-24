using System;
using System.Globalization;

namespace UnityEngine.UIElements;

public class UnsignedIntegerField : TextValueField<uint>
{
	public new class UxmlFactory : UxmlFactory<UnsignedIntegerField, UxmlTraits>
	{
	}

	public new class UxmlTraits : TextValueFieldTraits<uint, UxmlUnsignedIntAttributeDescription>
	{
	}

	private class UnsignedIntegerInput : TextValueInput
	{
		private UnsignedIntegerField parentUnsignedIntegerField => (UnsignedIntegerField)base.parent;

		protected override string allowedCharacters => UINumericFieldsUtils.k_AllowedCharactersForInt;

		internal UnsignedIntegerInput()
		{
			base.formatString = UINumericFieldsUtils.k_IntFieldFormatString;
		}

		public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, uint startValue)
		{
			double num = NumericFieldDraggerUtility.CalculateIntDragSensitivity(startValue);
			float acceleration = NumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
			long num2 = StringToValue(base.text);
			num2 += (long)Math.Round((double)NumericFieldDraggerUtility.NiceDelta(delta, acceleration) * num);
			if (parentUnsignedIntegerField.isDelayed)
			{
				base.text = ValueToString(Mathf.ClampToUInt(num2));
			}
			else
			{
				parentUnsignedIntegerField.value = Mathf.ClampToUInt(num2);
			}
		}

		protected override string ValueToString(uint v)
		{
			return v.ToString(base.formatString);
		}

		protected override uint StringToValue(string str)
		{
			return parentUnsignedIntegerField.StringToValue(str);
		}
	}

	public new static readonly string ussClassName = "unity-unsigned-integer-field";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	private UnsignedIntegerInput integerInput => (UnsignedIntegerInput)base.textInputBase;

	protected override string ValueToString(uint v)
	{
		return v.ToString(base.formatString, CultureInfo.InvariantCulture.NumberFormat);
	}

	protected override uint StringToValue(string str)
	{
		uint num;
		ExpressionEvaluator.Expression expression;
		bool flag = UINumericFieldsUtils.TryConvertStringToUInt(str, base.textInputBase.originalText, out num, out expression);
		expressionEvaluated?.Invoke(expression);
		return flag ? num : base.rawValue;
	}

	public UnsignedIntegerField()
		: this(null)
	{
	}

	public UnsignedIntegerField(int maxLength)
		: this(null, maxLength)
	{
	}

	public UnsignedIntegerField(string label, int maxLength = 1000)
		: base(label, maxLength, (TextValueInput)new UnsignedIntegerInput())
	{
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
		AddLabelDragger<uint>();
	}

	internal override bool CanTryParse(string textString)
	{
		uint result;
		return uint.TryParse(textString, out result);
	}

	public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, uint startValue)
	{
		integerInput.ApplyInputDeviceDelta(delta, speed, startValue);
	}
}
