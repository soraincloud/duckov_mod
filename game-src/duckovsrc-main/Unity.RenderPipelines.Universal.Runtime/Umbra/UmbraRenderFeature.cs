using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Umbra;

[DisallowMultipleRendererFeature("Umbra Render Feature")]
[Tooltip("Umbra Render Feature")]
internal class UmbraRenderFeature : ScreenSpaceShadows
{
	private static class ShaderParams
	{
		public static readonly int MainTex = Shader.PropertyToID("_MainTex");

		public static readonly int ShadowData = Shader.PropertyToID("_ShadowData");

		public static readonly int ShadowData2 = Shader.PropertyToID("_ShadowData2");

		public static readonly int ShadowData3 = Shader.PropertyToID("_ShadowData3");

		public static readonly int ShadowData4 = Shader.PropertyToID("_ShadowData4");

		public static readonly int BlurTemp = Shader.PropertyToID("_BlurTemp");

		public static readonly int BlurTemp2 = Shader.PropertyToID("_BlurTemp2");

		public static readonly int BlurScale = Shader.PropertyToID("_BlurScale");

		public static readonly int BlurSpread = Shader.PropertyToID("_BlurSpread");

		public static readonly int UmbraCascadeRects = Shader.PropertyToID("_UmbraCascadeRects");

		public static readonly int UmbraCascadeScales = Shader.PropertyToID("_UmbraCascadeScales");

		public static readonly int DownsampledDepth = Shader.PropertyToID("_DownsampledDepth");

		public static readonly int NoiseTex = Shader.PropertyToID("_NoiseTex");

		public static readonly int SourceSize = Shader.PropertyToID("_SourceSize");

		public static readonly int BlendCascadeData = Shader.PropertyToID("_BlendCascadeData");

		public static readonly int MaskTexture = Shader.PropertyToID("_MaskTex");

		public static readonly int CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");

		public static readonly int ContactShadowsSampleCount = Shader.PropertyToID("_ContactShadowsSampleCount");

		public static readonly int ContactShadowsData1 = Shader.PropertyToID("_ContactShadowsData1");

		public static readonly int ContactShadowsData2 = Shader.PropertyToID("_ContactShadowsData2");

		public static readonly int ContactShadowsData3 = Shader.PropertyToID("_ContactShadowsData3");

		public static Vector3 Vector3Back = Vector3.back;

		public static Vector3 Vector3Forward = Vector3.forward;

		public const string SKW_LOOP_STEP_X3 = "_LOOP_STEP_X3";

		public const string SKW_LOOP_STEP_X2 = "_LOOP_STEP_X2";

		public const string SKW_PRESERVE_EDGES = "_PRESERVE_EDGES";

		public const string SKW_BLUR_HQ = "_BLUR_HQ";

		public const string SKW_NORMALS_TEXTURE = "_NORMALS_TEXTURE";

		public const string SKW_CONTACT_HARDENING = "_CONTACT_HARDENING";

		public const string SKW_MASK_TEXTURE = "_MASK_TEXTURE";
	}

	private enum Pass
	{
		UmbraCastShadows,
		BlurHoriz,
		BlurVert,
		BoxBlur,
		ComposeWithBlending,
		DownsampledDepth,
		CascadeBlending,
		UnityShadows,
		ComposeUnity,
		ContactShadows,
		Compose,
		DebugShadows,
		ContactShadowsAfterOpaque
	}

	private struct CameraLocation
	{
		public Vector3 position;

		public Vector3 forward;
	}

	private class UmbraScreenSpaceShadowsPass : ScriptableRenderPass
	{
		private static string m_ProfilerTag = "UmbraSoftShadows";

		private static ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);

		public static Material mat;

		private static RTHandle m_RenderTarget;

		private static RTHandle m_DownscaledRenderTarget;

