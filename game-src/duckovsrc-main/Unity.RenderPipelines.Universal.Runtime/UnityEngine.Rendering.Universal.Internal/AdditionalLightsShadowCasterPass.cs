using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

public class AdditionalLightsShadowCasterPass : ScriptableRenderPass
{
	private static class AdditionalShadowsConstantBuffer
	{
		public static int _AdditionalLightsWorldToShadow;

		public static int _AdditionalShadowParams;

		public static int _AdditionalShadowOffset0;

		public static int _AdditionalShadowOffset1;

		public static int _AdditionalShadowFadeParams;

		public static int _AdditionalShadowmapSize;
	}

	internal struct ShadowResolutionRequest
	{
		public int visibleLightIndex;

		public int perLightShadowSliceIndex;

		public int requestedResolution;

		public bool softShadow;

		public bool pointLightShadow;

		public int offsetX;

		public int offsetY;

		public int allocatedResolution;

		public ShadowResolutionRequest(int _visibleLightIndex, int _perLightShadowSliceIndex, int _requestedResolution, bool _softShadow, bool _pointLightShadow)
		{
			visibleLightIndex = _visibleLightIndex;
			perLightShadowSliceIndex = _perLightShadowSliceIndex;
			requestedResolution = _requestedResolution;
			softShadow = _softShadow;
			pointLightShadow = _pointLightShadow;
			offsetX = 0;
			offsetY = 0;
			allocatedResolution = 0;
		}
	}

	private class PassData
	{
		internal AdditionalLightsShadowCasterPass pass;

		internal RenderGraph graph;

		internal TextureHandle shadowmapTexture;

		internal RenderingData renderingData;

		internal int shadowmapID;

		internal bool emptyShadowmap;
	}

	[Obsolete("AdditionalLightsShadowCasterPass.m_AdditionalShadowsBufferId was deprecated. Shadow slice matrix is now passed to the GPU using an entry in buffer m_AdditionalLightsWorldToShadow_SSBO", false)]
	public static int m_AdditionalShadowsBufferId;

	[Obsolete("AdditionalLightsShadowCasterPass.m_AdditionalShadowsIndicesId was deprecated. Shadow slice index is now passed to the GPU using last member of an entry in buffer m_AdditionalShadowParams_SSBO", false)]
	public static int m_AdditionalShadowsIndicesId;

	private static readonly Vector4 c_DefaultShadowParams = new Vector4(0f, 0f, 0f, -1f);

	private static int m_AdditionalLightsWorldToShadow_SSBO;

	private static int m_AdditionalShadowParams_SSBO;

	private bool m_UseStructuredBuffer;

	private const int k_ShadowmapBufferBits = 16;

	private int m_AdditionalLightsShadowmapID;

	internal RTHandle m_AdditionalLightsShadowmapHandle;

	private bool m_CreateEmptyShadowmap;

	private RTHandle m_EmptyAdditionalLightShadowmapTexture;

	private const int k_EmptyShadowMapDimensions = 1;

	private const string k_AdditionalLightShadowMapTextureName = "_AdditionalLightsShadowmapTexture";

	private const string k_EmptyAdditionalLightShadowMapTextureName = "_EmptyAdditionalLightShadowmapTexture";

	internal static Vector4[] s_EmptyAdditionalLightIndexToShadowParams = null;

	private float m_MaxShadowDistanceSq;

	private float m_CascadeBorder;

	private ShadowSliceData[] m_AdditionalLightsShadowSlices;

	private int[] m_VisibleLightIndexToAdditionalLightIndex;

	private int[] m_AdditionalLightIndexToVisibleLightIndex;

	private List<int> m_ShadowSliceToAdditionalLightIndex = new List<int>();

	private List<int> m_GlobalShadowSliceIndexToPerLightShadowSliceIndex = new List<int>();

	private Vector4[] m_AdditionalLightIndexToShadowParams;

	private Matrix4x4[] m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix;

	private List<ShadowResolutionRequest> m_ShadowResolutionRequests = new List<ShadowResolutionRequest>();

	private float[] m_VisibleLightIndexToCameraSquareDistance;

	private ShadowResolutionRequest[] m_SortedShadowResolutionRequests;

	private int[] m_VisibleLightIndexToSortedShadowResolutionRequestsFirstSliceIndex;

	private List<RectInt> m_UnusedAtlasSquareAreas = new List<RectInt>();

	private int renderTargetWidth;

	private int renderTargetHeight;

	private ProfilingSampler m_ProfilingSetupSampler = new ProfilingSampler("Setup Additional Shadows");

	private const float LightTypeIdentifierInShadowParams_Spot = 0f;

	private const float LightTypeIdentifierInShadowParams_Point = 1f;

	private const int kMinimumPunctualLightHardShadowResolution = 8;

	private const int kMinimumPunctualLightSoftShadowResolution = 16;

	private Dictionary<int, ulong> m_ShadowRequestsHashes = new Dictionary<int, ulong>();

