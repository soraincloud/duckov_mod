#define UNITY_ASSERTIONS
using System;
using Unity.Collections;

namespace UnityEngine.UIElements.UIR;

internal struct UIRVEShaderInfoAllocator
{
	private BaseShaderInfoStorage m_Storage;

	private BitmapAllocator32 m_TransformAllocator;

	private BitmapAllocator32 m_ClipRectAllocator;

	private BitmapAllocator32 m_OpacityAllocator;

	private BitmapAllocator32 m_ColorAllocator;

	private BitmapAllocator32 m_TextSettingsAllocator;

	private bool m_StorageReallyCreated;

	private bool m_VertexTexturingEnabled;

	private NativeArray<Transform3x4> m_Transforms;

	private NativeArray<Vector4> m_ClipRects;

	internal static readonly Vector2Int identityTransformTexel = new Vector2Int(0, 0);

	internal static readonly Vector2Int infiniteClipRectTexel = new Vector2Int(0, 32);

	internal static readonly Vector2Int fullOpacityTexel = new Vector2Int(32, 32);

	internal static readonly Vector2Int clearColorTexel = new Vector2Int(0, 40);

	internal static readonly Vector2Int defaultTextCoreSettingsTexel = new Vector2Int(32, 0);

	internal static readonly Matrix4x4 identityTransformValue = Matrix4x4.identity;

	internal static readonly Vector4 identityTransformRow0Value = identityTransformValue.GetRow(0);

	internal static readonly Vector4 identityTransformRow1Value = identityTransformValue.GetRow(1);

	internal static readonly Vector4 identityTransformRow2Value = identityTransformValue.GetRow(2);

	internal static readonly Vector4 infiniteClipRectValue = new Vector4(0f, 0f, 0f, 0f);

	internal static readonly Vector4 fullOpacityValue = new Vector4(1f, 1f, 1f, 1f);

	internal static readonly Vector4 clearColorValue = new Vector4(0f, 0f, 0f, 0f);

	internal static readonly TextCoreSettings defaultTextCoreSettingsValue = new TextCoreSettings
	{
		faceColor = Color.white,
		outlineColor = Color.clear,
		outlineWidth = 0f,
		underlayColor = Color.clear,
		underlayOffset = Vector2.zero,
		underlaySoftness = 0f
	};

	public static readonly BMPAlloc identityTransform;

	public static readonly BMPAlloc infiniteClipRect;

	public static readonly BMPAlloc fullOpacity;

	public static readonly BMPAlloc clearColor;

	public static readonly BMPAlloc defaultTextCoreSettings;

	private static int pageWidth => 32;

	private static int pageHeight => 8;

	public NativeSlice<Transform3x4> transformConstants => m_Transforms;

	public NativeSlice<Vector4> clipRectConstants => m_ClipRects;

	public Texture atlas
	{
		get
		{
			if (m_StorageReallyCreated)
			{
				return m_Storage.texture;
			}
			return m_VertexTexturingEnabled ? UIRenderDevice.defaultShaderInfoTexFloat : UIRenderDevice.defaultShaderInfoTexARGB8;
		}
	}

	public bool internalAtlasCreated => m_StorageReallyCreated;

	private static Vector2Int AllocToTexelCoord(ref BitmapAllocator32 allocator, BMPAlloc alloc)
	{
		allocator.GetAllocPageAtlasLocation(alloc.page, out var x, out var y);
		return new Vector2Int(alloc.bitIndex * allocator.entryWidth + x, alloc.pageLine * allocator.entryHeight + y);
	}

	private static int AllocToConstantBufferIndex(BMPAlloc alloc)
	{
		return alloc.pageLine * pageWidth + alloc.bitIndex;
	}

	private static bool AtlasRectMatchesPage(ref BitmapAllocator32 allocator, BMPAlloc defAlloc, RectInt atlasRect)
	{
		allocator.GetAllocPageAtlasLocation(defAlloc.page, out var x, out var y);
		return x == atlasRect.xMin && y == atlasRect.yMin && allocator.entryWidth * pageWidth == atlasRect.width && allocator.entryHeight * pageHeight == atlasRect.height;
	}

