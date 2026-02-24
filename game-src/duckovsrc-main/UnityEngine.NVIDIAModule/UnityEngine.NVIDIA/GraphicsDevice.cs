using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;

namespace UnityEngine.NVIDIA;

public class GraphicsDevice
{
	private static string s_DefaultProjectID = "231313132";

	private static string s_DefaultAppDir = ".\\";

	private static GraphicsDevice sGraphicsDeviceInstance = null;

	private InitDeviceContext m_InitDeviceContext = null;

	private Stack<DLSSContext> s_ContextObjectPool = new Stack<DLSSContext>();

	public static GraphicsDevice device => sGraphicsDeviceInstance;

	public static uint version => NVUP_GetDeviceVersion();

	private GraphicsDevice(string projectId, string engineVersion, string appDir)
	{
		m_InitDeviceContext = new InitDeviceContext(projectId, engineVersion, appDir);
	}

	private bool Initialize()
	{
		return NVUP_InitApi(m_InitDeviceContext.GetInitCmdPtr());
	}

	private void Shutdown()
	{
		NVUP_ShutdownApi();
	}

	~GraphicsDevice()
	{
		Shutdown();
	}

	private void InsertEventCall(CommandBuffer cmd, PluginEvent pluginEvent, IntPtr ptr)
	{
		cmd.IssuePluginEventAndData(NVUP_GetRenderEventCallback(), (int)(pluginEvent + NVUP_GetBaseEventId()), ptr);
	}

	private static GraphicsDevice InternalCreate(string appIdOrProjectId, string engineVersion, string appDir)
	{
		if (sGraphicsDeviceInstance != null)
		{
			sGraphicsDeviceInstance.Shutdown();
			sGraphicsDeviceInstance.Initialize();
			return sGraphicsDeviceInstance;
		}
		GraphicsDevice graphicsDevice = new GraphicsDevice(appIdOrProjectId, engineVersion, appDir);
		if (graphicsDevice.Initialize())
		{
			sGraphicsDeviceInstance = graphicsDevice;
			return graphicsDevice;
		}
		return null;
	}

	private static int CreateSetTextureUserData(int featureId, int textureSlot, bool clearTextureTable)
	{
		int num = featureId & 0xFFFF;
		int num2 = textureSlot & 0x7FFF;
		int num3 = (clearTextureTable ? 1 : 0);
		return (num << 16) | (num2 << 1) | num3;
	}

	private void SetTexture(CommandBuffer cmd, DLSSContext dlssContext, DLSSCommandExecutionData.Textures textureSlot, Texture texture, bool clearTextureTable = false)
	{
		if (!(texture == null))
		{
			uint userData = (uint)CreateSetTextureUserData((int)dlssContext.featureSlot, (int)textureSlot, clearTextureTable);
			cmd.IssuePluginCustomTextureUpdateV2(NVUP_GetSetTextureEventCallback(), texture, userData);
		}
	}

	internal GraphicsDeviceDebugInfo GetDebugInfo(uint debugViewId)
	{
		GraphicsDeviceDebugInfo data = default(GraphicsDeviceDebugInfo);
		NVUP_GetGraphicsDeviceDebugInfo(debugViewId, out data);
		return data;
	}

	internal uint CreateDebugViewId()
	{
		return NVUP_CreateDebugView();
	}

	internal void DeleteDebugViewId(uint debugViewId)
	{
		NVUP_DeleteDebugView(debugViewId);
	}

	public static GraphicsDevice CreateGraphicsDevice()
	{
		return InternalCreate(s_DefaultProjectID, Application.unityVersion, s_DefaultAppDir);
	}

	public static GraphicsDevice CreateGraphicsDevice(string projectID)
	{
		return InternalCreate(projectID, Application.unityVersion, s_DefaultAppDir);
	}

	public static GraphicsDevice CreateGraphicsDevice(string projectID, string appDir)
	{
		return InternalCreate(projectID, Application.unityVersion, appDir);
	}

	public bool IsFeatureAvailable(GraphicsDeviceFeature featureID)
	{
		return NVUP_IsFeatureAvailable(featureID);
	}

	public DLSSContext CreateFeature(CommandBuffer cmd, in DLSSCommandInitializationData initSettings)
	{
		if (!IsFeatureAvailable(GraphicsDeviceFeature.DLSS))
		{
			return null;
		}
		DLSSContext dLSSContext = null;
		dLSSContext = ((s_ContextObjectPool.Count != 0) ? s_ContextObjectPool.Pop() : new DLSSContext());
		dLSSContext.Init(initSettings, NVUP_CreateFeatureSlot());
		InsertEventCall(cmd, PluginEvent.DLSSInit, dLSSContext.GetInitCmdPtr());
		return dLSSContext;
	}

