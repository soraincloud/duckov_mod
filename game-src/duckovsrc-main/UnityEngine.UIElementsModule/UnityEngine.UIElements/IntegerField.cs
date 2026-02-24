using System;
using System.Globalization;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public class IntegerField : TextValueField<int>
{
	public new class UxmlFactory : UxmlFactory<IntegerField, UxmlTraits>
	{
	}

	public new class UxmlTraits : TextValueFieldTraits<int, UxmlIntAttributeDescription>
	{
	}

	private class IntegerInput : TextValueInput
	{
		private IntegerField parentIntegerField => (IntegerField)base.parent;

		protected override string allowedCharacters => UINumericFieldsUtils.k_AllowedCharactersForInt;

		internal IntegerInput()
		{
			base.formatString = UINumericFieldsUtils.k_IntFieldFormatString;
		}

		public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, int startValue)
		{
			double num = NumericFieldDraggerUtility.CalculateIntDragSensitivity(startValue);
			float acceleration = NumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
			long num2 = StringToValue(base.text);
			num2 += (long)Math.Round((double)NumericFieldDraggerUtility.NiceDelta(delta, acceleration) * num);
			if (parentIntegerField.isDelayed)
			{
				base.text = ValueToString(Mathf.ClampToInt(num2));
			}
			else
			{
				parentIntegerField.value = Mathf.ClampToInt(num2);
			}
		}

		protected override string ValueToString(int v)
		{
			return v.ToString(base.formatString);
		}

		protected override int StringToValue(string str)
		{
			return parentIntegerField.StringToValue(str);
		}
	}

	public new static readonly string ussClassName = "unity-integer-field";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	private IntegerInput integerInput => (IntegerInput)base.textInputBase;

	protected override string ValueToString(int v)
	{
		return v.ToString(base.formatString, CultureInfo.InvariantCulture.NumberFormat);
	}

	protected override int StringToValue(string str)
	{
		int num;
		ExpressionEvaluator.Expression expression;
		bool flag = UINumericFieldsUtils.TryConvertStringToInt(str, base.textInputBase.originalText, out num, out expression);
		expressionEvaluated?.Invoke(expression);
		return flag ? num : base.rawValue;
	}

	public IntegerField()
		: this(null)
	{
	}

	public IntegerField(int maxLength)
		: this(null, maxLength)
	{
	}

	public IntegerField(string label, int maxLength = 1000)
		: base(label, maxLength, (TextValueInput)new IntegerInput())
	{
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
		AddLabelDragger<int>();
	}

	internal override bool CanTryParse(string textString)
	{
		int result;
		return int.TryParse(textString, out result);
	}

	public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, int startValue)
	{
		integerInput.ApplyInputDeviceDelta(delta, speed, startValue);
	}
}
