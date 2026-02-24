using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

public static class RenderingUtils
{
	private static List<ShaderTagId> m_LegacyShaderPassNames = new List<ShaderTagId>
	{
		new ShaderTagId("Always"),
		new ShaderTagId("ForwardBase"),
		new ShaderTagId("PrepassBase"),
		new ShaderTagId("Vertex"),
		new ShaderTagId("VertexLMRGBM"),
		new ShaderTagId("VertexLM")
	};

	private static AttachmentDescriptor s_EmptyAttachment = new AttachmentDescriptor(GraphicsFormat.None);

	private static Mesh s_FullscreenMesh = null;

	private static Material s_ErrorMaterial;

	private static Dictionary<RenderTextureFormat, bool> m_RenderTextureFormatSupport = new Dictionary<RenderTextureFormat, bool>();

	private static Dictionary<GraphicsFormat, Dictionary<FormatUsage, bool>> m_GraphicsFormatSupport = new Dictionary<GraphicsFormat, Dictionary<FormatUsage, bool>>();

	internal static AttachmentDescriptor emptyAttachment => s_EmptyAttachment;

	[Obsolete("Use Blitter.BlitCameraTexture instead of CommandBuffer.DrawMesh(fullscreenMesh, ...)")]
	public static Mesh fullscreenMesh
	{
		get
		{
			if (s_FullscreenMesh != null)
			{
				return s_FullscreenMesh;
			}
			float y = 1f;
			float y2 = 0f;
			s_FullscreenMesh = new Mesh
			{
				name = "Fullscreen Quad"
			};
			s_FullscreenMesh.SetVertices(new List<Vector3>
			{
				new Vector3(-1f, -1f, 0f),
				new Vector3(-1f, 1f, 0f),
				new Vector3(1f, -1f, 0f),
				new Vector3(1f, 1f, 0f)
			});
			s_FullscreenMesh.SetUVs(0, new List<Vector2>
			{
				new Vector2(0f, y2),
				new Vector2(0f, y),
				new Vector2(1f, y2),
				new Vector2(1f, y)
			});
			s_FullscreenMesh.SetIndices(new int[6] { 0, 1, 2, 2, 1, 3 }, MeshTopology.Triangles, 0, calculateBounds: false);
			s_FullscreenMesh.UploadMeshData(markNoLongerReadable: true);
			return s_FullscreenMesh;
		}
	}

	internal static bool useStructuredBuffer => false;

	private static Material errorMaterial
	{
		get
		{
			if (s_ErrorMaterial == null)
			{
				try
				{
					s_ErrorMaterial = new Material(Shader.Find("Hidden/Universal Render Pipeline/FallbackError"));
				}
				catch
				{
				}
			}
			return s_ErrorMaterial;
		}
	}

	internal static bool SupportsLightLayers(GraphicsDeviceType type)
	{
		return type != GraphicsDeviceType.OpenGLES2;
	}

	public static void SetViewAndProjectionMatrices(CommandBuffer cmd, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, bool setInverseMatrices)
	{
		Matrix4x4 value = projectionMatrix * viewMatrix;
		cmd.SetGlobalMatrix(ShaderPropertyId.viewMatrix, viewMatrix);
		cmd.SetGlobalMatrix(ShaderPropertyId.projectionMatrix, projectionMatrix);
		cmd.SetGlobalMatrix(ShaderPropertyId.viewAndProjectionMatrix, value);
		if (setInverseMatrices)
		{
			Matrix4x4 matrix4x = Matrix4x4.Inverse(viewMatrix);
			Matrix4x4 matrix4x2 = Matrix4x4.Inverse(projectionMatrix);
			Matrix4x4 value2 = matrix4x * matrix4x2;
			cmd.SetGlobalMatrix(ShaderPropertyId.inverseViewMatrix, matrix4x);
			cmd.SetGlobalMatrix(ShaderPropertyId.inverseProjectionMatrix, matrix4x2);
			cmd.SetGlobalMatrix(ShaderPropertyId.inverseViewAndProjectionMatrix, value2);
		}
	}