	public void DestroyFeature(CommandBuffer cmd, DLSSContext dlssContext)
	{
		InsertEventCall(cmd, PluginEvent.DestroyFeature, new IntPtr(dlssContext.featureSlot));
		dlssContext.Reset();
		s_ContextObjectPool.Push(dlssContext);
	}

	public void ExecuteDLSS(CommandBuffer cmd, DLSSContext dlssContext, in DLSSTextureTable textures)
	{
		SetTexture(cmd, dlssContext, DLSSCommandExecutionData.Textures.ColorInput, textures.colorInput, clearTextureTable: true);
		SetTexture(cmd, dlssContext, DLSSCommandExecutionData.Textures.ColorOutput, textures.colorOutput);
		SetTexture(cmd, dlssContext, DLSSCommandExecutionData.Textures.Depth, textures.depth);
		SetTexture(cmd, dlssContext, DLSSCommandExecutionData.Textures.MotionVectors, textures.motionVectors);
		SetTexture(cmd, dlssContext, DLSSCommandExecutionData.Textures.TransparencyMask, textures.transparencyMask);
		SetTexture(cmd, dlssContext, DLSSCommandExecutionData.Textures.ExposureTexture, textures.exposureTexture);
		SetTexture(cmd, dlssContext, DLSSCommandExecutionData.Textures.BiasColorMask, textures.biasColorMask);
		InsertEventCall(cmd, PluginEvent.DLSSExecute, dlssContext.GetExecuteCmdPtr());
	}

	public bool GetOptimalSettings(uint targetWidth, uint targetHeight, DLSSQuality quality, out OptimalDLSSSettingsData optimalSettings)
	{
		return NVUP_GetOptimalSettings(targetWidth, targetHeight, quality, out optimalSettings);
	}

	public GraphicsDeviceDebugView CreateDebugView()
	{
		return new GraphicsDeviceDebugView(CreateDebugViewId());
	}

	public unsafe void UpdateDebugView(GraphicsDeviceDebugView debugView)
	{
		if (debugView != null)
		{
			GraphicsDeviceDebugInfo debugInfo = GetDebugInfo(debugView.m_ViewId);
			debugView.m_DeviceVersion = debugInfo.NVDeviceVersion;
			debugView.m_NgxVersion = debugInfo.NGXVersion;
			if (debugView.m_DlssDebugFeatures == null || debugInfo.dlssInfosCount != debugView.m_DlssDebugFeatures.Length)
			{
				debugView.m_DlssDebugFeatures = new DLSSDebugFeatureInfos[debugInfo.dlssInfosCount];
			}
			for (int i = 0; i < debugInfo.dlssInfosCount; i++)
			{
				debugView.m_DlssDebugFeatures[i] = debugInfo.dlssInfos[i];
			}
		}
	}

	public void DeleteDebugView(GraphicsDeviceDebugView debugView)
	{
		if (debugView != null)
		{
			DeleteDebugViewId(debugView.m_ViewId);
		}
	}

	[DllImport("NVUnityPlugin", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	private static extern bool NVUP_InitApi(IntPtr initData);

	[DllImport("NVUnityPlugin", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	private static extern void NVUP_ShutdownApi();

	[DllImport("NVUnityPlugin", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	private static extern bool NVUP_IsFeatureAvailable(GraphicsDeviceFeature featureID);

	[DllImport("NVUnityPlugin", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	private static extern bool NVUP_GetOptimalSettings(uint inTargetWidth, uint inTargetHeight, DLSSQuality inPerfVQuality, out OptimalDLSSSettingsData data);

	[DllImport("NVUnityPlugin", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	private static extern IntPtr NVUP_GetRenderEventCallback();

	[DllImport("NVUnityPlugin", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	private static extern IntPtr NVUP_GetSetTextureEventCallback();

	[DllImport("NVUnityPlugin", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	private static extern uint NVUP_CreateFeatureSlot();

	[DllImport("NVUnityPlugin", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	private static extern uint NVUP_GetDeviceVersion();

	[DllImport("NVUnityPlugin", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	private static extern uint NVUP_CreateDebugView();

	[DllImport("NVUnityPlugin", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	private static extern void NVUP_GetGraphicsDeviceDebugInfo(uint debugViewId, out GraphicsDeviceDebugInfo data);

	[DllImport("NVUnityPlugin", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	private static extern void NVUP_DeleteDebugView(uint debugViewId);

	[DllImport("NVUnityPlugin", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	private static extern int NVUP_GetBaseEventId();
}
