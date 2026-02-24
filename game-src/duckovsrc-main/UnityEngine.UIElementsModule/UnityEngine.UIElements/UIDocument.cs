#define UNITY_ASSERTIONS
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace UnityEngine.UIElements;

[AddComponentMenu("UI Toolkit/UI Document")]
[HelpURL("UIE-get-started-with-runtime-ui")]
[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)]
[ExecuteAlways]
public sealed class UIDocument : MonoBehaviour
{
	internal const string k_RootStyleClassName = "unity-ui-document__root";

	internal const string k_VisualElementNameSuffix = "-container";

	private const int k_DefaultSortingOrder = 0;

	private static int s_CurrentUIDocumentCounter;

	internal readonly int m_UIDocumentCreationIndex;

	[SerializeField]
	private PanelSettings m_PanelSettings;

	private PanelSettings m_PreviousPanelSettings = null;

	[SerializeField]
	private UIDocument m_ParentUI;

	private UIDocumentList m_ChildrenContent = null;

	private List<UIDocument> m_ChildrenContentCopy = null;

	[SerializeField]
	private VisualTreeAsset sourceAsset;

	private VisualElement m_RootVisualElement;

	private int m_FirstChildInsertIndex;

	[SerializeField]
	private float m_SortingOrder = 0f;

	public PanelSettings panelSettings
	{
		get
		{
			return m_PanelSettings;
		}
		set
		{
			if (parentUI == null)
			{
				if (m_PanelSettings == value)
				{
					m_PreviousPanelSettings = m_PanelSettings;
					return;
				}
				if (m_PanelSettings != null)
				{
					m_PanelSettings.DetachUIDocument(this);
				}
				m_PanelSettings = value;
				if (m_PanelSettings != null)
				{
					m_PanelSettings.AttachAndInsertUIDocumentToVisualTree(this);
				}
			}
			else
			{
				Assert.AreEqual(parentUI.m_PanelSettings, value);
				m_PanelSettings = parentUI.m_PanelSettings;
			}
			if (m_ChildrenContent != null)
			{
				foreach (UIDocument attachedUIDocument in m_ChildrenContent.m_AttachedUIDocuments)
				{
					attachedUIDocument.panelSettings = m_PanelSettings;
				}
			}
			m_PreviousPanelSettings = m_PanelSettings;
		}
	}

	public UIDocument parentUI
	{
		get
		{
			return m_ParentUI;
		}
		private set
		{
			m_ParentUI = value;
		}
	}

	public VisualTreeAsset visualTreeAsset
	{
		get
		{
			return sourceAsset;
		}
		set
		{
			sourceAsset = value;
			RecreateUI();
		}
	}

	public VisualElement rootVisualElement => m_RootVisualElement;

	internal int firstChildInserIndex => m_FirstChildInsertIndex;

	public float sortingOrder
	{
		get
		{
			return m_SortingOrder;
		}
		set
		{
			if (m_SortingOrder != value)
			{
				m_SortingOrder = value;
				ApplySortingOrder();
			}
		}
	}

	internal void ApplySortingOrder()
	{
		AddRootVisualElementToTree();
	}

	private UIDocument()
	{
		m_UIDocumentCreationIndex = s_CurrentUIDocumentCounter++;
	}

	private void Awake()
	{
		SetupFromHierarchy();
	}

	private void OnEnable()
	{
		if (parentUI != null && m_PanelSettings == null)
		{
			m_PanelSettings = parentUI.m_PanelSettings;
		}
		if (m_RootVisualElement == null)
		{
			RecreateUI();
		}
		else
		{
			AddRootVisualElementToTree();
		}
	}

	private void SetupFromHierarchy()
	{
		if (parentUI != null)
		{
			parentUI.RemoveChild(this);
		}
		parentUI = FindUIDocumentParent();
	}

	private UIDocument FindUIDocumentParent()
	{
		Transform transform = base.transform;
		Transform parent = transform.parent;
		if (parent != null)
		{
			UIDocument[] componentsInParent = parent.GetComponentsInParent<UIDocument>(includeInactive: true);
			if (componentsInParent != null && componentsInParent.Length != 0)
			{
				return componentsInParent[0];
			}
		}
		return null;
	}

