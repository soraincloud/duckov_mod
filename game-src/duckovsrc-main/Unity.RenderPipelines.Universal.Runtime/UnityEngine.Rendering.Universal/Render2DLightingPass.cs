using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal;

internal class Render2DLightingPass : ScriptableRenderPass, IRenderPass2D
{
	private static readonly int k_HDREmulationScaleID = Shader.PropertyToID("_HDREmulationScale");

	private static readonly int k_InverseHDREmulationScaleID = Shader.PropertyToID("_InverseHDREmulationScale");

	private static readonly int k_UseSceneLightingID = Shader.PropertyToID("_UseSceneLighting");

	private static readonly int k_RendererColorID = Shader.PropertyToID("_RendererColor");

	private static readonly int[] k_ShapeLightTextureIDs = new int[4]
	{
		Shader.PropertyToID("_ShapeLightTexture0"),
		Shader.PropertyToID("_ShapeLightTexture1"),
		Shader.PropertyToID("_ShapeLightTexture2"),
		Shader.PropertyToID("_ShapeLightTexture3")
	};

	private static readonly ShaderTagId k_CombinedRenderingPassName = new ShaderTagId("Universal2D");

	private static readonly ShaderTagId k_NormalsRenderingPassName = new ShaderTagId("NormalsRendering");

	private static readonly ShaderTagId k_LegacyPassName = new ShaderTagId("SRPDefaultUnlit");

	private static readonly List<ShaderTagId> k_ShaderTags = new List<ShaderTagId> { k_LegacyPassName, k_CombinedRenderingPassName };

	private static readonly ProfilingSampler m_ProfilingDrawLights = new ProfilingSampler("Draw 2D Lights");

	private static readonly ProfilingSampler m_ProfilingDrawLightTextures = new ProfilingSampler("Draw 2D Lights Textures");

	private static readonly ProfilingSampler m_ProfilingDrawRenderers = new ProfilingSampler("Draw All Renderers");

	private static readonly ProfilingSampler m_ProfilingDrawLayerBatch = new ProfilingSampler("Draw Layer Batch");

	private static readonly ProfilingSampler m_ProfilingSamplerUnlit = new ProfilingSampler("Render Unlit");

	private Material m_BlitMaterial;

	private Material m_SamplingMaterial;

	private readonly Renderer2DData m_Renderer2DData;

	private bool m_NeedsDepth;

	private short m_CameraSortingLayerBoundsIndex;

	Renderer2DData IRenderPass2D.rendererData => m_Renderer2DData;

	public Render2DLightingPass(Renderer2DData rendererData, Material blitMaterial, Material samplingMaterial)
	{
		m_Renderer2DData = rendererData;
		m_BlitMaterial = blitMaterial;
		m_SamplingMaterial = samplingMaterial;
		m_CameraSortingLayerBoundsIndex = GetCameraSortingLayerBoundsIndex();
	}

	internal void Setup(bool useDepth)
	{
		m_NeedsDepth = useDepth;
	}

	private void GetTransparencySortingMode(Camera camera, ref SortingSettings sortingSettings)
	{
		TransparencySortMode transparencySortMode = m_Renderer2DData.transparencySortMode;
		if (transparencySortMode == TransparencySortMode.Default)
		{
			transparencySortMode = ((!camera.orthographic) ? TransparencySortMode.Perspective : TransparencySortMode.Orthographic);
		}
		switch (transparencySortMode)
		{
		case TransparencySortMode.Perspective:
			sortingSettings.distanceMetric = DistanceMetric.Perspective;
			break;
		case TransparencySortMode.Orthographic:
			sortingSettings.distanceMetric = DistanceMetric.Orthographic;
			break;
		default:
			sortingSettings.distanceMetric = DistanceMetric.CustomAxis;
			sortingSettings.customAxis = m_Renderer2DData.transparencySortAxis;
			break;
		}
	}

