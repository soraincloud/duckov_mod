namespace UnityEngine.UIElements;

[EventCategory(EventCategory.Navigation)]
public abstract class NavigationEventBase<T> : EventBase<T>, INavigationEvent where T : NavigationEventBase<T>, new()
{
	public EventModifiers modifiers { get; protected set; }

	public bool shiftKey => (modifiers & EventModifiers.Shift) != 0;

	public bool ctrlKey => (modifiers & EventModifiers.Control) != 0;

	public bool commandKey => (modifiers & EventModifiers.Command) != 0;

	public bool altKey => (modifiers & EventModifiers.Alt) != 0;

	public bool actionKey
	{
		get
		{
			if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
			{
				return commandKey;
			}
			return ctrlKey;
		}
	}

	NavigationDeviceType INavigationEvent.deviceType => deviceType;

	internal NavigationDeviceType deviceType { get; private set; }

	protected NavigationEventBase()
	{
		LocalInit();
	}

	protected override void Init()
	{
		base.Init();
		LocalInit();
	}

	private void LocalInit()
	{
		base.propagation = EventPropagation.Bubbles | EventPropagation.TricklesDown | EventPropagation.Cancellable | EventPropagation.SkipDisabledElements;
		base.propagateToIMGUI = false;
		modifiers = EventModifiers.None;
		deviceType = NavigationDeviceType.Unknown;
	}

	public static T GetPooled(EventModifiers modifiers = EventModifiers.None)
	{
		T val = EventBase<T>.GetPooled();
		val.modifiers = modifiers;
		val.deviceType = NavigationDeviceType.Unknown;
		return val;
	}

	internal static T GetPooled(NavigationDeviceType deviceType, EventModifiers modifiers = EventModifiers.None)
	{
		T val = EventBase<T>.GetPooled();
		val.modifiers = modifiers;
		val.deviceType = deviceType;
		return val;
	}
}