		private static readonly Vector4[][] cascadeRects = new Vector4[4][]
		{
			new Vector4[4]
			{
				new Vector4(0f, 0f, 1f, 1f),
				new Vector4(0f, 0f, 1f, 1f),
				new Vector4(0f, 0f, 1f, 1f),
				new Vector4(0f, 0f, 1f, 1f)
			},
			new Vector4[4]
			{
				new Vector4(0f, 0f, 0.5f, 1f),
				new Vector4(0.5f, 0f, 1f, 1f),
				new Vector4(0f, 0f, 1f, 1f),
				new Vector4(0f, 0f, 1f, 1f)
			},
			new Vector4[4]
			{
				new Vector4(0f, 0f, 0.5f, 0.5f),
				new Vector4(0.5f, 0f, 1f, 0.5f),
				new Vector4(0f, 0.5f, 0.5f, 1f),
				new Vector4(0.5f, 0.5f, 1f, 1f)
			},
			new Vector4[4]
			{
				new Vector4(0f, 0f, 0.5f, 0.5f),
				new Vector4(0.5f, 0f, 1f, 0.5f),
				new Vector4(0f, 0.5f, 0.5f, 1f),
				new Vector4(0.5f, 0.5f, 1f, 1f)
			}
		};

		private static readonly Vector4[][] cascadeRectsWithPadding = new Vector4[4][]
		{
			new Vector4[4]
			{
				new Vector4(0f, 0f, 1f, 1f),
				new Vector4(0f, 0f, 1f, 1f),
				new Vector4(0f, 0f, 1f, 1f),
				new Vector4(0f, 0f, 1f, 1f)
			},
			new Vector4[4]
			{
				new Vector4(0f, 0f, 0.5f, 1f),
				new Vector4(0.5f, 0f, 1f, 1f),
				new Vector4(0f, 0f, 1f, 1f),
				new Vector4(0f, 0f, 1f, 1f)
			},
			new Vector4[4]
			{
				new Vector4(0f, 0f, 0.5f, 0.5f),
				new Vector4(0.5f, 0f, 1f, 0.5f),
				new Vector4(0f, 0.5f, 0.5f, 1f),
				new Vector4(0.5f, 0.5f, 1f, 1f)
			},
			new Vector4[4]
			{
				new Vector4(0f, 0f, 0.5f, 0.5f),
				new Vector4(0.5f, 0f, 1f, 0.5f),
				new Vector4(0f, 0.5f, 0.5f, 1f),
				new Vector4(0.5f, 0.5f, 1f, 1f)
			}
		};

		private static readonly float[] cascadeScales = new float[4] { 1f, 1f, 1f, 1f };

		private static RenderTextureDescriptor desc;

		private GraphicsFormat screenShadowTextureFormat;

		public readonly Dictionary<Camera, RTHandle> shadowTextures = new Dictionary<Camera, RTHandle>();

		private static bool newShadowmap = true;

		private static readonly float[] autoCascadeScales = new float[4];

		private static Mesh _fullScreenMesh;

		private static Mesh fullscreenMesh
		{
			get
			{
				if (_fullScreenMesh != null)
				{
					return _fullScreenMesh;
				}
				float y = 1f;
				float y2 = 0f;
				_fullScreenMesh = new Mesh();
				_fullScreenMesh.SetVertices(new List<Vector3>
				{
					new Vector3(-1f, -1f, 0f),
					new Vector3(-1f, 1f, 0f),
					new Vector3(1f, -1f, 0f),
					new Vector3(1f, 1f, 0f)
				});
				_fullScreenMesh.SetUVs(0, new List<Vector2>
				{
					new Vector2(0f, y2),
					new Vector2(0f, y),
					new Vector2(1f, y2),
					new Vector2(1f, y)
				});
				_fullScreenMesh.SetIndices(new int[6] { 0, 1, 2, 2, 1, 3 }, MeshTopology.Triangles, 0, calculateBounds: false);
				_fullScreenMesh.UploadMeshData(markNoLongerReadable: true);
				return _fullScreenMesh;
			}
		}

		public void Dispose()
		{
			foreach (RTHandle value in shadowTextures.Values)
			{
				value?.Release();
			}
			shadowTextures.Clear();
			m_RenderTarget?.Release();
			m_DownscaledRenderTarget?.Release();
		}

