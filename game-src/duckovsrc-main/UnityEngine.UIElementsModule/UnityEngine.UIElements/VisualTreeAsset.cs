#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace UnityEngine.UIElements;

[Serializable]
[HelpURL("UIE-VisualTree-landing")]
public class VisualTreeAsset : ScriptableObject
{
	[Serializable]
	internal struct UsingEntry
	{
		internal static readonly IComparer<UsingEntry> comparer = new UsingEntryComparer();

		[SerializeField]
		public string alias;

		[SerializeField]
		public string path;

		[SerializeField]
		public VisualTreeAsset asset;

		public UsingEntry(string alias, string path)
		{
			this.alias = alias;
			this.path = path;
			asset = null;
		}

		public UsingEntry(string alias, VisualTreeAsset asset)
		{
			this.alias = alias;
			path = null;
			this.asset = asset;
		}
	}

	private class UsingEntryComparer : IComparer<UsingEntry>
	{
		public int Compare(UsingEntry x, UsingEntry y)
		{
			return string.CompareOrdinal(x.alias, y.alias);
		}
	}

	[Serializable]
	internal struct SlotDefinition
	{
		[SerializeField]
		public string name;

		[SerializeField]
		public int insertionPointId;
	}

	[Serializable]
	internal struct SlotUsageEntry
	{
		[SerializeField]
		public string slotName;

		[SerializeField]
		public int assetId;

		public SlotUsageEntry(string slotName, int assetId)
		{
			this.slotName = slotName;
			this.assetId = assetId;
		}
	}

	[Serializable]
	internal struct UxmlObjectEntry
	{
		[SerializeField]
		public int parentId;

		[SerializeField]
		public List<UxmlObjectAsset> uxmlObjectAssets;

		public UxmlObjectEntry(int parentId, List<UxmlObjectAsset> uxmlObjectAssets)
		{
			this.parentId = parentId;
			this.uxmlObjectAssets = uxmlObjectAssets;
		}
	}

	[Serializable]
	private struct AssetEntry
	{
		[SerializeField]
		public string path;

		[SerializeField]
		public string typeFullName;

		[SerializeField]
		public Object asset;

		private Type m_CachedType;

		public Type type => m_CachedType ?? (m_CachedType = Type.GetType(typeFullName));

		public AssetEntry(string path, Type type, Object asset)
		{
			this.path = path;
			typeFullName = type.AssemblyQualifiedName;
			this.asset = asset;
			m_CachedType = type;
		}
	}

	internal static string LinkedVEAInTemplatePropertyName = "--unity-linked-vea-in-template";

	[SerializeField]
	private bool m_ImportedWithErrors;

	[SerializeField]
	private bool m_ImportedWithWarnings;

	private static readonly Dictionary<string, VisualElement> s_TemporarySlotInsertionPoints = new Dictionary<string, VisualElement>();

	[SerializeField]
	private List<UsingEntry> m_Usings;

	[SerializeField]
	internal StyleSheet inlineSheet;

	[SerializeField]
	private List<VisualElementAsset> m_VisualElementAssets;

	[SerializeField]
	private List<TemplateAsset> m_TemplateAssets;

	[SerializeField]
	private List<UxmlObjectEntry> m_UxmlObjectEntries;

	[SerializeField]
	private List<int> m_UxmlObjectIds;

	[SerializeField]
	private List<AssetEntry> m_AssetEntries;

	[SerializeField]
	private List<SlotDefinition> m_Slots;

	[SerializeField]
	private int m_ContentContainerId;

	[SerializeField]
	private int m_ContentHash;

	public bool importedWithErrors
	{
		get
		{
			return m_ImportedWithErrors;
		}
		internal set
		{
			m_ImportedWithErrors = value;
		}
	}

	public bool importedWithWarnings
	{
		get
		{
			return m_ImportedWithWarnings;
		}
		internal set
		{
			m_ImportedWithWarnings = value;
		}
	}

	public IEnumerable<VisualTreeAsset> templateDependencies
	{
		get
		{
			if (m_Usings == null || m_Usings.Count == 0)
			{
				yield break;
			}
			HashSet<VisualTreeAsset> sent = new HashSet<VisualTreeAsset>();
			foreach (UsingEntry entry in m_Usings)
			{
				if (entry.asset != null && !sent.Contains(entry.asset))
				{
					sent.Add(entry.asset);
					yield return entry.asset;
				}
				else if (!string.IsNullOrEmpty(entry.path))
				{
					VisualTreeAsset vta = Panel.LoadResource(entry.path, typeof(VisualTreeAsset), GUIUtility.pixelsPerPoint) as VisualTreeAsset;
					if (vta != null && !sent.Contains(entry.asset))
					{
						sent.Add(entry.asset);
						yield return vta;
					}
				}
			}
		}
	}