	internal static void SetScaleBiasRt(CommandBuffer cmd, in RenderingData renderingData)
	{
		ScriptableRenderer renderer = renderingData.cameraData.renderer;
		CameraData cameraData = renderingData.cameraData;
		float num = ((cameraData.cameraType != CameraType.Game || !(renderer.cameraColorTargetHandle.nameID == BuiltinRenderTextureType.CameraTarget) || !(cameraData.camera.targetTexture == null)) ? (-1f) : 1f);
		Vector4 value = ((num < 0f) ? new Vector4(num, 1f, -1f, 1f) : new Vector4(num, 0f, 1f, 1f));
		cmd.SetGlobalVector(Shader.PropertyToID("_ScaleBiasRt"), value);
	}

	internal static void Blit(CommandBuffer cmd, RTHandle source, Rect viewport, RTHandle destination, RenderBufferLoadAction loadAction, RenderBufferStoreAction storeAction, ClearFlag clearFlag, Color clearColor, Material material, int passIndex = 0)
	{
		Vector2 vector = (source.useScaling ? new Vector2(source.rtHandleProperties.rtHandleScale.x, source.rtHandleProperties.rtHandleScale.y) : Vector2.one);
		CoreUtils.SetRenderTarget(cmd, destination, loadAction, storeAction, ClearFlag.None, Color.clear);
		cmd.SetViewport(viewport);
		Blitter.BlitTexture(cmd, source, vector, material, passIndex);
	}

	internal static void Blit(CommandBuffer cmd, RTHandle source, Rect viewport, RTHandle destinationColor, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction, RTHandle destinationDepthStencil, RenderBufferLoadAction depthStencilLoadAction, RenderBufferStoreAction depthStencilStoreAction, ClearFlag clearFlag, Color clearColor, Material material, int passIndex = 0)
	{
		Vector2 vector = (source.useScaling ? new Vector2(source.rtHandleProperties.rtHandleScale.x, source.rtHandleProperties.rtHandleScale.y) : Vector2.one);
		CoreUtils.SetRenderTarget(cmd, destinationColor, colorLoadAction, colorStoreAction, destinationDepthStencil, depthStencilLoadAction, depthStencilStoreAction, clearFlag, clearColor);
		cmd.SetViewport(viewport);
		Blitter.BlitTexture(cmd, source, vector, material, passIndex);
	}

