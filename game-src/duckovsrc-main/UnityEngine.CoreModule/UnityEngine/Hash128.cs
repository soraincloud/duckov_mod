using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[Serializable]
[NativeHeader("Runtime/Utilities/Hash128.h")]
[NativeHeader("Runtime/Export/Hashing/Hash128.bindings.h")]
[UsedByNativeCode]
public struct Hash128 : IComparable, IComparable<Hash128>, IEquatable<Hash128>
{
	internal ulong u64_0;

	internal ulong u64_1;

	private const ulong kConst = 16045690984833335023uL;

	public bool isValid => u64_0 != 0L || u64_1 != 0;

	public Hash128(uint u32_0, uint u32_1, uint u32_2, uint u32_3)
	{
		u64_0 = ((ulong)u32_1 << 32) | u32_0;
		u64_1 = ((ulong)u32_3 << 32) | u32_2;
	}

	public Hash128(ulong u64_0, ulong u64_1)
	{
		this.u64_0 = u64_0;
		this.u64_1 = u64_1;
	}

	public int CompareTo(Hash128 rhs)
	{
		if (this < rhs)
		{
			return -1;
		}
		if (this > rhs)
		{
			return 1;
		}
		return 0;
	}

	public override string ToString()
	{
		return Hash128ToStringImpl(this);
	}

	[FreeFunction("StringToHash128", IsThreadSafe = true)]
	public static Hash128 Parse(string hashString)
	{
		Parse_Injected(hashString, out var ret);
		return ret;
	}

