using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace UnityEngine.Experimental.Rendering;

public static class XRSystem
{
	private static XRLayout s_Layout = new XRLayout();

	private static Func<XRPassCreateInfo, XRPass> s_PassAllocator = null;

	private static List<XRDisplaySubsystem> s_DisplayList = new List<XRDisplaySubsystem>();

	private static XRDisplaySubsystem s_Display;

	private static MSAASamples s_MSAASamples = MSAASamples.None;

	private static Material s_OcclusionMeshMaterial;

	private static Material s_MirrorViewMaterial;

	private static Action<XRLayout, Camera> s_LayoutOverride = null;

	public static readonly XRPass emptyPass = new XRPass();

	public static bool displayActive
	{
		get
		{
			if (s_Display == null)
			{
				return false;
			}
			return s_Display.running;
		}
	}

	public static bool isHDRDisplayOutputActive => s_Display?.hdrOutputSettings?.active == true;

	public static bool singlePassAllowed { get; set; } = true;

	public static FoveatedRenderingCaps foveatedRenderingCaps { get; set; }

	public static bool dumpDebugInfo { get; set; } = false;

	public static XRDisplaySubsystem GetActiveDisplay()
	{
		return s_Display;
	}

	public static void Initialize(Func<XRPassCreateInfo, XRPass> passAllocator, Shader occlusionMeshPS, Shader mirrorViewPS)
	{
		if (passAllocator == null)
		{
			throw new ArgumentNullException("passCreator");
		}
		s_PassAllocator = passAllocator;
		RefreshDeviceInfo();
		foveatedRenderingCaps = SystemInfo.foveatedRenderingCaps;
		if (occlusionMeshPS != null && s_OcclusionMeshMaterial == null)
		{
			s_OcclusionMeshMaterial = CoreUtils.CreateEngineMaterial(occlusionMeshPS);
		}
		if (mirrorViewPS != null && s_MirrorViewMaterial == null)
		{
			s_MirrorViewMaterial = CoreUtils.CreateEngineMaterial(mirrorViewPS);
		}
		if (XRGraphicsAutomatedTests.enabled)
		{
			SetLayoutOverride(XRGraphicsAutomatedTests.OverrideLayout);
		}
	}

	public static void SetDisplayMSAASamples(MSAASamples msaaSamples)
	{
		if (s_MSAASamples == msaaSamples)
		{
			return;
		}
		s_MSAASamples = msaaSamples;
		SubsystemManager.GetInstances(s_DisplayList);
		foreach (XRDisplaySubsystem s_Display in s_DisplayList)
		{
			s_Display.SetMSAALevel((int)s_MSAASamples);
		}
	}

	public static MSAASamples GetDisplayMSAASamples()
	{
		return s_MSAASamples;
	}

	public static void SetRenderScale(float renderScale)
	{
		SubsystemManager.GetInstances(s_DisplayList);
		foreach (XRDisplaySubsystem s_Display in s_DisplayList)
		{
			s_Display.scaleOfAllRenderTargets = renderScale;
		}
	}

	public static XRLayout NewLayout()
	{
		RefreshDeviceInfo();
		if (s_Layout.GetActivePasses().Count > 0)
		{
			Debug.LogWarning("Render Pipeline error : the XR layout still contains active passes. Executing XRSystem.EndLayout() right now.");
			EndLayout();
		}
		return s_Layout;
	}

	public static void EndLayout()
	{
		if (dumpDebugInfo)
		{
			s_Layout.LogDebugInfo();
		}
		s_Layout.Clear();
	}

	public static void RenderMirrorView(CommandBuffer cmd, Camera camera)
	{
		XRMirrorView.RenderMirrorView(cmd, camera, s_MirrorViewMaterial, s_Display);
	}

	public static void Dispose()
	{
		if (s_OcclusionMeshMaterial != null)
		{
			CoreUtils.Destroy(s_OcclusionMeshMaterial);
			s_OcclusionMeshMaterial = null;
		}
		if (s_MirrorViewMaterial != null)
		{
			CoreUtils.Destroy(s_MirrorViewMaterial);
			s_MirrorViewMaterial = null;
		}
	}

	internal static void SetDisplayZRange(float zNear, float zFar)
	{
		if (s_Display != null)
		{
			s_Display.zNear = zNear;
			s_Display.zFar = zFar;
		}
	}