	public AdditionalLightsShadowCasterPass(RenderPassEvent evt)
	{
		base.profilingSampler = new ProfilingSampler("AdditionalLightsShadowCasterPass");
		base.renderPassEvent = evt;
		AdditionalShadowsConstantBuffer._AdditionalLightsWorldToShadow = Shader.PropertyToID("_AdditionalLightsWorldToShadow");
		AdditionalShadowsConstantBuffer._AdditionalShadowParams = Shader.PropertyToID("_AdditionalShadowParams");
		AdditionalShadowsConstantBuffer._AdditionalShadowOffset0 = Shader.PropertyToID("_AdditionalShadowOffset0");
		AdditionalShadowsConstantBuffer._AdditionalShadowOffset1 = Shader.PropertyToID("_AdditionalShadowOffset1");
		AdditionalShadowsConstantBuffer._AdditionalShadowFadeParams = Shader.PropertyToID("_AdditionalShadowFadeParams");
		AdditionalShadowsConstantBuffer._AdditionalShadowmapSize = Shader.PropertyToID("_AdditionalShadowmapSize");
		m_AdditionalLightsShadowmapID = Shader.PropertyToID("_AdditionalLightsShadowmapTexture");
		m_AdditionalLightsWorldToShadow_SSBO = Shader.PropertyToID("_AdditionalLightsWorldToShadow_SSBO");
		m_AdditionalShadowParams_SSBO = Shader.PropertyToID("_AdditionalShadowParams_SSBO");
		m_UseStructuredBuffer = RenderingUtils.useStructuredBuffer;
		int maxVisibleAdditionalLights = UniversalRenderPipeline.maxVisibleAdditionalLights;
		int num = maxVisibleAdditionalLights + 1;
		int num2 = (m_UseStructuredBuffer ? num : Math.Min(num, maxVisibleAdditionalLights));
		m_AdditionalLightIndexToVisibleLightIndex = new int[num2];
		m_VisibleLightIndexToAdditionalLightIndex = new int[num];
		m_VisibleLightIndexToSortedShadowResolutionRequestsFirstSliceIndex = new int[num];
		m_AdditionalLightIndexToShadowParams = new Vector4[num2];
		m_VisibleLightIndexToCameraSquareDistance = new float[num];
		s_EmptyAdditionalLightIndexToShadowParams = new Vector4[num2];
		for (int i = 0; i < s_EmptyAdditionalLightIndexToShadowParams.Length; i++)
		{
			s_EmptyAdditionalLightIndexToShadowParams[i] = c_DefaultShadowParams;
		}
		if (!m_UseStructuredBuffer)
		{
			m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix = new Matrix4x4[maxVisibleAdditionalLights];
			m_UnusedAtlasSquareAreas.Capacity = maxVisibleAdditionalLights;
			m_ShadowResolutionRequests.Capacity = maxVisibleAdditionalLights;
		}
	}

	public void Dispose()
	{
		m_AdditionalLightsShadowmapHandle?.Release();
	}

	private int GetPunctualLightShadowSlicesCount(in LightType lightType)
	{
		return lightType switch
		{
			LightType.Spot => 1, 
			LightType.Point => 6, 
			_ => 0, 
		};
	}

	internal static float CalcGuardAngle(float frustumAngleInDegrees, float guardBandSizeInTexels, float sliceResolutionInTexels)
	{
		float num = frustumAngleInDegrees * (MathF.PI / 180f) / 2f;
		float num2 = Mathf.Tan(num);
		float num3 = sliceResolutionInTexels / 2f;
		float num4 = guardBandSizeInTexels / 2f;
		float num5 = 1f + num4 / num3;
		float num6 = Mathf.Atan(num2 * num5) - num;
		return 2f * num6 * 57.29578f;
	}

	private int MinimalPunctualLightShadowResolution(bool softShadow)
	{
		if (!softShadow)
		{
			return 8;
		}
		return 16;
	}

	internal static float GetPointLightShadowFrustumFovBiasInDegrees(int shadowSliceResolution, bool shadowFiltering)
	{
		float num = 4f;
		if (shadowSliceResolution > 8)
		{
			if (shadowSliceResolution <= 16)
			{
				num = 43f;
			}
			else if (shadowSliceResolution <= 32)
			{
				num = 18.55f;
			}
			else if (shadowSliceResolution <= 64)
			{
				num = 8.63f;
			}
			else if (shadowSliceResolution <= 128)
			{
				num = 4.13f;
			}
			else if (shadowSliceResolution <= 256)
			{
				num = 2.03f;
			}
			else if (shadowSliceResolution <= 512)
			{
				num = 1f;
			}
			else if (shadowSliceResolution <= 1024)
			{
				num = 0.5f;
			}
		}
		if (shadowFiltering && shadowSliceResolution > 16)
		{
			if (shadowSliceResolution <= 32)
			{
				num += 9.35f;
			}
			else if (shadowSliceResolution <= 64)
			{
				num += 4.07f;
			}
			else if (shadowSliceResolution <= 128)
			{
				num += 1.77f;
			}
			else if (shadowSliceResolution <= 256)
			{
				num += 0.85f;
			}
			else if (shadowSliceResolution <= 512)
			{
				num += 0.39f;
			}
			else if (shadowSliceResolution <= 1024)
			{
				num += 0.17f;
			}
		}
		return num;
	}

