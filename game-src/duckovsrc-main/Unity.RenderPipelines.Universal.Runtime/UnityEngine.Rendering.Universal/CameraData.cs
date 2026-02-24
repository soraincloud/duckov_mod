using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

public struct CameraData
{
	private Matrix4x4 m_ViewMatrix;

	private Matrix4x4 m_ProjectionMatrix;

	private Matrix4x4 m_JitterMatrix;

	public Camera camera;

	public CameraRenderType renderType;

	public RenderTexture targetTexture;

	public RenderTextureDescriptor cameraTargetDescriptor;

	internal Rect pixelRect;

	internal bool useScreenCoordOverride;

	internal Vector4 screenSizeOverride;

	internal Vector4 screenCoordScaleBias;

	internal int pixelWidth;

	internal int pixelHeight;

	internal float aspectRatio;

	public float renderScale;

	internal ImageScalingMode imageScalingMode;

	internal ImageUpscalingFilter upscalingFilter;

	internal bool fsrOverrideSharpness;

	internal float fsrSharpness;

	internal HDRColorBufferPrecision hdrColorBufferPrecision;

	public bool clearDepth;

	public CameraType cameraType;

	public bool isDefaultViewport;

	public bool isHdrEnabled;

	public bool allowHDROutput;

	public bool requiresDepthTexture;

	public bool requiresOpaqueTexture;

	public bool postProcessingRequiresDepthTexture;

	public bool xrRendering;

	internal bool stackLastCameraOutputToHDR;

	public SortingCriteria defaultOpaqueSortFlags;

	[Obsolete("Please use xr.enabled instead.", true)]
	public bool isStereoEnabled;

	public float maxShadowDistance;

	public bool postProcessEnabled;

	internal bool stackAnyPostProcessingEnabled;

	public IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>> captureActions;

	public LayerMask volumeLayerMask;

	public Transform volumeTrigger;

	public bool isStopNaNEnabled;

	public bool isDitheringEnabled;

	public AntialiasingMode antialiasing;

	public AntialiasingQuality antialiasingQuality;

	public ScriptableRenderer renderer;

	public bool resolveFinalTarget;

	public Vector3 worldSpaceCameraPos;

	public Color backgroundColor;

	internal TaaPersistentData taaPersistentData;

	internal TemporalAA.Settings taaSettings;

	public Camera baseCamera;

	public int scaledWidth => Mathf.Max(1, (int)((float)camera.pixelWidth * renderScale));

	public int scaledHeight => Mathf.Max(1, (int)((float)camera.pixelHeight * renderScale));

	internal bool requireSrgbConversion
	{
		get
		{
			if (xr.enabled)
			{
				if (!xr.renderTargetDesc.sRGB && (xr.renderTargetDesc.graphicsFormat == GraphicsFormat.R8G8B8A8_UNorm || xr.renderTargetDesc.graphicsFormat == GraphicsFormat.B8G8R8A8_UNorm))
				{
					return QualitySettings.activeColorSpace == ColorSpace.Linear;
				}
				return false;
			}
			if (targetTexture == null)
			{
				return Display.main.requiresSrgbBlitToBackbuffer;
			}
			return false;
		}
	}

	public bool isSceneViewCamera => cameraType == CameraType.SceneView;

	public bool isPreviewCamera => cameraType == CameraType.Preview;

	internal bool isRenderPassSupportedCamera
	{
		get
		{
			if (cameraType != CameraType.Game)
			{
				return cameraType == CameraType.Reflection;
			}
			return true;
		}
	}

	internal bool resolveToScreen
	{
		get
		{
			if (targetTexture == null && resolveFinalTarget)
			{
				if (cameraType != CameraType.Game)
				{
					return camera.cameraType == CameraType.VR;
				}
				return true;
			}
			return false;
		}
	}

	public bool isHDROutputActive
	{
		get
		{
			bool flag = UniversalRenderPipeline.HDROutputForMainDisplayIsActive();
			if (xr.enabled)
			{
				flag = xr.isHDRDisplayOutputActive;
			}
			if (flag && allowHDROutput)
			{
				return resolveToScreen;
			}
			return false;
		}
	}

	public HDROutputUtils.HDRDisplayInformation hdrDisplayInformation
	{
		get
		{
			if (xr.enabled)
			{
				return xr.hdrDisplayOutputInformation;
			}
			HDROutputSettings main = HDROutputSettings.main;
			return new HDROutputUtils.HDRDisplayInformation(main.maxFullFrameToneMapLuminance, main.maxToneMapLuminance, main.minToneMapLuminance, main.paperWhiteNits);
		}
	}

	public ColorGamut hdrDisplayColorGamut
	{
		get
		{
			if (xr.enabled)
			{
				return xr.hdrDisplayOutputColorGamut;
			}
			return HDROutputSettings.main.displayColorGamut;
		}
	}

	public bool rendersOverlayUI
	{
		get
		{
			if (SupportedRenderingFeatures.active.rendersUIOverlay)
			{
				return resolveToScreen;
			}
			return false;
		}
	}

	public XRPass xr { get; internal set; }

	internal XRPassUniversal xrUniversal => xr as XRPassUniversal;

	internal bool resetHistory => taaSettings.resetHistoryFrames != 0;

