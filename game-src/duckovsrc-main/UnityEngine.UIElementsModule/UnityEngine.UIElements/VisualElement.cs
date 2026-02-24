#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Profiling;
using UnityEngine.Assertions;
using UnityEngine.UIElements.Experimental;
using UnityEngine.UIElements.StyleSheets;
using UnityEngine.UIElements.UIR;
using UnityEngine.Yoga;

namespace UnityEngine.UIElements;

public class VisualElement : Focusable, IResolvedStyle, IStylePropertyAnimations, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler
{
	public class UxmlFactory : UxmlFactory<VisualElement, UxmlTraits>
	{
	}

	public class UxmlTraits : UnityEngine.UIElements.UxmlTraits
	{
		protected UxmlStringAttributeDescription m_Name = new UxmlStringAttributeDescription
		{
			name = "name"
		};

		private UxmlStringAttributeDescription m_ViewDataKey = new UxmlStringAttributeDescription
		{
			name = "view-data-key"
		};

		protected UxmlEnumAttributeDescription<PickingMode> m_PickingMode = new UxmlEnumAttributeDescription<PickingMode>
		{
			name = "picking-mode",
			obsoleteNames = new string[1] { "pickingMode" }
		};

		private UxmlStringAttributeDescription m_Tooltip = new UxmlStringAttributeDescription
		{
			name = "tooltip"
		};

		private UxmlEnumAttributeDescription<UsageHints> m_UsageHints = new UxmlEnumAttributeDescription<UsageHints>
		{
			name = "usage-hints"
		};

		private UxmlIntAttributeDescription m_TabIndex = new UxmlIntAttributeDescription
		{
			name = "tabindex",
			defaultValue = 0
		};

		private UxmlStringAttributeDescription m_Class = new UxmlStringAttributeDescription
		{
			name = "class"
		};

		private UxmlStringAttributeDescription m_ContentContainer = new UxmlStringAttributeDescription
		{
			name = "content-container",
			obsoleteNames = new string[1] { "contentContainer" }
		};

		private UxmlStringAttributeDescription m_Style = new UxmlStringAttributeDescription
		{
			name = "style"
		};

		protected UxmlIntAttributeDescription focusIndex { get; set; } = new UxmlIntAttributeDescription
		{
			name = null,
			obsoleteNames = new string[2] { "focus-index", "focusIndex" },
			defaultValue = -1
		};

		protected UxmlBoolAttributeDescription focusable { get; set; } = new UxmlBoolAttributeDescription
		{
			name = "focusable",
			defaultValue = false
		};