	internal void InsertionSort(ShadowResolutionRequest[] array, int startIndex, int lastIndex)
	{
		for (int i = startIndex + 1; i < lastIndex; i++)
		{
			ShadowResolutionRequest shadowResolutionRequest = array[i];
			int num = i - 1;
			while (num >= 0 && (shadowResolutionRequest.requestedResolution > array[num].requestedResolution || (shadowResolutionRequest.requestedResolution == array[num].requestedResolution && !shadowResolutionRequest.softShadow && array[num].softShadow) || (shadowResolutionRequest.requestedResolution == array[num].requestedResolution && shadowResolutionRequest.softShadow == array[num].softShadow && !shadowResolutionRequest.pointLightShadow && array[num].pointLightShadow) || (shadowResolutionRequest.requestedResolution == array[num].requestedResolution && shadowResolutionRequest.softShadow == array[num].softShadow && shadowResolutionRequest.pointLightShadow == array[num].pointLightShadow && m_VisibleLightIndexToCameraSquareDistance[shadowResolutionRequest.visibleLightIndex] < m_VisibleLightIndexToCameraSquareDistance[array[num].visibleLightIndex]) || (shadowResolutionRequest.requestedResolution == array[num].requestedResolution && shadowResolutionRequest.softShadow == array[num].softShadow && shadowResolutionRequest.pointLightShadow == array[num].pointLightShadow && m_VisibleLightIndexToCameraSquareDistance[shadowResolutionRequest.visibleLightIndex] == m_VisibleLightIndexToCameraSquareDistance[array[num].visibleLightIndex] && shadowResolutionRequest.visibleLightIndex < array[num].visibleLightIndex) || (shadowResolutionRequest.requestedResolution == array[num].requestedResolution && shadowResolutionRequest.softShadow == array[num].softShadow && shadowResolutionRequest.pointLightShadow == array[num].pointLightShadow && m_VisibleLightIndexToCameraSquareDistance[shadowResolutionRequest.visibleLightIndex] == m_VisibleLightIndexToCameraSquareDistance[array[num].visibleLightIndex] && shadowResolutionRequest.visibleLightIndex == array[num].visibleLightIndex && shadowResolutionRequest.perLightShadowSliceIndex < array[num].perLightShadowSliceIndex)))
			{
				array[num + 1] = array[num];
				num--;
			}
			array[num + 1] = shadowResolutionRequest;
		}
	}

	private int EstimateScaleFactorNeededToFitAllShadowsInAtlas(in ShadowResolutionRequest[] shadowResolutionRequests, int endIndex, int atlasWidth)
	{
		long num = atlasWidth * atlasWidth;
		long num2 = 0L;
		for (int i = 0; i < endIndex; i++)
		{
			num2 += shadowResolutionRequests[i].requestedResolution * shadowResolutionRequests[i].requestedResolution;
		}
		int num3 = 1;
		while (num2 > num * num3 * num3)
		{
			num3 *= 2;
		}
		return num3;
	}

	private void AtlasLayout(int atlasSize, int totalShadowSlicesCount, int estimatedScaleFactor)
	{
		bool flag = false;
		bool flag2 = false;
		int num = estimatedScaleFactor;
		while (!flag && !flag2)
		{
			m_UnusedAtlasSquareAreas.Clear();
			m_UnusedAtlasSquareAreas.Add(new RectInt(0, 0, atlasSize, atlasSize));
			flag = true;
			for (int i = 0; i < totalShadowSlicesCount; i++)
			{
				int num2 = m_SortedShadowResolutionRequests[i].requestedResolution / num;
				if (num2 < MinimalPunctualLightShadowResolution(m_SortedShadowResolutionRequests[i].softShadow))
				{
					flag2 = true;
					break;
				}
				bool flag3 = false;
				for (int j = 0; j < m_UnusedAtlasSquareAreas.Count; j++)
				{
					RectInt rectInt = m_UnusedAtlasSquareAreas[j];
					int width = rectInt.width;
					int height = rectInt.height;
					int x = rectInt.x;
					int y = rectInt.y;
					if (width < num2)
					{
						continue;
					}
					m_SortedShadowResolutionRequests[i].offsetX = x;
					m_SortedShadowResolutionRequests[i].offsetY = y;
					m_SortedShadowResolutionRequests[i].allocatedResolution = num2;
					m_UnusedAtlasSquareAreas.RemoveAt(j);
					int num3 = totalShadowSlicesCount - i - 1;
					int k = 0;
					int num4 = num2;
					int num5 = num2;
					int num6 = x;
					int num7 = y;
					for (; k < num3; k++)
					{
						num6 += num4;
						if (num6 + num4 > x + width)
						{
							num6 = x;
							num7 += num5;
							if (num7 + num5 > y + height)
							{
								break;
							}
						}
						m_UnusedAtlasSquareAreas.Insert(j + k, new RectInt(num6, num7, num4, num5));
					}
					flag3 = true;
					break;
				}
				if (!flag3)
				{
					flag = false;
					break;
				}
			}
			if (!flag && !flag2)
			{
				num *= 2;
			}
		}
	}

	private ulong ResolutionLog2ForHash(int resolution)
	{
		return resolution switch
		{
			4096 => 12uL, 
			2048 => 11uL, 
			1024 => 10uL, 
			512 => 9uL, 
			_ => 8uL, 
		};
	}

