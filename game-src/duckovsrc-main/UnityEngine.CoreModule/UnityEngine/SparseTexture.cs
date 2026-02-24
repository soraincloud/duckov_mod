using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Internal;

namespace UnityEngine;

[NativeHeader("Runtime/Graphics/SparseTexture.h")]
public sealed class SparseTexture : Texture
{
	public extern int tileWidth
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern int tileHeight
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern bool isCreated
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("IsInitialized")]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "SparseTextureScripting::Create", ThrowsException = true)]
	private static extern void Internal_Create([Writable] SparseTexture mono, int width, int height, GraphicsFormat format, TextureColorSpace colorSpace, int mipCount);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "SparseTextureScripting::UpdateTile", HasExplicitThis = true)]
	public extern void UpdateTile(int tileX, int tileY, int miplevel, [Unmarshalled] Color32[] data);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "SparseTextureScripting::UpdateTileRaw", HasExplicitThis = true)]
	public extern void UpdateTileRaw(int tileX, int tileY, int miplevel, [Unmarshalled] byte[] data);

	public void UnloadTile(int tileX, int tileY, int miplevel)
	{
		UpdateTileRaw(tileX, tileY, miplevel, null);
	}

	internal bool ValidateFormat(TextureFormat format, int width, int height)
	{
		bool flag = ValidateFormat(format);
		if (flag && TextureFormat.PVRTC_RGB2 <= format && format <= TextureFormat.PVRTC_RGBA4 && (width != height || !Mathf.IsPowerOfTwo(width)))
		{
			throw new UnityException($"'{format.ToString()}' demands texture to be square and have power-of-two dimensions");
		}
		return flag;
	}

	internal bool ValidateFormat(GraphicsFormat format, int width, int height)
	{
		bool flag = ValidateFormat(format, FormatUsage.Sparse);
		if (flag && GraphicsFormatUtility.IsPVRTCFormat(format) && (width != height || !Mathf.IsPowerOfTwo(width)))
		{
			throw new UnityException($"'{format.ToString()}' demands texture to be square and have power-of-two dimensions");
		}
		return flag;
	}

	internal bool ValidateSize(int width, int height, GraphicsFormat format)
	{
		if (GraphicsFormatUtility.GetBlockSize(format) * (width / GraphicsFormatUtility.GetBlockWidth(format)) * (height / GraphicsFormatUtility.GetBlockHeight(format)) < 65536)
		{
			Debug.LogError("SparseTexture creation failed. The minimum size in bytes of a SparseTexture is 64KB.", this);
			return false;
		}
		return true;
	}

	private static void ValidateIsNotCrunched(TextureFormat textureFormat)
	{
		if (GraphicsFormatUtility.IsCrunchFormat(textureFormat))
		{
			throw new ArgumentException("Crunched SparseTexture is not supported.");
		}
	}

	[ExcludeFromDocs]
	public SparseTexture(int width, int height, DefaultFormat format, int mipCount)
		: this(width, height, SystemInfo.GetGraphicsFormat(format), mipCount)
	{
	}

	[ExcludeFromDocs]
	public SparseTexture(int width, int height, GraphicsFormat format, int mipCount)
	{
		if (ValidateFormat(format, width, height) && ValidateSize(width, height, format))
		{
			Internal_Create(this, width, height, format, GetTextureColorSpace(format), mipCount);
		}
	}

	[ExcludeFromDocs]
	public SparseTexture(int width, int height, TextureFormat textureFormat, int mipCount)
		: this(width, height, textureFormat, mipCount, linear: false)
	{
	}

	public SparseTexture(int width, int height, TextureFormat textureFormat, int mipCount, [DefaultValue("false")] bool linear)
	{
		if (ValidateFormat(textureFormat, width, height))
		{
			ValidateIsNotCrunched(textureFormat);
			GraphicsFormat format = GraphicsFormatUtility.GetGraphicsFormat(textureFormat, !linear);
			if (!SystemInfo.IsFormatSupported(format, FormatUsage.Sparse))
			{
				Debug.LogError($"Creation of a SparseTexture with '{textureFormat}' is not supported on this platform.");
			}
			else if (ValidateSize(width, height, format))
			{
				Internal_Create(this, width, height, format, GetTextureColorSpace(linear), mipCount);
			}
		}
	}
}
