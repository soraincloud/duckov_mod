#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine.UIElements.UIR.Implementation;

namespace UnityEngine.UIElements.UIR;

internal class RenderChain : IDisposable
{
	private struct DepthOrderedDirtyTracking
	{
		public List<VisualElement> heads;

		public List<VisualElement> tails;

		public int[] minDepths;

		public int[] maxDepths;

		public uint dirtyID;

		public void EnsureFits(int maxDepth)
		{
			while (heads.Count <= maxDepth)
			{
				heads.Add(null);
				tails.Add(null);
			}
		}

		public void RegisterDirty(VisualElement ve, RenderDataDirtyTypes dirtyTypes, RenderDataDirtyTypeClasses dirtyTypeClass)
		{
			Debug.Assert(dirtyTypes != RenderDataDirtyTypes.None);
			int hierarchyDepth = ve.renderChainData.hierarchyDepth;
			minDepths[(int)dirtyTypeClass] = ((hierarchyDepth < minDepths[(int)dirtyTypeClass]) ? hierarchyDepth : minDepths[(int)dirtyTypeClass]);
			maxDepths[(int)dirtyTypeClass] = ((hierarchyDepth > maxDepths[(int)dirtyTypeClass]) ? hierarchyDepth : maxDepths[(int)dirtyTypeClass]);
			if (ve.renderChainData.dirtiedValues != RenderDataDirtyTypes.None)
			{
				ve.renderChainData.dirtiedValues |= dirtyTypes;
				return;
			}
			ve.renderChainData.dirtiedValues = dirtyTypes;
			if (tails[hierarchyDepth] != null)
			{
				tails[hierarchyDepth].renderChainData.nextDirty = ve;
				ve.renderChainData.prevDirty = tails[hierarchyDepth];
				tails[hierarchyDepth] = ve;
			}
			else
			{
				List<VisualElement> list = heads;
				VisualElement value = (tails[hierarchyDepth] = ve);
				list[hierarchyDepth] = value;
			}
		}

		public void ClearDirty(VisualElement ve, RenderDataDirtyTypes dirtyTypesInverse)
		{
			Debug.Assert(ve.renderChainData.dirtiedValues != RenderDataDirtyTypes.None);
			ve.renderChainData.dirtiedValues &= dirtyTypesInverse;
			if (ve.renderChainData.dirtiedValues == RenderDataDirtyTypes.None)
			{
				if (ve.renderChainData.prevDirty != null)
				{
					ve.renderChainData.prevDirty.renderChainData.nextDirty = ve.renderChainData.nextDirty;
				}
				if (ve.renderChainData.nextDirty != null)
				{
					ve.renderChainData.nextDirty.renderChainData.prevDirty = ve.renderChainData.prevDirty;
				}
				if (tails[ve.renderChainData.hierarchyDepth] == ve)
				{
					Debug.Assert(ve.renderChainData.nextDirty == null);
					tails[ve.renderChainData.hierarchyDepth] = ve.renderChainData.prevDirty;
				}
				if (heads[ve.renderChainData.hierarchyDepth] == ve)
				{
					Debug.Assert(ve.renderChainData.prevDirty == null);
					heads[ve.renderChainData.hierarchyDepth] = ve.renderChainData.nextDirty;
				}
				ve.renderChainData.prevDirty = (ve.renderChainData.nextDirty = null);
			}
		}

