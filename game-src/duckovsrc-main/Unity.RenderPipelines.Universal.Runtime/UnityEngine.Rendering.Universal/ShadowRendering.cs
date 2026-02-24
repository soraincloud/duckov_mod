using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

internal static class ShadowRendering
{
	private static readonly int k_LightPosID = Shader.PropertyToID("_LightPos");

	private static readonly int k_SelfShadowingID = Shader.PropertyToID("_SelfShadowing");

	private static readonly int k_ShadowStencilGroupID = Shader.PropertyToID("_ShadowStencilGroup");

	private static readonly int k_ShadowIntensityID = Shader.PropertyToID("_ShadowIntensity");

	private static readonly int k_ShadowVolumeIntensityID = Shader.PropertyToID("_ShadowVolumeIntensity");

	private static readonly int k_ShadowRadiusID = Shader.PropertyToID("_ShadowRadius");

	private static readonly int k_ShadowColorMaskID = Shader.PropertyToID("_ShadowColorMask");

	private static readonly int k_ShadowModelMatrixID = Shader.PropertyToID("_ShadowModelMatrix");

	private static readonly int k_ShadowModelInvMatrixID = Shader.PropertyToID("_ShadowModelInvMatrix");

	private static readonly int k_ShadowModelScaleID = Shader.PropertyToID("_ShadowModelScale");

	private static readonly ProfilingSampler m_ProfilingSamplerShadows = new ProfilingSampler("Draw 2D Shadow Texture");

	private static readonly ProfilingSampler m_ProfilingSamplerShadowsA = new ProfilingSampler("Draw 2D Shadows (A)");

	private static readonly ProfilingSampler m_ProfilingSamplerShadowsR = new ProfilingSampler("Draw 2D Shadows (R)");

	private static readonly ProfilingSampler m_ProfilingSamplerShadowsG = new ProfilingSampler("Draw 2D Shadows (G)");

	private static readonly ProfilingSampler m_ProfilingSamplerShadowsB = new ProfilingSampler("Draw 2D Shadows (B)");

	private static RTHandle[] m_RenderTargets = null;

	private static int[] m_RenderTargetIds = null;

	private static RenderTargetIdentifier[] m_LightInputTextures = null;

	private static readonly Color[] k_ColorLookup = new Color[4]
	{
		new Color(0f, 0f, 0f, 1f),
		new Color(0f, 0f, 1f, 0f),
		new Color(0f, 1f, 0f, 0f),
		new Color(1f, 0f, 0f, 0f)
	};

	private static readonly ProfilingSampler[] m_ProfilingSamplerShadowColorsLookup = new ProfilingSampler[4] { m_ProfilingSamplerShadowsA, m_ProfilingSamplerShadowsB, m_ProfilingSamplerShadowsG, m_ProfilingSamplerShadowsR };

	public static uint maxTextureCount { get; private set; }

	public static void InitializeBudget(uint maxTextureCount)
	{
		if (m_RenderTargets == null || m_RenderTargets.Length != maxTextureCount)
		{
			m_RenderTargets = new RTHandle[maxTextureCount];
			m_RenderTargetIds = new int[maxTextureCount];
			ShadowRendering.maxTextureCount = maxTextureCount;
			for (int i = 0; i < maxTextureCount; i++)
			{
				m_RenderTargetIds[i] = Shader.PropertyToID($"ShadowTex_{i}");
				m_RenderTargets[i] = RTHandles.Alloc(m_RenderTargetIds[i], $"ShadowTex_{i}");
			}
		}
		if (m_LightInputTextures == null || m_LightInputTextures.Length != maxTextureCount)
		{
			m_LightInputTextures = new RenderTargetIdentifier[maxTextureCount];
		}
	}

	private static Material[] CreateMaterials(Shader shader, int pass = 0)
	{
		Material[] array = new Material[4];
		for (int i = 0; i < 4; i++)
		{
			array[i] = CoreUtils.CreateEngineMaterial(shader);
			array[i].SetInt(k_ShadowColorMaskID, 1 << i);
			array[i].SetPass(pass);
		}
		return array;
	}

