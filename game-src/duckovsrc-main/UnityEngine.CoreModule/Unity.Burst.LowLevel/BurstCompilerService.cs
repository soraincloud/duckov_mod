using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Bindings;

namespace Unity.Burst.LowLevel;

[NativeHeader("Runtime/Burst/BurstDelegateCache.h")]
[StaticAccessor("BurstCompilerService::Get()", StaticAccessorType.Arrow)]
[NativeHeader("Runtime/Burst/Burst.h")]
internal static class BurstCompilerService
{
	public delegate bool ExtractCompilerFlags(Type jobType, out string flags);

	public enum BurstLogType
	{
		Info,
		Warning,
		Error
	}

	public static extern bool IsInitialized
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("Initialize")]
	private static extern string InitializeInternal(string path, ExtractCompilerFlags extractCompilerFlags);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ThreadSafe]
	public static extern string GetDisassembly(MethodInfo m, string compilerOptions);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern int CompileAsyncDelegateMethod(object delegateMethod, string compilerOptions);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern void* GetAsyncCompiledAsyncDelegateMethod(int userID);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ThreadSafe]
	public unsafe static extern void* GetOrCreateSharedMemory(ref Hash128 key, uint size_of, uint alignment);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ThreadSafe]
	public static extern string GetMethodSignature(MethodInfo method);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ThreadSafe]
	public static extern void SetCurrentExecutionMode(uint environment);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ThreadSafe]
	public static extern uint GetCurrentExecutionMode();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("DefaultBurstLogCallback", true)]
	public unsafe static extern void Log(void* userData, BurstLogType logType, byte* message, byte* filename, int lineNumber);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("DefaultBurstRuntimeLogCallback", true)]
	public unsafe static extern void RuntimeLog(void* userData, BurstLogType logType, byte* message, byte* filename, int lineNumber);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern bool LoadBurstLibrary(string fullPathToLibBurstGenerated);

	public static void Initialize(string folderRuntime, ExtractCompilerFlags extractCompilerFlags)
	{
		if (folderRuntime == null)
		{
			throw new ArgumentNullException("folderRuntime");
		}
		if (extractCompilerFlags == null)
		{
			throw new ArgumentNullException("extractCompilerFlags");
		}
		if (!Directory.Exists(folderRuntime))
		{
			Debug.LogError("Unable to initialize the burst JIT compiler. The folder `" + folderRuntime + "` does not exist");
			return;
		}
		string text = InitializeInternal(folderRuntime, extractCompilerFlags);
		if (!string.IsNullOrEmpty(text))
		{
			Debug.LogError("Unexpected error while trying to initialize the burst JIT compiler: " + text);
		}
	}
}
