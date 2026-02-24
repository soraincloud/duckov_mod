#define UNITY_ASSERTIONS
using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Internal;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering;

[NativeHeader("Runtime/Graphics/TextureFormat.h")]
[NativeHeader("Runtime/Graphics/GraphicsFormatUtility.bindings.h")]
[NativeHeader("Runtime/Graphics/Format.h")]
public class GraphicsFormatUtility
{
	private static readonly GraphicsFormat[] tableNoStencil = new GraphicsFormat[5]
	{
		GraphicsFormat.None,
		GraphicsFormat.D16_UNorm,
		GraphicsFormat.D16_UNorm,
		GraphicsFormat.D24_UNorm,
		GraphicsFormat.D32_SFloat
	};

	private static readonly GraphicsFormat[] tableStencil = new GraphicsFormat[5]
	{
		GraphicsFormat.S8_UInt,
		GraphicsFormat.D16_UNorm_S8_UInt,
		GraphicsFormat.D16_UNorm_S8_UInt,
		GraphicsFormat.D24_UNorm_S8_UInt,
		GraphicsFormat.D32_SFloat_S8_UInt
	};

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("GetGraphicsFormat_Native_Texture")]
	internal static extern GraphicsFormat GetFormat([NotNull("NullExceptionObject")] Texture texture);

	public static GraphicsFormat GetGraphicsFormat(TextureFormat format, bool isSRGB)
	{
		return GetGraphicsFormat_Native_TextureFormat(format, isSRGB);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	private static extern GraphicsFormat GetGraphicsFormat_Native_TextureFormat(TextureFormat format, bool isSRGB);

	public static TextureFormat GetTextureFormat(GraphicsFormat format)
	{
		return GetTextureFormat_Native_GraphicsFormat(format);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	private static extern TextureFormat GetTextureFormat_Native_GraphicsFormat(GraphicsFormat format);

	public static GraphicsFormat GetGraphicsFormat(RenderTextureFormat format, bool isSRGB)
	{
		return GetGraphicsFormat_Native_RenderTextureFormat(format, isSRGB);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = false)]
	private static extern GraphicsFormat GetGraphicsFormat_Native_RenderTextureFormat(RenderTextureFormat format, bool isSRGB);

	public static GraphicsFormat GetGraphicsFormat(RenderTextureFormat format, RenderTextureReadWrite readWrite)
	{
		bool flag = QualitySettings.activeColorSpace == ColorSpace.Linear;
		bool isSRGB = ((readWrite == RenderTextureReadWrite.Default) ? flag : (readWrite == RenderTextureReadWrite.sRGB));
		return GetGraphicsFormat(format, isSRGB);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	private static extern GraphicsFormat GetDepthStencilFormatFromBitsLegacy_Native(int minimumDepthBits);

	internal static GraphicsFormat GetDepthStencilFormat(int minimumDepthBits)
	{
		return GetDepthStencilFormatFromBitsLegacy_Native(minimumDepthBits);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern int GetDepthBits(GraphicsFormat format);

	public static GraphicsFormat GetDepthStencilFormat(int minimumDepthBits, int minimumStencilBits)
	{
		if (minimumDepthBits == 0 && minimumStencilBits == 0)
		{
			return GraphicsFormat.None;
		}
		if (minimumDepthBits < 0 || minimumStencilBits < 0)
		{
			throw new ArgumentException("Number of bits in DepthStencil format can't be negative.");
		}
		if (minimumDepthBits > 32)
		{
			throw new ArgumentException("Number of depth buffer bits cannot exceed 32.");
		}
		if (minimumStencilBits > 8)
		{
			throw new ArgumentException("Number of stencil buffer bits cannot exceed 8.");
		}
		minimumDepthBits = ((minimumDepthBits != 0) ? ((minimumDepthBits <= 16) ? 16 : ((minimumDepthBits > 24) ? 32 : 24)) : 0);
		if (minimumStencilBits != 0)
		{
			minimumStencilBits = 8;
		}
		Debug.Assert(tableNoStencil.Length == tableStencil.Length);
		GraphicsFormat[] array = ((minimumStencilBits > 0) ? tableStencil : tableNoStencil);
		int num = minimumDepthBits / 8;
		for (int i = num; i < array.Length; i++)
		{
			GraphicsFormat graphicsFormat = array[i];
			if (SystemInfo.IsFormatSupported(graphicsFormat, FormatUsage.Render))
			{
				return graphicsFormat;
			}
		}
		return GraphicsFormat.None;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsSRGBFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsSwizzleFormat(GraphicsFormat format);

	public static bool IsSwizzleFormat(TextureFormat format)
	{
		return IsSwizzleFormat(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern GraphicsFormat GetSRGBFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern GraphicsFormat GetLinearFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern RenderTextureFormat GetRenderTextureFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern uint GetColorComponentCount(GraphicsFormat format);

	public static uint GetColorComponentCount(TextureFormat format)
	{
		return GetColorComponentCount(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern uint GetAlphaComponentCount(GraphicsFormat format);

	public static uint GetAlphaComponentCount(TextureFormat format)
	{
		return GetAlphaComponentCount(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern uint GetComponentCount(GraphicsFormat format);

	public static uint GetComponentCount(TextureFormat format)
	{
		return GetComponentCount(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern string GetFormatString(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	private static extern string GetFormatString_Native_TextureFormat(TextureFormat format);

	public static string GetFormatString(TextureFormat format)
	{
		return GetFormatString_Native_TextureFormat(format);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsCompressedFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	private static extern bool IsCompressedFormat_Native_TextureFormat(TextureFormat format);

	[Obsolete("IsCompressedTextureFormat is obsolete, please use IsCompressedFormat instead.")]
	internal static bool IsCompressedTextureFormat(TextureFormat format)
	{
		return IsCompressedFormat(format);
	}

	public static bool IsCompressedFormat(TextureFormat format)
	{
		return IsCompressedFormat_Native_TextureFormat(format);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	private static extern bool CanDecompressFormat(GraphicsFormat format, bool wholeImage);

	internal static bool CanDecompressFormat(GraphicsFormat format)
	{
		return CanDecompressFormat(format, wholeImage: true);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsPackedFormat(GraphicsFormat format);

	public static bool IsPackedFormat(TextureFormat format)
	{
		return IsPackedFormat(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool Is16BitPackedFormat(GraphicsFormat format);

	public static bool Is16BitPackedFormat(TextureFormat format)
	{
		return Is16BitPackedFormat(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern GraphicsFormat ConvertToAlphaFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	private static extern TextureFormat ConvertToAlphaFormat_Native_TextureFormat(TextureFormat format);

	public static TextureFormat ConvertToAlphaFormat(TextureFormat format)
	{
		return ConvertToAlphaFormat_Native_TextureFormat(format);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsAlphaOnlyFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	private static extern bool IsAlphaOnlyFormat_Native_TextureFormat(TextureFormat format);

	public static bool IsAlphaOnlyFormat(TextureFormat format)
	{
		return IsAlphaOnlyFormat_Native_TextureFormat(format);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsAlphaTestFormat(GraphicsFormat format);

	public static bool IsAlphaTestFormat(TextureFormat format)
	{
		return IsAlphaTestFormat(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool HasAlphaChannel(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	private static extern bool HasAlphaChannel_Native_TextureFormat(TextureFormat format);

	public static bool HasAlphaChannel(TextureFormat format)
	{
		return HasAlphaChannel_Native_TextureFormat(format);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsDepthFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsStencilFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsDepthStencilFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsIEEE754Format(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsFloatFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsHalfFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsUnsignedFormat(GraphicsFormat format);

	public static bool IsUnsignedFormat(TextureFormat format)
	{
		return IsUnsignedFormat(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsSignedFormat(GraphicsFormat format);

	public static bool IsSignedFormat(TextureFormat format)
	{
		return IsSignedFormat(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsNormFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsUNormFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsSNormFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsIntegerFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsUIntFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsSIntFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsXRFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsDXTCFormat(GraphicsFormat format);

	public static bool IsDXTCFormat(TextureFormat format)
	{
		return IsDXTCFormat(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsRGTCFormat(GraphicsFormat format);

	public static bool IsRGTCFormat(TextureFormat format)
	{
		return IsRGTCFormat(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsBPTCFormat(GraphicsFormat format);

	public static bool IsBPTCFormat(TextureFormat format)
	{
		return IsBPTCFormat(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsBCFormat(GraphicsFormat format);

	public static bool IsBCFormat(TextureFormat format)
	{
		return IsBCFormat(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsPVRTCFormat(GraphicsFormat format);

	public static bool IsPVRTCFormat(TextureFormat format)
	{
		return IsPVRTCFormat(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsETCFormat(GraphicsFormat format);

	public static bool IsETCFormat(TextureFormat format)
	{
		return IsETCFormat(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsEACFormat(GraphicsFormat format);

	public static bool IsEACFormat(TextureFormat format)
	{
		return IsEACFormat(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsASTCFormat(GraphicsFormat format);

	public static bool IsASTCFormat(TextureFormat format)
	{
		return IsASTCFormat(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool IsHDRFormat(GraphicsFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	private static extern bool IsHDRFormat_Native_TextureFormat(TextureFormat format);

	public static bool IsHDRFormat(TextureFormat format)
	{
		return IsHDRFormat_Native_TextureFormat(format);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("IsCompressedCrunchTextureFormat", IsThreadSafe = true)]
	public static extern bool IsCrunchFormat(TextureFormat format);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern FormatSwizzle GetSwizzleR(GraphicsFormat format);

	public static FormatSwizzle GetSwizzleR(TextureFormat format)
	{
		return GetSwizzleR(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern FormatSwizzle GetSwizzleG(GraphicsFormat format);

	public static FormatSwizzle GetSwizzleG(TextureFormat format)
	{
		return GetSwizzleG(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern FormatSwizzle GetSwizzleB(GraphicsFormat format);

	public static FormatSwizzle GetSwizzleB(TextureFormat format)
	{
		return GetSwizzleB(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern FormatSwizzle GetSwizzleA(GraphicsFormat format);

	public static FormatSwizzle GetSwizzleA(TextureFormat format)
	{
		return GetSwizzleA(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern uint GetBlockSize(GraphicsFormat format);

	public static uint GetBlockSize(TextureFormat format)
	{
		return GetBlockSize(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern uint GetBlockWidth(GraphicsFormat format);

	public static uint GetBlockWidth(TextureFormat format)
	{
		return GetBlockWidth(GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern uint GetBlockHeight(GraphicsFormat format);

	public static uint GetBlockHeight(TextureFormat format)
	{
		return GetBlockHeight(GetGraphicsFormat(format, isSRGB: false));
	}

	public static uint ComputeMipmapSize(int width, int height, GraphicsFormat format)
	{
		return ComputeMipChainSize_Native_2D(width, height, format, 1);
	}

	public static uint ComputeMipmapSize(int width, int height, TextureFormat format)
	{
		return ComputeMipmapSize(width, height, GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	private static extern uint ComputeMipChainSize_Native_2D(int width, int height, GraphicsFormat format, int mipCount);

	public static uint ComputeMipChainSize(int width, int height, GraphicsFormat format, [DefaultValue("-1")] int mipCount = -1)
	{
		return ComputeMipChainSize_Native_2D(width, height, format, mipCount);
	}

	public static uint ComputeMipChainSize(int width, int height, TextureFormat format, [DefaultValue("-1")] int mipCount = -1)
	{
		return ComputeMipChainSize_Native_2D(width, height, GetGraphicsFormat(format, isSRGB: false), mipCount);
	}

	public static uint ComputeMipmapSize(int width, int height, int depth, GraphicsFormat format)
	{
		return ComputeMipChainSize_Native_3D(width, height, depth, format, 1);
	}

	public static uint ComputeMipmapSize(int width, int height, int depth, TextureFormat format)
	{
		return ComputeMipmapSize(width, height, depth, GetGraphicsFormat(format, isSRGB: false));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	private static extern uint ComputeMipChainSize_Native_3D(int width, int height, int depth, GraphicsFormat format, int mipCount);

	public static uint ComputeMipChainSize(int width, int height, int depth, GraphicsFormat format, [DefaultValue("-1")] int mipCount = -1)
	{
		return ComputeMipChainSize_Native_3D(width, height, depth, format, mipCount);
	}

	public static uint ComputeMipChainSize(int width, int height, int depth, TextureFormat format, [DefaultValue("-1")] int mipCount = -1)
	{
		return ComputeMipChainSize_Native_3D(width, height, depth, GetGraphicsFormat(format, isSRGB: false), mipCount);
	}
}
