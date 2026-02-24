#define UNITY_ASSERTIONS
using System;

namespace UnityEngine.UIElements;

internal static class PointerDeviceState
{
	[Flags]
	internal enum LocationFlag
	{
		None = 0,
		OutsidePanel = 1
	}

	private struct PointerLocation
	{
		internal Vector2 Position { get; private set; }

		internal IPanel Panel { get; private set; }

		internal LocationFlag Flags { get; private set; }

		internal void SetLocation(Vector2 position, IPanel panel)
		{
			Position = position;
			Panel = panel;
			Flags = LocationFlag.None;
			if (panel == null || !panel.visualTree.layout.Contains(position))
			{
				Flags |= LocationFlag.OutsidePanel;
			}
		}
	}

	private static PointerLocation[] s_PlayerPointerLocations = new PointerLocation[PointerId.maxPointers];

	private static int[] s_PressedButtons = new int[PointerId.maxPointers];

	private static readonly IPanel[] s_PlayerPanelWithSoftPointerCapture = new IPanel[PointerId.maxPointers];

	internal static void Reset()
	{
		for (int i = 0; i < PointerId.maxPointers; i++)
		{
			s_PlayerPointerLocations[i].SetLocation(Vector2.zero, null);
			s_PressedButtons[i] = 0;
			s_PlayerPanelWithSoftPointerCapture[i] = null;
		}
	}

	internal static void RemovePanelData(IPanel panel)
	{
		for (int i = 0; i < PointerId.maxPointers; i++)
		{
			if (s_PlayerPointerLocations[i].Panel == panel)
			{
				s_PlayerPointerLocations[i].SetLocation(Vector2.zero, null);
			}
			if (s_PlayerPanelWithSoftPointerCapture[i] == panel)
			{
				s_PlayerPanelWithSoftPointerCapture[i] = null;
			}
		}
	}

	public static void SavePointerPosition(int pointerId, Vector2 position, IPanel panel, ContextType contextType)
	{
		if ((uint)contextType > 1u)
		{
		}
		s_PlayerPointerLocations[pointerId].SetLocation(position, panel);
	}

	public static void PressButton(int pointerId, int buttonId)
	{
		Debug.Assert(buttonId >= 0);
		Debug.Assert(buttonId < 32);
		s_PressedButtons[pointerId] |= 1 << buttonId;
	}

	public static void ReleaseButton(int pointerId, int buttonId)
	{
		Debug.Assert(buttonId >= 0);
		Debug.Assert(buttonId < 32);
		s_PressedButtons[pointerId] &= ~(1 << buttonId);
	}

	public static void ReleaseAllButtons(int pointerId)
	{
		s_PressedButtons[pointerId] = 0;
	}

	public static Vector2 GetPointerPosition(int pointerId, ContextType contextType)
	{
		if ((uint)contextType > 1u)
		{
		}
		return s_PlayerPointerLocations[pointerId].Position;
	}

	public static IPanel GetPanel(int pointerId, ContextType contextType)
	{
		if ((uint)contextType > 1u)
		{
		}
		return s_PlayerPointerLocations[pointerId].Panel;
	}

	private static bool HasFlagFast(LocationFlag flagSet, LocationFlag flag)
	{
		return (flagSet & flag) == flag;
	}

	public static bool HasLocationFlag(int pointerId, ContextType contextType, LocationFlag flag)
	{
		if ((uint)contextType > 1u)
		{
		}
		return HasFlagFast(s_PlayerPointerLocations[pointerId].Flags, flag);
	}

	public static int GetPressedButtons(int pointerId)
	{
		return s_PressedButtons[pointerId];
	}

	internal static bool HasAdditionalPressedButtons(int pointerId, int exceptButtonId)
	{
		return (s_PressedButtons[pointerId] & ~(1 << exceptButtonId)) != 0;
	}

	internal static void SetPlayerPanelWithSoftPointerCapture(int pointerId, IPanel panel)
	{
		s_PlayerPanelWithSoftPointerCapture[pointerId] = panel;
	}

	internal static IPanel GetPlayerPanelWithSoftPointerCapture(int pointerId)
	{
		return s_PlayerPanelWithSoftPointerCapture[pointerId];
	}
}