	[FreeFunction("Hash128ToString", IsThreadSafe = true)]
	private static string Hash128ToStringImpl(Hash128 hash)
	{
		return Hash128ToStringImpl_Injected(ref hash);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ComputeHash128FromScriptString", IsThreadSafe = true)]
	private static extern void ComputeFromString(string data, ref Hash128 hash);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ComputeHash128FromScriptPointer", IsThreadSafe = true)]
	private static extern void ComputeFromPtr(IntPtr data, int start, int count, int elemSize, ref Hash128 hash);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ComputeHash128FromScriptArray", IsThreadSafe = true)]
	private static extern void ComputeFromArray(Array data, int start, int count, int elemSize, ref Hash128 hash);

	public static Hash128 Compute(string data)
	{
		Hash128 hash = default(Hash128);
		ComputeFromString(data, ref hash);
		return hash;
	}

	public unsafe static Hash128 Compute<T>(NativeArray<T> data) where T : struct
	{
		Hash128 hash = default(Hash128);
		ComputeFromPtr((IntPtr)data.GetUnsafeReadOnlyPtr(), 0, data.Length, UnsafeUtility.SizeOf<T>(), ref hash);
		return hash;
	}

	public unsafe static Hash128 Compute<T>(NativeArray<T> data, int start, int count) where T : struct
	{
		if (start < 0 || count < 0 || start + count > data.Length)
		{
			throw new ArgumentOutOfRangeException($"Bad start/count arguments (start:{start} count:{count})");
		}
		Hash128 hash = default(Hash128);
		ComputeFromPtr((IntPtr)data.GetUnsafeReadOnlyPtr(), start, count, UnsafeUtility.SizeOf<T>(), ref hash);
		return hash;
	}

	public static Hash128 Compute<T>(T[] data) where T : struct
	{
		if (!UnsafeUtility.IsArrayBlittable(data))
		{
			throw new ArgumentException("Array passed to Compute must be blittable.\n" + UnsafeUtility.GetReasonForArrayNonBlittable(data));
		}
		Hash128 hash = default(Hash128);
		ComputeFromArray(data, 0, data.Length, UnsafeUtility.SizeOf<T>(), ref hash);
		return hash;
	}

	public static Hash128 Compute<T>(T[] data, int start, int count) where T : struct
	{
		if (!UnsafeUtility.IsArrayBlittable(data))
		{
			throw new ArgumentException("Array passed to Compute must be blittable.\n" + UnsafeUtility.GetReasonForArrayNonBlittable(data));
		}
		if (start < 0 || count < 0 || start + count > data.Length)
		{
			throw new ArgumentOutOfRangeException($"Bad start/count arguments (start:{start} count:{count})");
		}
		Hash128 hash = default(Hash128);
		ComputeFromArray(data, start, count, UnsafeUtility.SizeOf<T>(), ref hash);
		return hash;
	}

	public static Hash128 Compute<T>(List<T> data) where T : struct
	{
		if (!UnsafeUtility.IsGenericListBlittable<T>())
		{
			throw new ArgumentException(string.Format("List<{0}> passed to {1} must be blittable.\n{2}", typeof(T), "Compute", UnsafeUtility.GetReasonForGenericListNonBlittable<T>()));
		}
		Hash128 hash = default(Hash128);
		ComputeFromArray(NoAllocHelpers.ExtractArrayFromList(data), 0, data.Count, UnsafeUtility.SizeOf<T>(), ref hash);
		return hash;
	}

	public static Hash128 Compute<T>(List<T> data, int start, int count) where T : struct
	{
		if (!UnsafeUtility.IsGenericListBlittable<T>())
		{
			throw new ArgumentException(string.Format("List<{0}> passed to {1} must be blittable.\n{2}", typeof(T), "Compute", UnsafeUtility.GetReasonForGenericListNonBlittable<T>()));
		}
		if (start < 0 || count < 0 || start + count > data.Count)
		{
			throw new ArgumentOutOfRangeException($"Bad start/count arguments (start:{start} count:{count})");
		}
		Hash128 hash = default(Hash128);
		ComputeFromArray(NoAllocHelpers.ExtractArrayFromList(data), start, count, UnsafeUtility.SizeOf<T>(), ref hash);
		return hash;
	}

	public unsafe static Hash128 Compute<T>(ref T val) where T : unmanaged
	{
		fixed (T* ptr = &val)
		{
			void* ptr2 = ptr;
			Hash128 hash = default(Hash128);
			ComputeFromPtr((IntPtr)ptr2, 0, 1, UnsafeUtility.SizeOf<T>(), ref hash);
			return hash;
		}
	}

	public static Hash128 Compute(int val)
	{
		Hash128 result = default(Hash128);
		result.Append(val);
		return result;
	}

	public static Hash128 Compute(float val)
	{
		Hash128 result = default(Hash128);
		result.Append(val);
		return result;
	}

	public unsafe static Hash128 Compute(void* data, ulong size)
	{
		Hash128 hash = default(Hash128);
		ComputeFromPtr(new IntPtr(data), 0, (int)size, 1, ref hash);
		return hash;
	}

	public void Append(string data)
	{
		ComputeFromString(data, ref this);
	}

	public unsafe void Append<T>(NativeArray<T> data) where T : struct
	{
		ComputeFromPtr((IntPtr)data.GetUnsafeReadOnlyPtr(), 0, data.Length, UnsafeUtility.SizeOf<T>(), ref this);
	}

	public unsafe void Append<T>(NativeArray<T> data, int start, int count) where T : struct
	{
		if (start < 0 || count < 0 || start + count > data.Length)
		{
			throw new ArgumentOutOfRangeException($"Bad start/count arguments (start:{start} count:{count})");
		}
		ComputeFromPtr((IntPtr)data.GetUnsafeReadOnlyPtr(), start, count, UnsafeUtility.SizeOf<T>(), ref this);
	}

	public void Append<T>(T[] data) where T : struct
	{
		if (!UnsafeUtility.IsArrayBlittable(data))
		{
			throw new ArgumentException("Array passed to Append must be blittable.\n" + UnsafeUtility.GetReasonForArrayNonBlittable(data));
		}
		ComputeFromArray(data, 0, data.Length, UnsafeUtility.SizeOf<T>(), ref this);
	}

	public void Append<T>(T[] data, int start, int count) where T : struct
	{
		if (!UnsafeUtility.IsArrayBlittable(data))
		{
			throw new ArgumentException("Array passed to Append must be blittable.\n" + UnsafeUtility.GetReasonForArrayNonBlittable(data));
		}
		if (start < 0 || count < 0 || start + count > data.Length)
		{
			throw new ArgumentOutOfRangeException($"Bad start/count arguments (start:{start} count:{count})");
		}
		ComputeFromArray(data, start, count, UnsafeUtility.SizeOf<T>(), ref this);
	}

	public void Append<T>(List<T> data) where T : struct
	{
		if (!UnsafeUtility.IsGenericListBlittable<T>())
		{
			throw new ArgumentException(string.Format("List<{0}> passed to {1} must be blittable.\n{2}", typeof(T), "Append", UnsafeUtility.GetReasonForGenericListNonBlittable<T>()));
		}
		ComputeFromArray(NoAllocHelpers.ExtractArrayFromList(data), 0, data.Count, UnsafeUtility.SizeOf<T>(), ref this);
	}

	public void Append<T>(List<T> data, int start, int count) where T : struct
	{
		if (!UnsafeUtility.IsGenericListBlittable<T>())
		{
			throw new ArgumentException(string.Format("List<{0}> passed to {1} must be blittable.\n{2}", typeof(T), "Append", UnsafeUtility.GetReasonForGenericListNonBlittable<T>()));
		}
		if (start < 0 || count < 0 || start + count > data.Count)
		{
			throw new ArgumentOutOfRangeException($"Bad start/count arguments (start:{start} count:{count})");
		}
		ComputeFromArray(NoAllocHelpers.ExtractArrayFromList(data), start, count, UnsafeUtility.SizeOf<T>(), ref this);
	}

	public unsafe void Append<T>(ref T val) where T : unmanaged
	{
		fixed (T* ptr = &val)
		{
			void* ptr2 = ptr;
			ComputeFromPtr((IntPtr)ptr2, 0, 1, UnsafeUtility.SizeOf<T>(), ref this);
		}
	}

	public void Append(int val)
	{
		ShortHash4((uint)val);
	}

	public unsafe void Append(float val)
	{
		ShortHash4(*(uint*)(&val));
	}

	public unsafe void Append(void* data, ulong size)
	{
		ComputeFromPtr(new IntPtr(data), 0, (int)size, 1, ref this);
	}

	public override bool Equals(object obj)
	{
		return obj is Hash128 && this == (Hash128)obj;
	}

	public bool Equals(Hash128 obj)
	{
		return this == obj;
	}

	public override int GetHashCode()
	{
		return u64_0.GetHashCode() ^ u64_1.GetHashCode();
	}

	public int CompareTo(object obj)
	{
		if (obj == null || !(obj is Hash128))
		{
			return 1;
		}
		Hash128 rhs = (Hash128)obj;
		return CompareTo(rhs);
	}

	public static bool operator ==(Hash128 hash1, Hash128 hash2)
	{
		return hash1.u64_0 == hash2.u64_0 && hash1.u64_1 == hash2.u64_1;
	}

	public static bool operator !=(Hash128 hash1, Hash128 hash2)
	{
		return !(hash1 == hash2);
	}

	public static bool operator <(Hash128 x, Hash128 y)
	{
		if (x.u64_0 != y.u64_0)
		{
			return x.u64_0 < y.u64_0;
		}
		return x.u64_1 < y.u64_1;
	}

	public static bool operator >(Hash128 x, Hash128 y)
	{
		if (x < y)
		{
			return false;
		}
		if (x == y)
		{
			return false;
		}
		return true;
	}

	private void ShortHash4(uint data)
	{
		ulong h = u64_0;
		ulong h2 = u64_1;
		ulong num = 16045690984833335023uL;
		ulong num2 = 16045690984833335023uL;
		num2 += 288230376151711744L;
		num += data;
		ShortEnd(ref h, ref h2, ref num, ref num2);
		u64_0 = h;
		u64_1 = h2;
	}

	private static void ShortEnd(ref ulong h0, ref ulong h1, ref ulong h2, ref ulong h3)
	{
		h3 ^= h2;
		Rot64(ref h2, 15);
		h3 += h2;
		h0 ^= h3;
		Rot64(ref h3, 52);
		h0 += h3;
		h1 ^= h0;
		Rot64(ref h0, 26);
		h1 += h0;
		h2 ^= h1;
		Rot64(ref h1, 51);
		h2 += h1;
		h3 ^= h2;
		Rot64(ref h2, 28);
		h3 += h2;
		h0 ^= h3;
		Rot64(ref h3, 9);
		h0 += h3;
		h1 ^= h0;
		Rot64(ref h0, 47);
		h1 += h0;
		h2 ^= h1;
		Rot64(ref h1, 54);
		h2 += h1;
		h3 ^= h2;
		Rot64(ref h2, 32);
		h3 += h2;
		h0 ^= h3;
		Rot64(ref h3, 25);
		h0 += h3;
		h1 ^= h0;
		Rot64(ref h0, 63);
		h1 += h0;
	}

	private static void Rot64(ref ulong x, int k)
	{
		x = (x << k) | (x >> 64 - k);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Parse_Injected(string hashString, out Hash128 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string Hash128ToStringImpl_Injected(ref Hash128 hash);
}
