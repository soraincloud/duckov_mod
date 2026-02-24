#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;
using UnityEngine.Rendering;

namespace UnityEngine.UIElements.UIR;

internal class UIRenderDevice : IDisposable
{
	internal struct AllocToUpdate
	{
		public uint id;

		public uint allocTime;

		public MeshHandle meshHandle;

		public Alloc permAllocVerts;

		public Alloc permAllocIndices;

		public Page permPage;

		public bool copyBackIndices;
	}

	private struct AllocToFree
	{
		public Alloc alloc;

		public Page page;

		public bool vertices;
	}

	private struct DeviceToFree
	{
		public uint handle;

		public Page page;

		public void Dispose()
		{
			while (this.page != null)
			{
				Page page = this.page;
				this.page = this.page.next;
				page.Dispose();
			}
		}
	}

	private struct EvaluationState
	{
		public MaterialPropertyBlock stateMatProps;

		public Material defaultMat;

		public State curState;

		public Page curPage;

		public bool mustApplyMaterial;

		public bool mustApplyCommonBlock;

		public bool mustApplyStateBlock;

		public bool mustApplyStencil;
	}

	internal struct AllocationStatistics
	{
		public struct PageStatistics
		{
			internal HeapStatistics vertices;

			internal HeapStatistics indices;
		}

		public PageStatistics[] pages;

		public int[] freesDeferred;

		public bool completeInit;
	}

	internal struct DrawStatistics
	{
		public int currentFrameIndex;

		public uint totalIndices;

		public uint commandCount;

		public uint drawCommandCount;

		public uint materialSetCount;

		public uint drawRangeCount;

		public uint drawRangeCallCount;

		public uint immediateDraws;

		public uint stencilRefChanges;
	}

	internal const uint k_MaxQueuedFrameCount = 4u;

	internal const int k_PruneEmptyPageFrameCount = 60;

	private readonly bool m_MockDevice;

	private IntPtr m_DefaultStencilState;

	private IntPtr m_VertexDecl;

	private Page m_FirstPage;

	private uint m_NextPageVertexCount;

	private uint m_LargeMeshVertexCount;

	private float m_IndexToVertexCountRatio;

	private List<List<AllocToFree>> m_DeferredFrees;

	private List<List<AllocToUpdate>> m_Updates;

	private uint[] m_Fences;

	private MaterialPropertyBlock m_StandardMatProps;

	private uint m_FrameIndex;

	private uint m_NextUpdateID = 1u;

	private DrawStatistics m_DrawStats;

	private readonly LinkedPool<MeshHandle> m_MeshHandles = new LinkedPool<MeshHandle>(() => new MeshHandle(), delegate
	{
	});

	private readonly DrawParams m_DrawParams = new DrawParams();

	private readonly TextureSlotManager m_TextureSlotManager = new TextureSlotManager();

	private static LinkedList<DeviceToFree> m_DeviceFreeQueue;

	private static int m_ActiveDeviceCount;

	private static bool m_SubscribedToNotifications;

	private static bool m_SynchronousFree;

	private static readonly int s_GradientSettingsTexID;

	private static readonly int s_ShaderInfoTexID;

	private static readonly int s_TransformsPropID;

	private static readonly int s_ClipRectsPropID;

	private static ProfilerMarker s_MarkerAllocate;

	private static ProfilerMarker s_MarkerFree;

	private static ProfilerMarker s_MarkerAdvanceFrame;

	private static ProfilerMarker s_MarkerFence;

	private static ProfilerMarker s_MarkerBeforeDraw;

	private static bool? s_VertexTexturingIsAvailable;

	private const string k_VertexTexturingIsAvailableTag = "UIE_VertexTexturingIsAvailable";

	private const string k_VertexTexturingIsAvailableTrue = "1";

	private static bool? s_ShaderModelIs35;

	private const string k_ShaderModelIs35Tag = "UIE_ShaderModelIs35";

	private const string k_ShaderModelIs35True = "1";

	private static Texture2D s_DefaultShaderInfoTexFloat;

	private static Texture2D s_DefaultShaderInfoTexARGB8;

	internal uint maxVerticesPerPage { get; } = 65535u;

	internal bool breakBatches { get; set; }

	internal static Texture2D defaultShaderInfoTexFloat
	{
		get
		{
			if (s_DefaultShaderInfoTexFloat == null)
			{
				s_DefaultShaderInfoTexFloat = new Texture2D(64, 64, TextureFormat.RGBAFloat, mipChain: false);
				s_DefaultShaderInfoTexFloat.name = "DefaultShaderInfoTexFloat";
				s_DefaultShaderInfoTexFloat.hideFlags = HideFlags.HideAndDontSave;
				s_DefaultShaderInfoTexFloat.filterMode = FilterMode.Point;
				s_DefaultShaderInfoTexFloat.SetPixel(UIRVEShaderInfoAllocator.identityTransformTexel.x, UIRVEShaderInfoAllocator.identityTransformTexel.y, UIRVEShaderInfoAllocator.identityTransformRow0Value);
				s_DefaultShaderInfoTexFloat.SetPixel(UIRVEShaderInfoAllocator.identityTransformTexel.x, UIRVEShaderInfoAllocator.identityTransformTexel.y + 1, UIRVEShaderInfoAllocator.identityTransformRow1Value);
				s_DefaultShaderInfoTexFloat.SetPixel(UIRVEShaderInfoAllocator.identityTransformTexel.x, UIRVEShaderInfoAllocator.identityTransformTexel.y + 2, UIRVEShaderInfoAllocator.identityTransformRow2Value);
				s_DefaultShaderInfoTexFloat.SetPixel(UIRVEShaderInfoAllocator.infiniteClipRectTexel.x, UIRVEShaderInfoAllocator.infiniteClipRectTexel.y, UIRVEShaderInfoAllocator.infiniteClipRectValue);
				s_DefaultShaderInfoTexFloat.SetPixel(UIRVEShaderInfoAllocator.fullOpacityTexel.x, UIRVEShaderInfoAllocator.fullOpacityTexel.y, UIRVEShaderInfoAllocator.fullOpacityValue);
				s_DefaultShaderInfoTexFloat.SetPixel(UIRVEShaderInfoAllocator.defaultTextCoreSettingsTexel.x, UIRVEShaderInfoAllocator.defaultTextCoreSettingsTexel.y, Color.white);
				s_DefaultShaderInfoTexFloat.SetPixel(UIRVEShaderInfoAllocator.defaultTextCoreSettingsTexel.x, UIRVEShaderInfoAllocator.defaultTextCoreSettingsTexel.y + 1, Color.clear);
				s_DefaultShaderInfoTexFloat.SetPixel(UIRVEShaderInfoAllocator.defaultTextCoreSettingsTexel.x, UIRVEShaderInfoAllocator.defaultTextCoreSettingsTexel.y + 2, Color.clear);
				s_DefaultShaderInfoTexFloat.SetPixel(UIRVEShaderInfoAllocator.defaultTextCoreSettingsTexel.x, UIRVEShaderInfoAllocator.defaultTextCoreSettingsTexel.y + 3, Color.clear);
				s_DefaultShaderInfoTexFloat.Apply(updateMipmaps: false, makeNoLongerReadable: true);
			}
			return s_DefaultShaderInfoTexFloat;
		}
	}