	public void Construct()
	{
		m_OpacityAllocator = (m_ColorAllocator = (m_ClipRectAllocator = (m_TransformAllocator = (m_TextSettingsAllocator = default(BitmapAllocator32)))));
		m_TransformAllocator.Construct(pageHeight, 1, 3);
		m_TransformAllocator.ForceFirstAlloc((ushort)identityTransformTexel.x, (ushort)identityTransformTexel.y);
		m_ClipRectAllocator.Construct(pageHeight);
		m_ClipRectAllocator.ForceFirstAlloc((ushort)infiniteClipRectTexel.x, (ushort)infiniteClipRectTexel.y);
		m_OpacityAllocator.Construct(pageHeight);
		m_OpacityAllocator.ForceFirstAlloc((ushort)fullOpacityTexel.x, (ushort)fullOpacityTexel.y);
		m_ColorAllocator.Construct(pageHeight);
		m_ColorAllocator.ForceFirstAlloc((ushort)clearColorTexel.x, (ushort)clearColorTexel.y);
		m_TextSettingsAllocator.Construct(pageHeight, 1, 4);
		m_TextSettingsAllocator.ForceFirstAlloc((ushort)defaultTextCoreSettingsTexel.x, (ushort)defaultTextCoreSettingsTexel.y);
		m_VertexTexturingEnabled = UIRenderDevice.vertexTexturingIsAvailable;
		if (!m_VertexTexturingEnabled)
		{
			int length = 20;
			m_Transforms = new NativeArray<Transform3x4>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			m_ClipRects = new NativeArray<Vector4>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			m_Transforms[0] = new Transform3x4
			{
				v0 = identityTransformRow0Value,
				v1 = identityTransformRow1Value,
				v2 = identityTransformRow2Value
			};
			m_ClipRects[0] = infiniteClipRectValue;
		}
	}

	private void ReallyCreateStorage()
	{
		if (m_VertexTexturingEnabled)
		{
			m_Storage = new ShaderInfoStorageRGBAFloat();
		}
		else
		{
			m_Storage = new ShaderInfoStorageRGBA32();
		}
		m_Storage.AllocateRect(pageWidth * m_TransformAllocator.entryWidth, pageHeight * m_TransformAllocator.entryHeight, out var uvs);
		m_Storage.AllocateRect(pageWidth * m_ClipRectAllocator.entryWidth, pageHeight * m_ClipRectAllocator.entryHeight, out var uvs2);
		m_Storage.AllocateRect(pageWidth * m_OpacityAllocator.entryWidth, pageHeight * m_OpacityAllocator.entryHeight, out var uvs3);
		m_Storage.AllocateRect(pageWidth * m_ColorAllocator.entryWidth, pageHeight * m_ColorAllocator.entryHeight, out var uvs4);
		m_Storage.AllocateRect(pageWidth * m_TextSettingsAllocator.entryWidth, pageHeight * m_TextSettingsAllocator.entryHeight, out var uvs5);
		if (!AtlasRectMatchesPage(ref m_TransformAllocator, identityTransform, uvs))
		{
			throw new Exception("Atlas identity transform allocation failed unexpectedly");
		}
		if (!AtlasRectMatchesPage(ref m_ClipRectAllocator, infiniteClipRect, uvs2))
		{
			throw new Exception("Atlas infinite clip rect allocation failed unexpectedly");
		}
		if (!AtlasRectMatchesPage(ref m_OpacityAllocator, fullOpacity, uvs3))
		{
			throw new Exception("Atlas full opacity allocation failed unexpectedly");
		}
		if (!AtlasRectMatchesPage(ref m_ColorAllocator, clearColor, uvs4))
		{
			throw new Exception("Atlas clear color allocation failed unexpectedly");
		}
		if (!AtlasRectMatchesPage(ref m_TextSettingsAllocator, defaultTextCoreSettings, uvs5))
		{
			throw new Exception("Atlas text setting allocation failed unexpectedly");
		}
		SetTransformValue(identityTransform, identityTransformValue);
		SetClipRectValue(infiniteClipRect, infiniteClipRectValue);
		SetOpacityValue(fullOpacity, fullOpacityValue.w);
		SetColorValue(clearColor, clearColorValue, isEditorContext: false);
		SetTextCoreSettingValue(defaultTextCoreSettings, defaultTextCoreSettingsValue, isEditorContext: false);
		m_StorageReallyCreated = true;
	}

	public void Dispose()
	{
		if (m_Storage != null)
		{
			m_Storage.Dispose();
		}
		m_Storage = null;
		if (m_ClipRects.IsCreated)
		{
			m_ClipRects.Dispose();
		}
		if (m_Transforms.IsCreated)
		{
			m_Transforms.Dispose();
		}
		m_StorageReallyCreated = false;
	}

