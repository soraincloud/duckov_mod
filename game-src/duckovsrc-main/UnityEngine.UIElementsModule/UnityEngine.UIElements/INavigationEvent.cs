namespace UnityEngine.UIElements;

public interface INavigationEvent
{
	EventModifiers modifiers { get; }

	internal NavigationDeviceType deviceType { get; }

	bool shiftKey { get; }

	bool ctrlKey { get; }

	bool commandKey { get; }

	bool altKey { get; }

	bool actionKey { get; }
}
