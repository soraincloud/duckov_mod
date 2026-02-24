using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Bindings;

namespace UnityEngine;

[NativeHeader("Modules/Animation/HumanPoseHandler.h")]
[NativeHeader("Modules/Animation/ScriptBindings/Animation.bindings.h")]
public class HumanPoseHandler : IDisposable
{
	internal IntPtr m_Ptr;

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("AnimationBindings::CreateHumanPoseHandler")]
	private static extern IntPtr Internal_CreateFromRoot(Avatar avatar, Transform root);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("AnimationBindings::CreateHumanPoseHandler", IsThreadSafe = true)]
	private static extern IntPtr Internal_CreateFromJointPaths(Avatar avatar, string[] jointPaths);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("AnimationBindings::DestroyHumanPoseHandler")]
	private static extern void Internal_Destroy(IntPtr ptr);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetHumanPose(out Vector3 bodyPosition, out Quaternion bodyRotation, [Out] float[] muscles);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetHumanPose(ref Vector3 bodyPosition, ref Quaternion bodyRotation, float[] muscles);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ThreadSafe]
	private extern void GetInternalHumanPose(out Vector3 bodyPosition, out Quaternion bodyRotation, [Out] float[] muscles);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ThreadSafe]
	private extern void SetInternalHumanPose(ref Vector3 bodyPosition, ref Quaternion bodyRotation, float[] muscles);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ThreadSafe]
	private unsafe extern void GetInternalAvatarPose(void* avatarPose, int avatarPoseLength);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ThreadSafe]
	private unsafe extern void SetInternalAvatarPose(void* avatarPose, int avatarPoseLength);

	public void Dispose()
	{
		if (m_Ptr != IntPtr.Zero)
		{
			Internal_Destroy(m_Ptr);
			m_Ptr = IntPtr.Zero;
		}
		GC.SuppressFinalize(this);
	}

	public HumanPoseHandler(Avatar avatar, Transform root)
	{
		m_Ptr = IntPtr.Zero;
		if (root == null)
		{
			throw new ArgumentNullException("HumanPoseHandler root Transform is null");
		}
		if (avatar == null)
		{
			throw new ArgumentNullException("HumanPoseHandler avatar is null");
		}
		if (!avatar.isValid)
		{
			throw new ArgumentException("HumanPoseHandler avatar is invalid");
		}
		if (!avatar.isHuman)
		{
			throw new ArgumentException("HumanPoseHandler avatar is not human");
		}
		m_Ptr = Internal_CreateFromRoot(avatar, root);
	}

	public HumanPoseHandler(Avatar avatar, string[] jointPaths)
	{
		m_Ptr = IntPtr.Zero;
		if (jointPaths == null)
		{
			throw new ArgumentNullException("HumanPoseHandler jointPaths array is null");
		}
		if (avatar == null)
		{
			throw new ArgumentNullException("HumanPoseHandler avatar is null");
		}
		if (!avatar.isValid)
		{
			throw new ArgumentException("HumanPoseHandler avatar is invalid");
		}
		if (!avatar.isHuman)
		{
			throw new ArgumentException("HumanPoseHandler avatar is not human");
		}
		m_Ptr = Internal_CreateFromJointPaths(avatar, jointPaths);
	}

	public void GetHumanPose(ref HumanPose humanPose)
	{
		if (m_Ptr == IntPtr.Zero)
		{
			throw new NullReferenceException("HumanPoseHandler is not initialized properly");
		}
		humanPose.Init();
		GetHumanPose(out humanPose.bodyPosition, out humanPose.bodyRotation, humanPose.muscles);
	}

	public void SetHumanPose(ref HumanPose humanPose)
	{
		if (m_Ptr == IntPtr.Zero)
		{
			throw new NullReferenceException("HumanPoseHandler is not initialized properly");
		}
		humanPose.Init();
		SetHumanPose(ref humanPose.bodyPosition, ref humanPose.bodyRotation, humanPose.muscles);
	}

	public void GetInternalHumanPose(ref HumanPose humanPose)
	{
		if (m_Ptr == IntPtr.Zero)
		{
			throw new NullReferenceException("HumanPoseHandler is not initialized properly");
		}
		humanPose.Init();
		GetInternalHumanPose(out humanPose.bodyPosition, out humanPose.bodyRotation, humanPose.muscles);
	}

	public void SetInternalHumanPose(ref HumanPose humanPose)
	{
		if (m_Ptr == IntPtr.Zero)
		{
			throw new NullReferenceException("HumanPoseHandler is not initialized properly");
		}
		humanPose.Init();
		SetInternalHumanPose(ref humanPose.bodyPosition, ref humanPose.bodyRotation, humanPose.muscles);
	}

	public unsafe void GetInternalAvatarPose(NativeArray<float> avatarPose)
	{
		if (m_Ptr == IntPtr.Zero)
		{
			throw new NullReferenceException("HumanPoseHandler is not initialized properly");
		}
		GetInternalAvatarPose(avatarPose.GetUnsafePtr(), avatarPose.Length);
	}

	public unsafe void SetInternalAvatarPose(NativeArray<float> avatarPose)
	{
		if (m_Ptr == IntPtr.Zero)
		{
			throw new NullReferenceException("HumanPoseHandler is not initialized properly");
		}
		SetInternalAvatarPose(avatarPose.GetUnsafeReadOnlyPtr(), avatarPose.Length);
	}
}