	public void IssuePendingStorageChanges()
	{
		m_Storage?.UpdateTexture();
	}

	public BMPAlloc AllocTransform()
	{
		if (!m_StorageReallyCreated)
		{
			ReallyCreateStorage();
		}
		if (m_VertexTexturingEnabled)
		{
			return m_TransformAllocator.Allocate(m_Storage);
		}
		BMPAlloc bMPAlloc = m_TransformAllocator.Allocate(null);
		if (AllocToConstantBufferIndex(bMPAlloc) < m_Transforms.Length)
		{
			return bMPAlloc;
		}
		m_TransformAllocator.Free(bMPAlloc);
		return BMPAlloc.Invalid;
	}

	public BMPAlloc AllocClipRect()
	{
		if (!m_StorageReallyCreated)
		{
			ReallyCreateStorage();
		}
		if (m_VertexTexturingEnabled)
		{
			return m_ClipRectAllocator.Allocate(m_Storage);
		}
		BMPAlloc bMPAlloc = m_ClipRectAllocator.Allocate(null);
		if (AllocToConstantBufferIndex(bMPAlloc) < m_ClipRects.Length)
		{
			return bMPAlloc;
		}
		m_ClipRectAllocator.Free(bMPAlloc);
		return BMPAlloc.Invalid;
	}

	public BMPAlloc AllocOpacity()
	{
		if (!m_StorageReallyCreated)
		{
			ReallyCreateStorage();
		}
		return m_OpacityAllocator.Allocate(m_Storage);
	}

	public BMPAlloc AllocColor()
	{
		if (!m_StorageReallyCreated)
		{
			ReallyCreateStorage();
		}
		return m_ColorAllocator.Allocate(m_Storage);
	}

	public BMPAlloc AllocTextCoreSettings(TextCoreSettings settings)
	{
		if (!m_StorageReallyCreated)
		{
			ReallyCreateStorage();
		}
		return m_TextSettingsAllocator.Allocate(m_Storage);
	}

	public void SetTransformValue(BMPAlloc alloc, Matrix4x4 xform)
	{
		Debug.Assert(alloc.IsValid());
		if (m_VertexTexturingEnabled)
		{
			Vector2Int vector2Int = AllocToTexelCoord(ref m_TransformAllocator, alloc);
			m_Storage.SetTexel(vector2Int.x, vector2Int.y, xform.GetRow(0));
			m_Storage.SetTexel(vector2Int.x, vector2Int.y + 1, xform.GetRow(1));
			m_Storage.SetTexel(vector2Int.x, vector2Int.y + 2, xform.GetRow(2));
		}
		else
		{
			m_Transforms[AllocToConstantBufferIndex(alloc)] = new Transform3x4
			{
				v0 = xform.GetRow(0),
				v1 = xform.GetRow(1),
				v2 = xform.GetRow(2)
			};
		}
	}

	public void SetClipRectValue(BMPAlloc alloc, Vector4 clipRect)
	{
		Debug.Assert(alloc.IsValid());
		if (m_VertexTexturingEnabled)
		{
			Vector2Int vector2Int = AllocToTexelCoord(ref m_ClipRectAllocator, alloc);
			m_Storage.SetTexel(vector2Int.x, vector2Int.y, clipRect);
		}
		else
		{
			m_ClipRects[AllocToConstantBufferIndex(alloc)] = clipRect;
		}
	}

	public void SetOpacityValue(BMPAlloc alloc, float opacity)
	{
		Debug.Assert(alloc.IsValid());
		Vector2Int vector2Int = AllocToTexelCoord(ref m_OpacityAllocator, alloc);
		m_Storage.SetTexel(vector2Int.x, vector2Int.y, new Color(1f, 1f, 1f, opacity));
	}

	public void SetColorValue(BMPAlloc alloc, Color color, bool isEditorContext)
	{
		Debug.Assert(alloc.IsValid());
		Vector2Int vector2Int = AllocToTexelCoord(ref m_ColorAllocator, alloc);
		if (QualitySettings.activeColorSpace == ColorSpace.Linear && !isEditorContext)
		{
			m_Storage.SetTexel(vector2Int.x, vector2Int.y, color.linear);
		}
		else
		{
			m_Storage.SetTexel(vector2Int.x, vector2Int.y, color);
		}
	}

