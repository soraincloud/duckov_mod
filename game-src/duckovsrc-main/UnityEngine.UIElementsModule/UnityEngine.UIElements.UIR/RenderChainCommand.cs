#define UNITY_ASSERTIONS
using System;
using Unity.Profiling;

namespace UnityEngine.UIElements.UIR;

internal class RenderChainCommand : LinkedPoolItem<RenderChainCommand>
{
	internal VisualElement owner;

	internal RenderChainCommand prev;

	internal RenderChainCommand next;

	internal bool closing;

	internal CommandType type;

	internal State state;

	internal MeshHandle mesh;

	internal int indexOffset;

	internal int indexCount;

	internal Action callback;

	private static readonly int k_ID_MainTex = Shader.PropertyToID("_MainTex");

	private static ProfilerMarker s_ImmediateOverheadMarker = new ProfilerMarker("UIR.ImmediateOverhead");

	internal void Reset()
	{
		owner = null;
		prev = (next = null);
		closing = false;
		type = CommandType.Draw;
		state = default(State);
		mesh = null;
		indexOffset = (indexCount = 0);
		callback = null;
	}

	internal void ExecuteNonDrawMesh(DrawParams drawParams, float pixelsPerPoint, ref Exception immediateException)
	{
		switch (type)
		{
		case CommandType.ImmediateCull:
			if (!RectPointsToPixelsAndFlipYAxis(owner.worldBound, pixelsPerPoint).Overlaps(Utility.GetActiveViewport()))
			{
				break;
			}
			goto case CommandType.Immediate;
		case CommandType.Immediate:
		{
			if (immediateException != null)
			{
				break;
			}
			Matrix4x4 unityProjectionMatrix = Utility.GetUnityProjectionMatrix();
			Camera current = Camera.current;
			RenderTexture active = RenderTexture.active;
			bool flag = drawParams.scissor.Count > 1;
			if (flag)
			{
				Utility.DisableScissor();
			}
			using (new GUIClip.ParentClipScope(owner.worldTransform, owner.worldClip))
			{
				try
				{
					callback();
				}
				catch (Exception ex)
				{
					immediateException = ex;
				}
			}
			Camera.SetupCurrent(current);
			RenderTexture.active = active;
			GL.modelview = drawParams.view.Peek();
			GL.LoadProjectionMatrix(unityProjectionMatrix);
			if (flag)
			{
				Utility.SetScissorRect(RectPointsToPixelsAndFlipYAxis(drawParams.scissor.Peek(), pixelsPerPoint));
			}
			break;
		}
		case CommandType.PushView:
		{
			drawParams.view.Push(owner.worldTransform);
			GL.modelview = owner.worldTransform;
			Rect rect3 = owner.hierarchy.parent?.worldClip ?? DrawParams.k_FullNormalizedRect;
			drawParams.scissor.Push(rect3);
			Utility.SetScissorRect(RectPointsToPixelsAndFlipYAxis(rect3, pixelsPerPoint));
			break;
		}
		case CommandType.PopView:
		{
			drawParams.view.Pop();
			GL.modelview = drawParams.view.Peek();
			drawParams.scissor.Pop();
			Rect rect = drawParams.scissor.Peek();
			if (rect.x == DrawParams.k_UnlimitedRect.x)
			{
				Utility.DisableScissor();
			}
			else
			{
				Utility.SetScissorRect(RectPointsToPixelsAndFlipYAxis(rect, pixelsPerPoint));
			}
			break;
		}
		case CommandType.PushScissor:
		{
			Rect rect4 = CombineScissorRects(owner.worldClip, drawParams.scissor.Peek());
			drawParams.scissor.Push(rect4);
			Utility.SetScissorRect(RectPointsToPixelsAndFlipYAxis(rect4, pixelsPerPoint));
			break;
		}
		case CommandType.PopScissor:
		{
			drawParams.scissor.Pop();
			Rect rect2 = drawParams.scissor.Peek();
			if (rect2.x == DrawParams.k_UnlimitedRect.x)
			{
				Utility.DisableScissor();
			}
			else
			{
				Utility.SetScissorRect(RectPointsToPixelsAndFlipYAxis(rect2, pixelsPerPoint));
			}
			break;
		}
		case CommandType.PushRenderTexture:
		{
			RectInt activeViewport = Utility.GetActiveViewport();
			RenderTexture temporary = RenderTexture.GetTemporary(activeViewport.width, activeViewport.height, 24, RenderTextureFormat.ARGBHalf);
			RenderTexture.active = temporary;
			GL.Clear(clearDepth: true, clearColor: true, new Color(0f, 0f, 0f, 0f), 0.99f);
			drawParams.renderTexture.Add(RenderTexture.active);
			break;
		}
		case CommandType.PopRenderTexture:
		{
			int num = drawParams.renderTexture.Count - 1;
			Debug.Assert(num > 0);
			Debug.Assert(drawParams.renderTexture[num - 1] == RenderTexture.active, "Content of previous render texture was probably not blitted");
			RenderTexture renderTexture2 = drawParams.renderTexture[num];
			if (renderTexture2 != null)
			{
				RenderTexture.ReleaseTemporary(renderTexture2);
			}
			drawParams.renderTexture.RemoveAt(num);
			break;
		}
		case CommandType.BlitToPreviousRT:
		{
			RenderTexture renderTexture = drawParams.renderTexture[drawParams.renderTexture.Count - 1];
			RenderTexture destination = drawParams.renderTexture[drawParams.renderTexture.Count - 2];
			Debug.Assert(renderTexture == RenderTexture.active, "Unexpected render target change: Current renderTarget is not the one on the top of the stack");
			Blit(renderTexture, destination, 0f);
			break;
		}
		case CommandType.PushDefaultMaterial:
			break;
		case CommandType.PopDefaultMaterial:
			break;
		}
	}