	internal void Reset()
	{
		if (parentUI == null)
		{
			m_PreviousPanelSettings?.DetachUIDocument(this);
			panelSettings = null;
		}
		SetupFromHierarchy();
		if (parentUI != null)
		{
			m_PanelSettings = parentUI.m_PanelSettings;
			AddRootVisualElementToTree();
		}
		else if (m_PanelSettings != null)
		{
			AddRootVisualElementToTree();
		}
	}

	private void AddChildAndInsertContentToVisualTree(UIDocument child)
	{
		if (m_ChildrenContent == null)
		{
			m_ChildrenContent = new UIDocumentList();
		}
		else
		{
			m_ChildrenContent.RemoveFromListAndFromVisualTree(child);
		}
		m_ChildrenContent.AddToListAndToVisualTree(child, m_RootVisualElement, m_FirstChildInsertIndex);
	}

	private void RemoveChild(UIDocument child)
	{
		m_ChildrenContent?.RemoveFromListAndFromVisualTree(child);
	}

	private void RecreateUI()
	{
		if (m_RootVisualElement != null)
		{
			RemoveFromHierarchy();
			m_RootVisualElement = null;
		}
		if (sourceAsset != null)
		{
			m_RootVisualElement = sourceAsset.Instantiate();
			if (m_RootVisualElement == null)
			{
				Debug.LogError("The UXML file set for the UIDocument could not be cloned.");
			}
		}
		if (m_RootVisualElement == null)
		{
			m_RootVisualElement = new TemplateContainer
			{
				name = base.gameObject.name + "-container"
			};
		}
		else
		{
			m_RootVisualElement.name = base.gameObject.name + "-container";
		}
		m_RootVisualElement.pickingMode = PickingMode.Ignore;
		if (base.isActiveAndEnabled)
		{
			AddRootVisualElementToTree();
		}
		m_FirstChildInsertIndex = m_RootVisualElement.childCount;
		if (m_ChildrenContent != null)
		{
			if (m_ChildrenContentCopy == null)
			{
				m_ChildrenContentCopy = new List<UIDocument>(m_ChildrenContent.m_AttachedUIDocuments);
			}
			else
			{
				m_ChildrenContentCopy.AddRange(m_ChildrenContent.m_AttachedUIDocuments);
			}
			foreach (UIDocument item in m_ChildrenContentCopy)
			{
				if (item.isActiveAndEnabled)
				{
					if (item.m_RootVisualElement == null)
					{
						item.RecreateUI();
					}
					else
					{
						AddChildAndInsertContentToVisualTree(item);
					}
				}
			}
			m_ChildrenContentCopy.Clear();
		}
		SetupRootClassList();
	}

	private void SetupRootClassList()
	{
		m_RootVisualElement?.EnableInClassList("unity-ui-document__root", parentUI == null);
	}

	private void AddRootVisualElementToTree()
	{
		if (base.enabled)
		{
			if (parentUI != null)
			{
				parentUI.AddChildAndInsertContentToVisualTree(this);
			}
			else if (m_PanelSettings != null)
			{
				m_PanelSettings.AttachAndInsertUIDocumentToVisualTree(this);
			}
		}
	}

	private void RemoveFromHierarchy()
	{
		if (parentUI != null)
		{
			parentUI.RemoveChild(this);
		}
		else if (m_PanelSettings != null)
		{
			m_PanelSettings.DetachUIDocument(this);
		}
	}

	private void OnDisable()
	{
		if (m_RootVisualElement != null)
		{
			RemoveFromHierarchy();
			m_RootVisualElement = null;
		}
	}

	private void OnTransformChildrenChanged()
	{
		if (m_ChildrenContent == null)
		{
			return;
		}
		if (m_ChildrenContentCopy == null)
		{
			m_ChildrenContentCopy = new List<UIDocument>(m_ChildrenContent.m_AttachedUIDocuments);
		}
		else
		{
			m_ChildrenContentCopy.AddRange(m_ChildrenContent.m_AttachedUIDocuments);
		}
		foreach (UIDocument item in m_ChildrenContentCopy)
		{
			item.ReactToHierarchyChanged();
		}
		m_ChildrenContentCopy.Clear();
	}

	private void OnTransformParentChanged()
	{
		ReactToHierarchyChanged();
	}

	internal void ReactToHierarchyChanged()
	{
		SetupFromHierarchy();
		if (parentUI != null)
		{
			panelSettings = parentUI.m_PanelSettings;
		}
		m_RootVisualElement?.RemoveFromHierarchy();
		AddRootVisualElementToTree();
		SetupRootClassList();
	}
}
