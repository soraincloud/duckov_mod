using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal;

internal class TransparentSettingsPass : ScriptableRenderPass
{
	private bool m_shouldReceiveShadows;

	private const string m_ProfilerTag = "Transparent Settings Pass";

	private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Transparent Settings Pass");

	public TransparentSettingsPass(RenderPassEvent evt, bool shadowReceiveSupported)
	{
		base.profilingSampler = new ProfilingSampler("TransparentSettingsPass");
		base.renderPassEvent = evt;
		m_shouldReceiveShadows = shadowReceiveSupported;
	}

	public bool Setup()
	{
		return !m_shouldReceiveShadows;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		ExecutePass(renderingData.commandBuffer, m_shouldReceiveShadows);
	}

	public static void ExecutePass(CommandBuffer cmd, bool shouldReceiveShadows)
	{
		using (new ProfilingScope(cmd, m_ProfilingSampler))
		{
			MainLightShadowCasterPass.SetEmptyMainLightShadowParams(cmd);
			AdditionalLightsShadowCasterPass.SetEmptyAdditionalLightShadowParams(cmd, AdditionalLightsShadowCasterPass.s_EmptyAdditionalLightIndexToShadowParams);
		}
	}
}