	public IEnumerable<StyleSheet> stylesheets
	{
		get
		{
			HashSet<StyleSheet> sent = new HashSet<StyleSheet>();
			foreach (VisualElementAsset vea in m_VisualElementAssets)
			{
				if (vea.hasStylesheets)
				{
					foreach (StyleSheet stylesheet in vea.stylesheets)
					{
						if (!sent.Contains(stylesheet))
						{
							sent.Add(stylesheet);
							yield return stylesheet;
						}
					}
				}
				if (!vea.hasStylesheetPaths)
				{
					continue;
				}
				foreach (string stylesheetPath in vea.stylesheetPaths)
				{
					StyleSheet stylesheet2 = Panel.LoadResource(stylesheetPath, typeof(StyleSheet), GUIUtility.pixelsPerPoint) as StyleSheet;
					if (stylesheet2 != null && !sent.Contains(stylesheet2))
					{
						sent.Add(stylesheet2);
						yield return stylesheet2;
					}
				}
			}
		}
	}

	internal List<VisualElementAsset> visualElementAssets
	{
		get
		{
			return m_VisualElementAssets;
		}
		set
		{
			m_VisualElementAssets = value;
		}
	}

	internal List<TemplateAsset> templateAssets
	{
		get
		{
			return m_TemplateAssets;
		}
		set
		{
			m_TemplateAssets = value;
		}
	}

	internal List<UxmlObjectEntry> uxmlObjectEntries => m_UxmlObjectEntries;

	internal List<int> uxmlObjectIds => m_UxmlObjectIds;

	internal List<SlotDefinition> slots
	{
		get
		{
			return m_Slots;
		}
		set
		{
			m_Slots = value;
		}
	}

	internal int contentContainerId
	{
		get
		{
			return m_ContentContainerId;
		}
		set
		{
			m_ContentContainerId = value;
		}
	}

	public int contentHash
	{
		get
		{
			return m_ContentHash;
		}
		set
		{
			m_ContentHash = value;
		}
	}

	internal int GetNextChildSerialNumber()
	{
		int num = m_VisualElementAssets?.Count ?? 0;
		num += m_TemplateAssets?.Count ?? 0;
		return num + (m_UxmlObjectEntries?.Count ?? 0);
	}

	internal void RegisterUxmlObject(UxmlObjectAsset uxmlObjectAsset)
	{
		if (m_UxmlObjectEntries == null)
		{
			m_UxmlObjectEntries = new List<UxmlObjectEntry>();
		}
		if (m_UxmlObjectIds == null)
		{
			m_UxmlObjectIds = new List<int>();
		}
		UxmlObjectEntry uxmlObjectEntry = GetUxmlObjectEntry(uxmlObjectAsset.parentId);
		if (uxmlObjectEntry.uxmlObjectAssets != null)
		{
			uxmlObjectEntry.uxmlObjectAssets.Add(uxmlObjectAsset);
			return;
		}
		m_UxmlObjectEntries.Add(new UxmlObjectEntry(uxmlObjectAsset.parentId, new List<UxmlObjectAsset> { uxmlObjectAsset }));
		m_UxmlObjectIds.Add(uxmlObjectAsset.id);
	}

	internal List<T> GetUxmlObjects<T>(IUxmlAttributes asset, CreationContext cc) where T : new()
	{
		if (m_UxmlObjectEntries == null)
		{
			return null;
		}
		if (asset is UxmlAsset uxmlAsset)
		{
			UxmlObjectEntry uxmlObjectEntry = GetUxmlObjectEntry(uxmlAsset.id);
			if (uxmlObjectEntry.uxmlObjectAssets != null)
			{
				List<T> list = null;
				foreach (UxmlObjectAsset uxmlObjectAsset in uxmlObjectEntry.uxmlObjectAssets)
				{
					IBaseUxmlObjectFactory uxmlObjectFactory = GetUxmlObjectFactory(uxmlObjectAsset);
					if (uxmlObjectFactory is IUxmlObjectFactory<T> uxmlObjectFactory2)
					{
						T item = uxmlObjectFactory2.CreateObject(uxmlObjectAsset, cc);
						if (list == null)
						{
							list = new List<T> { item };
						}
						else
						{
							list.Add(item);
						}
					}
				}
				return list;
			}
		}
		return null;
	}

