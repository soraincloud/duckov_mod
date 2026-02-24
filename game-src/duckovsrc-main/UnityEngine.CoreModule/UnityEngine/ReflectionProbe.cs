using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Internal;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace UnityEngine;

[NativeHeader("Runtime/Camera/ReflectionProbes.h")]
public sealed class ReflectionProbe : Behaviour
{
	public enum ReflectionProbeEvent
	{
		ReflectionProbeAdded,
		ReflectionProbeRemoved
	}

	private static Dictionary<int, Action<Texture>> registeredDefaultReflectionSetActions = new Dictionary<int, Action<Texture>>();

	private static List<Action<Texture>> registeredDefaultReflectionTextureActions = new List<Action<Texture>>();

	[NativeName("ProbeType")]
	[Obsolete("type property has been deprecated. Starting with Unity 5.4, the only supported reflection probe type is Cube.", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern ReflectionProbeType type
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeName("BoxSize")]
	public Vector3 size
	{
		get
		{
			get_size_Injected(out var ret);
			return ret;
		}
		set
		{
			set_size_Injected(ref value);
		}
	}

	[NativeName("BoxOffset")]
	public Vector3 center
	{
		get
		{
			get_center_Injected(out var ret);
			return ret;
		}
		set
		{
			set_center_Injected(ref value);
		}
	}

	[NativeName("Near")]
	public extern float nearClipPlane
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeName("Far")]
	public extern float farClipPlane
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeName("IntensityMultiplier")]
	public extern float intensity
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeName("GlobalAABB")]
	public Bounds bounds
	{
		get
		{
			get_bounds_Injected(out var ret);
			return ret;
		}
	}

	[NativeName("HDR")]
	public extern bool hdr
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeName("RenderDynamicObjects")]
	public extern bool renderDynamicObjects
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float shadowDistance
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int resolution
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int cullingMask
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern ReflectionProbeClearFlags clearFlags
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public Color backgroundColor
	{
		get
		{
			get_backgroundColor_Injected(out var ret);
			return ret;
		}
		set
		{
			set_backgroundColor_Injected(ref value);
		}
	}

	public extern float blendDistance
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool boxProjection
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern ReflectionProbeMode mode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int importance
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern ReflectionProbeRefreshMode refreshMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern ReflectionProbeTimeSlicingMode timeSlicingMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern Texture bakedTexture
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern Texture customBakedTexture
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern RenderTexture realtimeTexture
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern Texture texture
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public Vector4 textureHDRDecodeValues
	{
		[NativeName("CalculateHDRDecodeValues")]
		get
		{
			get_textureHDRDecodeValues_Injected(out var ret);
			return ret;
		}
	}

	[StaticAccessor("GetReflectionProbes()")]
	public static extern int minBakedCubemapResolution
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[StaticAccessor("GetReflectionProbes()")]
	public static extern int maxBakedCubemapResolution
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[StaticAccessor("GetReflectionProbes()")]
	public static Vector4 defaultTextureHDRDecodeValues
	{
		get
		{
			get_defaultTextureHDRDecodeValues_Injected(out var ret);
			return ret;
		}
	}

	[StaticAccessor("GetReflectionProbes()")]
	public static extern Texture defaultTexture
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public static event Action<ReflectionProbe, ReflectionProbeEvent> reflectionProbeChanged;

	[Obsolete("ReflectionProbe.defaultReflectionSet has been deprecated. Use ReflectionProbe.defaultReflectionTexture. (UnityUpgradable) -> UnityEngine.ReflectionProbe.defaultReflectionTexture", false)]
	public static event Action<Cubemap> defaultReflectionSet
	{
		add
		{
			if (registeredDefaultReflectionTextureActions.Any((Action<Texture> h) => h.Method == value.Method))
			{
				return;
			}
			Action<Texture> value2 = delegate(Texture b)
			{
				if (b is Cubemap obj)
				{
					value(obj);
				}
			};
			defaultReflectionTexture += value2;
			registeredDefaultReflectionSetActions[value.Method.GetHashCode()] = value2;
		}
		remove
		{
			if (registeredDefaultReflectionSetActions.TryGetValue(value.Method.GetHashCode(), out var value2))
			{
				defaultReflectionTexture -= value2;
				registeredDefaultReflectionSetActions.Remove(value.Method.GetHashCode());
			}
		}
	}

	public static event Action<Texture> defaultReflectionTexture
	{
		add
		{
			if (!registeredDefaultReflectionTextureActions.Any((Action<Texture> h) => h.Method == value.Method) && !registeredDefaultReflectionSetActions.ContainsKey(value.Method.GetHashCode()))
			{
				registeredDefaultReflectionTextureActions.Add(value);
			}
		}
		remove
		{
			registeredDefaultReflectionTextureActions.Remove(value);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void Reset();

	public int RenderProbe()
	{
		return RenderProbe(null);
	}

	public int RenderProbe([UnityEngine.Internal.DefaultValue("null")] RenderTexture targetTexture)
	{
		return ScheduleRender(timeSlicingMode, targetTexture);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern bool IsFinishedRendering(int renderId);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern int ScheduleRender(ReflectionProbeTimeSlicingMode timeSlicingMode, RenderTexture targetTexture);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("CubemapGPUBlend")]
	[NativeHeader("Runtime/Camera/CubemapGPUUtility.h")]
	public static extern bool BlendCubemap(Texture src, Texture dst, float blend, RenderTexture target);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("GetReflectionProbes()")]
	[NativeMethod("UpdateSampleData")]
	public static extern void UpdateCachedState();

	[RequiredByNativeCode]
	private static void CallReflectionProbeEvent(ReflectionProbe probe, ReflectionProbeEvent probeEvent)
	{
		ReflectionProbe.reflectionProbeChanged?.Invoke(probe, probeEvent);
	}

	[RequiredByNativeCode]
	private static void CallSetDefaultReflection(Texture defaultReflectionCubemap)
	{
		foreach (Action<Texture> registeredDefaultReflectionTextureAction in registeredDefaultReflectionTextureActions)
		{
			registeredDefaultReflectionTextureAction(defaultReflectionCubemap);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_size_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_size_Injected(ref Vector3 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_center_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_center_Injected(ref Vector3 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_bounds_Injected(out Bounds ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_backgroundColor_Injected(out Color ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_backgroundColor_Injected(ref Color value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_textureHDRDecodeValues_Injected(out Vector4 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private static extern void get_defaultTextureHDRDecodeValues_Injected(out Vector4 ret);
}
