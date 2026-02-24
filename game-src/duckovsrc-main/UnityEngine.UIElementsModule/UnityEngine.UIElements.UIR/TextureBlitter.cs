using System;
using System.Collections.Generic;
using Unity.Profiling;

namespace UnityEngine.UIElements.UIR;

internal class TextureBlitter : IDisposable
{
	private struct BlitInfo
	{
		public Texture src;

		public RectInt srcRect;

		public Vector2Int dstPos;

		public int border;

		public Color tint;
	}

	private const int k_TextureSlotCount = 8;

	private static readonly int[] k_TextureIds;

	private static ProfilerMarker s_CommitSampler;

	private BlitInfo[] m_SingleBlit = new BlitInfo[1];

	private Material m_BlitMaterial;

	private MaterialPropertyBlock m_Properties;

	private RectInt m_Viewport;

	private RenderTexture m_PrevRT;

	private List<BlitInfo> m_PendingBlits;

	protected bool disposed { get; private set; }

	public int queueLength => m_PendingBlits.Count;

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposed)
		{
			if (disposing)
			{
				UIRUtility.Destroy(m_BlitMaterial);
				m_BlitMaterial = null;
			}
			disposed = true;
		}
	}

	static TextureBlitter()
	{
		s_CommitSampler = new ProfilerMarker("UIR.TextureBlitter.Commit");
		k_TextureIds = new int[8];
		for (int i = 0; i < 8; i++)
		{
			k_TextureIds[i] = Shader.PropertyToID("_MainTex" + i);
		}
	}

	public TextureBlitter(int capacity = 512)
	{
		m_PendingBlits = new List<BlitInfo>(capacity);
	}

	public void QueueBlit(Texture src, RectInt srcRect, Vector2Int dstPos, bool addBorder, Color tint)
	{
		if (disposed)
		{
			DisposeHelper.NotifyDisposedUsed(this);
			return;
		}
		m_PendingBlits.Add(new BlitInfo
		{
			src = src,
			srcRect = srcRect,
			dstPos = dstPos,
			border = (addBorder ? 1 : 0),
			tint = tint
		});
	}

	public void BlitOneNow(RenderTexture dst, Texture src, RectInt srcRect, Vector2Int dstPos, bool addBorder, Color tint)
	{
		if (disposed)
		{
			DisposeHelper.NotifyDisposedUsed(this);
			return;
		}
		m_SingleBlit[0] = new BlitInfo
		{
			src = src,
			srcRect = srcRect,
			dstPos = dstPos,
			border = (addBorder ? 1 : 0),
			tint = tint
		};
		BeginBlit(dst);
		DoBlit(m_SingleBlit, 0);
		EndBlit();
	}

	public void Commit(RenderTexture dst)
	{
		if (disposed)
		{
			DisposeHelper.NotifyDisposedUsed(this);
		}
		else if (m_PendingBlits.Count != 0)
		{
			BeginBlit(dst);
			for (int i = 0; i < m_PendingBlits.Count; i += 8)
			{
				DoBlit(m_PendingBlits, i);
			}
			EndBlit();
			m_PendingBlits.Clear();
		}
	}

	public void Reset()
	{
		m_PendingBlits.Clear();
	}

	private void BeginBlit(RenderTexture dst)
	{
		if (m_BlitMaterial == null)
		{
			Shader shader = Shader.Find(Shaders.k_AtlasBlit);
			m_BlitMaterial = new Material(shader);
			m_BlitMaterial.hideFlags |= HideFlags.DontSaveInEditor;
		}
		if (m_Properties == null)
		{
			m_Properties = new MaterialPropertyBlock();
		}
		m_Viewport = Utility.GetActiveViewport();
		m_PrevRT = RenderTexture.active;
		GL.LoadPixelMatrix(0f, dst.width, 0f, dst.height);
		Graphics.SetRenderTarget(dst);
		m_BlitMaterial.SetPass(0);
	}

	private void DoBlit(IList<BlitInfo> blitInfos, int startIndex)
	{
		int num = Mathf.Min(blitInfos.Count - startIndex, 8);
		int num2 = startIndex + num;
		int num3 = startIndex;
		int num4 = 0;
		while (num3 < num2)
		{
			Texture src = blitInfos[num3].src;
			if (src != null)
			{
				m_Properties.SetTexture(k_TextureIds[num4], src);
			}
			num3++;
			num4++;
		}
		Utility.SetPropertyBlock(m_Properties);
		GL.Begin(7);
		int num5 = startIndex;
		int num6 = 0;
		while (num5 < num2)
		{
			BlitInfo blitInfo = blitInfos[num5];
			float num7 = 1f / (float)blitInfo.src.width;
			float num8 = 1f / (float)blitInfo.src.height;
			float x = blitInfo.dstPos.x - blitInfo.border;
			float y = blitInfo.dstPos.y - blitInfo.border;
			float x2 = blitInfo.dstPos.x + blitInfo.srcRect.width + blitInfo.border;
			float y2 = blitInfo.dstPos.y + blitInfo.srcRect.height + blitInfo.border;
			float x3 = (float)(blitInfo.srcRect.x - blitInfo.border) * num7;
			float y3 = (float)(blitInfo.srcRect.y - blitInfo.border) * num8;
			float x4 = (float)(blitInfo.srcRect.xMax + blitInfo.border) * num7;
			float y4 = (float)(blitInfo.srcRect.yMax + blitInfo.border) * num8;
			GL.Color(blitInfo.tint);
			GL.TexCoord3(x3, y3, num6);
			GL.Vertex3(x, y, 0f);
			GL.Color(blitInfo.tint);
			GL.TexCoord3(x3, y4, num6);
			GL.Vertex3(x, y2, 0f);
			GL.Color(blitInfo.tint);
			GL.TexCoord3(x4, y4, num6);
			GL.Vertex3(x2, y2, 0f);
			GL.Color(blitInfo.tint);
			GL.TexCoord3(x4, y3, num6);
			GL.Vertex3(x2, y, 0f);
			num5++;
			num6++;
		}
		GL.End();
	}

	private void EndBlit()
	{
		Graphics.SetRenderTarget(m_PrevRT);
		GL.Viewport(new Rect(m_Viewport.x, m_Viewport.y, m_Viewport.width, m_Viewport.height));
	}
}
