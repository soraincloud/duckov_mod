using System;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.ResourceManagement.Diagnostics;

public class DiagnosticEventCollector : MonoBehaviour
{
	private static DiagnosticEventCollector s_Collector;

	public static Guid PlayerConnectionGuid => DiagnosticEventCollectorSingleton.PlayerConnectionGuid;

	public static DiagnosticEventCollector FindOrCreateGlobalInstance()
	{
		if (s_Collector == null)
		{
			GameObject obj = new GameObject("EventCollector", typeof(DiagnosticEventCollector));
			s_Collector = obj.GetComponent<DiagnosticEventCollector>();
			obj.hideFlags = HideFlags.DontSave;
		}
		return s_Collector;
	}

	public static bool RegisterEventHandler(Action<DiagnosticEvent> handler, bool register, bool create)
	{
		return DiagnosticEventCollectorSingleton.RegisterEventHandler(handler, register, create);
	}

	public void UnregisterEventHandler(Action<DiagnosticEvent> handler)
	{
		ComponentSingleton<DiagnosticEventCollectorSingleton>.Instance.UnregisterEventHandler(handler);
	}

	public void PostEvent(DiagnosticEvent diagnosticEvent)
	{
		ComponentSingleton<DiagnosticEventCollectorSingleton>.Instance.PostEvent(diagnosticEvent);
	}
}
