using System;

namespace UnityEngine.Rendering.Universal;

public class DebugDisplaySettingsRendering : IDebugDisplaySettingsData, IDebugDisplaySettingsQuery
{
	public enum TaaDebugMode
	{
		None,
		ShowRawFrame,
		ShowRawFrameNoJitter,
		ShowClampedHistory
	}

	private static class Strings
	{
		public const string RangeValidationSettingsContainerName = "Pixel Range Settings";

		public static readonly DebugUI.Widget.NameAndTooltip MapOverlays = new DebugUI.Widget.NameAndTooltip
		{
			name = "Map Overlays",
			tooltip = "Overlays render pipeline textures to validate the scene."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MapSize = new DebugUI.Widget.NameAndTooltip
		{
			name = "Map Size",
			tooltip = "Set the size of the render pipeline texture in the scene."
		};

		public static readonly DebugUI.Widget.NameAndTooltip AdditionalWireframeModes = new DebugUI.Widget.NameAndTooltip
		{
			name = "Additional Wireframe Modes",
			tooltip = "Debug the scene with additional wireframe shader views that are different from those in the scene view."
		};

		public static readonly DebugUI.Widget.NameAndTooltip WireframeNotSupportedWarning = new DebugUI.Widget.NameAndTooltip
		{
			name = "Warning: This platform might not support wireframe rendering.",
			tooltip = "Some platforms, for example, mobile platforms using OpenGL ES and Vulkan, might not support wireframe rendering."
		};

		public static readonly DebugUI.Widget.NameAndTooltip OverdrawMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Overdraw Mode",
			tooltip = "Debug anywhere materials that overdrawn pixels top of each other."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MaxOverdrawCount = new DebugUI.Widget.NameAndTooltip
		{
			name = "Max Overdraw Count",
			tooltip = "Maximum overdraw count allowed for a single pixel."
		};

		public static readonly DebugUI.Widget.NameAndTooltip PostProcessing = new DebugUI.Widget.NameAndTooltip
		{
			name = "Post-processing",
			tooltip = "Override the controls for Post Processing in the scene."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MSAA = new DebugUI.Widget.NameAndTooltip
		{
			name = "MSAA",
			tooltip = "Use the checkbox to disable MSAA in the scene."
		};

		public static readonly DebugUI.Widget.NameAndTooltip HDR = new DebugUI.Widget.NameAndTooltip
		{
			name = "HDR",
			tooltip = "Use the checkbox to disable High Dynamic Range in the scene."
		};

		public static readonly DebugUI.Widget.NameAndTooltip TaaDebugMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "TAA Debug Mode",
			tooltip = "Choose whether to force TAA to output the raw jittered frame or clamped reprojected history."
		};

		public static readonly DebugUI.Widget.NameAndTooltip PixelValidationMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Pixel Validation Mode",
			tooltip = "Choose between modes that validate pixel on screen."
		};

		public static readonly DebugUI.Widget.NameAndTooltip Channels = new DebugUI.Widget.NameAndTooltip
		{
			name = "Channels",
			tooltip = "Choose the texture channel used to validate the scene."
		};

		public static readonly DebugUI.Widget.NameAndTooltip ValueRangeMin = new DebugUI.Widget.NameAndTooltip
		{
			name = "Value Range Min",
			tooltip = "Any values set below this field will be considered invalid and will appear red on screen."
		};

		public static readonly DebugUI.Widget.NameAndTooltip ValueRangeMax = new DebugUI.Widget.NameAndTooltip
		{
			name = "Value Range Max",
			tooltip = "Any values set above this field will be considered invalid and will appear blue on screen."
		};
	}

