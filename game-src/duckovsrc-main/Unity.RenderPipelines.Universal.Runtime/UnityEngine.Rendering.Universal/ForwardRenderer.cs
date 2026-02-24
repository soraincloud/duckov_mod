using System;

namespace UnityEngine.Rendering.Universal;

[Obsolete("ForwardRenderer has been deprecated (UnityUpgradable) -> UniversalRenderer", true)]
public sealed class ForwardRenderer : ScriptableRenderer
{
	private static readonly string k_ErrorMessage = "ForwardRenderer has been deprecated. Use UniversalRenderer instead";

	public ForwardRenderer(ForwardRendererData data)
		: base(data)
	{
		throw new NotSupportedException(k_ErrorMessage);
	}

	public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		throw new NotSupportedException(k_ErrorMessage);
	}

	public override void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		throw new NotSupportedException(k_ErrorMessage);
	}

	public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, ref CameraData cameraData)
	{
		throw new NotSupportedException(k_ErrorMessage);
	}

	public override void FinishRendering(CommandBuffer cmd)
	{
		throw new NotSupportedException(k_ErrorMessage);
	}

	internal override void SwapColorBuffer(CommandBuffer cmd)
	{
		throw new NotSupportedException(k_ErrorMessage);
	}

	internal override RTHandle GetCameraColorFrontBuffer(CommandBuffer cmd)
	{
		throw new NotImplementedException();
	}
}
