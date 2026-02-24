using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Apple;

[NativeHeader("Runtime/Export/Apple/FrameCaptureMetalScriptBindings.h")]
[NativeConditional("PLATFORM_APPLE")]
public class FrameCapture
{
	private FrameCapture()
	{
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("FrameCaptureMetalScripting::IsDestinationSupported")]
	private static extern bool IsDestinationSupportedImpl(FrameCaptureDestination dest);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("FrameCaptureMetalScripting::BeginCapture")]
	private static extern void BeginCaptureImpl(FrameCaptureDestination dest, string path);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("FrameCaptureMetalScripting::EndCapture")]
	private static extern void EndCaptureImpl();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("FrameCaptureMetalScripting::CaptureNextFrame")]
	private static extern void CaptureNextFrameImpl(FrameCaptureDestination dest, string path);

	public static bool IsDestinationSupported(FrameCaptureDestination dest)
	{
		if (dest != FrameCaptureDestination.DevTools && dest != FrameCaptureDestination.GPUTraceDocument)
		{
			throw new ArgumentException("dest", "Argument dest has bad value (not one of FrameCaptureDestination enum values)");
		}
		return IsDestinationSupportedImpl(dest);
	}

	public static void BeginCaptureToXcode()
	{
		if (!IsDestinationSupported(FrameCaptureDestination.DevTools))
		{
			throw new InvalidOperationException("Frame Capture with DevTools is not supported.");
		}
		BeginCaptureImpl(FrameCaptureDestination.DevTools, null);
	}

	public static void BeginCaptureToFile(string path)
	{
		if (!IsDestinationSupported(FrameCaptureDestination.GPUTraceDocument))
		{
			throw new InvalidOperationException("Frame Capture to file is not supported.");
		}
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentException("path", "Path must be supplied when capture destination is GPUTraceDocument.");
		}
		if (Path.GetExtension(path) != ".gputrace")
		{
			throw new ArgumentException("path", "Destination file should have .gputrace extension.");
		}
		BeginCaptureImpl(FrameCaptureDestination.GPUTraceDocument, new Uri(path).AbsoluteUri);
	}

	public static void EndCapture()
	{
		EndCaptureImpl();
	}

	public static void CaptureNextFrameToXcode()
	{
		if (!IsDestinationSupported(FrameCaptureDestination.DevTools))
		{
			throw new InvalidOperationException("Frame Capture with DevTools is not supported.");
		}
		CaptureNextFrameImpl(FrameCaptureDestination.DevTools, null);
	}

	public static void CaptureNextFrameToFile(string path)
	{
		if (!IsDestinationSupported(FrameCaptureDestination.GPUTraceDocument))
		{
			throw new InvalidOperationException("Frame Capture to file is not supported.");
		}
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentException("path", "Path must be supplied when capture destination is GPUTraceDocument.");
		}
		if (Path.GetExtension(path) != ".gputrace")
		{
			throw new ArgumentException("path", "Destination file should have .gputrace extension.");
		}
		CaptureNextFrameImpl(FrameCaptureDestination.GPUTraceDocument, new Uri(path).AbsoluteUri);
	}
}