	internal static void FinalBlit(CommandBuffer cmd, ref CameraData cameraData, RTHandle source, RTHandle destination, RenderBufferLoadAction loadAction, RenderBufferStoreAction storeAction, Material material, int passIndex)
	{
		bool flag = !cameraData.isSceneViewCamera;
		if (cameraData.xr.enabled)
		{
			flag = new RenderTargetIdentifier(destination.nameID, 0, CubemapFace.Unknown, -1) == new RenderTargetIdentifier(cameraData.xr.renderTarget, 0, CubemapFace.Unknown, -1);
		}
		Vector2 vector = (source.useScaling ? new Vector2(source.rtHandleProperties.rtHandleScale.x, source.rtHandleProperties.rtHandleScale.y) : Vector2.one);
		Vector4 scaleBias = ((flag && cameraData.targetTexture == null && SystemInfo.graphicsUVStartsAtTop) ? new Vector4(vector.x, 0f - vector.y, 0f, vector.y) : new Vector4(vector.x, vector.y, 0f, 0f));
		CoreUtils.SetRenderTarget(cmd, destination, loadAction, storeAction, ClearFlag.None, Color.clear);
		if (flag)
		{
			cmd.SetViewport(cameraData.pixelRect);
		}
		if (GL.wireframe && cameraData.isSceneViewCamera)
		{
			cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget, loadAction, storeAction, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
			if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan)
			{
				cmd.SetWireframe(enable: false);
				cmd.Blit(source, destination);
				cmd.SetWireframe(enable: true);
			}
			else
			{
				cmd.Blit(source, destination);
			}
		}
		else if (source.rt == null)
		{
			Blitter.BlitTexture(cmd, source.nameID, scaleBias, material, passIndex);
		}
		else
		{
			Blitter.BlitTexture(cmd, source, scaleBias, material, passIndex);
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	internal static void RenderObjectsWithError(ScriptableRenderContext context, ref CullingResults cullResults, Camera camera, FilteringSettings filterSettings, SortingCriteria sortFlags)
	{
		if (!(errorMaterial == null))
		{
			SortingSettings sortingSettings = new SortingSettings(camera);
			sortingSettings.criteria = sortFlags;
			SortingSettings sortingSettings2 = sortingSettings;
			DrawingSettings drawingSettings = new DrawingSettings(m_LegacyShaderPassNames[0], sortingSettings2);
			drawingSettings.perObjectData = PerObjectData.None;
			drawingSettings.overrideMaterial = errorMaterial;
			drawingSettings.overrideMaterialPassIndex = 0;
			DrawingSettings drawingSettings2 = drawingSettings;
			for (int i = 1; i < m_LegacyShaderPassNames.Count; i++)
			{
				drawingSettings2.SetShaderPassName(i, m_LegacyShaderPassNames[i]);
			}
			context.DrawRenderers(cullResults, ref drawingSettings2, ref filterSettings);
		}
	}

	internal static void ClearSystemInfoCache()
	{
		m_RenderTextureFormatSupport.Clear();
		m_GraphicsFormatSupport.Clear();
	}

	public static bool SupportsRenderTextureFormat(RenderTextureFormat format)
	{
		if (!m_RenderTextureFormatSupport.TryGetValue(format, out var value))
		{
			value = SystemInfo.SupportsRenderTextureFormat(format);
			m_RenderTextureFormatSupport.Add(format, value);
		}
		return value;
	}

	public static bool SupportsGraphicsFormat(GraphicsFormat format, FormatUsage usage)
	{
		bool value = false;
		if (!m_GraphicsFormatSupport.TryGetValue(format, out var value2))
		{
			value2 = new Dictionary<FormatUsage, bool>();
			value = SystemInfo.IsFormatSupported(format, usage);
			value2.Add(usage, value);
			m_GraphicsFormatSupport.Add(format, value2);
		}
		else if (!value2.TryGetValue(usage, out value))
		{
			value = SystemInfo.IsFormatSupported(format, usage);
			value2.Add(usage, value);
		}
		return value;
	}

	internal static int GetLastValidColorBufferIndex(RenderTargetIdentifier[] colorBuffers)
	{
		int num = colorBuffers.Length - 1;
		while (num >= 0 && !(colorBuffers[num] != 0))
		{
			num--;
		}
		return num;
	}

	[Obsolete("Use RTHandles for colorBuffers")]
	internal static uint GetValidColorBufferCount(RenderTargetIdentifier[] colorBuffers)
	{
		uint num = 0u;
		if (colorBuffers != null)
		{
			for (int i = 0; i < colorBuffers.Length; i++)
			{
				if (colorBuffers[i] != 0)
				{
					num++;
				}
			}
		}
		return num;
	}

	internal static uint GetValidColorBufferCount(RTHandle[] colorBuffers)
	{
		uint num = 0u;
		if (colorBuffers != null)
		{
			foreach (RTHandle rTHandle in colorBuffers)
			{
				if (rTHandle != null && rTHandle.nameID != 0)
				{
					num++;
				}
			}
		}
		return num;
	}

	[Obsolete("Use RTHandles for colorBuffers")]
	internal static bool IsMRT(RenderTargetIdentifier[] colorBuffers)
	{
		return GetValidColorBufferCount(colorBuffers) > 1;
	}

	internal static bool IsMRT(RTHandle[] colorBuffers)
	{
		return GetValidColorBufferCount(colorBuffers) > 1;
	}

	internal static bool Contains(RenderTargetIdentifier[] source, RenderTargetIdentifier value)
	{
		for (int i = 0; i < source.Length; i++)
		{
			if (source[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	internal static int IndexOf(RenderTargetIdentifier[] source, RenderTargetIdentifier value)
	{
		for (int i = 0; i < source.Length; i++)
		{
			if (source[i] == value)
			{
				return i;
			}
		}
		return -1;
	}

	internal static uint CountDistinct(RenderTargetIdentifier[] source, RenderTargetIdentifier value)
	{
		uint num = 0u;
		for (int i = 0; i < source.Length; i++)
		{
			if (source[i] != value && source[i] != 0)
			{
				num++;
			}
		}
		return num;
	}

	internal static int LastValid(RenderTargetIdentifier[] source)
	{
		for (int num = source.Length - 1; num >= 0; num--)
		{
			if (source[num] != 0)
			{
				return num;
			}
		}
		return -1;
	}

	internal static bool Contains(ClearFlag a, ClearFlag b)
	{
		return (a & b) == b;
	}

	internal static bool SequenceEqual(RenderTargetIdentifier[] left, RenderTargetIdentifier[] right)
	{
		if (left.Length != right.Length)
		{
			return false;
		}
		for (int i = 0; i < left.Length; i++)
		{
			if (left[i] != right[i])
			{
				return false;
			}
		}
		return true;
	}

	internal static bool MultisampleDepthResolveSupported()
	{
		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
		{
			return false;
		}
		if (SystemInfo.supportsMultisampleResolveDepth)
		{
			return SystemInfo.supportsMultisampleResolveStencil;
		}
		return false;
	}

	internal static bool RTHandleNeedsReAlloc(RTHandle handle, in TextureDesc descriptor, bool scaled)
	{
		if (handle == null || handle.rt == null)
		{
			return true;
		}
		if (handle.useScaling != scaled)
		{
			return true;
		}
		if (!scaled && (handle.rt.width != descriptor.width || handle.rt.height != descriptor.height))
		{
			return true;
		}
		if (handle.rt.descriptor.depthBufferBits == (int)descriptor.depthBufferBits && (handle.rt.descriptor.depthBufferBits != 0 || descriptor.isShadowMap || handle.rt.descriptor.graphicsFormat == descriptor.colorFormat) && handle.rt.descriptor.dimension == descriptor.dimension && handle.rt.descriptor.enableRandomWrite == descriptor.enableRandomWrite && handle.rt.descriptor.useMipMap == descriptor.useMipMap && handle.rt.descriptor.autoGenerateMips == descriptor.autoGenerateMips && handle.rt.descriptor.msaaSamples == (int)descriptor.msaaSamples && handle.rt.descriptor.bindMS == descriptor.bindTextureMS && handle.rt.descriptor.useDynamicScale == descriptor.useDynamicScale && handle.rt.descriptor.memoryless == descriptor.memoryless && handle.rt.filterMode == descriptor.filterMode && handle.rt.wrapMode == descriptor.wrapMode && handle.rt.anisoLevel == descriptor.anisoLevel && handle.rt.mipMapBias == descriptor.mipMapBias)
		{
			return handle.name != descriptor.name;
		}
		return true;
	}

	internal static RenderTargetIdentifier GetCameraTargetIdentifier(ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.cameraData;
		RenderTargetIdentifier result = ((cameraData.targetTexture != null) ? new RenderTargetIdentifier(cameraData.targetTexture) : ((RenderTargetIdentifier)BuiltinRenderTextureType.CameraTarget));
		if (cameraData.xr.enabled)
		{
			if (cameraData.xr.singlePassEnabled)
			{
				result = cameraData.xr.renderTarget;
			}
			else
			{
				int textureArraySlice = cameraData.xr.GetTextureArraySlice();
				result = new RenderTargetIdentifier(cameraData.xr.renderTarget, 0, CubemapFace.Unknown, textureArraySlice);
			}
		}
		return result;
	}

	public static bool ReAllocateIfNeeded(ref RTHandle handle, in RenderTextureDescriptor descriptor, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, string name = "")
	{
		TextureDesc descriptor2 = RTHandleResourcePool.CreateTextureDesc(descriptor, TextureSizeMode.Explicit, anisoLevel, 0f, filterMode, wrapMode, name);
		if (RTHandleNeedsReAlloc(handle, in descriptor2, scaled: false))
		{
			if (handle != null && handle.rt != null)
			{
				AddStaleResourceToPoolOrRelease(RTHandleResourcePool.CreateTextureDesc(handle.rt.descriptor, TextureSizeMode.Explicit, handle.rt.anisoLevel, handle.rt.mipMapBias, handle.rt.filterMode, handle.rt.wrapMode, handle.name), handle);
			}
			if (UniversalRenderPipeline.s_RTHandlePool.TryGetResource(in descriptor2, out handle))
			{
				return true;
			}
			handle = RTHandles.Alloc(in descriptor, filterMode, wrapMode, isShadowMap, anisoLevel, mipMapBias, name);
			return true;
		}
		return false;
	}

	public static bool ReAllocateIfNeeded(ref RTHandle handle, Vector2 scaleFactor, in RenderTextureDescriptor descriptor, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, string name = "")
	{
		bool num = handle != null && handle.useScaling && handle.scaleFactor == scaleFactor;
		TextureDesc texDesc = RTHandleResourcePool.CreateTextureDesc(descriptor, TextureSizeMode.Scale, anisoLevel, 0f, filterMode, wrapMode);
		if (!num || RTHandleNeedsReAlloc(handle, in texDesc, scaled: true))
		{
			if (handle != null && handle.rt != null)
			{
				AddStaleResourceToPoolOrRelease(RTHandleResourcePool.CreateTextureDesc(handle.rt.descriptor, TextureSizeMode.Scale, handle.rt.anisoLevel, handle.rt.mipMapBias, handle.rt.filterMode, handle.rt.wrapMode), handle);
			}
			if (UniversalRenderPipeline.s_RTHandlePool.TryGetResource(in texDesc, out handle))
			{
				return true;
			}
			handle = RTHandles.Alloc(scaleFactor, in descriptor, filterMode, wrapMode, isShadowMap, anisoLevel, mipMapBias, name);
			return true;
		}
		return false;
	}

	public static bool ReAllocateIfNeeded(ref RTHandle handle, ScaleFunc scaleFunc, in RenderTextureDescriptor descriptor, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, string name = "")
	{
		bool num = handle != null && handle.useScaling && handle.scaleFactor == Vector2.zero;
		TextureDesc texDesc = RTHandleResourcePool.CreateTextureDesc(descriptor, TextureSizeMode.Functor, anisoLevel, 0f, filterMode, wrapMode);
		if (!num || RTHandleNeedsReAlloc(handle, in texDesc, scaled: true))
		{
			if (handle != null && handle.rt != null)
			{
				AddStaleResourceToPoolOrRelease(RTHandleResourcePool.CreateTextureDesc(handle.rt.descriptor, TextureSizeMode.Functor, handle.rt.anisoLevel, handle.rt.mipMapBias, handle.rt.filterMode, handle.rt.wrapMode), handle);
			}
			if (UniversalRenderPipeline.s_RTHandlePool.TryGetResource(in texDesc, out handle))
			{
				return true;
			}
			handle = RTHandles.Alloc(scaleFunc, in descriptor, filterMode, wrapMode, isShadowMap, anisoLevel, mipMapBias, name);
			return true;
		}
		return false;
	}

	public static bool SetMaxRTHandlePoolCapacity(int capacity)
	{
		if (UniversalRenderPipeline.s_RTHandlePool == null)
		{
			return false;
		}
		UniversalRenderPipeline.s_RTHandlePool.staleResourceCapacity = capacity;
		return true;
	}

	internal static void AddStaleResourceToPoolOrRelease(TextureDesc desc, RTHandle handle)
	{
		if (!UniversalRenderPipeline.s_RTHandlePool.AddResourceToPool(in desc, handle, Time.frameCount))
		{
			RTHandles.Release(handle);
		}
	}

	public static DrawingSettings CreateDrawingSettings(ShaderTagId shaderTagId, ref RenderingData renderingData, SortingCriteria sortingCriteria)
	{
		Camera camera = renderingData.cameraData.camera;
		SortingSettings sortingSettings = new SortingSettings(camera);
		sortingSettings.criteria = sortingCriteria;
		SortingSettings sortingSettings2 = sortingSettings;
		DrawingSettings result = new DrawingSettings(shaderTagId, sortingSettings2);
		result.perObjectData = renderingData.perObjectData;
		result.mainLightIndex = renderingData.lightData.mainLightIndex;
		result.enableDynamicBatching = renderingData.supportsDynamicBatching;
		result.enableInstancing = camera.cameraType != CameraType.Preview;
		return result;
	}

	public static DrawingSettings CreateDrawingSettings(List<ShaderTagId> shaderTagIdList, ref RenderingData renderingData, SortingCriteria sortingCriteria)
	{
		if (shaderTagIdList == null || shaderTagIdList.Count == 0)
		{
			Debug.LogWarning("ShaderTagId list is invalid. DrawingSettings is created with default pipeline ShaderTagId");
			return CreateDrawingSettings(new ShaderTagId("UniversalPipeline"), ref renderingData, sortingCriteria);
		}
		DrawingSettings result = CreateDrawingSettings(shaderTagIdList[0], ref renderingData, sortingCriteria);
		for (int i = 1; i < shaderTagIdList.Count; i++)
		{
			result.SetShaderPassName(i, shaderTagIdList[i]);
		}
		return result;
	}
}