		public void Reset()
		{
			for (int i = 0; i < minDepths.Length; i++)
			{
				minDepths[i] = int.MaxValue;
				maxDepths[i] = int.MinValue;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct RenderChainStaticIndexAllocator
	{
		private static List<RenderChain> renderChains = new List<RenderChain>(4);

		public static int AllocateIndex(RenderChain renderChain)
		{
			int num = renderChains.IndexOf(null);
			if (num >= 0)
			{
				renderChains[num] = renderChain;
			}
			else
			{
				num = renderChains.Count;
				renderChains.Add(renderChain);
			}
			return num;
		}

		public static void FreeIndex(int index)
		{
			renderChains[index] = null;
		}

		public static RenderChain AccessIndex(int index)
		{
			return renderChains[index];
		}
	}

	private struct RenderNodeData
	{
		public Material standardMaterial;

		public Material initialMaterial;

		public MaterialPropertyBlock matPropBlock;

		public RenderChainCommand firstCommand;

		public UIRenderDevice device;

		public Texture vectorAtlas;

		public Texture shaderInfoAtlas;

		public float dpiScale;

		public NativeSlice<Transform3x4> transformConstants;

		public NativeSlice<Vector4> clipRectConstants;
	}

	private RenderChainCommand m_FirstCommand;

	private DepthOrderedDirtyTracking m_DirtyTracker;

	private LinkedPool<RenderChainCommand> m_CommandPool = new LinkedPool<RenderChainCommand>(() => new RenderChainCommand(), delegate
	{
	});

	private BasicNodePool<TextureEntry> m_TexturePool = new BasicNodePool<TextureEntry>();

	private List<RenderNodeData> m_RenderNodesData = new List<RenderNodeData>();

	private Shader m_DefaultShader;

	private Shader m_DefaultWorldSpaceShader;

	private Material m_DefaultMat;

	private Material m_DefaultWorldSpaceMat;

	private bool m_BlockDirtyRegistration;

	private int m_StaticIndex = -1;

	private int m_ActiveRenderNodes = 0;

	private int m_CustomMaterialCommands = 0;

	private ChainBuilderStats m_Stats;

	private uint m_StatsElementsAdded;

	private uint m_StatsElementsRemoved;

	private TextureRegistry m_TextureRegistry = TextureRegistry.instance;

	private static ProfilerMarker s_MarkerProcess;

	private static ProfilerMarker s_MarkerClipProcessing;

	private static ProfilerMarker s_MarkerOpacityProcessing;

	private static ProfilerMarker s_MarkerColorsProcessing;

	private static ProfilerMarker s_MarkerTransformProcessing;

	private static ProfilerMarker s_MarkerVisualsProcessing;

	private static ProfilerMarker s_MarkerTextRegen;

	internal static Action OnPreRender;

	internal UIRVEShaderInfoAllocator shaderInfoAllocator;

	internal RenderChainCommand firstCommand => m_FirstCommand;

	public OpacityIdAccelerator opacityIdAccelerator { get; private set; }

	protected bool disposed { get; private set; }

	internal ChainBuilderStats stats => m_Stats;

	internal BaseVisualElementPanel panel { get; private set; }

	internal UIRenderDevice device { get; private set; }

	internal AtlasBase atlas { get; private set; }

	internal VectorImageManager vectorImageManager { get; private set; }

	internal TempAllocator<Vertex> vertsPool { get; private set; }

	internal TempAllocator<ushort> indicesPool { get; private set; }

	internal JobManager jobManager { get; private set; }

	internal UIRStylePainter painter { get; private set; }

	internal bool drawStats { get; set; }

	internal bool drawInCameras { get; private set; }

	internal Shader defaultShader
	{
		get
		{
			return m_DefaultShader;
		}
		set
		{
			if (!(m_DefaultShader == value))
			{
				m_DefaultShader = value;
				UIRUtility.Destroy(m_DefaultMat);
				m_DefaultMat = null;
			}
		}
	}

	internal Shader defaultWorldSpaceShader
	{
		get
		{
			return m_DefaultWorldSpaceShader;
		}
		set
		{
			if (!(m_DefaultWorldSpaceShader == value))
			{
				m_DefaultWorldSpaceShader = value;
				UIRUtility.Destroy(m_DefaultWorldSpaceMat);
				m_DefaultWorldSpaceMat = null;
			}
		}
	}

	static RenderChain()
	{
		s_MarkerProcess = new ProfilerMarker("RenderChain.Process");
		s_MarkerClipProcessing = new ProfilerMarker("RenderChain.UpdateClips");
		s_MarkerOpacityProcessing = new ProfilerMarker("RenderChain.UpdateOpacity");
		s_MarkerColorsProcessing = new ProfilerMarker("RenderChain.UpdateColors");
		s_MarkerTransformProcessing = new ProfilerMarker("RenderChain.UpdateTransforms");
		s_MarkerVisualsProcessing = new ProfilerMarker("RenderChain.UpdateVisuals");
		s_MarkerTextRegen = new ProfilerMarker("RenderChain.RegenText");
		OnPreRender = null;
		Utility.RegisterIntermediateRenderers += OnRegisterIntermediateRenderers;
		Utility.RenderNodeExecute += OnRenderNodeExecute;
	}

	public RenderChain(BaseVisualElementPanel panel)
	{
		Constructor(panel, new UIRenderDevice(), panel.atlas, new VectorImageManager(panel.atlas));
	}

	protected RenderChain(BaseVisualElementPanel panel, UIRenderDevice device, AtlasBase atlas, VectorImageManager vectorImageManager)
	{
		Constructor(panel, device, atlas, vectorImageManager);
	}

	private void Constructor(BaseVisualElementPanel panelObj, UIRenderDevice deviceObj, AtlasBase atlas, VectorImageManager vectorImageMan)
	{
		if (disposed)
		{
			DisposeHelper.NotifyDisposedUsed(this);
		}
		m_DirtyTracker.heads = new List<VisualElement>(8);
		m_DirtyTracker.tails = new List<VisualElement>(8);
		m_DirtyTracker.minDepths = new int[5];
		m_DirtyTracker.maxDepths = new int[5];
		m_DirtyTracker.Reset();
		if (m_RenderNodesData.Count < 1)
		{
			m_RenderNodesData.Add(new RenderNodeData
			{
				matPropBlock = new MaterialPropertyBlock()
			});
		}
		panel = panelObj;
		device = deviceObj;
		this.atlas = atlas;
		vectorImageManager = vectorImageMan;
		vertsPool = new TempAllocator<Vertex>(8192, 2048, 65536);
		indicesPool = new TempAllocator<ushort>(16384, 4096, 131072);
		jobManager = new JobManager();
		shaderInfoAllocator.Construct();
		opacityIdAccelerator = new OpacityIdAccelerator();
		painter = new UIRStylePainter(this);
		if (panel is BaseRuntimePanel { drawToCameras: not false })
		{
			drawInCameras = true;
			m_StaticIndex = RenderChainStaticIndexAllocator.AllocateIndex(this);
		}
	}

	private void Destructor()
	{
		if (m_StaticIndex >= 0)
		{
			RenderChainStaticIndexAllocator.FreeIndex(m_StaticIndex);
		}
		m_StaticIndex = -1;
		for (VisualElement visualElement = GetFirstElementInPanel(m_FirstCommand?.owner); visualElement != null; visualElement = visualElement.renderChainData.next)
		{
			ResetTextures(visualElement);
		}
		UIRUtility.Destroy(m_DefaultMat);
		UIRUtility.Destroy(m_DefaultWorldSpaceMat);
		m_DefaultMat = (m_DefaultWorldSpaceMat = null);
		vertsPool.Dispose();
		indicesPool.Dispose();
		jobManager.Dispose();
		vectorImageManager?.Dispose();
		shaderInfoAllocator.Dispose();
		device?.Dispose();
		opacityIdAccelerator?.Dispose();
		if (painter != null)
		{
			if (painter.meshGenerationContext.hasPainter2D)
			{
				painter.meshGenerationContext.painter2D.Dispose();
			}
			painter = null;
		}
		atlas = null;
		shaderInfoAllocator = default(UIRVEShaderInfoAllocator);
		device = null;
		m_ActiveRenderNodes = 0;
		m_RenderNodesData.Clear();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected void Dispose(bool disposing)
	{
		if (!disposed)
		{
			if (disposing)
			{
				Destructor();
			}
			disposed = true;
		}
	}

	public void ProcessChanges()
	{
		m_Stats = default(ChainBuilderStats);
		m_Stats.elementsAdded += m_StatsElementsAdded;
		m_Stats.elementsRemoved += m_StatsElementsRemoved;
		m_StatsElementsAdded = (m_StatsElementsRemoved = 0u);
		m_DirtyTracker.dirtyID++;
		int num = 0;
		RenderDataDirtyTypes renderDataDirtyTypes = RenderDataDirtyTypes.Clipping | RenderDataDirtyTypes.ClippingHierarchy;
		RenderDataDirtyTypes dirtyTypesInverse = ~renderDataDirtyTypes;
		for (int i = m_DirtyTracker.minDepths[num]; i <= m_DirtyTracker.maxDepths[num]; i++)
		{
			VisualElement visualElement = m_DirtyTracker.heads[i];
			while (visualElement != null)
			{
				VisualElement nextDirty = visualElement.renderChainData.nextDirty;
				if ((visualElement.renderChainData.dirtiedValues & renderDataDirtyTypes) != RenderDataDirtyTypes.None)
				{
					if (visualElement.renderChainData.isInChain && visualElement.renderChainData.dirtyID != m_DirtyTracker.dirtyID)
					{
						RenderEvents.ProcessOnClippingChanged(this, visualElement, m_DirtyTracker.dirtyID, ref m_Stats);
					}
					m_DirtyTracker.ClearDirty(visualElement, dirtyTypesInverse);
				}
				visualElement = nextDirty;
				m_Stats.dirtyProcessed++;
			}
		}
		m_DirtyTracker.dirtyID++;
		num = 1;
		renderDataDirtyTypes = RenderDataDirtyTypes.Opacity | RenderDataDirtyTypes.OpacityHierarchy;
		dirtyTypesInverse = ~renderDataDirtyTypes;
		for (int j = m_DirtyTracker.minDepths[num]; j <= m_DirtyTracker.maxDepths[num]; j++)
		{
			VisualElement visualElement2 = m_DirtyTracker.heads[j];
			while (visualElement2 != null)
			{
				VisualElement nextDirty2 = visualElement2.renderChainData.nextDirty;
				if ((visualElement2.renderChainData.dirtiedValues & renderDataDirtyTypes) != RenderDataDirtyTypes.None)
				{
					if (visualElement2.renderChainData.isInChain && visualElement2.renderChainData.dirtyID != m_DirtyTracker.dirtyID)
					{
						RenderEvents.ProcessOnOpacityChanged(this, visualElement2, m_DirtyTracker.dirtyID, ref m_Stats);
					}
					m_DirtyTracker.ClearDirty(visualElement2, dirtyTypesInverse);
				}
				visualElement2 = nextDirty2;
				m_Stats.dirtyProcessed++;
			}
		}
		m_DirtyTracker.dirtyID++;
		num = 2;
		renderDataDirtyTypes = RenderDataDirtyTypes.Color;
		dirtyTypesInverse = ~renderDataDirtyTypes;
		for (int k = m_DirtyTracker.minDepths[num]; k <= m_DirtyTracker.maxDepths[num]; k++)
		{
			VisualElement visualElement3 = m_DirtyTracker.heads[k];
			while (visualElement3 != null)
			{
				VisualElement nextDirty3 = visualElement3.renderChainData.nextDirty;
				if ((visualElement3.renderChainData.dirtiedValues & renderDataDirtyTypes) != RenderDataDirtyTypes.None)
				{
					if (visualElement3.renderChainData.isInChain && visualElement3.renderChainData.dirtyID != m_DirtyTracker.dirtyID)
					{
						RenderEvents.ProcessOnColorChanged(this, visualElement3, m_DirtyTracker.dirtyID, ref m_Stats);
					}
					m_DirtyTracker.ClearDirty(visualElement3, dirtyTypesInverse);
				}
				visualElement3 = nextDirty3;
				m_Stats.dirtyProcessed++;
			}
		}
		m_DirtyTracker.dirtyID++;
		num = 3;
		renderDataDirtyTypes = RenderDataDirtyTypes.Transform | RenderDataDirtyTypes.ClipRectSize;
		dirtyTypesInverse = ~renderDataDirtyTypes;
		for (int l = m_DirtyTracker.minDepths[num]; l <= m_DirtyTracker.maxDepths[num]; l++)
		{
			VisualElement visualElement4 = m_DirtyTracker.heads[l];
			while (visualElement4 != null)
			{
				VisualElement nextDirty4 = visualElement4.renderChainData.nextDirty;
				if ((visualElement4.renderChainData.dirtiedValues & renderDataDirtyTypes) != RenderDataDirtyTypes.None)
				{
					if (visualElement4.renderChainData.isInChain && visualElement4.renderChainData.dirtyID != m_DirtyTracker.dirtyID)
					{
						RenderEvents.ProcessOnTransformOrSizeChanged(this, visualElement4, m_DirtyTracker.dirtyID, ref m_Stats);
					}
					m_DirtyTracker.ClearDirty(visualElement4, dirtyTypesInverse);
				}
				visualElement4 = nextDirty4;
				m_Stats.dirtyProcessed++;
			}
		}
		jobManager.CompleteNudgeJobs();
		m_BlockDirtyRegistration = true;
		m_DirtyTracker.dirtyID++;
		num = 4;
		renderDataDirtyTypes = RenderDataDirtyTypes.AllVisuals;
		dirtyTypesInverse = ~renderDataDirtyTypes;
		for (int m = m_DirtyTracker.minDepths[num]; m <= m_DirtyTracker.maxDepths[num]; m++)
		{
			VisualElement visualElement5 = m_DirtyTracker.heads[m];
			while (visualElement5 != null)
			{
				VisualElement nextDirty5 = visualElement5.renderChainData.nextDirty;
				if ((visualElement5.renderChainData.dirtiedValues & renderDataDirtyTypes) != RenderDataDirtyTypes.None)
				{
					if (visualElement5.renderChainData.isInChain && visualElement5.renderChainData.dirtyID != m_DirtyTracker.dirtyID)
					{
						RenderEvents.ProcessOnVisualsChanged(this, visualElement5, m_DirtyTracker.dirtyID, ref m_Stats);
					}
					m_DirtyTracker.ClearDirty(visualElement5, dirtyTypesInverse);
				}
				visualElement5 = nextDirty5;
				m_Stats.dirtyProcessed++;
			}
		}
		jobManager.CompleteConvertMeshJobs();
		jobManager.CompleteClosingMeshJobs();
		opacityIdAccelerator.CompleteJobs();
		m_BlockDirtyRegistration = false;
		vertsPool.Reset();
		indicesPool.Reset();
		m_DirtyTracker.Reset();
		atlas?.InvokeUpdateDynamicTextures(panel);
		vectorImageManager?.Commit();
		shaderInfoAllocator.IssuePendingStorageChanges();
		device?.OnFrameRenderingBegin();
	}

	public void Render()
	{
		Material standardMaterial = GetStandardMaterial();
		panel.InvokeUpdateMaterial(standardMaterial);
		Exception immediateException = null;
		if (m_FirstCommand != null && !drawInCameras)
		{
			Rect layout = panel.visualTree.layout;
			standardMaterial?.SetPass(0);
			Matrix4x4 mat = ProjectionUtils.Ortho(layout.xMin, layout.xMax, layout.yMax, layout.yMin, -0.001f, 1.001f);
			GL.LoadProjectionMatrix(mat);
			GL.modelview = Matrix4x4.identity;
			device.EvaluateChain(m_FirstCommand, standardMaterial, standardMaterial, vectorImageManager?.atlas, shaderInfoAllocator.atlas, panel.scaledPixelsPerPoint, shaderInfoAllocator.transformConstants, shaderInfoAllocator.clipRectConstants, m_RenderNodesData[0].matPropBlock, allowMaterialChange: true, ref immediateException);
		}
		if (immediateException != null)
		{
			if (GUIUtility.IsExitGUIException(immediateException))
			{
				throw immediateException;
			}
			throw new ImmediateModeException(immediateException);
		}
		if (drawStats)
		{
			DrawStats();
		}
	}

	public void UIEOnChildAdded(VisualElement ve)
	{
		VisualElement parent = ve.hierarchy.parent;
		int index = parent?.hierarchy.IndexOf(ve) ?? 0;
		if (m_BlockDirtyRegistration)
		{
			throw new InvalidOperationException("VisualElements cannot be added to an active visual tree during generateVisualContent callback execution nor during visual tree rendering");
		}
		if (parent == null || parent.renderChainData.isInChain)
		{
			uint num = RenderEvents.DepthFirstOnChildAdded(this, parent, ve, index, resetState: true);
			Debug.Assert(ve.renderChainData.isInChain);
			Debug.Assert(ve.panel == panel);
			UIEOnClippingChanged(ve, hierarchical: true);
			UIEOnOpacityChanged(ve);
			UIEOnVisualsChanged(ve, hierarchical: true);
			m_StatsElementsAdded += num;
		}
	}

	public void UIEOnChildrenReordered(VisualElement ve)
	{
		if (m_BlockDirtyRegistration)
		{
			throw new InvalidOperationException("VisualElements cannot be moved under an active visual tree during generateVisualContent callback execution nor during visual tree rendering");
		}
		int childCount = ve.hierarchy.childCount;
		for (int i = 0; i < childCount; i++)
		{
			RenderEvents.DepthFirstOnChildRemoving(this, ve.hierarchy[i]);
		}
		for (int j = 0; j < childCount; j++)
		{
			RenderEvents.DepthFirstOnChildAdded(this, ve, ve.hierarchy[j], j, resetState: false);
		}
		UIEOnClippingChanged(ve, hierarchical: true);
		UIEOnOpacityChanged(ve, hierarchical: true);
		UIEOnVisualsChanged(ve, hierarchical: true);
	}

	public void UIEOnChildRemoving(VisualElement ve)
	{
		if (m_BlockDirtyRegistration)
		{
			throw new InvalidOperationException("VisualElements cannot be removed from an active visual tree during generateVisualContent callback execution nor during visual tree rendering");
		}
		m_StatsElementsRemoved += RenderEvents.DepthFirstOnChildRemoving(this, ve);
		Debug.Assert(!ve.renderChainData.isInChain);
	}

	public void UIEOnRenderHintsChanged(VisualElement ve)
	{
		if (ve.renderChainData.isInChain)
		{
			if (m_BlockDirtyRegistration)
			{
				throw new InvalidOperationException("Render Hints cannot change under an active visual tree during generateVisualContent callback execution nor during visual tree rendering");
			}
			if ((ve.renderHints & RenderHints.DirtyAll) == RenderHints.DirtyDynamicColor)
			{
				UIEOnVisualsChanged(ve, hierarchical: false);
			}
			else
			{
				UIEOnChildRemoving(ve);
				UIEOnChildAdded(ve);
			}
			ve.MarkRenderHintsClean();
		}
	}

	public void UIEOnClippingChanged(VisualElement ve, bool hierarchical)
	{
		if (ve.renderChainData.isInChain)
		{
			if (m_BlockDirtyRegistration)
			{
				throw new InvalidOperationException("VisualElements cannot change clipping state under an active visual tree during generateVisualContent callback execution nor during visual tree rendering");
			}
			m_DirtyTracker.RegisterDirty(ve, (RenderDataDirtyTypes)(4 | (hierarchical ? 8 : 0)), RenderDataDirtyTypeClasses.Clipping);
		}
	}

	public void UIEOnOpacityChanged(VisualElement ve, bool hierarchical = false)
	{
		if (ve.renderChainData.isInChain)
		{
			if (m_BlockDirtyRegistration)
			{
				throw new InvalidOperationException("VisualElements cannot change opacity under an active visual tree during generateVisualContent callback execution nor during visual tree rendering");
			}
			m_DirtyTracker.RegisterDirty(ve, (RenderDataDirtyTypes)(0x80 | (hierarchical ? 256 : 0)), RenderDataDirtyTypeClasses.Opacity);
		}
	}

	public void UIEOnColorChanged(VisualElement ve)
	{
		if (ve.renderChainData.isInChain)
		{
			if (m_BlockDirtyRegistration)
			{
				throw new InvalidOperationException("VisualElements cannot change background color under an active visual tree during generateVisualContent callback execution nor during visual tree rendering");
			}
			m_DirtyTracker.RegisterDirty(ve, RenderDataDirtyTypes.Color, RenderDataDirtyTypeClasses.Color);
		}
	}

	public void UIEOnTransformOrSizeChanged(VisualElement ve, bool transformChanged, bool clipRectSizeChanged)
	{
		if (ve.renderChainData.isInChain)
		{
			if (m_BlockDirtyRegistration)
			{
				throw new InvalidOperationException("VisualElements cannot change size or transform under an active visual tree during generateVisualContent callback execution nor during visual tree rendering");
			}
			RenderDataDirtyTypes dirtyTypes = (RenderDataDirtyTypes)((transformChanged ? 1 : 0) | (clipRectSizeChanged ? 2 : 0));
			m_DirtyTracker.RegisterDirty(ve, dirtyTypes, RenderDataDirtyTypeClasses.TransformSize);
		}
	}

	public void UIEOnVisualsChanged(VisualElement ve, bool hierarchical)
	{
		if (ve.renderChainData.isInChain)
		{
			if (m_BlockDirtyRegistration)
			{
				throw new InvalidOperationException("VisualElements cannot be marked for dirty repaint under an active visual tree during generateVisualContent callback execution nor during visual tree rendering");
			}
			m_DirtyTracker.RegisterDirty(ve, (RenderDataDirtyTypes)(0x10 | (hierarchical ? 32 : 0)), RenderDataDirtyTypeClasses.Visuals);
		}
	}

	public void UIEOnOpacityIdChanged(VisualElement ve)
	{
		if (ve.renderChainData.isInChain)
		{
			if (m_BlockDirtyRegistration)
			{
				throw new InvalidOperationException("VisualElements cannot for opacity id change under an active visual tree during generateVisualContent callback execution nor during visual tree rendering");
			}
			m_DirtyTracker.RegisterDirty(ve, RenderDataDirtyTypes.VisualsOpacityId, RenderDataDirtyTypeClasses.Visuals);
		}
	}

	internal Material GetStandardMaterial()
	{
		if (m_DefaultMat == null && m_DefaultShader != null)
		{
			m_DefaultMat = new Material(m_DefaultShader);
			m_DefaultMat.hideFlags |= HideFlags.DontSaveInEditor;
		}
		return m_DefaultMat;
	}

	internal Material GetStandardWorldSpaceMaterial()
	{
		if (m_DefaultWorldSpaceMat == null && m_DefaultWorldSpaceShader != null)
		{
			m_DefaultWorldSpaceMat = new Material(m_DefaultWorldSpaceShader);
			m_DefaultWorldSpaceMat.hideFlags |= HideFlags.DontSaveInEditor;
		}
		return m_DefaultWorldSpaceMat;
	}

	internal void EnsureFitsDepth(int depth)
	{
		m_DirtyTracker.EnsureFits(depth);
	}

	internal void ChildWillBeRemoved(VisualElement ve)
	{
		if (ve.renderChainData.dirtiedValues != RenderDataDirtyTypes.None)
		{
			m_DirtyTracker.ClearDirty(ve, ~ve.renderChainData.dirtiedValues);
		}
		Debug.Assert(ve.renderChainData.dirtiedValues == RenderDataDirtyTypes.None);
		Debug.Assert(ve.renderChainData.prevDirty == null);
		Debug.Assert(ve.renderChainData.nextDirty == null);
	}

	internal RenderChainCommand AllocCommand()
	{
		RenderChainCommand renderChainCommand = m_CommandPool.Get();
		renderChainCommand.Reset();
		return renderChainCommand;
	}

	internal void FreeCommand(RenderChainCommand cmd)
	{
		if (cmd.state.material != null)
		{
			m_CustomMaterialCommands--;
		}
		cmd.Reset();
		m_CommandPool.Return(cmd);
	}

	internal void OnRenderCommandAdded(RenderChainCommand command)
	{
		if (command.prev == null)
		{
			m_FirstCommand = command;
		}
		if (command.state.material != null)
		{
			m_CustomMaterialCommands++;
		}
	}

	internal void OnRenderCommandsRemoved(RenderChainCommand firstCommand, RenderChainCommand lastCommand)
	{
		if (firstCommand.prev == null)
		{
			m_FirstCommand = lastCommand.next;
		}
	}

	private unsafe static RenderNodeData AccessRenderNodeData(IntPtr obj)
	{
		int* ptr = (int*)obj.ToPointer();
		RenderChain renderChain = RenderChainStaticIndexAllocator.AccessIndex(*ptr);
		return renderChain.m_RenderNodesData[ptr[1]];
	}

	private static void OnRenderNodeExecute(IntPtr obj)
	{
		RenderNodeData renderNodeData = AccessRenderNodeData(obj);
		Exception immediateException = null;
		renderNodeData.device.EvaluateChain(renderNodeData.firstCommand, renderNodeData.initialMaterial, renderNodeData.standardMaterial, renderNodeData.vectorAtlas, renderNodeData.shaderInfoAtlas, renderNodeData.dpiScale, renderNodeData.transformConstants, renderNodeData.clipRectConstants, renderNodeData.matPropBlock, allowMaterialChange: false, ref immediateException);
	}

	private static void OnRegisterIntermediateRenderers(Camera camera)
	{
		int num = 0;
		Dictionary<int, Panel>.Enumerator panelsIterator = UIElementsUtility.GetPanelsIterator();
		while (panelsIterator.MoveNext())
		{
			Panel value = panelsIterator.Current.Value;
			RenderChain renderChain = (value.GetUpdater(VisualTreeUpdatePhase.Repaint) as UIRRepaintUpdater)?.renderChain;
			if (renderChain == null || renderChain.m_StaticIndex < 0 || renderChain.m_FirstCommand == null)
			{
				continue;
			}
			BaseRuntimePanel baseRuntimePanel = (BaseRuntimePanel)value;
			Material standardWorldSpaceMaterial = renderChain.GetStandardWorldSpaceMaterial();
			RenderNodeData rnd = new RenderNodeData
			{
				device = renderChain.device,
				standardMaterial = standardWorldSpaceMaterial,
				vectorAtlas = renderChain.vectorImageManager?.atlas,
				shaderInfoAtlas = renderChain.shaderInfoAllocator.atlas,
				dpiScale = baseRuntimePanel.scaledPixelsPerPoint,
				transformConstants = renderChain.shaderInfoAllocator.transformConstants,
				clipRectConstants = renderChain.shaderInfoAllocator.clipRectConstants
			};
			if (renderChain.m_CustomMaterialCommands == 0)
			{
				rnd.initialMaterial = standardWorldSpaceMaterial;
				rnd.firstCommand = renderChain.m_FirstCommand;
				OnRegisterIntermediateRendererMat(baseRuntimePanel, renderChain, ref rnd, camera, num++);
				continue;
			}
			Material material = null;
			RenderChainCommand next = renderChain.m_FirstCommand;
			RenderChainCommand renderChainCommand = next;
			while (next != null)
			{
				if (next.type != CommandType.Draw)
				{
					next = next.next;
					continue;
				}
				Material material2 = ((next.state.material == null) ? standardWorldSpaceMaterial : next.state.material);
				if (material2 != material)
				{
					if (material != null)
					{
						rnd.initialMaterial = material;
						rnd.firstCommand = renderChainCommand;
						OnRegisterIntermediateRendererMat(baseRuntimePanel, renderChain, ref rnd, camera, num++);
						renderChainCommand = next;
					}
					material = material2;
				}
				next = next.next;
			}
			if (renderChainCommand != null)
			{
				rnd.initialMaterial = material;
				rnd.firstCommand = renderChainCommand;
				OnRegisterIntermediateRendererMat(baseRuntimePanel, renderChain, ref rnd, camera, num++);
			}
		}
	}

	private unsafe static void OnRegisterIntermediateRendererMat(BaseRuntimePanel rtp, RenderChain renderChain, ref RenderNodeData rnd, Camera camera, int sameDistanceSortPriority)
	{
		int num = renderChain.m_ActiveRenderNodes++;
		if (num < renderChain.m_RenderNodesData.Count)
		{
			rnd.matPropBlock = renderChain.m_RenderNodesData[num].matPropBlock;
			renderChain.m_RenderNodesData[num] = rnd;
		}
		else
		{
			rnd.matPropBlock = new MaterialPropertyBlock();
			num = renderChain.m_RenderNodesData.Count;
			renderChain.m_RenderNodesData.Add(rnd);
		}
		int* ptr = stackalloc int[2];
		*ptr = renderChain.m_StaticIndex;
		ptr[1] = num;
		Utility.RegisterIntermediateRenderer(camera, rnd.initialMaterial, rtp.panelToWorld, new Bounds(Vector3.zero, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue)), 3, 0, receiveShadows: false, sameDistanceSortPriority, (ulong)camera.cullingMask, 2, new IntPtr(ptr), 8);
	}

	internal void RepaintTexturedElements()
	{
		for (VisualElement visualElement = GetFirstElementInPanel(m_FirstCommand?.owner); visualElement != null; visualElement = visualElement.renderChainData.next)
		{
			if (visualElement.renderChainData.textures != null)
			{
				UIEOnVisualsChanged(visualElement, hierarchical: false);
			}
		}
		UIEOnOpacityChanged(panel.visualTree);
	}

	public void InsertTexture(VisualElement ve, Texture src, TextureId id, bool isAtlas)
	{
		BasicNode<TextureEntry> basicNode = m_TexturePool.Get();
		basicNode.data.source = src;
		basicNode.data.actual = id;
		basicNode.data.replaced = isAtlas;
		basicNode.InsertFirst(ref ve.renderChainData.textures);
	}

	public void ResetTextures(VisualElement ve)
	{
		AtlasBase atlasBase = atlas;
		TextureRegistry textureRegistry = m_TextureRegistry;
		BasicNodePool<TextureEntry> texturePool = m_TexturePool;
		BasicNode<TextureEntry> basicNode = ve.renderChainData.textures;
		ve.renderChainData.textures = null;
		while (basicNode != null)
		{
			BasicNode<TextureEntry> next = basicNode.next;
			if (basicNode.data.replaced)
			{
				atlasBase.ReturnAtlas(ve, basicNode.data.source as Texture2D, basicNode.data.actual);
			}
			else
			{
				textureRegistry.Release(basicNode.data.actual);
			}
			texturePool.Return(basicNode);
			basicNode = next;
		}
	}

	private void DrawStats()
	{
		bool flag = device != null;
		float num = 12f;
		Rect position = new Rect(30f, 60f, 1000f, 100f);
		GUI.Box(new Rect(20f, 40f, 200f, flag ? 380 : 256), "UI Toolkit Draw Stats");
		GUI.Label(position, "Elements added\t: " + m_Stats.elementsAdded);
		position.y += num;
		GUI.Label(position, "Elements removed\t: " + m_Stats.elementsRemoved);
		position.y += num;
		GUI.Label(position, "Mesh allocs allocated\t: " + m_Stats.newMeshAllocations);
		position.y += num;
		GUI.Label(position, "Mesh allocs updated\t: " + m_Stats.updatedMeshAllocations);
		position.y += num;
		GUI.Label(position, "Clip update roots\t: " + m_Stats.recursiveClipUpdates);
		position.y += num;
		GUI.Label(position, "Clip update total\t: " + m_Stats.recursiveClipUpdatesExpanded);
		position.y += num;
		GUI.Label(position, "Opacity update roots\t: " + m_Stats.recursiveOpacityUpdates);
		position.y += num;
		GUI.Label(position, "Opacity update total\t: " + m_Stats.recursiveOpacityUpdatesExpanded);
		position.y += num;
		GUI.Label(position, "Opacity ID update\t: " + m_Stats.opacityIdUpdates);
		position.y += num;
		GUI.Label(position, "Xform update roots\t: " + m_Stats.recursiveTransformUpdates);
		position.y += num;
		GUI.Label(position, "Xform update total\t: " + m_Stats.recursiveTransformUpdatesExpanded);
		position.y += num;
		GUI.Label(position, "Xformed by bone\t: " + m_Stats.boneTransformed);
		position.y += num;
		GUI.Label(position, "Xformed by skipping\t: " + m_Stats.skipTransformed);
		position.y += num;
		GUI.Label(position, "Xformed by nudging\t: " + m_Stats.nudgeTransformed);
		position.y += num;
		GUI.Label(position, "Xformed by repaint\t: " + m_Stats.visualUpdateTransformed);
		position.y += num;
		GUI.Label(position, "Visual update roots\t: " + m_Stats.recursiveVisualUpdates);
		position.y += num;
		GUI.Label(position, "Visual update total\t: " + m_Stats.recursiveVisualUpdatesExpanded);
		position.y += num;
		GUI.Label(position, "Visual update flats\t: " + m_Stats.nonRecursiveVisualUpdates);
		position.y += num;
		GUI.Label(position, "Dirty processed\t: " + m_Stats.dirtyProcessed);
		position.y += num;
		GUI.Label(position, "Group-xform updates\t: " + m_Stats.groupTransformElementsChanged);
		position.y += num;
		if (flag)
		{
			position.y += num;
			UIRenderDevice.DrawStatistics drawStatistics = device.GatherDrawStatistics();
			GUI.Label(position, "Frame index\t: " + drawStatistics.currentFrameIndex);
			position.y += num;
			GUI.Label(position, "Command count\t: " + drawStatistics.commandCount);
			position.y += num;
			GUI.Label(position, "Draw commands\t: " + drawStatistics.drawCommandCount);
			position.y += num;
			GUI.Label(position, "Draw ranges\t: " + drawStatistics.drawRangeCount);
			position.y += num;
			GUI.Label(position, "Draw range calls\t: " + drawStatistics.drawRangeCallCount);
			position.y += num;
			GUI.Label(position, "Material sets\t: " + drawStatistics.materialSetCount);
			position.y += num;
			GUI.Label(position, "Stencil changes\t: " + drawStatistics.stencilRefChanges);
			position.y += num;
			GUI.Label(position, "Immediate draws\t: " + drawStatistics.immediateDraws);
			position.y += num;
			GUI.Label(position, "Total triangles\t: " + drawStatistics.totalIndices / 3);
			position.y += num;
		}
	}

	private static VisualElement GetFirstElementInPanel(VisualElement ve)
	{
		while (ve != null)
		{
			VisualElement prev = ve.renderChainData.prev;
			if (prev == null || !prev.renderChainData.isInChain)
			{
				break;
			}
			ve = ve.renderChainData.prev;
		}
		return ve;
	}
}
