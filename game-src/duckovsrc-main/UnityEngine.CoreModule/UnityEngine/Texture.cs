using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace UnityEngine;

[UsedByNativeCode]
[NativeHeader("Runtime/Streaming/TextureStreamingManager.h")]
[NativeHeader("Runtime/Graphics/Texture.h")]
public class Texture : Object
{
	public static readonly int GenerateAllMips = -1;

	[Obsolete("masterTextureLimit has been deprecated. Use globalMipmapLimit instead (UnityUpgradable) -> globalMipmapLimit", false)]
	[NativeProperty("ActiveGlobalMipmapLimit")]
	public static extern int masterTextureLimit
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("ActiveGlobalMipmapLimit")]
	[Obsolete("globalMipmapLimit is not supported. Use QualitySettings.globalTextureMipmapLimit or Mipmap Limit Groups instead.", false)]
	public static extern int globalMipmapLimit
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int mipmapCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetMipmapCount")]
		get;
	}

	[NativeProperty("AnisoLimit")]
	public static extern AnisotropicFiltering anisotropicFiltering
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public virtual GraphicsFormat graphicsFormat => GraphicsFormatUtility.GetFormat(this);

	public virtual int width
	{
		get
		{
			return GetDataWidth();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public virtual int height
	{
		get
		{
			return GetDataHeight();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public virtual TextureDimension dimension
	{
		get
		{
			return GetDimension();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	internal extern bool isNativeTexture
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("IsNativeTexture")]
		get;
	}

	public virtual extern bool isReadable
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern TextureWrapMode wrapMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetWrapModeU")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern TextureWrapMode wrapModeU
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern TextureWrapMode wrapModeV
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern TextureWrapMode wrapModeW
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern FilterMode filterMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int anisoLevel
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float mipMapBias
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public Vector2 texelSize
	{
		[NativeName("GetTexelSize")]
		get
		{
			get_texelSize_Injected(out var ret);
			return ret;
		}
	}

	public extern uint updateCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	internal ColorSpace activeTextureColorSpace
	{
		[VisibleToOtherModules(new string[] { "UnityEngine.UIElementsModule", "Unity.UIElements" })]
		get
		{
			return (Internal_GetActiveTextureColorSpace() == 0) ? ColorSpace.Linear : ColorSpace.Gamma;
		}
	}

	public bool isDataSRGB => Internal_GetStoredColorSpace() == TextureColorSpace.sRGB;

	public static extern ulong totalTextureMemory
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetTextureStreamingManager().GetTotalTextureMemory")]
		get;
	}

	public static extern ulong desiredTextureMemory
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetTextureStreamingManager().GetDesiredTextureMemory")]
		get;
	}

	public static extern ulong targetTextureMemory
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetTextureStreamingManager().GetTargetTextureMemory")]
		get;
	}

	public static extern ulong currentTextureMemory
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetTextureStreamingManager().GetCurrentTextureMemory")]
		get;
	}

	public static extern ulong nonStreamingTextureMemory
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetTextureStreamingManager().GetNonStreamingTextureMemory")]
		get;
	}

	public static extern ulong streamingMipmapUploadCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetTextureStreamingManager().GetStreamingMipmapUploadCount")]
		get;
	}

	public static extern ulong streamingRendererCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetTextureStreamingManager().GetStreamingRendererCount")]
		get;
	}

	public static extern ulong streamingTextureCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetTextureStreamingManager().GetStreamingTextureCount")]
		get;
	}

	public static extern ulong nonStreamingTextureCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetTextureStreamingManager().GetNonStreamingTextureCount")]
		get;
	}

	public static extern ulong streamingTexturePendingLoadCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetTextureStreamingManager().GetStreamingTexturePendingLoadCount")]
		get;
	}

	public static extern ulong streamingTextureLoadingCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetTextureStreamingManager().GetStreamingTextureLoadingCount")]
		get;
	}

	public static extern bool streamingTextureForceLoadAll
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction(Name = "GetTextureStreamingManager().GetForceLoadAll")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction(Name = "GetTextureStreamingManager().SetForceLoadAll")]
		set;
	}

	public static extern bool streamingTextureDiscardUnusedMips
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction(Name = "GetTextureStreamingManager().GetDiscardUnusedMips")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction(Name = "GetTextureStreamingManager().SetDiscardUnusedMips")]
		set;
	}

	public static extern bool allowThreadedTextureCreation
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction(Name = "Texture2DScripting::IsCreateTextureThreadedEnabled")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction(Name = "Texture2DScripting::EnableCreateTextureThreaded")]
		set;
	}

	protected Texture()
	{
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("SetGlobalAnisoLimits")]
	public static extern void SetGlobalAnisotropicFilteringLimits(int forcedMin, int globalMax);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern int GetDataWidth();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern int GetDataHeight();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern TextureDimension GetDimension();

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern IntPtr GetNativeTexturePtr();

	[Obsolete("Use GetNativeTexturePtr instead.", false)]
	public int GetNativeTextureID()
	{
		return (int)GetNativeTexturePtr();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void IncrementUpdateCount();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("GetActiveTextureColorSpace")]
	private extern int Internal_GetActiveTextureColorSpace();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("GetStoredColorSpace")]
	private extern TextureColorSpace Internal_GetStoredColorSpace();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("GetTextureStreamingManager().SetStreamingTextureMaterialDebugProperties")]
	public static extern void SetStreamingTextureMaterialDebugProperties();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern ulong GetPixelDataSize(int mipLevel, int element = 0);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern ulong GetPixelDataOffset(int mipLevel, int element = 0);

	internal TextureColorSpace GetTextureColorSpace(bool linear)
	{
		return (!linear) ? TextureColorSpace.sRGB : TextureColorSpace.Linear;
	}

	internal TextureColorSpace GetTextureColorSpace(GraphicsFormat format)
	{
		return GetTextureColorSpace(!GraphicsFormatUtility.IsSRGBFormat(format));
	}

	internal bool ValidateFormat(RenderTextureFormat format)
	{
		if (SystemInfo.SupportsRenderTextureFormat(format))
		{
			return true;
		}
		Debug.LogError($"RenderTexture creation failed. '{format.ToString()}' is not supported on this platform. Use 'SystemInfo.SupportsRenderTextureFormat' C# API to check format support.", this);
		return false;
	}

	internal bool ValidateFormat(TextureFormat format)
	{
		if (SystemInfo.SupportsTextureFormat(format))
		{
			return true;
		}
		if (GraphicsFormatUtility.IsCompressedFormat(format) && GraphicsFormatUtility.CanDecompressFormat(GraphicsFormatUtility.GetGraphicsFormat(format, isSRGB: false)))
		{
			Debug.LogWarning($"'{format.ToString()}' is not supported on this platform. Decompressing texture. Use 'SystemInfo.SupportsTextureFormat' C# API to check format support.", this);
			return true;
		}
		Debug.LogError($"Texture creation failed. '{format.ToString()}' is not supported on this platform. Use 'SystemInfo.SupportsTextureFormat' C# API to check format support.", this);
		return false;
	}

	internal bool ValidateFormat(GraphicsFormat format, FormatUsage usage)
	{
		if (usage != FormatUsage.Render && (format == GraphicsFormat.ShadowAuto || format == GraphicsFormat.DepthAuto))
		{
			Debug.LogWarning($"'{format.ToString()}' is not allowed because it is an auto format and not an exact format. Use GraphicsFormatUtility.GetDepthStencilFormat to get an exact depth/stencil format.", this);
			return false;
		}
		if (SystemInfo.IsFormatSupported(format, usage))
		{
			return true;
		}
		Debug.LogError($"Texture creation failed. '{format.ToString()}' is not supported for {usage.ToString()} usage on this platform. Use 'SystemInfo.IsFormatSupported' C# API to check format support.", this);
		return false;
	}

	internal UnityException CreateNonReadableException(Texture t)
	{
		return new UnityException($"Texture '{t.name}' is not readable, the texture memory can not be accessed from scripts. You can make the texture readable in the Texture Import Settings.");
	}

	internal UnityException CreateNativeArrayLengthOverflowException()
	{
		return new UnityException("Failed to create NativeArray, length exceeds the allowed maximum of Int32.MaxValue. Use a larger type as template argument to reduce the array length.");
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_texelSize_Injected(out Vector2 ret);
}
