using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering;

public abstract class RenderPipeline
{
	public class StandardRequest
	{
		public RenderTexture destination = null;

		public int mipLevel = 0;

		public CubemapFace face = CubemapFace.Unknown;

		public int slice = 0;
	}

	public bool disposed { get; private set; }

	public virtual RenderPipelineGlobalSettings defaultSettings => null;

	protected abstract void Render(ScriptableRenderContext context, Camera[] cameras);

	protected virtual void ProcessRenderRequests<RequestData>(ScriptableRenderContext context, Camera camera, RequestData renderRequest)
	{
	}

	protected internal virtual bool IsRenderRequestSupported<RequestData>(Camera camera, RequestData data)
	{
		return false;
	}

	protected static void BeginFrameRendering(ScriptableRenderContext context, Camera[] cameras)
	{
		RenderPipelineManager.BeginContextRendering(context, new List<Camera>(cameras));
	}

	protected static void BeginContextRendering(ScriptableRenderContext context, List<Camera> cameras)
	{
		RenderPipelineManager.BeginContextRendering(context, cameras);
	}

	protected static void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		RenderPipelineManager.BeginCameraRendering(context, camera);
	}

	protected static void EndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
	{
		RenderPipelineManager.EndContextRendering(context, cameras);
	}

	protected static void EndFrameRendering(ScriptableRenderContext context, Camera[] cameras)
	{
		RenderPipelineManager.EndContextRendering(context, new List<Camera>(cameras));
	}

	protected static void EndCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		RenderPipelineManager.EndCameraRendering(context, camera);
	}

	protected virtual void Render(ScriptableRenderContext context, List<Camera> cameras)
	{
		Render(context, cameras.ToArray());
	}

	internal void InternalRender(ScriptableRenderContext context, List<Camera> cameras)
	{
		if (disposed)
		{
			throw new ObjectDisposedException($"{this} has been disposed. Do not call Render on disposed a RenderPipeline.");
		}
		Render(context, cameras);
	}

	internal void InternalProcessRenderRequests<RequestData>(ScriptableRenderContext context, Camera camera, RequestData renderRequest)
	{
		if (disposed)
		{
			throw new ObjectDisposedException($"{this} has been disposed. Do not call Render on disposed a RenderPipeline.");
		}
		ProcessRenderRequests(context, camera, renderRequest);
	}

	public static bool SupportsRenderRequest<RequestData>(Camera camera, RequestData data)
	{
		bool result = false;
		if (GraphicsSettings.currentRenderPipeline != null)
		{
			if (RenderPipelineManager.currentPipeline == null)
			{
				RenderPipelineManager.PrepareRenderPipeline(GraphicsSettings.currentRenderPipeline);
			}
			result = RenderPipelineManager.currentPipeline.IsRenderRequestSupported(camera, data);
		}
		return result;
	}

	public static void SubmitRenderRequest<RequestData>(Camera camera, RequestData data)
	{
		camera.SubmitRenderRequest(data);
	}

	internal void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
		disposed = true;
	}

	protected virtual void Dispose(bool disposing)
	{
	}
}
