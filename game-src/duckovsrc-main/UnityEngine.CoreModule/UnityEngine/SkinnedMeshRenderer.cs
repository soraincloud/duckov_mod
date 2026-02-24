using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[RequiredByNativeCode]
[NativeHeader("Runtime/Graphics/Mesh/SkinnedMeshRenderer.h")]
public class SkinnedMeshRenderer : Renderer
{
	public extern SkinQuality quality
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool updateWhenOffscreen
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool forceMatrixRecalculationPerRender
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern Transform rootBone
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern Transform[] bones
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("Mesh")]
	public extern Mesh sharedMesh
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("SkinnedMeshMotionVectors")]
	public extern bool skinnedMotionVectors
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern GraphicsBuffer.Target vertexBufferTarget
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern float GetBlendShapeWeight(int index);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetBlendShapeWeight(int index, float value);

	public void BakeMesh(Mesh mesh)
	{
		BakeMesh(mesh, useScale: false);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void BakeMesh([NotNull("NullExceptionObject")] Mesh mesh, bool useScale);

	public GraphicsBuffer GetVertexBuffer()
	{
		if (this == null)
		{
			throw new NullReferenceException();
		}
		return GetVertexBufferImpl();
	}

	public GraphicsBuffer GetPreviousVertexBuffer()
	{
		if (this == null)
		{
			throw new NullReferenceException();
		}
		return GetPreviousVertexBufferImpl();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "SkinnedMeshRendererScripting::GetVertexBufferPtr", HasExplicitThis = true)]
	private extern GraphicsBuffer GetVertexBufferImpl();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "SkinnedMeshRendererScripting::GetPreviousVertexBufferPtr", HasExplicitThis = true)]
	private extern GraphicsBuffer GetPreviousVertexBufferImpl();
}
