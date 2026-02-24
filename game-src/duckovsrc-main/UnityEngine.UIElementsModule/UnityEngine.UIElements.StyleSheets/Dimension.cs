using System;
using System.Globalization;

namespace UnityEngine.UIElements.StyleSheets;

[Serializable]
internal struct Dimension : IEquatable<Dimension>
{
	public enum Unit
	{
		Unitless,
		Pixel,
		Percent,
		Second,
		Millisecond,
		Degree,
		Gradian,
		Radian,
		Turn
	}

	public Unit unit;

	public float value;

	public Dimension(float value, Unit unit)
	{
		this.unit = unit;
		this.value = value;
	}

	public Length ToLength()
	{
		LengthUnit lengthUnit = ((unit == Unit.Percent) ? LengthUnit.Percent : LengthUnit.Pixel);
		return new Length(value, lengthUnit);
	}

	public TimeValue ToTime()
	{
		TimeUnit timeUnit = ((unit == Unit.Millisecond) ? TimeUnit.Millisecond : TimeUnit.Second);
		return new TimeValue(value, timeUnit);
	}

	public Angle ToAngle()
	{
		return unit switch
		{
			Unit.Degree => new Angle(value, AngleUnit.Degree), 
			Unit.Gradian => new Angle(value, AngleUnit.Gradian), 
			Unit.Radian => new Angle(value, AngleUnit.Radian), 
			Unit.Turn => new Angle(value, AngleUnit.Turn), 
			_ => new Angle(value, AngleUnit.Degree), 
		};
	}

	public static bool operator ==(Dimension lhs, Dimension rhs)
	{
		return lhs.value == rhs.value && lhs.unit == rhs.unit;
	}

	public static bool operator !=(Dimension lhs, Dimension rhs)
	{
		return !(lhs == rhs);
	}

	public bool Equals(Dimension other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Dimension dimension))
		{
			return false;
		}
		return dimension == this;
	}

	public override int GetHashCode()
	{
		int num = -799583767;
		num = num * -1521134295 + unit.GetHashCode();
		return num * -1521134295 + value.GetHashCode();
	}

	public override string ToString()
	{
		string text = string.Empty;
		switch (unit)
		{
		case Unit.Pixel:
			text = "px";
			break;
		case Unit.Percent:
			text = "%";
			break;
		case Unit.Second:
			text = "s";
			break;
		case Unit.Millisecond:
			text = "ms";
			break;
		case Unit.Degree:
			text = "deg";
			break;
		case Unit.Gradian:
			text = "grad";
			break;
		case Unit.Radian:
			text = "rad";
			break;
		case Unit.Turn:
			text = "turn";
			break;
		case Unit.Unitless:
			text = string.Empty;
			break;
		}
		return value.ToString(CultureInfo.InvariantCulture.NumberFormat) + text;
	}
}
