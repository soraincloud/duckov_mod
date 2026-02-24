#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine.UIElements.UIR;

namespace UnityEngine.UIElements;

internal class DynamicAtlasCore : IDisposable
{
	private int m_InitialSize;

	private UIRAtlasAllocator m_Allocator;

	private Dictionary<Texture2D, RectInt> m_UVs;

	private bool m_ForceReblitAll;

	private FilterMode m_FilterMode;

	private ColorSpace m_ColorSpace;

	private TextureBlitter m_Blitter;

	private int m_2SidePadding;

	private int m_1SidePadding;

	private int m_MaxAtlasSize;

	private static ProfilerMarker s_MarkerReset = new ProfilerMarker("UIR.AtlasManager.Reset");

	private static int s_TextureCounter;

	public int maxImageSize { get; }

	public RenderTextureFormat format { get; }

	public RenderTexture atlas { get; private set; }

	protected bool disposed { get; private set; }

	public DynamicAtlasCore(RenderTextureFormat format = RenderTextureFormat.ARGB32, FilterMode filterMode = FilterMode.Bilinear, int maxImageSize = 64, int initialSize = 64, int maxAtlasSize = 4096)
	{
		Debug.Assert(filterMode == FilterMode.Bilinear || filterMode == FilterMode.Point);
		Debug.Assert(maxAtlasSize <= SystemInfo.maxRenderTextureSize);
		Debug.Assert(initialSize <= maxAtlasSize);
		Debug.Assert(Mathf.IsPowerOfTwo(maxImageSize));
		Debug.Assert(Mathf.IsPowerOfTwo(initialSize));
		Debug.Assert(Mathf.IsPowerOfTwo(maxAtlasSize));
		m_MaxAtlasSize = maxAtlasSize;
		this.format = format;
		this.maxImageSize = maxImageSize;
		m_FilterMode = filterMode;
		m_UVs = new Dictionary<Texture2D, RectInt>(64);
		m_Blitter = new TextureBlitter(64);
		m_InitialSize = initialSize;
		m_2SidePadding = ((filterMode != FilterMode.Point) ? 2 : 0);
		m_1SidePadding = ((filterMode != FilterMode.Point) ? 1 : 0);
		m_Allocator = new UIRAtlasAllocator(m_InitialSize, m_MaxAtlasSize, m_1SidePadding);
		m_ColorSpace = QualitySettings.activeColorSpace;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposed)
		{
			return;
		}
		if (disposing)
		{
			UIRUtility.Destroy(atlas);
			atlas = null;
			if (m_Allocator != null)
			{
				m_Allocator.Dispose();
				m_Allocator = null;
			}
			if (m_Blitter != null)
			{
				m_Blitter.Dispose();
				m_Blitter = null;
			}
		}
		disposed = true;
	}

	private static void LogDisposeError()
	{
		Debug.LogError("An attempt to use a disposed atlas manager has been detected.");
	}

	public bool IsReleased()
	{
		return atlas != null && !atlas.IsCreated();
	}

	public bool TryGetRect(Texture2D image, out RectInt uvs, Func<Texture2D, bool> filter = null)
	{
		uvs = default(RectInt);
		if (disposed)
		{
			LogDisposeError();
			return false;
		}
		if (image == null)
		{
			return false;
		}
		if (m_UVs.TryGetValue(image, out uvs))
		{
			return true;
		}
		if (filter != null && !filter(image))
		{
			return false;
		}
		if (!AllocateRect(image.width, image.height, out uvs))
		{
			return false;
		}
		m_UVs[image] = uvs;
		m_Blitter.QueueBlit(image, new RectInt(0, 0, image.width, image.height), new Vector2Int(uvs.x, uvs.y), addBorder: true, Color.white);
		return true;
	}

	public void UpdateTexture(Texture2D image)
	{
		RectInt value;
		if (disposed)
		{
			LogDisposeError();
		}
		else if (m_UVs.TryGetValue(image, out value))
		{
			m_Blitter.QueueBlit(image, new RectInt(0, 0, image.width, image.height), new Vector2Int(value.x, value.y), addBorder: true, Color.white);
		}
	}

	public bool AllocateRect(int width, int height, out RectInt uvs)
	{
		if (!m_Allocator.TryAllocate(width + m_2SidePadding, height + m_2SidePadding, out uvs))
		{
			return false;
		}
		uvs = new RectInt(uvs.x + m_1SidePadding, uvs.y + m_1SidePadding, width, height);
		return true;
	}

	public void EnqueueBlit(Texture image, RectInt srcRect, int x, int y, bool addBorder, Color tint)
	{
		m_Blitter.QueueBlit(image, srcRect, new Vector2Int(x, y), addBorder, tint);
	}

	public void Commit()
	{
		if (disposed)
		{
			LogDisposeError();
			return;
		}
		UpdateAtlasTexture();
		if (m_ForceReblitAll)
		{
			m_ForceReblitAll = false;
			m_Blitter.Reset();
			foreach (KeyValuePair<Texture2D, RectInt> uV in m_UVs)
			{
				m_Blitter.QueueBlit(uV.Key, new RectInt(0, 0, uV.Key.width, uV.Key.height), new Vector2Int(uV.Value.x, uV.Value.y), addBorder: true, Color.white);
			}
		}
		m_Blitter.Commit(atlas);
	}

	private void UpdateAtlasTexture()
	{
		if (atlas == null)
		{
			if (m_UVs.Count > m_Blitter.queueLength)
			{
				m_ForceReblitAll = true;
			}
			atlas = CreateAtlasTexture();
		}
		else if (atlas.width != m_Allocator.physicalWidth || atlas.height != m_Allocator.physicalHeight)
		{
			RenderTexture renderTexture = CreateAtlasTexture();
			if (renderTexture == null)
			{
				Debug.LogErrorFormat("Failed to allocate a render texture for the dynamic atlas. Current Size = {0}x{1}. Requested Size = {2}x{3}.", atlas.width, atlas.height, m_Allocator.physicalWidth, m_Allocator.physicalHeight);
			}
			else
			{
				m_Blitter.BlitOneNow(renderTexture, atlas, new RectInt(0, 0, atlas.width, atlas.height), new Vector2Int(0, 0), addBorder: false, Color.white);
			}
			UIRUtility.Destroy(atlas);
			atlas = renderTexture;
		}
	}

	private RenderTexture CreateAtlasTexture()
	{
		if (m_Allocator.physicalWidth == 0 || m_Allocator.physicalHeight == 0)
		{
			return null;
		}
		return new RenderTexture(m_Allocator.physicalWidth, m_Allocator.physicalHeight, 0, format)
		{
			hideFlags = HideFlags.HideAndDontSave,
			name = "UIR Dynamic Atlas " + s_TextureCounter++,
			filterMode = m_FilterMode
		};
	}
}
