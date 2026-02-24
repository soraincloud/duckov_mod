using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;

namespace UnityEngine.Rendering.VirtualTexturing;

[StructLayout(LayoutKind.Sequential)]
[NativeHeader("Modules/VirtualTexturing/Public/VirtualTextureResolver.h")]
public class Resolver : IDisposable
{
	internal IntPtr m_Ptr;

	public int CurrentWidth { get; private set; } = 0;

	public int CurrentHeight { get; private set; } = 0;

	public Resolver()
	{
		if (!System.enabled)
		{
			throw new InvalidOperationException("Virtual texturing is not enabled in the player settings.");
		}
		m_Ptr = InitNative();
	}

	~Resolver()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_Ptr != IntPtr.Zero)
		{
			Flush_Internal();
			ReleaseNative(m_Ptr);
			m_Ptr = IntPtr.Zero;
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr InitNative();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	private static extern void ReleaseNative(IntPtr ptr);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Flush_Internal();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Init_Internal(int width, int height);

	public void UpdateSize(int width, int height)
	{
		if (CurrentWidth != width || CurrentHeight != height)
		{
			if (width <= 0 || height <= 0)
			{
				throw new ArgumentException($"Zero sized dimensions are invalid (width: {width}, height: {height}.");
			}
			CurrentWidth = width;
			CurrentHeight = height;
			Flush_Internal();
			Init_Internal(CurrentWidth, CurrentHeight);
		}
	}

	public void Process(CommandBuffer cmd, RenderTargetIdentifier rt)
	{
		Process(cmd, rt, 0, CurrentWidth, 0, CurrentHeight, 0, 0);
	}

	public void Process(CommandBuffer cmd, RenderTargetIdentifier rt, int x, int width, int y, int height, int mip, int slice)
	{
		if (cmd == null)
		{
			throw new ArgumentNullException("cmd");
		}
		cmd.ProcessVTFeedback(rt, m_Ptr, slice, x, width, y, height, mip);
	}
}
