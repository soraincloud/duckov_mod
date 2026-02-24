using System;

namespace UnityEngine.Rendering;

public interface IDebugDisplaySettings : IDebugDisplaySettingsQuery
{
	void Reset();

	void ForEach(Action<IDebugDisplaySettingsData> onExecute);
}
