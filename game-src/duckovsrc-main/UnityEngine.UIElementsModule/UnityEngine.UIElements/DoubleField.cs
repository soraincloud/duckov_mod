using System.Globalization;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public class DoubleField : TextValueField<double>
{
	public new class UxmlFactory : UxmlFactory<DoubleField, UxmlTraits>
	{
	}

	public new class UxmlTraits : TextValueFieldTraits<double, UxmlDoubleAttributeDescription>
	{
	}

	private class DoubleInput : TextValueInput
	{
		private DoubleField parentDoubleField => (DoubleField)base.parent;

		protected override string allowedCharacters => UINumericFieldsUtils.k_AllowedCharactersForFloat;

		internal DoubleInput()
		{
			base.formatString = UINumericFieldsUtils.k_DoubleFieldFormatString;
		}

		public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, double startValue)
		{
			double num = NumericFieldDraggerUtility.CalculateFloatDragSensitivity(startValue);
			float acceleration = NumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
			double num2 = StringToValue(base.text);
			num2 += (double)NumericFieldDraggerUtility.NiceDelta(delta, acceleration) * num;
			num2 = Mathf.RoundBasedOnMinimumDifference(num2, num);
			if (parentDoubleField.isDelayed)
			{
				base.text = ValueToString(num2);
			}
			else
			{
				parentDoubleField.value = num2;
			}
		}

		protected override string ValueToString(double v)
		{
			return v.ToString(base.formatString);
		}

		protected override double StringToValue(string str)
		{
			return parentDoubleField.StringToValue(str);
		}
	}

	public new static readonly string ussClassName = "unity-double-field";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	private DoubleInput doubleInput => (DoubleInput)base.textInputBase;

	protected override string ValueToString(double v)
	{
		return v.ToString(base.formatString, CultureInfo.InvariantCulture.NumberFormat);
	}

	protected override double StringToValue(string str)
	{
		double num;
		ExpressionEvaluator.Expression expression;
		bool flag = UINumericFieldsUtils.TryConvertStringToDouble(str, base.textInputBase.originalText, out num, out expression);
		expressionEvaluated?.Invoke(expression);
		return flag ? num : base.rawValue;
	}

	public DoubleField()
		: this(null)
	{
	}

	public DoubleField(int maxLength)
		: this(null, maxLength)
	{
	}

	public DoubleField(string label, int maxLength = 1000)
		: base(label, maxLength, (TextValueInput)new DoubleInput())
	{
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
		AddLabelDragger<double>();
	}

	internal override bool CanTryParse(string textString)
	{
		double result;
		return double.TryParse(textString, out result);
	}

	public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, double startValue)
	{
		doubleInput.ApplyInputDeviceDelta(delta, speed, startValue);
	}
}
