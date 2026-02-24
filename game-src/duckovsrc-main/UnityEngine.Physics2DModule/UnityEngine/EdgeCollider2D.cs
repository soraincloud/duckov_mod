using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine;

[NativeHeader("Modules/Physics2D/Public/EdgeCollider2D.h")]
public sealed class EdgeCollider2D : Collider2D
{
	public extern float edgeRadius
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int edgeCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern int pointCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern Vector2[] points
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool useAdjacentStartPoint
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool useAdjacentEndPoint
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public Vector2 adjacentStartPoint
	{
		get
		{
			get_adjacentStartPoint_Injected(out var ret);
			return ret;
		}
		set
		{
			set_adjacentStartPoint_Injected(ref value);
		}
	}

	public Vector2 adjacentEndPoint
	{
		get
		{
			get_adjacentEndPoint_Injected(out var ret);
			return ret;
		}
		set
		{
			set_adjacentEndPoint_Injected(ref value);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void Reset();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("GetPoints_Binding")]
	public extern int GetPoints([NotNull("ArgumentNullException")] List<Vector2> points);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("SetPoints_Binding")]
	public extern bool SetPoints([NotNull("ArgumentNullException")] List<Vector2> points);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_adjacentStartPoint_Injected(out Vector2 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_adjacentStartPoint_Injected(ref Vector2 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_adjacentEndPoint_Injected(out Vector2 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_adjacentEndPoint_Injected(ref Vector2 value);
}
