using System;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace UnityEngine.Experimental.Rendering;

public struct XRPassCreateInfo
{
	internal RenderTargetIdentifier renderTarget;

	internal RenderTextureDescriptor renderTargetDesc;

	internal ScriptableCullingParameters cullingParameters;

	internal Material occlusionMeshMaterial;

	internal float occlusionMeshScale;

	internal IntPtr foveatedRenderingInfo;

	internal int multipassId;

	internal int cullingPassId;

	internal bool copyDepth;

	internal XRDisplaySubsystem.XRRenderPass xrSdkRenderPass;
}
