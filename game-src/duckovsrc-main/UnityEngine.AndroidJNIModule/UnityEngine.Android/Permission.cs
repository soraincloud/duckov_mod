using System.Runtime.InteropServices;

namespace UnityEngine.Android;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct Permission
{
	public const string Camera = "android.permission.CAMERA";

	public const string Microphone = "android.permission.RECORD_AUDIO";

	public const string FineLocation = "android.permission.ACCESS_FINE_LOCATION";

	public const string CoarseLocation = "android.permission.ACCESS_COARSE_LOCATION";

	public const string ExternalStorageRead = "android.permission.READ_EXTERNAL_STORAGE";

	public const string ExternalStorageWrite = "android.permission.WRITE_EXTERNAL_STORAGE";

	private static AndroidJavaObject m_UnityPermissions;

	private static AndroidJavaObject GetUnityPermissions()
	{
		if (m_UnityPermissions != null)
		{
			return m_UnityPermissions;
		}
		m_UnityPermissions = new AndroidJavaClass("com.unity3d.player.UnityPermissions");
		return m_UnityPermissions;
	}

	public static bool HasUserAuthorizedPermission(string permission)
	{
		if (permission == null)
		{
			return false;
		}
		return true;
	}

	public static void RequestUserPermission(string permission)
	{
		if (permission != null)
		{
			RequestUserPermissions(new string[1] { permission }, null);
		}
	}

	public static void RequestUserPermissions(string[] permissions)
	{
		if (permissions != null && permissions.Length != 0)
		{
			RequestUserPermissions(permissions, null);
		}
	}

	public static void RequestUserPermission(string permission, PermissionCallbacks callbacks)
	{
		if (permission != null)
		{
			RequestUserPermissions(new string[1] { permission }, callbacks);
		}
	}

	public static void RequestUserPermissions(string[] permissions, PermissionCallbacks callbacks)
	{
		if (permissions != null && permissions.Length != 0)
		{
		}
	}
}
