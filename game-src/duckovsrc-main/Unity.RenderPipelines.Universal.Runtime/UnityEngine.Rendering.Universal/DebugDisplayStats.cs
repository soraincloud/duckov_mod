using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal;

internal class DebugDisplayStats : IDebugDisplaySettingsData, IDebugDisplaySettingsQuery
{
	[DisplayInfo(name = "Display Stats", order = int.MinValue)]
	private class StatsPanel : DebugDisplaySettingsPanel
	{
		public override DebugUI.Flags Flags => DebugUI.Flags.RuntimeOnly;

		public StatsPanel(DebugFrameTiming frameTiming)
		{
			List<DebugUI.Widget> list = new List<DebugUI.Widget>();
			frameTiming.RegisterDebugUI(list);
			foreach (DebugUI.Widget item in list)
			{
				AddWidget(item);
			}
		}
	}

	private DebugFrameTiming m_DebugFrameTiming = new DebugFrameTiming();

	public bool AreAnySettingsActive => false;

	public bool IsPostProcessingAllowed => true;

	public bool IsLightingActive => true;

	public void UpdateFrameTiming()
	{
		m_DebugFrameTiming.UpdateFrameTiming();
	}

	public bool TryGetScreenClearColor(ref Color _)
	{
		return false;
	}

	public IDebugDisplaySettingsPanelDisposable CreatePanel()
	{
		return new StatsPanel(m_DebugFrameTiming);
	}
}