		public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
		{
			get
			{
				yield return new UxmlChildElementDescription(typeof(VisualElement));
			}
		}

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			if (ve == null)
			{
				throw new ArgumentNullException("ve");
			}
			ve.name = m_Name.GetValueFromBag(bag, cc);
			ve.viewDataKey = m_ViewDataKey.GetValueFromBag(bag, cc);
			ve.pickingMode = m_PickingMode.GetValueFromBag(bag, cc);
			ve.usageHints = m_UsageHints.GetValueFromBag(bag, cc);
			ve.tooltip = m_Tooltip.GetValueFromBag(bag, cc);
			int value = 0;
			if (focusIndex.TryGetValueFromBag(bag, cc, ref value))
			{
				ve.tabIndex = ((value >= 0) ? value : 0);
				ve.focusable = value >= 0;
			}
			ve.tabIndex = m_TabIndex.GetValueFromBag(bag, cc);
			ve.focusable = focusable.GetValueFromBag(bag, cc);
		}
	}

	public enum MeasureMode
	{
		Undefined,
		Exactly,
		AtMost
	}

	internal enum RenderTargetMode
	{
		None,
		NoColorConversion,
		LinearToGamma,
		GammaToLinear
	}

	public struct Hierarchy
	{
		private const string k_InvalidHierarchyChangeMsg = "Cannot modify VisualElement hierarchy during layout calculation";

		private readonly VisualElement m_Owner;

		public VisualElement parent => m_Owner.m_PhysicalParent;

		internal List<VisualElement> children => m_Owner.m_Children;

		public int childCount => m_Owner.m_Children.Count;

		public VisualElement this[int key] => m_Owner.m_Children[key];

		internal Hierarchy(VisualElement element)
		{
			m_Owner = element;
		}

		public void Add(VisualElement child)
		{
			if (child == null)
			{
				throw new ArgumentException("Cannot add null child");
			}
			Insert(childCount, child);
		}

		public void Insert(int index, VisualElement child)
		{
			if (child == null)
			{
				throw new ArgumentException("Cannot insert null child");
			}
			if (index > childCount)
			{
				throw new ArgumentOutOfRangeException("Index out of range: " + index);
			}
			if (child == m_Owner)
			{
				throw new ArgumentException("Cannot insert element as its own child");
			}
			if (m_Owner.elementPanel != null && m_Owner.elementPanel.duringLayoutPhase)
			{
				throw new InvalidOperationException("Cannot modify VisualElement hierarchy during layout calculation");
			}
			child.RemoveFromHierarchy();
			if (m_Owner.m_Children == s_EmptyList)
			{
				m_Owner.m_Children = VisualElementListPool.Get();
			}
			if (m_Owner.yogaNode.IsMeasureDefined)
			{
				m_Owner.RemoveMeasureFunction();
			}
			PutChildAtIndex(child, index);
			int num = child.imguiContainerDescendantCount + (child.isIMGUIContainer ? 1 : 0);
			if (num > 0)
			{
				m_Owner.ChangeIMGUIContainerCount(num);
			}
			child.hierarchy.SetParent(m_Owner);
			child.PropagateEnabledToChildren(m_Owner.enabledInHierarchy);
			if (child.languageDirection == LanguageDirection.Inherit)
			{
				child.localLanguageDirection = m_Owner.localLanguageDirection;
			}
			child.InvokeHierarchyChanged(HierarchyChangeType.Add);
			child.IncrementVersion(VersionChangeType.Hierarchy);
			m_Owner.IncrementVersion(VersionChangeType.Hierarchy);
		}

		public void Remove(VisualElement child)
		{
			if (child == null)
			{
				throw new ArgumentException("Cannot remove null child");
			}
			if (child.hierarchy.parent != m_Owner)
			{
				throw new ArgumentException("This VisualElement is not my child");
			}
			int index = m_Owner.m_Children.IndexOf(child);
			RemoveAt(index);
		}

		public void RemoveAt(int index)
		{
			if (m_Owner.elementPanel != null && m_Owner.elementPanel.duringLayoutPhase)
			{
				throw new InvalidOperationException("Cannot modify VisualElement hierarchy during layout calculation");
			}
			if (index < 0 || index >= childCount)
			{
				throw new ArgumentOutOfRangeException("Index out of range: " + index);
			}
			VisualElement visualElement = m_Owner.m_Children[index];
			visualElement.InvokeHierarchyChanged(HierarchyChangeType.Remove);
			RemoveChildAtIndex(index);
			int num = visualElement.imguiContainerDescendantCount + (visualElement.isIMGUIContainer ? 1 : 0);
			if (num > 0)
			{
				m_Owner.ChangeIMGUIContainerCount(-num);
			}
			visualElement.hierarchy.SetParent(null);
			if (childCount == 0)
			{
				ReleaseChildList();
				if (m_Owner.requireMeasureFunction)
				{
					m_Owner.AssignMeasureFunction();
				}
			}
			m_Owner.elementPanel?.OnVersionChanged(visualElement, VersionChangeType.Hierarchy);
			m_Owner.IncrementVersion(VersionChangeType.Hierarchy);
		}

		public void Clear()
		{
			if (m_Owner.elementPanel != null && m_Owner.elementPanel.duringLayoutPhase)
			{
				throw new InvalidOperationException("Cannot modify VisualElement hierarchy during layout calculation");
			}
			if (childCount <= 0)
			{
				return;
			}
			List<VisualElement> list = VisualElementListPool.Copy(m_Owner.m_Children);
			ReleaseChildList();
			m_Owner.yogaNode.Clear();
			if (m_Owner.requireMeasureFunction)
			{
				m_Owner.AssignMeasureFunction();
			}
			foreach (VisualElement item in list)
			{
				item.InvokeHierarchyChanged(HierarchyChangeType.Remove);
				item.hierarchy.SetParent(null);
				item.m_LogicalParent = null;
				m_Owner.elementPanel?.OnVersionChanged(item, VersionChangeType.Hierarchy);
			}
			if (m_Owner.imguiContainerDescendantCount > 0)
			{
				int num = m_Owner.imguiContainerDescendantCount;
				if (m_Owner.isIMGUIContainer)
				{
					num--;
				}
				m_Owner.ChangeIMGUIContainerCount(-num);
			}
			VisualElementListPool.Release(list);
			m_Owner.IncrementVersion(VersionChangeType.Hierarchy);
		}

		internal void BringToFront(VisualElement child)
		{
			if (childCount > 1)
			{
				int num = m_Owner.m_Children.IndexOf(child);
				if (num >= 0 && num < childCount - 1)
				{
					MoveChildElement(child, num, childCount);
				}
			}
		}

		internal void SendToBack(VisualElement child)
		{
			if (childCount > 1)
			{
				int num = m_Owner.m_Children.IndexOf(child);
				if (num > 0)
				{
					MoveChildElement(child, num, 0);
				}
			}
		}

		internal void PlaceBehind(VisualElement child, VisualElement over)
		{
			if (childCount <= 0)
			{
				return;
			}
			int num = m_Owner.m_Children.IndexOf(child);
			if (num >= 0)
			{
				int num2 = m_Owner.m_Children.IndexOf(over);
				if (num2 > 0 && num < num2)
				{
					num2--;
				}
				MoveChildElement(child, num, num2);
			}
		}

		internal void PlaceInFront(VisualElement child, VisualElement under)
		{
			if (childCount <= 0)
			{
				return;
			}
			int num = m_Owner.m_Children.IndexOf(child);
			if (num >= 0)
			{
				int num2 = m_Owner.m_Children.IndexOf(under);
				if (num > num2)
				{
					num2++;
				}
				MoveChildElement(child, num, num2);
			}
		}

		private void MoveChildElement(VisualElement child, int currentIndex, int nextIndex)
		{
			if (m_Owner.elementPanel != null && m_Owner.elementPanel.duringLayoutPhase)
			{
				throw new InvalidOperationException("Cannot modify VisualElement hierarchy during layout calculation");
			}
			child.InvokeHierarchyChanged(HierarchyChangeType.Remove);
			RemoveChildAtIndex(currentIndex);
			PutChildAtIndex(child, nextIndex);
			child.InvokeHierarchyChanged(HierarchyChangeType.Add);
			m_Owner.IncrementVersion(VersionChangeType.Hierarchy);
		}

		public int IndexOf(VisualElement element)
		{
			return m_Owner.m_Children.IndexOf(element);
		}

		public VisualElement ElementAt(int index)
		{
			return this[index];
		}

		public IEnumerable<VisualElement> Children()
		{
			return m_Owner.m_Children;
		}

		private void SetParent(VisualElement value)
		{
			m_Owner.m_PhysicalParent = value;
			m_Owner.m_LogicalParent = value;
			m_Owner.DirtyNextParentWithEventCallback();
			m_Owner.SetPanel(value?.elementPanel);
		}

		public void Sort(Comparison<VisualElement> comp)
		{
			if (m_Owner.elementPanel != null && m_Owner.elementPanel.duringLayoutPhase)
			{
				throw new InvalidOperationException("Cannot modify VisualElement hierarchy during layout calculation");
			}
			if (childCount > 1)
			{
				m_Owner.m_Children.Sort(comp);
				m_Owner.yogaNode.Clear();
				for (int i = 0; i < m_Owner.m_Children.Count; i++)
				{
					m_Owner.yogaNode.Insert(i, m_Owner.m_Children[i].yogaNode);
				}
				m_Owner.InvokeHierarchyChanged(HierarchyChangeType.Move);
				m_Owner.IncrementVersion(VersionChangeType.Hierarchy);
			}
		}

		private void PutChildAtIndex(VisualElement child, int index)
		{
			if (index >= childCount)
			{
				m_Owner.m_Children.Add(child);
				m_Owner.yogaNode.Insert(m_Owner.yogaNode.Count, child.yogaNode);
			}
			else
			{
				m_Owner.m_Children.Insert(index, child);
				m_Owner.yogaNode.Insert(index, child.yogaNode);
			}
		}

		private void RemoveChildAtIndex(int index)
		{
			m_Owner.m_Children.RemoveAt(index);
			m_Owner.yogaNode.RemoveAt(index);
		}

		private void ReleaseChildList()
		{
			if (m_Owner.m_Children != s_EmptyList)
			{
				List<VisualElement> elements = m_Owner.m_Children;
				m_Owner.m_Children = s_EmptyList;
				VisualElementListPool.Release(elements);
			}
		}

		public bool Equals(Hierarchy other)
		{
			return other == this;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			return obj is Hierarchy && Equals((Hierarchy)obj);
		}

		public override int GetHashCode()
		{
			return (m_Owner != null) ? m_Owner.GetHashCode() : 0;
		}

		public static bool operator ==(Hierarchy x, Hierarchy y)
		{
			return x.m_Owner == y.m_Owner;
		}

		public static bool operator !=(Hierarchy x, Hierarchy y)
		{
			return !(x == y);
		}
	}

	private abstract class BaseVisualElementScheduledItem : ScheduledItem, IVisualElementScheduledItem, IVisualElementPanelActivatable
	{
		public bool isScheduled = false;

		private VisualElementPanelActivator m_Activator;

		public VisualElement element { get; private set; }

		public bool isActive => m_Activator.isActive;

		protected BaseVisualElementScheduledItem(VisualElement handler)
		{
			element = handler;
			m_Activator = new VisualElementPanelActivator(this);
		}

		public IVisualElementScheduledItem StartingIn(long delayMs)
		{
			base.delayMs = delayMs;
			return this;
		}

		public IVisualElementScheduledItem Until(Func<bool> stopCondition)
		{
			if (stopCondition == null)
			{
				stopCondition = ScheduledItem.ForeverCondition;
			}
			timerUpdateStopCondition = stopCondition;
			return this;
		}

		public IVisualElementScheduledItem ForDuration(long durationMs)
		{
			SetDuration(durationMs);
			return this;
		}

		public IVisualElementScheduledItem Every(long intervalMs)
		{
			base.intervalMs = intervalMs;
			if (timerUpdateStopCondition == ScheduledItem.OnceCondition)
			{
				timerUpdateStopCondition = ScheduledItem.ForeverCondition;
			}
			return this;
		}

		internal override void OnItemUnscheduled()
		{
			base.OnItemUnscheduled();
			isScheduled = false;
			if (!m_Activator.isDetaching)
			{
				m_Activator.SetActive(action: false);
			}
		}

		public void Resume()
		{
			m_Activator.SetActive(action: true);
		}

		public void Pause()
		{
			m_Activator.SetActive(action: false);
		}

		public void ExecuteLater(long delayMs)
		{
			if (!isScheduled)
			{
				Resume();
			}
			ResetStartTime();
			StartingIn(delayMs);
		}

		public void OnPanelActivate()
		{
			if (!isScheduled)
			{
				isScheduled = true;
				ResetStartTime();
				element.elementPanel.scheduler.Schedule(this);
			}
		}

		public void OnPanelDeactivate()
		{
			if (isScheduled)
			{
				isScheduled = false;
				element.elementPanel.scheduler.Unschedule(this);
			}
		}

		public bool CanBeActivated()
		{
			return element != null && element.elementPanel != null && element.elementPanel.scheduler != null;
		}
	}

	private abstract class VisualElementScheduledItem<ActionType> : BaseVisualElementScheduledItem
	{
		public ActionType updateEvent;

		public VisualElementScheduledItem(VisualElement handler, ActionType upEvent)
			: base(handler)
		{
			updateEvent = upEvent;
		}

		public static bool Matches(ScheduledItem item, ActionType updateEvent)
		{
			if (item is VisualElementScheduledItem<ActionType> visualElementScheduledItem)
			{
				return EqualityComparer<ActionType>.Default.Equals(visualElementScheduledItem.updateEvent, updateEvent);
			}
			return false;
		}
	}

	private class TimerStateScheduledItem : VisualElementScheduledItem<Action<TimerState>>
	{
		public TimerStateScheduledItem(VisualElement handler, Action<TimerState> updateEvent)
			: base(handler, updateEvent)
		{
		}

		public override void PerformTimerUpdate(TimerState state)
		{
			if (isScheduled)
			{
				updateEvent(state);
			}
		}
	}

	private class SimpleScheduledItem : VisualElementScheduledItem<Action>
	{
		public SimpleScheduledItem(VisualElement handler, Action updateEvent)
			: base(handler, updateEvent)
		{
		}

		public override void PerformTimerUpdate(TimerState state)
		{
			if (isScheduled)
			{
				updateEvent();
			}
		}
	}

	internal class CustomStyleAccess : ICustomStyle
	{
		private Dictionary<string, StylePropertyValue> m_CustomProperties;

		private float m_DpiScaling;

		public void SetContext(Dictionary<string, StylePropertyValue> customProperties, float dpiScaling)
		{
			m_CustomProperties = customProperties;
			m_DpiScaling = dpiScaling;
		}

		public bool TryGetValue(CustomStyleProperty<float> property, out float value)
		{
			if (TryGetValue(property.name, StyleValueType.Float, out var customProp) && customProp.sheet.TryReadFloat(customProp.handle, out value))
			{
				return true;
			}
			value = 0f;
			return false;
		}

		public bool TryGetValue(CustomStyleProperty<int> property, out int value)
		{
			if (TryGetValue(property.name, StyleValueType.Float, out var customProp) && customProp.sheet.TryReadFloat(customProp.handle, out var value2))
			{
				value = (int)value2;
				return true;
			}
			value = 0;
			return false;
		}

		public bool TryGetValue(CustomStyleProperty<bool> property, out bool value)
		{
			if (m_CustomProperties != null && m_CustomProperties.TryGetValue(property.name, out var value2))
			{
				value = value2.sheet.ReadKeyword(value2.handle) == StyleValueKeyword.True;
				return true;
			}
			value = false;
			return false;
		}

		public bool TryGetValue(CustomStyleProperty<Color> property, out Color value)
		{
			if (m_CustomProperties != null && m_CustomProperties.TryGetValue(property.name, out var value2))
			{
				StyleValueHandle handle = value2.handle;
				switch (handle.valueType)
				{
				case StyleValueType.Enum:
				{
					string text = value2.sheet.ReadAsString(handle);
					return StyleSheetColor.TryGetColor(text.ToLowerInvariant(), out value);
				}
				case StyleValueType.Color:
					if (value2.sheet.TryReadColor(value2.handle, out value))
					{
						return true;
					}
					break;
				default:
					LogCustomPropertyWarning(property.name, StyleValueType.Color, value2);
					break;
				}
			}
			value = Color.clear;
			return false;
		}

		public bool TryGetValue(CustomStyleProperty<Texture2D> property, out Texture2D value)
		{
			if (m_CustomProperties != null && m_CustomProperties.TryGetValue(property.name, out var value2))
			{
				ImageSource source = default(ImageSource);
				if (StylePropertyReader.TryGetImageSourceFromValue(value2, m_DpiScaling, out source) && source.texture != null)
				{
					value = source.texture;
					return true;
				}
			}
			value = null;
			return false;
		}

		public bool TryGetValue(CustomStyleProperty<Sprite> property, out Sprite value)
		{
			if (m_CustomProperties != null && m_CustomProperties.TryGetValue(property.name, out var value2))
			{
				ImageSource source = default(ImageSource);
				if (StylePropertyReader.TryGetImageSourceFromValue(value2, m_DpiScaling, out source) && source.sprite != null)
				{
					value = source.sprite;
					return true;
				}
			}
			value = null;
			return false;
		}

		public bool TryGetValue(CustomStyleProperty<VectorImage> property, out VectorImage value)
		{
			if (m_CustomProperties != null && m_CustomProperties.TryGetValue(property.name, out var value2))
			{
				ImageSource source = default(ImageSource);
				if (StylePropertyReader.TryGetImageSourceFromValue(value2, m_DpiScaling, out source) && source.vectorImage != null)
				{
					value = source.vectorImage;
					return true;
				}
			}
			value = null;
			return false;
		}

		public bool TryGetValue<T>(CustomStyleProperty<T> property, out T value) where T : Object
		{
			if (m_CustomProperties != null && m_CustomProperties.TryGetValue(property.name, out var value2) && value2.sheet.TryReadAssetReference(value2.handle, out var value3))
			{
				value = value3 as T;
				return value != null;
			}
			value = null;
			return false;
		}

		public bool TryGetValue(CustomStyleProperty<string> property, out string value)
		{
			if (m_CustomProperties != null && m_CustomProperties.TryGetValue(property.name, out var value2))
			{
				value = value2.sheet.ReadAsString(value2.handle);
				return true;
			}
			value = string.Empty;
			return false;
		}

		private bool TryGetValue(string propertyName, StyleValueType valueType, out StylePropertyValue customProp)
		{
			customProp = default(StylePropertyValue);
			if (m_CustomProperties != null && m_CustomProperties.TryGetValue(propertyName, out customProp))
			{
				StyleValueHandle handle = customProp.handle;
				if (handle.valueType != valueType)
				{
					LogCustomPropertyWarning(propertyName, valueType, customProp);
					return false;
				}
				return true;
			}
			return false;
		}

		private static void LogCustomPropertyWarning(string propertyName, StyleValueType valueType, StylePropertyValue customProp)
		{
			Debug.LogWarning($"Trying to read custom property {propertyName} value as {valueType} while parsed type is {customProp.handle.valueType}");
		}
	}

	internal class TypeData
	{
		private string m_FullTypeName = string.Empty;

		private string m_TypeName = string.Empty;

		private string m_TypeNamespace = string.Empty;

		public Type type { get; }

		public string fullTypeName
		{
			get
			{
				if (string.IsNullOrEmpty(m_FullTypeName))
				{
					m_FullTypeName = type.FullName;
				}
				return m_FullTypeName;
			}
		}

		public string typeName
		{
			get
			{
				if (string.IsNullOrEmpty(m_TypeName))
				{
					bool isGenericType = type.IsGenericType;
					m_TypeName = type.Name;
					if (isGenericType)
					{
						int num = m_TypeName.IndexOf('`');
						if (num >= 0)
						{
							m_TypeName = m_TypeName.Remove(num);
						}
					}
				}
				return m_TypeName;
			}
		}

		public string typeNamespace
		{
			get
			{
				if (string.IsNullOrEmpty(m_TypeNamespace))
				{
					m_TypeNamespace = type.Namespace;
				}
				return m_TypeNamespace;
			}
		}

		public TypeData(Type type)
		{
			this.type = type;
		}
	}

	private static uint s_NextId;

	private static List<string> s_EmptyClassList = new List<string>(0);

	internal static readonly PropertyName userDataPropertyKey = new PropertyName("--unity-user-data");

	public static readonly string disabledUssClassName = "unity-disabled";

	private string m_Name;

	private List<string> m_ClassList;

	private List<KeyValuePair<PropertyName, object>> m_PropertyBag;

	internal VisualElementFlags m_Flags;

	private string m_ViewDataKey;

	private RenderHints m_RenderHints;

	internal Rect lastLayout;

	internal Rect lastPseudoPadding;

	internal RenderChainVEData renderChainData;

	private Rect m_Layout;

	private Rect m_BoundingBox;

	private const VisualElementFlags worldBoundingBoxDirtyDependencies = VisualElementFlags.WorldTransformDirty | VisualElementFlags.BoundingBoxDirty | VisualElementFlags.WorldBoundingBoxDirty;

	private Rect m_WorldBoundingBox;

	private const VisualElementFlags worldTransformInverseDirtyDependencies = VisualElementFlags.WorldTransformDirty | VisualElementFlags.WorldTransformInverseDirty;

	private Matrix4x4 m_WorldTransformCache = Matrix4x4.identity;

	private Matrix4x4 m_WorldTransformInverseCache = Matrix4x4.identity;

	private Rect m_WorldClip = Rect.zero;

	private Rect m_WorldClipMinusGroup = Rect.zero;

	private bool m_WorldClipIsInfinite = false;

	internal static readonly Rect s_InfiniteRect = new Rect(-10000f, -10000f, 40000f, 40000f);

	internal PseudoStates triggerPseudoMask;

	internal PseudoStates dependencyPseudoMask;

	private PseudoStates m_PseudoStates;

	private PickingMode m_PickingMode;

	internal ComputedStyle m_Style = InitialStyle.Acquire();

	internal StyleVariableContext variableContext = StyleVariableContext.none;

	internal int inheritedStylesHash = 0;

	internal readonly uint controlid;

	internal int imguiContainerDescendantCount = 0;

	private LanguageDirection m_LanguageDirection;

	private LanguageDirection m_LocalLanguageDirection;

	private static readonly ProfilerMarker k_GenerateVisualContentMarker = new ProfilerMarker("GenerateVisualContent");

	private RenderTargetMode m_SubRenderTargetMode = RenderTargetMode.None;

	private static Material s_runtimeMaterial;

	private Material m_defaultMaterial;

	private List<IValueAnimationUpdate> m_RunningAnimations;

	private static uint s_NextParentVersion;

	private uint m_NextParentCachedVersion;

	private uint m_NextParentRequiredVersion;

	private VisualElement m_CachedNextParentWithEventCallback;

	private int m_EventCallbackCategories = 0;

	private int m_CachedEventCallbackParentCategories = 0;

	private readonly int m_DefaultActionEventCategories;

	private readonly int m_DefaultActionAtTargetEventCategories;

	internal const string k_RootVisualContainerName = "rootVisualContainer";

	private VisualElement m_PhysicalParent;

	private VisualElement m_LogicalParent;

	private static readonly List<VisualElement> s_EmptyList = new List<VisualElement>();

	private List<VisualElement> m_Children;

	private VisualTreeAsset m_VisualTreeAssetSource = null;

	internal static CustomStyleAccess s_CustomStyleAccess = new CustomStyleAccess();

	internal InlineStyleAccess inlineStyleAccess;

	internal List<StyleSheet> styleSheetList;

	private static readonly Regex s_InternalStyleSheetPath = new Regex("^instanceId:[-0-9]+$", RegexOptions.Compiled);

	internal static readonly PropertyName tooltipPropertyKey = new PropertyName("--unity-tooltip");

	private static readonly Dictionary<Type, TypeData> s_TypeData = new Dictionary<Type, TypeData>();

	private TypeData m_TypeData;

	public IResolvedStyle resolvedStyle => this;

	Align IResolvedStyle.alignContent => computedStyle.alignContent;

	Align IResolvedStyle.alignItems => computedStyle.alignItems;

	Align IResolvedStyle.alignSelf => computedStyle.alignSelf;

	Color IResolvedStyle.backgroundColor => computedStyle.backgroundColor;

	Background IResolvedStyle.backgroundImage => computedStyle.backgroundImage;

	BackgroundPosition IResolvedStyle.backgroundPositionX => computedStyle.backgroundPositionX;

	BackgroundPosition IResolvedStyle.backgroundPositionY => computedStyle.backgroundPositionY;

	BackgroundRepeat IResolvedStyle.backgroundRepeat => computedStyle.backgroundRepeat;

	BackgroundSize IResolvedStyle.backgroundSize => computedStyle.backgroundSize;

	Color IResolvedStyle.borderBottomColor => computedStyle.borderBottomColor;

	float IResolvedStyle.borderBottomLeftRadius => computedStyle.borderBottomLeftRadius.value;

	float IResolvedStyle.borderBottomRightRadius => computedStyle.borderBottomRightRadius.value;

	float IResolvedStyle.borderBottomWidth => yogaNode.LayoutBorderBottom;

	Color IResolvedStyle.borderLeftColor => computedStyle.borderLeftColor;

	float IResolvedStyle.borderLeftWidth => yogaNode.LayoutBorderLeft;

	Color IResolvedStyle.borderRightColor => computedStyle.borderRightColor;

	float IResolvedStyle.borderRightWidth => yogaNode.LayoutBorderRight;

	Color IResolvedStyle.borderTopColor => computedStyle.borderTopColor;

	float IResolvedStyle.borderTopLeftRadius => computedStyle.borderTopLeftRadius.value;

	float IResolvedStyle.borderTopRightRadius => computedStyle.borderTopRightRadius.value;

	float IResolvedStyle.borderTopWidth => yogaNode.LayoutBorderTop;

	float IResolvedStyle.bottom => yogaNode.LayoutBottom;

	Color IResolvedStyle.color => computedStyle.color;

	DisplayStyle IResolvedStyle.display => computedStyle.display;

	StyleFloat IResolvedStyle.flexBasis => new StyleFloat(yogaNode.ComputedFlexBasis);

	FlexDirection IResolvedStyle.flexDirection => computedStyle.flexDirection;

	float IResolvedStyle.flexGrow => computedStyle.flexGrow;

	float IResolvedStyle.flexShrink => computedStyle.flexShrink;

	Wrap IResolvedStyle.flexWrap => computedStyle.flexWrap;

	float IResolvedStyle.fontSize => computedStyle.fontSize.value;

	float IResolvedStyle.height => yogaNode.LayoutHeight;

	Justify IResolvedStyle.justifyContent => computedStyle.justifyContent;

	float IResolvedStyle.left => yogaNode.LayoutX;

	float IResolvedStyle.letterSpacing => computedStyle.letterSpacing.value;

	float IResolvedStyle.marginBottom => yogaNode.LayoutMarginBottom;

	float IResolvedStyle.marginLeft => yogaNode.LayoutMarginLeft;

	float IResolvedStyle.marginRight => yogaNode.LayoutMarginRight;

	float IResolvedStyle.marginTop => yogaNode.LayoutMarginTop;

	StyleFloat IResolvedStyle.maxHeight => ResolveLengthValue(computedStyle.maxHeight, isRow: false);

	StyleFloat IResolvedStyle.maxWidth => ResolveLengthValue(computedStyle.maxWidth, isRow: true);

	StyleFloat IResolvedStyle.minHeight => ResolveLengthValue(computedStyle.minHeight, isRow: false);

	StyleFloat IResolvedStyle.minWidth => ResolveLengthValue(computedStyle.minWidth, isRow: true);

	float IResolvedStyle.opacity => computedStyle.opacity;

	float IResolvedStyle.paddingBottom => yogaNode.LayoutPaddingBottom;

	float IResolvedStyle.paddingLeft => yogaNode.LayoutPaddingLeft;

	float IResolvedStyle.paddingRight => yogaNode.LayoutPaddingRight;

	float IResolvedStyle.paddingTop => yogaNode.LayoutPaddingTop;

	Position IResolvedStyle.position => computedStyle.position;

	float IResolvedStyle.right => yogaNode.LayoutRight;

	Rotate IResolvedStyle.rotate => computedStyle.rotate;

	Scale IResolvedStyle.scale => computedStyle.scale;

	TextOverflow IResolvedStyle.textOverflow => computedStyle.textOverflow;

	float IResolvedStyle.top => yogaNode.LayoutY;

	Vector3 IResolvedStyle.transformOrigin => ResolveTransformOrigin();

	IEnumerable<TimeValue> IResolvedStyle.transitionDelay => computedStyle.transitionDelay;

	IEnumerable<TimeValue> IResolvedStyle.transitionDuration => computedStyle.transitionDuration;

	IEnumerable<StylePropertyName> IResolvedStyle.transitionProperty => computedStyle.transitionProperty;

	IEnumerable<EasingFunction> IResolvedStyle.transitionTimingFunction => computedStyle.transitionTimingFunction;

	Vector3 IResolvedStyle.translate => ResolveTranslate();

	Color IResolvedStyle.unityBackgroundImageTintColor => computedStyle.unityBackgroundImageTintColor;

	Font IResolvedStyle.unityFont => computedStyle.unityFont;

	FontDefinition IResolvedStyle.unityFontDefinition => computedStyle.unityFontDefinition;

	FontStyle IResolvedStyle.unityFontStyleAndWeight => computedStyle.unityFontStyleAndWeight;

	float IResolvedStyle.unityParagraphSpacing => computedStyle.unityParagraphSpacing.value;

	int IResolvedStyle.unitySliceBottom => computedStyle.unitySliceBottom;

	int IResolvedStyle.unitySliceLeft => computedStyle.unitySliceLeft;

	int IResolvedStyle.unitySliceRight => computedStyle.unitySliceRight;

	float IResolvedStyle.unitySliceScale => computedStyle.unitySliceScale;

	int IResolvedStyle.unitySliceTop => computedStyle.unitySliceTop;

	TextAnchor IResolvedStyle.unityTextAlign => computedStyle.unityTextAlign;

	Color IResolvedStyle.unityTextOutlineColor => computedStyle.unityTextOutlineColor;

	float IResolvedStyle.unityTextOutlineWidth => computedStyle.unityTextOutlineWidth;

	TextOverflowPosition IResolvedStyle.unityTextOverflowPosition => computedStyle.unityTextOverflowPosition;

	Visibility IResolvedStyle.visibility => computedStyle.visibility;

	WhiteSpace IResolvedStyle.whiteSpace => computedStyle.whiteSpace;

	float IResolvedStyle.width => yogaNode.LayoutWidth;

	float IResolvedStyle.wordSpacing => computedStyle.wordSpacing.value;

	internal bool hasRunningAnimations => styleAnimation.runningAnimationCount > 0;

	internal bool hasCompletedAnimations => styleAnimation.completedAnimationCount > 0;

	int IStylePropertyAnimations.runningAnimationCount { get; set; }

	int IStylePropertyAnimations.completedAnimationCount { get; set; }

	internal IStylePropertyAnimations styleAnimation => this;

	internal bool isCompositeRoot
	{
		get
		{
			return (m_Flags & VisualElementFlags.CompositeRoot) == VisualElementFlags.CompositeRoot;
		}
		set
		{
			m_Flags = (value ? (m_Flags | VisualElementFlags.CompositeRoot) : (m_Flags & ~VisualElementFlags.CompositeRoot));
			if (value)
			{
				SetAsNextParentWithEventCallback();
			}
		}
	}

	internal bool isHierarchyDisplayed
	{
		get
		{
			return (m_Flags & VisualElementFlags.HierarchyDisplayed) == VisualElementFlags.HierarchyDisplayed;
		}
		set
		{
			m_Flags = (value ? (m_Flags | VisualElementFlags.HierarchyDisplayed) : (m_Flags & ~VisualElementFlags.HierarchyDisplayed));
		}
	}

	public string viewDataKey
	{
		get
		{
			return m_ViewDataKey;
		}
		set
		{
			if (m_ViewDataKey != value)
			{
				m_ViewDataKey = value;
				if (!string.IsNullOrEmpty(value))
				{
					IncrementVersion(VersionChangeType.ViewData);
				}
			}
		}
	}

	internal bool enableViewDataPersistence
	{
		get
		{
			return (m_Flags & VisualElementFlags.EnableViewDataPersistence) == VisualElementFlags.EnableViewDataPersistence;
		}
		private set
		{
			m_Flags = (value ? (m_Flags | VisualElementFlags.EnableViewDataPersistence) : (m_Flags & ~VisualElementFlags.EnableViewDataPersistence));
		}
	}

	public object userData
	{
		get
		{
			TryGetPropertyInternal(userDataPropertyKey, out var value);
			return value;
		}
		set
		{
			SetPropertyInternal(userDataPropertyKey, value);
		}
	}

	public override bool canGrabFocus
	{
		get
		{
			bool flag = false;
			for (VisualElement visualElement = hierarchy.parent; visualElement != null; visualElement = visualElement.parent)
			{
				if (visualElement.isCompositeRoot)
				{
					flag |= !visualElement.canGrabFocus;
					break;
				}
			}
			return !flag && visible && resolvedStyle.display != DisplayStyle.None && enabledInHierarchy && base.canGrabFocus;
		}
	}

	public override FocusController focusController => panel?.focusController;

	public UsageHints usageHints
	{
		get
		{
			return (UsageHints)((((renderHints & RenderHints.GroupTransform) != RenderHints.None) ? 2 : 0) | (((renderHints & RenderHints.BoneTransform) != RenderHints.None) ? 1 : 0) | (((renderHints & RenderHints.MaskContainer) != RenderHints.None) ? 4 : 0) | (((renderHints & RenderHints.DynamicColor) != RenderHints.None) ? 8 : 0));
		}
		set
		{
			if ((value & UsageHints.GroupTransform) != UsageHints.None)
			{
				renderHints |= RenderHints.GroupTransform;
			}
			else
			{
				renderHints &= ~RenderHints.GroupTransform;
			}
			if ((value & UsageHints.DynamicTransform) != UsageHints.None)
			{
				renderHints |= RenderHints.BoneTransform;
			}
			else
			{
				renderHints &= ~RenderHints.BoneTransform;
			}
			if ((value & UsageHints.MaskContainer) != UsageHints.None)
			{
				renderHints |= RenderHints.MaskContainer;
			}
			else
			{
				renderHints &= ~RenderHints.MaskContainer;
			}
			if ((value & UsageHints.DynamicColor) != UsageHints.None)
			{
				renderHints |= RenderHints.DynamicColor;
			}
			else
			{
				renderHints &= ~RenderHints.DynamicColor;
			}
		}
	}

	internal RenderHints renderHints
	{
		get
		{
			return m_RenderHints;
		}
		set
		{
			RenderHints renderHints = m_RenderHints & ~RenderHints.DirtyAll;
			RenderHints renderHints2 = value & ~RenderHints.DirtyAll;
			RenderHints renderHints3 = renderHints ^ renderHints2;
			if (renderHints3 != RenderHints.None)
			{
				RenderHints renderHints4 = m_RenderHints & RenderHints.DirtyAll;
				RenderHints renderHints5 = (RenderHints)((int)renderHints3 << 5);
				m_RenderHints = renderHints2 | renderHints4 | renderHints5;
				IncrementVersion(VersionChangeType.RenderHints);
			}
		}
	}

	public ITransform transform => this;

	Vector3 ITransform.position
	{
		get
		{
			return resolvedStyle.translate;
		}
		set
		{
			style.translate = new Translate(value.x, value.y, value.z);
		}
	}

	Quaternion ITransform.rotation
	{
		get
		{
			return resolvedStyle.rotate.ToQuaternion();
		}
		set
		{
			value.ToAngleAxis(out var angle, out var axis);
			style.rotate = new Rotate(angle, axis);
		}
	}

	Vector3 ITransform.scale
	{
		get
		{
			return resolvedStyle.scale.value;
		}
		set
		{
			style.scale = new Scale((Vector2)value);
		}
	}

	Matrix4x4 ITransform.matrix => Matrix4x4.TRS(resolvedStyle.translate, resolvedStyle.rotate.ToQuaternion(), resolvedStyle.scale.value);

	internal bool isLayoutManual
	{
		get
		{
			return (m_Flags & VisualElementFlags.LayoutManual) == VisualElementFlags.LayoutManual;
		}
		private set
		{
			m_Flags = (value ? (m_Flags | VisualElementFlags.LayoutManual) : (m_Flags & ~VisualElementFlags.LayoutManual));
		}
	}

	internal float scaledPixelsPerPoint => elementPanel?.scaledPixelsPerPoint ?? GUIUtility.pixelsPerPoint;

	StyleEnum<ScaleMode> IResolvedStyle.unityBackgroundScaleMode
	{
		get
		{
			bool valid;
			return BackgroundPropertyHelper.ResolveUnityBackgroundScaleMode(computedStyle.backgroundPositionX, computedStyle.backgroundPositionY, computedStyle.backgroundRepeat, computedStyle.backgroundSize, out valid);
		}
	}

	public Rect layout
	{
		get
		{
			Rect result = m_Layout;
			if (yogaNode != null && !isLayoutManual)
			{
				result.x = yogaNode.LayoutX;
				result.y = yogaNode.LayoutY;
				result.width = yogaNode.LayoutWidth;
				result.height = yogaNode.LayoutHeight;
			}
			return result;
		}
		internal set
		{
			if (yogaNode == null)
			{
				yogaNode = new YogaNode();
			}
			if (!isLayoutManual || !(m_Layout == value))
			{
				Rect rect = layout;
				VersionChangeType versionChangeType = (VersionChangeType)0;
				if (!Mathf.Approximately(rect.x, value.x) || !Mathf.Approximately(rect.y, value.y))
				{
					versionChangeType |= VersionChangeType.Transform;
				}
				if (!Mathf.Approximately(rect.width, value.width) || !Mathf.Approximately(rect.height, value.height))
				{
					versionChangeType |= VersionChangeType.Size;
				}
				m_Layout = value;
				isLayoutManual = true;
				IStyle style = this.style;
				style.position = Position.Absolute;
				style.marginLeft = 0f;
				style.marginRight = 0f;
				style.marginBottom = 0f;
				style.marginTop = 0f;
				style.left = value.x;
				style.top = value.y;
				style.right = float.NaN;
				style.bottom = float.NaN;
				style.width = value.width;
				style.height = value.height;
				if (versionChangeType != 0)
				{
					IncrementVersion(versionChangeType);
				}
			}
		}
	}

	public Rect contentRect
	{
		get
		{
			Spacing spacing = new Spacing(resolvedStyle.paddingLeft, resolvedStyle.paddingTop, resolvedStyle.paddingRight, resolvedStyle.paddingBottom);
			return paddingRect - spacing;
		}
	}

	protected Rect paddingRect
	{
		get
		{
			Spacing spacing = new Spacing(resolvedStyle.borderLeftWidth, resolvedStyle.borderTopWidth, resolvedStyle.borderRightWidth, resolvedStyle.borderBottomWidth);
			return rect - spacing;
		}
	}

	internal bool isBoundingBoxDirty
	{
		get
		{
			return (m_Flags & VisualElementFlags.BoundingBoxDirty) == VisualElementFlags.BoundingBoxDirty;
		}
		set
		{
			m_Flags = (value ? (m_Flags | VisualElementFlags.BoundingBoxDirty) : (m_Flags & ~VisualElementFlags.BoundingBoxDirty));
		}
	}

	internal bool isWorldBoundingBoxDirty
	{
		get
		{
			return (m_Flags & VisualElementFlags.WorldBoundingBoxDirty) == VisualElementFlags.WorldBoundingBoxDirty;
		}
		set
		{
			m_Flags = (value ? (m_Flags | VisualElementFlags.WorldBoundingBoxDirty) : (m_Flags & ~VisualElementFlags.WorldBoundingBoxDirty));
		}
	}

	internal bool isWorldBoundingBoxOrDependenciesDirty => (m_Flags & (VisualElementFlags.WorldTransformDirty | VisualElementFlags.BoundingBoxDirty | VisualElementFlags.WorldBoundingBoxDirty)) != 0;

	internal Rect boundingBox
	{
		get
		{
			if (isBoundingBoxDirty)
			{
				UpdateBoundingBox();
				isBoundingBoxDirty = false;
			}
			return m_BoundingBox;
		}
	}

	internal Rect worldBoundingBox
	{
		get
		{
			if (isWorldBoundingBoxOrDependenciesDirty)
			{
				UpdateWorldBoundingBox();
				isWorldBoundingBoxDirty = false;
			}
			return m_WorldBoundingBox;
		}
	}

	private Rect boundingBoxInParentSpace
	{
		get
		{
			Rect result = boundingBox;
			TransformAlignedRectToParentSpace(ref result);
			return result;
		}
	}

	public Rect worldBound
	{
		get
		{
			Rect result = rect;
			TransformAlignedRect(ref worldTransformRef, ref result);
			return result;
		}
	}

	public Rect localBound
	{
		get
		{
			Rect result = rect;
			TransformAlignedRectToParentSpace(ref result);
			return result;
		}
	}

	internal Rect rect
	{
		get
		{
			Rect rect = layout;
			return new Rect(0f, 0f, rect.width, rect.height);
		}
	}

	internal bool isWorldTransformDirty
	{
		get
		{
			return (m_Flags & VisualElementFlags.WorldTransformDirty) == VisualElementFlags.WorldTransformDirty;
		}
		set
		{
			m_Flags = (value ? (m_Flags | VisualElementFlags.WorldTransformDirty) : (m_Flags & ~VisualElementFlags.WorldTransformDirty));
		}
	}

	internal bool isWorldTransformInverseDirty
	{
		get
		{
			return (m_Flags & VisualElementFlags.WorldTransformInverseDirty) == VisualElementFlags.WorldTransformInverseDirty;
		}
		set
		{
			m_Flags = (value ? (m_Flags | VisualElementFlags.WorldTransformInverseDirty) : (m_Flags & ~VisualElementFlags.WorldTransformInverseDirty));
		}
	}

	internal bool isWorldTransformInverseOrDependenciesDirty => (m_Flags & (VisualElementFlags.WorldTransformDirty | VisualElementFlags.WorldTransformInverseDirty)) != 0;

	public Matrix4x4 worldTransform
	{
		get
		{
			if (isWorldTransformDirty)
			{
				UpdateWorldTransform();
			}
			return m_WorldTransformCache;
		}
	}

	internal ref Matrix4x4 worldTransformRef
	{
		get
		{
			if (isWorldTransformDirty)
			{
				UpdateWorldTransform();
			}
			return ref m_WorldTransformCache;
		}
	}

	internal ref Matrix4x4 worldTransformInverse
	{
		get
		{
			if (isWorldTransformInverseOrDependenciesDirty)
			{
				UpdateWorldTransformInverse();
			}
			return ref m_WorldTransformInverseCache;
		}
	}

	internal bool isWorldClipDirty
	{
		get
		{
			return (m_Flags & VisualElementFlags.WorldClipDirty) == VisualElementFlags.WorldClipDirty;
		}
		set
		{
			m_Flags = (value ? (m_Flags | VisualElementFlags.WorldClipDirty) : (m_Flags & ~VisualElementFlags.WorldClipDirty));
		}
	}

	internal Rect worldClip
	{
		get
		{
			if (isWorldClipDirty)
			{
				UpdateWorldClip();
				isWorldClipDirty = false;
			}
			return m_WorldClip;
		}
	}

	internal Rect worldClipMinusGroup
	{
		get
		{
			if (isWorldClipDirty)
			{
				UpdateWorldClip();
				isWorldClipDirty = false;
			}
			return m_WorldClipMinusGroup;
		}
	}

	internal bool worldClipIsInfinite
	{
		get
		{
			if (isWorldClipDirty)
			{
				UpdateWorldClip();
				isWorldClipDirty = false;
			}
			return m_WorldClipIsInfinite;
		}
	}

	internal PseudoStates pseudoStates
	{
		get
		{
			return m_PseudoStates;
		}
		set
		{
			PseudoStates pseudoStates = m_PseudoStates ^ value;
			if (pseudoStates <= (PseudoStates)0)
			{
				return;
			}
			if ((value & PseudoStates.Root) == PseudoStates.Root)
			{
				isRootVisualContainer = true;
			}
			if (pseudoStates != PseudoStates.Root)
			{
				PseudoStates pseudoStates2 = pseudoStates & value;
				PseudoStates pseudoStates3 = pseudoStates & m_PseudoStates;
				if ((triggerPseudoMask & pseudoStates2) != 0 || (dependencyPseudoMask & pseudoStates3) != 0)
				{
					IncrementVersion(VersionChangeType.StyleSheet);
				}
			}
			m_PseudoStates = value;
		}
	}

	internal int containedPointerIds { get; private set; }

	public PickingMode pickingMode
	{
		get
		{
			return m_PickingMode;
		}
		set
		{
			if (m_PickingMode != value)
			{
				m_PickingMode = value;
				IncrementVersion(VersionChangeType.Picking);
			}
		}
	}

	public string name
	{
		get
		{
			return m_Name;
		}
		set
		{
			if (!(m_Name == value))
			{
				m_Name = value;
				IncrementVersion(VersionChangeType.StyleSheet);
			}
		}
	}

	internal List<string> classList
	{
		get
		{
			if (m_ClassList == s_EmptyClassList)
			{
				m_ClassList = ObjectListPool<string>.Get();
			}
			return m_ClassList;
		}
	}

	internal string fullTypeName => typeData.fullTypeName;

	internal string typeName => typeData.typeName;

	internal YogaNode yogaNode { get; private set; }

	internal ref ComputedStyle computedStyle => ref m_Style;

	internal bool hasInlineStyle => inlineStyleAccess != null;

	internal bool styleInitialized
	{
		get
		{
			return (m_Flags & VisualElementFlags.StyleInitialized) == VisualElementFlags.StyleInitialized;
		}
		set
		{
			m_Flags = (value ? (m_Flags | VisualElementFlags.StyleInitialized) : (m_Flags & ~VisualElementFlags.StyleInitialized));
		}
	}

	internal float opacity
	{
		get
		{
			return resolvedStyle.opacity;
		}
		set
		{
			style.opacity = value;
		}
	}

	private bool isParentEnabledInHierarchy => hierarchy.parent == null || hierarchy.parent.enabledInHierarchy;

	public bool enabledInHierarchy => (pseudoStates & PseudoStates.Disabled) != PseudoStates.Disabled;

	public bool enabledSelf { get; private set; }

	public LanguageDirection languageDirection
	{
		get
		{
			return m_LanguageDirection;
		}
		set
		{
			if (m_LanguageDirection != value)
			{
				m_LanguageDirection = value;
				localLanguageDirection = m_LanguageDirection;
			}
		}
	}

	internal LanguageDirection localLanguageDirection
	{
		get
		{
			return m_LocalLanguageDirection;
		}
		set
		{
			if (m_LocalLanguageDirection == value)
			{
				return;
			}
			m_LocalLanguageDirection = value;
			IncrementVersion(VersionChangeType.Layout);
			int count = m_Children.Count;
			for (int i = 0; i < count; i++)
			{
				if (m_Children[i].languageDirection == LanguageDirection.Inherit)
				{
					m_Children[i].localLanguageDirection = m_LocalLanguageDirection;
				}
			}
		}
	}

	public bool visible
	{
		get
		{
			return resolvedStyle.visibility == Visibility.Visible;
		}
		set
		{
			style.visibility = ((!value) ? Visibility.Hidden : Visibility.Visible);
		}
	}

	public Action<MeshGenerationContext> generateVisualContent { get; set; }

	internal bool requireMeasureFunction
	{
		get
		{
			return (m_Flags & VisualElementFlags.RequireMeasureFunction) == VisualElementFlags.RequireMeasureFunction;
		}
		set
		{
			m_Flags = (value ? (m_Flags | VisualElementFlags.RequireMeasureFunction) : (m_Flags & ~VisualElementFlags.RequireMeasureFunction));
			if (value && !yogaNode.IsMeasureDefined)
			{
				AssignMeasureFunction();
			}
			else if (!value && yogaNode.IsMeasureDefined)
			{
				RemoveMeasureFunction();
			}
		}
	}

	internal RenderTargetMode subRenderTargetMode
	{
		get
		{
			return m_SubRenderTargetMode;
		}
		set
		{
			if (m_SubRenderTargetMode != value)
			{
				Debug.Assert(Application.isEditor, "subRenderTargetMode is not supported on runtime yet");
				m_SubRenderTargetMode = value;
				IncrementVersion(VersionChangeType.Repaint);
			}
		}
	}

	internal Material defaultMaterial
	{
		get
		{
			return m_defaultMaterial;
		}
		private set
		{
			if (!(m_defaultMaterial == value))
			{
				m_defaultMaterial = value;
				IncrementVersion(VersionChangeType.Layout | VersionChangeType.Repaint);
			}
		}
	}

	internal VisualElement nextParentWithEventCallback
	{
		get
		{
			if (GetCachedNextParentWithEventCallback(out var nextParent))
			{
				return nextParent;
			}
			for (VisualElement visualElement = hierarchy.parent; visualElement != null; visualElement = visualElement.hierarchy.parent)
			{
				if (visualElement.m_NextParentRequiredVersion != 0)
				{
					PropagateCachedNextParentWithEventCallback(visualElement, visualElement);
					return visualElement;
				}
				if (visualElement.GetCachedNextParentWithEventCallback(out var nextParent2))
				{
					PropagateCachedNextParentWithEventCallback(nextParent2, visualElement);
					return nextParent2;
				}
			}
			m_CachedNextParentWithEventCallback = null;
			return null;
		}
	}

	internal int eventCallbackCategories
	{
		get
		{
			return m_EventCallbackCategories;
		}
		set
		{
			if (m_EventCallbackCategories != value)
			{
				int num = m_EventCallbackCategories ^ value;
				if ((num & -2769) != 0)
				{
					SetAsNextParentWithEventCallback();
					IncrementVersion(VersionChangeType.EventCallbackCategories);
				}
				else
				{
					m_CachedEventCallbackParentCategories |= value;
				}
				m_EventCallbackCategories = value;
			}
		}
	}

	internal int eventCallbackParentCategories
	{
		get
		{
			if (elementPanel == null)
			{
				return -1;
			}
			if (isEventCallbackParentCategoriesDirty)
			{
				UpdateCallbackParentCategories();
				isEventCallbackParentCategoriesDirty = false;
			}
			return m_CachedEventCallbackParentCategories;
		}
	}

	internal bool isEventCallbackParentCategoriesDirty
	{
		get
		{
			return (m_Flags & VisualElementFlags.EventCallbackParentCategoriesDirty) == VisualElementFlags.EventCallbackParentCategoriesDirty;
		}
		set
		{
			m_Flags = (value ? (m_Flags | VisualElementFlags.EventCallbackParentCategoriesDirty) : (m_Flags & ~VisualElementFlags.EventCallbackParentCategoriesDirty));
		}
	}

	public IExperimentalFeatures experimental => this;

	ITransitionAnimations IExperimentalFeatures.animation => this;

	public Hierarchy hierarchy { get; private set; }

	internal bool isRootVisualContainer { get; set; }

	[Obsolete("VisualElement.cacheAsBitmap is deprecated and has no effect")]
	public bool cacheAsBitmap { get; set; }

	internal bool disableClipping
	{
		get
		{
			return (m_Flags & VisualElementFlags.DisableClipping) == VisualElementFlags.DisableClipping;
		}
		set
		{
			m_Flags = (value ? (m_Flags | VisualElementFlags.DisableClipping) : (m_Flags & ~VisualElementFlags.DisableClipping));
		}
	}

	public VisualElement parent => m_LogicalParent;

	internal BaseVisualElementPanel elementPanel { get; private set; }

	public IPanel panel => elementPanel;

	public virtual VisualElement contentContainer => this;

	public VisualTreeAsset visualTreeAssetSource
	{
		get
		{
			return m_VisualTreeAssetSource;
		}
		internal set
		{
			m_VisualTreeAssetSource = value;
		}
	}

	public VisualElement this[int key]
	{
		get
		{
			if (contentContainer == this)
			{
				return hierarchy[key];
			}
			return contentContainer?[key];
		}
	}

	public int childCount
	{
		get
		{
			if (contentContainer == this)
			{
				return hierarchy.childCount;
			}
			return contentContainer?.childCount ?? 0;
		}
	}

	private Vector3 positionWithLayout => ResolveTranslate() + (Vector3)layout.min;

	internal bool hasDefaultRotationAndScale
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return computedStyle.rotate.angle.value == 0f && computedStyle.scale.value == Vector3.one;
		}
	}

	public IVisualElementScheduler schedule => this;

	public IStyle style
	{
		get
		{
			if (inlineStyleAccess == null)
			{
				inlineStyleAccess = new InlineStyleAccess(this);
			}
			return inlineStyleAccess;
		}
	}

	public ICustomStyle customStyle
	{
		get
		{
			s_CustomStyleAccess.SetContext(computedStyle.customProperties, computedStyle.dpiScaling);
			return s_CustomStyleAccess;
		}
	}

	public VisualElementStyleSheetSet styleSheets => new VisualElementStyleSheetSet(this);

	public string tooltip
	{
		get
		{
			string text = GetProperty(tooltipPropertyKey) as string;
			return text ?? string.Empty;
		}
		set
		{
			if (!HasProperty(tooltipPropertyKey))
			{
				if (string.IsNullOrEmpty(value))
				{
					return;
				}
				RegisterCallback<TooltipEvent>(SetTooltip);
			}
			SetProperty(tooltipPropertyKey, value);
		}
	}

	private TypeData typeData
	{
		get
		{
			if (m_TypeData == null)
			{
				Type type = GetType();
				if (!s_TypeData.TryGetValue(type, out m_TypeData))
				{
					m_TypeData = new TypeData(type);
					s_TypeData.Add(type, m_TypeData);
				}
			}
			return m_TypeData;
		}
	}

	private IStylePropertyAnimationSystem GetStylePropertyAnimationSystem()
	{
		return elementPanel?.styleAnimationSystem;
	}

	bool IStylePropertyAnimations.Start(StylePropertyId id, float from, float to, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		return GetStylePropertyAnimationSystem().StartTransition(this, id, from, to, durationMs, delayMs, easingCurve);
	}

	bool IStylePropertyAnimations.Start(StylePropertyId id, int from, int to, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		return GetStylePropertyAnimationSystem().StartTransition(this, id, from, to, durationMs, delayMs, easingCurve);
	}

	bool IStylePropertyAnimations.Start(StylePropertyId id, Length from, Length to, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		return GetStylePropertyAnimationSystem().StartTransition(this, id, from, to, durationMs, delayMs, easingCurve);
	}

	bool IStylePropertyAnimations.Start(StylePropertyId id, Color from, Color to, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		return GetStylePropertyAnimationSystem().StartTransition(this, id, from, to, durationMs, delayMs, easingCurve);
	}

	bool IStylePropertyAnimations.StartEnum(StylePropertyId id, int from, int to, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		return GetStylePropertyAnimationSystem().StartTransition(this, id, from, to, durationMs, delayMs, easingCurve);
	}

	bool IStylePropertyAnimations.Start(StylePropertyId id, Background from, Background to, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		return GetStylePropertyAnimationSystem().StartTransition(this, id, from, to, durationMs, delayMs, easingCurve);
	}

	bool IStylePropertyAnimations.Start(StylePropertyId id, FontDefinition from, FontDefinition to, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		return GetStylePropertyAnimationSystem().StartTransition(this, id, from, to, durationMs, delayMs, easingCurve);
	}

	bool IStylePropertyAnimations.Start(StylePropertyId id, Font from, Font to, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		return GetStylePropertyAnimationSystem().StartTransition(this, id, from, to, durationMs, delayMs, easingCurve);
	}

	bool IStylePropertyAnimations.Start(StylePropertyId id, TextShadow from, TextShadow to, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		return GetStylePropertyAnimationSystem().StartTransition(this, id, from, to, durationMs, delayMs, easingCurve);
	}

	bool IStylePropertyAnimations.Start(StylePropertyId id, Scale from, Scale to, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		return GetStylePropertyAnimationSystem().StartTransition(this, id, from, to, durationMs, delayMs, easingCurve);
	}

	bool IStylePropertyAnimations.Start(StylePropertyId id, Translate from, Translate to, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		return GetStylePropertyAnimationSystem().StartTransition(this, id, from, to, durationMs, delayMs, easingCurve);
	}

	bool IStylePropertyAnimations.Start(StylePropertyId id, Rotate from, Rotate to, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		return GetStylePropertyAnimationSystem().StartTransition(this, id, from, to, durationMs, delayMs, easingCurve);
	}

	bool IStylePropertyAnimations.Start(StylePropertyId id, TransformOrigin from, TransformOrigin to, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		return GetStylePropertyAnimationSystem().StartTransition(this, id, from, to, durationMs, delayMs, easingCurve);
	}

	bool IStylePropertyAnimations.Start(StylePropertyId id, BackgroundPosition from, BackgroundPosition to, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		return GetStylePropertyAnimationSystem().StartTransition(this, id, from, to, durationMs, delayMs, easingCurve);
	}

	bool IStylePropertyAnimations.Start(StylePropertyId id, BackgroundRepeat from, BackgroundRepeat to, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		return GetStylePropertyAnimationSystem().StartTransition(this, id, from, to, durationMs, delayMs, easingCurve);
	}

	bool IStylePropertyAnimations.Start(StylePropertyId id, BackgroundSize from, BackgroundSize to, int durationMs, int delayMs, Func<float, float> easingCurve)
	{
		return GetStylePropertyAnimationSystem().StartTransition(this, id, from, to, durationMs, delayMs, easingCurve);
	}

	void IStylePropertyAnimations.CancelAnimation(StylePropertyId id)
	{
		GetStylePropertyAnimationSystem()?.CancelAnimation(this, id);
	}

	void IStylePropertyAnimations.CancelAllAnimations()
	{
		if (hasRunningAnimations || hasCompletedAnimations)
		{
			GetStylePropertyAnimationSystem()?.CancelAllAnimations(this);
		}
	}

	bool IStylePropertyAnimations.HasRunningAnimation(StylePropertyId id)
	{
		return hasRunningAnimations && GetStylePropertyAnimationSystem().HasRunningAnimation(this, id);
	}

	void IStylePropertyAnimations.UpdateAnimation(StylePropertyId id)
	{
		GetStylePropertyAnimationSystem().UpdateAnimation(this, id);
	}

	void IStylePropertyAnimations.GetAllAnimations(List<StylePropertyId> outPropertyIds)
	{
		if (hasRunningAnimations || hasCompletedAnimations)
		{
			GetStylePropertyAnimationSystem().GetAllAnimations(this, outPropertyIds);
		}
	}

	internal bool TryConvertLengthUnits(StylePropertyId id, ref Length from, ref Length to, int subPropertyIndex = 0)
	{
		if (from.IsAuto() || from.IsNone() || to.IsAuto() || to.IsNone())
		{
			return false;
		}
		if (float.IsNaN(from.value) || float.IsNaN(to.value))
		{
			return false;
		}
		if (from.unit == to.unit)
		{
			return true;
		}
		if (to.unit == LengthUnit.Pixel)
		{
			if (Mathf.Approximately(from.value, 0f))
			{
				from = new Length(0f, LengthUnit.Pixel);
				return true;
			}
			float? parentSizeForLengthConversion = GetParentSizeForLengthConversion(id, subPropertyIndex);
			if (!parentSizeForLengthConversion.HasValue || !(parentSizeForLengthConversion.Value >= 0f))
			{
				return false;
			}
			from = new Length(from.value * parentSizeForLengthConversion.Value / 100f, LengthUnit.Pixel);
		}
		else
		{
			Assert.AreEqual(LengthUnit.Percent, to.unit);
			float? parentSizeForLengthConversion2 = GetParentSizeForLengthConversion(id, subPropertyIndex);
			if (!parentSizeForLengthConversion2.HasValue || !(parentSizeForLengthConversion2.Value > 0f))
			{
				return false;
			}
			from = new Length(from.value * 100f / parentSizeForLengthConversion2.Value, LengthUnit.Percent);
		}
		return true;
	}

	internal bool TryConvertTransformOriginUnits(ref TransformOrigin from, ref TransformOrigin to)
	{
		Length from2 = from.x;
		Length from3 = from.y;
		Length to2 = to.x;
		Length to3 = to.y;
		if (!TryConvertLengthUnits(StylePropertyId.TransformOrigin, ref from2, ref to2))
		{
			return false;
		}
		if (!TryConvertLengthUnits(StylePropertyId.TransformOrigin, ref from3, ref to3, 1))
		{
			return false;
		}
		from.x = from2;
		from.y = from3;
		return true;
	}

	internal bool TryConvertTranslateUnits(ref Translate from, ref Translate to)
	{
		Length from2 = from.x;
		Length from3 = from.y;
		Length to2 = to.x;
		Length to3 = to.y;
		if (!TryConvertLengthUnits(StylePropertyId.Translate, ref from2, ref to2))
		{
			return false;
		}
		if (!TryConvertLengthUnits(StylePropertyId.Translate, ref from3, ref to3, 1))
		{
			return false;
		}
		from.x = from2;
		from.y = from3;
		return true;
	}

	internal bool TryConvertBackgroundPositionUnits(ref BackgroundPosition from, ref BackgroundPosition to)
	{
		Length from2 = from.offset;
		Length to2 = to.offset;
		if (!TryConvertLengthUnits(StylePropertyId.BackgroundPosition, ref from2, ref to2))
		{
			return false;
		}
		from.offset = from2;
		return true;
	}

	internal bool TryConvertBackgroundSizeUnits(ref BackgroundSize from, ref BackgroundSize to)
	{
		Length from2 = from.x;
		Length from3 = from.y;
		Length to2 = to.x;
		Length to3 = to.y;
		if (!TryConvertLengthUnits(StylePropertyId.BackgroundSize, ref from2, ref to2))
		{
			return false;
		}
		if (!TryConvertLengthUnits(StylePropertyId.BackgroundSize, ref from3, ref to3, 1))
		{
			return false;
		}
		from.x = from2;
		from.y = from3;
		return true;
	}

	private float? GetParentSizeForLengthConversion(StylePropertyId id, int subPropertyIndex = 0)
	{
		switch (id)
		{
		case StylePropertyId.Bottom:
		case StylePropertyId.Height:
		case StylePropertyId.MaxHeight:
		case StylePropertyId.MinHeight:
		case StylePropertyId.Top:
			return hierarchy.parent?.resolvedStyle.height;
		case StylePropertyId.Left:
		case StylePropertyId.MarginBottom:
		case StylePropertyId.MarginLeft:
		case StylePropertyId.MarginRight:
		case StylePropertyId.MarginTop:
		case StylePropertyId.MaxWidth:
		case StylePropertyId.MinWidth:
		case StylePropertyId.PaddingBottom:
		case StylePropertyId.PaddingLeft:
		case StylePropertyId.PaddingRight:
		case StylePropertyId.PaddingTop:
		case StylePropertyId.Right:
		case StylePropertyId.Width:
			return hierarchy.parent?.resolvedStyle.width;
		case StylePropertyId.FlexBasis:
		{
			if (hierarchy.parent == null)
			{
				return null;
			}
			FlexDirection flexDirection = hierarchy.parent.resolvedStyle.flexDirection;
			FlexDirection flexDirection2 = flexDirection;
			if ((uint)flexDirection2 <= 1u)
			{
				return hierarchy.parent.resolvedStyle.height;
			}
			return hierarchy.parent.resolvedStyle.width;
		}
		case StylePropertyId.BorderBottomLeftRadius:
		case StylePropertyId.BorderBottomRightRadius:
		case StylePropertyId.BorderTopLeftRadius:
		case StylePropertyId.BorderTopRightRadius:
			return resolvedStyle.width;
		case StylePropertyId.FontSize:
		case StylePropertyId.LetterSpacing:
		case StylePropertyId.UnityParagraphSpacing:
		case StylePropertyId.WordSpacing:
			return null;
		case StylePropertyId.TransformOrigin:
		case StylePropertyId.Translate:
			return (subPropertyIndex == 0) ? resolvedStyle.width : resolvedStyle.height;
		default:
			return null;
		}
	}

	internal void MarkRenderHintsClean()
	{
		m_RenderHints &= ~RenderHints.DirtyAll;
	}

	internal void ClearManualLayout()
	{
		isLayoutManual = false;
		IStyle style = this.style;
		style.position = StyleKeyword.Null;
		style.marginLeft = StyleKeyword.Null;
		style.marginRight = StyleKeyword.Null;
		style.marginBottom = StyleKeyword.Null;
		style.marginTop = StyleKeyword.Null;
		style.left = StyleKeyword.Null;
		style.top = StyleKeyword.Null;
		style.right = StyleKeyword.Null;
		style.bottom = StyleKeyword.Null;
		style.width = StyleKeyword.Null;
		style.height = StyleKeyword.Null;
	}

	internal void UpdateBoundingBox()
	{
		if (float.IsNaN(this.rect.x) || float.IsNaN(this.rect.y) || float.IsNaN(this.rect.width) || float.IsNaN(this.rect.height))
		{
			m_BoundingBox = Rect.zero;
		}
		else
		{
			m_BoundingBox = this.rect;
			if (!ShouldClip())
			{
				int count = m_Children.Count;
				for (int i = 0; i < count; i++)
				{
					Rect rect = m_Children[i].boundingBoxInParentSpace;
					m_BoundingBox.xMin = Math.Min(m_BoundingBox.xMin, rect.xMin);
					m_BoundingBox.xMax = Math.Max(m_BoundingBox.xMax, rect.xMax);
					m_BoundingBox.yMin = Math.Min(m_BoundingBox.yMin, rect.yMin);
					m_BoundingBox.yMax = Math.Max(m_BoundingBox.yMax, rect.yMax);
				}
			}
		}
		isWorldBoundingBoxDirty = true;
	}

	internal void UpdateWorldBoundingBox()
	{
		m_WorldBoundingBox = boundingBox;
		TransformAlignedRect(ref worldTransformRef, ref m_WorldBoundingBox);
	}

	internal void UpdateWorldTransform()
	{
		if (elementPanel != null && !elementPanel.duringLayoutPhase)
		{
			isWorldTransformDirty = false;
		}
		if (hierarchy.parent != null)
		{
			if (hasDefaultRotationAndScale)
			{
				TranslateMatrix34(ref hierarchy.parent.worldTransformRef, positionWithLayout, out m_WorldTransformCache);
			}
			else
			{
				GetPivotedMatrixWithLayout(out var result);
				MultiplyMatrix34(ref hierarchy.parent.worldTransformRef, ref result, out m_WorldTransformCache);
			}
		}
		else
		{
			GetPivotedMatrixWithLayout(out m_WorldTransformCache);
		}
		isWorldTransformInverseDirty = true;
		isWorldBoundingBoxDirty = true;
	}

	internal void UpdateWorldTransformInverse()
	{
		Matrix4x4.Inverse3DAffine(worldTransform, ref m_WorldTransformInverseCache);
		isWorldTransformInverseDirty = false;
	}

	internal void EnsureWorldTransformAndClipUpToDate()
	{
		if (isWorldTransformDirty)
		{
			UpdateWorldTransform();
		}
		if (isWorldClipDirty)
		{
			UpdateWorldClip();
			isWorldClipDirty = false;
		}
	}

	private void UpdateWorldClip()
	{
		if (hierarchy.parent != null)
		{
			m_WorldClip = hierarchy.parent.worldClip;
			bool flag = hierarchy.parent.worldClipIsInfinite;
			if (hierarchy.parent != renderChainData.groupTransformAncestor)
			{
				m_WorldClipMinusGroup = hierarchy.parent.worldClipMinusGroup;
			}
			else
			{
				flag = true;
				m_WorldClipMinusGroup = s_InfiniteRect;
			}
			if (ShouldClip())
			{
				Rect rect = SubstractBorderPadding(worldBound);
				m_WorldClip = CombineClipRects(rect, m_WorldClip);
				m_WorldClipMinusGroup = (flag ? rect : CombineClipRects(rect, m_WorldClipMinusGroup));
				m_WorldClipIsInfinite = false;
			}
			else
			{
				m_WorldClipIsInfinite = flag;
			}
		}
		else
		{
			m_WorldClipMinusGroup = (m_WorldClip = ((panel != null) ? panel.visualTree.rect : s_InfiniteRect));
			m_WorldClipIsInfinite = true;
		}
	}

	private Rect CombineClipRects(Rect rect, Rect parentRect)
	{
		float num = Mathf.Max(rect.xMin, parentRect.xMin);
		float num2 = Mathf.Min(rect.xMax, parentRect.xMax);
		float num3 = Mathf.Max(rect.yMin, parentRect.yMin);
		float num4 = Mathf.Min(rect.yMax, parentRect.yMax);
		float width = Mathf.Max(num2 - num, 0f);
		float height = Mathf.Max(num4 - num3, 0f);
		return new Rect(num, num3, width, height);
	}

	private Rect SubstractBorderPadding(Rect worldRect)
	{
		float m = worldTransform.m00;
		float m2 = worldTransform.m11;
		worldRect.x += resolvedStyle.borderLeftWidth * m;
		worldRect.y += resolvedStyle.borderTopWidth * m2;
		worldRect.width -= (resolvedStyle.borderLeftWidth + resolvedStyle.borderRightWidth) * m;
		worldRect.height -= (resolvedStyle.borderTopWidth + resolvedStyle.borderBottomWidth) * m2;
		if (computedStyle.unityOverflowClipBox == OverflowClipBox.ContentBox)
		{
			worldRect.x += resolvedStyle.paddingLeft * m;
			worldRect.y += resolvedStyle.paddingTop * m2;
			worldRect.width -= (resolvedStyle.paddingLeft + resolvedStyle.paddingRight) * m;
			worldRect.height -= (resolvedStyle.paddingTop + resolvedStyle.paddingBottom) * m2;
		}
		return worldRect;
	}

	internal static Rect ComputeAAAlignedBound(Rect position, Matrix4x4 mat)
	{
		Rect rect = position;
		Vector3 vector = mat.MultiplyPoint3x4(new Vector3(rect.x, rect.y, 0f));
		Vector3 vector2 = mat.MultiplyPoint3x4(new Vector3(rect.x + rect.width, rect.y, 0f));
		Vector3 vector3 = mat.MultiplyPoint3x4(new Vector3(rect.x, rect.y + rect.height, 0f));
		Vector3 vector4 = mat.MultiplyPoint3x4(new Vector3(rect.x + rect.width, rect.y + rect.height, 0f));
		return Rect.MinMaxRect(Mathf.Min(vector.x, Mathf.Min(vector2.x, Mathf.Min(vector3.x, vector4.x))), Mathf.Min(vector.y, Mathf.Min(vector2.y, Mathf.Min(vector3.y, vector4.y))), Mathf.Max(vector.x, Mathf.Max(vector2.x, Mathf.Max(vector3.x, vector4.x))), Mathf.Max(vector.y, Mathf.Max(vector2.y, Mathf.Max(vector3.y, vector4.y))));
	}

	private void UpdateHoverPseudoState()
	{
		if (containedPointerIds == 0 || panel == null)
		{
			pseudoStates &= ~PseudoStates.Hover;
			return;
		}
		bool flag = false;
		for (int i = 0; i < PointerId.maxPointers; i++)
		{
			if ((containedPointerIds & (1 << i)) != 0 && IsPartOfCapturedChain(this, panel.GetCapturingElement(i)))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			pseudoStates |= PseudoStates.Hover;
		}
		else
		{
			pseudoStates &= ~PseudoStates.Hover;
		}
	}

	private static bool IsPartOfCapturedChain(VisualElement self, in IEventHandler capturingElement)
	{
		if (self == null)
		{
			return false;
		}
		if (capturingElement == null)
		{
			return true;
		}
		if (capturingElement == self)
		{
			return true;
		}
		return self.Contains(capturingElement as VisualElement);
	}

	private void ChangeIMGUIContainerCount(int delta)
	{
		for (VisualElement visualElement = this; visualElement != null; visualElement = visualElement.hierarchy.parent)
		{
			visualElement.imguiContainerDescendantCount += delta;
		}
	}

	public VisualElement()
	{
		UIElementsRuntimeUtilityNative.VisualElementCreation();
		m_Children = s_EmptyList;
		controlid = ++s_NextId;
		hierarchy = new Hierarchy(this);
		m_ClassList = s_EmptyClassList;
		m_Flags = VisualElementFlags.Init;
		SetEnabled(value: true);
		base.focusable = false;
		name = string.Empty;
		yogaNode = new YogaNode();
		renderHints = RenderHints.None;
		EventInterestReflectionUtils.GetDefaultEventInterests(GetType(), out m_DefaultActionEventCategories, out m_DefaultActionAtTargetEventCategories);
	}

	[EventInterest(new Type[]
	{
		typeof(MouseOverEvent),
		typeof(MouseOutEvent),
		typeof(MouseCaptureOutEvent),
		typeof(PointerEnterEvent),
		typeof(PointerLeaveEvent),
		typeof(PointerCaptureEvent),
		typeof(PointerCaptureOutEvent),
		typeof(BlurEvent),
		typeof(FocusEvent),
		typeof(TooltipEvent)
	})]
	protected override void ExecuteDefaultAction(EventBase evt)
	{
		base.ExecuteDefaultAction(evt);
		if (evt == null)
		{
			return;
		}
		if (evt.eventTypeId == EventBase<MouseOverEvent>.TypeId() || evt.eventTypeId == EventBase<MouseOutEvent>.TypeId() || evt.eventTypeId == EventBase<MouseCaptureOutEvent>.TypeId())
		{
			UpdateCursorStyle(evt.eventTypeId);
		}
		else if (evt.eventTypeId == EventBase<PointerEnterEvent>.TypeId())
		{
			containedPointerIds |= 1 << ((IPointerEvent)evt).pointerId;
			UpdateHoverPseudoState();
		}
		else if (evt.eventTypeId == EventBase<PointerLeaveEvent>.TypeId())
		{
			containedPointerIds &= ~(1 << ((IPointerEvent)evt).pointerId);
			UpdateHoverPseudoState();
		}
		else if (evt.eventTypeId == EventBase<PointerCaptureEvent>.TypeId() || evt.eventTypeId == EventBase<PointerCaptureOutEvent>.TypeId())
		{
			for (VisualElement visualElement = this; visualElement != null; visualElement = visualElement.parent)
			{
				visualElement.UpdateHoverPseudoState();
			}
			VisualElement visualElement2 = elementPanel?.GetTopElementUnderPointer(((IPointerCaptureEventInternal)evt).pointerId);
			VisualElement visualElement3 = visualElement2;
			while (visualElement3 != null && visualElement3 != this)
			{
				visualElement3.UpdateHoverPseudoState();
				visualElement3 = visualElement3.parent;
			}
		}
		else if (evt.eventTypeId == EventBase<BlurEvent>.TypeId())
		{
			pseudoStates &= ~PseudoStates.Focus;
		}
		else if (evt.eventTypeId == EventBase<FocusEvent>.TypeId())
		{
			pseudoStates |= PseudoStates.Focus;
		}
		else if (evt.eventTypeId == EventBase<TooltipEvent>.TypeId())
		{
			SetTooltip((TooltipEvent)evt);
		}
	}

	internal virtual Rect GetTooltipRect()
	{
		return worldBound;
	}

	private void SetTooltip(TooltipEvent e)
	{
		if (e.currentTarget is VisualElement visualElement && !string.IsNullOrEmpty(visualElement.tooltip))
		{
			e.rect = visualElement.GetTooltipRect();
			e.tooltip = visualElement.tooltip;
			e.StopImmediatePropagation();
		}
	}

	public sealed override void Focus()
	{
		if (!canGrabFocus && hierarchy.parent != null)
		{
			hierarchy.parent.Focus();
		}
		else
		{
			base.Focus();
		}
	}

	internal void SetPanel(BaseVisualElementPanel p)
	{
		if (panel == p)
		{
			return;
		}
		List<VisualElement> list = VisualElementListPool.Get();
		try
		{
			list.Add(this);
			GatherAllChildren(list);
			EventDispatcherGate? eventDispatcherGate = null;
			if (p?.dispatcher != null)
			{
				eventDispatcherGate = new EventDispatcherGate(p.dispatcher);
			}
			EventDispatcherGate? eventDispatcherGate2 = null;
			if (panel?.dispatcher != null && panel.dispatcher != p?.dispatcher)
			{
				eventDispatcherGate2 = new EventDispatcherGate(panel.dispatcher);
			}
			BaseVisualElementPanel baseVisualElementPanel = elementPanel;
			uint num = baseVisualElementPanel?.hierarchyVersion ?? 0;
			using (eventDispatcherGate)
			{
				using (eventDispatcherGate2)
				{
					panel?.dispatcher?.m_ClickDetector.Cleanup(list);
					foreach (VisualElement item in list)
					{
						item.WillChangePanel(p);
					}
					uint num2 = baseVisualElementPanel?.hierarchyVersion ?? 0;
					if (num != num2)
					{
						list.Clear();
						list.Add(this);
						GatherAllChildren(list);
					}
					VisualElementFlags visualElementFlags = ((p != null) ? VisualElementFlags.NeedsAttachToPanelEvent : ((VisualElementFlags)0));
					foreach (VisualElement item2 in list)
					{
						item2.elementPanel = p;
						item2.m_Flags |= visualElementFlags;
						item2.m_CachedNextParentWithEventCallback = null;
					}
					foreach (VisualElement item3 in list)
					{
						item3.HasChangedPanel(baseVisualElementPanel);
					}
				}
			}
		}
		finally
		{
			VisualElementListPool.Release(list);
		}
	}

	private void WillChangePanel(BaseVisualElementPanel destinationPanel)
	{
		if (panel == null)
		{
			return;
		}
		UnregisterRunningAnimations();
		if ((m_Flags & VisualElementFlags.NeedsAttachToPanelEvent) == 0 && HasEventCallbacksOrDefaultActions(EventBase<DetachFromPanelEvent>.EventCategory))
		{
			using DetachFromPanelEvent detachFromPanelEvent = PanelChangedEventBase<DetachFromPanelEvent>.GetPooled(panel, destinationPanel);
			detachFromPanelEvent.target = this;
			HandleEventAtTargetAndDefaultPhase(detachFromPanelEvent);
		}
		UnregisterRunningAnimations();
	}

	private void HasChangedPanel(BaseVisualElementPanel prevPanel)
	{
		if (panel != null)
		{
			yogaNode.Config = elementPanel.yogaConfig;
			RegisterRunningAnimations();
			pseudoStates &= ~(PseudoStates.Active | PseudoStates.Hover | PseudoStates.Focus);
			if ((m_Flags & VisualElementFlags.NeedsAttachToPanelEvent) == VisualElementFlags.NeedsAttachToPanelEvent)
			{
				if (HasEventCallbacksOrDefaultActions(EventBase<AttachToPanelEvent>.EventCategory))
				{
					using AttachToPanelEvent attachToPanelEvent = PanelChangedEventBase<AttachToPanelEvent>.GetPooled(prevPanel, panel);
					attachToPanelEvent.target = this;
					HandleEventAtTargetAndDefaultPhase(attachToPanelEvent);
				}
				m_Flags &= ~VisualElementFlags.NeedsAttachToPanelEvent;
			}
		}
		else
		{
			yogaNode.Config = YogaConfig.Default;
		}
		styleInitialized = false;
		IncrementVersion(VersionChangeType.Layout | VersionChangeType.StyleSheet | VersionChangeType.Transform);
		if (!string.IsNullOrEmpty(viewDataKey))
		{
			IncrementVersion(VersionChangeType.ViewData);
		}
	}

	public sealed override void SendEvent(EventBase e)
	{
		elementPanel?.SendEvent(e);
	}

	internal sealed override void SendEvent(EventBase e, DispatchMode dispatchMode)
	{
		elementPanel?.SendEvent(e, dispatchMode);
	}

	internal void IncrementVersion(VersionChangeType changeType)
	{
		elementPanel?.OnVersionChanged(this, changeType);
	}

	internal void InvokeHierarchyChanged(HierarchyChangeType changeType)
	{
		elementPanel?.InvokeHierarchyChanged(this, changeType);
	}

	[Obsolete("SetEnabledFromHierarchy is deprecated and will be removed in a future release. Please use SetEnabled instead.")]
	protected internal bool SetEnabledFromHierarchy(bool state)
	{
		return SetEnabledFromHierarchyPrivate(state);
	}

	private bool SetEnabledFromHierarchyPrivate(bool state)
	{
		bool flag = enabledInHierarchy;
		bool flag2 = false;
		if (state)
		{
			if (isParentEnabledInHierarchy)
			{
				if (enabledSelf)
				{
					RemoveFromClassList(disabledUssClassName);
				}
				else
				{
					flag2 = true;
					AddToClassList(disabledUssClassName);
				}
			}
			else
			{
				flag2 = true;
				RemoveFromClassList(disabledUssClassName);
			}
		}
		else
		{
			flag2 = true;
			EnableInClassList(disabledUssClassName, isParentEnabledInHierarchy);
		}
		if (flag2)
		{
			if (focusController != null && focusController.IsFocused(this))
			{
				EventDispatcherGate? eventDispatcherGate = null;
				if (panel?.dispatcher != null)
				{
					eventDispatcherGate = new EventDispatcherGate(panel.dispatcher);
				}
				using (eventDispatcherGate)
				{
					BlurImmediately();
				}
			}
			pseudoStates |= PseudoStates.Disabled;
		}
		else
		{
			pseudoStates &= ~PseudoStates.Disabled;
		}
		return flag != enabledInHierarchy;
	}

	public void SetEnabled(bool value)
	{
		if (enabledSelf != value)
		{
			enabledSelf = value;
			PropagateEnabledToChildren(value);
		}
	}

	private void PropagateEnabledToChildren(bool value)
	{
		if (SetEnabledFromHierarchyPrivate(value))
		{
			int count = m_Children.Count;
			for (int i = 0; i < count; i++)
			{
				m_Children[i].PropagateEnabledToChildren(value);
			}
		}
	}

	public void MarkDirtyRepaint()
	{
		IncrementVersion(VersionChangeType.Repaint);
	}

	internal void InvokeGenerateVisualContent(MeshGenerationContext mgc)
	{
		if (generateVisualContent == null)
		{
			return;
		}
		try
		{
			using (k_GenerateVisualContentMarker.Auto())
			{
				generateVisualContent(mgc);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	internal void GetFullHierarchicalViewDataKey(StringBuilder key)
	{
		if (parent != null)
		{
			parent.GetFullHierarchicalViewDataKey(key);
		}
		if (!string.IsNullOrEmpty(viewDataKey))
		{
			key.Append("__");
			key.Append(viewDataKey);
		}
	}

	internal string GetFullHierarchicalViewDataKey()
	{
		StringBuilder stringBuilder = new StringBuilder();
		GetFullHierarchicalViewDataKey(stringBuilder);
		return stringBuilder.ToString();
	}

	internal T GetOrCreateViewData<T>(object existing, string key) where T : class, new()
	{
		Debug.Assert(elementPanel != null, "VisualElement.elementPanel is null! Cannot load persistent data.");
		ISerializableJsonDictionary serializableJsonDictionary = ((elementPanel == null || elementPanel.getViewDataDictionary == null) ? null : elementPanel.getViewDataDictionary());
		if (serializableJsonDictionary == null || string.IsNullOrEmpty(viewDataKey) || !enableViewDataPersistence)
		{
			if (existing != null)
			{
				return existing as T;
			}
			return new T();
		}
		string key2 = key + "__" + typeof(T);
		if (!serializableJsonDictionary.ContainsKey(key2))
		{
			serializableJsonDictionary.Set(key2, new T());
		}
		return serializableJsonDictionary.Get<T>(key2);
	}

	internal T GetOrCreateViewData<T>(ScriptableObject existing, string key) where T : ScriptableObject
	{
		Debug.Assert(elementPanel != null, "VisualElement.elementPanel is null! Cannot load view data.");
		ISerializableJsonDictionary serializableJsonDictionary = ((elementPanel == null || elementPanel.getViewDataDictionary == null) ? null : elementPanel.getViewDataDictionary());
		if (serializableJsonDictionary == null || string.IsNullOrEmpty(viewDataKey) || !enableViewDataPersistence)
		{
			if (existing != null)
			{
				return existing as T;
			}
			return ScriptableObject.CreateInstance<T>();
		}
		string key2 = key + "__" + typeof(T);
		if (!serializableJsonDictionary.ContainsKey(key2))
		{
			serializableJsonDictionary.Set(key2, ScriptableObject.CreateInstance<T>());
		}
		return serializableJsonDictionary.GetScriptable<T>(key2);
	}

	internal void OverwriteFromViewData(object obj, string key)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		Debug.Assert(elementPanel != null, "VisualElement.elementPanel is null! Cannot load view data.");
		ISerializableJsonDictionary serializableJsonDictionary = ((elementPanel == null || elementPanel.getViewDataDictionary == null) ? null : elementPanel.getViewDataDictionary());
		if (serializableJsonDictionary != null && !string.IsNullOrEmpty(viewDataKey) && enableViewDataPersistence)
		{
			string key2 = key + "__" + obj.GetType();
			if (!serializableJsonDictionary.ContainsKey(key2))
			{
				serializableJsonDictionary.Set(key2, obj);
			}
			else
			{
				serializableJsonDictionary.Overwrite(obj, key2);
			}
		}
	}

	internal void SaveViewData()
	{
		if (elementPanel != null && elementPanel.saveViewData != null && !string.IsNullOrEmpty(viewDataKey) && enableViewDataPersistence)
		{
			elementPanel.saveViewData();
		}
	}

	internal bool IsViewDataPersitenceSupportedOnChildren(bool existingState)
	{
		bool result = existingState;
		if (string.IsNullOrEmpty(viewDataKey) && this != contentContainer)
		{
			result = false;
		}
		if (parent != null && this == parent.contentContainer)
		{
			result = true;
		}
		return result;
	}

	internal void OnViewDataReady(bool enablePersistence)
	{
		enableViewDataPersistence = enablePersistence;
		OnViewDataReady();
	}

	internal virtual void OnViewDataReady()
	{
	}

	public virtual bool ContainsPoint(Vector2 localPoint)
	{
		return rect.Contains(localPoint);
	}

	public virtual bool Overlaps(Rect rectangle)
	{
		return rect.Overlaps(rectangle, allowInverse: true);
	}

	private void AssignMeasureFunction()
	{
		yogaNode.SetMeasureFunction((YogaNode node, float f, YogaMeasureMode mode, float f1, YogaMeasureMode heightMode) => Measure(node, f, mode, f1, heightMode));
	}

	private void RemoveMeasureFunction()
	{
		yogaNode.SetMeasureFunction(null);
	}

	protected internal virtual Vector2 DoMeasure(float desiredWidth, MeasureMode widthMode, float desiredHeight, MeasureMode heightMode)
	{
		return new Vector2(float.NaN, float.NaN);
	}

	internal YogaSize Measure(YogaNode node, float width, YogaMeasureMode widthMode, float height, YogaMeasureMode heightMode)
	{
		Debug.Assert(node == yogaNode, "YogaNode instance mismatch");
		Vector2 vector = DoMeasure(width, (MeasureMode)widthMode, height, (MeasureMode)heightMode);
		float pixelsPerPoint = scaledPixelsPerPoint;
		return MeasureOutput.Make(AlignmentUtils.RoundToPixelGrid(vector.x, pixelsPerPoint), AlignmentUtils.RoundToPixelGrid(vector.y, pixelsPerPoint));
	}

	internal void SetSize(Vector2 size)
	{
		Rect rect = layout;
		rect.width = size.x;
		rect.height = size.y;
		layout = rect;
	}

	private void FinalizeLayout()
	{
		if (hasInlineStyle || hasRunningAnimations)
		{
			computedStyle.SyncWithLayout(yogaNode);
		}
		else
		{
			yogaNode.CopyStyle(computedStyle.yogaNode);
		}
	}

	internal void SetInlineRule(StyleSheet sheet, StyleRule rule)
	{
		if (inlineStyleAccess == null)
		{
			inlineStyleAccess = new InlineStyleAccess(this);
		}
		inlineStyleAccess.SetInlineRule(sheet, rule);
	}

	internal void UpdateInlineRule(StyleSheet sheet, StyleRule rule)
	{
		ComputedStyle x = computedStyle.Acquire();
		long matchingRulesHash = computedStyle.matchingRulesHash;
		if (!StyleCache.TryGetValue(matchingRulesHash, out var data))
		{
			data = InitialStyle.Get();
		}
		m_Style.CopyFrom(ref data);
		SetInlineRule(sheet, rule);
		FinalizeLayout();
		VersionChangeType changeType = ComputedStyle.CompareChanges(ref x, ref computedStyle);
		x.Release();
		IncrementVersion(changeType);
	}

	internal void SetComputedStyle(ref ComputedStyle newStyle)
	{
		if (m_Style.matchingRulesHash != newStyle.matchingRulesHash)
		{
			VersionChangeType changeType = ComputedStyle.CompareChanges(ref m_Style, ref newStyle);
			m_Style.CopyFrom(ref newStyle);
			FinalizeLayout();
			if (elementPanel?.GetTopElementUnderPointer(PointerId.mousePointerId) == this)
			{
				elementPanel.cursorManager.SetCursor(m_Style.cursor);
			}
			IncrementVersion(changeType);
		}
	}

	internal void ResetPositionProperties()
	{
		if (hasInlineStyle)
		{
			style.position = StyleKeyword.Null;
			style.marginLeft = StyleKeyword.Null;
			style.marginRight = StyleKeyword.Null;
			style.marginBottom = StyleKeyword.Null;
			style.marginTop = StyleKeyword.Null;
			style.left = StyleKeyword.Null;
			style.top = StyleKeyword.Null;
			style.right = StyleKeyword.Null;
			style.bottom = StyleKeyword.Null;
			style.width = StyleKeyword.Null;
			style.height = StyleKeyword.Null;
		}
	}

	public override string ToString()
	{
		return GetType().Name + " " + name + " " + layout.ToString() + " world rect: " + worldBound.ToString();
	}

	public IEnumerable<string> GetClasses()
	{
		return m_ClassList;
	}

	internal List<string> GetClassesForIteration()
	{
		return m_ClassList;
	}

	public void ClearClassList()
	{
		if (m_ClassList.Count > 0)
		{
			ObjectListPool<string>.Release(m_ClassList);
			m_ClassList = s_EmptyClassList;
			IncrementVersion(VersionChangeType.StyleSheet);
		}
	}

	public void AddToClassList(string className)
	{
		if (string.IsNullOrEmpty(className))
		{
			return;
		}
		if (m_ClassList == s_EmptyClassList)
		{
			m_ClassList = ObjectListPool<string>.Get();
		}
		else
		{
			if (m_ClassList.Contains(className))
			{
				return;
			}
			if (m_ClassList.Capacity == m_ClassList.Count)
			{
				m_ClassList.Capacity++;
			}
		}
		m_ClassList.Add(className);
		IncrementVersion(VersionChangeType.StyleSheet);
	}

	public void RemoveFromClassList(string className)
	{
		if (m_ClassList.Remove(className))
		{
			if (m_ClassList.Count == 0)
			{
				ObjectListPool<string>.Release(m_ClassList);
				m_ClassList = s_EmptyClassList;
			}
			IncrementVersion(VersionChangeType.StyleSheet);
		}
	}

	public void ToggleInClassList(string className)
	{
		if (ClassListContains(className))
		{
			RemoveFromClassList(className);
		}
		else
		{
			AddToClassList(className);
		}
	}

	public void EnableInClassList(string className, bool enable)
	{
		if (enable)
		{
			AddToClassList(className);
		}
		else
		{
			RemoveFromClassList(className);
		}
	}

	public bool ClassListContains(string cls)
	{
		for (int i = 0; i < m_ClassList.Count; i++)
		{
			if (m_ClassList[i].Equals(cls, StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}

	public object FindAncestorUserData()
	{
		for (VisualElement visualElement = parent; visualElement != null; visualElement = visualElement.parent)
		{
			if (visualElement.userData != null)
			{
				return visualElement.userData;
			}
		}
		return null;
	}

	internal object GetProperty(PropertyName key)
	{
		CheckUserKeyArgument(key);
		TryGetPropertyInternal(key, out var value);
		return value;
	}

	internal void SetProperty(PropertyName key, object value)
	{
		CheckUserKeyArgument(key);
		SetPropertyInternal(key, value);
	}

	internal bool HasProperty(PropertyName key)
	{
		CheckUserKeyArgument(key);
		object value;
		return TryGetPropertyInternal(key, out value);
	}

	private bool TryGetPropertyInternal(PropertyName key, out object value)
	{
		value = null;
		if (m_PropertyBag != null)
		{
			for (int i = 0; i < m_PropertyBag.Count; i++)
			{
				if (m_PropertyBag[i].Key == key)
				{
					value = m_PropertyBag[i].Value;
					return true;
				}
			}
		}
		return false;
	}

	private static void CheckUserKeyArgument(PropertyName key)
	{
		if (PropertyName.IsNullOrEmpty(key))
		{
			throw new ArgumentNullException("key");
		}
		if (key == userDataPropertyKey)
		{
			throw new InvalidOperationException($"The {userDataPropertyKey} key is reserved by the system");
		}
	}

	private void SetPropertyInternal(PropertyName key, object value)
	{
		KeyValuePair<PropertyName, object> keyValuePair = new KeyValuePair<PropertyName, object>(key, value);
		if (m_PropertyBag == null)
		{
			m_PropertyBag = new List<KeyValuePair<PropertyName, object>>(1);
			m_PropertyBag.Add(keyValuePair);
			return;
		}
		for (int i = 0; i < m_PropertyBag.Count; i++)
		{
			if (m_PropertyBag[i].Key == key)
			{
				m_PropertyBag[i] = keyValuePair;
				return;
			}
		}
		if (m_PropertyBag.Capacity == m_PropertyBag.Count)
		{
			m_PropertyBag.Capacity++;
		}
		m_PropertyBag.Add(keyValuePair);
	}

	private void UpdateCursorStyle(long eventType)
	{
		if (elementPanel == null)
		{
			return;
		}
		if (eventType == EventBase<MouseCaptureOutEvent>.TypeId())
		{
			VisualElement topElementUnderPointer = elementPanel.GetTopElementUnderPointer(PointerId.mousePointerId);
			if (topElementUnderPointer != null)
			{
				elementPanel.cursorManager.SetCursor(topElementUnderPointer.computedStyle.cursor);
			}
			else
			{
				elementPanel.cursorManager.ResetCursor();
			}
			return;
		}
		IEventHandler capturingElement = elementPanel.GetCapturingElement(PointerId.mousePointerId);
		if (capturingElement == null || capturingElement == this)
		{
			if (eventType == EventBase<MouseOverEvent>.TypeId() && elementPanel.GetTopElementUnderPointer(PointerId.mousePointerId) == this)
			{
				elementPanel.cursorManager.SetCursor(computedStyle.cursor);
			}
			else if (eventType == EventBase<MouseOutEvent>.TypeId() && capturingElement == null)
			{
				elementPanel.cursorManager.ResetCursor();
			}
		}
	}

	private Material getRuntimeMaterial()
	{
		if (s_runtimeMaterial != null)
		{
			return s_runtimeMaterial;
		}
		Shader shader = Shader.Find(UIRUtility.k_DefaultShaderName);
		Debug.Assert(shader != null, "Failed to load UIElements default shader");
		if (shader != null)
		{
			shader.hideFlags |= HideFlags.DontSaveInEditor;
			Material material = new Material(shader);
			material.hideFlags |= HideFlags.DontSaveInEditor;
			return s_runtimeMaterial = material;
		}
		return null;
	}

	private VisualElementAnimationSystem GetAnimationSystem()
	{
		if (elementPanel != null)
		{
			return elementPanel.GetUpdater(VisualTreeUpdatePhase.Animation) as VisualElementAnimationSystem;
		}
		return null;
	}

	internal void RegisterAnimation(IValueAnimationUpdate anim)
	{
		if (m_RunningAnimations == null)
		{
			m_RunningAnimations = new List<IValueAnimationUpdate>();
		}
		m_RunningAnimations.Add(anim);
		GetAnimationSystem()?.RegisterAnimation(anim);
	}

	internal void UnregisterAnimation(IValueAnimationUpdate anim)
	{
		if (m_RunningAnimations != null)
		{
			m_RunningAnimations.Remove(anim);
		}
		GetAnimationSystem()?.UnregisterAnimation(anim);
	}

	private void UnregisterRunningAnimations()
	{
		if (m_RunningAnimations != null && m_RunningAnimations.Count > 0)
		{
			GetAnimationSystem()?.UnregisterAnimations(m_RunningAnimations);
		}
		styleAnimation.CancelAllAnimations();
	}

	private void RegisterRunningAnimations()
	{
		if (m_RunningAnimations != null && m_RunningAnimations.Count > 0)
		{
			GetAnimationSystem()?.RegisterAnimations(m_RunningAnimations);
		}
	}

	ValueAnimation<float> ITransitionAnimations.Start(float from, float to, int durationMs, Action<VisualElement, float> onValueChanged)
	{
		return experimental.animation.Start((VisualElement e) => from, to, durationMs, onValueChanged);
	}

	ValueAnimation<Rect> ITransitionAnimations.Start(Rect from, Rect to, int durationMs, Action<VisualElement, Rect> onValueChanged)
	{
		return experimental.animation.Start((VisualElement e) => from, to, durationMs, onValueChanged);
	}

	ValueAnimation<Color> ITransitionAnimations.Start(Color from, Color to, int durationMs, Action<VisualElement, Color> onValueChanged)
	{
		return experimental.animation.Start((VisualElement e) => from, to, durationMs, onValueChanged);
	}

	ValueAnimation<Vector3> ITransitionAnimations.Start(Vector3 from, Vector3 to, int durationMs, Action<VisualElement, Vector3> onValueChanged)
	{
		return experimental.animation.Start((VisualElement e) => from, to, durationMs, onValueChanged);
	}

	ValueAnimation<Vector2> ITransitionAnimations.Start(Vector2 from, Vector2 to, int durationMs, Action<VisualElement, Vector2> onValueChanged)
	{
		return experimental.animation.Start((VisualElement e) => from, to, durationMs, onValueChanged);
	}

	ValueAnimation<Quaternion> ITransitionAnimations.Start(Quaternion from, Quaternion to, int durationMs, Action<VisualElement, Quaternion> onValueChanged)
	{
		return experimental.animation.Start((VisualElement e) => from, to, durationMs, onValueChanged);
	}

	ValueAnimation<StyleValues> ITransitionAnimations.Start(StyleValues from, StyleValues to, int durationMs)
	{
		if (from.m_StyleValues == null)
		{
			from.Values();
		}
		if (to.m_StyleValues == null)
		{
			to.Values();
		}
		return Start((VisualElement e) => from, to, durationMs);
	}

	ValueAnimation<float> ITransitionAnimations.Start(Func<VisualElement, float> fromValueGetter, float to, int durationMs, Action<VisualElement, float> onValueChanged)
	{
		return StartAnimation(ValueAnimation<float>.Create(this, Lerp.Interpolate), fromValueGetter, to, durationMs, onValueChanged);
	}

	ValueAnimation<Rect> ITransitionAnimations.Start(Func<VisualElement, Rect> fromValueGetter, Rect to, int durationMs, Action<VisualElement, Rect> onValueChanged)
	{
		return StartAnimation(ValueAnimation<Rect>.Create(this, Lerp.Interpolate), fromValueGetter, to, durationMs, onValueChanged);
	}

	ValueAnimation<Color> ITransitionAnimations.Start(Func<VisualElement, Color> fromValueGetter, Color to, int durationMs, Action<VisualElement, Color> onValueChanged)
	{
		return StartAnimation(ValueAnimation<Color>.Create(this, Lerp.Interpolate), fromValueGetter, to, durationMs, onValueChanged);
	}

	ValueAnimation<Vector3> ITransitionAnimations.Start(Func<VisualElement, Vector3> fromValueGetter, Vector3 to, int durationMs, Action<VisualElement, Vector3> onValueChanged)
	{
		return StartAnimation(ValueAnimation<Vector3>.Create(this, Lerp.Interpolate), fromValueGetter, to, durationMs, onValueChanged);
	}

	ValueAnimation<Vector2> ITransitionAnimations.Start(Func<VisualElement, Vector2> fromValueGetter, Vector2 to, int durationMs, Action<VisualElement, Vector2> onValueChanged)
	{
		return StartAnimation(ValueAnimation<Vector2>.Create(this, Lerp.Interpolate), fromValueGetter, to, durationMs, onValueChanged);
	}

	ValueAnimation<Quaternion> ITransitionAnimations.Start(Func<VisualElement, Quaternion> fromValueGetter, Quaternion to, int durationMs, Action<VisualElement, Quaternion> onValueChanged)
	{
		return StartAnimation(ValueAnimation<Quaternion>.Create(this, Lerp.Interpolate), fromValueGetter, to, durationMs, onValueChanged);
	}

	private static ValueAnimation<T> StartAnimation<T>(ValueAnimation<T> anim, Func<VisualElement, T> fromValueGetter, T to, int durationMs, Action<VisualElement, T> onValueChanged)
	{
		anim.initialValue = fromValueGetter;
		anim.to = to;
		anim.durationMs = durationMs;
		anim.valueUpdated = onValueChanged;
		anim.Start();
		return anim;
	}

	private static void AssignStyleValues(VisualElement ve, StyleValues src)
	{
		IStyle style = ve.style;
		if (src.m_StyleValues == null)
		{
			return;
		}
		foreach (StyleValue value in src.m_StyleValues.m_Values)
		{
			switch (value.id)
			{
			case StylePropertyId.MarginLeft:
				style.marginLeft = value.number;
				break;
			case StylePropertyId.MarginTop:
				style.marginTop = value.number;
				break;
			case StylePropertyId.MarginRight:
				style.marginRight = value.number;
				break;
			case StylePropertyId.MarginBottom:
				style.marginBottom = value.number;
				break;
			case StylePropertyId.PaddingLeft:
				style.paddingLeft = value.number;
				break;
			case StylePropertyId.PaddingTop:
				style.paddingTop = value.number;
				break;
			case StylePropertyId.PaddingRight:
				style.paddingRight = value.number;
				break;
			case StylePropertyId.PaddingBottom:
				style.paddingBottom = value.number;
				break;
			case StylePropertyId.Left:
				style.left = value.number;
				break;
			case StylePropertyId.Top:
				style.top = value.number;
				break;
			case StylePropertyId.Right:
				style.right = value.number;
				break;
			case StylePropertyId.Bottom:
				style.bottom = value.number;
				break;
			case StylePropertyId.Width:
				style.width = value.number;
				break;
			case StylePropertyId.Height:
				style.height = value.number;
				break;
			case StylePropertyId.FlexGrow:
				style.flexGrow = value.number;
				break;
			case StylePropertyId.FlexShrink:
				style.flexShrink = value.number;
				break;
			case StylePropertyId.BorderLeftWidth:
				style.borderLeftWidth = value.number;
				break;
			case StylePropertyId.BorderTopWidth:
				style.borderTopWidth = value.number;
				break;
			case StylePropertyId.BorderRightWidth:
				style.borderRightWidth = value.number;
				break;
			case StylePropertyId.BorderBottomWidth:
				style.borderBottomWidth = value.number;
				break;
			case StylePropertyId.BorderTopLeftRadius:
				style.borderTopLeftRadius = value.number;
				break;
			case StylePropertyId.BorderTopRightRadius:
				style.borderTopRightRadius = value.number;
				break;
			case StylePropertyId.BorderBottomRightRadius:
				style.borderBottomRightRadius = value.number;
				break;
			case StylePropertyId.BorderBottomLeftRadius:
				style.borderBottomLeftRadius = value.number;
				break;
			case StylePropertyId.FontSize:
				style.fontSize = value.number;
				break;
			case StylePropertyId.Color:
				style.color = value.color;
				break;
			case StylePropertyId.BackgroundColor:
				style.backgroundColor = value.color;
				break;
			case StylePropertyId.BorderColor:
				style.borderLeftColor = value.color;
				style.borderTopColor = value.color;
				style.borderRightColor = value.color;
				style.borderBottomColor = value.color;
				break;
			case StylePropertyId.UnityBackgroundImageTintColor:
				style.unityBackgroundImageTintColor = value.color;
				break;
			case StylePropertyId.Opacity:
				style.opacity = value.number;
				break;
			}
		}
	}

	private StyleValues ReadCurrentValues(VisualElement ve, StyleValues targetValuesToRead)
	{
		StyleValues result = default(StyleValues);
		IResolvedStyle resolvedStyle = ve.resolvedStyle;
		if (targetValuesToRead.m_StyleValues != null)
		{
			using List<StyleValue>.Enumerator enumerator = targetValuesToRead.m_StyleValues.m_Values.GetEnumerator();
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current.id)
				{
				case StylePropertyId.MarginLeft:
					result.marginLeft = resolvedStyle.marginLeft;
					break;
				case StylePropertyId.MarginTop:
					result.marginTop = resolvedStyle.marginTop;
					break;
				case StylePropertyId.MarginRight:
					result.marginRight = resolvedStyle.marginRight;
					break;
				case StylePropertyId.MarginBottom:
					result.marginBottom = resolvedStyle.marginBottom;
					break;
				case StylePropertyId.PaddingLeft:
					result.paddingLeft = resolvedStyle.paddingLeft;
					break;
				case StylePropertyId.PaddingTop:
					result.paddingTop = resolvedStyle.paddingTop;
					break;
				case StylePropertyId.PaddingRight:
					result.paddingRight = resolvedStyle.paddingRight;
					break;
				case StylePropertyId.PaddingBottom:
					result.paddingBottom = resolvedStyle.paddingBottom;
					break;
				case StylePropertyId.Left:
					result.left = resolvedStyle.left;
					break;
				case StylePropertyId.Top:
					result.top = resolvedStyle.top;
					break;
				case StylePropertyId.Right:
					result.right = resolvedStyle.right;
					break;
				case StylePropertyId.Bottom:
					result.bottom = resolvedStyle.bottom;
					break;
				case StylePropertyId.Width:
					result.width = resolvedStyle.width;
					break;
				case StylePropertyId.Height:
					result.height = resolvedStyle.height;
					break;
				case StylePropertyId.FlexGrow:
					result.flexGrow = resolvedStyle.flexGrow;
					break;
				case StylePropertyId.FlexShrink:
					result.flexShrink = resolvedStyle.flexShrink;
					break;
				case StylePropertyId.BorderLeftWidth:
					result.borderLeftWidth = resolvedStyle.borderLeftWidth;
					break;
				case StylePropertyId.BorderTopWidth:
					result.borderTopWidth = resolvedStyle.borderTopWidth;
					break;
				case StylePropertyId.BorderRightWidth:
					result.borderRightWidth = resolvedStyle.borderRightWidth;
					break;
				case StylePropertyId.BorderBottomWidth:
					result.borderBottomWidth = resolvedStyle.borderBottomWidth;
					break;
				case StylePropertyId.BorderTopLeftRadius:
					result.borderTopLeftRadius = resolvedStyle.borderTopLeftRadius;
					break;
				case StylePropertyId.BorderTopRightRadius:
					result.borderTopRightRadius = resolvedStyle.borderTopRightRadius;
					break;
				case StylePropertyId.BorderBottomRightRadius:
					result.borderBottomRightRadius = resolvedStyle.borderBottomRightRadius;
					break;
				case StylePropertyId.BorderBottomLeftRadius:
					result.borderBottomLeftRadius = resolvedStyle.borderBottomLeftRadius;
					break;
				case StylePropertyId.Color:
					result.color = resolvedStyle.color;
					break;
				case StylePropertyId.BackgroundColor:
					result.backgroundColor = resolvedStyle.backgroundColor;
					break;
				case StylePropertyId.BorderColor:
					result.borderColor = resolvedStyle.borderLeftColor;
					break;
				case StylePropertyId.UnityBackgroundImageTintColor:
					result.unityBackgroundImageTintColor = resolvedStyle.unityBackgroundImageTintColor;
					break;
				case StylePropertyId.Opacity:
					result.opacity = resolvedStyle.opacity;
					break;
				}
			}
		}
		return result;
	}

	ValueAnimation<StyleValues> ITransitionAnimations.Start(StyleValues to, int durationMs)
	{
		if (to.m_StyleValues == null)
		{
			to.Values();
		}
		return Start((VisualElement e) => ReadCurrentValues(e, to), to, durationMs);
	}

	private ValueAnimation<StyleValues> Start(Func<VisualElement, StyleValues> fromValueGetter, StyleValues to, int durationMs)
	{
		return StartAnimation(ValueAnimation<StyleValues>.Create(this, Lerp.Interpolate), fromValueGetter, to, durationMs, AssignStyleValues);
	}

	ValueAnimation<Rect> ITransitionAnimations.Layout(Rect to, int durationMs)
	{
		return experimental.animation.Start((VisualElement e) => new Rect(e.resolvedStyle.left, e.resolvedStyle.top, e.resolvedStyle.width, e.resolvedStyle.height), to, durationMs, delegate(VisualElement e, Rect c)
		{
			e.style.left = c.x;
			e.style.top = c.y;
			e.style.width = c.width;
			e.style.height = c.height;
		});
	}

	ValueAnimation<Vector2> ITransitionAnimations.TopLeft(Vector2 to, int durationMs)
	{
		return experimental.animation.Start((VisualElement e) => new Vector2(e.resolvedStyle.left, e.resolvedStyle.top), to, durationMs, delegate(VisualElement e, Vector2 c)
		{
			e.style.left = c.x;
			e.style.top = c.y;
		});
	}

	ValueAnimation<Vector2> ITransitionAnimations.Size(Vector2 to, int durationMs)
	{
		return experimental.animation.Start((VisualElement e) => e.layout.size, to, durationMs, delegate(VisualElement e, Vector2 c)
		{
			e.style.width = c.x;
			e.style.height = c.y;
		});
	}

	ValueAnimation<float> ITransitionAnimations.Scale(float to, int durationMs)
	{
		return experimental.animation.Start((VisualElement e) => e.transform.scale.x, to, durationMs, delegate(VisualElement e, float c)
		{
			e.transform.scale = new Vector3(c, c, c);
		});
	}

	ValueAnimation<Vector3> ITransitionAnimations.Position(Vector3 to, int durationMs)
	{
		return experimental.animation.Start((VisualElement e) => e.transform.position, to, durationMs, delegate(VisualElement e, Vector3 c)
		{
			e.transform.position = c;
		});
	}

	ValueAnimation<Quaternion> ITransitionAnimations.Rotation(Quaternion to, int durationMs)
	{
		return experimental.animation.Start((VisualElement e) => e.transform.rotation, to, durationMs, delegate(VisualElement e, Quaternion c)
		{
			e.transform.rotation = c;
		});
	}

	private void DirtyNextParentWithEventCallback()
	{
		if (m_CachedNextParentWithEventCallback != null && m_NextParentCachedVersion == m_CachedNextParentWithEventCallback.m_NextParentRequiredVersion)
		{
			m_CachedNextParentWithEventCallback.m_NextParentRequiredVersion = ++s_NextParentVersion;
		}
	}

	private void SetAsNextParentWithEventCallback()
	{
		if (m_NextParentRequiredVersion == 0)
		{
			m_NextParentRequiredVersion = ++s_NextParentVersion;
			if (m_CachedNextParentWithEventCallback != null && m_NextParentCachedVersion == m_CachedNextParentWithEventCallback.m_NextParentRequiredVersion)
			{
				m_CachedNextParentWithEventCallback.m_NextParentRequiredVersion = ++s_NextParentVersion;
			}
		}
	}

	internal bool GetCachedNextParentWithEventCallback(out VisualElement nextParent)
	{
		nextParent = m_CachedNextParentWithEventCallback;
		return nextParent != null && nextParent.m_NextParentRequiredVersion == m_NextParentCachedVersion;
	}

	private void PropagateCachedNextParentWithEventCallback(VisualElement nextParent, VisualElement stopParent)
	{
		for (VisualElement visualElement = this; visualElement != stopParent; visualElement = visualElement.hierarchy.parent)
		{
			visualElement.m_CachedNextParentWithEventCallback = nextParent;
			visualElement.m_NextParentCachedVersion = nextParent.m_NextParentRequiredVersion;
		}
	}

	private void UpdateCallbackParentCategories()
	{
		m_CachedEventCallbackParentCategories = m_EventCallbackCategories;
		if (isCompositeRoot)
		{
			m_CachedEventCallbackParentCategories |= m_DefaultActionEventCategories;
		}
		VisualElement visualElement = nextParentWithEventCallback;
		if (visualElement == null)
		{
			return;
		}
		m_CachedEventCallbackParentCategories |= visualElement.eventCallbackParentCategories;
		if (hierarchy.parent != null)
		{
			for (VisualElement visualElement2 = hierarchy.parent; visualElement2 != visualElement; visualElement2 = visualElement2.hierarchy.parent)
			{
				visualElement2.m_CachedEventCallbackParentCategories = m_CachedEventCallbackParentCategories;
				visualElement2.isEventCallbackParentCategoriesDirty = false;
			}
		}
	}

	internal bool HasEventCallbacks(EventCategory eventCategory)
	{
		return (eventCallbackCategories & (1 << (int)eventCategory)) != 0;
	}

	internal bool HasParentEventCallbacks(EventCategory eventCategory)
	{
		return (eventCallbackParentCategories & (1 << (int)eventCategory)) != 0;
	}

	internal bool HasParentEventCallbacksOrDefaultActions(EventCategory eventCategory)
	{
		return ((m_DefaultActionEventCategories | m_DefaultActionAtTargetEventCategories | eventCallbackParentCategories) & (1 << (int)eventCategory)) != 0;
	}

	internal bool HasEventCallbacksOrDefaultActions(EventCategory eventCategory)
	{
		return ((m_DefaultActionEventCategories | m_DefaultActionAtTargetEventCategories | eventCallbackCategories) & (1 << (int)eventCategory)) != 0;
	}

	internal bool HasParentEventCallbacksOrDefaultActionAtTarget(EventCategory eventCategory)
	{
		return ((m_DefaultActionAtTargetEventCategories | eventCallbackParentCategories) & (1 << (int)eventCategory)) != 0;
	}

	internal bool HasEventCallbacksOrDefaultActionAtTarget(EventCategory eventCategory)
	{
		return ((m_DefaultActionAtTargetEventCategories | eventCallbackCategories) & (1 << (int)eventCategory)) != 0;
	}

	internal bool HasDefaultAction(EventCategory eventCategory)
	{
		return (m_DefaultActionEventCategories & (1 << (int)eventCategory)) != 0;
	}

	internal bool ShouldClip()
	{
		return computedStyle.overflow != OverflowInternal.Visible && !disableClipping;
	}

	public void Add(VisualElement child)
	{
		if (child != null)
		{
			VisualElement visualElement = contentContainer;
			if (visualElement == null)
			{
				throw new InvalidOperationException("You can't add directly to this VisualElement. Use hierarchy.Add() if you know what you're doing.");
			}
			if (visualElement == this)
			{
				hierarchy.Add(child);
			}
			else
			{
				visualElement?.Add(child);
			}
			child.m_LogicalParent = this;
		}
	}

	public void Insert(int index, VisualElement element)
	{
		if (element != null)
		{
			if (contentContainer == this)
			{
				hierarchy.Insert(index, element);
			}
			else
			{
				contentContainer?.Insert(index, element);
			}
			element.m_LogicalParent = this;
		}
	}

	public void Remove(VisualElement element)
	{
		if (contentContainer == this)
		{
			hierarchy.Remove(element);
		}
		else
		{
			contentContainer?.Remove(element);
		}
	}

	public void RemoveAt(int index)
	{
		if (contentContainer == this)
		{
			hierarchy.RemoveAt(index);
		}
		else
		{
			contentContainer?.RemoveAt(index);
		}
	}

	public void Clear()
	{
		if (contentContainer == this)
		{
			hierarchy.Clear();
		}
		else
		{
			contentContainer?.Clear();
		}
	}

	public VisualElement ElementAt(int index)
	{
		return this[index];
	}

	public int IndexOf(VisualElement element)
	{
		if (contentContainer == this)
		{
			return hierarchy.IndexOf(element);
		}
		return contentContainer?.IndexOf(element) ?? (-1);
	}

	internal VisualElement ElementAtTreePath(List<int> childIndexes)
	{
		VisualElement visualElement = this;
		foreach (int childIndex in childIndexes)
		{
			if (childIndex >= 0 && childIndex < visualElement.hierarchy.childCount)
			{
				visualElement = visualElement.hierarchy[childIndex];
				continue;
			}
			return null;
		}
		return visualElement;
	}

	internal bool FindElementInTree(VisualElement element, List<int> outChildIndexes)
	{
		VisualElement visualElement = element;
		for (VisualElement visualElement2 = visualElement.hierarchy.parent; visualElement2 != null; visualElement2 = visualElement2.hierarchy.parent)
		{
			outChildIndexes.Insert(0, visualElement2.hierarchy.IndexOf(visualElement));
			if (visualElement2 == this)
			{
				return true;
			}
			visualElement = visualElement2;
		}
		outChildIndexes.Clear();
		return false;
	}

	public IEnumerable<VisualElement> Children()
	{
		if (contentContainer == this)
		{
			return hierarchy.Children();
		}
		return contentContainer?.Children() ?? s_EmptyList;
	}

	public void Sort(Comparison<VisualElement> comp)
	{
		if (contentContainer == this)
		{
			hierarchy.Sort(comp);
		}
		else
		{
			contentContainer?.Sort(comp);
		}
	}

	public void BringToFront()
	{
		if (hierarchy.parent != null)
		{
			hierarchy.parent.hierarchy.BringToFront(this);
		}
	}

	public void SendToBack()
	{
		if (hierarchy.parent != null)
		{
			hierarchy.parent.hierarchy.SendToBack(this);
		}
	}

	public void PlaceBehind(VisualElement sibling)
	{
		if (sibling == null)
		{
			throw new ArgumentNullException("sibling");
		}
		if (hierarchy.parent == null || sibling.hierarchy.parent != hierarchy.parent)
		{
			throw new ArgumentException("VisualElements are not siblings");
		}
		hierarchy.parent.hierarchy.PlaceBehind(this, sibling);
	}

	public void PlaceInFront(VisualElement sibling)
	{
		if (sibling == null)
		{
			throw new ArgumentNullException("sibling");
		}
		if (hierarchy.parent == null || sibling.hierarchy.parent != hierarchy.parent)
		{
			throw new ArgumentException("VisualElements are not siblings");
		}
		hierarchy.parent.hierarchy.PlaceInFront(this, sibling);
	}

	public void RemoveFromHierarchy()
	{
		if (hierarchy.parent != null)
		{
			hierarchy.parent.hierarchy.Remove(this);
		}
	}

	public T GetFirstOfType<T>() where T : class
	{
		if (this is T result)
		{
			return result;
		}
		return GetFirstAncestorOfType<T>();
	}

	public T GetFirstAncestorOfType<T>() where T : class
	{
		for (VisualElement visualElement = hierarchy.parent; visualElement != null; visualElement = visualElement.hierarchy.parent)
		{
			if (visualElement is T result)
			{
				return result;
			}
		}
		return null;
	}

	internal VisualElement GetFirstAncestorWhere(Predicate<VisualElement> predicate)
	{
		for (VisualElement visualElement = hierarchy.parent; visualElement != null; visualElement = visualElement.hierarchy.parent)
		{
			if (predicate(visualElement))
			{
				return visualElement;
			}
		}
		return null;
	}

	public bool Contains(VisualElement child)
	{
		while (child != null)
		{
			if (child.hierarchy.parent == this)
			{
				return true;
			}
			child = child.hierarchy.parent;
		}
		return false;
	}

	private void GatherAllChildren(List<VisualElement> elements)
	{
		if (m_Children.Count > 0)
		{
			int i = elements.Count;
			elements.AddRange(m_Children);
			for (; i < elements.Count; i++)
			{
				VisualElement visualElement = elements[i];
				elements.AddRange(visualElement.m_Children);
			}
		}
	}

	public VisualElement FindCommonAncestor(VisualElement other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (panel != other.panel)
		{
			return null;
		}
		VisualElement visualElement = this;
		int num = 0;
		while (visualElement != null)
		{
			num++;
			visualElement = visualElement.hierarchy.parent;
		}
		VisualElement visualElement2 = other;
		int num2 = 0;
		while (visualElement2 != null)
		{
			num2++;
			visualElement2 = visualElement2.hierarchy.parent;
		}
		visualElement = this;
		visualElement2 = other;
		while (num > num2)
		{
			num--;
			visualElement = visualElement.hierarchy.parent;
		}
		while (num2 > num)
		{
			num2--;
			visualElement2 = visualElement2.hierarchy.parent;
		}
		while (visualElement != visualElement2)
		{
			visualElement = visualElement.hierarchy.parent;
			visualElement2 = visualElement2.hierarchy.parent;
		}
		return visualElement;
	}

	internal VisualElement GetRoot()
	{
		if (panel != null)
		{
			return panel.visualTree;
		}
		VisualElement visualElement = this;
		while (visualElement.m_PhysicalParent != null)
		{
			visualElement = visualElement.m_PhysicalParent;
		}
		return visualElement;
	}

	internal VisualElement GetRootVisualContainer()
	{
		VisualElement result = null;
		for (VisualElement visualElement = this; visualElement != null; visualElement = visualElement.hierarchy.parent)
		{
			if (visualElement.isRootVisualContainer)
			{
				result = visualElement;
			}
		}
		return result;
	}

	internal VisualElement GetNextElementDepthFirst()
	{
		if (m_Children.Count > 0)
		{
			return m_Children[0];
		}
		VisualElement physicalParent = m_PhysicalParent;
		VisualElement visualElement = this;
		while (physicalParent != null)
		{
			int i;
			for (i = 0; i < physicalParent.m_Children.Count && physicalParent.m_Children[i] != visualElement; i++)
			{
			}
			if (i < physicalParent.m_Children.Count - 1)
			{
				return physicalParent.m_Children[i + 1];
			}
			visualElement = physicalParent;
			physicalParent = physicalParent.m_PhysicalParent;
		}
		return null;
	}

	internal VisualElement GetPreviousElementDepthFirst()
	{
		if (m_PhysicalParent != null)
		{
			int i;
			for (i = 0; i < m_PhysicalParent.m_Children.Count && m_PhysicalParent.m_Children[i] != this; i++)
			{
			}
			if (i > 0)
			{
				VisualElement visualElement = m_PhysicalParent.m_Children[i - 1];
				while (visualElement.m_Children.Count > 0)
				{
					visualElement = visualElement.m_Children[visualElement.m_Children.Count - 1];
				}
				return visualElement;
			}
			return m_PhysicalParent;
		}
		return null;
	}

	internal VisualElement RetargetElement(VisualElement retargetAgainst)
	{
		if (retargetAgainst == null)
		{
			return this;
		}
		VisualElement visualElement = retargetAgainst.m_PhysicalParent ?? retargetAgainst;
		while (visualElement.m_PhysicalParent != null && !visualElement.isCompositeRoot)
		{
			visualElement = visualElement.m_PhysicalParent;
		}
		VisualElement result = this;
		VisualElement physicalParent = m_PhysicalParent;
		while (physicalParent != null)
		{
			physicalParent = physicalParent.m_PhysicalParent;
			if (physicalParent == visualElement)
			{
				return result;
			}
			if (physicalParent != null && physicalParent.isCompositeRoot)
			{
				result = physicalParent;
			}
		}
		return this;
	}

	internal void GetPivotedMatrixWithLayout(out Matrix4x4 result)
	{
		Vector3 vector = ResolveTransformOrigin();
		result = Matrix4x4.TRS(positionWithLayout + vector, ResolveRotation(), ResolveScale());
		TranslateMatrix34InPlace(ref result, -vector);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static float Min(float a, float b, float c, float d)
	{
		return Mathf.Min(Mathf.Min(a, b), Mathf.Min(c, d));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static float Max(float a, float b, float c, float d)
	{
		return Mathf.Max(Mathf.Max(a, b), Mathf.Max(c, d));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void TransformAlignedRectToParentSpace(ref Rect rect)
	{
		if (hasDefaultRotationAndScale)
		{
			rect.position += (Vector2)positionWithLayout;
			return;
		}
		GetPivotedMatrixWithLayout(out var result);
		rect = CalculateConservativeRect(ref result, rect);
	}

	internal static Rect CalculateConservativeRect(ref Matrix4x4 matrix, Rect rect)
	{
		if (float.IsNaN(rect.height) | float.IsNaN(rect.width) | float.IsNaN(rect.x) | float.IsNaN(rect.y))
		{
			rect = new Rect(MultiplyMatrix44Point2(ref matrix, rect.position), MultiplyVector2(ref matrix, rect.size));
			OrderMinMaxRect(ref rect);
			return rect;
		}
		Vector2 vector = new Vector2(rect.xMin, rect.yMin);
		Vector2 vector2 = new Vector2(rect.xMax, rect.yMax);
		Vector2 vector3 = new Vector2(rect.xMax, rect.yMin);
		Vector2 vector4 = new Vector2(rect.xMin, rect.yMax);
		Vector3 vector5 = matrix.MultiplyPoint3x4(vector);
		Vector3 vector6 = matrix.MultiplyPoint3x4(vector2);
		Vector3 vector7 = matrix.MultiplyPoint3x4(vector3);
		Vector3 vector8 = matrix.MultiplyPoint3x4(vector4);
		Vector2 vector9 = new Vector2(Min(vector5.x, vector6.x, vector7.x, vector8.x), Min(vector5.y, vector6.y, vector7.y, vector8.y));
		Vector2 vector10 = new Vector2(Max(vector5.x, vector6.x, vector7.x, vector8.x), Max(vector5.y, vector6.y, vector7.y, vector8.y));
		return new Rect(vector9.x, vector9.y, vector10.x - vector9.x, vector10.y - vector9.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void TransformAlignedRect(ref Matrix4x4 matrix, ref Rect rect)
	{
		rect = CalculateConservativeRect(ref matrix, rect);
	}

	internal static void OrderMinMaxRect(ref Rect rect)
	{
		if (rect.width < 0f)
		{
			rect.x += rect.width;
			rect.width = 0f - rect.width;
		}
		if (rect.height < 0f)
		{
			rect.y += rect.height;
			rect.height = 0f - rect.height;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Vector2 MultiplyMatrix44Point2(ref Matrix4x4 lhs, Vector2 point)
	{
		Vector2 result = default(Vector2);
		result.x = lhs.m00 * point.x + lhs.m01 * point.y + lhs.m03;
		result.y = lhs.m10 * point.x + lhs.m11 * point.y + lhs.m13;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Vector2 MultiplyVector2(ref Matrix4x4 lhs, Vector2 vector)
	{
		Vector2 result = default(Vector2);
		result.x = lhs.m00 * vector.x + lhs.m01 * vector.y;
		result.y = lhs.m10 * vector.x + lhs.m11 * vector.y;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Rect MultiplyMatrix44Rect2(ref Matrix4x4 lhs, Rect r)
	{
		r.position = MultiplyMatrix44Point2(ref lhs, r.position);
		r.size = MultiplyVector2(ref lhs, r.size);
		return r;
	}

	internal static void MultiplyMatrix34(ref Matrix4x4 lhs, ref Matrix4x4 rhs, out Matrix4x4 res)
	{
		res.m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20;
		res.m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21;
		res.m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22;
		res.m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03;
		res.m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20;
		res.m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21;
		res.m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22;
		res.m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13;
		res.m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20;
		res.m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21;
		res.m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22;
		res.m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23;
		res.m30 = 0f;
		res.m31 = 0f;
		res.m32 = 0f;
		res.m33 = 1f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void TranslateMatrix34(ref Matrix4x4 lhs, Vector3 rhs, out Matrix4x4 res)
	{
		res = lhs;
		TranslateMatrix34InPlace(ref res, rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void TranslateMatrix34InPlace(ref Matrix4x4 lhs, Vector3 rhs)
	{
		lhs.m03 += lhs.m00 * rhs.x + lhs.m01 * rhs.y + lhs.m02 * rhs.z;
		lhs.m13 += lhs.m10 * rhs.x + lhs.m11 * rhs.y + lhs.m12 * rhs.z;
		lhs.m23 += lhs.m20 * rhs.x + lhs.m21 * rhs.y + lhs.m22 * rhs.z;
	}

	IVisualElementScheduledItem IVisualElementScheduler.Execute(Action<TimerState> timerUpdateEvent)
	{
		TimerStateScheduledItem timerStateScheduledItem = new TimerStateScheduledItem(this, timerUpdateEvent)
		{
			timerUpdateStopCondition = ScheduledItem.OnceCondition
		};
		timerStateScheduledItem.Resume();
		return timerStateScheduledItem;
	}

	IVisualElementScheduledItem IVisualElementScheduler.Execute(Action updateEvent)
	{
		SimpleScheduledItem simpleScheduledItem = new SimpleScheduledItem(this, updateEvent)
		{
			timerUpdateStopCondition = ScheduledItem.OnceCondition
		};
		simpleScheduledItem.Resume();
		return simpleScheduledItem;
	}

	internal void AddStyleSheetPath(string sheetPath)
	{
		StyleSheet styleSheet = Panel.LoadResource(sheetPath, typeof(StyleSheet), scaledPixelsPerPoint) as StyleSheet;
		if (styleSheet == null)
		{
			if (!s_InternalStyleSheetPath.IsMatch(sheetPath))
			{
				Debug.LogWarning($"Style sheet not found for path \"{sheetPath}\"");
			}
		}
		else
		{
			styleSheets.Add(styleSheet);
		}
	}

	internal bool HasStyleSheetPath(string sheetPath)
	{
		StyleSheet styleSheet = Panel.LoadResource(sheetPath, typeof(StyleSheet), scaledPixelsPerPoint) as StyleSheet;
		if (styleSheet == null)
		{
			Debug.LogWarning($"Style sheet not found for path \"{sheetPath}\"");
			return false;
		}
		return styleSheets.Contains(styleSheet);
	}

	internal void RemoveStyleSheetPath(string sheetPath)
	{
		StyleSheet styleSheet = Panel.LoadResource(sheetPath, typeof(StyleSheet), scaledPixelsPerPoint) as StyleSheet;
		if (styleSheet == null)
		{
			Debug.LogWarning($"Style sheet not found for path \"{sheetPath}\"");
		}
		else
		{
			styleSheets.Remove(styleSheet);
		}
	}

	private StyleFloat ResolveLengthValue(Length length, bool isRow)
	{
		if (length.IsAuto())
		{
			return new StyleFloat(StyleKeyword.Auto);
		}
		if (length.IsNone())
		{
			return new StyleFloat(StyleKeyword.None);
		}
		if (length.unit != LengthUnit.Percent)
		{
			return new StyleFloat(length.value);
		}
		VisualElement visualElement = hierarchy.parent;
		if (visualElement == null)
		{
			return 0f;
		}
		float num = (isRow ? visualElement.resolvedStyle.width : visualElement.resolvedStyle.height);
		return length.value * num / 100f;
	}

	private Vector3 ResolveTranslate()
	{
		Translate translate = computedStyle.translate;
		Length x = translate.x;
		float x2;
		if (x.unit == LengthUnit.Percent)
		{
			float width = resolvedStyle.width;
			x2 = (float.IsNaN(width) ? 0f : (width * x.value / 100f));
		}
		else
		{
			x2 = x.value;
			x2 = (float.IsNaN(x2) ? 0f : x2);
		}
		Length y = translate.y;
		float y2;
		if (y.unit == LengthUnit.Percent)
		{
			float height = resolvedStyle.height;
			y2 = (float.IsNaN(height) ? 0f : (height * y.value / 100f));
		}
		else
		{
			y2 = y.value;
			y2 = (float.IsNaN(y2) ? 0f : y2);
		}
		float z = translate.z;
		z = (float.IsNaN(z) ? 0f : z);
		return new Vector3(x2, y2, z);
	}

	private Vector3 ResolveTransformOrigin()
	{
		TransformOrigin transformOrigin = computedStyle.transformOrigin;
		float num = float.NaN;
		Length x = transformOrigin.x;
		if (x.IsNone())
		{
			float width = resolvedStyle.width;
			num = (float.IsNaN(width) ? 0f : (width / 2f));
		}
		else if (x.unit == LengthUnit.Percent)
		{
			float width2 = resolvedStyle.width;
			num = (float.IsNaN(width2) ? 0f : (width2 * x.value / 100f));
		}
		else
		{
			num = x.value;
		}
		float num2 = float.NaN;
		Length y = transformOrigin.y;
		if (y.IsNone())
		{
			float height = resolvedStyle.height;
			num2 = (float.IsNaN(height) ? 0f : (height / 2f));
		}
		else if (y.unit == LengthUnit.Percent)
		{
			float height2 = resolvedStyle.height;
			num2 = (float.IsNaN(height2) ? 0f : (height2 * y.value / 100f));
		}
		else
		{
			num2 = y.value;
		}
		float z = transformOrigin.z;
		return new Vector3(num, num2, z);
	}

	private Quaternion ResolveRotation()
	{
		Rotate rotate = computedStyle.rotate;
		Vector3 axis = rotate.axis;
		if (float.IsNaN(rotate.angle.value) || float.IsNaN(axis.x) || float.IsNaN(axis.y) || float.IsNaN(axis.z))
		{
			rotate = Rotate.Initial();
		}
		return rotate.ToQuaternion();
	}

	private Vector3 ResolveScale()
	{
		Vector3 value = computedStyle.scale.value;
		return (float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z)) ? Vector3.one : value;
	}

	internal static TypeData GetOrCreateTypeData(Type t)
	{
		if (!s_TypeData.TryGetValue(t, out var value))
		{
			value = new TypeData(t);
			s_TypeData.Add(t, value);
		}
		return value;
	}
}