	internal static class WidgetFactory
	{
		internal static DebugUI.Widget CreateMapOverlays(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.MapOverlays,
				autoEnum = typeof(DebugFullScreenMode),
				getter = () => (int)panel.data.fullScreenDebugMode,
				setter = delegate(int value)
				{
					panel.data.fullScreenDebugMode = (DebugFullScreenMode)value;
				},
				getIndex = () => (int)panel.data.fullScreenDebugMode,
				setIndex = delegate(int value)
				{
					panel.data.fullScreenDebugMode = (DebugFullScreenMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateMapOverlaySize(SettingsPanel panel)
		{
			return new DebugUI.Container
			{
				children = { (DebugUI.Widget)new DebugUI.IntField
				{
					nameAndTooltip = Strings.MapSize,
					getter = () => panel.data.fullScreenDebugModeOutputSizeScreenPercent,
					setter = delegate(int value)
					{
						panel.data.fullScreenDebugModeOutputSizeScreenPercent = value;
					},
					incStep = 10,
					min = () => 0,
					max = () => 100
				} }
			};
		}

		internal static DebugUI.Widget CreateAdditionalWireframeShaderViews(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.AdditionalWireframeModes,
				autoEnum = typeof(DebugWireframeMode),
				getter = () => (int)panel.data.wireframeMode,
				setter = delegate(int value)
				{
					panel.data.wireframeMode = (DebugWireframeMode)value;
				},
				getIndex = () => (int)panel.data.wireframeMode,
				setIndex = delegate(int value)
				{
					panel.data.wireframeMode = (DebugWireframeMode)value;
				},
				onValueChanged = delegate
				{
					DebugManager.instance.ReDrawOnScreenDebug();
				}
			};
		}

		internal static DebugUI.Widget CreateWireframeNotSupportedWarning(SettingsPanel panel)
		{
			return new DebugUI.MessageBox
			{
				nameAndTooltip = Strings.WireframeNotSupportedWarning,
				style = DebugUI.MessageBox.Style.Warning,
				isHiddenCallback = delegate
				{
					GraphicsDeviceType graphicsDeviceType = SystemInfo.graphicsDeviceType;
					return (graphicsDeviceType != GraphicsDeviceType.OpenGLES2 && graphicsDeviceType != GraphicsDeviceType.OpenGLES3 && graphicsDeviceType != GraphicsDeviceType.Vulkan) || panel.data.wireframeMode == DebugWireframeMode.None;
				}
			};
		}

		internal static DebugUI.Widget CreateOverdrawMode(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.OverdrawMode,
				autoEnum = typeof(DebugOverdrawMode),
				getter = () => (int)panel.data.overdrawMode,
				setter = delegate(int value)
				{
					panel.data.overdrawMode = (DebugOverdrawMode)value;
				},
				getIndex = () => (int)panel.data.overdrawMode,
				setIndex = delegate(int value)
				{
					panel.data.overdrawMode = (DebugOverdrawMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateMaxOverdrawCount(SettingsPanel panel)
		{
			return new DebugUI.Container
			{
				isHiddenCallback = () => panel.data.overdrawMode == DebugOverdrawMode.None,
				children = { (DebugUI.Widget)new DebugUI.IntField
				{
					nameAndTooltip = Strings.MaxOverdrawCount,
					getter = () => panel.data.maxOverdrawCount,
					setter = delegate(int value)
					{
						panel.data.maxOverdrawCount = value;
					},
					incStep = 10,
					min = () => 1,
					max = () => 500
				} }
			};
		}

		internal static DebugUI.Widget CreatePostProcessing(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.PostProcessing,
				autoEnum = typeof(DebugPostProcessingMode),
				getter = () => (int)panel.data.postProcessingDebugMode,
				setter = delegate(int value)
				{
					panel.data.postProcessingDebugMode = (DebugPostProcessingMode)value;
				},
				getIndex = () => (int)panel.data.postProcessingDebugMode,
				setIndex = delegate(int value)
				{
					panel.data.postProcessingDebugMode = (DebugPostProcessingMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateMSAA(SettingsPanel panel)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.MSAA,
				getter = () => panel.data.enableMsaa,
				setter = delegate(bool value)
				{
					panel.data.enableMsaa = value;
				}
			};
		}

		internal static DebugUI.Widget CreateHDR(SettingsPanel panel)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.HDR,
				getter = () => panel.data.enableHDR,
				setter = delegate(bool value)
				{
					panel.data.enableHDR = value;
				}
			};
		}

		internal static DebugUI.Widget CreateTaaDebugMode(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.TaaDebugMode,
				autoEnum = typeof(TaaDebugMode),
				getter = () => (int)panel.data.taaDebugMode,
				setter = delegate(int value)
				{
					panel.data.taaDebugMode = (TaaDebugMode)value;
				},
				getIndex = () => (int)panel.data.taaDebugMode,
				setIndex = delegate(int value)
				{
					panel.data.taaDebugMode = (TaaDebugMode)value;
				},
				onValueChanged = delegate
				{
					DebugManager.instance.ReDrawOnScreenDebug();
				}
			};
		}

		internal static DebugUI.Widget CreatePixelValidationMode(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.PixelValidationMode,
				autoEnum = typeof(DebugValidationMode),
				getter = () => (int)panel.data.validationMode,
				setter = delegate(int value)
				{
					panel.data.validationMode = (DebugValidationMode)value;
				},
				getIndex = () => (int)panel.data.validationMode,
				setIndex = delegate(int value)
				{
					panel.data.validationMode = (DebugValidationMode)value;
				},
				onValueChanged = delegate
				{
					DebugManager.instance.ReDrawOnScreenDebug();
				}
			};
		}

		internal static DebugUI.Widget CreatePixelValidationChannels(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.Channels,
				autoEnum = typeof(PixelValidationChannels),
				getter = () => (int)panel.data.validationChannels,
				setter = delegate(int value)
				{
					panel.data.validationChannels = (PixelValidationChannels)value;
				},
				getIndex = () => (int)panel.data.validationChannels,
				setIndex = delegate(int value)
				{
					panel.data.validationChannels = (PixelValidationChannels)value;
				}
			};
		}

		internal static DebugUI.Widget CreatePixelValueRangeMin(SettingsPanel panel)
		{
			return new DebugUI.FloatField
			{
				nameAndTooltip = Strings.ValueRangeMin,
				getter = () => panel.data.validationRangeMin,
				setter = delegate(float value)
				{
					panel.data.validationRangeMin = value;
				},
				incStep = 0.01f
			};
		}

		internal static DebugUI.Widget CreatePixelValueRangeMax(SettingsPanel panel)
		{
			return new DebugUI.FloatField
			{
				nameAndTooltip = Strings.ValueRangeMax,
				getter = () => panel.data.validationRangeMax,
				setter = delegate(float value)
				{
					panel.data.validationRangeMax = value;
				},
				incStep = 0.01f
			};
		}
	}

