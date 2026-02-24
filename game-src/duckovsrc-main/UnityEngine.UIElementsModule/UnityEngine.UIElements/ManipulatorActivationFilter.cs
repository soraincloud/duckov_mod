using System;

namespace UnityEngine.UIElements;

public struct ManipulatorActivationFilter : IEquatable<ManipulatorActivationFilter>
{
	public MouseButton button { get; set; }

	public EventModifiers modifiers { get; set; }

	public int clickCount { get; set; }

	public override bool Equals(object obj)
	{
		return obj is ManipulatorActivationFilter && Equals((ManipulatorActivationFilter)obj);
	}

	public bool Equals(ManipulatorActivationFilter other)
	{
		return button == other.button && modifiers == other.modifiers && clickCount == other.clickCount;
	}

	public override int GetHashCode()
	{
		int num = 390957112;
		num = num * -1521134295 + button.GetHashCode();
		num = num * -1521134295 + modifiers.GetHashCode();
		return num * -1521134295 + clickCount.GetHashCode();
	}

	public bool Matches(IMouseEvent e)
	{
		if (e == null)
		{
			return false;
		}
		bool flag = clickCount == 0 || e.clickCount >= clickCount;
		return button == (MouseButton)e.button && HasModifiers(e) && flag;
	}

	private bool HasModifiers(IMouseEvent e)
	{
		if (e == null)
		{
			return false;
		}
		return MatchModifiers(e.altKey, e.ctrlKey, e.shiftKey, e.commandKey);
	}

	public bool Matches(IPointerEvent e)
	{
		if (e == null)
		{
			return false;
		}
		bool flag = clickCount == 0 || e.clickCount >= clickCount;
		return button == (MouseButton)e.button && HasModifiers(e) && flag;
	}

	private bool HasModifiers(IPointerEvent e)
	{
		if (e == null)
		{
			return false;
		}
		return MatchModifiers(e.altKey, e.ctrlKey, e.shiftKey, e.commandKey);
	}

	private bool MatchModifiers(bool alt, bool ctrl, bool shift, bool command)
	{
		if (((modifiers & EventModifiers.Alt) != EventModifiers.None && !alt) || ((modifiers & EventModifiers.Alt) == 0 && alt))
		{
			return false;
		}
		if (((modifiers & EventModifiers.Control) != EventModifiers.None && !ctrl) || ((modifiers & EventModifiers.Control) == 0 && ctrl))
		{
			return false;
		}
		if (((modifiers & EventModifiers.Shift) != EventModifiers.None && !shift) || ((modifiers & EventModifiers.Shift) == 0 && shift))
		{
			return false;
		}
		return ((modifiers & EventModifiers.Command) == 0 || command) && ((modifiers & EventModifiers.Command) != EventModifiers.None || !command);
	}

	public static bool operator ==(ManipulatorActivationFilter filter1, ManipulatorActivationFilter filter2)
	{
		return filter1.Equals(filter2);
	}

	public static bool operator !=(ManipulatorActivationFilter filter1, ManipulatorActivationFilter filter2)
	{
		return !(filter1 == filter2);
	}
}
