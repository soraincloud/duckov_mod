using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace UnityEngine.XR;

[NativeConditional("ENABLE_XR")]
[NativeHeader("Modules/XR/XRPrefix.h")]
[UsedByNativeCode]
[NativeType(Header = "Modules/XR/Subsystems/Display/XRDisplaySubsystem.h")]
public class XRDisplaySubsystem : IntegratedSubsystem<XRDisplaySubsystemDescriptor>
{
	[Flags]
	public enum FoveatedRenderingFlags
	{
		None = 0,
		GazeAllowed = 1
	}

	public enum LateLatchNode
	{
		Head,
		LeftHand,
		RightHand
	}

	[Flags]
	public enum TextureLayout
	{
		Texture2DArray = 1,
		SingleTexture2D = 2,
		SeparateTexture2Ds = 4
	}

	public enum ReprojectionMode
	{
		Unspecified,
		PositionAndOrientation,
		OrientationOnly,
		None
	}

	[NativeHeader("Modules/XR/Subsystems/Display/XRDisplaySubsystem.bindings.h")]
	public struct XRRenderParameter
	{
		public Matrix4x4 view;

		public Matrix4x4 projection;

		public Rect viewport;

		public Mesh occlusionMesh;

		public int textureArraySlice;

		public Matrix4x4 previousView;

		public bool isPreviousViewValid;
	}

	[NativeHeader("Modules/XR/Subsystems/Display/XRDisplaySubsystem.bindings.h")]
	[NativeHeader("Runtime/Graphics/RenderTextureDesc.h")]
	[NativeHeader("Runtime/Graphics/CommandBuffer/RenderingCommandBuffer.h")]
	public struct XRRenderPass
	{
		private IntPtr displaySubsystemInstance;

		public int renderPassIndex;

		public RenderTargetIdentifier renderTarget;

		public RenderTextureDescriptor renderTargetDesc;

		public bool hasMotionVectorPass;

		public RenderTargetIdentifier motionVectorRenderTarget;

		public RenderTextureDescriptor motionVectorRenderTargetDesc;

		public bool shouldFillOutDepth;

		public int cullingPassIndex;

		public IntPtr foveatedRenderingInfo;

		[NativeConditional("ENABLE_XR")]
		[NativeMethod(Name = "XRRenderPassScriptApi::GetRenderParameter", IsFreeFunction = true, HasExplicitThis = true, ThrowsException = true)]
		public void GetRenderParameter(Camera camera, int renderParameterIndex, out XRRenderParameter renderParameter)
		{
			GetRenderParameter_Injected(ref this, camera, renderParameterIndex, out renderParameter);
		}

