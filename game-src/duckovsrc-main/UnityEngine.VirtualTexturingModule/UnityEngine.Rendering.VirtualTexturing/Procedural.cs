using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Bindings;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering.VirtualTexturing;

[NativeHeader("Modules/VirtualTexturing/ScriptBindings/VirtualTexturing.bindings.h")]
[Obsolete("Procedural Virtual Texturing is experimental, not ready for production use and Unity does not currently support it. The feature might be changed or removed in the future.", false)]
[StaticAccessor("VirtualTexturing::Procedural", StaticAccessorType.DoubleColon)]
public static class Procedural
{
	[NativeHeader("Modules/VirtualTexturing/ScriptBindings/VirtualTexturing.bindings.h")]
	[StaticAccessor("VirtualTexturing::Procedural", StaticAccessorType.DoubleColon)]
	internal static class Binding
	{
		internal static ulong Create(CreationParameters p)
		{
			return Create_Injected(ref p);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void Destroy(ulong handle);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeThrows]
		internal static extern int PopRequests(ulong handle, IntPtr requestHandles, int length);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ThreadSafe]
		[NativeThrows]
		internal static extern void GetRequestParameters(IntPtr requestHandles, IntPtr requestParameters, int length);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeThrows]
		[ThreadSafe]
		internal static extern void UpdateRequestState(IntPtr requestHandles, IntPtr requestUpdates, int length);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeThrows]
		[ThreadSafe]
		internal static extern void UpdateRequestStateWithCommandBuffer(IntPtr requestHandles, IntPtr requestUpdates, int length, CommandBuffer fenceBuffer);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void BindToMaterialPropertyBlock(ulong handle, [NotNull("ArgumentNullException")] MaterialPropertyBlock material, string name);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void BindToMaterial(ulong handle, [NotNull("ArgumentNullException")] Material material, string name);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void BindGlobally(ulong handle, string name);

		[NativeThrows]
		internal static void RequestRegion(ulong handle, Rect r, int mipMap, int numMips)
		{
			RequestRegion_Injected(handle, ref r, mipMap, numMips);
		}

		[NativeThrows]
		internal static void InvalidateRegion(ulong handle, Rect r, int mipMap, int numMips)
		{
			InvalidateRegion_Injected(handle, ref r, mipMap, numMips);
		}

		[NativeThrows]
		public static void EvictRegion(ulong handle, Rect r, int mipMap, int numMips)
		{
			EvictRegion_Injected(handle, ref r, mipMap, numMips);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern ulong Create_Injected(ref CreationParameters p);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void RequestRegion_Injected(ulong handle, ref Rect r, int mipMap, int numMips);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void InvalidateRegion_Injected(ulong handle, ref Rect r, int mipMap, int numMips);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void EvictRegion_Injected(ulong handle, ref Rect r, int mipMap, int numMips);
	}

	[NativeHeader("Modules/VirtualTexturing/ScriptBindings/VirtualTexturing.bindings.h")]
	public struct CreationParameters
	{
		public const int MaxNumLayers = 4;

		public const int MaxRequestsPerFrameSupported = 4095;

		public int width;

		public int height;

		public int maxActiveRequests;

		public int tilesize;

		public GraphicsFormat[] layers;

		public FilterMode filterMode;

		internal int borderSize;

		internal int gpuGeneration;

		internal int flags;

		internal void Validate()
		{
			if (width <= 0 || height <= 0 || tilesize <= 0)
			{
				throw new ArgumentException($"Zero sized dimensions are invalid (width: {width}, height: {height}, tilesize {tilesize}");
			}
			if (layers == null || layers.Length > 4)
			{
				throw new ArgumentException($"layers is either invalid or has too many layers (maxNumLayers: {4})");
			}
			if (gpuGeneration == 1 && filterMode != FilterMode.Bilinear)
			{
				throw new ArgumentException("Filter mode invalid for GPU PVT; only FilterMode.Bilinear is currently supported");
			}
			if (gpuGeneration == 0 && filterMode != FilterMode.Bilinear && filterMode != FilterMode.Trilinear)
			{
				throw new ArgumentException("Filter mode invalid for CPU PVT; only FilterMode.Bilinear and FilterMode.Trilinear are currently supported");
			}
			GraphicsFormat[] array = new GraphicsFormat[22]
			{
				GraphicsFormat.R8G8B8A8_SRGB,
				GraphicsFormat.R8G8B8A8_UNorm,
				GraphicsFormat.R32G32B32A32_SFloat,
				GraphicsFormat.R8G8_SRGB,
				GraphicsFormat.R8G8_UNorm,
				GraphicsFormat.R32_SFloat,
				GraphicsFormat.RGB_DXT1_SRGB,
				GraphicsFormat.RGB_DXT1_UNorm,
				GraphicsFormat.RGBA_DXT5_SRGB,
				GraphicsFormat.RGBA_DXT5_UNorm,
				GraphicsFormat.RGBA_BC7_SRGB,
				GraphicsFormat.RGBA_BC7_UNorm,
				GraphicsFormat.RG_BC5_SNorm,
				GraphicsFormat.RG_BC5_UNorm,
				GraphicsFormat.RGB_BC6H_SFloat,
				GraphicsFormat.RGB_BC6H_UFloat,
				GraphicsFormat.R16_SFloat,
				GraphicsFormat.R16_UNorm,
				GraphicsFormat.R16G16_SFloat,
				GraphicsFormat.R16G16_UNorm,
				GraphicsFormat.R16G16B16A16_SFloat,
				GraphicsFormat.R16G16B16A16_UNorm
			};
			GraphicsFormat[] array2 = new GraphicsFormat[8]
			{
				GraphicsFormat.R8G8B8A8_SRGB,
				GraphicsFormat.R8G8B8A8_UNorm,
				GraphicsFormat.R32G32B32A32_SFloat,
				GraphicsFormat.R8G8_SRGB,
				GraphicsFormat.R8G8_UNorm,
				GraphicsFormat.R32_SFloat,
				GraphicsFormat.A2B10G10R10_UNormPack32,
				GraphicsFormat.R16_UNorm
			};
			FormatUsage usage = ((gpuGeneration == 1) ? FormatUsage.Render : FormatUsage.Sample);
			for (int i = 0; i < layers.Length; i++)
			{
				if (SystemInfo.GetCompatibleFormat(layers[i], usage) != layers[i])
				{
					throw new ArgumentException($"Requested format {layers[i]} on layer {i} is not supported on this platform");
				}
				bool flag = false;
				GraphicsFormat[] array3 = ((gpuGeneration == 1) ? array2 : array);
				for (int j = 0; j < array3.Length; j++)
				{
					if (layers[i] == array3[j])
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					string arg = ((gpuGeneration == 1) ? "GPU" : "CPU");
					throw new ArgumentException($"{arg} Procedural Virtual Texturing doesn't support GraphicsFormat {layers[i]} for stack layer {i}");
				}
			}
			if (maxActiveRequests > 4095 || maxActiveRequests <= 0)
			{
				throw new ArgumentException($"Invalid requests per frame (maxActiveRequests: ]0, {maxActiveRequests}])");
			}
		}
	}

	[NativeHeader("Modules/VirtualTexturing/ScriptBindings/VirtualTexturing.bindings.h")]
	[UsedByNativeCode]
	internal struct RequestHandlePayload : IEquatable<RequestHandlePayload>
	{
		internal int id;

		internal int lifetime;

		[NativeDisableUnsafePtrRestriction]
		internal IntPtr callback;

		public static bool operator !=(RequestHandlePayload lhs, RequestHandlePayload rhs)
		{
			return !(lhs == rhs);
		}

		public override bool Equals(object obj)
		{
			return obj is RequestHandlePayload && this == (RequestHandlePayload)obj;
		}

		public bool Equals(RequestHandlePayload other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			int num = -2128608763;
			num = num * -1521134295 + id.GetHashCode();
			num = num * -1521134295 + lifetime.GetHashCode();
			return num * -1521134295 + callback.GetHashCode();
		}

		public static bool operator ==(RequestHandlePayload lhs, RequestHandlePayload rhs)
		{
			return lhs.id == rhs.id && lhs.lifetime == rhs.lifetime && lhs.callback == rhs.callback;
		}
	}

	public struct TextureStackRequestHandle<T> : IEquatable<TextureStackRequestHandle<T>> where T : struct
	{
		internal RequestHandlePayload payload;

		public static bool operator !=(TextureStackRequestHandle<T> h1, TextureStackRequestHandle<T> h2)
		{
			return !(h1 == h2);
		}

		public override bool Equals(object obj)
		{
			return obj is TextureStackRequestHandle<T> && this == (TextureStackRequestHandle<T>)obj;
		}

		public bool Equals(TextureStackRequestHandle<T> other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return payload.GetHashCode();
		}

		public static bool operator ==(TextureStackRequestHandle<T> h1, TextureStackRequestHandle<T> h2)
		{
			return h1.payload == h2.payload;
		}

		public unsafe void CompleteRequest(RequestStatus status)
		{
			Binding.UpdateRequestState((IntPtr)UnsafeUtility.AddressOf(ref this), (IntPtr)UnsafeUtility.AddressOf(ref status), 1);
		}

		public unsafe void CompleteRequest(RequestStatus status, CommandBuffer fenceBuffer)
		{
			Binding.UpdateRequestStateWithCommandBuffer((IntPtr)UnsafeUtility.AddressOf(ref this), (IntPtr)UnsafeUtility.AddressOf(ref status), 1, fenceBuffer);
		}

		public unsafe static void CompleteRequests(NativeSlice<TextureStackRequestHandle<T>> requestHandles, NativeSlice<RequestStatus> status)
		{
			if (!System.enabled)
			{
				throw new InvalidOperationException("Virtual texturing is not enabled in the player settings.");
			}
			bool flag = true;
			if (requestHandles.Length != status.Length)
			{
				throw new ArgumentException($"Array sizes do not match ({requestHandles.Length} handles, {status.Length} requests)");
			}
			Binding.UpdateRequestState((IntPtr)requestHandles.GetUnsafePtr(), (IntPtr)status.GetUnsafePtr(), requestHandles.Length);
		}

		public unsafe static void CompleteRequests(NativeSlice<TextureStackRequestHandle<T>> requestHandles, NativeSlice<RequestStatus> status, CommandBuffer fenceBuffer)
		{
			if (!System.enabled)
			{
				throw new InvalidOperationException("Virtual texturing is not enabled in the player settings.");
			}
			bool flag = true;
			if (requestHandles.Length != status.Length)
			{
				throw new ArgumentException($"Array sizes do not match ({requestHandles.Length} handles, {status.Length} requests)");
			}
			Binding.UpdateRequestStateWithCommandBuffer((IntPtr)requestHandles.GetUnsafePtr(), (IntPtr)status.GetUnsafePtr(), requestHandles.Length, fenceBuffer);
		}

		public unsafe T GetRequestParameters()
		{
			T output = new T();
			Binding.GetRequestParameters((IntPtr)UnsafeUtility.AddressOf(ref this), (IntPtr)UnsafeUtility.AddressOf(ref output), 1);
			return output;
		}

		public unsafe static void GetRequestParameters(NativeSlice<TextureStackRequestHandle<T>> handles, NativeSlice<T> requests)
		{
			if (!System.enabled)
			{
				throw new InvalidOperationException("Virtual texturing is not enabled in the player settings.");
			}
			bool flag = true;
			if (handles.Length != requests.Length)
			{
				throw new ArgumentException($"Array sizes do not match ({handles.Length} handles, {requests.Length} requests)");
			}
			Binding.GetRequestParameters((IntPtr)handles.GetUnsafePtr(), (IntPtr)requests.GetUnsafePtr(), handles.Length);
		}
	}

	[NativeHeader("Modules/VirtualTexturing/ScriptBindings/VirtualTexturing.bindings.h")]
	[UsedByNativeCode]
	public struct GPUTextureStackRequestLayerParameters
	{
		public int destX;

		public int destY;

		public RenderTargetIdentifier dest;

		public int GetWidth()
		{
			return GetWidth_Injected(ref this);
		}

		public int GetHeight()
		{
			return GetHeight_Injected(ref this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int GetWidth_Injected(ref GPUTextureStackRequestLayerParameters _unity_self);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int GetHeight_Injected(ref GPUTextureStackRequestLayerParameters _unity_self);
	}

	[NativeHeader("Modules/VirtualTexturing/ScriptBindings/VirtualTexturing.bindings.h")]
	[UsedByNativeCode]
	public struct CPUTextureStackRequestLayerParameters
	{
		internal int _scanlineSize;

		internal int dataSize;

		[NativeDisableUnsafePtrRestriction]
		internal unsafe void* data;

		internal int _mipScanlineSize;

		internal int mipDataSize;

		[NativeDisableUnsafePtrRestriction]
		internal unsafe void* mipData;

		public int scanlineSize => _scanlineSize;

		public int mipScanlineSize => _mipScanlineSize;

		public bool requiresCachedMip => mipDataSize != 0;

		public unsafe NativeArray<T> GetData<T>() where T : struct
		{
			return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(data, dataSize, Allocator.None);
		}

		public unsafe NativeArray<T> GetMipData<T>() where T : struct
		{
			return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(mipData, mipDataSize, Allocator.None);
		}
	}

	[NativeHeader("Modules/VirtualTexturing/ScriptBindings/VirtualTexturing.bindings.h")]
	[UsedByNativeCode]
	public struct GPUTextureStackRequestParameters
	{
		public int level;

		public int x;

		public int y;

		public int width;

		public int height;

		public int numLayers;

		private GPUTextureStackRequestLayerParameters layer0;

		private GPUTextureStackRequestLayerParameters layer1;

		private GPUTextureStackRequestLayerParameters layer2;

		private GPUTextureStackRequestLayerParameters layer3;

		public GPUTextureStackRequestLayerParameters GetLayer(int index)
		{
			return index switch
			{
				0 => layer0, 
				1 => layer1, 
				2 => layer2, 
				3 => layer3, 
				_ => throw new IndexOutOfRangeException(), 
			};
		}
	}

	[NativeHeader("Modules/VirtualTexturing/ScriptBindings/VirtualTexturing.bindings.h")]
	[UsedByNativeCode]
	public struct CPUTextureStackRequestParameters
	{
		public int level;

		public int x;

		public int y;

		public int width;

		public int height;

		public int numLayers;

		private CPUTextureStackRequestLayerParameters layer0;

		private CPUTextureStackRequestLayerParameters layer1;

		private CPUTextureStackRequestLayerParameters layer2;

		private CPUTextureStackRequestLayerParameters layer3;

		public CPUTextureStackRequestLayerParameters GetLayer(int index)
		{
			return index switch
			{
				0 => layer0, 
				1 => layer1, 
				2 => layer2, 
				3 => layer3, 
				_ => throw new IndexOutOfRangeException(), 
			};
		}
	}

	[UsedByNativeCode]
	internal enum ProceduralTextureStackRequestStatus
	{
		StatusFree = 65535,
		StatusRequested,
		StatusProcessing,
		StatusComplete,
		StatusDropped
	}

	public enum RequestStatus
	{
		Dropped = 65539,
		Generated = 65538
	}

	public class TextureStackBase<T> : IDisposable where T : struct
	{
		internal ulong handle;

		public static readonly int borderSize = 8;

		private string name;

		private CreationParameters creationParams;

		public const int AllMips = int.MaxValue;

		public unsafe int PopRequests(NativeSlice<TextureStackRequestHandle<T>> requestHandles)
		{
			if (!IsValid())
			{
				throw new InvalidOperationException("Invalid ProceduralTextureStack " + name);
			}
			bool flag = false;
			return Binding.PopRequests(handle, (IntPtr)requestHandles.GetUnsafePtr(), requestHandles.Length);
		}

		public bool IsValid()
		{
			return handle != 0;
		}

		public TextureStackBase(string _name, CreationParameters _creationParams, bool gpuGeneration)
		{
			if (!System.enabled)
			{
				throw new InvalidOperationException("Virtual texturing is not enabled in the player settings.");
			}
			name = _name;
			creationParams = _creationParams;
			creationParams.borderSize = borderSize;
			creationParams.gpuGeneration = (gpuGeneration ? 1 : 0);
			creationParams.flags = 0;
			creationParams.Validate();
			handle = Binding.Create(creationParams);
		}

		public void Dispose()
		{
			if (IsValid())
			{
				Binding.Destroy(handle);
				handle = 0uL;
			}
		}

		public void BindToMaterialPropertyBlock(MaterialPropertyBlock mpb)
		{
			if (mpb == null)
			{
				throw new ArgumentNullException("mbp");
			}
			if (!IsValid())
			{
				throw new InvalidOperationException("Invalid ProceduralTextureStack " + name);
			}
			Binding.BindToMaterialPropertyBlock(handle, mpb, name);
		}

		public void BindToMaterial(Material mat)
		{
			if (mat == null)
			{
				throw new ArgumentNullException("mat");
			}
			if (!IsValid())
			{
				throw new InvalidOperationException("Invalid ProceduralTextureStack " + name);
			}
			Binding.BindToMaterial(handle, mat, name);
		}

		public void BindGlobally()
		{
			if (!IsValid())
			{
				throw new InvalidOperationException("Invalid ProceduralTextureStack " + name);
			}
			Binding.BindGlobally(handle, name);
		}

		public void RequestRegion(Rect r, int mipMap, int numMips)
		{
			if (!IsValid())
			{
				throw new InvalidOperationException("Invalid ProceduralTextureStack " + name);
			}
			Binding.RequestRegion(handle, r, mipMap, numMips);
		}

		public void InvalidateRegion(Rect r, int mipMap, int numMips)
		{
			if (!IsValid())
			{
				throw new InvalidOperationException("Invalid ProceduralTextureStack " + name);
			}
			Binding.InvalidateRegion(handle, r, mipMap, numMips);
		}

		public void EvictRegion(Rect r, int mipMap, int numMips)
		{
			if (!IsValid())
			{
				throw new InvalidOperationException("Invalid ProceduralTextureStack " + name);
			}
			Binding.EvictRegion(handle, r, mipMap, numMips);
		}
	}

	public sealed class GPUTextureStack : TextureStackBase<GPUTextureStackRequestParameters>
	{
		public GPUTextureStack(string _name, CreationParameters creationParams)
			: base(_name, creationParams, gpuGeneration: true)
		{
		}
	}

	public sealed class CPUTextureStack : TextureStackBase<CPUTextureStackRequestParameters>
	{
		public CPUTextureStack(string _name, CreationParameters creationParams)
			: base(_name, creationParams, gpuGeneration: false)
		{
		}
	}

	[NativeThrows]
	public static void SetDebugFlagInteger(Guid guid, long value)
	{
		System.SetDebugFlagInteger(guid, value);
	}

	[NativeThrows]
	public static void SetDebugFlagDouble(Guid guid, double value)
	{
		System.SetDebugFlagDouble(guid, value);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern void SetCPUCacheSize(int sizeInMegabytes);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern int GetCPUCacheSize();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern void SetGPUCacheSettings(GPUCacheSetting[] cacheSettings);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern GPUCacheSetting[] GetGPUCacheSettings();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern void SetGPUCacheStagingAreaCapacity(uint tilesPerFrame);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern uint GetGPUCacheStagingAreaCapacity();
}
