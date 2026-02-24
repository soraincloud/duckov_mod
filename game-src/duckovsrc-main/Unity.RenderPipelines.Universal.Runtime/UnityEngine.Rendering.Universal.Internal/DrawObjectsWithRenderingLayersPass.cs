using System;

namespace UnityEngine.Rendering.Universal.Internal;

internal class DrawObjectsWithRenderingLayersPass : DrawObjectsPass
{
	private RTHandle[] m_ColorTargetIndentifiers;

	private RTHandle m_DepthTargetIndentifiers;

	public DrawObjectsWithRenderingLayersPass(URPProfileId profilerTag, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference)
		: base(profilerTag, opaque, evt, renderQueueRange, layerMask, stencilState, stencilReference)
	{
		m_ColorTargetIndentifiers = new RTHandle[2];
	}

	public void Setup(RTHandle colorAttachment, RTHandle renderingLayersTexture, RTHandle depthAttachment)
	{
		if (colorAttachment == null)
		{
			throw new ArgumentException("Color attachment can not be null", "colorAttachment");
		}
		if (renderingLayersTexture == null)
		{
			throw new ArgumentException("Rendering layers attachment can not be null", "renderingLayersTexture");
		}
		if (depthAttachment == null)
		{
			throw new ArgumentException("Depth attachment can not be null", "depthAttachment");
		}
		m_ColorTargetIndentifiers[0] = colorAttachment;
		m_ColorTargetIndentifiers[1] = renderingLayersTexture;
		m_DepthTargetIndentifiers = depthAttachment;
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		ConfigureTarget(m_ColorTargetIndentifiers, m_DepthTargetIndentifiers);
	}

	protected override void OnExecute(CommandBuffer cmd)
	{
		CoreUtils.SetKeyword(cmd, "_WRITE_RENDERING_LAYERS", state: true);
	}
}
