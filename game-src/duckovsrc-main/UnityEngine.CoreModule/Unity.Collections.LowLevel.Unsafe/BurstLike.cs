using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.LowLevel;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace Unity.Collections.LowLevel.Unsafe;

[NativeHeader("Runtime/Export/BurstLike/BurstLike.bindings.h")]
[StaticAccessor("BurstLike", StaticAccessorType.DoubleColon)]
internal static class BurstLike
{
	internal readonly struct SharedStatic<T> where T : unmanaged
	{
		private unsafe readonly void* _buffer;

		public unsafe ref T Data => ref UnsafeUtility.AsRef<T>(_buffer);

		public unsafe void* UnsafeDataPointer => _buffer;

		private unsafe SharedStatic(void* buffer)
		{
			_buffer = buffer;
		}

		public unsafe static SharedStatic<T> GetOrCreate<TContext>(uint alignment = 0u)
		{
			return new SharedStatic<T>(SharedStatic.GetOrCreateSharedStaticInternal(BurstRuntime.GetHashCode64<TContext>(), 0L, (uint)UnsafeUtility.SizeOf<T>(), alignment));
		}

		public unsafe static SharedStatic<T> GetOrCreate<TContext, TSubContext>(uint alignment = 0u)
		{
			return new SharedStatic<T>(SharedStatic.GetOrCreateSharedStaticInternal(BurstRuntime.GetHashCode64<TContext>(), BurstRuntime.GetHashCode64<TSubContext>(), (uint)UnsafeUtility.SizeOf<T>(), alignment));
		}
	}

	internal static class SharedStatic
	{
		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		private static void CheckSizeOf(uint sizeOf)
		{
			if (sizeOf == 0)
			{
				throw new ArgumentException("sizeOf must be > 0", "sizeOf");
			}
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		private unsafe static void CheckResult(void* result)
		{
			if (result == null)
			{
				throw new InvalidOperationException("Unable to create a SharedStatic for this key. This is most likely due to the size of the struct inside of the SharedStatic having changed or the same key being reused for differently sized values. To fix this the editor needs to be restarted.");
			}
		}

		[RequiredMember]
		public unsafe static void* GetOrCreateSharedStaticInternal(long getHashCode64, long getSubHashCode64, uint sizeOf, uint alignment)
		{
			Hash128 key = new Hash128((ulong)getHashCode64, (ulong)getSubHashCode64);
			return BurstCompilerService.GetOrCreateSharedMemory(ref key, sizeOf, (alignment == 0) ? 4u : alignment);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[BurstAuthorizedExternalMethod]
	[ThreadSafe(ThrowsException = false)]
	internal static extern int NativeFunctionCall_Int_IntPtr_IntPtr(IntPtr function, IntPtr p0, IntPtr p1, out int error);
}
