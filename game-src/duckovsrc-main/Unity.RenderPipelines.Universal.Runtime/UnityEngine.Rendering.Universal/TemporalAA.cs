using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.Universal;

public static class TemporalAA
{
	internal static class ShaderConstants
	{
		public static readonly int _TaaAccumulationTex = Shader.PropertyToID("_TaaAccumulationTex");

		public static readonly int _TaaMotionVectorTex = Shader.PropertyToID("_TaaMotionVectorTex");

		public static readonly int _TaaFilterWeights = Shader.PropertyToID("_TaaFilterWeights");

		public static readonly int _TaaFrameInfluence = Shader.PropertyToID("_TaaFrameInfluence");

		public static readonly int _TaaVarianceClampScale = Shader.PropertyToID("_TaaVarianceClampScale");

		public static readonly int _CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
	}

	internal static class ShaderKeywords
	{
		public static readonly string TAA_LOW_PRECISION_SOURCE = "TAA_LOW_PRECISION_SOURCE";
	}

	[Serializable]
	public struct Settings
	{
		[SerializeField]
		[FormerlySerializedAs("quality")]
		internal TemporalAAQuality m_Quality;

		[SerializeField]
		[FormerlySerializedAs("frameInfluence")]
		internal float m_FrameInfluence;

		[SerializeField]
		[FormerlySerializedAs("jitterScale")]
		internal float m_JitterScale;

		[SerializeField]
		[FormerlySerializedAs("mipBias")]
		internal float m_MipBias;

		[SerializeField]
		[FormerlySerializedAs("varianceClampScale")]
		internal float m_VarianceClampScale;

		[SerializeField]
		[FormerlySerializedAs("contrastAdaptiveSharpening")]
		internal float m_ContrastAdaptiveSharpening;

		[NonSerialized]
		internal int resetHistoryFrames;

		[NonSerialized]
		internal int jitterFrameCountOffset;

		public TemporalAAQuality quality
		{
			get
			{
				return m_Quality;
			}
			set
			{
				m_Quality = (TemporalAAQuality)Mathf.Clamp((int)value, 0, 4);
			}
		}

		public float baseBlendFactor
		{
			get
			{
				return 1f - m_FrameInfluence;
			}
			set
			{
				m_FrameInfluence = Mathf.Clamp01(1f - value);
			}
		}

		public float jitterScale
		{
			get
			{
				return m_JitterScale;
			}
			set
			{
				m_JitterScale = Mathf.Clamp01(value);
			}
		}

		public float mipBias
		{
			get
			{
				return m_MipBias;
			}
			set
			{
				m_MipBias = Mathf.Clamp(value, -1f, 0f);
			}
		}

		public float varianceClampScale
		{
			get
			{
				return m_VarianceClampScale;
			}
			set
			{
				m_VarianceClampScale = Mathf.Clamp(value, 0.001f, 10f);
			}
		}

		public float contrastAdaptiveSharpening
		{
			get
			{
				return m_ContrastAdaptiveSharpening;
			}
			set
			{
				m_ContrastAdaptiveSharpening = Mathf.Clamp01(value);
			}
		}

		public static Settings Create()
		{
			Settings result = default(Settings);
			result.m_Quality = TemporalAAQuality.High;
			result.m_FrameInfluence = 0.1f;
			result.m_JitterScale = 1f;
			result.m_MipBias = 0f;
			result.m_VarianceClampScale = 0.9f;
			result.m_ContrastAdaptiveSharpening = 0f;
			result.resetHistoryFrames = 0;
			result.jitterFrameCountOffset = 0;
			return result;
		}
	}

	private class TaaPassData
	{
		internal TextureHandle dstTex;

		internal TextureHandle srcColorTex;

		internal TextureHandle srcDepthTex;

		internal TextureHandle srcMotionVectorTex;

		internal TextureHandle srcTaaAccumTex;

		internal Material material;

		internal int passIndex;

		internal float taaFrameInfluence;

		internal float taaVarianceClampScale;

		internal float[] taaFilterWeights;

		internal bool taaLowPrecisionSource;
	}

	private static readonly Vector2[] taaFilterOffsets = new Vector2[9]
	{
		new Vector2(0f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 0f),
		new Vector2(-1f, 0f),
		new Vector2(0f, -1f),
		new Vector2(-1f, 1f),
		new Vector2(1f, -1f),
		new Vector2(1f, 1f),
		new Vector2(-1f, -1f)
	};

	private static readonly float[] taaFilterWeights = new float[taaFilterOffsets.Length + 1];