	private void Blit(Texture source, RenderTexture destination, float depth)
	{
		GL.PushMatrix();
		GL.LoadOrtho();
		RenderTexture.active = destination;
		state.material.SetTexture(k_ID_MainTex, source);
		state.material.SetPass(0);
		GL.Begin(7);
		GL.TexCoord2(0f, 0f);
		GL.Vertex3(0f, 0f, depth);
		GL.TexCoord2(0f, 1f);
		GL.Vertex3(0f, 1f, depth);
		GL.TexCoord2(1f, 1f);
		GL.Vertex3(1f, 1f, depth);
		GL.TexCoord2(1f, 0f);
		GL.Vertex3(1f, 0f, depth);
		GL.End();
		GL.PopMatrix();
	}

	private static Vector4 RectToClipSpace(Rect rc)
	{
		Matrix4x4 deviceProjectionMatrix = Utility.GetDeviceProjectionMatrix();
		Vector3 vector = deviceProjectionMatrix.MultiplyPoint(new Vector3(rc.xMin, rc.yMin, 0f));
		Vector3 vector2 = deviceProjectionMatrix.MultiplyPoint(new Vector3(rc.xMax, rc.yMax, 0f));
		return new Vector4(Mathf.Min(vector.x, vector2.x), Mathf.Min(vector.y, vector2.y), Mathf.Max(vector.x, vector2.x), Mathf.Max(vector.y, vector2.y));
	}

	private static Rect CombineScissorRects(Rect r0, Rect r1)
	{
		Rect result = new Rect(0f, 0f, 0f, 0f);
		result.x = Math.Max(r0.x, r1.x);
		result.y = Math.Max(r0.y, r1.y);
		result.xMax = Math.Max(result.x, Math.Min(r0.xMax, r1.xMax));
		result.yMax = Math.Max(result.y, Math.Min(r0.yMax, r1.yMax));
		return result;
	}

	private static RectInt RectPointsToPixelsAndFlipYAxis(Rect rect, float pixelsPerPoint)
	{
		float num = Utility.GetActiveViewport().height;
		RectInt result = new RectInt(0, 0, 0, 0);
		result.x = Mathf.RoundToInt(rect.x * pixelsPerPoint);
		result.y = Mathf.RoundToInt(num - rect.yMax * pixelsPerPoint);
		result.width = Mathf.RoundToInt(rect.width * pixelsPerPoint);
		result.height = Mathf.RoundToInt(rect.height * pixelsPerPoint);
		return result;
	}
}
