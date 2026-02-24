using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

internal static class RendererLighting
{
	private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Draw Normals");

	private static readonly ShaderTagId k_NormalsRenderingPassName = new ShaderTagId("NormalsRendering");

	private static readonly Color k_NormalClearColor = new Color(0.5f, 0.5f, 0.5f, 1f);

	private static readonly string k_SpriteLightKeyword = "SPRITE_LIGHT";

	private static readonly string k_UsePointLightCookiesKeyword = "USE_POINT_LIGHT_COOKIES";

	private static readonly string k_LightQualityFastKeyword = "LIGHT_QUALITY_FAST";

	private static readonly string k_UseNormalMap = "USE_NORMAL_MAP";

	private static readonly string k_UseAdditiveBlendingKeyword = "USE_ADDITIVE_BLENDING";

	private static readonly string[] k_UseBlendStyleKeywords = new string[4] { "USE_SHAPE_LIGHT_TYPE_0", "USE_SHAPE_LIGHT_TYPE_1", "USE_SHAPE_LIGHT_TYPE_2", "USE_SHAPE_LIGHT_TYPE_3" };

	private static readonly int[] k_BlendFactorsPropIDs = new int[4]
	{
		Shader.PropertyToID("_ShapeLightBlendFactors0"),
		Shader.PropertyToID("_ShapeLightBlendFactors1"),
		Shader.PropertyToID("_ShapeLightBlendFactors2"),
		Shader.PropertyToID("_ShapeLightBlendFactors3")
	};

	private static readonly int[] k_MaskFilterPropIDs = new int[4]
	{
		Shader.PropertyToID("_ShapeLightMaskFilter0"),
		Shader.PropertyToID("_ShapeLightMaskFilter1"),
		Shader.PropertyToID("_ShapeLightMaskFilter2"),
		Shader.PropertyToID("_ShapeLightMaskFilter3")
	};

	private static readonly int[] k_InvertedFilterPropIDs = new int[4]
	{
		Shader.PropertyToID("_ShapeLightInvertedFilter0"),
		Shader.PropertyToID("_ShapeLightInvertedFilter1"),
		Shader.PropertyToID("_ShapeLightInvertedFilter2"),
		Shader.PropertyToID("_ShapeLightInvertedFilter3")
	};

	private static GraphicsFormat s_RenderTextureFormatToUse = GraphicsFormat.R8G8B8A8_UNorm;

	private static bool s_HasSetupRenderTextureFormatToUse;

	private static readonly int k_SrcBlendID = Shader.PropertyToID("_SrcBlend");

	private static readonly int k_DstBlendID = Shader.PropertyToID("_DstBlend");

	private static readonly int k_FalloffIntensityID = Shader.PropertyToID("_FalloffIntensity");

	private static readonly int k_FalloffDistanceID = Shader.PropertyToID("_FalloffDistance");

	private static readonly int k_LightColorID = Shader.PropertyToID("_LightColor");

	private static readonly int k_VolumeOpacityID = Shader.PropertyToID("_VolumeOpacity");

	private static readonly int k_CookieTexID = Shader.PropertyToID("_CookieTex");

	private static readonly int k_FalloffLookupID = Shader.PropertyToID("_FalloffLookup");

	private static readonly int k_LightPositionID = Shader.PropertyToID("_LightPosition");

	private static readonly int k_LightInvMatrixID = Shader.PropertyToID("_LightInvMatrix");

	private static readonly int k_InnerRadiusMultID = Shader.PropertyToID("_InnerRadiusMult");

	private static readonly int k_OuterAngleID = Shader.PropertyToID("_OuterAngle");

	private static readonly int k_InnerAngleMultID = Shader.PropertyToID("_InnerAngleMult");

	private static readonly int k_LightLookupID = Shader.PropertyToID("_LightLookup");

	private static readonly int k_IsFullSpotlightID = Shader.PropertyToID("_IsFullSpotlight");

	private static readonly int k_LightZDistanceID = Shader.PropertyToID("_LightZDistance");

	private static readonly int k_PointLightCookieTexID = Shader.PropertyToID("_PointLightCookieTex");