	private ulong ComputeShadowRequestHash(ref RenderingData renderingData)
	{
		ulong num = 0uL;
		ulong num2 = 0uL;
		ulong num3 = 0uL;
		ulong num4 = 0uL;
		ulong num5 = 0uL;
		ulong num6 = 0uL;
		ulong num7 = 0uL;
		ulong num8 = 0uL;
		NativeArray<VisibleLight> visibleLights = renderingData.lightData.visibleLights;
		for (int i = 0; i < visibleLights.Length; i++)
		{
			if (IsValidShadowCastingLight(ref renderingData.lightData, i))
			{
				ref VisibleLight reference = ref visibleLights.UnsafeElementAt(i);
				if (reference.lightType == LightType.Point)
				{
					num++;
				}
				if (reference.light.shadows == LightShadows.Soft)
				{
					num2++;
				}
				if (renderingData.shadowData.resolution[i] == 128)
				{
					num3++;
				}
				if (renderingData.shadowData.resolution[i] == 256)
				{
					num4++;
				}
				if (renderingData.shadowData.resolution[i] == 512)
				{
					num5++;
				}
				if (renderingData.shadowData.resolution[i] == 1024)
				{
					num6++;
				}
				if (renderingData.shadowData.resolution[i] == 2048)
				{
					num7++;
				}
				if (renderingData.shadowData.resolution[i] == 4096)
				{
					num8++;
				}
			}
		}
		return (ResolutionLog2ForHash(renderingData.shadowData.additionalLightsShadowmapWidth) - 8) | (num << 3) | (num2 << 11) | (num3 << 19) | (num4 << 27) | (num5 << 35) | (num6 << 43) | (num7 << 50) | (num8 << 57);
	}

