using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullScreenPassRendererFeature : ScriptableRendererFeature, ISerializationCallbackReceiver
{
	public enum InjectionPoint
	{
		BeforeRenderingTransparents = 450,
		BeforeRenderingPostProcessing = 550,
		AfterRenderingPostProcessing = 600
	}

	internal class FullScreenRenderPass : ScriptableRenderPass
	{
		private Material m_Material;

		private int m_PassIndex;

		private bool m_CopyActiveColor;

		private bool m_BindDepthStencilAttachment;

		private RTHandle m_CopiedColor;

		private static MaterialPropertyBlock s_SharedPropertyBlock = new MaterialPropertyBlock();

		public FullScreenRenderPass(string passName)
		{
			base.profilingSampler = new ProfilingSampler(passName);
		}

		public void SetupMembers(Material material, int passIndex, bool copyActiveColor, bool bindDepthStencilAttachment)
		{
			m_Material = material;
			m_PassIndex = passIndex;
			m_CopyActiveColor = copyActiveColor;
			m_BindDepthStencilAttachment = bindDepthStencilAttachment;
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			ResetTarget();
			if (m_CopyActiveColor)
			{
				ReAllocate(renderingData.cameraData.cameraTargetDescriptor);
			}
		}

		internal void ReAllocate(RenderTextureDescriptor desc)
		{
			desc.msaaSamples = 1;
			desc.depthBufferBits = 0;
			RenderingUtils.ReAllocateIfNeeded(ref m_CopiedColor, in desc, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "_FullscreenPassColorCopy");
		}

		public void Dispose()
		{
			m_CopiedColor?.Release();
		}

		private static void ExecuteCopyColorPass(CommandBuffer cmd, RTHandle sourceTexture)
		{
			Blitter.BlitTexture(cmd, sourceTexture, new Vector4(1f, 1f, 0f, 0f), 0f, bilinear: false);
		}

		private static void ExecuteMainPass(CommandBuffer cmd, RTHandle sourceTexture, Material material, int passIndex)
		{
			s_SharedPropertyBlock.Clear();
			if (sourceTexture != null)
			{
				s_SharedPropertyBlock.SetTexture(ShaderPropertyId.blitTexture, sourceTexture);
			}
			s_SharedPropertyBlock.SetVector(ShaderPropertyId.blitScaleBias, new Vector4(1f, 1f, 0f, 0f));
			cmd.DrawProcedural(Matrix4x4.identity, material, passIndex, MeshTopology.Triangles, 3, 1, s_SharedPropertyBlock);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			ref CameraData cameraData = ref renderingData.cameraData;
			CommandBuffer commandBuffer = renderingData.commandBuffer;
			using (new ProfilingScope(commandBuffer, base.profilingSampler))
			{
				if (m_CopyActiveColor)
				{
					CoreUtils.SetRenderTarget(commandBuffer, m_CopiedColor);
					ExecuteCopyColorPass(commandBuffer, cameraData.renderer.cameraColorTargetHandle);
				}
				if (m_BindDepthStencilAttachment)
				{
					CoreUtils.SetRenderTarget(commandBuffer, cameraData.renderer.cameraColorTargetHandle, cameraData.renderer.cameraDepthTargetHandle);
				}
				else
				{
					CoreUtils.SetRenderTarget(commandBuffer, cameraData.renderer.cameraColorTargetHandle);
				}
				ExecuteMainPass(commandBuffer, m_CopyActiveColor ? m_CopiedColor : null, m_Material, m_PassIndex);
			}
		}
	}

	private enum Version
	{
		Uninitialised = -1,
		Initial = 0,
		AddFetchColorBufferCheckbox = 1,
		Count = 2,
		Latest = 1
	}

	public InjectionPoint injectionPoint = InjectionPoint.AfterRenderingPostProcessing;

	public bool fetchColorBuffer = true;

	public ScriptableRenderPassInput requirements;

	public Material passMaterial;

	internal bool showAdditionalProperties;

	public int passIndex;

	public bool bindDepthStencilAttachment;

	private FullScreenRenderPass m_FullScreenPass;

	[SerializeField]
	[HideInInspector]
	private Version m_Version = Version.Uninitialised;

	public override void Create()
	{
		m_FullScreenPass = new FullScreenRenderPass(base.name);
	}

	internal override bool RequireRenderingLayers(bool isDeferred, bool needsGBufferAccurateNormals, out RenderingLayerUtils.Event atEvent, out RenderingLayerUtils.MaskSize maskSize)
	{
		atEvent = RenderingLayerUtils.Event.Opaque;
		maskSize = RenderingLayerUtils.MaskSize.Bits8;
		return false;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (!UniversalRenderer.IsOffscreenDepthTexture(in renderingData.cameraData) && renderingData.cameraData.cameraType != CameraType.Preview && renderingData.cameraData.cameraType != CameraType.Reflection)
		{
			if (passMaterial == null)
			{
				Debug.LogWarningFormat("The full screen feature \"{0}\" will not execute - no material is assigned. Please make sure a material is assigned for this feature on the renderer asset.", base.name);
				return;
			}
			if (passIndex < 0 || passIndex >= passMaterial.passCount)
			{
				Debug.LogWarningFormat("The full screen feature \"{0}\" will not execute - the pass index is out of bounds for the material.", base.name);
				return;
			}
			m_FullScreenPass.renderPassEvent = (RenderPassEvent)injectionPoint;
			m_FullScreenPass.ConfigureInput(requirements);
			m_FullScreenPass.SetupMembers(passMaterial, passIndex, fetchColorBuffer, bindDepthStencilAttachment);
			renderer.EnqueuePass(m_FullScreenPass);
		}
	}

	protected override void Dispose(bool disposing)
	{
		m_FullScreenPass.Dispose();
	}

	private void UpgradeIfNeeded()
	{
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		if (m_Version == Version.Uninitialised)
		{
			m_Version = Version.AddFetchColorBufferCheckbox;
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (m_Version == Version.Uninitialised)
		{
			m_Version = Version.Initial;
		}
		UpgradeIfNeeded();
	}
}