	private static Material GetProjectedShadowMaterial(this Renderer2DData rendererData, int colorIndex)
	{
		if (rendererData.projectedShadowMaterial == null || rendererData.projectedShadowMaterial.Length == 0 || rendererData.projectedShadowShader != rendererData.projectedShadowMaterial[0].shader)
		{
			rendererData.projectedShadowMaterial = CreateMaterials(rendererData.projectedShadowShader);
		}
		return rendererData.projectedShadowMaterial[colorIndex];
	}

	private static Material GetStencilOnlyShadowMaterial(this Renderer2DData rendererData, int colorIndex)
	{
		if (rendererData.stencilOnlyShadowMaterial == null || rendererData.stencilOnlyShadowMaterial.Length == 0 || rendererData.projectedShadowShader != rendererData.stencilOnlyShadowMaterial[0].shader)
		{
			rendererData.stencilOnlyShadowMaterial = CreateMaterials(rendererData.projectedShadowShader, 1);
		}
		return rendererData.stencilOnlyShadowMaterial[colorIndex];
	}

	private static Material GetSpriteSelfShadowMaterial(this Renderer2DData rendererData, int colorIndex)
	{
		if (rendererData.spriteSelfShadowMaterial == null || rendererData.spriteSelfShadowMaterial.Length == 0 || rendererData.spriteShadowShader != rendererData.spriteSelfShadowMaterial[0].shader)
		{
			rendererData.spriteSelfShadowMaterial = CreateMaterials(rendererData.spriteShadowShader);
		}
		return rendererData.spriteSelfShadowMaterial[colorIndex];
	}

	private static Material GetSpriteUnshadowMaterial(this Renderer2DData rendererData, int colorIndex)
	{
		if (rendererData.spriteUnshadowMaterial == null || rendererData.spriteUnshadowMaterial.Length == 0 || rendererData.spriteUnshadowShader != rendererData.spriteUnshadowMaterial[0].shader)
		{
			rendererData.spriteUnshadowMaterial = CreateMaterials(rendererData.spriteUnshadowShader);
		}
		return rendererData.spriteUnshadowMaterial[colorIndex];
	}

	private static Material GetGeometryUnshadowMaterial(this Renderer2DData rendererData, int colorIndex)
	{
		if (rendererData.geometryUnshadowMaterial == null || rendererData.geometryUnshadowMaterial.Length == 0 || rendererData.geometryUnshadowShader != rendererData.geometryUnshadowMaterial[0].shader)
		{
			rendererData.geometryUnshadowMaterial = CreateMaterials(rendererData.geometryUnshadowShader);
		}
		return rendererData.geometryUnshadowMaterial[colorIndex];
	}

	public static void CreateShadowRenderTexture(IRenderPass2D pass, RenderingData renderingData, CommandBuffer cmdBuffer, int shadowIndex)
	{
		CreateShadowRenderTexture(pass, m_RenderTargetIds[shadowIndex], renderingData, cmdBuffer);
	}

	public static bool PrerenderShadows(IRenderPass2D pass, RenderingData renderingData, CommandBuffer cmdBuffer, int layerToRender, Light2D light, int shadowIndex, float shadowIntensity)
	{
		int num = shadowIndex % 4;
		int num2 = shadowIndex / 4;
		if (num == 0)
		{
			CreateShadowRenderTexture(pass, renderingData, cmdBuffer, num2);
		}
		bool result = RenderShadows(pass, renderingData, cmdBuffer, layerToRender, light, shadowIntensity, m_RenderTargets[num2].nameID, num);
		m_LightInputTextures[num2] = m_RenderTargets[num2].nameID;
		return result;
	}

