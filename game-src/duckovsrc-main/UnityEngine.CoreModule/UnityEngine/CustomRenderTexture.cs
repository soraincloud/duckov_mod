using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Internal;
using UnityEngine.Scripting;

namespace UnityEngine;

[NativeHeader("Runtime/Graphics/CustomRenderTexture.h")]
[UsedByNativeCode]
public sealed class CustomRenderTexture : RenderTexture
{
	public extern Material material
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern Material initializationMaterial
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern Texture initializationTexture
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern CustomRenderTextureInitializationSource initializationSource
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public Color initializationColor
	{
		get
		{
			get_initializationColor_Injected(out var ret);
			return ret;
		}
		set
		{
			set_initializationColor_Injected(ref value);
		}
	}

	public extern CustomRenderTextureUpdateMode updateMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern CustomRenderTextureUpdateMode initializationMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern CustomRenderTextureUpdateZoneSpace updateZoneSpace
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int shaderPass
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern uint cubemapFaceMask
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool doubleBuffered
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool wrapUpdateZones
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float updatePeriod
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "CustomRenderTextureScripting::Create")]
	private static extern void Internal_CreateCustomRenderTexture([Writable] CustomRenderTexture rt);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("TriggerUpdate")]
	private extern void TriggerUpdate(int count);

	public void Update(int count)
	{
		CustomRenderTextureManager.InvokeTriggerUpdate(this, count);
		TriggerUpdate(count);
	}

	public void Update()
	{
		Update(1);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("TriggerInitialization")]
	private extern void TriggerInitialization();

	public void Initialize()
	{
		TriggerInitialization();
		CustomRenderTextureManager.InvokeTriggerInitialize(this);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void ClearUpdateZones();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "CustomRenderTextureScripting::GetUpdateZonesInternal", HasExplicitThis = true)]
	internal extern void GetUpdateZonesInternal([NotNull("ArgumentNullException")] object updateZones);

	public void GetUpdateZones(List<CustomRenderTextureUpdateZone> updateZones)
	{
		GetUpdateZonesInternal(updateZones);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "CustomRenderTextureScripting::SetUpdateZonesInternal", HasExplicitThis = true)]
	private extern void SetUpdateZonesInternal([Unmarshalled] CustomRenderTextureUpdateZone[] updateZones);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "CustomRenderTextureScripting::GetDoubleBufferRenderTexture", HasExplicitThis = true)]
	public extern RenderTexture GetDoubleBufferRenderTexture();

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void EnsureDoubleBufferConsistency();

	public void SetUpdateZones(CustomRenderTextureUpdateZone[] updateZones)
	{
		if (updateZones == null)
		{
			throw new ArgumentNullException("updateZones");
		}
		SetUpdateZonesInternal(updateZones);
	}

	public CustomRenderTexture(int width, int height, RenderTextureFormat format, [DefaultValue("RenderTextureReadWrite.Default")] RenderTextureReadWrite readWrite)
		: this(width, height, RenderTexture.GetCompatibleFormat(format, readWrite))
	{
	}

	[ExcludeFromDocs]
	public CustomRenderTexture(int width, int height, RenderTextureFormat format)
		: this(width, height, format, RenderTextureReadWrite.Default)
	{
	}

	[ExcludeFromDocs]
	public CustomRenderTexture(int width, int height)
		: this(width, height, SystemInfo.GetGraphicsFormat(DefaultFormat.LDR))
	{
	}

	[ExcludeFromDocs]
	public CustomRenderTexture(int width, int height, [DefaultValue("DefaultFormat.LDR")] DefaultFormat defaultFormat)
		: this(width, height, RenderTexture.GetDefaultColorFormat(defaultFormat))
	{
		if (defaultFormat == DefaultFormat.DepthStencil || defaultFormat == DefaultFormat.Shadow)
		{
			base.depthStencilFormat = SystemInfo.GetGraphicsFormat(defaultFormat);
		}
	}

	[ExcludeFromDocs]
	public CustomRenderTexture(int width, int height, GraphicsFormat format)
	{
		if (format == GraphicsFormat.None || ValidateFormat(format, FormatUsage.Render))
		{
			Internal_CreateCustomRenderTexture(this);
			this.width = width;
			this.height = height;
			base.graphicsFormat = format;
			SetSRGBReadWrite(GraphicsFormatUtility.IsSRGBFormat(format));
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_initializationColor_Injected(out Color ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_initializationColor_Injected(ref Color value);
}