	public bool Setup(ref RenderingData renderingData)
	{
		using (new ProfilingScope(null, m_ProfilingSetupSampler))
		{
			if (!renderingData.shadowData.additionalLightShadowsEnabled)
			{
				return false;
			}
			if (!renderingData.shadowData.supportsAdditionalLightShadows)
			{
				return SetupForEmptyRendering(ref renderingData);
			}
			Clear();
			renderTargetWidth = renderingData.shadowData.additionalLightsShadowmapWidth;
			renderTargetHeight = renderingData.shadowData.additionalLightsShadowmapHeight;
			NativeArray<VisibleLight> visibleLights = renderingData.lightData.visibleLights;
			int additionalLightsShadowmapWidth = renderingData.shadowData.additionalLightsShadowmapWidth;
			int num = 0;
			m_ShadowResolutionRequests.Clear();
			if (m_VisibleLightIndexToAdditionalLightIndex.Length < visibleLights.Length)
			{
				m_VisibleLightIndexToAdditionalLightIndex = new int[visibleLights.Length];
				m_VisibleLightIndexToCameraSquareDistance = new float[visibleLights.Length];
				m_VisibleLightIndexToSortedShadowResolutionRequestsFirstSliceIndex = new int[visibleLights.Length];
			}
			int num2 = (m_UseStructuredBuffer ? visibleLights.Length : Math.Min(visibleLights.Length, UniversalRenderPipeline.maxVisibleAdditionalLights));
			if (m_AdditionalLightIndexToVisibleLightIndex.Length < num2)
			{
				m_AdditionalLightIndexToVisibleLightIndex = new int[num2];
				m_AdditionalLightIndexToShadowParams = new Vector4[num2];
			}
			for (int i = 0; i < m_VisibleLightIndexToCameraSquareDistance.Length; i++)
			{
				m_VisibleLightIndexToCameraSquareDistance[i] = float.MaxValue;
			}
			for (int j = 0; j < visibleLights.Length; j++)
			{
				if (j != renderingData.lightData.mainLightIndex && IsValidShadowCastingLight(ref renderingData.lightData, j))
				{
					ref VisibleLight reference = ref visibleLights.UnsafeElementAt(j);
					int punctualLightShadowSlicesCount = GetPunctualLightShadowSlicesCount(reference.lightType);
					num += punctualLightShadowSlicesCount;
					for (int k = 0; k < punctualLightShadowSlicesCount; k++)
					{
						m_ShadowResolutionRequests.Add(new ShadowResolutionRequest(j, k, renderingData.shadowData.resolution[j], reference.light.shadows == LightShadows.Soft, reference.lightType == LightType.Point));
					}
					m_VisibleLightIndexToCameraSquareDistance[j] = (renderingData.cameraData.camera.transform.position - reference.light.transform.position).sqrMagnitude;
				}
			}
			if (m_SortedShadowResolutionRequests == null || m_SortedShadowResolutionRequests.Length < num)
			{
				m_SortedShadowResolutionRequests = new ShadowResolutionRequest[num];
			}
			for (int l = 0; l < m_ShadowResolutionRequests.Count; l++)
			{
				m_SortedShadowResolutionRequests[l] = m_ShadowResolutionRequests[l];
			}
			for (int m = num; m < m_SortedShadowResolutionRequests.Length; m++)
			{
				m_SortedShadowResolutionRequests[m].requestedResolution = 0;
			}
			InsertionSort(m_SortedShadowResolutionRequests, 0, num);
			int num3 = (m_UseStructuredBuffer ? num : Math.Min(num, UniversalRenderPipeline.maxVisibleAdditionalLights));
			bool flag = false;
			int num4 = 1;
			while (!flag && num3 > 0)
			{
				num4 = EstimateScaleFactorNeededToFitAllShadowsInAtlas(in m_SortedShadowResolutionRequests, num3, additionalLightsShadowmapWidth);
				if (m_SortedShadowResolutionRequests[num3 - 1].requestedResolution >= num4 * MinimalPunctualLightShadowResolution(m_SortedShadowResolutionRequests[num3 - 1].softShadow))
				{
					flag = true;
				}
				else
				{
					num3 -= GetPunctualLightShadowSlicesCount(m_SortedShadowResolutionRequests[num3 - 1].pointLightShadow ? LightType.Point : LightType.Spot);
				}
			}
			for (int n = num3; n < m_SortedShadowResolutionRequests.Length; n++)
			{
				m_SortedShadowResolutionRequests[n].requestedResolution = 0;
			}
			for (int num5 = 0; num5 < m_VisibleLightIndexToSortedShadowResolutionRequestsFirstSliceIndex.Length; num5++)
			{
				m_VisibleLightIndexToSortedShadowResolutionRequestsFirstSliceIndex[num5] = -1;
			}
			for (int num6 = num3 - 1; num6 >= 0; num6--)
			{
				m_VisibleLightIndexToSortedShadowResolutionRequestsFirstSliceIndex[m_SortedShadowResolutionRequests[num6].visibleLightIndex] = num6;
			}
			AtlasLayout(additionalLightsShadowmapWidth, num3, num4);
			if (m_AdditionalLightsShadowSlices == null || m_AdditionalLightsShadowSlices.Length < num3)
			{
				m_AdditionalLightsShadowSlices = new ShadowSliceData[num3];
			}
			if (m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix == null || (m_UseStructuredBuffer && m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix.Length < num3))
			{
				m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix = new Matrix4x4[num3];
			}
			for (int num7 = 0; num7 < num2; num7++)
			{
				m_AdditionalLightIndexToShadowParams[num7] = c_DefaultShadowParams;
			}
			int num8 = 0;
			bool supportsSoftShadows = renderingData.shadowData.supportsSoftShadows;
			int num9 = 0;
			for (int num10 = 0; num10 < visibleLights.Length; num10++)
			{
				ref VisibleLight reference2 = ref visibleLights.UnsafeElementAt(num10);
				if (num10 == renderingData.lightData.mainLightIndex)
				{
					m_VisibleLightIndexToAdditionalLightIndex[num10] = -1;
					continue;
				}
				int num11 = num9++;
				if (num11 >= m_AdditionalLightIndexToVisibleLightIndex.Length)
				{
					continue;
				}
				m_AdditionalLightIndexToVisibleLightIndex[num11] = num10;
				m_VisibleLightIndexToAdditionalLightIndex[num10] = num11;
				if (m_ShadowSliceToAdditionalLightIndex.Count >= num3 || num11 >= num2)
				{
					continue;
				}
				LightType lightType = reference2.lightType;
				int punctualLightShadowSlicesCount2 = GetPunctualLightShadowSlicesCount(in lightType);
				if (m_ShadowSliceToAdditionalLightIndex.Count + punctualLightShadowSlicesCount2 > num3 && IsValidShadowCastingLight(ref renderingData.lightData, num10))
				{
					break;
				}
				int count = m_ShadowSliceToAdditionalLightIndex.Count;
				bool flag2 = false;
				for (int num12 = 0; num12 < punctualLightShadowSlicesCount2; num12++)
				{
					int count2 = m_ShadowSliceToAdditionalLightIndex.Count;
					if (!renderingData.cullResults.GetShadowCasterBounds(num10, out var _) || !renderingData.shadowData.supportsAdditionalLightShadows || !IsValidShadowCastingLight(ref renderingData.lightData, num10) || m_VisibleLightIndexToSortedShadowResolutionRequestsFirstSliceIndex[num10] == -1)
					{
						continue;
					}
					switch (lightType)
					{
					case LightType.Spot:
					{
						if (ShadowUtils.ExtractSpotLightMatrix(ref renderingData.cullResults, ref renderingData.shadowData, num10, out var shadowMatrix2, out m_AdditionalLightsShadowSlices[count2].viewMatrix, out m_AdditionalLightsShadowSlices[count2].projectionMatrix, out m_AdditionalLightsShadowSlices[count2].splitData))
						{
							m_ShadowSliceToAdditionalLightIndex.Add(num11);
							m_GlobalShadowSliceIndexToPerLightShadowSliceIndex.Add(num12);
							Light light2 = reference2.light;
							float shadowStrength2 = light2.shadowStrength;
							float y2 = ShadowUtils.SoftShadowQualityToShaderProperty(light2, supportsSoftShadows && light2.shadows == LightShadows.Soft);
							Vector4 vector2 = new Vector4(shadowStrength2, y2, 0f, count);
							m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix[count2] = shadowMatrix2;
							m_AdditionalLightIndexToShadowParams[num11] = vector2;
							flag2 = true;
						}
						break;
					}
					case LightType.Point:
					{
						float pointLightShadowFrustumFovBiasInDegrees = GetPointLightShadowFrustumFovBiasInDegrees(m_SortedShadowResolutionRequests[m_VisibleLightIndexToSortedShadowResolutionRequestsFirstSliceIndex[num10]].allocatedResolution, reference2.light.shadows == LightShadows.Soft);
						if (ShadowUtils.ExtractPointLightMatrix(ref renderingData.cullResults, ref renderingData.shadowData, num10, (CubemapFace)num12, pointLightShadowFrustumFovBiasInDegrees, out var shadowMatrix, out m_AdditionalLightsShadowSlices[count2].viewMatrix, out m_AdditionalLightsShadowSlices[count2].projectionMatrix, out m_AdditionalLightsShadowSlices[count2].splitData))
						{
							m_ShadowSliceToAdditionalLightIndex.Add(num11);
							m_GlobalShadowSliceIndexToPerLightShadowSliceIndex.Add(num12);
							Light light = reference2.light;
							float shadowStrength = light.shadowStrength;
							float y = ShadowUtils.SoftShadowQualityToShaderProperty(light, supportsSoftShadows && light.shadows == LightShadows.Soft);
							Vector4 vector = new Vector4(shadowStrength, y, 1f, count);
							m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix[count2] = shadowMatrix;
							m_AdditionalLightIndexToShadowParams[num11] = vector;
							flag2 = true;
						}
						break;
					}
					}
				}
				if (flag2)
				{
					num8++;
				}
			}
			if (num8 == 0)
			{
				return SetupForEmptyRendering(ref renderingData);
			}
			int count3 = m_ShadowSliceToAdditionalLightIndex.Count;
			int num13 = 0;
			int num14 = 0;
			for (int num15 = 0; num15 < num3; num15++)
			{
				ShadowResolutionRequest shadowResolutionRequest = m_SortedShadowResolutionRequests[num15];
				num13 = Mathf.Max(num13, shadowResolutionRequest.offsetX + shadowResolutionRequest.allocatedResolution);
				num14 = Mathf.Max(num14, shadowResolutionRequest.offsetY + shadowResolutionRequest.allocatedResolution);
			}
			renderTargetWidth = Mathf.NextPowerOfTwo(num13);
			renderTargetHeight = Mathf.NextPowerOfTwo(num14);
			float num16 = 1f / (float)renderTargetWidth;
			float num17 = 1f / (float)renderTargetHeight;
			for (int num18 = 0; num18 < count3; num18++)
			{
				int num19 = m_ShadowSliceToAdditionalLightIndex[num18];
				if (!Mathf.Approximately(m_AdditionalLightIndexToShadowParams[num19].x, 0f) && !Mathf.Approximately(m_AdditionalLightIndexToShadowParams[num19].w, -1f))
				{
					int num20 = m_AdditionalLightIndexToVisibleLightIndex[num19];
					int num21 = m_VisibleLightIndexToSortedShadowResolutionRequestsFirstSliceIndex[num20];
					int num22 = m_GlobalShadowSliceIndexToPerLightShadowSliceIndex[num18];
					ShadowResolutionRequest shadowResolutionRequest2 = m_SortedShadowResolutionRequests[num21 + num22];
					int allocatedResolution = shadowResolutionRequest2.allocatedResolution;
					Matrix4x4 identity = Matrix4x4.identity;
					identity.m00 = (float)allocatedResolution * num16;
					identity.m11 = (float)allocatedResolution * num17;
					m_AdditionalLightsShadowSlices[num18].offsetX = shadowResolutionRequest2.offsetX;
					m_AdditionalLightsShadowSlices[num18].offsetY = shadowResolutionRequest2.offsetY;
					m_AdditionalLightsShadowSlices[num18].resolution = allocatedResolution;
					identity.m03 = (float)m_AdditionalLightsShadowSlices[num18].offsetX * num16;
					identity.m13 = (float)m_AdditionalLightsShadowSlices[num18].offsetY * num17;
					m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix[num18] = identity * m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix[num18];
				}
			}
			ShadowUtils.ShadowRTReAllocateIfNeeded(ref m_AdditionalLightsShadowmapHandle, renderTargetWidth, renderTargetHeight, 16, 1, 0f, "_AdditionalLightsShadowmapTexture");
			m_MaxShadowDistanceSq = renderingData.cameraData.maxShadowDistance * renderingData.cameraData.maxShadowDistance;
			m_CascadeBorder = renderingData.shadowData.mainLightShadowCascadeBorder;
			m_CreateEmptyShadowmap = false;
			base.useNativeRenderPass = true;
			return true;
		}
	}

