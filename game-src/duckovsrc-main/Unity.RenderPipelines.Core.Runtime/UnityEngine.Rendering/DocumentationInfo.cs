namespace UnityEngine.Rendering;

public class DocumentationInfo
{
	private const string fallbackVersion = "13.1";

	private const string url = "https://docs.unity3d.com/Packages/{0}@{1}/manual/{2}.html{3}";

	public static string version => "13.1";

	public static string GetPageLink(string packageName, string pageName)
	{
		return string.Format("https://docs.unity3d.com/Packages/{0}@{1}/manual/{2}.html{3}", packageName, version, pageName, "");
	}

	public static string GetPageLink(string packageName, string pageName, string pageHash)
	{
		return $"https://docs.unity3d.com/Packages/{packageName}@{version}/manual/{pageName}.html{pageHash}";
	}
}