	internal static Matrix4x4 CalculateJitterMatrix(ref CameraData cameraData)
	{
		Matrix4x4 result = Matrix4x4.identity;
		if (cameraData.IsTemporalAAEnabled())
		{
			int jitterFrameCountOffset = cameraData.taaSettings.jitterFrameCountOffset;
			int frameIndex = Time.frameCount + jitterFrameCountOffset;
			float num = cameraData.cameraTargetDescriptor.width;
			float num2 = cameraData.cameraTargetDescriptor.height;
			float jitterScale = cameraData.taaSettings.jitterScale;
			Vector2 vector = CalculateJitter(frameIndex) * jitterScale;
			float x = vector.x * (2f / num);
			float y = vector.y * (2f / num2);
			result = Matrix4x4.Translate(new Vector3(x, y, 0f));
		}
		return result;
	}

	internal static Vector2 CalculateJitter(int frameIndex)
	{
		float x = HaltonSequence.Get((frameIndex & 0x3FF) + 1, 2) - 0.5f;
		float y = HaltonSequence.Get((frameIndex & 0x3FF) + 1, 3) - 0.5f;
		return new Vector2(x, y);
	}

	internal static float[] CalculateFilterWeights(float jitterScale)
	{
		float num = 0f;
		for (int i = 0; i < 9; i++)
		{
			Vector2 vector = CalculateJitter(Time.frameCount) * jitterScale;
			float num2 = taaFilterOffsets[i].x - vector.x;
			float num3 = taaFilterOffsets[i].y - vector.y;
			float num4 = num2 * num2 + num3 * num3;
			taaFilterWeights[i] = Mathf.Exp(-2.2727273f * num4);
			num += taaFilterWeights[i];
		}
		for (int j = 0; j < 9; j++)
		{
			taaFilterWeights[j] /= num;
		}
		return taaFilterWeights;
	}

	internal static string ValidateAndWarn(ref CameraData cameraData)
	{
		string text = null;
		if (cameraData.taaPersistentData == null)
		{
			text = "Disabling TAA due to invalid persistent data.";
		}
		if (text == null && cameraData.cameraTargetDescriptor.msaaSamples != 1)
		{
			text = ((cameraData.xr == null || !cameraData.xr.enabled) ? "Disabling TAA because MSAA is on." : "Disabling TAA because MSAA is on. MSAA must be disabled globally for all cameras in XR mode.");
		}
		if (text == null && cameraData.camera.TryGetComponent<UniversalAdditionalCameraData>(out var component) && (component.renderType == CameraRenderType.Overlay || component.cameraStack.Count > 0))
		{
			text = "Disabling TAA because camera is stacked.";
		}
		if (text == null && cameraData.camera.allowDynamicResolution)
		{
			text = "Disabling TAA because camera has dynamic resolution enabled. You can use a constant render scale instead.";
		}
		if (text == null && !cameraData.postProcessEnabled)
		{
			text = "Disabling TAA because camera has post-processing disabled.";
		}
		if (Time.frameCount % 60 == 0)
		{
			Debug.LogWarning(text);
		}
		return text;
	}