	private bool SetupForEmptyRendering(ref RenderingData renderingData)
	{
		if (!renderingData.cameraData.renderer.stripShadowsOffVariants)
		{
			return false;
		}
		renderingData.shadowData.isKeywordAdditionalLightShadowsEnabled = true;
		ShadowUtils.ShadowRTReAllocateIfNeeded(ref m_EmptyAdditionalLightShadowmapTexture, 1, 1, 16, 1, 0f, "_EmptyAdditionalLightShadowmapTexture");
		m_CreateEmptyShadowmap = true;
		base.useNativeRenderPass = false;
		for (int i = 0; i < m_AdditionalLightIndexToShadowParams.Length; i++)
		{
			m_AdditionalLightIndexToShadowParams[i] = c_DefaultShadowParams;
		}
		return true;
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		if (Application.platform == RuntimePlatform.Android && PlatformAutoDetect.isRunningOnPowerVRGPU)
		{
			ResetTarget();
		}
		if (m_CreateEmptyShadowmap)
		{
			ConfigureTarget(m_EmptyAdditionalLightShadowmapTexture);
		}
		else
		{
			ConfigureTarget(m_AdditionalLightsShadowmapHandle);
		}
		ConfigureClear(ClearFlag.All, Color.black);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (m_CreateEmptyShadowmap)
		{
			SetEmptyAdditionalShadowmapAtlas(ref context, ref renderingData);
			renderingData.commandBuffer.SetGlobalTexture(m_AdditionalLightsShadowmapID, m_EmptyAdditionalLightShadowmapTexture);
		}
		else if (renderingData.shadowData.supportsAdditionalLightShadows)
		{
			RenderAdditionalShadowmapAtlas(ref context, ref renderingData);
			renderingData.commandBuffer.SetGlobalTexture(m_AdditionalLightsShadowmapID, m_AdditionalLightsShadowmapHandle.nameID);
		}
	}