	public static void SetGlobalShadowTexture(CommandBuffer cmdBuffer, Light2D light, int shadowIndex)
	{
		int num = shadowIndex % 4;
		int num2 = shadowIndex / 4;
		cmdBuffer.SetGlobalTexture("_ShadowTex", m_LightInputTextures[num2]);
		cmdBuffer.SetGlobalColor(k_ShadowColorMaskID, k_ColorLookup[num]);
		cmdBuffer.SetGlobalFloat(k_ShadowIntensityID, 1f - light.shadowIntensity);
		cmdBuffer.SetGlobalFloat(k_ShadowVolumeIntensityID, 1f - light.shadowVolumeIntensity);
	}

	public static void DisableGlobalShadowTexture(CommandBuffer cmdBuffer)
	{
		cmdBuffer.SetGlobalFloat(k_ShadowIntensityID, 1f);
		cmdBuffer.SetGlobalFloat(k_ShadowVolumeIntensityID, 1f);
	}

	private static void CreateShadowRenderTexture(IRenderPass2D pass, int handleId, RenderingData renderingData, CommandBuffer cmdBuffer)
	{
		float num = Mathf.Clamp(pass.rendererData.lightRenderTextureScale, 0.01f, 1f);
		int width = (int)((float)renderingData.cameraData.cameraTargetDescriptor.width * num);
		int height = (int)((float)renderingData.cameraData.cameraTargetDescriptor.height * num);
		RenderTextureDescriptor desc = new RenderTextureDescriptor(width, height);
		desc.useMipMap = false;
		desc.autoGenerateMips = false;
		desc.depthBufferBits = 24;
		desc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
		desc.msaaSamples = 1;
		desc.dimension = TextureDimension.Tex2D;
		cmdBuffer.GetTemporaryRT(handleId, desc, FilterMode.Bilinear);
	}

	public static void ReleaseShadowRenderTexture(CommandBuffer cmdBuffer, int shadowIndex)
	{
		int num = shadowIndex % 4;
		int num2 = shadowIndex / 4;
		if (num == 0)
		{
			cmdBuffer.ReleaseTemporaryRT(m_RenderTargetIds[num2]);
		}
	}

	public static void SetShadowProjectionGlobals(CommandBuffer cmdBuffer, ShadowCaster2D shadowCaster)
	{
		cmdBuffer.SetGlobalVector(k_ShadowModelScaleID, shadowCaster.m_CachedLossyScale);
		cmdBuffer.SetGlobalMatrix(k_ShadowModelMatrixID, shadowCaster.m_CachedShadowMatrix);
		cmdBuffer.SetGlobalMatrix(k_ShadowModelInvMatrixID, shadowCaster.m_CachedInverseShadowMatrix);
	}

