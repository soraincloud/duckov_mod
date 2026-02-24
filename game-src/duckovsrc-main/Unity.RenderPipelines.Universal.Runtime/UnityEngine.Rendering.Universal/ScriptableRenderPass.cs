using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

public abstract class ScriptableRenderPass
{
	public static RTHandle k_CameraTarget = RTHandles.Alloc(BuiltinRenderTextureType.CameraTarget);

	private RenderBufferStoreAction[] m_ColorStoreActions = new RenderBufferStoreAction[1];

	private RenderBufferStoreAction m_DepthStoreAction;

	private bool[] m_OverriddenColorStoreActions = new bool[1];

	private bool m_OverriddenDepthStoreAction;

	internal NativeArray<int> m_ColorAttachmentIndices;

	internal NativeArray<int> m_InputAttachmentIndices;

	internal bool m_UsesRTHandles;

	private RTHandle[] m_ColorAttachments;

	private RenderTargetIdentifier[] m_ColorAttachmentIds;

	internal RTHandle[] m_InputAttachments = new RTHandle[8];

	internal bool[] m_InputAttachmentIsTransient = new bool[8];

	private RTHandle m_DepthAttachment;

	private RenderTargetIdentifier m_DepthAttachmentId;

	private ScriptableRenderPassInput m_Input;

	private ClearFlag m_ClearFlag;

	private Color m_ClearColor = Color.black;

	public RenderPassEvent renderPassEvent { get; set; }

	[Obsolete("Use colorAttachmentHandles")]
	public RenderTargetIdentifier[] colorAttachments => m_ColorAttachmentIds;

	[Obsolete("Use colorAttachmentHandle")]
	public RenderTargetIdentifier colorAttachment => m_ColorAttachmentIds[0];

	[Obsolete("Use depthAttachmentHandle")]
	public RenderTargetIdentifier depthAttachment
	{
		get
		{
			if (!m_UsesRTHandles)
			{
				return m_DepthAttachmentId;
			}
			return new RenderTargetIdentifier(m_DepthAttachment.nameID, 0, CubemapFace.Unknown, -1);
		}
	}

	public RTHandle[] colorAttachmentHandles => m_ColorAttachments;

	public RTHandle colorAttachmentHandle => m_ColorAttachments[0];

	public RTHandle depthAttachmentHandle => m_DepthAttachment;

	public RenderBufferStoreAction[] colorStoreActions => m_ColorStoreActions;

	public RenderBufferStoreAction depthStoreAction => m_DepthStoreAction;

	internal bool[] overriddenColorStoreActions => m_OverriddenColorStoreActions;

	internal bool overriddenDepthStoreAction => m_OverriddenDepthStoreAction;

	public ScriptableRenderPassInput input => m_Input;

	public ClearFlag clearFlag => m_ClearFlag;

	public Color clearColor => m_ClearColor;

	protected internal ProfilingSampler profilingSampler { get; set; }

	internal bool overrideCameraTarget { get; set; }

	internal bool isBlitRenderPass { get; set; }

	internal bool useNativeRenderPass { get; set; }

	internal int renderPassQueueIndex { get; set; }