	public int GetShadowLightIndexFromLightIndex(int visibleLightIndex)
	{
		if (visibleLightIndex < 0 || visibleLightIndex >= m_VisibleLightIndexToAdditionalLightIndex.Length)
		{
			return -1;
		}
		return m_VisibleLightIndexToAdditionalLightIndex[visibleLightIndex];
	}

	private void Clear()
	{
		m_ShadowSliceToAdditionalLightIndex.Clear();
		m_GlobalShadowSliceIndexToPerLightShadowSliceIndex.Clear();
	}

	private void SetEmptyAdditionalShadowmapAtlas(ref ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		CoreUtils.SetKeyword(commandBuffer, "_ADDITIONAL_LIGHT_SHADOWS", state: true);
		SetEmptyAdditionalLightShadowParams(commandBuffer, m_AdditionalLightIndexToShadowParams);
		context.ExecuteCommandBuffer(commandBuffer);
		commandBuffer.Clear();
	}

	internal static void SetEmptyAdditionalLightShadowParams(CommandBuffer cmd, Vector4[] lightIndexToShadowParams)
	{
		if (RenderingUtils.useStructuredBuffer)
		{
			ComputeBuffer additionalLightShadowParamsStructuredBuffer = ShaderData.instance.GetAdditionalLightShadowParamsStructuredBuffer(lightIndexToShadowParams.Length);
			additionalLightShadowParamsStructuredBuffer.SetData(lightIndexToShadowParams);
			cmd.SetGlobalBuffer(m_AdditionalShadowParams_SSBO, additionalLightShadowParamsStructuredBuffer);
		}
		else
		{
			cmd.SetGlobalVectorArray(AdditionalShadowsConstantBuffer._AdditionalShadowParams, lightIndexToShadowParams);
		}
	}

	private void RenderAdditionalShadowmapAtlas(ref ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CullingResults cullResults = renderingData.cullResults;
		LightData lightData = renderingData.lightData;
		NativeArray<VisibleLight> visibleLights = lightData.visibleLights;
		bool flag = false;
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		using (new ProfilingScope(commandBuffer, ProfilingSampler.Get(URPProfileId.AdditionalLightsShadow)))
		{
			ShadowUtils.SetWorldToCameraMatrix(commandBuffer, renderingData.cameraData.GetViewMatrix());
			bool flag2 = false;
			int count = m_ShadowSliceToAdditionalLightIndex.Count;
			for (int i = 0; i < count; i++)
			{
				int num = m_ShadowSliceToAdditionalLightIndex[i];
				if (!Mathf.Approximately(m_AdditionalLightIndexToShadowParams[num].x, 0f) && !Mathf.Approximately(m_AdditionalLightIndexToShadowParams[num].w, -1f))
				{
					int num2 = m_AdditionalLightIndexToVisibleLightIndex[num];
					ref VisibleLight reference = ref visibleLights.UnsafeElementAt(num2);
					ShadowSliceData shadowSliceData = m_AdditionalLightsShadowSlices[i];
					ShadowDrawingSettings settings = new ShadowDrawingSettings(cullResults, num2, BatchCullingProjectionType.Perspective);
					settings.useRenderingLayerMaskTest = UniversalRenderPipeline.asset.useRenderingLayers;
					settings.splitData = shadowSliceData.splitData;
					Vector4 shadowBias = ShadowUtils.GetShadowBias(ref reference, num2, ref renderingData.shadowData, shadowSliceData.projectionMatrix, shadowSliceData.resolution);
					ShadowUtils.SetupShadowCasterConstantBuffer(commandBuffer, ref reference, shadowBias);
					CoreUtils.SetKeyword(commandBuffer, "_CASTING_PUNCTUAL_LIGHT_SHADOW", state: true);
					ShadowUtils.RenderShadowSlice(commandBuffer, ref context, ref shadowSliceData, ref settings);
					flag |= reference.light.shadows == LightShadows.Soft;
					flag2 = true;
				}
			}
			bool flag3 = renderingData.shadowData.supportsMainLightShadows && lightData.mainLightIndex != -1 && visibleLights[lightData.mainLightIndex].light.shadows == LightShadows.Soft;
			bool flag4 = !renderingData.cameraData.renderer.stripShadowsOffVariants;
			renderingData.shadowData.isKeywordAdditionalLightShadowsEnabled = !flag4 || flag2;
			CoreUtils.SetKeyword(commandBuffer, "_ADDITIONAL_LIGHT_SHADOWS", renderingData.shadowData.isKeywordAdditionalLightShadowsEnabled);
			bool flag5 = renderingData.shadowData.supportsSoftShadows && (flag3 || flag);
			renderingData.shadowData.isKeywordSoftShadowsEnabled = flag5;
			ShadowUtils.SetSoftShadowQualityShaderKeywords(commandBuffer, ref renderingData.shadowData);
			if (flag2)
			{
				SetupAdditionalLightsShadowReceiverConstants(commandBuffer, flag5);
			}
		}
	}