	private static GraphicsFormat GetRenderTextureFormat()
	{
		if (!s_HasSetupRenderTextureFormatToUse)
		{
			if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, FormatUsage.Blend))
			{
				s_RenderTextureFormatToUse = GraphicsFormat.B10G11R11_UFloatPack32;
			}
			else if (SystemInfo.IsFormatSupported(GraphicsFormat.R16G16B16A16_SFloat, FormatUsage.Blend))
			{
				s_RenderTextureFormatToUse = GraphicsFormat.R16G16B16A16_SFloat;
			}
			s_HasSetupRenderTextureFormatToUse = true;
		}
		return s_RenderTextureFormatToUse;
	}

	public static void CreateNormalMapRenderTexture(this IRenderPass2D pass, RenderingData renderingData, CommandBuffer cmd, float renderScale)
	{
		if (renderScale != pass.rendererData.normalsRenderTargetScale)
		{
			if (pass.rendererData.isNormalsRenderTargetValid)
			{
				cmd.ReleaseTemporaryRT(pass.rendererData.normalsRenderTargetId);
			}
			pass.rendererData.isNormalsRenderTargetValid = true;
			pass.rendererData.normalsRenderTargetScale = renderScale;
			RenderTextureDescriptor desc = new RenderTextureDescriptor((int)((float)renderingData.cameraData.cameraTargetDescriptor.width * renderScale), (int)((float)renderingData.cameraData.cameraTargetDescriptor.height * renderScale));
			desc.graphicsFormat = GetRenderTextureFormat();
			desc.useMipMap = false;
			desc.autoGenerateMips = false;
			desc.depthBufferBits = (pass.rendererData.useDepthStencilBuffer ? 32 : 0);
			desc.msaaSamples = renderingData.cameraData.cameraTargetDescriptor.msaaSamples;
			desc.dimension = TextureDimension.Tex2D;
			cmd.GetTemporaryRT(pass.rendererData.normalsRenderTargetId, desc, FilterMode.Bilinear);
		}
	}

	public static RenderTextureDescriptor GetBlendStyleRenderTextureDesc(this IRenderPass2D pass, RenderingData renderingData)
	{
		float num = Mathf.Clamp(pass.rendererData.lightRenderTextureScale, 0.01f, 1f);
		int width = (int)((float)renderingData.cameraData.cameraTargetDescriptor.width * num);
		int height = (int)((float)renderingData.cameraData.cameraTargetDescriptor.height * num);
		RenderTextureDescriptor result = new RenderTextureDescriptor(width, height);
		result.graphicsFormat = GetRenderTextureFormat();
		result.useMipMap = false;
		result.autoGenerateMips = false;
		result.depthBufferBits = 0;
		result.msaaSamples = 1;
		result.dimension = TextureDimension.Tex2D;
		return result;
	}

	public static void CreateCameraSortingLayerRenderTexture(this IRenderPass2D pass, RenderingData renderingData, CommandBuffer cmd, Downsampling downsamplingMethod)
	{
		float num = 1f;
		switch (downsamplingMethod)
		{
		case Downsampling._2xBilinear:
			num = 0.5f;
			break;
		case Downsampling._4xBox:
		case Downsampling._4xBilinear:
			num = 0.25f;
			break;
		}
		int width = (int)((float)renderingData.cameraData.cameraTargetDescriptor.width * num);
		int height = (int)((float)renderingData.cameraData.cameraTargetDescriptor.height * num);
		RenderTextureDescriptor desc = new RenderTextureDescriptor(width, height);
		desc.graphicsFormat = renderingData.cameraData.cameraTargetDescriptor.graphicsFormat;
		desc.useMipMap = false;
		desc.autoGenerateMips = false;
		desc.depthBufferBits = 0;
		desc.msaaSamples = 1;
		desc.dimension = TextureDimension.Tex2D;
		cmd.GetTemporaryRT(pass.rendererData.cameraSortingLayerRenderTargetId, desc, FilterMode.Bilinear);
	}

	public static void EnableBlendStyle(CommandBuffer cmd, int blendStyleIndex, bool enabled)
	{
		string keyword = k_UseBlendStyleKeywords[blendStyleIndex];
		if (enabled)
		{
			cmd.EnableShaderKeyword(keyword);
		}
		else
		{
			cmd.DisableShaderKeyword(keyword);
		}
	}

	public static void DisableAllKeywords(this IRenderPass2D pass, CommandBuffer cmd)
	{
		string[] array = k_UseBlendStyleKeywords;
		foreach (string keyword in array)
		{
			cmd.DisableShaderKeyword(keyword);
		}
	}

	public static void ReleaseRenderTextures(this IRenderPass2D pass, CommandBuffer cmd)
	{
		pass.rendererData.isNormalsRenderTargetValid = false;
		pass.rendererData.normalsRenderTargetScale = 0f;
		cmd.ReleaseTemporaryRT(pass.rendererData.normalsRenderTargetId);
		cmd.ReleaseTemporaryRT(pass.rendererData.shadowsRenderTargetId);
		cmd.ReleaseTemporaryRT(pass.rendererData.cameraSortingLayerRenderTargetId);
	}

	public static void DrawPointLight(CommandBuffer cmd, Light2D light, Mesh lightMesh, Material material)
	{
		Matrix4x4 matrix = Matrix4x4.TRS(s: new Vector3(light.pointLightOuterRadius, light.pointLightOuterRadius, light.pointLightOuterRadius), pos: light.transform.position, q: light.transform.rotation);
		cmd.DrawMesh(lightMesh, matrix, material);
	}

	private static bool CanCastShadows(Light2D light, int layerToRender)
	{
		if (light.shadowsEnabled && light.shadowIntensity > 0f)
		{
			return light.IsLitLayer(layerToRender);
		}
		return false;
	}

	private static bool CanCastVolumetricShadows(Light2D light, int endLayerValue)
	{
		int topMostLitLayer = light.GetTopMostLitLayer();
		if (light.volumetricShadowsEnabled && light.shadowVolumeIntensity > 0f)
		{
			return topMostLitLayer == endLayerValue;
		}
		return false;
	}

	private static bool ShouldRenderLight(Light2D light, int blendStyleIndex, int layerToRender)
	{
		if (light != null && light.lightType != Light2D.LightType.Global && light.blendStyleIndex == blendStyleIndex)
		{
			return light.IsLitLayer(layerToRender);
		}
		return false;
	}

	private static void RenderLightSet(IRenderPass2D pass, RenderingData renderingData, int blendStyleIndex, CommandBuffer cmd, int layerToRender, RenderTargetIdentifier renderTexture, List<Light2D> lights)
	{
		uint num = ShadowRendering.maxTextureCount * 4;
		bool flag = true;
		if (num < 1)
		{
			Debug.LogError("maxShadowTextureCount cannot be less than 1");
			return;
		}
		NativeArray<bool> nativeArray = new NativeArray<bool>(lights.Count, Allocator.Temp);
		int j;
		for (int i = 0; i < lights.Count; i += j)
		{
			long num2 = (uint)lights.Count - i;
			j = 0;
			int num3 = 0;
			for (; j < num2; j++)
			{
				if (num3 >= num)
				{
					break;
				}
				int index = i + j;
				Light2D light2D = lights[index];
				if (ShouldRenderLight(light2D, blendStyleIndex, layerToRender) && CanCastShadows(light2D, layerToRender))
				{
					nativeArray[index] = false;
					if (ShadowRendering.PrerenderShadows(pass, renderingData, cmd, layerToRender, light2D, num3, light2D.shadowIntensity))
					{
						nativeArray[index] = true;
						num3++;
					}
				}
			}
			if (num3 > 0 || flag)
			{
				cmd.SetRenderTarget(renderTexture, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
				flag = false;
			}
			num3 = 0;
			for (int k = 0; k < j; k++)
			{
				Light2D light2D2 = lights[i + k];
				if (!ShouldRenderLight(light2D2, blendStyleIndex, layerToRender))
				{
					continue;
				}
				Material lightMaterial = pass.rendererData.GetLightMaterial(light2D2, isVolume: false);
				if (lightMaterial == null)
				{
					continue;
				}
				Mesh lightMesh = light2D2.lightMesh;
				if (!(lightMesh == null))
				{
					if (nativeArray[i + k])
					{
						ShadowRendering.SetGlobalShadowTexture(cmd, light2D2, num3++);
					}
					else
					{
						ShadowRendering.DisableGlobalShadowTexture(cmd);
					}
					if (light2D2.lightType == Light2D.LightType.Sprite && light2D2.lightCookieSprite != null && light2D2.lightCookieSprite.texture != null)
					{
						cmd.SetGlobalTexture(k_CookieTexID, light2D2.lightCookieSprite.texture);
					}
					SetGeneralLightShaderGlobals(pass, cmd, light2D2);
					if (light2D2.normalMapQuality != Light2D.NormalMapQuality.Disabled || light2D2.lightType == Light2D.LightType.Point)
					{
						SetPointLightShaderGlobals(pass, cmd, light2D2);
					}
					if (light2D2.lightType == Light2D.LightType.Parametric || light2D2.lightType == Light2D.LightType.Freeform || light2D2.lightType == Light2D.LightType.Sprite)
					{
						cmd.DrawMesh(lightMesh, light2D2.transform.localToWorldMatrix, lightMaterial);
					}
					else if (light2D2.lightType == Light2D.LightType.Point)
					{
						DrawPointLight(cmd, light2D2, lightMesh, lightMaterial);
					}
				}
			}
			for (int num4 = num3 - 1; num4 >= 0; num4--)
			{
				ShadowRendering.ReleaseShadowRenderTexture(cmd, num4);
			}
		}
		nativeArray.Dispose();
	}

	public static void RenderLightVolumes(this IRenderPass2D pass, RenderingData renderingData, CommandBuffer cmd, int layerToRender, int endLayerValue, RenderTargetIdentifier renderTexture, RenderTargetIdentifier depthTexture, RenderBufferStoreAction intermediateStoreAction, RenderBufferStoreAction finalStoreAction, bool requiresRTInit, List<Light2D> lights)
	{
		uint num = ShadowRendering.maxTextureCount * 4;
		NativeArray<bool> nativeArray = new NativeArray<bool>(lights.Count, Allocator.Temp);
		if (num < 1)
		{
			Debug.LogError("maxShadowLightCount cannot be less than 1");
			return;
		}
		int num2 = lights.Count;
		if (intermediateStoreAction != finalStoreAction)
		{
			for (int num3 = lights.Count - 1; num3 >= 0; num3--)
			{
				if (lights[num3].renderVolumetricShadows)
				{
					num2 = num3;
					break;
				}
			}
		}
		int j;
		for (int i = 0; i < lights.Count; i += j)
		{
			long num4 = (uint)lights.Count - i;
			j = 0;
			int num5 = 0;
			for (; j < num4; j++)
			{
				if (num5 >= num)
				{
					break;
				}
				int index = i + j;
				Light2D light2D = lights[index];
				if (CanCastVolumetricShadows(light2D, endLayerValue))
				{
					nativeArray[index] = false;
					if (ShadowRendering.PrerenderShadows(pass, renderingData, cmd, layerToRender, light2D, num5, light2D.shadowVolumeIntensity))
					{
						nativeArray[index] = true;
						num5++;
					}
				}
			}
			if (num5 > 0 || requiresRTInit)
			{
				RenderBufferStoreAction renderBufferStoreAction = ((i + j >= num2) ? finalStoreAction : intermediateStoreAction);
				cmd.SetRenderTarget(renderTexture, RenderBufferLoadAction.Load, renderBufferStoreAction, depthTexture, RenderBufferLoadAction.Load, renderBufferStoreAction);
				requiresRTInit = false;
			}
			num5 = 0;
			for (int k = 0; k < j; k++)
			{
				Light2D light2D2 = lights[i + k];
				if (light2D2.lightType == Light2D.LightType.Global || light2D2.volumeIntensity <= 0f || !light2D2.volumeIntensityEnabled)
				{
					continue;
				}
				int topMostLitLayer = light2D2.GetTopMostLitLayer();
				if (endLayerValue == topMostLitLayer)
				{
					Material lightMaterial = pass.rendererData.GetLightMaterial(light2D2, isVolume: true);
					Mesh lightMesh = light2D2.lightMesh;
					if (nativeArray[i + k])
					{
						ShadowRendering.SetGlobalShadowTexture(cmd, light2D2, num5++);
					}
					else
					{
						ShadowRendering.DisableGlobalShadowTexture(cmd);
					}
					if (light2D2.lightType == Light2D.LightType.Sprite && light2D2.lightCookieSprite != null && light2D2.lightCookieSprite.texture != null)
					{
						cmd.SetGlobalTexture(k_CookieTexID, light2D2.lightCookieSprite.texture);
					}
					SetGeneralLightShaderGlobals(pass, cmd, light2D2);
					if (light2D2.normalMapQuality != Light2D.NormalMapQuality.Disabled || light2D2.lightType == Light2D.LightType.Point)
					{
						SetPointLightShaderGlobals(pass, cmd, light2D2);
					}
					if (light2D2.lightType == Light2D.LightType.Parametric || light2D2.lightType == Light2D.LightType.Freeform || light2D2.lightType == Light2D.LightType.Sprite)
					{
						cmd.DrawMesh(lightMesh, light2D2.transform.localToWorldMatrix, lightMaterial);
					}
					else if (light2D2.lightType == Light2D.LightType.Point)
					{
						DrawPointLight(cmd, light2D2, lightMesh, lightMaterial);
					}
				}
			}
			for (int num6 = num5 - 1; num6 >= 0; num6--)
			{
				ShadowRendering.ReleaseShadowRenderTexture(cmd, num6);
			}
		}
		nativeArray.Dispose();
	}

	public static void SetShapeLightShaderGlobals(this IRenderPass2D pass, CommandBuffer cmd)
	{
		for (int i = 0; i < pass.rendererData.lightBlendStyles.Length; i++)
		{
			Light2DBlendStyle light2DBlendStyle = pass.rendererData.lightBlendStyles[i];
			if (i >= k_BlendFactorsPropIDs.Length)
			{
				break;
			}
			cmd.SetGlobalVector(k_BlendFactorsPropIDs[i], light2DBlendStyle.blendFactors);
			cmd.SetGlobalVector(k_MaskFilterPropIDs[i], light2DBlendStyle.maskTextureChannelFilter.mask);
			cmd.SetGlobalVector(k_InvertedFilterPropIDs[i], light2DBlendStyle.maskTextureChannelFilter.inverted);
		}
		cmd.SetGlobalTexture(k_FalloffLookupID, pass.rendererData.fallOffLookup);
	}

	private static float GetNormalizedInnerRadius(Light2D light)
	{
		return light.pointLightInnerRadius / light.pointLightOuterRadius;
	}

	private static float GetNormalizedAngle(float angle)
	{
		return angle / 360f;
	}

	private static void GetScaledLightInvMatrix(Light2D light, out Matrix4x4 retMatrix)
	{
		float pointLightOuterRadius = light.pointLightOuterRadius;
		Vector3 one = Vector3.one;
		Vector3 s = new Vector3(one.x * pointLightOuterRadius, one.y * pointLightOuterRadius, one.z * pointLightOuterRadius);
		Transform transform = light.transform;
		Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, s);
		retMatrix = Matrix4x4.Inverse(m);
	}

	private static void SetGeneralLightShaderGlobals(IRenderPass2D pass, CommandBuffer cmd, Light2D light)
	{
		Color value = light.intensity * light.color.a * light.color;
		value.a = 1f;
		float volumeIntensity = light.volumeIntensity;
		cmd.SetGlobalFloat(k_FalloffIntensityID, light.falloffIntensity);
		cmd.SetGlobalFloat(k_FalloffDistanceID, light.shapeLightFalloffSize);
		cmd.SetGlobalColor(k_LightColorID, value);
		cmd.SetGlobalFloat(k_VolumeOpacityID, volumeIntensity);
	}

	private static void SetPointLightShaderGlobals(IRenderPass2D pass, CommandBuffer cmd, Light2D light)
	{
		GetScaledLightInvMatrix(light, out var retMatrix);
		float normalizedInnerRadius = GetNormalizedInnerRadius(light);
		float normalizedAngle = GetNormalizedAngle(light.pointLightInnerAngle);
		float normalizedAngle2 = GetNormalizedAngle(light.pointLightOuterAngle);
		float value = 1f / (1f - normalizedInnerRadius);
		cmd.SetGlobalVector(k_LightPositionID, light.transform.position);
		cmd.SetGlobalMatrix(k_LightInvMatrixID, retMatrix);
		cmd.SetGlobalFloat(k_InnerRadiusMultID, value);
		cmd.SetGlobalFloat(k_OuterAngleID, normalizedAngle2);
		cmd.SetGlobalFloat(k_InnerAngleMultID, 1f / (normalizedAngle2 - normalizedAngle));
		cmd.SetGlobalTexture(k_LightLookupID, Light2DLookupTexture.GetLightLookupTexture());
		cmd.SetGlobalTexture(k_FalloffLookupID, pass.rendererData.fallOffLookup);
		cmd.SetGlobalFloat(k_FalloffIntensityID, light.falloffIntensity);
		cmd.SetGlobalFloat(k_IsFullSpotlightID, (normalizedAngle == 1f) ? 1f : 0f);
		cmd.SetGlobalFloat(k_LightZDistanceID, light.normalMapDistance);
		if (light.lightCookieSprite != null && light.lightCookieSprite.texture != null)
		{
			cmd.SetGlobalTexture(k_PointLightCookieTexID, light.lightCookieSprite.texture);
		}
	}

	public static void ClearDirtyLighting(this IRenderPass2D pass, CommandBuffer cmd, uint blendStylesUsed)
	{
		for (int i = 0; i < pass.rendererData.lightBlendStyles.Length; i++)
		{
			if ((blendStylesUsed & (uint)(1 << i)) != 0 && pass.rendererData.lightBlendStyles[i].isDirty)
			{
				CoreUtils.SetRenderTarget(cmd, pass.rendererData.lightBlendStyles[i].renderTargetHandle, ClearFlag.Color, Color.black);
				pass.rendererData.lightBlendStyles[i].isDirty = false;
			}
		}
	}

	internal static void RenderNormals(this IRenderPass2D pass, ScriptableRenderContext context, RenderingData renderingData, DrawingSettings drawSettings, FilteringSettings filterSettings, RenderTargetIdentifier depthTarget, bool bFirstClear)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			float num = 0f;
			CreateNormalMapRenderTexture(renderScale: (!(depthTarget != BuiltinRenderTextureType.None)) ? Mathf.Clamp(pass.rendererData.lightRenderTextureScale, 0.01f, 1f) : 1f, pass: pass, renderingData: renderingData, cmd: commandBuffer);
			RenderBufferStoreAction renderBufferStoreAction = ((renderingData.cameraData.cameraTargetDescriptor.msaaSamples > 1) ? RenderBufferStoreAction.Resolve : RenderBufferStoreAction.Store);
			ClearFlag clearFlag = ((!(pass.rendererData.useDepthStencilBuffer && bFirstClear)) ? ClearFlag.Color : ClearFlag.All);
			if (depthTarget != BuiltinRenderTextureType.None)
			{
				CoreUtils.SetRenderTarget(commandBuffer, pass.rendererData.normalsRenderTarget, RenderBufferLoadAction.DontCare, renderBufferStoreAction, depthTarget, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, clearFlag, k_NormalClearColor);
			}
			else
			{
				CoreUtils.SetRenderTarget(commandBuffer, pass.rendererData.normalsRenderTarget, RenderBufferLoadAction.DontCare, renderBufferStoreAction, clearFlag, k_NormalClearColor);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			drawSettings.SetShaderPassName(0, k_NormalsRenderingPassName);
			context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);
		}
	}

	public static void RenderLights(this IRenderPass2D pass, RenderingData renderingData, CommandBuffer cmd, int layerToRender, ref LayerBatch layerBatch, ref RenderTextureDescriptor rtDesc)
	{
		List<Light2D> visibleLights = pass.rendererData.lightCullResult.visibleLights;
		for (int i = 0; i < visibleLights.Count; i++)
		{
			visibleLights[i].CacheValues();
		}
		ShadowCasterGroup2DManager.CacheValues();
		Light2DBlendStyle[] lightBlendStyles = pass.rendererData.lightBlendStyles;
		for (int j = 0; j < lightBlendStyles.Length; j++)
		{
			if ((layerBatch.lightStats.blendStylesUsed & (uint)(1 << j)) != 0)
			{
				string name = lightBlendStyles[j].name;
				cmd.BeginSample(name);
				if (!Light2DManager.GetGlobalColor(layerToRender, j, out var color))
				{
					color = Color.black;
				}
				bool num = (layerBatch.lightStats.blendStylesWithLights & (uint)(1 << j)) != 0;
				RenderTextureDescriptor desc = rtDesc;
				if (!num)
				{
					int width = (desc.height = 4);
					desc.width = width;
				}
				RenderTargetIdentifier rTId = layerBatch.GetRTId(cmd, desc, j);
				cmd.SetRenderTarget(rTId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
				cmd.ClearRenderTarget(clearDepth: false, clearColor: true, color);
				if (num)
				{
					RenderLightSet(pass, renderingData, j, cmd, layerToRender, rTId, pass.rendererData.lightCullResult.visibleLights);
				}
				cmd.EndSample(name);
			}
		}
	}

	private static void SetBlendModes(Material material, BlendMode src, BlendMode dst)
	{
		material.SetFloat(k_SrcBlendID, (float)src);
		material.SetFloat(k_DstBlendID, (float)dst);
	}

	private static uint GetLightMaterialIndex(Light2D light, bool isVolume)
	{
		bool isPointLight = light.isPointLight;
		int num = 0;
		uint num2 = (isVolume ? ((uint)(1 << num)) : 0u);
		num++;
		uint num3 = ((!isPointLight) ? ((uint)(1 << num)) : 0u);
		num++;
		uint num4 = ((light.overlapOperation != Light2D.OverlapOperation.AlphaBlend) ? ((uint)(1 << num)) : 0u);
		num++;
		uint num5 = ((light.lightType == Light2D.LightType.Sprite) ? ((uint)(1 << num)) : 0u);
		num++;
		uint num6 = ((isPointLight && light.lightCookieSprite != null && light.lightCookieSprite.texture != null) ? ((uint)(1 << num)) : 0u);
		num++;
		int num7 = ((light.normalMapQuality == Light2D.NormalMapQuality.Fast) ? (1 << num) : 0);
		num++;
		uint num8 = ((light.normalMapQuality != Light2D.NormalMapQuality.Disabled) ? ((uint)(1 << num)) : 0u);
		return (uint)num7 | num6 | num5 | num4 | num3 | num2 | num8;
	}

	private static Material CreateLightMaterial(Renderer2DData rendererData, Light2D light, bool isVolume)
	{
		bool isPointLight = light.isPointLight;
		Material material;
		if (isVolume)
		{
			material = CoreUtils.CreateEngineMaterial(isPointLight ? rendererData.pointLightVolumeShader : rendererData.shapeLightVolumeShader);
		}
		else
		{
			material = CoreUtils.CreateEngineMaterial(isPointLight ? rendererData.pointLightShader : rendererData.shapeLightShader);
			if (light.overlapOperation == Light2D.OverlapOperation.Additive)
			{
				SetBlendModes(material, BlendMode.One, BlendMode.One);
				material.EnableKeyword(k_UseAdditiveBlendingKeyword);
			}
			else
			{
				SetBlendModes(material, BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha);
			}
		}
		if (light.lightType == Light2D.LightType.Sprite)
		{
			material.EnableKeyword(k_SpriteLightKeyword);
		}
		if (isPointLight && light.lightCookieSprite != null && light.lightCookieSprite.texture != null)
		{
			material.EnableKeyword(k_UsePointLightCookiesKeyword);
		}
		if (light.normalMapQuality == Light2D.NormalMapQuality.Fast)
		{
			material.EnableKeyword(k_LightQualityFastKeyword);
		}
		if (light.normalMapQuality != Light2D.NormalMapQuality.Disabled)
		{
			material.EnableKeyword(k_UseNormalMap);
		}
		return material;
	}

	private static Material GetLightMaterial(this Renderer2DData rendererData, Light2D light, bool isVolume)
	{
		uint lightMaterialIndex = GetLightMaterialIndex(light, isVolume);
		if (!rendererData.lightMaterials.TryGetValue(lightMaterialIndex, out var value))
		{
			value = CreateLightMaterial(rendererData, light, isVolume);
			rendererData.lightMaterials[lightMaterialIndex] = value;
		}
		return value;
	}
}