	internal GraphicsFormat[] renderTargetFormat { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual void FrameCleanup(CommandBuffer cmd)
	{
		OnCameraCleanup(cmd);
	}

	internal static DebugHandler GetActiveDebugHandler(ref RenderingData renderingData)
	{
		DebugHandler debugHandler = renderingData.cameraData.renderer.DebugHandler;
		if (debugHandler != null && debugHandler.IsActiveForCamera(ref renderingData.cameraData))
		{
			return debugHandler;
		}
		return null;
	}

	public ScriptableRenderPass()
	{
		m_UsesRTHandles = true;
		renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
		m_ColorAttachments = new RTHandle[8] { k_CameraTarget, null, null, null, null, null, null, null };
		m_InputAttachments = new RTHandle[8];
		m_InputAttachmentIsTransient = new bool[8];
		m_DepthAttachment = k_CameraTarget;
		m_ColorStoreActions = new RenderBufferStoreAction[8];
		m_DepthStoreAction = RenderBufferStoreAction.Store;
		m_OverriddenColorStoreActions = new bool[8];
		m_OverriddenDepthStoreAction = false;
		m_DepthAttachment = k_CameraTarget;
		m_DepthAttachmentId = m_DepthAttachment.nameID;
		m_ColorAttachmentIds = new RenderTargetIdentifier[8] { k_CameraTarget.nameID, 0, 0, 0, 0, 0, 0, 0 };
		m_ClearFlag = ClearFlag.None;
		m_ClearColor = Color.black;
		overrideCameraTarget = false;
		isBlitRenderPass = false;
		profilingSampler = new ProfilingSampler("Unnamed_ScriptableRenderPass");
		useNativeRenderPass = true;
		renderPassQueueIndex = -1;
		renderTargetFormat = new GraphicsFormat[8];
	}

	public void ConfigureInput(ScriptableRenderPassInput passInput)
	{
		m_Input = passInput;
	}

	public void ConfigureColorStoreAction(RenderBufferStoreAction storeAction, uint attachmentIndex = 0u)
	{
		m_ColorStoreActions[attachmentIndex] = storeAction;
		m_OverriddenColorStoreActions[attachmentIndex] = true;
	}

	public void ConfigureColorStoreActions(RenderBufferStoreAction[] storeActions)
	{
		int num = Math.Min(storeActions.Length, m_ColorStoreActions.Length);
		for (uint num2 = 0u; num2 < num; num2++)
		{
			m_ColorStoreActions[num2] = storeActions[num2];
			m_OverriddenColorStoreActions[num2] = true;
		}
	}

	public void ConfigureDepthStoreAction(RenderBufferStoreAction storeAction)
	{
		m_DepthStoreAction = storeAction;
		m_OverriddenDepthStoreAction = true;
	}

	internal void ConfigureInputAttachments(RTHandle input, bool isTransient = false)
	{
		m_InputAttachments[0] = input;
		m_InputAttachmentIsTransient[0] = isTransient;
	}

	internal void ConfigureInputAttachments(RTHandle[] inputs)
	{
		m_InputAttachments = inputs;
	}

	internal void ConfigureInputAttachments(RTHandle[] inputs, bool[] isTransient)
	{
		ConfigureInputAttachments(inputs);
		m_InputAttachmentIsTransient = isTransient;
	}

	internal void SetInputAttachmentTransient(int idx, bool isTransient)
	{
		m_InputAttachmentIsTransient[idx] = isTransient;
	}

	internal bool IsInputAttachmentTransient(int idx)
	{
		return m_InputAttachmentIsTransient[idx];
	}

	public void ResetTarget()
	{
		overrideCameraTarget = false;
		m_UsesRTHandles = true;
		m_DepthAttachmentId = -1;
		m_DepthAttachment = null;
		m_ColorAttachments[0] = null;
		m_ColorAttachmentIds[0] = -1;
		for (int i = 1; i < m_ColorAttachments.Length; i++)
		{
			m_ColorAttachments[i] = null;
			m_ColorAttachmentIds[i] = 0;
		}
	}

	[Obsolete("Use RTHandles for colorAttachment and depthAttachment")]
	public void ConfigureTarget(RenderTargetIdentifier colorAttachment, RenderTargetIdentifier depthAttachment)
	{
		m_DepthAttachmentId = depthAttachment;
		m_DepthAttachment = null;
		ConfigureTarget(colorAttachment);
	}

	public void ConfigureTarget(RTHandle colorAttachment, RTHandle depthAttachment)
	{
		m_DepthAttachment = depthAttachment;
		m_DepthAttachmentId = m_DepthAttachment.nameID;
		ConfigureTarget(colorAttachment);
	}

	[Obsolete("Use RTHandles for colorAttachments and depthAttachment")]
	public void ConfigureTarget(RenderTargetIdentifier[] colorAttachments, RenderTargetIdentifier depthAttachment)
	{
		m_UsesRTHandles = false;
		overrideCameraTarget = true;
		uint validColorBufferCount = RenderingUtils.GetValidColorBufferCount(colorAttachments);
		if (validColorBufferCount > SystemInfo.supportedRenderTargetCount)
		{
			Debug.LogError("Trying to set " + validColorBufferCount + " renderTargets, which is more than the maximum supported:" + SystemInfo.supportedRenderTargetCount);
		}
		m_ColorAttachmentIds = colorAttachments;
		m_DepthAttachmentId = depthAttachment;
	}

	public void ConfigureTarget(RTHandle[] colorAttachments, RTHandle depthAttachment)
	{
		m_UsesRTHandles = true;
		overrideCameraTarget = true;
		uint validColorBufferCount = RenderingUtils.GetValidColorBufferCount(colorAttachments);
		if (validColorBufferCount > SystemInfo.supportedRenderTargetCount)
		{
			Debug.LogError("Trying to set " + validColorBufferCount + " renderTargets, which is more than the maximum supported:" + SystemInfo.supportedRenderTargetCount);
		}
		m_ColorAttachments = colorAttachments;
		if (m_ColorAttachmentIds.Length != m_ColorAttachments.Length)
		{
			m_ColorAttachmentIds = new RenderTargetIdentifier[m_ColorAttachments.Length];
		}
		for (int i = 0; i < m_ColorAttachmentIds.Length; i++)
		{
			m_ColorAttachmentIds[i] = new RenderTargetIdentifier(colorAttachments[i].nameID, 0, CubemapFace.Unknown, -1);
		}
		m_DepthAttachmentId = depthAttachment.nameID;
		m_DepthAttachment = depthAttachment;
	}

	internal void ConfigureTarget(RTHandle[] colorAttachments, RTHandle depthAttachment, GraphicsFormat[] formats)
	{
		ConfigureTarget(colorAttachments, depthAttachment);
		for (int i = 0; i < formats.Length; i++)
		{
			renderTargetFormat[i] = formats[i];
		}
	}

	[Obsolete("Use RTHandle for colorAttachment")]
	public void ConfigureTarget(RenderTargetIdentifier colorAttachment)
	{
		m_UsesRTHandles = false;
		overrideCameraTarget = true;
		m_ColorAttachmentIds[0] = colorAttachment;
		for (int i = 1; i < m_ColorAttachmentIds.Length; i++)
		{
			m_ColorAttachmentIds[i] = 0;
		}
	}

	public void ConfigureTarget(RTHandle colorAttachment)
	{
		m_UsesRTHandles = true;
		overrideCameraTarget = true;
		m_ColorAttachments[0] = colorAttachment;
		m_ColorAttachmentIds[0] = new RenderTargetIdentifier(colorAttachment.nameID, 0, CubemapFace.Unknown, -1);
		for (int i = 1; i < m_ColorAttachments.Length; i++)
		{
			m_ColorAttachments[i] = null;
			m_ColorAttachmentIds[i] = 0;
		}
	}

	[Obsolete("Use RTHandles for colorAttachments")]
	public void ConfigureTarget(RenderTargetIdentifier[] colorAttachments)
	{
		ConfigureTarget(colorAttachments, k_CameraTarget.nameID);
	}

	public void ConfigureTarget(RTHandle[] colorAttachments)
	{
		ConfigureTarget(colorAttachments, k_CameraTarget);
	}

	public void ConfigureClear(ClearFlag clearFlag, Color clearColor)
	{
		m_ClearFlag = clearFlag;
		m_ClearColor = clearColor;
	}

	public virtual void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
	}