	internal bool AssetEntryExists(string path, Type type)
	{
		if (m_AssetEntries == null)
		{
			return false;
		}
		foreach (AssetEntry assetEntry in m_AssetEntries)
		{
			if (assetEntry.path == path && assetEntry.type == type)
			{
				return true;
			}
		}
		return false;
	}

	internal void RegisterAssetEntry(string path, Type type, Object asset)
	{
		if (m_AssetEntries == null)
		{
			m_AssetEntries = new List<AssetEntry>();
		}
		m_AssetEntries.Add(new AssetEntry(path, type, asset));
	}

	internal T GetAsset<T>(string path) where T : Object
	{
		foreach (AssetEntry assetEntry in m_AssetEntries)
		{
			if (assetEntry.path.Equals(path) && assetEntry.type == typeof(T))
			{
				return assetEntry.asset as T;
			}
		}
		return null;
	}

	internal UxmlObjectEntry GetUxmlObjectEntry(int id)
	{
		if (m_UxmlObjectEntries != null)
		{
			foreach (UxmlObjectEntry uxmlObjectEntry in m_UxmlObjectEntries)
			{
				if (uxmlObjectEntry.parentId == id)
				{
					return uxmlObjectEntry;
				}
			}
		}
		return default(UxmlObjectEntry);
	}

	private IBaseUxmlObjectFactory GetUxmlObjectFactory(UxmlObjectAsset uxmlObjectAsset)
	{
		if (!UxmlObjectFactoryRegistry.TryGetFactories(uxmlObjectAsset.fullTypeName, out var factoryList))
		{
			Debug.LogErrorFormat("Element '{0}' has no registered factory method.", uxmlObjectAsset.fullTypeName);
			return null;
		}
		IBaseUxmlObjectFactory baseUxmlObjectFactory = null;
		CreationContext cc = new CreationContext(null, this, null);
		foreach (IBaseUxmlObjectFactory item in factoryList)
		{
			if (item.AcceptsAttributeBag(uxmlObjectAsset, cc))
			{
				baseUxmlObjectFactory = item;
				break;
			}
		}
		if (baseUxmlObjectFactory == null)
		{
			Debug.LogErrorFormat("Element '{0}' has a no factory that accept the set of XML attributes specified.", uxmlObjectAsset.fullTypeName);
			return null;
		}
		return baseUxmlObjectFactory;
	}

	public TemplateContainer Instantiate()
	{
		TemplateContainer templateContainer = new TemplateContainer(base.name);
		try
		{
			CloneTree(templateContainer, s_TemporarySlotInsertionPoints, null);
		}
		finally
		{
			s_TemporarySlotInsertionPoints.Clear();
		}
		return templateContainer;
	}

	public TemplateContainer Instantiate(string bindingPath)
	{
		TemplateContainer templateContainer = Instantiate();
		templateContainer.bindingPath = bindingPath;
		return templateContainer;
	}

	public TemplateContainer CloneTree()
	{
		return Instantiate();
	}

	public TemplateContainer CloneTree(string bindingPath)
	{
		return Instantiate(bindingPath);
	}

	public void CloneTree(VisualElement target)
	{
		CloneTree(target, out var _, out var _);
	}

