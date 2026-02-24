using Unity.Collections;
using UnityEngine;

namespace Unity.Profiling;

public struct DebugScreenCapture
{
	public NativeArray<byte> RawImageDataReference { get; set; }

	public TextureFormat ImageFormat { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }
}