	private void CopyCameraSortingLayerRenderTexture(ScriptableRenderContext context, RenderingData renderingData, RenderBufferStoreAction mainTargetStoreAction)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		this.CreateCameraSortingLayerRenderTexture(renderingData, commandBuffer, m_Renderer2DData.cameraSortingLayerDownsamplingMethod);
		Material material = m_SamplingMaterial;
		int pass = 0;
		if (m_Renderer2DData.cameraSortingLayerDownsamplingMethod != Downsampling._4xBox)
		{
			material = m_BlitMaterial;
			pass = ((base.colorAttachmentHandle.rt.filterMode == FilterMode.Bilinear) ? 1 : 0);
		}
		Blitter.BlitCameraTexture(commandBuffer, base.colorAttachmentHandle, m_Renderer2DData.cameraSortingLayerRenderTarget, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, material, pass);
		CoreUtils.SetRenderTarget(commandBuffer, base.colorAttachmentHandle, RenderBufferLoadAction.Load, mainTargetStoreAction, base.depthAttachmentHandle, RenderBufferLoadAction.Load, mainTargetStoreAction, ClearFlag.None, Color.clear);
		commandBuffer.SetGlobalTexture(m_Renderer2DData.cameraSortingLayerRenderTargetId, m_Renderer2DData.cameraSortingLayerRenderTarget.nameID);
		context.ExecuteCommandBuffer(commandBuffer);
		commandBuffer.Clear();
	}

	private short GetCameraSortingLayerBoundsIndex()
	{
		SortingLayer[] cachedSortingLayer = Light2DManager.GetCachedSortingLayer();
		for (short num = 0; num < cachedSortingLayer.Length; num++)
		{
			if (cachedSortingLayer[num].id == m_Renderer2DData.cameraSortingLayerTextureBound)
			{
				return (short)cachedSortingLayer[num].value;
			}
		}
		return short.MinValue;
	}

	private void DetermineWhenToResolve(int startIndex, int batchesDrawn, int batchCount, LayerBatch[] layerBatches, out int resolveDuringBatch, out bool resolveIsAfterCopy)
	{
		bool flag = false;
		List<Light2D> visibleLights = m_Renderer2DData.lightCullResult.visibleLights;
		for (int i = 0; i < visibleLights.Count; i++)
		{
			flag = visibleLights[i].renderVolumetricShadows;
			if (flag)
			{
				break;
			}
		}
		int num = -1;
		if (flag)
		{
			for (int num2 = startIndex + batchesDrawn - 1; num2 >= startIndex; num2--)
			{
				if (layerBatches[num2].lightStats.totalVolumetricUsage > 0)
				{
					num = num2;
					break;
				}
			}
		}
		if (m_Renderer2DData.useCameraSortingLayerTexture)
		{
			short cameraSortingLayerBoundsIndex = GetCameraSortingLayerBoundsIndex();
			int num3 = -1;
			for (int j = startIndex; j < startIndex + batchesDrawn; j++)
			{
				LayerBatch layerBatch = layerBatches[j];
				if (cameraSortingLayerBoundsIndex >= layerBatch.layerRange.lowerBound && cameraSortingLayerBoundsIndex <= layerBatch.layerRange.upperBound)
				{
					num3 = j;
					break;
				}
			}
			resolveIsAfterCopy = num3 > num;
			resolveDuringBatch = (resolveIsAfterCopy ? num3 : num);
		}
		else
		{
			resolveDuringBatch = num;
			resolveIsAfterCopy = false;
		}
	}

	private void Render(ScriptableRenderContext context, CommandBuffer cmd, ref RenderingData renderingData, ref FilteringSettings filterSettings, DrawingSettings drawSettings)
	{
		DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(ref renderingData);
		if (activeDebugHandler != null)
		{
			RenderStateBlock renderStateBlock = default(RenderStateBlock);
			activeDebugHandler.DrawWithDebugRenderState(context, cmd, ref renderingData, ref drawSettings, ref filterSettings, ref renderStateBlock, delegate(ScriptableRenderContext ctx, ref RenderingData data, ref DrawingSettings ds, ref FilteringSettings fs, ref RenderStateBlock rsb)
			{
				ctx.DrawRenderers(data.cullResults, ref ds, ref fs, ref rsb);
			});
		}
		else
		{
			context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);
		}
	}

	private int DrawLayerBatches(LayerBatch[] layerBatches, int batchCount, int startIndex, CommandBuffer cmd, ScriptableRenderContext context, ref RenderingData renderingData, ref FilteringSettings filterSettings, ref DrawingSettings normalsDrawSettings, ref DrawingSettings drawSettings, ref RenderTextureDescriptor desc)
	{
		bool flag = ScriptableRenderPass.GetActiveDebugHandler(ref renderingData)?.IsLightingActive ?? true;
		int num = 0;
		uint num2 = 0u;
		bool bFirstClear = true;
		using (new ProfilingScope(cmd, m_ProfilingDrawLights))
		{
			for (int i = startIndex; i < batchCount; i++)
			{
				ref LayerBatch reference = ref layerBatches[i];
				uint num3 = reference.lightStats.blendStylesUsed;
				uint num4 = 0u;
				while (num3 != 0)
				{
					num4 += num3 & 1;
					num3 >>= 1;
				}
				num2 += num4;
				if (num2 > LayerUtility.maxTextureCount)
				{
					break;
				}
				num++;
				if (reference.useNormals)
				{
					filterSettings.sortingLayerRange = reference.layerRange;
					RenderTargetIdentifier depthTarget = (m_NeedsDepth ? base.depthAttachmentHandle.nameID : ((RenderTargetIdentifier)BuiltinRenderTextureType.None));
					this.RenderNormals(context, renderingData, normalsDrawSettings, filterSettings, depthTarget, bFirstClear);
					bFirstClear = false;
				}
				using (new ProfilingScope(cmd, m_ProfilingDrawLightTextures))
				{
					this.RenderLights(renderingData, cmd, reference.startLayerID, ref reference, ref desc);
				}
			}
		}
		bool flag2 = renderingData.cameraData.cameraTargetDescriptor.msaaSamples > 1;
		bool flag3 = startIndex + num >= batchCount;
		int resolveDuringBatch = -1;
		bool resolveIsAfterCopy = false;
		if (flag2 && flag3)
		{
			DetermineWhenToResolve(startIndex, num, batchCount, layerBatches, out resolveDuringBatch, out resolveIsAfterCopy);
		}
		int num5 = m_Renderer2DData.lightBlendStyles.Length;
		using (new ProfilingScope(cmd, m_ProfilingDrawRenderers))
		{
			RenderBufferStoreAction colorStoreAction = (flag2 ? ((resolveDuringBatch < startIndex) ? RenderBufferStoreAction.Resolve : RenderBufferStoreAction.StoreAndResolve) : RenderBufferStoreAction.Store);
			CoreUtils.SetRenderTarget(cmd, base.colorAttachmentHandle, RenderBufferLoadAction.Load, colorStoreAction, base.depthAttachmentHandle, RenderBufferLoadAction.Load, colorStoreAction, ClearFlag.None, Color.clear);
			for (int j = startIndex; j < startIndex + num; j++)
			{
				using (new ProfilingScope(cmd, m_ProfilingDrawLayerBatch))
				{
					LayerBatch layerBatch = layerBatches[j];
					if (layerBatch.lightStats.totalLights > 0)
					{
						for (int k = 0; k < num5; k++)
						{
							uint num6 = (uint)(1 << k);
							bool flag4 = (layerBatch.lightStats.blendStylesUsed & num6) != 0;
							if (flag4)
							{
								RenderTargetIdentifier rTId = layerBatch.GetRTId(cmd, desc, k);
								cmd.SetGlobalTexture(k_ShapeLightTextureIDs[k], rTId);
							}
							RendererLighting.EnableBlendStyle(cmd, k, flag4);
						}
					}
					else
					{
						for (int l = 0; l < k_ShapeLightTextureIDs.Length; l++)
						{
							cmd.SetGlobalTexture(k_ShapeLightTextureIDs[l], Texture2D.blackTexture);
							RendererLighting.EnableBlendStyle(cmd, l, l == 0);
						}
					}
					context.ExecuteCommandBuffer(cmd);
					cmd.Clear();
					short cameraSortingLayerBoundsIndex = GetCameraSortingLayerBoundsIndex();
					RenderBufferStoreAction mainTargetStoreAction = (flag2 ? ((resolveDuringBatch == j && resolveIsAfterCopy) ? RenderBufferStoreAction.Resolve : RenderBufferStoreAction.StoreAndResolve) : RenderBufferStoreAction.Store);
					if (cameraSortingLayerBoundsIndex >= layerBatch.layerRange.lowerBound && cameraSortingLayerBoundsIndex < layerBatch.layerRange.upperBound && m_Renderer2DData.useCameraSortingLayerTexture)
					{
						filterSettings.sortingLayerRange = new SortingLayerRange(layerBatch.layerRange.lowerBound, cameraSortingLayerBoundsIndex);
						Render(context, cmd, ref renderingData, ref filterSettings, drawSettings);
						CopyCameraSortingLayerRenderTexture(context, renderingData, mainTargetStoreAction);
						filterSettings.sortingLayerRange = new SortingLayerRange((short)(cameraSortingLayerBoundsIndex + 1), layerBatch.layerRange.upperBound);
						Render(context, cmd, ref renderingData, ref filterSettings, drawSettings);
					}
					else
					{
						filterSettings.sortingLayerRange = new SortingLayerRange(layerBatch.layerRange.lowerBound, layerBatch.layerRange.upperBound);
						Render(context, cmd, ref renderingData, ref filterSettings, drawSettings);
						if (cameraSortingLayerBoundsIndex == layerBatch.layerRange.upperBound && m_Renderer2DData.useCameraSortingLayerTexture)
						{
							CopyCameraSortingLayerRenderTexture(context, renderingData, mainTargetStoreAction);
						}
					}
					if (flag && layerBatch.lightStats.totalVolumetricUsage > 0)
					{
						string name = "Render 2D Light Volumes";
						cmd.BeginSample(name);
						RendererLighting.RenderLightVolumes(finalStoreAction: flag2 ? ((resolveDuringBatch == j && !resolveIsAfterCopy) ? RenderBufferStoreAction.Resolve : RenderBufferStoreAction.StoreAndResolve) : RenderBufferStoreAction.Store, pass: this, renderingData: renderingData, cmd: cmd, layerToRender: layerBatch.startLayerID, endLayerValue: layerBatch.endLayerValue, renderTexture: base.colorAttachmentHandle.nameID, depthTexture: base.depthAttachmentHandle.nameID, intermediateStoreAction: RenderBufferStoreAction.Store, requiresRTInit: false, lights: m_Renderer2DData.lightCullResult.visibleLights);
						cmd.EndSample(name);
					}
				}
			}
		}
		for (int m = startIndex; m < startIndex + num; m++)
		{
			layerBatches[m].ReleaseRT(cmd);
		}
		return num;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		bool flag = true;
		Camera camera = renderingData.cameraData.camera;
		FilteringSettings filterSettings = new FilteringSettings
		{
			renderQueueRange = RenderQueueRange.all,
			layerMask = -1,
			renderingLayerMask = uint.MaxValue,
			sortingLayerRange = SortingLayerRange.all
		};
		LayerUtility.InitializeBudget(m_Renderer2DData.lightRenderTextureMemoryBudget);
		ShadowRendering.InitializeBudget(m_Renderer2DData.shadowRenderTextureMemoryBudget);
		if (m_Renderer2DData.lightCullResult.IsSceneLit())
		{
			DrawingSettings drawSettings = CreateDrawingSettings(k_ShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
			DrawingSettings normalsDrawSettings = CreateDrawingSettings(k_NormalsRenderingPassName, ref renderingData, SortingCriteria.CommonTransparent);
			SortingSettings sortingSettings = drawSettings.sortingSettings;
			GetTransparencySortingMode(camera, ref sortingSettings);
			drawSettings.sortingSettings = sortingSettings;
			normalsDrawSettings.sortingSettings = sortingSettings;
			CommandBuffer commandBuffer = renderingData.commandBuffer;
			commandBuffer.SetGlobalFloat(k_HDREmulationScaleID, m_Renderer2DData.hdrEmulationScale);
			commandBuffer.SetGlobalFloat(k_InverseHDREmulationScaleID, 1f / m_Renderer2DData.hdrEmulationScale);
			commandBuffer.SetGlobalFloat(k_UseSceneLightingID, flag ? 1f : 0f);
			commandBuffer.SetGlobalColor(k_RendererColorID, Color.white);
			this.SetShapeLightShaderGlobals(commandBuffer);
			RenderTextureDescriptor desc = this.GetBlendStyleRenderTextureDesc(renderingData);
			int batchCount;
			LayerBatch[] layerBatches = LayerUtility.CalculateBatches(m_Renderer2DData.lightCullResult, out batchCount);
			int num = 0;
			for (int i = 0; i < batchCount; i += num)
			{
				num = DrawLayerBatches(layerBatches, batchCount, i, commandBuffer, context, ref renderingData, ref filterSettings, ref normalsDrawSettings, ref drawSettings, ref desc);
			}
			this.DisableAllKeywords(commandBuffer);
			this.ReleaseRenderTextures(commandBuffer);
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
		}
		else
		{
			DrawingSettings drawSettings2 = CreateDrawingSettings(k_ShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
			RenderBufferStoreAction renderBufferStoreAction = ((renderingData.cameraData.cameraTargetDescriptor.msaaSamples > 1) ? RenderBufferStoreAction.Resolve : RenderBufferStoreAction.Store);
			SortingSettings sortingSettings2 = drawSettings2.sortingSettings;
			GetTransparencySortingMode(camera, ref sortingSettings2);
			drawSettings2.sortingSettings = sortingSettings2;
			CommandBuffer commandBuffer2 = renderingData.commandBuffer;
			using (new ProfilingScope(commandBuffer2, m_ProfilingSamplerUnlit))
			{
				CoreUtils.SetRenderTarget(commandBuffer2, base.colorAttachmentHandle, RenderBufferLoadAction.Load, renderBufferStoreAction, base.depthAttachmentHandle, RenderBufferLoadAction.Load, renderBufferStoreAction, ClearFlag.None, Color.clear);
				commandBuffer2.SetGlobalFloat(k_UseSceneLightingID, flag ? 1f : 0f);
				commandBuffer2.SetGlobalColor(k_RendererColorID, Color.white);
				for (int j = 0; j < k_ShapeLightTextureIDs.Length; j++)
				{
					if (j == 0)
					{
						commandBuffer2.SetGlobalTexture(k_ShapeLightTextureIDs[j], Texture2D.blackTexture);
					}
					RendererLighting.EnableBlendStyle(commandBuffer2, j, j == 0);
				}
			}
			this.DisableAllKeywords(commandBuffer2);
			context.ExecuteCommandBuffer(commandBuffer2);
			commandBuffer2.Clear();
			if (m_Renderer2DData.useCameraSortingLayerTexture)
			{
				filterSettings.sortingLayerRange = new SortingLayerRange(short.MinValue, m_CameraSortingLayerBoundsIndex);
				Render(context, commandBuffer2, ref renderingData, ref filterSettings, drawSettings2);
				CopyCameraSortingLayerRenderTexture(context, renderingData, renderBufferStoreAction);
				filterSettings.sortingLayerRange = new SortingLayerRange((short)(m_CameraSortingLayerBoundsIndex + 1), short.MaxValue);
				Render(context, commandBuffer2, ref renderingData, ref filterSettings, drawSettings2);
			}
			else
			{
				Render(context, commandBuffer2, ref renderingData, ref filterSettings, drawSettings2);
			}
		}
		filterSettings.sortingLayerRange = SortingLayerRange.all;
	}
}