	public void CloneTree(VisualElement target, out int firstElementIndex, out int elementAddedCount)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		firstElementIndex = target.childCount;
		try
		{
			CloneTree(target, s_TemporarySlotInsertionPoints, null);
		}
		finally
		{
			elementAddedCount = target.childCount - firstElementIndex;
			s_TemporarySlotInsertionPoints.Clear();
		}
	}

	internal void CloneTree(VisualElement target, Dictionary<string, VisualElement> slotInsertionPoints, List<TemplateAsset.AttributeOverride> attributeOverrides)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if ((visualElementAssets == null || visualElementAssets.Count <= 0) && (templateAssets == null || templateAssets.Count <= 0))
		{
			return;
		}
		if (target is TemplateContainer templateContainer)
		{
			templateContainer.templateSource = this;
		}
		Dictionary<int, List<VisualElementAsset>> dictionary = new Dictionary<int, List<VisualElementAsset>>();
		int num = ((visualElementAssets != null) ? visualElementAssets.Count : 0);
		int num2 = ((templateAssets != null) ? templateAssets.Count : 0);
		for (int i = 0; i < num + num2; i++)
		{
			VisualElementAsset visualElementAsset = ((i < num) ? visualElementAssets[i] : templateAssets[i - num]);
			if (!dictionary.TryGetValue(visualElementAsset.parentId, out var value))
			{
				value = new List<VisualElementAsset>();
				dictionary.Add(visualElementAsset.parentId, value);
			}
			value.Add(visualElementAsset);
		}
		dictionary.TryGetValue(0, out var value2);
		if (value2 == null || value2.Count == 0)
		{
			return;
		}
		Debug.Assert(value2.Count == 1);
		VisualElementAsset visualElementAsset2 = value2[0];
		AssignClassListFromAssetToElement(visualElementAsset2, target);
		AssignStyleSheetFromAssetToElement(visualElementAsset2, target);
		value2.Clear();
		dictionary.TryGetValue(visualElementAsset2.id, out value2);
		if (value2 == null || value2.Count == 0)
		{
			return;
		}
		value2.Sort(CompareForOrder);
		foreach (VisualElementAsset item in value2)
		{
			Assert.IsNotNull(item);
			VisualElement visualElement = CloneSetupRecursively(item, dictionary, new CreationContext(slotInsertionPoints, attributeOverrides, this, target));
			if (visualElement != null)
			{
				visualElement.visualTreeAssetSource = this;
				target.hierarchy.Add(visualElement);
			}
		}
	}

	private VisualElement CloneSetupRecursively(VisualElementAsset root, Dictionary<int, List<VisualElementAsset>> idToChildren, CreationContext context)
	{
		if (root.skipClone)
		{
			return null;
		}
		VisualElement visualElement = Create(root, context);
		if (visualElement == null)
		{
			return null;
		}
		if (root.id == context.visualTreeAsset.contentContainerId)
		{
			if (context.target is TemplateContainer)
			{
				((TemplateContainer)context.target).SetContentContainer(visualElement);
			}
			else
			{
				Debug.LogError("Trying to clone a VisualTreeAsset with a custom content container into a element which is not a template container");
			}
		}
		if (context.slotInsertionPoints != null && TryGetSlotInsertionPoint(root.id, out var slotName))
		{
			context.slotInsertionPoints.Add(slotName, visualElement);
		}
		if (root.ruleIndex != -1)
		{
			if (inlineSheet == null)
			{
				Debug.LogWarning("VisualElementAsset has a RuleIndex but no inlineStyleSheet");
			}
			else
			{
				StyleRule rule = inlineSheet.rules[root.ruleIndex];
				visualElement.SetInlineRule(inlineSheet, rule);
			}
		}
		if (root.ruleIndex != -1)
		{
			if (inlineSheet == null)
			{
				Debug.LogWarning("VisualElementAsset has a RuleIndex but no inlineStyleSheet");
			}
			else
			{
				StyleRule rule2 = inlineSheet.rules[root.ruleIndex];
				visualElement.SetInlineRule(inlineSheet, rule2);
			}
		}
		TemplateAsset templateAsset = root as TemplateAsset;
		if (idToChildren.TryGetValue(root.id, out var value))
		{
			value.Sort(CompareForOrder);
			foreach (VisualElementAsset childVea in value)
			{
				VisualElement visualElement2 = CloneSetupRecursively(childVea, idToChildren, context);
				if (visualElement2 == null)
				{
					continue;
				}
				if (templateAsset == null)
				{
					visualElement.Add(visualElement2);
					continue;
				}
				int num = ((templateAsset.slotUsages == null) ? (-1) : templateAsset.slotUsages.FindIndex((SlotUsageEntry u) => u.assetId == childVea.id));
				if (num != -1)
				{
					string slotName2 = templateAsset.slotUsages[num].slotName;
					Assert.IsFalse(string.IsNullOrEmpty(slotName2), "a lost name should not be null or empty, this probably points to an importer or serialization bug");
					if (context.slotInsertionPoints == null || !context.slotInsertionPoints.TryGetValue(slotName2, out var value2))
					{
						Debug.LogErrorFormat("Slot '{0}' was not found. Existing slots: {1}", slotName2, (context.slotInsertionPoints == null) ? string.Empty : string.Join(", ", context.slotInsertionPoints.Keys.ToArray()));
						visualElement.Add(visualElement2);
					}
					else
					{
						value2.Add(visualElement2);
					}
				}
				else
				{
					visualElement.Add(visualElement2);
				}
			}
		}
		if (templateAsset != null && context.slotInsertionPoints != null)
		{
			context.slotInsertionPoints.Clear();
		}
		return visualElement;
	}

	private static int CompareForOrder(VisualElementAsset a, VisualElementAsset b)
	{
		return a.orderInDocument.CompareTo(b.orderInDocument);
	}

	internal bool TryGetSlotInsertionPoint(int insertionPointId, out string slotName)
	{
		if (m_Slots == null)
		{
			slotName = null;
			return false;
		}
		for (int i = 0; i < m_Slots.Count; i++)
		{
			SlotDefinition slotDefinition = m_Slots[i];
			if (slotDefinition.insertionPointId == insertionPointId)
			{
				slotName = slotDefinition.name;
				return true;
			}
		}
		slotName = null;
		return false;
	}

	internal VisualTreeAsset ResolveTemplate(string templateName)
	{
		if (m_Usings == null || m_Usings.Count == 0)
		{
			return null;
		}
		int num = m_Usings.BinarySearch(new UsingEntry(templateName, string.Empty), UsingEntry.comparer);
		if (num < 0)
		{
			return null;
		}
		if ((bool)m_Usings[num].asset)
		{
			return m_Usings[num].asset;
		}
		string path = m_Usings[num].path;
		return Panel.LoadResource(path, typeof(VisualTreeAsset), GUIUtility.pixelsPerPoint) as VisualTreeAsset;
	}

	internal static VisualElement Create(VisualElementAsset asset, CreationContext ctx)
	{
		if (!VisualElementFactoryRegistry.TryGetValue(asset.fullTypeName, out var factoryList))
		{
			if (asset.fullTypeName.StartsWith("UnityEngine.Experimental.UIElements.") || asset.fullTypeName.StartsWith("UnityEditor.Experimental.UIElements."))
			{
				string fullTypeName = asset.fullTypeName.Replace(".Experimental.UIElements", ".UIElements");
				if (!VisualElementFactoryRegistry.TryGetValue(fullTypeName, out factoryList))
				{
					return CreateError();
				}
			}
			else
			{
				if (!(asset.fullTypeName == "UXML"))
				{
					return CreateError();
				}
				VisualElementFactoryRegistry.TryGetValue(typeof(UxmlRootElementFactory).Namespace + "." + asset.fullTypeName, out factoryList);
			}
		}
		IUxmlFactory uxmlFactory = null;
		foreach (IUxmlFactory item in factoryList)
		{
			if (item.AcceptsAttributeBag(asset, ctx))
			{
				uxmlFactory = item;
				break;
			}
		}
		if (uxmlFactory == null)
		{
			Debug.LogErrorFormat("Element '{0}' has a no factory that accept the set of XML attributes specified.", asset.fullTypeName);
			return new Label($"Type with no factory: '{asset.fullTypeName}'");
		}
		VisualElement visualElement = uxmlFactory.Create(asset, ctx);
		if (visualElement != null)
		{
			AssignClassListFromAssetToElement(asset, visualElement);
			AssignStyleSheetFromAssetToElement(asset, visualElement);
		}
		return visualElement;
		VisualElement CreateError()
		{
			Debug.LogErrorFormat("Element '{0}' has no registered factory method.", asset.fullTypeName);
			return new Label($"Unknown type: '{asset.fullTypeName}'");
		}
	}

	private static void AssignClassListFromAssetToElement(VisualElementAsset asset, VisualElement element)
	{
		if (asset.classes != null)
		{
			for (int i = 0; i < asset.classes.Length; i++)
			{
				element.AddToClassList(asset.classes[i]);
			}
		}
	}

	private static void AssignStyleSheetFromAssetToElement(VisualElementAsset asset, VisualElement element)
	{
		if (asset.hasStylesheetPaths)
		{
			for (int i = 0; i < asset.stylesheetPaths.Count; i++)
			{
				element.AddStyleSheetPath(asset.stylesheetPaths[i]);
			}
		}
		if (!asset.hasStylesheets)
		{
			return;
		}
		for (int j = 0; j < asset.stylesheets.Count; j++)
		{
			if (asset.stylesheets[j] != null)
			{
				element.styleSheets.Add(asset.stylesheets[j]);
			}
		}
	}
}
