using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Rendering;

[StaticAccessor("GetGraphicsSettings()", StaticAccessorType.Dot)]
[NativeHeader("Runtime/Camera/GraphicsSettings.h")]
public sealed class GraphicsSettings : Object
{
	public static extern TransparencySortMode transparencySortMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static Vector3 transparencySortAxis
	{
		get
		{
			get_transparencySortAxis_Injected(out var ret);
			return ret;
		}
		set
		{
			set_transparencySortAxis_Injected(ref value);
		}
	}

	public static extern bool realtimeDirectRectangularAreaLights
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern bool lightsUseLinearIntensity
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern bool lightsUseColorTemperature
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern uint defaultRenderingLayerMask
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern bool useScriptableRenderPipelineBatching
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern bool logWhenShaderIsCompiled
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern bool disableBuiltinCustomRenderTextureUpdate
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern VideoShadersIncludeMode videoShadersIncludeMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public static extern LightProbeOutsideHullStrategy lightProbeOutsideHullStrategy
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeName("CurrentRenderPipeline")]
	private static extern ScriptableObject INTERNAL_currentRenderPipeline
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public static RenderPipelineAsset currentRenderPipeline => INTERNAL_currentRenderPipeline as RenderPipelineAsset;

	public static RenderPipelineAsset renderPipelineAsset
	{
		get
		{
			return defaultRenderPipeline;
		}
		set
		{
			defaultRenderPipeline = value;
		}
	}

	[NativeName("DefaultRenderPipeline")]
	private static extern ScriptableObject INTERNAL_defaultRenderPipeline
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static RenderPipelineAsset defaultRenderPipeline
	{
		get
		{
			return INTERNAL_defaultRenderPipeline as RenderPipelineAsset;
		}
		set
		{
			INTERNAL_defaultRenderPipeline = value;
		}
	}

	public static RenderPipelineAsset[] allConfiguredRenderPipelines => GetAllConfiguredRenderPipelines().Cast<RenderPipelineAsset>().ToArray();

	public static extern bool cameraRelativeLightCulling
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern bool cameraRelativeShadowCulling
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	private GraphicsSettings()
	{
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern bool HasShaderDefine(GraphicsTier tier, BuiltinShaderDefine defineHash);

	public static bool HasShaderDefine(BuiltinShaderDefine defineHash)
	{
		return HasShaderDefine(Graphics.activeTier, defineHash);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetAllConfiguredRenderPipelinesForScript")]
	private static extern ScriptableObject[] GetAllConfiguredRenderPipelines();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction]
	public static extern Object GetGraphicsSettings();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("SetShaderModeScript")]
	public static extern void SetShaderMode(BuiltinShaderType type, BuiltinShaderMode mode);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetShaderModeScript")]
	public static extern BuiltinShaderMode GetShaderMode(BuiltinShaderType type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("SetCustomShaderScript")]
	public static extern void SetCustomShader(BuiltinShaderType type, Shader shader);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetCustomShaderScript")]
	public static extern Shader GetCustomShader(BuiltinShaderType type);

	public static void RegisterRenderPipelineSettings<T>(RenderPipelineGlobalSettings settings) where T : RenderPipeline
	{
		RegisterRenderPipeline(typeof(T).FullName, settings);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("RegisterRenderPipelineSettings")]
	private static extern void RegisterRenderPipeline(string renderpipelineName, Object settings);

	public static void UnregisterRenderPipelineSettings<T>() where T : RenderPipeline
	{
		UnregisterRenderPipeline(typeof(T).FullName);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("UnregisterRenderPipelineSettings")]
	private static extern void UnregisterRenderPipeline(string renderpipelineName);

	public static RenderPipelineGlobalSettings GetSettingsForRenderPipeline<T>() where T : RenderPipeline
	{
		return GetSettingsForRenderPipeline(typeof(T).FullName) as RenderPipelineGlobalSettings;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetSettingsForRenderPipeline")]
	private static extern Object GetSettingsForRenderPipeline(string renderpipelineName);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private static extern void get_transparencySortAxis_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private static extern void set_transparencySortAxis_Injected(ref Vector3 value);
}