	internal void SetViewAndProjectionMatrix(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
	{
		m_ViewMatrix = viewMatrix;
		m_ProjectionMatrix = projectionMatrix;
		m_JitterMatrix = Matrix4x4.identity;
	}

	internal void SetViewProjectionAndJitterMatrix(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 jitterMatrix)
	{
		m_ViewMatrix = viewMatrix;
		m_ProjectionMatrix = projectionMatrix;
		m_JitterMatrix = jitterMatrix;
	}

	internal void PushBuiltinShaderConstantsXR(CommandBuffer cmd, bool renderIntoTexture)
	{
		if (!xr.enabled)
		{
			return;
		}
		cmd.SetViewProjectionMatrices(GetViewMatrix(), GetProjectionMatrix());
		if (xr.singlePassEnabled)
		{
			for (int i = 0; i < xr.viewCount; i++)
			{
				XRBuiltinShaderConstants.UpdateBuiltinShaderConstants(GetViewMatrix(i), GetProjectionMatrix(i), renderIntoTexture, i);
			}
			XRBuiltinShaderConstants.SetBuiltinShaderConstants(cmd);
		}
		else
		{
			Vector3 vector = Matrix4x4.Inverse(GetViewMatrix()).GetColumn(3);
			cmd.SetGlobalVector(ShaderPropertyId.worldSpaceCameraPos, vector);
		}
	}

	public Matrix4x4 GetViewMatrix(int viewIndex = 0)
	{
		if (xr.enabled)
		{
			return xr.GetViewMatrix(viewIndex);
		}
		return m_ViewMatrix;
	}

	public Matrix4x4 GetProjectionMatrix(int viewIndex = 0)
	{
		if (xr.enabled)
		{
			return m_JitterMatrix * xr.GetProjMatrix(viewIndex);
		}
		return m_JitterMatrix * m_ProjectionMatrix;
	}

	internal Matrix4x4 GetProjectionMatrixNoJitter(int viewIndex = 0)
	{
		if (xr.enabled)
		{
			return xr.GetProjMatrix(viewIndex);
		}
		return m_ProjectionMatrix;
	}

	public Matrix4x4 GetGPUProjectionMatrix(int viewIndex = 0)
	{
		return m_JitterMatrix * GL.GetGPUProjectionMatrix(GetProjectionMatrixNoJitter(viewIndex), IsCameraProjectionMatrixFlipped());
	}

	public Matrix4x4 GetGPUProjectionMatrixNoJitter(int viewIndex = 0)
	{
		return GL.GetGPUProjectionMatrix(GetProjectionMatrixNoJitter(viewIndex), IsCameraProjectionMatrixFlipped());
	}

	internal Matrix4x4 GetGPUProjectionMatrix(bool renderIntoTexture, int viewIndex = 0)
	{
		return m_JitterMatrix * GL.GetGPUProjectionMatrix(GetProjectionMatrix(viewIndex), renderIntoTexture);
	}

	public bool IsHandleYFlipped(RTHandle handle)
	{
		if (!SystemInfo.graphicsUVStartsAtTop)
		{
			return false;
		}
		if (cameraType == CameraType.SceneView || cameraType == CameraType.Preview)
		{
			return true;
		}
		RenderTargetIdentifier renderTargetIdentifier = new RenderTargetIdentifier(handle.nameID, 0);
		bool flag = renderTargetIdentifier == BuiltinRenderTextureType.CameraTarget;
		if (xr.enabled)
		{
			flag |= renderTargetIdentifier == new RenderTargetIdentifier(xr.renderTarget, 0);
		}
		return !flag;
	}

	public bool IsCameraProjectionMatrixFlipped()
	{
		if (!SystemInfo.graphicsUVStartsAtTop)
		{
			return false;
		}
		ScriptableRenderer current = ScriptableRenderer.current;
		if (current != null)
		{
			RTHandle cameraColorTargetHandle = current.cameraColorTargetHandle;
			bool flag;
			if (cameraColorTargetHandle == null)
			{
				if (cameraType == CameraType.SceneView)
				{
					flag = true;
				}
				else
				{
					RenderTargetIdentifier renderTargetIdentifier = new RenderTargetIdentifier(current.cameraColorTarget, 0);
					bool flag2 = renderTargetIdentifier == BuiltinRenderTextureType.CameraTarget;
					if (xr.enabled)
					{
						flag2 |= renderTargetIdentifier == new RenderTargetIdentifier(xr.renderTarget, 0);
					}
					flag = !flag2;
				}
			}
			else
			{
				flag = IsHandleYFlipped(cameraColorTargetHandle);
			}
			if (!flag)
			{
				return targetTexture != null;
			}
			return true;
		}
		return true;
	}

	public bool IsRenderTargetProjectionMatrixFlipped(RTHandle color, RTHandle depth = null)
	{
		if (!SystemInfo.graphicsUVStartsAtTop)
		{
			return false;
		}
		if (!(targetTexture != null))
		{
			return IsHandleYFlipped(color ?? depth);
		}
		return true;
	}

	internal bool IsTemporalAAEnabled()
	{
		camera.TryGetComponent<UniversalAdditionalCameraData>(out var component);
		if (antialiasing == AntialiasingMode.TemporalAntiAliasing && taaPersistentData != null && cameraTargetDescriptor.msaaSamples == 1 && ((object)component == null || component.renderType != CameraRenderType.Overlay) && ((object)component == null || component.cameraStack.Count <= 0) && !camera.allowDynamicResolution)
		{
			return postProcessEnabled;
		}
		return false;
	}
}
