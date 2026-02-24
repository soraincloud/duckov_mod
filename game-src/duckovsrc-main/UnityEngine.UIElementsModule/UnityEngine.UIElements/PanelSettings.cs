using System;
using UnityEngine.UIElements.UIR;

namespace UnityEngine.UIElements;

[HelpURL("UIE-Runtime-Panel-Settings")]
public class PanelSettings : ScriptableObject
{
	private class RuntimePanelAccess
	{
		private readonly PanelSettings m_Settings;

		private BaseRuntimePanel m_RuntimePanel;

		internal bool isInitialized => m_RuntimePanel != null;

		internal BaseRuntimePanel panel
		{
			get
			{
				if (m_RuntimePanel == null)
				{
					m_RuntimePanel = CreateRelatedRuntimePanel();
					m_RuntimePanel.sortingPriority = m_Settings.m_SortingOrder;
					m_RuntimePanel.targetDisplay = m_Settings.m_TargetDisplay;
					VisualElement visualTree = m_RuntimePanel.visualTree;
					visualTree.name = m_Settings.name;
					m_Settings.ApplyThemeStyleSheet(visualTree);
					if (m_Settings.m_TargetTexture != null)
					{
						m_RuntimePanel.targetTexture = m_Settings.m_TargetTexture;
					}
					if (m_Settings.m_AssignedScreenToPanel != null)
					{
						m_Settings.SetScreenToPanelSpaceFunction(m_Settings.m_AssignedScreenToPanel);
					}
				}
				return m_RuntimePanel;
			}
		}

		internal RuntimePanelAccess(PanelSettings settings)
		{
			m_Settings = settings;
		}

		internal void DisposePanel()
		{
			if (m_RuntimePanel != null)
			{
				DisposeRelatedPanel();
				m_RuntimePanel = null;
			}
		}

		internal void SetTargetTexture()
		{
			if (m_RuntimePanel != null)
			{
				m_RuntimePanel.targetTexture = m_Settings.targetTexture;
			}
		}

		internal void SetSortingPriority()
		{
			if (m_RuntimePanel != null)
			{
				m_RuntimePanel.sortingPriority = m_Settings.m_SortingOrder;
			}
		}

		internal void SetTargetDisplay()
		{
			if (m_RuntimePanel != null)
			{
				m_RuntimePanel.targetDisplay = m_Settings.m_TargetDisplay;
			}
		}

		private BaseRuntimePanel CreateRelatedRuntimePanel()
		{
			return (RuntimePanel)UIElementsRuntimeUtility.FindOrCreateRuntimePanel(m_Settings, RuntimePanel.Create);
		}

		private void DisposeRelatedPanel()
		{
			UIElementsRuntimeUtility.DisposeRuntimePanel(m_Settings);
		}

		internal void MarkPotentiallyEmpty()
		{
			UIElementsRuntimeUtility.MarkPotentiallyEmpty(m_Settings);
		}
	}

	private const int k_DefaultSortingOrder = 0;

	private const float k_DefaultScaleValue = 1f;

	internal const string k_DefaultStyleSheetPath = "Packages/com.unity.ui/PackageResources/StyleSheets/Generated/Default.tss.asset";

	[SerializeField]
	private ThemeStyleSheet themeUss;

	[SerializeField]
	private RenderTexture m_TargetTexture;

	[SerializeField]
	private PanelScaleMode m_ScaleMode = PanelScaleMode.ConstantPhysicalSize;

	[SerializeField]
	private float m_ReferenceSpritePixelsPerUnit = 100f;

	[SerializeField]
	private float m_Scale = 1f;

	private const float DefaultDpi = 96f;

	[SerializeField]
	private float m_ReferenceDpi = 96f;

	[SerializeField]
	private float m_FallbackDpi = 96f;

	[SerializeField]
	private Vector2Int m_ReferenceResolution = new Vector2Int(1200, 800);

	[SerializeField]
	private PanelScreenMatchMode m_ScreenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_Match = 0f;

	[SerializeField]
	private float m_SortingOrder = 0f;

	[SerializeField]
	private int m_TargetDisplay = 0;

	[SerializeField]
	private bool m_ClearDepthStencil = true;

	[SerializeField]
	private bool m_ClearColor;

	[SerializeField]
	private Color m_ColorClearValue = Color.clear;

	private RuntimePanelAccess m_PanelAccess;

	internal UIDocumentList m_AttachedUIDocumentsList;

	[SerializeField]
	[HideInInspector]
	private DynamicAtlasSettings m_DynamicAtlasSettings = DynamicAtlasSettings.defaults;

	[SerializeField]
	[HideInInspector]
	private Shader m_AtlasBlitShader;

	[SerializeField]
	[HideInInspector]
	private Shader m_RuntimeShader;

	[HideInInspector]
	[SerializeField]
	private Shader m_RuntimeWorldShader;

	[SerializeField]
	public PanelTextSettings textSettings;

	private Rect m_TargetRect;

