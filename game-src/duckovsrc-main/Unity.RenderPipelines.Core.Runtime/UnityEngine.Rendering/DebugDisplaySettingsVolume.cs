using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityEngine.Rendering;

public class DebugDisplaySettingsVolume : IDebugDisplaySettingsData, IDebugDisplaySettingsQuery
{
	private static class Styles
	{
		public static readonly GUIContent none = new GUIContent("None");

		public static readonly GUIContent editorCamera = new GUIContent("Editor Camera");
	}

	private static class Strings
	{
		public static readonly string none = "None";

		public static readonly string camera = "Camera";

		public static readonly string parameter = "Parameter";

		public static readonly string component = "Component";

		public static readonly string debugViewNotSupported = "Debug view not supported";

		public static readonly string volumeInfo = "Volume Info";

		public static readonly string interpolatedValue = "Interpolated Value";

		public static readonly string defaultValue = "Default Value";

		public static readonly string global = "Global";

		public static readonly string local = "Local";
	}

	internal static class WidgetFactory
	{
		public static DebugUI.EnumField CreateComponentSelector(SettingsPanel panel, Action<DebugUI.Field<int>, int> refresh)
		{
			int num = 0;
			List<GUIContent> list = new List<GUIContent> { Styles.none };
			List<int> list2 = new List<int> { num++ };
			foreach (var item in panel.data.volumeDebugSettings.volumeComponentsPathAndType)
			{
				GUIContent gUIContent = new GUIContent();
				(gUIContent.text, _) = item;
				list.Add(gUIContent);
				list2.Add(num++);
			}
			return new DebugUI.EnumField
			{
				displayName = Strings.component,
				getter = () => panel.data.volumeDebugSettings.selectedComponent,
				setter = delegate(int value)
				{
					panel.data.volumeDebugSettings.selectedComponent = value;
				},
				enumNames = list.ToArray(),
				enumValues = list2.ToArray(),
				getIndex = () => panel.data.volumeComponentEnumIndex,
				setIndex = delegate(int value)
				{
					panel.data.volumeComponentEnumIndex = value;
				},
				onValueChanged = refresh
			};
		}

		public static DebugUI.ObjectPopupField CreateCameraSelector(SettingsPanel panel, Action<DebugUI.Field<Object>, Object> refresh)
		{
			return new DebugUI.ObjectPopupField
			{
				displayName = Strings.camera,
				getter = () => panel.data.volumeDebugSettings.selectedCamera,
				setter = delegate(Object value)
				{
					Camera[] array = panel.data.volumeDebugSettings.cameras.ToArray();
					panel.data.volumeDebugSettings.selectedCameraIndex = Array.IndexOf(array, value as Camera);
				},
				getObjects = () => panel.data.volumeDebugSettings.cameras,
				onValueChanged = refresh
			};
		}

		private static DebugUI.Widget CreateVolumeParameterWidget(string name, VolumeParameter param, Func<bool> isHiddenCallback = null)
		{
			if (param == null)
			{
				return new DebugUI.Value
				{
					displayName = name,
					getter = () => "-"
				};
			}
			Type parameterType = param.GetType();
			if (parameterType == typeof(ColorParameter))
			{
				ColorParameter p = (ColorParameter)param;
				return new DebugUI.ColorField
				{
					displayName = name,
					hdr = p.hdr,
					showAlpha = p.showAlpha,
					getter = () => p.value,
					setter = delegate(Color value)
					{
						p.value = value;
					},
					isHiddenCallback = isHiddenCallback
				};
			}
			if (parameterType == typeof(BoolParameter))
			{
				BoolParameter p2 = (BoolParameter)param;
				return new DebugUI.BoolField
				{
					displayName = name,
					getter = () => p2.value,
					setter = delegate(bool value)
					{
						p2.value = value;
					},
					isHiddenCallback = isHiddenCallback
				};
			}
			Type[] genericTypeArguments = parameterType.GetTypeInfo().BaseType.GenericTypeArguments;
			if (genericTypeArguments.Length != 0 && genericTypeArguments[0].IsArray)
			{
				return new DebugUI.ObjectListField
				{
					displayName = name,
					getter = () => (Object[])parameterType.GetProperty("value").GetValue(param, null),
					type = parameterType
				};
			}
			PropertyInfo property = param.GetType().GetProperty("value");
			MethodInfo method = property.PropertyType.GetMethod("ToString", Type.EmptyTypes);
			if (method == null || method.DeclaringType == typeof(object) || method.DeclaringType == typeof(Object))
			{
				PropertyInfo nameProp = property.PropertyType.GetProperty("name");
				if (nameProp == null)
				{
					return new DebugUI.Value
					{
						displayName = name,
						getter = () => Strings.debugViewNotSupported
					};
				}
				return new DebugUI.Value
				{
					displayName = name,
					getter = delegate
					{
						object value = property.GetValue(param);
						return (value == null || value.Equals(null)) ? Strings.none : (nameProp.GetValue(value) ?? Strings.none);
					},
					isHiddenCallback = isHiddenCallback
				};
			}
			return new DebugUI.Value
			{
				displayName = name,
				getter = delegate
				{
					object value = property.GetValue(param);
					return (value != null) ? value.ToString() : Strings.none;
				},
				isHiddenCallback = isHiddenCallback
			};
		}