	[DisplayInfo(name = "Rendering", order = 1)]
	internal class SettingsPanel : DebugDisplaySettingsPanel<DebugDisplaySettingsRendering>
	{
		public SettingsPanel(DebugDisplaySettingsRendering data)
			: base(data)
		{
			AddWidget(DebugDisplaySettingsCommon.WidgetFactory.CreateMissingDebugShadersWarning());
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Rendering Debug",
				flags = DebugUI.Flags.FrequentlyUsed,
				isHeader = true,
				opened = true,
				children = 
				{
					WidgetFactory.CreateMapOverlays(this),
					WidgetFactory.CreateMapOverlaySize(this),
					WidgetFactory.CreateHDR(this),
					WidgetFactory.CreateMSAA(this),
					WidgetFactory.CreateTaaDebugMode(this),
					WidgetFactory.CreatePostProcessing(this),
					WidgetFactory.CreateAdditionalWireframeShaderViews(this),
					WidgetFactory.CreateWireframeNotSupportedWarning(this),
					WidgetFactory.CreateOverdrawMode(this),
					WidgetFactory.CreateMaxOverdrawCount(this)
				}
			});
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Pixel Validation",
				isHeader = true,
				opened = true,
				children = 
				{
					WidgetFactory.CreatePixelValidationMode(this),
					(DebugUI.Widget)new DebugUI.Container
					{
						displayName = "Pixel Range Settings",
						isHiddenCallback = () => data.validationMode != DebugValidationMode.HighlightOutsideOfRange,
						children = 
						{
							WidgetFactory.CreatePixelValidationChannels(this),
							WidgetFactory.CreatePixelValueRangeMin(this),
							WidgetFactory.CreatePixelValueRangeMax(this)
						}
					}
				}
			});
		}
	}

	private DebugWireframeMode m_WireframeMode;

	private bool m_Overdraw;

	private DebugOverdrawMode m_OverdrawMode;

	public DebugWireframeMode wireframeMode
	{
		get
		{
			return m_WireframeMode;
		}
		set
		{
			m_WireframeMode = value;
			UpdateDebugSceneOverrideMode();
		}
	}

	[Obsolete("overdraw has been deprecated. Use overdrawMode instead.", false)]
	public bool overdraw
	{
		get
		{
			return m_Overdraw;
		}
		set
		{
			m_Overdraw = value;
			UpdateDebugSceneOverrideMode();
		}
	}

	public DebugOverdrawMode overdrawMode
	{
		get
		{
			return m_OverdrawMode;
		}
		set
		{
			m_OverdrawMode = value;
			UpdateDebugSceneOverrideMode();
		}
	}

	public int maxOverdrawCount { get; set; } = 10;

	public DebugFullScreenMode fullScreenDebugMode { get; set; }

	public int fullScreenDebugModeOutputSizeScreenPercent { get; set; } = 50;

	internal DebugSceneOverrideMode sceneOverrideMode { get; set; }

	internal DebugMipInfoMode mipInfoMode { get; set; }

	public DebugPostProcessingMode postProcessingDebugMode { get; set; } = DebugPostProcessingMode.Auto;

	public bool enableMsaa { get; set; } = true;

	public bool enableHDR { get; set; } = true;

	public TaaDebugMode taaDebugMode { get; set; }

	public DebugValidationMode validationMode { get; set; }

	public PixelValidationChannels validationChannels { get; set; }

	public float validationRangeMin { get; set; }

	public float validationRangeMax { get; set; } = 1f;

	public bool AreAnySettingsActive
	{
		get
		{
			if (postProcessingDebugMode == DebugPostProcessingMode.Auto && fullScreenDebugMode == DebugFullScreenMode.None && sceneOverrideMode == DebugSceneOverrideMode.None && mipInfoMode == DebugMipInfoMode.None && validationMode == DebugValidationMode.None && enableMsaa && enableHDR)
			{
				return taaDebugMode != TaaDebugMode.None;
			}
			return true;
		}
	}

	public bool IsPostProcessingAllowed
	{
		get
		{
			if (postProcessingDebugMode != DebugPostProcessingMode.Disabled && sceneOverrideMode == DebugSceneOverrideMode.None)
			{
				return mipInfoMode == DebugMipInfoMode.None;
			}
			return false;
		}
	}

	public bool IsLightingActive
	{
		get
		{
			if (sceneOverrideMode == DebugSceneOverrideMode.None)
			{
				return mipInfoMode == DebugMipInfoMode.None;
			}
			return false;
		}
	}

	private void UpdateDebugSceneOverrideMode()
	{
		switch (wireframeMode)
		{
		case DebugWireframeMode.Wireframe:
			sceneOverrideMode = DebugSceneOverrideMode.Wireframe;
			break;
		case DebugWireframeMode.SolidWireframe:
			sceneOverrideMode = DebugSceneOverrideMode.SolidWireframe;
			break;
		case DebugWireframeMode.ShadedWireframe:
			sceneOverrideMode = DebugSceneOverrideMode.ShadedWireframe;
			break;
		default:
			sceneOverrideMode = ((overdrawMode != DebugOverdrawMode.None) ? DebugSceneOverrideMode.Overdraw : DebugSceneOverrideMode.None);
			break;
		}
	}

	public bool TryGetScreenClearColor(ref Color color)
	{
		switch (sceneOverrideMode)
		{
		case DebugSceneOverrideMode.None:
		case DebugSceneOverrideMode.ShadedWireframe:
			return false;
		case DebugSceneOverrideMode.Overdraw:
			color = Color.black;
			return true;
		case DebugSceneOverrideMode.Wireframe:
		case DebugSceneOverrideMode.SolidWireframe:
			color = new Color(0.1f, 0.1f, 0.1f, 1f);
			return true;
		default:
			throw new ArgumentOutOfRangeException("color");
		}
	}

	IDebugDisplaySettingsPanelDisposable IDebugDisplaySettingsData.CreatePanel()
	{
		return new SettingsPanel(this);
	}
}