	public virtual void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
	}

	public virtual void OnCameraCleanup(CommandBuffer cmd)
	{
	}

	public virtual void OnFinishCameraStackRendering(CommandBuffer cmd)
	{
	}

	public abstract void Execute(ScriptableRenderContext context, ref RenderingData renderingData);

	internal virtual void RecordRenderGraph(RenderGraph renderGraph, ref RenderingData renderingData)
	{
		Debug.LogWarning("RecordRenderGraph is not implemented, the pass " + ToString() + " won't be recorded in the current RenderGraph.");
	}

	[Obsolete("Use RTHandles for source and destination")]
	public void Blit(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material = null, int passIndex = 0)
	{
		ScriptableRenderer.SetRenderTarget(cmd, destination, BuiltinRenderTextureType.CameraTarget, clearFlag, clearColor);
		cmd.Blit(source, destination, material, passIndex);
	}

	public void Blit(CommandBuffer cmd, RTHandle source, RTHandle destination, Material material = null, int passIndex = 0)
	{
		if (material == null)
		{
			Blitter.BlitCameraTexture(cmd, source, destination, 0f, source.rt.filterMode == FilterMode.Bilinear);
		}
		else
		{
			Blitter.BlitCameraTexture(cmd, source, destination, material, passIndex);
		}
	}

	public void Blit(CommandBuffer cmd, ref RenderingData data, Material material, int passIndex = 0)
	{
		ScriptableRenderer renderer = data.cameraData.renderer;
		Blit(cmd, renderer.cameraColorTargetHandle, renderer.GetCameraColorFrontBuffer(cmd), material, passIndex);
		renderer.SwapColorBuffer(cmd);
	}

	public void Blit(CommandBuffer cmd, ref RenderingData data, RTHandle source, Material material, int passIndex = 0)
	{
		ScriptableRenderer renderer = data.cameraData.renderer;
		Blit(cmd, source, renderer.cameraColorTargetHandle, material, passIndex);
	}

	public DrawingSettings CreateDrawingSettings(ShaderTagId shaderTagId, ref RenderingData renderingData, SortingCriteria sortingCriteria)
	{
		return RenderingUtils.CreateDrawingSettings(shaderTagId, ref renderingData, sortingCriteria);
	}

	public DrawingSettings CreateDrawingSettings(List<ShaderTagId> shaderTagIdList, ref RenderingData renderingData, SortingCriteria sortingCriteria)
	{
		return RenderingUtils.CreateDrawingSettings(shaderTagIdList, ref renderingData, sortingCriteria);
	}

	public static bool operator <(ScriptableRenderPass lhs, ScriptableRenderPass rhs)
	{
		return lhs.renderPassEvent < rhs.renderPassEvent;
	}

	public static bool operator >(ScriptableRenderPass lhs, ScriptableRenderPass rhs)
	{
		return lhs.renderPassEvent > rhs.renderPassEvent;
	}

	internal static int GetRenderPassEventRange(RenderPassEvent renderPassEvent)
	{
		int num = RenderPassEventsEnumValues.values.Length;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (RenderPassEventsEnumValues.values[num2] == (int)renderPassEvent)
			{
				break;
			}
			num2++;
		}
		if (num2 >= num)
		{
			Debug.LogError("GetRenderPassEventRange: invalid renderPassEvent value cannot be found in the RenderPassEvent enumeration");
			return 0;
		}
		if (num2 + 1 >= num)
		{
			return 50;
		}
		return (int)(RenderPassEventsEnumValues.values[num2 + 1] - renderPassEvent);
	}
}