		public static DebugUI.Table CreateVolumeTable(DebugDisplaySettingsVolume data)
		{
			DebugUI.Table table = new DebugUI.Table
			{
				displayName = Strings.parameter,
				isReadOnly = true
			};
			Type selectedType = data.volumeDebugSettings.selectedComponentType;
			if (selectedType == null)
			{
				return table;
			}
			VolumeStack volumeStack = data.volumeDebugSettings.selectedCameraVolumeStack ?? VolumeManager.instance.stack;
			VolumeComponent stackComponent = volumeStack.GetComponent(selectedType);
			if (stackComponent == null)
			{
				return table;
			}
			Volume[] volumes = data.volumeDebugSettings.GetVolumes();
			VolumeComponent inst = (VolumeComponent)ScriptableObject.CreateInstance(selectedType);
			DebugUI.Table.Row row = new DebugUI.Table.Row
			{
				displayName = Strings.volumeInfo,
				opened = true,
				children = { (DebugUI.Widget)new DebugUI.Value
				{
					displayName = Strings.interpolatedValue,
					getter = () => string.Empty
				} }
			};
			DebugUI.Table.Row row2 = new DebugUI.Table.Row
			{
				displayName = "GameObject",
				children = { (DebugUI.Widget)new DebugUI.Value
				{
					getter = () => string.Empty
				} }
			};
			Volume[] array = volumes;
			foreach (Volume volume in array)
			{
				VolumeProfile volumeProfile = (volume.HasInstantiatedProfile() ? volume.profile : volume.sharedProfile);
				row.children.Add(new DebugUI.Value
				{
					displayName = volumeProfile.name,
					getter = delegate
					{
						string obj = (volume.isGlobal ? Strings.global : Strings.local);
						float volumeWeight = data.volumeDebugSettings.GetVolumeWeight(volume);
						return obj + " (" + volumeWeight * 100f + "%)";
					}
				});
				row2.children.Add(new DebugUI.ObjectField
				{
					displayName = volumeProfile.name,
					getter = () => volume
				});
			}
			row.children.Add(new DebugUI.Value
			{
				displayName = Strings.defaultValue,
				getter = () => string.Empty
			});
			table.children.Add(row);
			row2.children.Add(new DebugUI.Value
			{
				getter = () => string.Empty
			});
			table.children.Add(row2);
			List<DebugUI.Table.Row> rows = new List<DebugUI.Table.Row>();
			AddParameterRows(selectedType);
			foreach (DebugUI.Table.Row item in rows.OrderBy((DebugUI.Table.Row t) => t.displayName))
			{
				table.children.Add(item);
			}
			data.volumeDebugSettings.RefreshVolumes(volumes);
			for (int num2 = 0; num2 < volumes.Length; num2++)
			{
				table.SetColumnVisibility(num2 + 1, data.volumeDebugSettings.VolumeHasInfluence(volumes[num2]));
			}
			float timer = 0f;
			float refreshRate = 0.2f;
			table.isHiddenCallback = delegate
			{
				timer += Time.deltaTime;
				if (timer >= refreshRate)
				{
					if (data.volumeDebugSettings.selectedCamera != null)
					{
						Volume[] volumes2 = data.volumeDebugSettings.GetVolumes();
						if (!data.volumeDebugSettings.RefreshVolumes(volumes2))
						{
							for (int i = 0; i < volumes2.Length; i++)
							{
								bool visible = data.volumeDebugSettings.VolumeHasInfluence(volumes2[i]);
								table.SetColumnVisibility(i + 1, visible);
							}
						}
						if (!volumes.SequenceEqual(volumes2))
						{
							volumes = volumes2;
							DebugManager.instance.ReDrawOnScreenDebug();
						}
					}
					timer = 0f;
				}
				return false;
			};
			return table;
			int AddParameterRows(Type type, string baseName = null, int skip = 0)
			{
				foreach (FieldInfo item2 in from t in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
					orderby t.MetadataToken
					select t)
				{
					if (item2.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false).Length != 0)
					{
						skip++;
					}
					else
					{
						Type fieldType = item2.FieldType;
						if (fieldType.IsSubclassOf(typeof(VolumeParameter)))
						{
							AddRow(item2, baseName ?? string.Empty, skip);
						}
						else if (!fieldType.IsArray && fieldType.IsClass)
						{
							skip += AddParameterRows(fieldType, baseName ?? (item2.Name + " "), skip);
						}
					}
				}
				return skip;
			}
			void AddRow(FieldInfo f, string prefix, int skip)
			{
				string displayName = prefix + f.Name;
				DisplayInfoAttribute[] array2 = (DisplayInfoAttribute[])f.GetCustomAttributes(typeof(DisplayInfoAttribute), inherit: true);
				if (array2.Length != 0)
				{
					displayName = prefix + array2[0].name;
				}
				int currentParam = rows.Count + skip;
				row = new DebugUI.Table.Row
				{
					displayName = displayName,
					children = { CreateVolumeParameterWidget(Strings.interpolatedValue, stackComponent.parameterList[currentParam]) }
				};
				Volume[] array3 = volumes;
				foreach (Volume volume2 in array3)
				{
					VolumeParameter param = null;
					VolumeProfile volumeProfile2 = (volume2.HasInstantiatedProfile() ? volume2.profile : volume2.sharedProfile);
					if (volumeProfile2.TryGet<VolumeComponent>(selectedType, out var component))
					{
						param = component.parameterList[currentParam];
					}
					row.children.Add(CreateVolumeParameterWidget(volume2.name + " (" + volumeProfile2.name + ")", param, () => !component.parameterList[currentParam].overrideState));
				}
				row.children.Add(CreateVolumeParameterWidget(Strings.defaultValue, inst.parameterList[currentParam]));
				rows.Add(row);
			}
		}
	}

	[DisplayInfo(name = "Volume", order = int.MaxValue)]
	internal class SettingsPanel : DebugDisplaySettingsPanel<DebugDisplaySettingsVolume>
	{
		private DebugUI.Table m_VolumeTable;

		public SettingsPanel(DebugDisplaySettingsVolume data)
			: base(data)
		{
			AddWidget(WidgetFactory.CreateComponentSelector(this, delegate
			{
				Refresh();
			}));
			AddWidget(WidgetFactory.CreateCameraSelector(this, delegate
			{
				Refresh();
			}));
		}

		private void Refresh()
		{
			DebugUI.Panel panel = DebugManager.instance.GetPanel(PanelName);
			if (panel != null)
			{
				bool flag = false;
				if (m_VolumeTable != null)
				{
					flag = true;
					panel.children.Remove(m_VolumeTable);
				}
				if (m_Data.volumeDebugSettings.selectedComponent > 0 && m_Data.volumeDebugSettings.selectedCamera != null)
				{
					flag = true;
					m_VolumeTable = WidgetFactory.CreateVolumeTable(m_Data);
					AddWidget(m_VolumeTable);
					panel.children.Add(m_VolumeTable);
				}
				if (flag)
				{
					DebugManager.instance.ReDrawOnScreenDebug();
				}
			}
		}
	}

	internal int volumeComponentEnumIndex;

	public IVolumeDebugSettings2 volumeDebugSettings { get; }

	public bool AreAnySettingsActive => false;

	public bool IsPostProcessingAllowed => true;

	public bool IsLightingActive => true;

	public DebugDisplaySettingsVolume(IVolumeDebugSettings2 volumeDebugSettings)
	{
		this.volumeDebugSettings = volumeDebugSettings;
	}

	public bool TryGetScreenClearColor(ref Color color)
	{
		return false;
	}

	public IDebugDisplaySettingsPanelDisposable CreatePanel()
	{
		return new SettingsPanel(this);
	}
}