	internal static Texture2D defaultShaderInfoTexARGB8
	{
		get
		{
			if (s_DefaultShaderInfoTexARGB8 == null)
			{
				s_DefaultShaderInfoTexARGB8 = new Texture2D(64, 64, TextureFormat.RGBA32, mipChain: false);
				s_DefaultShaderInfoTexARGB8.name = "DefaultShaderInfoTexARGB8";
				s_DefaultShaderInfoTexARGB8.hideFlags = HideFlags.HideAndDontSave;
				s_DefaultShaderInfoTexARGB8.filterMode = FilterMode.Point;
				s_DefaultShaderInfoTexARGB8.SetPixel(UIRVEShaderInfoAllocator.fullOpacityTexel.x, UIRVEShaderInfoAllocator.fullOpacityTexel.y, UIRVEShaderInfoAllocator.fullOpacityValue);
				s_DefaultShaderInfoTexARGB8.SetPixel(UIRVEShaderInfoAllocator.defaultTextCoreSettingsTexel.x, UIRVEShaderInfoAllocator.defaultTextCoreSettingsTexel.y, Color.white);
				s_DefaultShaderInfoTexARGB8.SetPixel(UIRVEShaderInfoAllocator.defaultTextCoreSettingsTexel.x, UIRVEShaderInfoAllocator.defaultTextCoreSettingsTexel.y + 1, Color.clear);
				s_DefaultShaderInfoTexARGB8.SetPixel(UIRVEShaderInfoAllocator.defaultTextCoreSettingsTexel.x, UIRVEShaderInfoAllocator.defaultTextCoreSettingsTexel.y + 2, Color.clear);
				s_DefaultShaderInfoTexARGB8.SetPixel(UIRVEShaderInfoAllocator.defaultTextCoreSettingsTexel.x, UIRVEShaderInfoAllocator.defaultTextCoreSettingsTexel.y + 3, Color.clear);
				s_DefaultShaderInfoTexARGB8.Apply(updateMipmaps: false, makeNoLongerReadable: true);
			}
			return s_DefaultShaderInfoTexARGB8;
		}
	}

	internal static bool vertexTexturingIsAvailable
	{
		get
		{
			if (!s_VertexTexturingIsAvailable.HasValue)
			{
				Shader shader = Shader.Find(UIRUtility.k_DefaultShaderName);
				Material material = new Material(shader);
				material.hideFlags |= HideFlags.DontSaveInEditor;
				string tag = material.GetTag("UIE_VertexTexturingIsAvailable", searchFallbacks: false);
				UIRUtility.Destroy(material);
				s_VertexTexturingIsAvailable = tag == "1";
			}
			return s_VertexTexturingIsAvailable.Value;
		}
	}

	internal static bool shaderModelIs35
	{
		get
		{
			if (!s_ShaderModelIs35.HasValue)
			{
				Shader shader = Shader.Find(UIRUtility.k_DefaultShaderName);
				Material material = new Material(shader);
				material.hideFlags |= HideFlags.DontSaveInEditor;
				string tag = material.GetTag("UIE_ShaderModelIs35", searchFallbacks: false);
				UIRUtility.Destroy(material);
				s_ShaderModelIs35 = tag == "1";
			}
			return s_ShaderModelIs35.Value;
		}
	}

	private bool fullyCreated => m_Fences != null;

	protected bool disposed { get; private set; }

	static UIRenderDevice()
	{
		m_DeviceFreeQueue = new LinkedList<DeviceToFree>();
		m_ActiveDeviceCount = 0;
		s_GradientSettingsTexID = Shader.PropertyToID("_GradientSettingsTex");
		s_ShaderInfoTexID = Shader.PropertyToID("_ShaderInfoTex");
		s_TransformsPropID = Shader.PropertyToID("_Transforms");
		s_ClipRectsPropID = Shader.PropertyToID("_ClipRects");
		s_MarkerAllocate = new ProfilerMarker("UIR.Allocate");
		s_MarkerFree = new ProfilerMarker("UIR.Free");
		s_MarkerAdvanceFrame = new ProfilerMarker("UIR.AdvanceFrame");
		s_MarkerFence = new ProfilerMarker("UIR.WaitOnFence");
		s_MarkerBeforeDraw = new ProfilerMarker("UIR.BeforeDraw");
		Utility.EngineUpdate += OnEngineUpdateGlobal;
		Utility.FlushPendingResources += OnFlushPendingResources;
	}

	public UIRenderDevice(uint initialVertexCapacity = 0u, uint initialIndexCapacity = 0u)
		: this(initialVertexCapacity, initialIndexCapacity, mockDevice: false)
	{
	}

