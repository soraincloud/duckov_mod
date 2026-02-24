using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Internal;

namespace UnityEngine;

[NativeHeader("Runtime/Video/ScriptBindings/WebCamTexture.bindings.h")]
[NativeHeader("Runtime/Video/BaseWebCamTexture.h")]
[NativeHeader("AudioScriptingClasses.h")]
public sealed class WebCamTexture : Texture
{
	public static extern WebCamDevice[] devices
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[StaticAccessor("WebCamTextureBindings", StaticAccessorType.DoubleColon)]
		[NativeName("Internal_GetDevices")]
		get;
	}

	public extern bool isPlaying
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("IsPlaying")]
		get;
	}

	[NativeName("Device")]
	public extern string deviceName
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float requestedFPS
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int requestedWidth
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int requestedHeight
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int videoRotationAngle
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern bool videoVerticallyMirrored
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("IsVideoVerticallyMirrored")]
		get;
	}

	public extern bool didUpdateThisFrame
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("DidUpdateThisFrame")]
		get;
	}

	public Vector2? autoFocusPoint
	{
		get
		{
			return (internalAutoFocusPoint.x < 0f) ? ((Vector2?)null) : new Vector2?(internalAutoFocusPoint);
		}
		set
		{
			internalAutoFocusPoint = ((!value.HasValue) ? new Vector2(-1f, -1f) : value.Value);
		}
	}

	internal Vector2 internalAutoFocusPoint
	{
		get
		{
			get_internalAutoFocusPoint_Injected(out var ret);
			return ret;
		}
		set
		{
			set_internalAutoFocusPoint_Injected(ref value);
		}
	}

	public extern bool isDepth
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public WebCamTexture(string deviceName, int requestedWidth, int requestedHeight, int requestedFPS)
	{
		Internal_CreateWebCamTexture(this, deviceName, requestedWidth, requestedHeight, requestedFPS);
	}

	public WebCamTexture(string deviceName, int requestedWidth, int requestedHeight)
	{
		Internal_CreateWebCamTexture(this, deviceName, requestedWidth, requestedHeight, 0);
	}

	public WebCamTexture(string deviceName)
	{
		Internal_CreateWebCamTexture(this, deviceName, 0, 0, 0);
	}

	public WebCamTexture(int requestedWidth, int requestedHeight, int requestedFPS)
	{
		Internal_CreateWebCamTexture(this, "", requestedWidth, requestedHeight, requestedFPS);
	}

	public WebCamTexture(int requestedWidth, int requestedHeight)
	{
		Internal_CreateWebCamTexture(this, "", requestedWidth, requestedHeight, 0);
	}

	public WebCamTexture()
	{
		Internal_CreateWebCamTexture(this, "", 0, 0, 0);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void Play();

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void Pause();

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void Stop();

	[FreeFunction("WebCamTextureBindings::Internal_GetPixel", HasExplicitThis = true)]
	public Color GetPixel(int x, int y)
	{
		GetPixel_Injected(x, y, out var ret);
		return ret;
	}

	public Color[] GetPixels()
	{
		return GetPixels(0, 0, width, height);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("WebCamTextureBindings::Internal_GetPixels", HasExplicitThis = true, ThrowsException = true)]
	public extern Color[] GetPixels(int x, int y, int blockWidth, int blockHeight);

	[ExcludeFromDocs]
	public Color32[] GetPixels32()
	{
		return GetPixels32(null);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("WebCamTextureBindings::Internal_GetPixels32", HasExplicitThis = true, ThrowsException = true)]
	public extern Color32[] GetPixels32([Unmarshalled][DefaultValue("null")] Color32[] colors);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("WebCamTextureBindings", StaticAccessorType.DoubleColon)]
	private static extern void Internal_CreateWebCamTexture([Writable] WebCamTexture self, string scriptingDevice, int requestedWidth, int requestedHeight, int maxFramerate);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetPixel_Injected(int x, int y, out Color ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_internalAutoFocusPoint_Injected(out Vector2 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_internalAutoFocusPoint_Injected(ref Vector2 value);
}