	public void SetTextCoreSettingValue(BMPAlloc alloc, TextCoreSettings settings, bool isEditorContext)
	{
		Debug.Assert(alloc.IsValid());
		Vector2Int vector2Int = AllocToTexelCoord(ref m_TextSettingsAllocator, alloc);
		Color color = new Color(0f - settings.underlayOffset.x, settings.underlayOffset.y, settings.underlaySoftness, settings.outlineWidth);
		if (QualitySettings.activeColorSpace == ColorSpace.Linear && !isEditorContext)
		{
			m_Storage.SetTexel(vector2Int.x, vector2Int.y, settings.faceColor.linear);
			m_Storage.SetTexel(vector2Int.x, vector2Int.y + 1, settings.outlineColor.linear);
			m_Storage.SetTexel(vector2Int.x, vector2Int.y + 2, settings.underlayColor.linear);
		}
		else
		{
			m_Storage.SetTexel(vector2Int.x, vector2Int.y, settings.faceColor);
			m_Storage.SetTexel(vector2Int.x, vector2Int.y + 1, settings.outlineColor);
			m_Storage.SetTexel(vector2Int.x, vector2Int.y + 2, settings.underlayColor);
		}
		m_Storage.SetTexel(vector2Int.x, vector2Int.y + 3, color);
	}

	public void FreeTransform(BMPAlloc alloc)
	{
		Debug.Assert(alloc.IsValid());
		m_TransformAllocator.Free(alloc);
	}

	public void FreeClipRect(BMPAlloc alloc)
	{
		Debug.Assert(alloc.IsValid());
		m_ClipRectAllocator.Free(alloc);
	}

	public void FreeOpacity(BMPAlloc alloc)
	{
		Debug.Assert(alloc.IsValid());
		m_OpacityAllocator.Free(alloc);
	}

	public void FreeColor(BMPAlloc alloc)
	{
		Debug.Assert(alloc.IsValid());
		m_ColorAllocator.Free(alloc);
	}

	public void FreeTextCoreSettings(BMPAlloc alloc)
	{
		Debug.Assert(alloc.IsValid());
		m_TextSettingsAllocator.Free(alloc);
	}

	public Color32 TransformAllocToVertexData(BMPAlloc alloc)
	{
		Debug.Assert(pageWidth == 32 && pageHeight == 8);
		ushort x = 0;
		ushort y = 0;
		if (m_VertexTexturingEnabled)
		{
			m_TransformAllocator.GetAllocPageAtlasLocation(alloc.page, out x, out y);
		}
		return new Color32((byte)(x >> 5), (byte)(y >> 3), (byte)(alloc.pageLine * pageWidth + alloc.bitIndex), 0);
	}

	public Color32 ClipRectAllocToVertexData(BMPAlloc alloc)
	{
		Debug.Assert(pageWidth == 32 && pageHeight == 8);
		ushort x = 0;
		ushort y = 0;
		if (m_VertexTexturingEnabled)
		{
			m_ClipRectAllocator.GetAllocPageAtlasLocation(alloc.page, out x, out y);
		}
		return new Color32((byte)(x >> 5), (byte)(y >> 3), (byte)(alloc.pageLine * pageWidth + alloc.bitIndex), 0);
	}

	public Color32 OpacityAllocToVertexData(BMPAlloc alloc)
	{
		Debug.Assert(pageWidth == 32 && pageHeight == 8);
		m_OpacityAllocator.GetAllocPageAtlasLocation(alloc.page, out var x, out var y);
		return new Color32((byte)(x >> 5), (byte)(y >> 3), (byte)(alloc.pageLine * pageWidth + alloc.bitIndex), 0);
	}

	public Color32 ColorAllocToVertexData(BMPAlloc alloc)
	{
		Debug.Assert(pageWidth == 32 && pageHeight == 8);
		m_ColorAllocator.GetAllocPageAtlasLocation(alloc.page, out var x, out var y);
		return new Color32((byte)(x >> 5), (byte)(y >> 3), (byte)(alloc.pageLine * pageWidth + alloc.bitIndex), 0);
	}

	public Color32 TextCoreSettingsToVertexData(BMPAlloc alloc)
	{
		Debug.Assert(pageWidth == 32 && pageHeight == 8);
		m_TextSettingsAllocator.GetAllocPageAtlasLocation(alloc.page, out var x, out var y);
		return new Color32((byte)(x >> 5), (byte)(y >> 3), (byte)(alloc.pageLine * pageWidth + alloc.bitIndex), 0);
	}
}