	protected UIRenderDevice(uint initialVertexCapacity, uint initialIndexCapacity, bool mockDevice)
	{
		m_MockDevice = mockDevice;
		Debug.Assert(!m_SynchronousFree);
		Debug.Assert(condition: true);
		if (m_ActiveDeviceCount++ == 0 && !m_SubscribedToNotifications && !m_MockDevice)
		{
			Utility.NotifyOfUIREvents(subscribe: true);
			m_SubscribedToNotifications = true;
		}
		m_NextPageVertexCount = Math.Max(initialVertexCapacity, 2048u);
		m_LargeMeshVertexCount = m_NextPageVertexCount;
		m_IndexToVertexCountRatio = (float)initialIndexCapacity / (float)initialVertexCapacity;
		m_IndexToVertexCountRatio = Mathf.Max(m_IndexToVertexCountRatio, 2f);
		m_DeferredFrees = new List<List<AllocToFree>>(4);
		m_Updates = new List<List<AllocToUpdate>>(4);
		for (int num = 0; (long)num < 4L; num++)
		{
			m_DeferredFrees.Add(new List<AllocToFree>());
			m_Updates.Add(new List<AllocToUpdate>());
		}
	}

	private void InitVertexDeclaration()
	{
		VertexAttributeDescriptor[] vertexAttributes = new VertexAttributeDescriptor[10]
		{
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
			new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.UNorm8, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.UNorm8, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord3, VertexAttributeFormat.UNorm8, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord4, VertexAttributeFormat.UNorm8, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord5, VertexAttributeFormat.UNorm8, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord6, VertexAttributeFormat.Float32, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord7, VertexAttributeFormat.Float32, 1)
		};
		m_VertexDecl = Utility.GetVertexDeclaration(vertexAttributes);
	}

	private void CompleteCreation()
	{
		if (!m_MockDevice && !fullyCreated)
		{
			InitVertexDeclaration();
			m_Fences = new uint[4];
			m_StandardMatProps = new MaterialPropertyBlock();
			m_DefaultStencilState = Utility.CreateStencilState(new StencilState
			{
				enabled = true,
				readMask = byte.MaxValue,
				writeMask = byte.MaxValue,
				compareFunctionFront = CompareFunction.Equal,
				passOperationFront = StencilOp.Keep,
				failOperationFront = StencilOp.Keep,
				zFailOperationFront = StencilOp.IncrementSaturate,
				compareFunctionBack = CompareFunction.Less,
				passOperationBack = StencilOp.Keep,
				failOperationBack = StencilOp.Keep,
				zFailOperationBack = StencilOp.DecrementSaturate
			});
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	internal void DisposeImmediate()
	{
		Debug.Assert(!m_SynchronousFree);
		m_SynchronousFree = true;
		Dispose();
		m_SynchronousFree = false;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposed)
		{
			return;
		}
		m_ActiveDeviceCount--;
		if (disposing)
		{
			DeviceToFree value = new DeviceToFree
			{
				handle = ((!m_MockDevice) ? Utility.InsertCPUFence() : 0u),
				page = m_FirstPage
			};
			if (value.handle == 0)
			{
				value.Dispose();
			}
			else
			{
				m_DeviceFreeQueue.AddLast(value);
				if (m_SynchronousFree)
				{
					ProcessDeviceFreeQueue();
				}
			}
		}
		disposed = true;
	}

	public MeshHandle Allocate(uint vertexCount, uint indexCount, out NativeSlice<Vertex> vertexData, out NativeSlice<ushort> indexData, out ushort indexOffset)
	{
		MeshHandle meshHandle = m_MeshHandles.Get();
		meshHandle.triangleCount = indexCount / 3;
		Allocate(meshHandle, vertexCount, indexCount, out vertexData, out indexData, shortLived: false);
		indexOffset = (ushort)meshHandle.allocVerts.start;
		return meshHandle;
	}

	public void Update(MeshHandle mesh, uint vertexCount, out NativeSlice<Vertex> vertexData)
	{
		Debug.Assert(mesh.allocVerts.size >= vertexCount);
		if (mesh.allocTime == m_FrameIndex)
		{
			vertexData = mesh.allocPage.vertices.cpuData.Slice((int)mesh.allocVerts.start, (int)vertexCount);
			return;
		}
		uint start = mesh.allocVerts.start;
		NativeSlice<ushort> nativeSlice = new NativeSlice<ushort>(mesh.allocPage.indices.cpuData, (int)mesh.allocIndices.start, (int)mesh.allocIndices.size);
		UpdateAfterGPUUsedData(mesh, vertexCount, mesh.allocIndices.size, out vertexData, out var indexData, out var indexOffset, out var _, copyBackIndices: false);
		int size = (int)mesh.allocIndices.size;
		int num = (int)(indexOffset - start);
		for (int i = 0; i < size; i++)
		{
			indexData[i] = (ushort)(nativeSlice[i] + num);
		}
	}

	public void Update(MeshHandle mesh, uint vertexCount, uint indexCount, out NativeSlice<Vertex> vertexData, out NativeSlice<ushort> indexData, out ushort indexOffset)
	{
		Debug.Assert(mesh.allocVerts.size >= vertexCount);
		Debug.Assert(mesh.allocIndices.size >= indexCount);
		if (mesh.allocTime == m_FrameIndex)
		{
			vertexData = mesh.allocPage.vertices.cpuData.Slice((int)mesh.allocVerts.start, (int)vertexCount);
			indexData = mesh.allocPage.indices.cpuData.Slice((int)mesh.allocIndices.start, (int)indexCount);
			indexOffset = (ushort)mesh.allocVerts.start;
			UpdateCopyBackIndices(mesh, copyBackIndices: true);
		}
		else
		{
			UpdateAfterGPUUsedData(mesh, vertexCount, indexCount, out vertexData, out indexData, out indexOffset, out var _, copyBackIndices: true);
		}
	}

	private void UpdateCopyBackIndices(MeshHandle mesh, bool copyBackIndices)
	{
		if (mesh.updateAllocID != 0)
		{
			int index = (int)(mesh.updateAllocID - 1);
			List<AllocToUpdate> list = ActiveUpdatesForMeshHandle(mesh);
			AllocToUpdate value = list[index];
			value.copyBackIndices = true;
			list[index] = value;
		}
	}

	internal List<AllocToUpdate> ActiveUpdatesForMeshHandle(MeshHandle mesh)
	{
		return m_Updates[(int)mesh.allocTime % m_Updates.Count];
	}

	private bool TryAllocFromPage(Page page, uint vertexCount, uint indexCount, ref Alloc va, ref Alloc ia, bool shortLived)
	{
		va = page.vertices.allocator.Allocate(vertexCount, shortLived);
		if (va.size != 0)
		{
			ia = page.indices.allocator.Allocate(indexCount, shortLived);
			if (ia.size != 0)
			{
				return true;
			}
			page.vertices.allocator.Free(va);
			va.size = 0u;
		}
		return false;
	}

	private void Allocate(MeshHandle meshHandle, uint vertexCount, uint indexCount, out NativeSlice<Vertex> vertexData, out NativeSlice<ushort> indexData, bool shortLived)
	{
		Page page = null;
		Alloc va = default(Alloc);
		Alloc ia = default(Alloc);
		if (vertexCount <= m_LargeMeshVertexCount)
		{
			if (m_FirstPage != null)
			{
				page = m_FirstPage;
				while (!TryAllocFromPage(page, vertexCount, indexCount, ref va, ref ia, shortLived) && page.next != null)
				{
					page = page.next;
				}
			}
			else
			{
				CompleteCreation();
			}
			if (ia.size == 0)
			{
				m_NextPageVertexCount <<= 1;
				m_NextPageVertexCount = Math.Max(m_NextPageVertexCount, vertexCount * 2);
				m_NextPageVertexCount = Math.Min(m_NextPageVertexCount, maxVerticesPerPage);
				uint val = (uint)((float)m_NextPageVertexCount * m_IndexToVertexCountRatio + 0.5f);
				val = Math.Max(val, indexCount * 2);
				Debug.Assert(page?.next == null);
				page = new Page(m_NextPageVertexCount, val, 4u, m_MockDevice);
				page.next = m_FirstPage;
				m_FirstPage = page;
				va = page.vertices.allocator.Allocate(vertexCount, shortLived);
				ia = page.indices.allocator.Allocate(indexCount, shortLived);
				Debug.Assert(va.size != 0);
				Debug.Assert(ia.size != 0);
			}
		}
		else
		{
			CompleteCreation();
			Page page2 = m_FirstPage;
			Page page3 = m_FirstPage;
			int num = int.MaxValue;
			while (page2 != null)
			{
				int num2 = page2.vertices.cpuData.Length - (int)vertexCount;
				int num3 = page2.indices.cpuData.Length - (int)indexCount;
				if (page2.isEmpty && num2 >= 0 && num3 >= 0 && num2 < num)
				{
					page = page2;
					num = num2;
				}
				page3 = page2;
				page2 = page2.next;
			}
			if (page == null)
			{
				uint vertexMaxCount = ((vertexCount > maxVerticesPerPage) ? 2u : vertexCount);
				Debug.Assert(vertexCount <= maxVerticesPerPage, "Requested Vertex count is above the limit. Alloc will fail.");
				page = new Page(vertexMaxCount, indexCount, 4u, m_MockDevice);
				if (page3 != null)
				{
					page3.next = page;
				}
				else
				{
					m_FirstPage = page;
				}
			}
			va = page.vertices.allocator.Allocate(vertexCount, shortLived);
			ia = page.indices.allocator.Allocate(indexCount, shortLived);
		}
		Debug.Assert(va.size == vertexCount, "Vertices allocated != Vertices requested");
		Debug.Assert(ia.size == indexCount, "Indices allocated != Indices requested");
		if (va.size != vertexCount || ia.size != indexCount)
		{
			if (va.handle != null)
			{
				page.vertices.allocator.Free(va);
			}
			if (ia.handle != null)
			{
				page.vertices.allocator.Free(ia);
			}
			ia = default(Alloc);
			va = default(Alloc);
		}
		page.vertices.RegisterUpdate(va.start, va.size);
		page.indices.RegisterUpdate(ia.start, ia.size);
		vertexData = new NativeSlice<Vertex>(page.vertices.cpuData, (int)va.start, (int)va.size);
		indexData = new NativeSlice<ushort>(page.indices.cpuData, (int)ia.start, (int)ia.size);
		meshHandle.allocPage = page;
		meshHandle.allocVerts = va;
		meshHandle.allocIndices = ia;
		meshHandle.allocTime = m_FrameIndex;
	}

	private void UpdateAfterGPUUsedData(MeshHandle mesh, uint vertexCount, uint indexCount, out NativeSlice<Vertex> vertexData, out NativeSlice<ushort> indexData, out ushort indexOffset, out AllocToUpdate allocToUpdate, bool copyBackIndices)
	{
		allocToUpdate = new AllocToUpdate
		{
			id = m_NextUpdateID++,
			allocTime = m_FrameIndex,
			meshHandle = mesh,
			copyBackIndices = copyBackIndices
		};
		Debug.Assert(m_NextUpdateID != 0);
		if (mesh.updateAllocID == 0)
		{
			allocToUpdate.permAllocVerts = mesh.allocVerts;
			allocToUpdate.permAllocIndices = mesh.allocIndices;
			allocToUpdate.permPage = mesh.allocPage;
		}
		else
		{
			int index = (int)(mesh.updateAllocID - 1);
			List<AllocToUpdate> list = m_Updates[(int)mesh.allocTime % m_Updates.Count];
			AllocToUpdate value = list[index];
			Debug.Assert(value.id == mesh.updateAllocID);
			allocToUpdate.copyBackIndices |= value.copyBackIndices;
			allocToUpdate.permAllocVerts = value.permAllocVerts;
			allocToUpdate.permAllocIndices = value.permAllocIndices;
			allocToUpdate.permPage = value.permPage;
			value.allocTime = uint.MaxValue;
			list[index] = value;
			List<AllocToFree> list2 = m_DeferredFrees[(int)(m_FrameIndex % (uint)m_DeferredFrees.Count)];
			list2.Add(new AllocToFree
			{
				alloc = mesh.allocVerts,
				page = mesh.allocPage,
				vertices = true
			});
			list2.Add(new AllocToFree
			{
				alloc = mesh.allocIndices,
				page = mesh.allocPage,
				vertices = false
			});
		}
		if (TryAllocFromPage(mesh.allocPage, vertexCount, indexCount, ref mesh.allocVerts, ref mesh.allocIndices, shortLived: true))
		{
			mesh.allocPage.vertices.RegisterUpdate(mesh.allocVerts.start, mesh.allocVerts.size);
			mesh.allocPage.indices.RegisterUpdate(mesh.allocIndices.start, mesh.allocIndices.size);
		}
		else
		{
			Allocate(mesh, vertexCount, indexCount, out vertexData, out indexData, shortLived: true);
		}
		mesh.triangleCount = indexCount / 3;
		mesh.updateAllocID = allocToUpdate.id;
		mesh.allocTime = allocToUpdate.allocTime;
		m_Updates[(int)(m_FrameIndex % m_Updates.Count)].Add(allocToUpdate);
		vertexData = new NativeSlice<Vertex>(mesh.allocPage.vertices.cpuData, (int)mesh.allocVerts.start, (int)vertexCount);
		indexData = new NativeSlice<ushort>(mesh.allocPage.indices.cpuData, (int)mesh.allocIndices.start, (int)indexCount);
		indexOffset = (ushort)mesh.allocVerts.start;
	}

	public void Free(MeshHandle mesh)
	{
		if (mesh.updateAllocID != 0)
		{
			int index = (int)(mesh.updateAllocID - 1);
			List<AllocToUpdate> list = m_Updates[(int)mesh.allocTime % m_Updates.Count];
			AllocToUpdate value = list[index];
			Debug.Assert(value.id == mesh.updateAllocID);
			List<AllocToFree> list2 = m_DeferredFrees[(int)(m_FrameIndex % (uint)m_DeferredFrees.Count)];
			list2.Add(new AllocToFree
			{
				alloc = value.permAllocVerts,
				page = value.permPage,
				vertices = true
			});
			list2.Add(new AllocToFree
			{
				alloc = value.permAllocIndices,
				page = value.permPage,
				vertices = false
			});
			list2.Add(new AllocToFree
			{
				alloc = mesh.allocVerts,
				page = mesh.allocPage,
				vertices = true
			});
			list2.Add(new AllocToFree
			{
				alloc = mesh.allocIndices,
				page = mesh.allocPage,
				vertices = false
			});
			value.allocTime = uint.MaxValue;
			list[index] = value;
		}
		else if (mesh.allocTime != m_FrameIndex)
		{
			int index2 = (int)(m_FrameIndex % (uint)m_DeferredFrees.Count);
			m_DeferredFrees[index2].Add(new AllocToFree
			{
				alloc = mesh.allocVerts,
				page = mesh.allocPage,
				vertices = true
			});
			m_DeferredFrees[index2].Add(new AllocToFree
			{
				alloc = mesh.allocIndices,
				page = mesh.allocPage,
				vertices = false
			});
		}
		else
		{
			mesh.allocPage.vertices.allocator.Free(mesh.allocVerts);
			mesh.allocPage.indices.allocator.Free(mesh.allocIndices);
		}
		mesh.allocVerts = default(Alloc);
		mesh.allocIndices = default(Alloc);
		mesh.allocPage = null;
		mesh.updateAllocID = 0u;
		m_MeshHandles.Return(mesh);
	}

	public void OnFrameRenderingBegin()
	{
		AdvanceFrame();
		m_DrawStats = default(DrawStatistics);
		m_DrawStats.currentFrameIndex = (int)m_FrameIndex;
		for (Page page = m_FirstPage; page != null; page = page.next)
		{
			page.vertices.SendUpdates();
			page.indices.SendUpdates();
		}
	}

	internal unsafe static NativeSlice<T> PtrToSlice<T>(void* p, int count) where T : struct
	{
		return NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<T>(p, UnsafeUtility.SizeOf<T>(), count);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ApplyDrawCommandState(RenderChainCommand cmd, int textureSlot, Material newMat, bool newMatDiffers, ref EvaluationState st)
	{
		if (newMatDiffers)
		{
			st.curState.material = newMat;
			st.mustApplyMaterial = true;
		}
		st.curPage = cmd.mesh.allocPage;
		if (cmd.state.texture != TextureId.invalid)
		{
			if (textureSlot < 0)
			{
				textureSlot = m_TextureSlotManager.FindOldestSlot();
				m_TextureSlotManager.Bind(cmd.state.texture, cmd.state.sdfScale, textureSlot, st.stateMatProps);
				st.mustApplyStateBlock = true;
			}
			else
			{
				m_TextureSlotManager.MarkUsed(textureSlot);
			}
		}
		if (cmd.state.stencilRef != st.curState.stencilRef)
		{
			st.curState.stencilRef = cmd.state.stencilRef;
			st.mustApplyStencil = true;
		}
	}

	private void ApplyBatchState(ref EvaluationState st, bool allowMaterialChange)
	{
		if (!m_MockDevice)
		{
			if (st.mustApplyMaterial)
			{
				if (!allowMaterialChange)
				{
					Debug.LogError("Attempted to change material when it is not allowed to do so.");
					return;
				}
				m_DrawStats.materialSetCount++;
				st.curState.material.SetPass(0);
				if (m_StandardMatProps != null)
				{
					Utility.SetPropertyBlock(m_StandardMatProps);
				}
				st.mustApplyCommonBlock = true;
				st.mustApplyStateBlock = true;
				st.mustApplyStencil = true;
			}
			if (st.mustApplyStateBlock)
			{
				Utility.SetPropertyBlock(st.stateMatProps);
			}
			if (st.mustApplyStencil)
			{
				m_DrawStats.stencilRefChanges++;
				Utility.SetStencilState(m_DefaultStencilState, st.curState.stencilRef);
			}
		}
		st.mustApplyMaterial = false;
		st.mustApplyCommonBlock = false;
		st.mustApplyStateBlock = false;
		st.mustApplyStencil = false;
		m_TextureSlotManager.StartNewBatch();
	}

	public unsafe void EvaluateChain(RenderChainCommand head, Material initialMat, Material defaultMat, Texture gradientSettings, Texture shaderInfo, float pixelsPerPoint, NativeSlice<Transform3x4> transforms, NativeSlice<Vector4> clipRects, MaterialPropertyBlock stateMatProps, bool allowMaterialChange, ref Exception immediateException)
	{
		Utility.ProfileDrawChainBegin();
		bool flag = breakBatches;
		DrawParams drawParams = m_DrawParams;
		drawParams.Reset();
		drawParams.renderTexture.Add(RenderTexture.active);
		stateMatProps.Clear();
		m_TextureSlotManager.Reset();
		if (fullyCreated)
		{
			if (gradientSettings != null)
			{
				m_StandardMatProps.SetTexture(s_GradientSettingsTexID, gradientSettings);
			}
			if (shaderInfo != null)
			{
				m_StandardMatProps.SetTexture(s_ShaderInfoTexID, shaderInfo);
			}
			if (transforms.Length > 0)
			{
				Utility.SetVectorArray(m_StandardMatProps, s_TransformsPropID, transforms);
			}
			if (clipRects.Length > 0)
			{
				Utility.SetVectorArray(m_StandardMatProps, s_ClipRectsPropID, clipRects);
			}
			Utility.SetPropertyBlock(m_StandardMatProps);
		}
		int num = 1024;
		DrawBufferRange* ptr = stackalloc DrawBufferRange[num];
		int num2 = num - 1;
		int rangesStart = 0;
		int rangesReady = 0;
		DrawBufferRange drawBufferRange = default(DrawBufferRange);
		int num3 = -1;
		EvaluationState st = new EvaluationState
		{
			stateMatProps = stateMatProps,
			defaultMat = defaultMat,
			curState = new State
			{
				material = initialMat
			},
			mustApplyCommonBlock = true,
			mustApplyStateBlock = true,
			mustApplyStencil = true
		};
		while (head != null)
		{
			m_DrawStats.commandCount++;
			m_DrawStats.drawCommandCount += ((head.type == CommandType.Draw) ? 1u : 0u);
			bool flag2 = drawBufferRange.indexCount > 0 && rangesReady == num - 1;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			int num4 = -1;
			Material material = null;
			bool newMatDiffers = false;
			if (head.type == CommandType.Draw)
			{
				material = ((head.state.material != null) ? head.state.material : defaultMat);
				if (material != st.curState.material)
				{
					flag5 = true;
					newMatDiffers = true;
					flag3 = true;
					flag4 = true;
				}
				if (head.mesh.allocPage != st.curPage)
				{
					flag5 = true;
					flag3 = true;
					flag4 = true;
				}
				else if (num3 != head.mesh.allocIndices.start + head.indexOffset)
				{
					flag3 = true;
				}
				if (head.state.texture != TextureId.invalid)
				{
					flag5 = true;
					num4 = m_TextureSlotManager.IndexOf(head.state.texture);
					if (num4 < 0 && m_TextureSlotManager.FreeSlots < 1)
					{
						flag3 = true;
						flag4 = true;
					}
				}
				if (head.state.stencilRef != st.curState.stencilRef)
				{
					flag5 = true;
					flag3 = true;
					flag4 = true;
				}
				if (flag3 && flag2)
				{
					flag4 = true;
				}
			}
			else
			{
				flag3 = true;
				flag4 = true;
			}
			if (flag)
			{
				flag3 = true;
				flag4 = true;
			}
			if (flag3)
			{
				if (drawBufferRange.indexCount > 0)
				{
					int num5 = (rangesStart + rangesReady++) & num2;
					ptr[num5] = drawBufferRange;
					Debug.Assert(rangesReady < num || flag4);
					drawBufferRange = default(DrawBufferRange);
					m_DrawStats.drawRangeCount++;
				}
				if (head.type == CommandType.Draw)
				{
					drawBufferRange.firstIndex = (int)head.mesh.allocIndices.start + head.indexOffset;
					drawBufferRange.indexCount = head.indexCount;
					drawBufferRange.vertsReferenced = (int)head.mesh.allocVerts.size;
					drawBufferRange.minIndexVal = (int)head.mesh.allocVerts.start;
					num3 = drawBufferRange.firstIndex + head.indexCount;
					m_DrawStats.totalIndices += (uint)head.indexCount;
				}
				if (flag4)
				{
					if (rangesReady > 0)
					{
						ApplyBatchState(ref st, allowMaterialChange);
						KickRanges(ptr, ref rangesReady, ref rangesStart, num, st.curPage);
					}
					if (head.type != CommandType.Draw)
					{
						if (!m_MockDevice)
						{
							head.ExecuteNonDrawMesh(drawParams, pixelsPerPoint, ref immediateException);
						}
						if (head.type == CommandType.Immediate || head.type == CommandType.ImmediateCull || head.type == CommandType.BlitToPreviousRT || head.type == CommandType.PushRenderTexture || head.type == CommandType.PopDefaultMaterial || head.type == CommandType.PushDefaultMaterial)
						{
							st.curState.material = null;
							st.mustApplyMaterial = false;
							m_DrawStats.immediateDraws++;
							if (head.type == CommandType.PopDefaultMaterial)
							{
								int index = drawParams.defaultMaterial.Count - 1;
								defaultMat = drawParams.defaultMaterial[index];
								drawParams.defaultMaterial.RemoveAt(index);
							}
							if (head.type == CommandType.PushDefaultMaterial)
							{
								drawParams.defaultMaterial.Add(defaultMat);
								defaultMat = head.state.material;
							}
						}
					}
				}
				if (head.type == CommandType.Draw && flag5)
				{
					ApplyDrawCommandState(head, num4, material, newMatDiffers, ref st);
				}
				head = head.next;
			}
			else
			{
				if (drawBufferRange.indexCount == 0)
				{
					num3 = (drawBufferRange.firstIndex = (int)head.mesh.allocIndices.start + head.indexOffset);
				}
				drawBufferRange.indexCount += head.indexCount;
				int minIndexVal = drawBufferRange.minIndexVal;
				int start = (int)head.mesh.allocVerts.start;
				int a = drawBufferRange.minIndexVal + drawBufferRange.vertsReferenced;
				int b = (int)(head.mesh.allocVerts.start + head.mesh.allocVerts.size);
				drawBufferRange.minIndexVal = Mathf.Min(minIndexVal, start);
				drawBufferRange.vertsReferenced = Mathf.Max(a, b) - drawBufferRange.minIndexVal;
				num3 += head.indexCount;
				m_DrawStats.totalIndices += (uint)head.indexCount;
				if (flag5)
				{
					ApplyDrawCommandState(head, num4, material, newMatDiffers, ref st);
				}
				head = head.next;
			}
		}
		if (drawBufferRange.indexCount > 0)
		{
			int num6 = (rangesStart + rangesReady++) & num2;
			ptr[num6] = drawBufferRange;
		}
		if (rangesReady > 0)
		{
			ApplyBatchState(ref st, allowMaterialChange);
			KickRanges(ptr, ref rangesReady, ref rangesStart, num, st.curPage);
		}
		UpdateFenceValue();
		Utility.ProfileDrawChainEnd();
	}

	private unsafe void UpdateFenceValue()
	{
		if (m_Fences == null)
		{
			return;
		}
		uint num = Utility.InsertCPUFence();
		fixed (uint* ptr = &m_Fences[(int)(m_FrameIndex % m_Fences.Length)])
		{
			uint num2;
			int num3;
			do
			{
				num2 = *ptr;
				if ((int)(num - num2) <= 0)
				{
					break;
				}
				num3 = Interlocked.CompareExchange(ref *(int*)ptr, (int)num, (int)num2);
			}
			while (num3 != num2);
		}
	}

	private unsafe void KickRanges(DrawBufferRange* ranges, ref int rangesReady, ref int rangesStart, int rangesCount, Page curPage)
	{
		Debug.Assert(rangesReady > 0);
		if (rangesStart + rangesReady <= rangesCount)
		{
			if (!m_MockDevice)
			{
				DrawRanges(curPage.indices.gpuData, curPage.vertices.gpuData, PtrToSlice<DrawBufferRange>(ranges + rangesStart, rangesReady));
			}
			m_DrawStats.drawRangeCallCount++;
		}
		else
		{
			int num = rangesCount - rangesStart;
			int count = rangesReady - num;
			if (!m_MockDevice)
			{
				DrawRanges(curPage.indices.gpuData, curPage.vertices.gpuData, PtrToSlice<DrawBufferRange>(ranges + rangesStart, num));
				DrawRanges(curPage.indices.gpuData, curPage.vertices.gpuData, PtrToSlice<DrawBufferRange>(ranges, count));
			}
			m_DrawStats.drawRangeCallCount += 2u;
		}
		rangesStart = (rangesStart + rangesReady) & (rangesCount - 1);
		rangesReady = 0;
	}

	private unsafe void DrawRanges<I, T>(Utility.GPUBuffer<I> ib, Utility.GPUBuffer<T> vb, NativeSlice<DrawBufferRange> ranges) where I : struct where T : struct
	{
		IntPtr* ptr = stackalloc IntPtr[1];
		*ptr = vb.BufferPointer;
		Utility.DrawRanges(ib.BufferPointer, ptr, 1, new IntPtr(ranges.GetUnsafePtr()), ranges.Length, m_VertexDecl);
	}

	internal void WaitOnAllCpuFences()
	{
		for (int i = 0; i < m_Fences.Length; i++)
		{
			WaitOnCpuFence(m_Fences[i]);
		}
	}

	private void WaitOnCpuFence(uint fence)
	{
		if (fence != 0 && !Utility.CPUFencePassed(fence))
		{
			Utility.WaitForCPUFencePassed(fence);
		}
	}

	public void AdvanceFrame()
	{
		m_FrameIndex++;
		m_DrawStats.currentFrameIndex = (int)m_FrameIndex;
		if (m_Fences != null)
		{
			int num = (int)(m_FrameIndex % m_Fences.Length);
			uint fence = m_Fences[num];
			WaitOnCpuFence(fence);
			m_Fences[num] = 0u;
		}
		m_NextUpdateID = 1u;
		List<AllocToFree> list = m_DeferredFrees[(int)(m_FrameIndex % (uint)m_DeferredFrees.Count)];
		foreach (AllocToFree item in list)
		{
			if (item.vertices)
			{
				item.page.vertices.allocator.Free(item.alloc);
			}
			else
			{
				item.page.indices.allocator.Free(item.alloc);
			}
		}
		list.Clear();
		List<AllocToUpdate> list2 = m_Updates[(int)(m_FrameIndex % (uint)m_DeferredFrees.Count)];
		foreach (AllocToUpdate item2 in list2)
		{
			if (item2.meshHandle.updateAllocID != item2.id || item2.meshHandle.allocTime != item2.allocTime)
			{
				continue;
			}
			NativeSlice<Vertex> slice = new NativeSlice<Vertex>(item2.meshHandle.allocPage.vertices.cpuData, (int)item2.meshHandle.allocVerts.start, (int)item2.meshHandle.allocVerts.size);
			new NativeSlice<Vertex>(item2.permPage.vertices.cpuData, (int)item2.permAllocVerts.start, (int)item2.meshHandle.allocVerts.size).CopyFrom(slice);
			item2.permPage.vertices.RegisterUpdate(item2.permAllocVerts.start, item2.meshHandle.allocVerts.size);
			if (item2.copyBackIndices)
			{
				NativeSlice<ushort> nativeSlice = new NativeSlice<ushort>(item2.meshHandle.allocPage.indices.cpuData, (int)item2.meshHandle.allocIndices.start, (int)item2.meshHandle.allocIndices.size);
				NativeSlice<ushort> nativeSlice2 = new NativeSlice<ushort>(item2.permPage.indices.cpuData, (int)item2.permAllocIndices.start, (int)item2.meshHandle.allocIndices.size);
				int length = nativeSlice2.Length;
				int num2 = (int)(item2.permAllocVerts.start - item2.meshHandle.allocVerts.start);
				for (int i = 0; i < length; i++)
				{
					nativeSlice2[i] = (ushort)(nativeSlice[i] + num2);
				}
				item2.permPage.indices.RegisterUpdate(item2.permAllocIndices.start, item2.meshHandle.allocIndices.size);
			}
			list.Add(new AllocToFree
			{
				alloc = item2.meshHandle.allocVerts,
				page = item2.meshHandle.allocPage,
				vertices = true
			});
			list.Add(new AllocToFree
			{
				alloc = item2.meshHandle.allocIndices,
				page = item2.meshHandle.allocPage,
				vertices = false
			});
			item2.meshHandle.allocVerts = item2.permAllocVerts;
			item2.meshHandle.allocIndices = item2.permAllocIndices;
			item2.meshHandle.allocPage = item2.permPage;
			item2.meshHandle.updateAllocID = 0u;
		}
		list2.Clear();
		PruneUnusedPages();
	}

	private void PruneUnusedPages()
	{
		Page page2;
		Page page3;
		Page page4;
		Page page = (page2 = (page3 = (page4 = null)));
		Page page5 = m_FirstPage;
		while (page5 != null)
		{
			if (!page5.isEmpty)
			{
				page5.framesEmpty = 0;
			}
			else
			{
				page5.framesEmpty++;
			}
			if (page5.framesEmpty < 60)
			{
				if (page != null)
				{
					page2.next = page5;
				}
				else
				{
					page = page5;
				}
				page2 = page5;
			}
			else
			{
				if (page3 != null)
				{
					page4.next = page5;
				}
				else
				{
					page3 = page5;
				}
				page4 = page5;
			}
			Page next = page5.next;
			page5.next = null;
			page5 = next;
		}
		m_FirstPage = page;
		page5 = page3;
		while (page5 != null)
		{
			Page next2 = page5.next;
			page5.next = null;
			page5.Dispose();
			page5 = next2;
		}
	}

	internal static void PrepareForGfxDeviceRecreate()
	{
		m_ActiveDeviceCount++;
		if (s_DefaultShaderInfoTexFloat != null)
		{
			UIRUtility.Destroy(s_DefaultShaderInfoTexFloat);
			s_DefaultShaderInfoTexFloat = null;
		}
		if (s_DefaultShaderInfoTexARGB8 != null)
		{
			UIRUtility.Destroy(s_DefaultShaderInfoTexARGB8);
			s_DefaultShaderInfoTexARGB8 = null;
		}
	}

	internal static void WrapUpGfxDeviceRecreate()
	{
		m_ActiveDeviceCount--;
	}

	internal static void FlushAllPendingDeviceDisposes()
	{
		Utility.SyncRenderThread();
		ProcessDeviceFreeQueue();
	}

	internal AllocationStatistics GatherAllocationStatistics()
	{
		AllocationStatistics result = new AllocationStatistics
		{
			completeInit = fullyCreated,
			freesDeferred = new int[m_DeferredFrees.Count]
		};
		for (int i = 0; i < m_DeferredFrees.Count; i++)
		{
			result.freesDeferred[i] = m_DeferredFrees[i].Count;
		}
		int num = 0;
		for (Page page = m_FirstPage; page != null; page = page.next)
		{
			num++;
		}
		result.pages = new AllocationStatistics.PageStatistics[num];
		num = 0;
		for (Page page = m_FirstPage; page != null; page = page.next)
		{
			result.pages[num].vertices = page.vertices.allocator.GatherStatistics();
			result.pages[num].indices = page.indices.allocator.GatherStatistics();
			num++;
		}
		return result;
	}

	internal DrawStatistics GatherDrawStatistics()
	{
		return m_DrawStats;
	}

	public static void ProcessDeviceFreeQueue()
	{
		if (m_SynchronousFree)
		{
			Utility.SyncRenderThread();
		}
		LinkedListNode<DeviceToFree> first = m_DeviceFreeQueue.First;
		while (first != null && Utility.CPUFencePassed(first.Value.handle))
		{
			first.Value.Dispose();
			m_DeviceFreeQueue.RemoveFirst();
			first = m_DeviceFreeQueue.First;
		}
		Debug.Assert(!m_SynchronousFree || m_DeviceFreeQueue.Count == 0);
		if (m_ActiveDeviceCount == 0 && m_SubscribedToNotifications)
		{
			if (s_DefaultShaderInfoTexFloat != null)
			{
				UIRUtility.Destroy(s_DefaultShaderInfoTexFloat);
				s_DefaultShaderInfoTexFloat = null;
			}
			if (s_DefaultShaderInfoTexARGB8 != null)
			{
				UIRUtility.Destroy(s_DefaultShaderInfoTexARGB8);
				s_DefaultShaderInfoTexARGB8 = null;
			}
			Utility.NotifyOfUIREvents(subscribe: false);
			m_SubscribedToNotifications = false;
		}
	}

	private static void OnEngineUpdateGlobal()
	{
		ProcessDeviceFreeQueue();
	}

	private static void OnFlushPendingResources()
	{
		m_SynchronousFree = true;
		ProcessDeviceFreeQueue();
	}
}