	internal static void ExecutePass(CommandBuffer cmd, Material taaMaterial, ref CameraData cameraData, RTHandle source, RTHandle destination, RenderTexture motionVectors)
	{
		using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.TemporalAA)))
		{
			int num = 0;
			num = cameraData.xr.multipassId;
			bool flag = cameraData.taaPersistentData.GetLastAccumFrameIndex(num) != Time.frameCount;
			RTHandle rTHandle = cameraData.taaPersistentData.accumulationTexture(num);
			taaMaterial.SetTexture(ShaderConstants._TaaAccumulationTex, rTHandle);
			taaMaterial.SetTexture(ShaderConstants._TaaMotionVectorTex, flag ? ((Texture)motionVectors) : ((Texture)Texture2D.blackTexture));
			ref Settings taaSettings = ref cameraData.taaSettings;
			float value = ((taaSettings.resetHistoryFrames == 0) ? taaSettings.m_FrameInfluence : 1f);
			taaMaterial.SetFloat(ShaderConstants._TaaFrameInfluence, value);
			taaMaterial.SetFloat(ShaderConstants._TaaVarianceClampScale, taaSettings.varianceClampScale);
			if (taaSettings.quality == TemporalAAQuality.VeryHigh)
			{
				taaMaterial.SetFloatArray(ShaderConstants._TaaFilterWeights, CalculateFilterWeights(taaSettings.jitterScale));
			}
			GraphicsFormat graphicsFormat = rTHandle.rt.graphicsFormat;
			if (graphicsFormat == GraphicsFormat.R8G8B8A8_UNorm || graphicsFormat == GraphicsFormat.B8G8R8A8_UNorm || graphicsFormat == GraphicsFormat.B10G11R11_UFloatPack32)
			{
				taaMaterial.EnableKeyword(ShaderKeywords.TAA_LOW_PRECISION_SOURCE);
			}
			else
			{
				taaMaterial.DisableKeyword(ShaderKeywords.TAA_LOW_PRECISION_SOURCE);
			}
			Blitter.BlitCameraTexture(cmd, source, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, taaMaterial, (int)taaSettings.quality);
			if (flag)
			{
				int pass = taaMaterial.shader.passCount - 1;
				Blitter.BlitCameraTexture(cmd, destination, rTHandle, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, taaMaterial, pass);
				cameraData.taaPersistentData.SetLastAccumFrameIndex(num, Time.frameCount);
			}
		}
	}

	internal static void Render(RenderGraph renderGraph, Material taaMaterial, ref CameraData cameraData, ref TextureHandle srcColor, ref TextureHandle srcDepth, ref TextureHandle srcMotionVectors, ref TextureHandle dstColor)
	{
		int num = 0;
		num = cameraData.xr.multipassId;
		ref Settings taaSettings = ref cameraData.taaSettings;
		bool flag = cameraData.taaPersistentData.GetLastAccumFrameIndex(num) != Time.frameCount;
		float taaFrameInfluence = ((taaSettings.resetHistoryFrames == 0) ? taaSettings.m_FrameInfluence : 1f);
		RTHandle rTHandle = cameraData.taaPersistentData.accumulationTexture(num);
		TextureHandle input = renderGraph.ImportTexture(rTHandle);
		TextureHandle input2 = (flag ? srcMotionVectors : renderGraph.defaultResources.blackTexture);
		TaaPassData passData;
		using (RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<TaaPassData>("Temporal Anti-aliasing", out passData, ProfilingSampler.Get(URPProfileId.TemporalAA)))
		{
			passData.dstTex = renderGraphBuilder.UseColorBuffer(in dstColor, 0);
			passData.srcColorTex = renderGraphBuilder.ReadTexture(in srcColor);
			passData.srcDepthTex = renderGraphBuilder.ReadTexture(in srcDepth);
			passData.srcMotionVectorTex = renderGraphBuilder.ReadTexture(in input2);
			passData.srcTaaAccumTex = renderGraphBuilder.ReadTexture(in input);
			passData.material = taaMaterial;
			passData.passIndex = (int)taaSettings.quality;
			passData.taaFrameInfluence = taaFrameInfluence;
			passData.taaVarianceClampScale = taaSettings.varianceClampScale;
			if (taaSettings.quality == TemporalAAQuality.VeryHigh)
			{
				passData.taaFilterWeights = CalculateFilterWeights(taaSettings.jitterScale);
			}
			else
			{
				passData.taaFilterWeights = null;
			}
			GraphicsFormat graphicsFormat = rTHandle.rt.graphicsFormat;
			if (graphicsFormat == GraphicsFormat.R8G8B8A8_UNorm || graphicsFormat == GraphicsFormat.B8G8R8A8_UNorm || graphicsFormat == GraphicsFormat.B10G11R11_UFloatPack32)
			{
				passData.taaLowPrecisionSource = true;
			}
			else
			{
				passData.taaLowPrecisionSource = false;
			}
			renderGraphBuilder.SetRenderFunc(delegate(TaaPassData data, RenderGraphContext context)
			{
				data.material.SetFloat(ShaderConstants._TaaFrameInfluence, data.taaFrameInfluence);
				data.material.SetFloat(ShaderConstants._TaaVarianceClampScale, data.taaVarianceClampScale);
				data.material.SetTexture(ShaderConstants._TaaAccumulationTex, data.srcTaaAccumTex);
				data.material.SetTexture(ShaderConstants._TaaMotionVectorTex, data.srcMotionVectorTex);
				data.material.SetTexture(ShaderConstants._CameraDepthTexture, data.srcDepthTex);
				CoreUtils.SetKeyword(data.material, ShaderKeywords.TAA_LOW_PRECISION_SOURCE, data.taaLowPrecisionSource);
				if (data.taaFilterWeights != null)
				{
					data.material.SetFloatArray(ShaderConstants._TaaFilterWeights, data.taaFilterWeights);
				}
				Blitter.BlitTexture(context.cmd, data.srcColorTex, Vector2.one, data.material, data.passIndex);
			});
		}
		if (!flag)
		{
			return;
		}
		int passIndex = taaMaterial.shader.passCount - 1;
		TaaPassData passData2;
		using RenderGraphBuilder renderGraphBuilder2 = renderGraph.AddRenderPass<TaaPassData>("Temporal Anti-aliasing Copy History", out passData2, new ProfilingSampler("TemporalAAHistoryCopy"));
		passData2.dstTex = renderGraphBuilder2.UseColorBuffer(in input, 0);
		passData2.srcColorTex = renderGraphBuilder2.ReadTexture(in dstColor);
		passData2.material = taaMaterial;
		passData2.passIndex = passIndex;
		renderGraphBuilder2.SetRenderFunc(delegate(TaaPassData data, RenderGraphContext context)
		{
			Blitter.BlitTexture(context.cmd, data.srcColorTex, Vector2.one, data.material, data.passIndex);
		});
	}
}