	private float m_ResolvedScale;

	private StyleSheet m_OldThemeUss;

	private Func<Vector2, Vector2> m_AssignedScreenToPanel;

	public ThemeStyleSheet themeStyleSheet
	{
		get
		{
			return themeUss;
		}
		set
		{
			themeUss = value;
			ApplyThemeStyleSheet();
		}
	}

	public RenderTexture targetTexture
	{
		get
		{
			return m_TargetTexture;
		}
		set
		{
			m_TargetTexture = value;
			m_PanelAccess.SetTargetTexture();
		}
	}

	public PanelScaleMode scaleMode
	{
		get
		{
			return m_ScaleMode;
		}
		set
		{
			m_ScaleMode = value;
		}
	}

	public float referenceSpritePixelsPerUnit
	{
		get
		{
			return m_ReferenceSpritePixelsPerUnit;
		}
		set
		{
			m_ReferenceSpritePixelsPerUnit = value;
		}
	}

	public float scale
	{
		get
		{
			return m_Scale;
		}
		set
		{
			m_Scale = value;
		}
	}

	public float referenceDpi
	{
		get
		{
			return m_ReferenceDpi;
		}
		set
		{
			m_ReferenceDpi = ((value >= 1f) ? value : 96f);
		}
	}

	public float fallbackDpi
	{
		get
		{
			return m_FallbackDpi;
		}
		set
		{
			m_FallbackDpi = ((value >= 1f) ? value : 96f);
		}
	}

	public Vector2Int referenceResolution
	{
		get
		{
			return m_ReferenceResolution;
		}
		set
		{
			m_ReferenceResolution = value;
		}
	}

	public PanelScreenMatchMode screenMatchMode
	{
		get
		{
			return m_ScreenMatchMode;
		}
		set
		{
			m_ScreenMatchMode = value;
		}
	}

	public float match
	{
		get
		{
			return m_Match;
		}
		set
		{
			m_Match = value;
		}
	}

	public float sortingOrder
	{
		get
		{
			return m_SortingOrder;
		}
		set
		{
			m_SortingOrder = value;
			ApplySortingOrder();
		}
	}

	public int targetDisplay
	{
		get
		{
			return m_TargetDisplay;
		}
		set
		{
			m_TargetDisplay = value;
			m_PanelAccess.SetTargetDisplay();
		}
	}

	public bool clearDepthStencil
	{
		get
		{
			return m_ClearDepthStencil;
		}
		set
		{
			m_ClearDepthStencil = value;
		}
	}

	public float depthClearValue => 0.99f;

	public bool clearColor
	{
		get
		{
			return m_ClearColor;
		}
		set
		{
			m_ClearColor = value;
		}
	}

	public Color colorClearValue
	{
		get
		{
			return m_ColorClearValue;
		}
		set
		{
			m_ColorClearValue = value;
		}
	}

	internal BaseRuntimePanel panel => m_PanelAccess.panel;

	internal bool isInitialized => m_PanelAccess?.isInitialized ?? false;

	internal VisualElement visualTree => m_PanelAccess.panel.visualTree;

	public DynamicAtlasSettings dynamicAtlasSettings
	{
		get
		{
			return m_DynamicAtlasSettings;
		}
		set
		{
			m_DynamicAtlasSettings = value;
		}
	}

	private float ScreenDPI { get; set; }

	internal void ApplySortingOrder()
	{
		m_PanelAccess.SetSortingPriority();
	}

	private PanelSettings()
	{
		m_PanelAccess = new RuntimePanelAccess(this);
	}

	private void Reset()
	{
	}

	private void OnEnable()
	{
		if (themeUss == null)
		{
			Debug.LogWarning("No Theme Style Sheet set to PanelSettings " + base.name + ", UI will not render properly", this);
		}
		UpdateScreenDPI();
		InitializeShaders();
	}

	private void OnDisable()
	{
		m_PanelAccess.DisposePanel();
	}

	internal void DisposePanel()
	{
		m_PanelAccess.DisposePanel();
	}

	internal void UpdateScreenDPI()
	{
		ScreenDPI = Screen.dpi;
	}

	private void ApplyThemeStyleSheet(VisualElement root = null)
	{
		if (m_PanelAccess.isInitialized)
		{
			if (root == null)
			{
				root = visualTree;
			}
			if (m_OldThemeUss != themeUss && m_OldThemeUss != null)
			{
				root?.styleSheets.Remove(m_OldThemeUss);
			}
			if (themeUss != null)
			{
				themeUss.isDefaultStyleSheet = true;
				root?.styleSheets.Add(themeUss);
			}
			m_OldThemeUss = themeUss;
		}
	}

	private void InitializeShaders()
	{
		if (m_AtlasBlitShader == null)
		{
			m_AtlasBlitShader = Shader.Find(Shaders.k_AtlasBlit);
		}
		if (m_RuntimeShader == null)
		{
			m_RuntimeShader = Shader.Find(Shaders.k_Runtime);
		}
		if (m_RuntimeWorldShader == null)
		{
			m_RuntimeWorldShader = Shader.Find(Shaders.k_RuntimeWorld);
		}
		m_PanelAccess.SetTargetTexture();
	}

