namespace UnityEngine.Rendering;

public interface IDebugDisplaySettingsQuery
{
	bool AreAnySettingsActive { get; }

	bool IsPostProcessingAllowed { get; }

	bool IsLightingActive { get; }

	bool TryGetScreenClearColor(ref Color color);
}
