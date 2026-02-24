using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;
using UnityEngineInternal;

namespace UnityEngine;

[NativeHeader("Runtime/Misc/ResourceManagerUtility.h")]
[NativeHeader("Runtime/Export/Resources/Resources.bindings.h")]
internal static class ResourcesAPIInternal
{
	internal static class EntitiesAssetGC
	{
		internal delegate void AdditionalRootsHandlerDelegate(IntPtr state);

		internal static AdditionalRootsHandlerDelegate AdditionalRootsHandler;

		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("Resources_Bindings::MarkInstanceIDsAsRoot")]
		internal static extern void MarkInstanceIDsAsRoot(IntPtr instanceIDs, int count, IntPtr state);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("Resources_Bindings::EnableEntitiesAssetGCCallback")]
		internal static extern void EnableEntitiesAssetGCCallback();

		internal static void RegisterAdditionalRootsHandler(AdditionalRootsHandlerDelegate newAdditionalRootsHandler)
		{
			if (AdditionalRootsHandler == null)
			{
				EnableEntitiesAssetGCCallback();
				AdditionalRootsHandler = newAdditionalRootsHandler;
			}
			else
			{
				Debug.LogWarning("Attempting to register more than one AdditionalRootsHandlerDelegate! Only one may be registered at a time.");
			}
		}

		[UsedByNativeCode]
		private static void GetAdditionalRoots(IntPtr state)
		{
			if (AdditionalRootsHandler != null)
			{
				AdditionalRootsHandler(state);
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("Resources_Bindings::FindObjectsOfTypeAll")]
	[TypeInferenceRule(TypeInferenceRules.ArrayOfTypeReferencedByFirstArgument)]
	public static extern Object[] FindObjectsOfTypeAll(Type type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("GetShaderNameRegistry().FindShader")]
	public static extern Shader FindShaderByName(string name);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("Resources_Bindings::Load")]
	[NativeThrows]
	[TypeInferenceRule(TypeInferenceRules.TypeReferencedBySecondArgument)]
	public static extern Object Load(string path, [NotNull("ArgumentNullException")] Type systemTypeInstance);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	[FreeFunction("Resources_Bindings::LoadAll")]
	public static extern Object[] LoadAll([NotNull("ArgumentNullException")] string path, [NotNull("ArgumentNullException")] Type systemTypeInstance);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("Resources_Bindings::LoadAsyncInternal")]
	internal static extern ResourceRequest LoadAsyncInternal(string path, Type type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("Scripting::UnloadAssetFromScripting")]
	public static extern void UnloadAsset(Object assetToUnload);
}
