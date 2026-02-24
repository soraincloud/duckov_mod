using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal static class UIEventRegistration
{
	private static List<IUIElementsUtility> s_Utilities;

	static UIEventRegistration()
	{
		s_Utilities = new List<IUIElementsUtility>();
		GUIUtility.takeCapture = (Action)Delegate.Combine(GUIUtility.takeCapture, (Action)delegate
		{
			TakeCapture();
		});
		GUIUtility.releaseCapture = (Action)Delegate.Combine(GUIUtility.releaseCapture, (Action)delegate
		{
			ReleaseCapture();
		});
		GUIUtility.processEvent = (Func<int, IntPtr, bool>)Delegate.Combine(GUIUtility.processEvent, (Func<int, IntPtr, bool>)((int i, IntPtr ptr) => ProcessEvent(i, ptr)));
		GUIUtility.cleanupRoots = (Action)Delegate.Combine(GUIUtility.cleanupRoots, (Action)delegate
		{
			CleanupRoots();
		});
		GUIUtility.endContainerGUIFromException = (Func<Exception, bool>)Delegate.Combine(GUIUtility.endContainerGUIFromException, (Func<Exception, bool>)((Exception exception) => EndContainerGUIFromException(exception)));
		GUIUtility.guiChanged = (Action)Delegate.Combine(GUIUtility.guiChanged, (Action)delegate
		{
			MakeCurrentIMGUIContainerDirty();
		});
	}

	internal static void RegisterUIElementSystem(IUIElementsUtility utility)
	{
		s_Utilities.Insert(0, utility);
	}

	private static void TakeCapture()
	{
		foreach (IUIElementsUtility s_Utility in s_Utilities)
		{
			if (s_Utility.TakeCapture())
			{
				break;
			}
		}
	}

	private static void ReleaseCapture()
	{
		foreach (IUIElementsUtility s_Utility in s_Utilities)
		{
			if (s_Utility.ReleaseCapture())
			{
				break;
			}
		}
	}

	private static bool EndContainerGUIFromException(Exception exception)
	{
		foreach (IUIElementsUtility s_Utility in s_Utilities)
		{
			if (s_Utility.EndContainerGUIFromException(exception))
			{
				return true;
			}
		}
		return GUIUtility.ShouldRethrowException(exception);
	}

	private static bool ProcessEvent(int instanceID, IntPtr nativeEventPtr)
	{
		bool eventHandled = false;
		foreach (IUIElementsUtility s_Utility in s_Utilities)
		{
			if (s_Utility.ProcessEvent(instanceID, nativeEventPtr, ref eventHandled))
			{
				return eventHandled;
			}
		}
		return false;
	}

	private static void CleanupRoots()
	{
		foreach (IUIElementsUtility s_Utility in s_Utilities)
		{
			if (s_Utility.CleanupRoots())
			{
				break;
			}
		}
	}

	internal static void MakeCurrentIMGUIContainerDirty()
	{
		foreach (IUIElementsUtility s_Utility in s_Utilities)
		{
			if (s_Utility.MakeCurrentIMGUIContainerDirty())
			{
				break;
			}
		}
	}

	internal static void UpdateSchedulers()
	{
		foreach (IUIElementsUtility s_Utility in s_Utilities)
		{
			s_Utility.UpdateSchedulers();
		}
	}

	internal static void RequestRepaintForPanels(Action<ScriptableObject> repaintCallback)
	{
		foreach (IUIElementsUtility s_Utility in s_Utilities)
		{
			s_Utility.RequestRepaintForPanels(repaintCallback);
		}
	}
}