	private void SetupAdditionalLightsShadowReceiverConstants(CommandBuffer cmd, bool softShadows)
	{
		if (m_UseStructuredBuffer)
		{
			ComputeBuffer additionalLightShadowParamsStructuredBuffer = ShaderData.instance.GetAdditionalLightShadowParamsStructuredBuffer(m_AdditionalLightIndexToShadowParams.Length);
			additionalLightShadowParamsStructuredBuffer.SetData(m_AdditionalLightIndexToShadowParams);
			cmd.SetGlobalBuffer(m_AdditionalShadowParams_SSBO, additionalLightShadowParamsStructuredBuffer);
			ComputeBuffer additionalLightShadowSliceMatricesStructuredBuffer = ShaderData.instance.GetAdditionalLightShadowSliceMatricesStructuredBuffer(m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix.Length);
			additionalLightShadowSliceMatricesStructuredBuffer.SetData(m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix);
			cmd.SetGlobalBuffer(m_AdditionalLightsWorldToShadow_SSBO, additionalLightShadowSliceMatricesStructuredBuffer);
		}
		else
		{
			cmd.SetGlobalVectorArray(AdditionalShadowsConstantBuffer._AdditionalShadowParams, m_AdditionalLightIndexToShadowParams);
			cmd.SetGlobalMatrixArray(AdditionalShadowsConstantBuffer._AdditionalLightsWorldToShadow, m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix);
		}
		ShadowUtils.GetScaleAndBiasForLinearDistanceFade(m_MaxShadowDistanceSq, m_CascadeBorder, out var scale, out var bias);
		cmd.SetGlobalVector(AdditionalShadowsConstantBuffer._AdditionalShadowFadeParams, new Vector4(scale, bias, 0f, 0f));
		if (softShadows)
		{
			Vector2Int referenceSize = m_AdditionalLightsShadowmapHandle.referenceSize;
			Vector2 vector = Vector2.one / referenceSize;
			Vector2 vector2 = vector * 0.5f;
			cmd.SetGlobalVector(AdditionalShadowsConstantBuffer._AdditionalShadowOffset0, new Vector4(0f - vector2.x, 0f - vector2.y, vector2.x, 0f - vector2.y));
			cmd.SetGlobalVector(AdditionalShadowsConstantBuffer._AdditionalShadowOffset1, new Vector4(0f - vector2.x, vector2.y, vector2.x, vector2.y));
			cmd.SetGlobalVector(AdditionalShadowsConstantBuffer._AdditionalShadowmapSize, new Vector4(vector.x, vector.y, referenceSize.x, referenceSize.y));
		}
	}

	private bool IsValidShadowCastingLight(ref LightData lightData, int i)
	{
		if (i == lightData.mainLightIndex)
		{
			return false;
		}
		ref VisibleLight reference = ref lightData.visibleLights.UnsafeElementAt(i);
		if (reference.lightType == LightType.Directional)
		{
			return false;
		}
		Light light = reference.light;
		if (light != null && light.shadows != LightShadows.None)
		{
			return !Mathf.Approximately(light.shadowStrength, 0f);
		}
		return false;
	}

	internal TextureHandle Render(RenderGraph graph, ref RenderingData renderingData)
	{
		PassData passData;
		TextureHandle shadowmapTexture;
		using (RenderGraphBuilder renderGraphBuilder = graph.AddRenderPass<PassData>("Additional Lights Shadowmap", out passData, base.profilingSampler))
		{
			InitPassData(ref passData, ref renderingData, ref graph);
			if (!m_CreateEmptyShadowmap)
			{
				passData.shadowmapTexture = UniversalRenderer.CreateRenderGraphTexture(graph, m_AdditionalLightsShadowmapHandle.rt.descriptor, "Additional Shadowmap", clear: true, (!ShadowUtils.m_ForceShadowPointSampling) ? FilterMode.Bilinear : FilterMode.Point);
				renderGraphBuilder.UseDepthBuffer(in passData.shadowmapTexture, DepthAccess.Write);
			}
			renderGraphBuilder.AllowPassCulling(value: false);
			renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
			{
				if (!data.emptyShadowmap)
				{
					data.pass.RenderAdditionalShadowmapAtlas(ref context.renderContext, ref data.renderingData);
				}
			});
			shadowmapTexture = passData.shadowmapTexture;
		}
		PassData passData2;
		using RenderGraphBuilder renderGraphBuilder2 = graph.AddRenderPass<PassData>("Set Additional Shadow Globals", out passData2, base.profilingSampler);
		InitPassData(ref passData2, ref renderingData, ref graph);
		passData2.shadowmapTexture = shadowmapTexture;
		if (shadowmapTexture.IsValid())
		{
			renderGraphBuilder2.UseDepthBuffer(in shadowmapTexture, DepthAccess.Read);
		}
		renderGraphBuilder2.AllowPassCulling(value: false);
		renderGraphBuilder2.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			if (data.emptyShadowmap)
			{
				data.pass.SetEmptyAdditionalShadowmapAtlas(ref context.renderContext, ref data.renderingData);
				data.shadowmapTexture = data.graph.defaultResources.defaultShadowTexture;
			}
			data.renderingData.commandBuffer.SetGlobalTexture(data.shadowmapID, data.shadowmapTexture);
		});
		return passData2.shadowmapTexture;
	}

	private void InitPassData(ref PassData passData, ref RenderingData renderingData, ref RenderGraph graph)
	{
		passData.pass = this;
		passData.graph = graph;
		passData.emptyShadowmap = m_CreateEmptyShadowmap;
		passData.shadowmapID = m_AdditionalLightsShadowmapID;
		passData.renderingData = renderingData;
	}
}
