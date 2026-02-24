using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.Diagnostics;

namespace UnityEngine.AddressableAssets.Utility;

internal class DiagnosticInfo
{
	public string DisplayName;

	public int ObjectId;

	public int[] Dependencies;

	public DiagnosticEvent CreateEvent(string category, ResourceManager.DiagnosticEventType eventType, int frame, int val)
	{
		return new DiagnosticEvent(category, DisplayName, ObjectId, (int)eventType, frame, val, Dependencies);
	}
}