	private static void SetLayoutOverride(Action<XRLayout, Camera> action)
	{
		s_LayoutOverride = action;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void XRSystemInit()
	{
		if (GraphicsSettings.currentRenderPipeline != null)
		{
			RefreshDeviceInfo();
		}
	}

	private static void RefreshDeviceInfo()
	{
		SubsystemManager.GetInstances(s_DisplayList);
		if (s_DisplayList.Count > 0)
		{
			if (s_DisplayList.Count > 1)
			{
				throw new NotImplementedException("Only one XR display is supported!");
			}
			s_Display = s_DisplayList[0];
			s_Display.disableLegacyRenderer = true;
			s_Display.sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear;
			s_Display.textureLayout = XRDisplaySubsystem.TextureLayout.Texture2DArray;
			TextureXR.maxViews = Math.Max(TextureXR.slices, 2);
		}
		else
		{
			s_Display = null;
		}
	}

	internal static void CreateDefaultLayout(Camera camera)
	{
		if (s_Display == null)
		{
			throw new NullReferenceException("s_Display");
		}
		for (int i = 0; i < s_Display.GetRenderPassCount(); i++)
		{
			s_Display.GetRenderPass(i, out var renderPass);
			s_Display.GetCullingParameters(camera, renderPass.cullingPassIndex, out var scriptableCullingParameters);
			if (CanUseSinglePass(camera, renderPass))
			{
				XRPass xRPass = s_PassAllocator(BuildPass(renderPass, scriptableCullingParameters));
				for (int j = 0; j < renderPass.GetRenderParameterCount(); j++)
				{
					renderPass.GetRenderParameter(camera, j, out var renderParameter);
					xRPass.AddView(BuildView(renderPass, renderParameter));
				}
				s_Layout.AddPass(camera, xRPass);
			}
			else
			{
				for (int k = 0; k < renderPass.GetRenderParameterCount(); k++)
				{
					renderPass.GetRenderParameter(camera, k, out var renderParameter2);
					XRPass xRPass2 = s_PassAllocator(BuildPass(renderPass, scriptableCullingParameters));
					xRPass2.AddView(BuildView(renderPass, renderParameter2));
					s_Layout.AddPass(camera, xRPass2);
				}
			}
		}
		if (s_LayoutOverride != null)
		{
			s_LayoutOverride(s_Layout, camera);
		}
	}

	internal static void ReconfigurePass(XRPass xrPass, Camera camera)
	{
		if (xrPass.enabled && s_Display != null)
		{
			s_Display.GetRenderPass(xrPass.multipassId, out var renderPass);
			s_Display.GetCullingParameters(camera, renderPass.cullingPassIndex, out var scriptableCullingParameters);
			xrPass.AssignCullingParams(renderPass.cullingPassIndex, scriptableCullingParameters);
			for (int i = 0; i < renderPass.GetRenderParameterCount(); i++)
			{
				renderPass.GetRenderParameter(camera, i, out var renderParameter);
				xrPass.AssignView(i, BuildView(renderPass, renderParameter));
			}
			if (s_LayoutOverride != null)
			{
				s_LayoutOverride(s_Layout, camera);
			}
		}
	}

	private static bool CanUseSinglePass(Camera camera, XRDisplaySubsystem.XRRenderPass renderPass)
	{
		if (!singlePassAllowed)
		{
			return false;
		}
		if (renderPass.renderTargetDesc.dimension != TextureDimension.Tex2DArray)
		{
			return false;
		}
		if (renderPass.GetRenderParameterCount() != 2 || renderPass.renderTargetDesc.volumeDepth != 2)
		{
			return false;
		}
		renderPass.GetRenderParameter(camera, 0, out var renderParameter);
		renderPass.GetRenderParameter(camera, 1, out var renderParameter2);
		if (renderParameter.textureArraySlice != 0 || renderParameter2.textureArraySlice != 1)
		{
			return false;
		}
		if (renderParameter.viewport != renderParameter2.viewport)
		{
			return false;
		}
		return true;
	}

	private static XRView BuildView(XRDisplaySubsystem.XRRenderPass renderPass, XRDisplaySubsystem.XRRenderParameter renderParameter)
	{
		Rect viewport = renderParameter.viewport;
		viewport.x *= renderPass.renderTargetDesc.width;
		viewport.width *= renderPass.renderTargetDesc.width;
		viewport.y *= renderPass.renderTargetDesc.height;
		viewport.height *= renderPass.renderTargetDesc.height;
		Mesh occlusionMesh = (XRGraphicsAutomatedTests.running ? null : renderParameter.occlusionMesh);
		return new XRView(renderParameter.projection, renderParameter.view, viewport, occlusionMesh, renderParameter.textureArraySlice);
	}

	private static XRPassCreateInfo BuildPass(XRDisplaySubsystem.XRRenderPass xrRenderPass, ScriptableCullingParameters cullingParameters)
	{
		RenderTextureDescriptor renderTargetDesc = xrRenderPass.renderTargetDesc;
		RenderTextureDescriptor renderTargetDesc2 = new RenderTextureDescriptor(renderTargetDesc.width, renderTargetDesc.height, renderTargetDesc.colorFormat, renderTargetDesc.depthBufferBits, renderTargetDesc.mipCount);
		renderTargetDesc2.dimension = xrRenderPass.renderTargetDesc.dimension;
		renderTargetDesc2.volumeDepth = xrRenderPass.renderTargetDesc.volumeDepth;
		renderTargetDesc2.vrUsage = xrRenderPass.renderTargetDesc.vrUsage;
		renderTargetDesc2.sRGB = xrRenderPass.renderTargetDesc.sRGB;
		return new XRPassCreateInfo
		{
			renderTarget = xrRenderPass.renderTarget,
			renderTargetDesc = renderTargetDesc2,
			cullingParameters = cullingParameters,
			occlusionMeshMaterial = s_OcclusionMeshMaterial,
			foveatedRenderingInfo = xrRenderPass.foveatedRenderingInfo,
			multipassId = s_Layout.GetActivePasses().Count,
			cullingPassId = xrRenderPass.cullingPassIndex,
			copyDepth = xrRenderPass.shouldFillOutDepth,
			xrSdkRenderPass = xrRenderPass
		};
	}
}