	internal void ApplyPanelSettings()
	{
		Rect targetRect = m_TargetRect;
		float resolvedScale = m_ResolvedScale;
		UpdateScreenDPI();
		m_TargetRect = GetDisplayRect();
		m_ResolvedScale = ResolveScale(m_TargetRect, ScreenDPI);
		if (visualTree.style.width.value == 0f || m_ResolvedScale != resolvedScale || m_TargetRect.width != targetRect.width || m_TargetRect.height != targetRect.height)
		{
			panel.scale = ((m_ResolvedScale == 0f) ? 0f : (1f / m_ResolvedScale));
			visualTree.style.left = 0f;
			visualTree.style.top = 0f;
			visualTree.style.width = m_TargetRect.width * m_ResolvedScale;
			visualTree.style.height = m_TargetRect.height * m_ResolvedScale;
		}
		panel.targetTexture = targetTexture;
		panel.targetDisplay = targetDisplay;
		panel.drawToCameras = false;
		panel.clearSettings = new PanelClearSettings
		{
			clearColor = m_ClearColor,
			clearDepthStencil = m_ClearDepthStencil,
			color = m_ColorClearValue
		};
		panel.referenceSpritePixelsPerUnit = referenceSpritePixelsPerUnit;
		if (panel.atlas is DynamicAtlas dynamicAtlas)
		{
			dynamicAtlas.minAtlasSize = dynamicAtlasSettings.minAtlasSize;
			dynamicAtlas.maxAtlasSize = dynamicAtlasSettings.maxAtlasSize;
			dynamicAtlas.maxSubTextureSize = dynamicAtlasSettings.maxSubTextureSize;
			dynamicAtlas.activeFilters = dynamicAtlasSettings.activeFilters;
			dynamicAtlas.customFilter = dynamicAtlasSettings.customFilter;
		}
	}

	public void SetScreenToPanelSpaceFunction(Func<Vector2, Vector2> screentoPanelSpaceFunction)
	{
		m_AssignedScreenToPanel = screentoPanelSpaceFunction;
		panel.screenToPanelSpace = m_AssignedScreenToPanel;
	}

	internal float ResolveScale(Rect targetRect, float screenDpi)
	{
		float num = 1f;
		switch (scaleMode)
		{
		case PanelScaleMode.ConstantPhysicalSize:
		{
			float num3 = ((screenDpi == 0f) ? fallbackDpi : screenDpi);
			if (num3 != 0f)
			{
				num = referenceDpi / num3;
			}
			break;
		}
		case PanelScaleMode.ScaleWithScreenSize:
			if (referenceResolution.x * referenceResolution.y != 0)
			{
				Vector2 vector = referenceResolution;
				Vector2 vector2 = new Vector2(targetRect.width / vector.x, targetRect.height / vector.y);
				float num2 = 0f;
				switch (screenMatchMode)
				{
				case PanelScreenMatchMode.Expand:
					num2 = Mathf.Min(vector2.x, vector2.y);
					break;
				case PanelScreenMatchMode.Shrink:
					num2 = Mathf.Max(vector2.x, vector2.y);
					break;
				default:
				{
					float t = Mathf.Clamp01(match);
					num2 = Mathf.Lerp(vector2.x, vector2.y, t);
					break;
				}
				}
				if (num2 != 0f)
				{
					num = 1f / num2;
				}
			}
			break;
		}
		if (scale > 0f)
		{
			return num / scale;
		}
		return 0f;
	}

	internal Rect GetDisplayRect()
	{
		if (m_TargetTexture != null)
		{
			return new Rect(0f, 0f, m_TargetTexture.width, m_TargetTexture.height);
		}
		return new Rect(0f, 0f, BaseRuntimePanel.getScreenRenderingWidth(targetDisplay), BaseRuntimePanel.getScreenRenderingHeight(targetDisplay));
	}

	internal void AttachAndInsertUIDocumentToVisualTree(UIDocument uiDocument)
	{
		if (m_AttachedUIDocumentsList == null)
		{
			m_AttachedUIDocumentsList = new UIDocumentList();
		}
		else
		{
			m_AttachedUIDocumentsList.RemoveFromListAndFromVisualTree(uiDocument);
		}
		m_AttachedUIDocumentsList.AddToListAndToVisualTree(uiDocument, visualTree);
	}

	internal void DetachUIDocument(UIDocument uiDocument)
	{
		if (m_AttachedUIDocumentsList != null)
		{
			m_AttachedUIDocumentsList.RemoveFromListAndFromVisualTree(uiDocument);
			if (m_AttachedUIDocumentsList.m_AttachedUIDocuments.Count == 0)
			{
				m_PanelAccess.MarkPotentiallyEmpty();
			}
		}
	}
}