	public static bool RenderShadows(IRenderPass2D pass, RenderingData renderingData, CommandBuffer cmdBuffer, int layerToRender, Light2D light, float shadowIntensity, RenderTargetIdentifier renderTexture, int colorBit)
	{
		using (new ProfilingScope(cmdBuffer, m_ProfilingSamplerShadows))
		{
			bool flag = false;
			List<ShadowCasterGroup2D> shadowCasterGroups = ShadowCasterGroup2DManager.shadowCasterGroups;
			if (shadowCasterGroups != null && shadowCasterGroups.Count > 0)
			{
				for (int i = 0; i < shadowCasterGroups.Count; i++)
				{
					List<ShadowCaster2D> shadowCasters = shadowCasterGroups[i].GetShadowCasters();
					if (shadowCasters == null)
					{
						continue;
					}
					for (int j = 0; j < shadowCasters.Count; j++)
					{
						ShadowCaster2D shadowCaster2D = shadowCasters[j];
						if (shadowCaster2D != null && shadowCaster2D.IsLit(light) && shadowCaster2D.IsShadowedLayer(layerToRender))
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					cmdBuffer.SetRenderTarget(renderTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
					using (new ProfilingScope(cmdBuffer, m_ProfilingSamplerShadowColorsLookup[colorBit]))
					{
						if (colorBit == 0)
						{
							cmdBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
						}
						else
						{
							cmdBuffer.ClearRenderTarget(clearDepth: true, clearColor: false, Color.clear);
						}
						float radius = light.boundingSphere.radius;
						cmdBuffer.SetGlobalVector(k_LightPosID, light.transform.position);
						cmdBuffer.SetGlobalFloat(k_ShadowRadiusID, radius);
						cmdBuffer.SetGlobalColor(k_ShadowColorMaskID, k_ColorLookup[colorBit]);
						Material geometryUnshadowMaterial = pass.rendererData.GetGeometryUnshadowMaterial(colorBit);
						Material projectedShadowMaterial = pass.rendererData.GetProjectedShadowMaterial(colorBit);
						Material spriteSelfShadowMaterial = pass.rendererData.GetSpriteSelfShadowMaterial(colorBit);
						Material spriteUnshadowMaterial = pass.rendererData.GetSpriteUnshadowMaterial(colorBit);
						pass.rendererData.GetStencilOnlyShadowMaterial(colorBit);
						for (int k = 0; k < shadowCasterGroups.Count; k++)
						{
							List<ShadowCaster2D> shadowCasters2 = shadowCasterGroups[k].GetShadowCasters();
							if (shadowCasters2 == null)
							{
								continue;
							}
							for (int l = 0; l < shadowCasters2.Count; l++)
							{
								ShadowCaster2D shadowCaster2D2 = shadowCasters2[l];
								if (shadowCaster2D2.IsLit(light) && shadowCaster2D2 != null && projectedShadowMaterial != null && shadowCaster2D2.IsShadowedLayer(layerToRender) && shadowCaster2D2.castsShadows)
								{
									SetShadowProjectionGlobals(cmdBuffer, shadowCaster2D2);
									cmdBuffer.DrawMesh(shadowCaster2D2.mesh, shadowCaster2D2.m_CachedLocalToWorldMatrix, geometryUnshadowMaterial, 0, 0);
									cmdBuffer.DrawMesh(shadowCaster2D2.mesh, shadowCaster2D2.m_CachedLocalToWorldMatrix, projectedShadowMaterial, 0, 0);
									cmdBuffer.DrawMesh(shadowCaster2D2.mesh, shadowCaster2D2.m_CachedLocalToWorldMatrix, geometryUnshadowMaterial, 0, 1);
								}
							}
							for (int m = 0; m < shadowCasters2.Count; m++)
							{
								ShadowCaster2D shadowCaster2D3 = shadowCasters2[m];
								if (!shadowCaster2D3.IsLit(light) || !(shadowCaster2D3 != null) || !shadowCaster2D3.IsShadowedLayer(layerToRender))
								{
									continue;
								}
								if (shadowCaster2D3.useRendererSilhouette)
								{
									Renderer component = null;
									shadowCaster2D3.TryGetComponent<Renderer>(out component);
									if (component != null)
									{
										Material material = (shadowCaster2D3.selfShadows ? spriteSelfShadowMaterial : spriteUnshadowMaterial);
										if (material != null)
										{
											cmdBuffer.DrawRenderer(component, material);
										}
									}
								}
								else
								{
									Matrix4x4 cachedLocalToWorldMatrix = shadowCaster2D3.m_CachedLocalToWorldMatrix;
									Material material2 = (shadowCaster2D3.selfShadows ? spriteSelfShadowMaterial : spriteUnshadowMaterial);
									if (material2 != null)
									{
										cmdBuffer.DrawMesh(shadowCaster2D3.mesh, cachedLocalToWorldMatrix, material2);
									}
								}
							}
							for (int n = 0; n < shadowCasters2.Count; n++)
							{
								ShadowCaster2D shadowCaster2D4 = shadowCasters2[n];
								if (shadowCaster2D4.IsLit(light) && shadowCaster2D4 != null && projectedShadowMaterial != null && shadowCaster2D4.IsShadowedLayer(layerToRender) && shadowCaster2D4.castsShadows)
								{
									SetShadowProjectionGlobals(cmdBuffer, shadowCaster2D4);
									cmdBuffer.DrawMesh(shadowCaster2D4.mesh, shadowCaster2D4.m_CachedLocalToWorldMatrix, projectedShadowMaterial, 0, 1);
								}
							}
						}
					}
				}
			}
			return flag;
		}
	}
}
