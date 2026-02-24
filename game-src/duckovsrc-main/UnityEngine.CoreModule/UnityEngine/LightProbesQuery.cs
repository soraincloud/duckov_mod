using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.Bindings;
using UnityEngine.Rendering;

namespace UnityEngine;

[NativeHeader("Runtime/Camera/RenderLoops/LightProbeContext.h")]
[NativeContainer]
[StaticAccessor("LightProbeContextWrapper", StaticAccessorType.DoubleColon)]
public struct LightProbesQuery : IDisposable
{
	[NativeContainer]
	internal struct LightProbesQueryDispose
	{
		[NativeDisableUnsafePtrRestriction]
		internal IntPtr m_LightProbeContextWrapper;

		public void Dispose()
		{
			UnsafeUtility.LeakErase(m_LightProbeContextWrapper, LeakCategory.LightProbesQuery);
			Destroy(m_LightProbeContextWrapper);
		}
	}

	internal struct LightProbesQueryDisposeJob : IJob
	{
		internal LightProbesQueryDispose Data;

		public void Execute()
		{
			Data.Dispose();
		}
	}

	[NativeDisableUnsafePtrRestriction]
	internal IntPtr m_LightProbeContextWrapper;

	internal Allocator m_AllocatorLabel;

	public bool IsCreated => m_LightProbeContextWrapper != IntPtr.Zero;

	public LightProbesQuery(Allocator allocator)
	{
		m_LightProbeContextWrapper = Create();
		m_AllocatorLabel = allocator;
		UnsafeUtility.LeakRecord(m_LightProbeContextWrapper, LeakCategory.LightProbesQuery, 0);
	}

	public void Dispose()
	{
		if (m_LightProbeContextWrapper == IntPtr.Zero)
		{
			throw new ObjectDisposedException("The LightProbesQuery is already disposed.");
		}
		if (m_AllocatorLabel == Allocator.Invalid)
		{
			throw new InvalidOperationException("The LightProbesQuery can not be Disposed because it was not allocated with a valid allocator.");
		}
		if (m_AllocatorLabel > Allocator.None)
		{
			UnsafeUtility.LeakErase(m_LightProbeContextWrapper, LeakCategory.LightProbesQuery);
			Destroy(m_LightProbeContextWrapper);
			m_AllocatorLabel = Allocator.Invalid;
		}
		m_LightProbeContextWrapper = IntPtr.Zero;
	}

	public JobHandle Dispose(JobHandle inputDeps)
	{
		if (m_AllocatorLabel == Allocator.Invalid)
		{
			throw new InvalidOperationException("The LightProbesQuery can not be Disposed because it was not allocated with a valid allocator.");
		}
		if (m_LightProbeContextWrapper == IntPtr.Zero)
		{
			throw new InvalidOperationException("The LightProbesQuery is already disposed.");
		}
		if (m_AllocatorLabel > Allocator.None)
		{
			JobHandle result = new LightProbesQueryDisposeJob
			{
				Data = new LightProbesQueryDispose
				{
					m_LightProbeContextWrapper = m_LightProbeContextWrapper
				}
			}.Schedule(inputDeps);
			m_AllocatorLabel = Allocator.Invalid;
			m_LightProbeContextWrapper = IntPtr.Zero;
			return result;
		}
		m_LightProbeContextWrapper = IntPtr.Zero;
		return inputDeps;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr Create();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ThreadSafe]
	private static extern void Destroy(IntPtr lightProbeContextWrapper);

	public void CalculateInterpolatedLightAndOcclusionProbe(Vector3 position, ref int tetrahedronIndex, out SphericalHarmonicsL2 lightProbe, out Vector4 occlusionProbe)
	{
		CalculateInterpolatedLightAndOcclusionProbe(m_LightProbeContextWrapper, position, ref tetrahedronIndex, out lightProbe, out occlusionProbe);
	}

	public unsafe void CalculateInterpolatedLightAndOcclusionProbes(NativeArray<Vector3> positions, NativeArray<int> tetrahedronIndices, NativeArray<SphericalHarmonicsL2> lightProbes, NativeArray<Vector4> occlusionProbes)
	{
		bool flag = false;
		if (tetrahedronIndices.Length < positions.Length)
		{
			throw new ArgumentException("tetrahedronIndices", "Argument tetrahedronIndices is null or has fewer elements than positions.");
		}
		if (lightProbes.Length < positions.Length)
		{
			throw new ArgumentException("lightProbes", "Argument lightProbes is null or has fewer elements than positions.");
		}
		if (occlusionProbes.Length < positions.Length)
		{
			throw new ArgumentException("occlusionProbes", "Argument occlusionProbes is null or has fewer elements than positions.");
		}
		CalculateInterpolatedLightAndOcclusionProbes(m_LightProbeContextWrapper, (IntPtr)positions.GetUnsafeReadOnlyPtr(), (IntPtr)tetrahedronIndices.GetUnsafeReadOnlyPtr(), (IntPtr)lightProbes.GetUnsafePtr(), (IntPtr)occlusionProbes.GetUnsafePtr(), positions.Length);
	}

	[ThreadSafe]
	private static void CalculateInterpolatedLightAndOcclusionProbe(IntPtr lightProbeContextWrapper, Vector3 position, ref int tetrahedronIndex, out SphericalHarmonicsL2 lightProbe, out Vector4 occlusionProbe)
	{
		CalculateInterpolatedLightAndOcclusionProbe_Injected(lightProbeContextWrapper, ref position, ref tetrahedronIndex, out lightProbe, out occlusionProbe);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ThreadSafe]
	private static extern void CalculateInterpolatedLightAndOcclusionProbes(IntPtr lightProbeContextWrapper, IntPtr positions, IntPtr tetrahedronIndices, IntPtr lightProbes, IntPtr occlusionProbes, int count);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void CalculateInterpolatedLightAndOcclusionProbe_Injected(IntPtr lightProbeContextWrapper, ref Vector3 position, ref int tetrahedronIndex, out SphericalHarmonicsL2 lightProbe, out Vector4 occlusionProbe);
}
