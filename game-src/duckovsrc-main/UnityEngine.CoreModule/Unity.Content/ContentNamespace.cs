using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine.Bindings;

namespace Unity.Content;

[StaticAccessor("GetContentNamespaceManager()", StaticAccessorType.Dot)]
[NativeHeader("Runtime/Misc/ContentNamespace.h")]
public struct ContentNamespace
{
	internal ulong Id;

	private static bool s_defaultInitialized = false;

	private static ContentNamespace s_Default;

	private static Regex s_ValidName = new Regex("^[a-zA-Z0-9]{1,16}$", RegexOptions.Compiled);

	public bool IsValid => IsNamespaceHandleValid(this);

	public static ContentNamespace Default
	{
		get
		{
			if (!s_defaultInitialized)
			{
				s_defaultInitialized = true;
				s_Default = GetOrCreateNamespace("default");
			}
			return s_Default;
		}
	}

	public string GetName()
	{
		ThrowIfInvalidNamespace();
		return GetNamespaceName(this);
	}

	public void Delete()
	{
		if (Id == s_Default.Id)
		{
			throw new InvalidOperationException("Cannot delete the default namespace.");
		}
		ThrowIfInvalidNamespace();
		RemoveNamespace(this);
	}

	private void ThrowIfInvalidNamespace()
	{
		if (!IsValid)
		{
			throw new InvalidOperationException("The provided namespace is invalid. Did you already delete it?");
		}
	}

	public static ContentNamespace GetOrCreateNamespace(string name)
	{
		if (s_ValidName.IsMatch(name))
		{
			return GetOrCreate(name);
		}
		throw new InvalidOperationException("Namespace name can only contain alphanumeric characters and a maximum length of 16 characters.");
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern ContentNamespace[] GetAll();

	internal static ContentNamespace GetOrCreate(string name)
	{
		GetOrCreate_Injected(name, out var ret);
		return ret;
	}

	internal static void RemoveNamespace(ContentNamespace ns)
	{
		RemoveNamespace_Injected(ref ns);
	}

	internal static string GetNamespaceName(ContentNamespace ns)
	{
		return GetNamespaceName_Injected(ref ns);
	}

	internal static bool IsNamespaceHandleValid(ContentNamespace ns)
	{
		return IsNamespaceHandleValid_Injected(ref ns);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void GetOrCreate_Injected(string name, out ContentNamespace ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void RemoveNamespace_Injected(ref ContentNamespace ns);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string GetNamespaceName_Injected(ref ContentNamespace ns);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool IsNamespaceHandleValid_Injected(ref ContentNamespace ns);
}
