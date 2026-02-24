using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

internal static class RenderingLayerUtils
{
	public enum Event
	{
		DepthNormalPrePass,
		Opaque
	}

	public enum MaskSize
	{
		Bits8,
		Bits16,
		Bits24,
		Bits32
	}

	public static void CombineRendererEvents(bool isDeferred, int msaaSampleCount, Event rendererEvent, ref Event combinedEvent)
	{
		if (msaaSampleCount > 1 && !isDeferred)
		{
			combinedEvent = Event.DepthNormalPrePass;
		}
		else
		{
			combinedEvent = Combine(combinedEvent, rendererEvent);
		}
	}

	public static bool RequireRenderingLayers(UniversalRendererData universalRendererData, int msaaSampleCount, out Event combinedEvent, out MaskSize combinedMaskSize)
	{
		combinedEvent = Event.Opaque;
		combinedMaskSize = MaskSize.Bits8;
		bool flag = universalRendererData.renderingMode == RenderingMode.Deferred;
		bool flag2 = false;
		foreach (ScriptableRendererFeature rendererFeature in universalRendererData.rendererFeatures)
		{
			if (rendererFeature.isActive)
			{
				flag2 |= rendererFeature.RequireRenderingLayers(flag, universalRendererData.accurateGbufferNormals, out var atEvent, out var maskSize);
				combinedEvent = Combine(combinedEvent, atEvent);
				combinedMaskSize = Combine(combinedMaskSize, maskSize);
			}
		}
		if (msaaSampleCount > 1 && combinedEvent == Event.Opaque && !flag)
		{
			combinedEvent = Event.DepthNormalPrePass;
		}
		if ((bool)UniversalRenderPipelineGlobalSettings.instance)
		{
			MaskSize maskSize2 = GetMaskSize(UniversalRenderPipelineGlobalSettings.instance.renderingLayerMaskNames.Length);
			combinedMaskSize = Combine(combinedMaskSize, maskSize2);
		}
		return flag2;
	}

	public static bool RequireRenderingLayers(UniversalRenderer universalRenderer, List<ScriptableRendererFeature> rendererFeatures, int msaaSampleCount, out Event combinedEvent, out MaskSize combinedMaskSize)
	{
		combinedEvent = Event.Opaque;
		combinedMaskSize = MaskSize.Bits8;
		bool isDeferred = universalRenderer.renderingModeActual == RenderingMode.Deferred;
		bool flag = false;
		foreach (ScriptableRendererFeature rendererFeature in rendererFeatures)
		{
			if (rendererFeature.isActive)
			{
				flag |= rendererFeature.RequireRenderingLayers(isDeferred, universalRenderer.accurateGbufferNormals, out var atEvent, out var maskSize);
				combinedEvent = Combine(combinedEvent, atEvent);
				combinedMaskSize = Combine(combinedMaskSize, maskSize);
			}
		}
		if (msaaSampleCount > 1 && combinedEvent == Event.Opaque)
		{
			combinedEvent = Event.DepthNormalPrePass;
		}
		if ((bool)UniversalRenderPipelineGlobalSettings.instance)
		{
			MaskSize maskSize2 = GetMaskSize(UniversalRenderPipelineGlobalSettings.instance.renderingLayerMaskNames.Length);
			combinedMaskSize = Combine(combinedMaskSize, maskSize2);
		}
		return flag;
	}

	public static void SetupProperties(CommandBuffer cmd, MaskSize maskSize)
	{
		int bits = GetBits(maskSize);
		uint num = ((bits != 32) ? ((uint)((1 << bits) - 1)) : uint.MaxValue);
		float value = math.rcp(num);
		cmd.SetGlobalInt(ShaderPropertyId.renderingLayerMaxInt, (int)num);
		cmd.SetGlobalFloat(ShaderPropertyId.renderingLayerRcpMaxInt, value);
	}

	public static GraphicsFormat GetFormat(MaskSize maskSize)
	{
		switch (maskSize)
		{
		case MaskSize.Bits8:
			return GraphicsFormat.R8_UNorm;
		case MaskSize.Bits16:
			return GraphicsFormat.R16_UNorm;
		case MaskSize.Bits24:
		case MaskSize.Bits32:
			return GraphicsFormat.R32_SFloat;
		default:
			throw new NotImplementedException();
		}
	}

	public static uint ToValidRenderingLayers(uint renderingLayers)
	{
		if ((bool)UniversalRenderPipelineGlobalSettings.instance)
		{
			return UniversalRenderPipelineGlobalSettings.instance.validRenderingLayers & renderingLayers;
		}
		return renderingLayers;
	}

	private static MaskSize GetMaskSize(int bits)
	{
		return ((bits + 7) / 8) switch
		{
			0 => MaskSize.Bits8, 
			1 => MaskSize.Bits8, 
			2 => MaskSize.Bits16, 
			3 => MaskSize.Bits24, 
			4 => MaskSize.Bits32, 
			_ => MaskSize.Bits32, 
		};
	}

	private static int GetBits(MaskSize maskSize)
	{
		return maskSize switch
		{
			MaskSize.Bits8 => 8, 
			MaskSize.Bits16 => 16, 
			MaskSize.Bits24 => 24, 
			MaskSize.Bits32 => 32, 
			_ => throw new NotImplementedException(), 
		};
	}

	private static Event Combine(Event a, Event b)
	{
		return (Event)Mathf.Min((int)a, (int)b);
	}

	private static MaskSize Combine(MaskSize a, MaskSize b)
	{
		return (MaskSize)Mathf.Max((int)a, (int)b);
	}
}
