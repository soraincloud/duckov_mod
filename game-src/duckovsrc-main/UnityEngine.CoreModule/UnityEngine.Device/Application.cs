using System;
using System.Threading;
using UnityEngine.Events;

namespace UnityEngine.Device;

public static class Application
{
	public static string absoluteURL => UnityEngine.Application.absoluteURL;

	public static ThreadPriority backgroundLoadingPriority
	{
		get
		{
			return UnityEngine.Application.backgroundLoadingPriority;
		}
		set
		{
			UnityEngine.Application.backgroundLoadingPriority = value;
		}
	}

	public static string buildGUID => UnityEngine.Application.buildGUID;

	public static string cloudProjectId => UnityEngine.Application.cloudProjectId;

	public static string companyName => UnityEngine.Application.companyName;

	public static string consoleLogPath => UnityEngine.Application.consoleLogPath;

	public static string dataPath => UnityEngine.Application.dataPath;

	public static bool genuine => UnityEngine.Application.genuine;

	public static bool genuineCheckAvailable => UnityEngine.Application.genuineCheckAvailable;

	public static string identifier => UnityEngine.Application.identifier;

	public static string installerName => UnityEngine.Application.installerName;

	public static ApplicationInstallMode installMode => UnityEngine.Application.installMode;

	public static NetworkReachability internetReachability => UnityEngine.Application.internetReachability;

	public static bool isBatchMode => UnityEngine.Application.isBatchMode;

	public static bool isConsolePlatform => UnityEngine.Application.isConsolePlatform;

	public static bool isEditor => UnityEngine.Application.isEditor;

	public static bool isFocused => UnityEngine.Application.isFocused;

	public static bool isMobilePlatform => UnityEngine.Application.isMobilePlatform;

	public static bool isPlaying => UnityEngine.Application.isPlaying;

	public static string persistentDataPath => UnityEngine.Application.persistentDataPath;

	public static RuntimePlatform platform => UnityEngine.Application.platform;

	public static string productName => UnityEngine.Application.productName;

	public static bool runInBackground
	{
		get
		{
			return UnityEngine.Application.runInBackground;
		}
		set
		{
			UnityEngine.Application.runInBackground = value;
		}
	}

	public static ApplicationSandboxType sandboxType => UnityEngine.Application.sandboxType;

	public static string streamingAssetsPath => UnityEngine.Application.streamingAssetsPath;

	public static SystemLanguage systemLanguage => UnityEngine.Application.systemLanguage;

	public static int targetFrameRate
	{
		get
		{
			return UnityEngine.Application.targetFrameRate;
		}
		set
		{
			UnityEngine.Application.targetFrameRate = value;
		}
	}

	public static string temporaryCachePath => UnityEngine.Application.temporaryCachePath;

	public static string unityVersion => UnityEngine.Application.unityVersion;

	public static string version => UnityEngine.Application.version;

	public static CancellationToken exitCancellationToken => UnityEngine.Application.exitCancellationToken;

	public static event Action<string> deepLinkActivated
	{
		add
		{
			UnityEngine.Application.deepLinkActivated += value;
		}
		remove
		{
			UnityEngine.Application.deepLinkActivated -= value;
		}
	}

	public static event Action<bool> focusChanged
	{
		add
		{
			UnityEngine.Application.focusChanged += value;
		}
		remove
		{
			UnityEngine.Application.focusChanged -= value;
		}
	}

	public static event UnityEngine.Application.LogCallback logMessageReceived
	{
		add
		{
			UnityEngine.Application.logMessageReceived += value;
		}
		remove
		{
			UnityEngine.Application.logMessageReceived -= value;
		}
	}

	public static event UnityEngine.Application.LogCallback logMessageReceivedThreaded
	{
		add
		{
			UnityEngine.Application.logMessageReceivedThreaded += value;
		}
		remove
		{
			UnityEngine.Application.logMessageReceivedThreaded -= value;
		}
	}

	public static event UnityEngine.Application.LowMemoryCallback lowMemory
	{
		add
		{
			UnityEngine.Application.lowMemory += value;
		}
		remove
		{
			UnityEngine.Application.lowMemory -= value;
		}
	}

	public static event UnityEngine.Application.MemoryUsageChangedCallback memoryUsageChanged
	{
		add
		{
			UnityEngine.Application.memoryUsageChanged += value;
		}
		remove
		{
			UnityEngine.Application.memoryUsageChanged -= value;
		}
	}

	public static event UnityAction onBeforeRender
	{
		add
		{
			UnityEngine.Application.onBeforeRender += value;
		}
		remove
		{
			UnityEngine.Application.onBeforeRender -= value;
		}
	}

	public static event Action quitting
	{
		add
		{
			UnityEngine.Application.quitting += value;
		}
		remove
		{
			UnityEngine.Application.quitting -= value;
		}
	}

	public static event Func<bool> wantsToQuit
	{
		add
		{
			UnityEngine.Application.wantsToQuit += value;
		}
		remove
		{
			UnityEngine.Application.wantsToQuit -= value;
		}
	}

	public static event Action unloading
	{
		add
		{
			UnityEngine.Application.unloading += value;
		}
		remove
		{
			UnityEngine.Application.unloading -= value;
		}
	}

	public static bool CanStreamedLevelBeLoaded(int levelIndex)
	{
		return UnityEngine.Application.CanStreamedLevelBeLoaded(levelIndex);
	}

	public static bool CanStreamedLevelBeLoaded(string levelName)
	{
		return UnityEngine.Application.CanStreamedLevelBeLoaded(levelName);
	}

	[Obsolete("Application.GetBuildTags is no longer supported and will be removed.", false)]
	public static string[] GetBuildTags()
	{
		return UnityEngine.Application.GetBuildTags();
	}

	[Obsolete("Application.SetBuildTags is no longer supported and will be removed.", false)]
	public static void SetBuildTags(string[] buildTags)
	{
		UnityEngine.Application.SetBuildTags(buildTags);
	}

	public static StackTraceLogType GetStackTraceLogType(LogType logType)
	{
		return UnityEngine.Application.GetStackTraceLogType(logType);
	}

	public static bool HasProLicense()
	{
		return UnityEngine.Application.HasProLicense();
	}

	public static bool HasUserAuthorization(UserAuthorization mode)
	{
		return UnityEngine.Application.HasUserAuthorization(mode);
	}

	public static bool IsPlaying(Object obj)
	{
		return UnityEngine.Application.IsPlaying(obj);
	}

	public static void OpenURL(string url)
	{
		UnityEngine.Application.OpenURL(url);
	}

	public static void Quit()
	{
		UnityEngine.Application.Quit();
	}

	public static void Quit(int exitCode)
	{
		UnityEngine.Application.Quit(exitCode);
	}

	public static bool RequestAdvertisingIdentifierAsync(UnityEngine.Application.AdvertisingIdentifierCallback delegateMethod)
	{
		return UnityEngine.Application.RequestAdvertisingIdentifierAsync(delegateMethod);
	}

	public static AsyncOperation RequestUserAuthorization(UserAuthorization mode)
	{
		return UnityEngine.Application.RequestUserAuthorization(mode);
	}

	public static void SetStackTraceLogType(LogType logType, StackTraceLogType stackTraceType)
	{
		UnityEngine.Application.SetStackTraceLogType(logType, stackTraceType);
	}

	public static void Unload()
	{
		UnityEngine.Application.Unload();
	}
}
