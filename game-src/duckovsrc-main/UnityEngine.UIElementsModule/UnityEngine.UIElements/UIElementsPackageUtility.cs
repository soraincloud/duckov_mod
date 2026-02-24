namespace UnityEngine.UIElements;

internal static class UIElementsPackageUtility
{
	internal static bool IsUIEPackageLoaded { get; private set; }

	internal static string EditorResourcesBasePath { get; private set; }

	static UIElementsPackageUtility()
	{
		Refresh();
	}

	internal static void Refresh()
	{
		EditorResourcesBasePath = "";
		IsUIEPackageLoaded = false;
	}
}