		[NativeConditional("ENABLE_XR")]
		[NativeMethod(Name = "XRRenderPassScriptApi::GetRenderParameterCount", IsFreeFunction = true, HasExplicitThis = true)]
		public int GetRenderParameterCount()
		{
			return GetRenderParameterCount_Injected(ref this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetRenderParameter_Injected(ref XRRenderPass _unity_self, Camera camera, int renderParameterIndex, out XRRenderParameter renderParameter);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int GetRenderParameterCount_Injected(ref XRRenderPass _unity_self);
	}

	[NativeHeader("Runtime/Graphics/RenderTexture.h")]
	[NativeHeader("Modules/XR/Subsystems/Display/XRDisplaySubsystem.bindings.h")]
	public struct XRBlitParams
	{
		public RenderTexture srcTex;

		public int srcTexArraySlice;

		public Rect srcRect;

		public Rect destRect;

		public IntPtr foveatedRenderingInfo;

		public bool srcHdrEncoded;

		public ColorGamut srcHdrColorGamut;

		public int srcHdrMaxLuminance;
	}

	[NativeHeader("Modules/XR/Subsystems/Display/XRDisplaySubsystem.bindings.h")]
	public struct XRMirrorViewBlitDesc
	{
		private IntPtr displaySubsystemInstance;

		public bool nativeBlitAvailable;

		public bool nativeBlitInvalidStates;

		public int blitParamsCount;

		[NativeMethod(Name = "XRMirrorViewBlitDescScriptApi::GetBlitParameter", IsFreeFunction = true, HasExplicitThis = true)]
		[NativeConditional("ENABLE_XR")]
		public void GetBlitParameter(int blitParameterIndex, out XRBlitParams blitParameter)
		{
			GetBlitParameter_Injected(ref this, blitParameterIndex, out blitParameter);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetBlitParameter_Injected(ref XRMirrorViewBlitDesc _unity_self, int blitParameterIndex, out XRBlitParams blitParameter);
	}

	private HDROutputSettings m_HDROutputSettings;

	[Obsolete("singlePassRenderingDisabled{get;set;} is deprecated. Use textureLayout and supportedTextureLayouts instead.", false)]
	public bool singlePassRenderingDisabled
	{
		get
		{
			return (textureLayout & TextureLayout.Texture2DArray) == 0;
		}
		set
		{
			if (value)
			{
				textureLayout = TextureLayout.SeparateTexture2Ds;
			}
			else if ((supportedTextureLayouts & TextureLayout.Texture2DArray) > (TextureLayout)0)
			{
				textureLayout = TextureLayout.Texture2DArray;
			}
		}
	}

	public extern bool displayOpaque
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern bool contentProtectionEnabled
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float scaleOfAllViewports
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float scaleOfAllRenderTargets
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float zNear
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float zFar
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool sRGB
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float occlusionMaskScale
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float foveatedRenderingLevel
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern FoveatedRenderingFlags foveatedRenderingFlags
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern TextureLayout textureLayout
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern TextureLayout supportedTextureLayouts
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern ReprojectionMode reprojectionMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool disableLegacyRenderer
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public HDROutputSettings hdrOutputSettings
	{
		get
		{
			if (m_HDROutputSettings == null)
			{
				m_HDROutputSettings = new HDROutputSettings(-1);
			}
			return m_HDROutputSettings;
		}
	}

	public event Action<bool> displayFocusChanged;

	[RequiredByNativeCode]
	private void InvokeDisplayFocusChanged(bool focus)
	{
		if (this.displayFocusChanged != null)
		{
			this.displayFocusChanged(focus);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void MarkTransformLateLatched(Transform transform, LateLatchNode nodeType);

	public void SetFocusPlane(Vector3 point, Vector3 normal, Vector3 velocity)
	{
		SetFocusPlane_Injected(ref point, ref normal, ref velocity);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetMSAALevel(int level);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern int GetRenderPassCount();

	public void GetRenderPass(int renderPassIndex, out XRRenderPass renderPass)
	{
		if (!Internal_TryGetRenderPass(renderPassIndex, out renderPass))
		{
			throw new IndexOutOfRangeException("renderPassIndex");
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("TryGetRenderPass")]
	private extern bool Internal_TryGetRenderPass(int renderPassIndex, out XRRenderPass renderPass);

	public void EndRecordingIfLateLatched(Camera camera)
	{
		if (!Internal_TryEndRecordingIfLateLatched(camera) && camera == null)
		{
			throw new ArgumentNullException("camera");
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("TryEndRecordingIfLateLatched")]
	private extern bool Internal_TryEndRecordingIfLateLatched(Camera camera);

	public void BeginRecordingIfLateLatched(Camera camera)
	{
		if (!Internal_TryBeginRecordingIfLateLatched(camera) && camera == null)
		{
			throw new ArgumentNullException("camera");
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("TryBeginRecordingIfLateLatched")]
	private extern bool Internal_TryBeginRecordingIfLateLatched(Camera camera);

	public void GetCullingParameters(Camera camera, int cullingPassIndex, out ScriptableCullingParameters scriptableCullingParameters)
	{
		if (!Internal_TryGetCullingParams(camera, cullingPassIndex, out scriptableCullingParameters))
		{
			if (camera == null)
			{
				throw new ArgumentNullException("camera");
			}
			throw new IndexOutOfRangeException("cullingPassIndex");
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeHeader("Runtime/Graphics/ScriptableRenderLoop/ScriptableCulling.h")]
	[NativeMethod("TryGetCullingParams")]
	private extern bool Internal_TryGetCullingParams(Camera camera, int cullingPassIndex, out ScriptableCullingParameters scriptableCullingParameters);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("TryGetAppGPUTimeLastFrame")]
	public extern bool TryGetAppGPUTimeLastFrame(out float gpuTimeLastFrame);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("TryGetCompositorGPUTimeLastFrame")]
	public extern bool TryGetCompositorGPUTimeLastFrame(out float gpuTimeLastFrameCompositor);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("TryGetDroppedFrameCount")]
	public extern bool TryGetDroppedFrameCount(out int droppedFrameCount);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("TryGetFramePresentCount")]
	public extern bool TryGetFramePresentCount(out int framePresentCount);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("TryGetDisplayRefreshRate")]
	public extern bool TryGetDisplayRefreshRate(out float displayRefreshRate);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("TryGetMotionToPhoton")]
	public extern bool TryGetMotionToPhoton(out float motionToPhoton);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeConditional("ENABLE_XR")]
	[NativeMethod(Name = "UnityXRRenderTextureIdToRenderTexture", IsThreadSafe = false)]
	public extern RenderTexture GetRenderTexture(uint unityXrRenderTextureId);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeConditional("ENABLE_XR")]
	[NativeMethod(Name = "GetTextureForRenderPass", IsThreadSafe = false)]
	public extern RenderTexture GetRenderTextureForRenderPass(int renderPass);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeConditional("ENABLE_XR")]
	[NativeMethod(Name = "GetSharedDepthTextureForRenderPass", IsThreadSafe = false)]
	public extern RenderTexture GetSharedDepthTextureForRenderPass(int renderPass);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "GetPreferredMirrorViewBlitMode", IsThreadSafe = false)]
	[NativeConditional("ENABLE_XR")]
	public extern int GetPreferredMirrorBlitMode();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "SetPreferredMirrorViewBlitMode", IsThreadSafe = false)]
	[NativeConditional("ENABLE_XR")]
	public extern void SetPreferredMirrorBlitMode(int blitMode);

	[Obsolete("GetMirrorViewBlitDesc(RenderTexture, out XRMirrorViewBlitDesc) is deprecated. Use GetMirrorViewBlitDesc(RenderTexture, out XRMirrorViewBlitDesc, int) instead.", false)]
	public bool GetMirrorViewBlitDesc(RenderTexture mirrorRt, out XRMirrorViewBlitDesc outDesc)
	{
		return GetMirrorViewBlitDesc(mirrorRt, out outDesc, -1);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "QueryMirrorViewBlitDesc", IsThreadSafe = false)]
	[NativeConditional("ENABLE_XR")]
	public extern bool GetMirrorViewBlitDesc(RenderTexture mirrorRt, out XRMirrorViewBlitDesc outDesc, int mode);

	[Obsolete("AddGraphicsThreadMirrorViewBlit(CommandBuffer, bool) is deprecated. Use AddGraphicsThreadMirrorViewBlit(CommandBuffer, bool, int) instead.", false)]
	public bool AddGraphicsThreadMirrorViewBlit(CommandBuffer cmd, bool allowGraphicsStateInvalidate)
	{
		return AddGraphicsThreadMirrorViewBlit(cmd, allowGraphicsStateInvalidate, -1);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeConditional("ENABLE_XR")]
	[NativeHeader("Runtime/Graphics/CommandBuffer/RenderingCommandBuffer.h")]
	[NativeMethod(Name = "AddGraphicsThreadMirrorViewBlit", IsThreadSafe = false)]
	public extern bool AddGraphicsThreadMirrorViewBlit(CommandBuffer cmd, bool allowGraphicsStateInvalidate, int mode);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetFocusPlane_Injected(ref Vector3 point, ref Vector3 normal, ref Vector3 velocity);
}
