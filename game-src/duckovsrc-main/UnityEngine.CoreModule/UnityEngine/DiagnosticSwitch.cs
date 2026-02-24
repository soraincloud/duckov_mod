using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;

namespace UnityEngine;

[StructLayout(LayoutKind.Sequential)]
[NativeClass("DiagnosticSwitch", "struct DiagnosticSwitch;")]
[NativeHeader("Runtime/Utilities/DiagnosticSwitch.h")]
[NativeAsStruct]
internal class DiagnosticSwitch
{
	[Flags]
	internal enum Flags
	{
		None = 0,
		CanChangeAfterEngineStart = 1,
		PropagateToAssetImportWorkerProcess = 2
	}

	private IntPtr m_Ptr;

	public extern string name
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern string description
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[NativeName("OwningModuleName")]
	public extern string owningModule
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern Flags flags
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public object value
	{
		get
		{
			return GetScriptingValue();
		}
		set
		{
			SetScriptingValue(value, setPersistent: false);
		}
	}

	[NativeName("ScriptingDefaultValue")]
	public extern object defaultValue
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[NativeName("ScriptingMinValue")]
	public extern object minValue
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[NativeName("ScriptingMaxValue")]
	public extern object maxValue
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public object persistentValue
	{
		get
		{
			return GetScriptingPersistentValue();
		}
		set
		{
			SetScriptingValue(value, setPersistent: true);
		}
	}

	[NativeName("ScriptingEnumInfo")]
	public extern EnumInfo enumInfo
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public bool isSetToDefault => object.Equals(persistentValue, defaultValue);

	public bool needsRestart => !object.Equals(value, persistentValue);

	private DiagnosticSwitch()
	{
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern object GetScriptingValue();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern object GetScriptingPersistentValue();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	private extern void SetScriptingValue(object value, bool setPersistent);
}