		internal bool Setup(Material material)
		{
			if (settings == null || !settings.enabled || settings.profile == null)
			{
				return false;
			}
			mat = material;
			UmbraProfile profile = settings.profile;
			GraphicsFormat graphicsFormat = ((profile.shadowSource == ShadowSource.UmbraShadows && profile.blurIterations > 0 && profile.enableContactHardening) ? GraphicsFormat.R8G8_UNorm : GraphicsFormat.R8_UNorm);
			screenShadowTextureFormat = (RenderingUtils.SupportsGraphicsFormat(graphicsFormat, FormatUsage.Blend) ? graphicsFormat : GraphicsFormat.B8G8R8A8_UNorm);
			if (usesCachedShadowmap)
			{
				ConfigureInput(ScriptableRenderPassInput.None);
			}
			else if (!UmbraSoftShadows.isDeferred && profile.shadowSource == ShadowSource.UmbraShadows && (profile.normalsSource == NormalSource.NormalsPass || profile.downsample))
			{
				ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
			}
			else
			{
				ConfigureInput(ScriptableRenderPassInput.Depth);
			}
			return mat != null;
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			desc = renderingData.cameraData.cameraTargetDescriptor;
			desc.depthBufferBits = 0;
			desc.msaaSamples = 1;
			desc.graphicsFormat = screenShadowTextureFormat;
			UmbraProfile profile = settings.profile;
			if (profile.downsample && !profile.preserveEdges)
			{
				desc.width /= 2;
				desc.height /= 2;
			}
			Camera camera = renderingData.cameraData.camera;
			if (profile.frameSkipOptimization)
			{
				newShadowmap = !shadowTextures.TryGetValue(camera, out m_RenderTarget);
			}
			if (RenderingUtils.ReAllocateIfNeeded(ref m_RenderTarget, in desc, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_ScreenSpaceShadowmapTexture"))
			{
				newShadowmap = true;
			}
			if (newShadowmap)
			{
				shadowTextures[camera] = m_RenderTarget;
			}
			cmd.SetGlobalTexture(m_RenderTarget.name, m_RenderTarget.nameID);
			ConfigureTarget(m_RenderTarget);
			ConfigureClear(ClearFlag.None, Color.white);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if (mat == null)
			{
				Debug.LogError("Umbra material not initialized");
				return;
			}
			UmbraProfile profile = settings.profile;
			CommandBuffer commandBuffer = renderingData.commandBuffer;
			using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
			{
				int frameCount = Time.frameCount;
				if (!profile.frameSkipOptimization || newShadowmap || !usesCachedShadowmap)
				{
					cachedShadowmapTimestap = frameCount;
					newShadowmap = false;
					RTHandle rTHandle;
					if (profile.downsample && profile.preserveEdges)
					{
						desc.width /= 2;
						desc.height /= 2;
						RenderingUtils.ReAllocateIfNeeded(ref m_DownscaledRenderTarget, in desc, FilterMode.Point, TextureWrapMode.Clamp);
						rTHandle = m_DownscaledRenderTarget;
					}
					else
					{
						rTHandle = m_RenderTarget;
					}
					int mainLightShadowCascadesCount = renderingData.shadowData.mainLightShadowCascadesCount;
					float farClipPlane = renderingData.cameraData.camera.farClipPlane;
					UniversalRenderPipelineAsset universalRenderPipelineAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
					float shadowDistance = universalRenderPipelineAsset.shadowDistance;
					Pass pass;
					if (profile.shadowSource == ShadowSource.UnityShadows)
					{
						pass = Pass.UnityShadows;
					}
					else
					{
						pass = Pass.UmbraCastShadows;
						commandBuffer.SetGlobalVector(ShaderParams.ShadowData, new Vector4(profile.sampleCount, 1024f / Mathf.Pow(2f, profile.posterization), profile.blurEdgeTolerance * 1000f, (profile.blurDepthAttenStart + profile.blurDepthAttenLength) / farClipPlane));
						commandBuffer.SetGlobalVector(ShaderParams.ShadowData2, new Vector4(1f - profile.contactStrength, profile.distantSpread, shadowDistance / farClipPlane, profile.lightSize * 0.02f));
						commandBuffer.SetGlobalVector(ShaderParams.ShadowData3, new Vector4(profile.blurDepthAttenStart / farClipPlane, profile.blurDepthAttenLength / farClipPlane, profile.blurGrazingAttenuation, profile.blurEdgeSharpness));
						commandBuffer.SetGlobalVector(ShaderParams.ShadowData4, new Vector4(profile.occludersCount, profile.occludersSearchRadius * 0.02f, (profile.contactStrength > 0f) ? (profile.contactStrengthKnee * 0.1f) : 1E-05f, profile.maskScale));
						commandBuffer.SetGlobalVector(ShaderParams.SourceSize, new Vector4(desc.width, desc.height, 0f, 0f));
						if (mainLightShadowCascadesCount > 1)
						{
							float shadowNearPlane = renderingData.lightData.visibleLights[shadowLightIndex].light.shadowNearPlane;
							for (int i = 0; i < mainLightShadowCascadesCount; i++)
							{
								renderingData.cullResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(shadowLightIndex, i, mainLightShadowCascadesCount, renderingData.shadowData.mainLightShadowCascadesSplit, universalRenderPipelineAsset.mainLightShadowmapResolution, shadowNearPlane, out var _, out var projMatrix, out var _);
								Matrix4x4 inverse = projMatrix.inverse;
								autoCascadeScales[i] = Mathf.Abs(inverse.MultiplyPoint(ShaderParams.Vector3Back).z - inverse.MultiplyPoint(ShaderParams.Vector3Forward).z) / 100f;
							}
							Vector4[] array = cascadeRectsWithPadding[mainLightShadowCascadesCount - 1];
							float num = 1f / (float)universalRenderPipelineAsset.mainLightShadowmapResolution;
							for (int j = 0; j < 4; j++)
							{
								Vector4 vector = cascadeRects[mainLightShadowCascadesCount - 1][j];
								Vector4 vector2 = array[j];
								vector2.x = vector.x + num;
								vector2.y = vector.y + num;
								vector2.z = vector.z - num;
								vector2.w = vector.w - num;
								array[j] = vector2;
							}
							commandBuffer.SetGlobalVectorArray(ShaderParams.UmbraCascadeRects, array);
							cascadeScales[0] = profile.cascade1Scale * autoCascadeScales[0];
							cascadeScales[1] = profile.cascade2Scale * autoCascadeScales[1];
							cascadeScales[2] = profile.cascade3Scale * autoCascadeScales[2];
							cascadeScales[3] = profile.cascade4Scale * autoCascadeScales[3];
							commandBuffer.SetGlobalFloatArray(ShaderParams.UmbraCascadeScales, cascadeScales);
						}
						if (UmbraSoftShadows.isDeferred || profile.normalsSource == NormalSource.NormalsPass || profile.downsample)
						{
							mat.EnableKeyword("_NORMALS_TEXTURE");
						}
						else
						{
							mat.DisableKeyword("_NORMALS_TEXTURE");
						}
						if (profile.enableContactHardening)
						{
							mat.EnableKeyword("_CONTACT_HARDENING");
						}
						else
						{
							mat.DisableKeyword("_CONTACT_HARDENING");
						}
						mat.DisableKeyword("_LOOP_STEP_X3");
						mat.DisableKeyword("_LOOP_STEP_X2");
						if (profile.loopStepOptimization == LoopStep.x3)
						{
							mat.EnableKeyword("_LOOP_STEP_X3");
						}
						else if (profile.loopStepOptimization == LoopStep.x2)
						{
							mat.EnableKeyword("_LOOP_STEP_X2");
						}
						if (profile.style == Style.Textured && profile.maskTexture != null)
						{
							mat.EnableKeyword("_MASK_TEXTURE");
							mat.SetTexture(ShaderParams.MaskTexture, profile.maskTexture);
						}
						else
						{
							mat.DisableKeyword("_MASK_TEXTURE");
						}
					}
					Blitter.BlitCameraTexture(commandBuffer, m_RenderTarget, rTHandle, mat, (int)pass);
					if (profile.shadowSource == ShadowSource.UmbraShadows && profile.blendCascades && mainLightShadowCascadesCount > 1)
					{
						commandBuffer.SetGlobalVector(ShaderParams.BlendCascadeData, new Vector4(profile.cascade1BlendingStrength * 100f, profile.cascade2BlendingStrength * 100f, profile.cascade3BlendingStrength * 100f, 1f));
						Blitter.BlitCameraTexture(commandBuffer, rTHandle, rTHandle, mat, 6);
					}
					if (profile.contactShadows)
					{
						mat.SetInt(ShaderParams.ContactShadowsSampleCount, profile.contactShadowsSampleCount);
						float z = ((settings.profile.shadowSource != ShadowSource.UnityShadows && settings.profile.contactShadowsInjectionPoint != ContactShadowsInjectionPoint.AfterOpaque) ? (shadowDistance / farClipPlane) : 1f);
						mat.SetVector(ShaderParams.ContactShadowsData1, new Vector4(profile.contactShadowsStepping, profile.contactShadowsIntensityMultiplier, profile.contactShadowsJitter, profile.contactShadowsDistanceFade));
						mat.SetVector(ShaderParams.ContactShadowsData2, new Vector4(profile.contactShadowsStartDistance / farClipPlane, profile.contactShadowsStartDistanceFade / farClipPlane, z, profile.contactShadowsNormalBias));
						mat.SetVector(ShaderParams.ContactShadowsData3, new Vector4(profile.contactShadowsThicknessNear / farClipPlane, profile.contactShadowsThicknessDistanceMultiplier * 0.1f, profile.contactShadowsVignetteSize, 0f));
						if (!settings.debugShadows && profile.contactShadowsInjectionPoint == ContactShadowsInjectionPoint.ShadowTexture)
						{
							Blitter.BlitCameraTexture(commandBuffer, rTHandle, rTHandle, mat, 9);
						}
					}
					if (profile.downsample && profile.preserveEdges)
					{
						RenderTextureDescriptor renderTextureDescriptor = desc;
						renderTextureDescriptor.colorFormat = RenderTextureFormat.RFloat;
						commandBuffer.GetTemporaryRT(ShaderParams.DownsampledDepth, renderTextureDescriptor);
						FullScreenBlit(commandBuffer, ShaderParams.DownsampledDepth, mat, 5);
						mat.EnableKeyword("_PRESERVE_EDGES");
					}
					else
					{
						mat.DisableKeyword("_PRESERVE_EDGES");
					}
					if (profile.shadowSource == ShadowSource.UnityShadows)
					{
						if (profile.downsample && profile.preserveEdges)
						{
							FullScreenBlit(commandBuffer, m_DownscaledRenderTarget, m_RenderTarget, mat, 8);
						}
					}
					else if (profile.style == Style.Default && profile.blurIterations > 0)
					{
						commandBuffer.SetGlobalFloat(ShaderParams.BlurSpread, profile.blurSpread);
						commandBuffer.GetTemporaryRT(ShaderParams.BlurTemp, desc);
						commandBuffer.GetTemporaryRT(ShaderParams.BlurTemp2, desc);
						commandBuffer.SetGlobalFloat(ShaderParams.BlurScale, 1f);
						RenderTargetIdentifier source = rTHandle;
						RenderTargetIdentifier renderTargetIdentifier = ShaderParams.BlurTemp2;
						if (profile.blurType == BlurType.Box)
						{
							for (int k = 0; k < profile.blurIterations; k++)
							{
								renderTargetIdentifier = ((k % 2 == 0) ? ShaderParams.BlurTemp2 : ShaderParams.BlurTemp);
								FullScreenBlit(commandBuffer, source, renderTargetIdentifier, mat, 3);
								source = renderTargetIdentifier;
								commandBuffer.SetGlobalFloat(ShaderParams.BlurScale, (float)k + 1f);
							}
						}
						else
						{
							if (profile.blurType == BlurType.Gaussian15)
							{
								mat.EnableKeyword("_BLUR_HQ");
							}
							else
							{
								mat.DisableKeyword("_BLUR_HQ");
							}
							FullScreenBlit(commandBuffer, source, ShaderParams.BlurTemp, mat, 1);
							commandBuffer.SetGlobalFloat(ShaderParams.BlurScale, 1f);
							for (int l = 0; l < profile.blurIterations - 1; l++)
							{
								FullScreenBlit(commandBuffer, ShaderParams.BlurTemp, ShaderParams.BlurTemp2, mat, 2);
								commandBuffer.SetGlobalFloat(ShaderParams.BlurScale, (float)l + 2f);
								FullScreenBlit(commandBuffer, ShaderParams.BlurTemp2, ShaderParams.BlurTemp, mat, 1);
							}
							FullScreenBlit(commandBuffer, ShaderParams.BlurTemp, ShaderParams.BlurTemp2, mat, 2);
						}
						FullScreenBlit(commandBuffer, renderTargetIdentifier, m_RenderTarget, mat, 4);
					}
					else if (profile.downsample && profile.preserveEdges)
					{
						FullScreenBlit(commandBuffer, m_DownscaledRenderTarget, m_RenderTarget, mat, 10);
					}
				}
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS", state: false);
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS_CASCADE", state: false);
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS_SCREEN", state: true);
			}
		}

		private static void FullScreenBlit(CommandBuffer cmd, RenderTargetIdentifier destination, Material material, int passIndex)
		{
			destination = new RenderTargetIdentifier(destination, 0, CubemapFace.Unknown, -1);
			cmd.SetRenderTarget(destination);
			cmd.DrawMesh(fullscreenMesh, Matrix4x4.identity, material, 0, passIndex);
		}

		private static void FullScreenBlit(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material, int passIndex)
		{
			destination = new RenderTargetIdentifier(destination, 0, CubemapFace.Unknown, -1);
			cmd.SetRenderTarget(destination);
			cmd.SetGlobalTexture(ShaderParams.MainTex, source);
			cmd.DrawMesh(fullscreenMesh, Matrix4x4.identity, material, 0, passIndex);
		}
	}

	private class UmbraScreenSpaceShadowsPostPass : ScriptableRenderPass
	{
		private static string m_ProfilerTag = "Umbra Screen Space Shadows Post Pass";

		private static ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);

		private static readonly RTHandle k_CurrentActive = RTHandles.Alloc(BuiltinRenderTextureType.CurrentActive);

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			ConfigureTarget(k_CurrentActive);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer commandBuffer = renderingData.commandBuffer;
			using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
			{
				int mainLightShadowCascadesCount = renderingData.shadowData.mainLightShadowCascadesCount;
				bool supportsMainLightShadows = renderingData.shadowData.supportsMainLightShadows;
				bool state = supportsMainLightShadows && mainLightShadowCascadesCount == 1;
				bool state2 = supportsMainLightShadows && mainLightShadowCascadesCount > 1;
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS_SCREEN", state: false);
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS", state);
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS_CASCADE", state2);
			}
		}
	}

	private class UmbraDebugPass : ScriptableRenderPass
	{
		private static string m_ProfilerTag = "Umbra Debug Pass";

		private static ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);

		private RTHandle source;

		private static UmbraScreenSpaceShadowsPass shadowPass;

		public void Setup(UmbraScreenSpaceShadowsPass shadowPass)
		{
			UmbraDebugPass.shadowPass = shadowPass;
			if (settings.debugShadows)
			{
				ConfigureInput(ScriptableRenderPassInput.Depth);
			}
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			source = renderingData.cameraData.renderer.cameraColorTargetHandle;
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			Material mat = UmbraScreenSpaceShadowsPass.mat;
			if (mat == null)
			{
				return;
			}
			RTHandle rTHandle = null;
			Camera camera = renderingData.cameraData.camera;
			Dictionary<Camera, RTHandle> shadowTextures = shadowPass.shadowTextures;
			if (shadowTextures != null)
			{
				rTHandle = shadowTextures[camera];
			}
			if (rTHandle == null)
			{
				return;
			}
			CommandBuffer commandBuffer = renderingData.commandBuffer;
			using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
			{
				Blitter.BlitCameraTexture(commandBuffer, rTHandle, source, mat, 11);
				if (settings.debugShadows && settings.profile != null && settings.profile.contactShadows)
				{
					Blitter.BlitCameraTexture(commandBuffer, source, source, mat, 12);
				}
			}
		}
	}

	private class UmbraContactShadowsAfterOpaquePass : ScriptableRenderPass
	{
		private static string m_ProfilerTag = "Umbra Contact Shadows After Opaque Pass";

		private static ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);

		private RTHandle source;

		private static UmbraScreenSpaceShadowsPass shadowPass;

		public void Setup(UmbraScreenSpaceShadowsPass shadowPass)
		{
			UmbraContactShadowsAfterOpaquePass.shadowPass = shadowPass;
			if (settings.debugShadows)
			{
				ConfigureInput(ScriptableRenderPassInput.Depth);
			}
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			source = renderingData.cameraData.renderer.cameraColorTargetHandle;
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			Material mat = UmbraScreenSpaceShadowsPass.mat;
			if (mat == null)
			{
				return;
			}
			CommandBuffer commandBuffer = renderingData.commandBuffer;
			using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
			{
				Blitter.BlitCameraTexture(commandBuffer, source, source, mat, 12);
			}
		}
	}

	private const string k_ShaderName = "Hidden/Kronnect/UmbraScreenSpaceShadows";

	[Tooltip("Specify which cameras can render Umbra Soft Shadows")]
	public LayerMask camerasLayerMask = -1;

	public static UmbraSoftShadows settings;

	public static int shadowLightIndex;

	private static int cachedShadowmapTimestap;

	private static bool usesCachedShadowmap;

	private readonly Dictionary<Camera, CameraLocation> cameraPrevLocation = new Dictionary<Camera, CameraLocation>();

	private static readonly Dictionary<Light, UmbraSoftShadows> umbraSettings = new Dictionary<Light, UmbraSoftShadows>();

	private Material mat;

	private UmbraScreenSpaceShadowsPass m_SSShadowsPass;

	private UmbraScreenSpaceShadowsPostPass m_SSShadowsPostPass;

	private UmbraDebugPass m_SSSShadowsDebugPass;

	private UmbraContactShadowsAfterOpaquePass m_ContactShadowsAfterOpaquePass;

	public static void RegisterUmbraLight(UmbraSoftShadows settings)
	{
		Light component = settings.GetComponent<Light>();
		if (component != null)
		{
			if (component.type == LightType.Directional)
			{
				umbraSettings[component] = settings;
			}
			else
			{
				Debug.LogError("Umbra Soft Shadows only work on directiona light.");
			}
		}
	}

	public static void UnregisterUmbraLight(UmbraSoftShadows settings)
	{
		Light component = settings.GetComponent<Light>();
		if (component != null && umbraSettings.ContainsKey(component))
		{
			umbraSettings.Remove(component);
		}
	}

	public override void Create()
	{
		if (m_SSShadowsPass == null)
		{
			m_SSShadowsPass = new UmbraScreenSpaceShadowsPass();
		}
		if (m_SSShadowsPostPass == null)
		{
			m_SSShadowsPostPass = new UmbraScreenSpaceShadowsPostPass();
		}
		if (m_ContactShadowsAfterOpaquePass == null)
		{
			m_ContactShadowsAfterOpaquePass = new UmbraContactShadowsAfterOpaquePass();
		}
		if (m_SSSShadowsDebugPass == null)
		{
			m_SSSShadowsDebugPass = new UmbraDebugPass();
		}
		cachedShadowmapTimestap = -100;
		LoadMaterial();
		m_SSShadowsPass.renderPassEvent = RenderPassEvent.AfterRenderingGbuffer;
		m_SSShadowsPostPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
		m_ContactShadowsAfterOpaquePass.renderPassEvent = (RenderPassEvent)301;
		m_SSSShadowsDebugPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
	}

	private void OnDisable()
	{
		UmbraSoftShadows.installed = false;
	}

	protected override void Dispose(bool disposing)
	{
		m_SSShadowsPass?.Dispose();
		m_SSShadowsPass = null;
		CoreUtils.Destroy(mat);
	}

	private static bool IsOffscreenDepthTexture(ref CameraData cameraData)
	{
		if (cameraData.targetTexture != null)
		{
			return cameraData.targetTexture.format == RenderTextureFormat.Depth;
		}
		return false;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		UmbraSoftShadows.installed = true;
		if (IsOffscreenDepthTexture(ref renderingData.cameraData))
		{
			return;
		}
		if (!LoadMaterial())
		{
			Debug.LogError("Umbra: can't load material");
			return;
		}
		shadowLightIndex = renderingData.lightData.mainLightIndex;
		if (shadowLightIndex < 0)
		{
			return;
		}
		Light light = renderingData.lightData.visibleLights[shadowLightIndex].light;
		if (light == null || !umbraSettings.TryGetValue(light, out settings) || settings == null)
		{
			return;
		}
		Camera camera = renderingData.cameraData.camera;
		usesCachedShadowmap = cachedShadowmapTimestap == Time.frameCount - 1 && settings != null && settings.profile != null && settings.profile.frameSkipOptimization && Application.isPlaying;
		if (usesCachedShadowmap)
		{
			Transform transform = camera.transform;
			Vector3 position = transform.position;
			Vector3 forward = transform.forward;
			bool flag = true;
			if (cameraPrevLocation.TryGetValue(camera, out var value))
			{
				float num = position.x - value.position.x;
				float num2 = position.y - value.position.y;
				float num3 = position.z - value.position.z;
				if (num < 0f)
				{
					num = 0f - num;
				}
				if (num2 < 0f)
				{
					num2 = 0f - num2;
				}
				if (num3 < 0f)
				{
					num3 = 0f - num3;
				}
				float skipFrameMaxCameraDisplacement = settings.profile.skipFrameMaxCameraDisplacement;
				if (num <= skipFrameMaxCameraDisplacement && num2 <= skipFrameMaxCameraDisplacement && num3 <= skipFrameMaxCameraDisplacement && Vector3.Angle(value.forward, forward) <= settings.profile.skipFrameMaxCameraRotation)
				{
					flag = false;
				}
			}
			if (flag)
			{
				value.position = position;
				value.forward = forward;
				cameraPrevLocation[camera] = value;
				usesCachedShadowmap = false;
			}
		}
		UmbraSoftShadows.isDeferred = renderer is UniversalRenderer && ((UniversalRenderer)renderer).renderingModeRequested == RenderingMode.Deferred;
		if (renderingData.shadowData.supportsMainLightShadows && renderingData.lightData.mainLightIndex != -1 && renderingData.lightData.visibleLights[renderingData.lightData.mainLightIndex].light.shadowStrength > 0f && ((int)camerasLayerMask & (1 << camera.gameObject.layer)) != 0 && m_SSShadowsPass.Setup(mat))
		{
			m_SSShadowsPass.renderPassEvent = (UmbraSoftShadows.isDeferred ? RenderPassEvent.AfterRenderingGbuffer : ((RenderPassEvent)201));
			renderer.EnqueuePass(m_SSShadowsPass);
			renderer.EnqueuePass(m_SSShadowsPostPass);
			if (!settings.debugShadows && settings.profile != null && settings.profile.contactShadows && (settings.profile.shadowSource == ShadowSource.UnityShadows || settings.profile.contactShadowsInjectionPoint == ContactShadowsInjectionPoint.AfterOpaque))
			{
				m_ContactShadowsAfterOpaquePass.Setup(m_SSShadowsPass);
				renderer.EnqueuePass(m_ContactShadowsAfterOpaquePass);
			}
			if (settings.debugShadows)
			{
				m_SSSShadowsDebugPass.Setup(m_SSShadowsPass);
				renderer.EnqueuePass(m_SSSShadowsDebugPass);
			}
		}
	}

	private bool LoadMaterial()
	{
		if (mat != null)
		{
			return true;
		}
		Shader shader = Shader.Find("Hidden/Kronnect/UmbraScreenSpaceShadows");
		if (shader == null)
		{
			return false;
		}
		mat = CoreUtils.CreateEngineMaterial(shader);
		Texture2D value = Resources.Load<Texture2D>("Umbra/Textures/NoiseTex");
		mat.SetTexture(ShaderParams.NoiseTex, value);
		return mat != null;
	}
}
