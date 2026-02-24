using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

internal class HDRDebugViewPass : ScriptableRenderPass
{
	private enum HDRDebugPassId
	{
		CIExyPrepass,
		DebugViewPass
	}

	private class PassData
	{
		internal Material material;

		internal HDRDebugMode hdrDebugMode;

		internal Vector4 luminanceParameters;

		internal CameraData cameraData;
	}

	internal class ShaderConstants
	{
		public static readonly int _DebugHDRModeId = Shader.PropertyToID("_DebugHDRMode");

		public static readonly int _HDRDebugParamsId = Shader.PropertyToID("_HDRDebugParams");

		public static readonly int _xyTextureId = Shader.PropertyToID("_xyBuffer");

		public static readonly int _SizeOfHDRXYMapping = 512;

		public static readonly int _CIExyUAVIndex = 1;
	}

	private PassData m_PassData;

	private RTHandle m_CIExyTarget;

	private RTHandle m_PassthroughRT;

	private RTHandle m_CameraTargetHandle;

	public HDRDebugViewPass(Material mat)
	{
		base.profilingSampler = new ProfilingSampler("HDRDebugViewPass");
		base.renderPassEvent = (RenderPassEvent)1003;
		m_PassData = new PassData
		{
			material = mat
		};
		base.useNativeRenderPass = false;
	}

	public static void ConfigureDescriptor(ref RenderTextureDescriptor descriptor)
	{
		descriptor.useMipMap = false;
		descriptor.autoGenerateMips = false;
		descriptor.useDynamicScale = true;
		descriptor.depthBufferBits = 0;
	}

	public static void ConfigureDescriptorForCIEPrepass(ref RenderTextureDescriptor descriptor)
	{
		descriptor.graphicsFormat = GraphicsFormat.R32_SFloat;
		int width = (descriptor.height = ShaderConstants._SizeOfHDRXYMapping);
		descriptor.width = width;
		descriptor.enableRandomWrite = true;
		descriptor.msaaSamples = 1;
		descriptor.dimension = TextureDimension.Tex2D;
		descriptor.vrUsage = VRTextureUsage.None;
	}

	internal static Vector4 GetLuminanceParameters(ref CameraData cameraData)
	{
		Vector4 hdrOutputParameters = Vector4.zero;
		if (cameraData.isHDROutputActive)
		{
			Tonemapping component = VolumeManager.instance.stack.GetComponent<Tonemapping>();
			UniversalRenderPipeline.GetHDROutputLuminanceParameters(cameraData.hdrDisplayInformation, cameraData.hdrDisplayColorGamut, component, out hdrOutputParameters);
		}
		else
		{
			hdrOutputParameters.z = 1f;
		}
		return hdrOutputParameters;
	}

	private static void ExecuteCIExyPrepass(CommandBuffer cmd, PassData data, RTHandle sourceTexture, RTHandle xyTarget, RTHandle destTexture)
	{
		using (new ProfilingScope(cmd, new ProfilingSampler("Generate HDR DebugView CIExy")))
		{
			CoreUtils.SetRenderTarget(cmd, destTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, ClearFlag.None, Color.clear);
			Vector4 value = new Vector4(ShaderConstants._SizeOfHDRXYMapping, ShaderConstants._SizeOfHDRXYMapping, 0f, 0f);
			cmd.SetRandomWriteTarget(ShaderConstants._CIExyUAVIndex, xyTarget);
			data.material.SetVector(ShaderConstants._HDRDebugParamsId, value);
			data.material.SetVector(ShaderPropertyId.hdrOutputLuminanceParams, data.luminanceParameters);
			Vector2 vector = (sourceTexture.useScaling ? new Vector2(sourceTexture.rtHandleProperties.rtHandleScale.x, sourceTexture.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Blitter.BlitTexture(cmd, sourceTexture, vector, data.material, 0);
			cmd.ClearRandomWriteTargets();
		}
	}

	private static void ExecuteHDRDebugViewFinalPass(CommandBuffer cmd, PassData data, RTHandle sourceTexture, RTHandle destination, RTHandle xyTarget)
	{
		using (new ProfilingScope(cmd, new ProfilingSampler("HDR DebugView")))
		{
			if (data.cameraData.isHDROutputActive)
			{
				HDROutputUtils.ConfigureHDROutput(data.material, data.cameraData.hdrDisplayColorGamut, HDROutputUtils.Operation.ColorEncoding);
			}
			data.material.SetTexture(ShaderConstants._xyTextureId, xyTarget);
			Vector4 value = new Vector4(ShaderConstants._SizeOfHDRXYMapping, ShaderConstants._SizeOfHDRXYMapping, 0f, 0f);
			data.material.SetVector(ShaderConstants._HDRDebugParamsId, value);
			data.material.SetVector(ShaderPropertyId.hdrOutputLuminanceParams, data.luminanceParameters);
			data.material.SetInteger(ShaderConstants._DebugHDRModeId, (int)data.hdrDebugMode);
			RenderingUtils.FinalBlit(cmd, ref data.cameraData, sourceTexture, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, data.material, 1);
		}
	}

	public void Dispose()
	{
		m_CIExyTarget?.Release();
		m_PassthroughRT?.Release();
		m_CameraTargetHandle?.Release();
	}

	public void Setup(ref CameraData cameraData, HDRDebugMode hdrdebugMode)
	{
		RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
		DebugHandler.ConfigureColorDescriptorForDebugScreen(ref descriptor, cameraData.pixelWidth, cameraData.pixelHeight);
		RenderingUtils.ReAllocateIfNeeded(ref m_PassthroughRT, in descriptor, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "_HDRDebugDummyRT");
		ConfigureDescriptorForCIEPrepass(ref descriptor);
		RenderingUtils.ReAllocateIfNeeded(ref m_CIExyTarget, in descriptor, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "_xyBuffer");
		m_PassData.hdrDebugMode = hdrdebugMode;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		m_PassData.luminanceParameters = GetLuminanceParameters(ref renderingData.cameraData);
		m_PassData.cameraData = renderingData.cameraData;
		RTHandle cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
		RenderTargetIdentifier cameraTargetIdentifier = RenderingUtils.GetCameraTargetIdentifier(ref renderingData);
		if (m_CameraTargetHandle != cameraTargetIdentifier)
		{
			m_CameraTargetHandle?.Release();
			m_CameraTargetHandle = RTHandles.Alloc(cameraTargetIdentifier);
		}
		m_PassData.material.enabledKeywords = null;
		CoreUtils.SetRenderTarget(commandBuffer, m_CIExyTarget, ClearFlag.Color, Color.clear);
		ExecutePass(commandBuffer, m_PassData, cameraColorTargetHandle, m_CIExyTarget);
	}

	private void ExecutePass(CommandBuffer cmd, PassData data, RTHandle sourceTexture, RTHandle xyTarget)
	{
		ExecuteCIExyPrepass(cmd, data, sourceTexture, xyTarget, m_PassthroughRT);
		ExecuteHDRDebugViewFinalPass(cmd, data, m_PassthroughRT, m_CameraTargetHandle, xyTarget);
		data.cameraData.renderer.ConfigureCameraTarget(m_CameraTargetHandle, m_CameraTargetHandle);
	}
}
