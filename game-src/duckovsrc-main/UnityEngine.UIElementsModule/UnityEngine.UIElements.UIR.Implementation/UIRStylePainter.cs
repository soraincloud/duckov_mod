#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.TextCore.Text;

namespace UnityEngine.UIElements.UIR.Implementation;

internal class UIRStylePainter : IStylePainter
{
	internal struct Entry
	{
		public NativeSlice<Vertex> vertices;

		public NativeSlice<ushort> indices;

		public Material material;

		public float fontTexSDFScale;

		public TextureId texture;

		public RenderChainCommand customCommand;

		public BMPAlloc clipRectID;

		public VertexFlags addFlags;

		public bool uvIsDisplacement;

		public bool isTextEntry;

		public bool isClipRegisterEntry;

		public int stencilRef;

		public int maskDepth;
	}

	internal struct ClosingInfo
	{
		public bool needsClosing;

		public bool popViewMatrix;

		public bool popScissorClip;

		public bool blitAndPopRenderTexture;

		public bool PopDefaultMaterial;

		public RenderChainCommand clipUnregisterDrawCommand;

		public NativeSlice<Vertex> clipperRegisterVertices;

		public NativeSlice<ushort> clipperRegisterIndices;

		public int clipperRegisterIndexOffset;

		public int maskStencilRef;
	}

	private struct RepeatRectUV
	{
		public Rect rect;

		public Rect uv;
	}

	private RenderChain m_Owner;

	private List<Entry> m_Entries = new List<Entry>();

	private AtlasBase m_Atlas;

	private VectorImageManager m_VectorImageManager;

	private Entry m_CurrentEntry;

	private ClosingInfo m_ClosingInfo;

	private int m_MaskDepth;

	private int m_StencilRef;

	private BMPAlloc m_ClipRectID = UIRVEShaderInfoAllocator.infiniteClipRect;

	private int m_SVGBackgroundEntryIndex = -1;

	private TempAllocator<Vertex> m_VertsPool;

	private TempAllocator<ushort> m_IndicesPool;

	private List<MeshWriteData> m_MeshWriteDataPool;

	private int m_NextMeshWriteDataPoolItem;

	private List<RepeatRectUV>[] m_RepeatRectUVList = null;

	private MeshBuilder.AllocMeshData.Allocator m_AllocRawVertsIndicesDelegate;

	private MeshBuilder.AllocMeshData.Allocator m_AllocThroughDrawMeshDelegate;

	private TextInfo m_TextInfo = new TextInfo();

	public MeshGenerationContext meshGenerationContext { get; }

	public VisualElement currentElement { get; private set; }

	public List<Entry> entries => m_Entries;

	public ClosingInfo closingInfo => m_ClosingInfo;

	public int totalVertices { get; private set; }

	public int totalIndices { get; private set; }

	public VisualElement visualElement => currentElement;

	private MeshWriteData GetPooledMeshWriteData()
	{
		if (m_NextMeshWriteDataPoolItem == m_MeshWriteDataPool.Count)
		{
			m_MeshWriteDataPool.Add(new MeshWriteData());
		}
		return m_MeshWriteDataPool[m_NextMeshWriteDataPoolItem++];
	}

	private MeshWriteData AllocRawVertsIndices(uint vertexCount, uint indexCount, ref MeshBuilder.AllocMeshData allocatorData)
	{
		m_CurrentEntry.vertices = m_VertsPool.Alloc((int)vertexCount);
		m_CurrentEntry.indices = m_IndicesPool.Alloc((int)indexCount);
		MeshWriteData pooledMeshWriteData = GetPooledMeshWriteData();
		pooledMeshWriteData.Reset(m_CurrentEntry.vertices, m_CurrentEntry.indices);
		return pooledMeshWriteData;
	}

	private MeshWriteData AllocThroughDrawMesh(uint vertexCount, uint indexCount, ref MeshBuilder.AllocMeshData allocatorData)
	{
		return DrawMesh((int)vertexCount, (int)indexCount, allocatorData.texture, allocatorData.material, allocatorData.flags);
	}

	public UIRStylePainter(RenderChain renderChain)
	{
		m_Owner = renderChain;
		meshGenerationContext = new MeshGenerationContext(this);
		m_Atlas = renderChain.atlas;
		m_VectorImageManager = renderChain.vectorImageManager;
		m_AllocRawVertsIndicesDelegate = AllocRawVertsIndices;
		m_AllocThroughDrawMeshDelegate = AllocThroughDrawMesh;
		int num = 32;
		m_MeshWriteDataPool = new List<MeshWriteData>(num);
		for (int i = 0; i < num; i++)
		{
			m_MeshWriteDataPool.Add(new MeshWriteData());
		}
		m_VertsPool = renderChain.vertsPool;
		m_IndicesPool = renderChain.indicesPool;
	}

	public void Begin(VisualElement ve)
	{
		currentElement = ve;
		m_NextMeshWriteDataPoolItem = 0;
		m_SVGBackgroundEntryIndex = -1;
		currentElement.renderChainData.displacementUVStart = (currentElement.renderChainData.displacementUVEnd = 0);
		m_MaskDepth = 0;
		m_StencilRef = 0;
		VisualElement parent = currentElement.hierarchy.parent;
		if (parent != null)
		{
			m_MaskDepth = parent.renderChainData.childrenMaskDepth;
			m_StencilRef = parent.renderChainData.childrenStencilRef;
		}
		bool flag = (currentElement.renderHints & RenderHints.GroupTransform) != 0;
		if (flag)
		{
			RenderChainCommand renderChainCommand = m_Owner.AllocCommand();
			renderChainCommand.owner = currentElement;
			renderChainCommand.type = CommandType.PushView;
			m_Entries.Add(new Entry
			{
				customCommand = renderChainCommand
			});
			m_ClosingInfo.needsClosing = (m_ClosingInfo.popViewMatrix = true);
		}
		if (parent != null)
		{
			m_ClipRectID = (flag ? UIRVEShaderInfoAllocator.infiniteClipRect : parent.renderChainData.clipRectID);
		}
		else
		{
			m_ClipRectID = UIRVEShaderInfoAllocator.infiniteClipRect;
		}
		if (ve.subRenderTargetMode != VisualElement.RenderTargetMode.None)
		{
			RenderChainCommand renderChainCommand2 = m_Owner.AllocCommand();
			renderChainCommand2.owner = currentElement;
			renderChainCommand2.type = CommandType.PushRenderTexture;
			m_Entries.Add(new Entry
			{
				customCommand = renderChainCommand2
			});
			m_ClosingInfo.needsClosing = (m_ClosingInfo.blitAndPopRenderTexture = true);
			if (m_MaskDepth > 0 || m_StencilRef > 0)
			{
				Debug.LogError("The RenderTargetMode feature must not be used within a stencil mask.");
			}
		}
		if (ve.defaultMaterial != null)
		{
			RenderChainCommand renderChainCommand3 = m_Owner.AllocCommand();
			renderChainCommand3.owner = currentElement;
			renderChainCommand3.type = CommandType.PushDefaultMaterial;
			renderChainCommand3.state.material = ve.defaultMaterial;
			m_Entries.Add(new Entry
			{
				customCommand = renderChainCommand3
			});
			m_ClosingInfo.needsClosing = (m_ClosingInfo.PopDefaultMaterial = true);
		}
		if (meshGenerationContext.hasPainter2D)
		{
			meshGenerationContext.painter2D.Reset();
		}
	}

	public void LandClipUnregisterMeshDrawCommand(RenderChainCommand cmd)
	{
		Debug.Assert(m_ClosingInfo.needsClosing);
		m_ClosingInfo.clipUnregisterDrawCommand = cmd;
	}

	public void LandClipRegisterMesh(NativeSlice<Vertex> vertices, NativeSlice<ushort> indices, int indexOffset)
	{
		Debug.Assert(m_ClosingInfo.needsClosing);
		m_ClosingInfo.clipperRegisterVertices = vertices;
		m_ClosingInfo.clipperRegisterIndices = indices;
		m_ClosingInfo.clipperRegisterIndexOffset = indexOffset;
	}

	public MeshWriteData AddGradientsEntry(int vertexCount, int indexCount, TextureId texture, Material material, MeshGenerationContext.MeshFlags flags)
	{
		MeshWriteData pooledMeshWriteData = GetPooledMeshWriteData();
		if (vertexCount == 0 || indexCount == 0)
		{
			pooledMeshWriteData.Reset(default(NativeSlice<Vertex>), default(NativeSlice<ushort>));
			return pooledMeshWriteData;
		}
		m_CurrentEntry = new Entry
		{
			vertices = m_VertsPool.Alloc(vertexCount),
			indices = m_IndicesPool.Alloc(indexCount),
			material = material,
			texture = texture,
			clipRectID = m_ClipRectID,
			stencilRef = m_StencilRef,
			maskDepth = m_MaskDepth,
			addFlags = VertexFlags.IsSvgGradients
		};
		Debug.Assert(m_CurrentEntry.vertices.Length == vertexCount);
		Debug.Assert(m_CurrentEntry.indices.Length == indexCount);
		pooledMeshWriteData.Reset(m_CurrentEntry.vertices, m_CurrentEntry.indices, new Rect(0f, 0f, 1f, 1f));
		m_Entries.Add(m_CurrentEntry);
		totalVertices += m_CurrentEntry.vertices.Length;
		totalIndices += m_CurrentEntry.indices.Length;
		m_CurrentEntry = default(Entry);
		return pooledMeshWriteData;
	}

	public MeshWriteData DrawMesh(int vertexCount, int indexCount, Texture texture, Material material, MeshGenerationContext.MeshFlags flags)
	{
		MeshWriteData pooledMeshWriteData = GetPooledMeshWriteData();
		if (vertexCount == 0 || indexCount == 0)
		{
			pooledMeshWriteData.Reset(default(NativeSlice<Vertex>), default(NativeSlice<ushort>));
			return pooledMeshWriteData;
		}
		m_CurrentEntry = new Entry
		{
			vertices = m_VertsPool.Alloc(vertexCount),
			indices = m_IndicesPool.Alloc(indexCount),
			material = material,
			uvIsDisplacement = ((flags & MeshGenerationContext.MeshFlags.UVisDisplacement) == MeshGenerationContext.MeshFlags.UVisDisplacement),
			clipRectID = m_ClipRectID,
			stencilRef = m_StencilRef,
			maskDepth = m_MaskDepth,
			addFlags = VertexFlags.IsSolid
		};
		Debug.Assert(m_CurrentEntry.vertices.Length == vertexCount);
		Debug.Assert(m_CurrentEntry.indices.Length == indexCount);
		Rect uvRegion = new Rect(0f, 0f, 1f, 1f);
		if (texture != null)
		{
			if ((flags & MeshGenerationContext.MeshFlags.SkipDynamicAtlas) != MeshGenerationContext.MeshFlags.SkipDynamicAtlas && m_Atlas != null && m_Atlas.TryGetAtlas(currentElement, texture as Texture2D, out var atlas, out var atlasRect))
			{
				m_CurrentEntry.addFlags = VertexFlags.IsDynamic;
				uvRegion = new Rect(atlasRect.x, atlasRect.y, atlasRect.width, atlasRect.height);
				m_CurrentEntry.texture = atlas;
				m_Owner.InsertTexture(currentElement, texture, atlas, isAtlas: true);
			}
			else
			{
				TextureId textureId = TextureRegistry.instance.Acquire(texture);
				m_CurrentEntry.addFlags = VertexFlags.IsTextured;
				m_CurrentEntry.texture = textureId;
				m_Owner.InsertTexture(currentElement, texture, textureId, isAtlas: false);
			}
		}
		pooledMeshWriteData.Reset(m_CurrentEntry.vertices, m_CurrentEntry.indices, uvRegion);
		m_Entries.Add(m_CurrentEntry);
		totalVertices += m_CurrentEntry.vertices.Length;
		totalIndices += m_CurrentEntry.indices.Length;
		m_CurrentEntry = default(Entry);
		return pooledMeshWriteData;
	}

	internal void TryAtlasTexture(Texture texture, MeshGenerationContext.MeshFlags flags, out Rect outUVRegion, out bool outIsAtlas, out TextureId outTextureId, out VertexFlags outAddFlags)
	{
		outUVRegion = new Rect(0f, 0f, 1f, 1f);
		outIsAtlas = false;
		outTextureId = default(TextureId);
		outAddFlags = VertexFlags.IsSolid;
		if (!(texture == null))
		{
			if ((flags & MeshGenerationContext.MeshFlags.SkipDynamicAtlas) != MeshGenerationContext.MeshFlags.SkipDynamicAtlas && m_Atlas != null && m_Atlas.TryGetAtlas(currentElement, texture as Texture2D, out var atlas, out var atlasRect))
			{
				outAddFlags = VertexFlags.IsDynamic;
				outUVRegion = new Rect(atlasRect.x, atlasRect.y, atlasRect.width, atlasRect.height);
				outIsAtlas = true;
				outTextureId = atlas;
			}
			else
			{
				outAddFlags = VertexFlags.IsTextured;
				outTextureId = TextureRegistry.instance.Acquire(texture);
			}
		}
	}

	internal unsafe void BuildEntryFromNativeMesh(MeshWriteDataInterface meshData, Texture texture, TextureId textureId, bool isAtlas, Material material, MeshGenerationContext.MeshFlags flags, Rect uvRegion, VertexFlags addFlags)
	{
		if (meshData.vertexCount == 0 || meshData.indexCount == 0)
		{
			return;
		}
		NativeSlice<Vertex> slice = UIRenderDevice.PtrToSlice<Vertex>((void*)meshData.vertices, meshData.vertexCount);
		NativeSlice<ushort> slice2 = UIRenderDevice.PtrToSlice<ushort>((void*)meshData.indices, meshData.indexCount);
		if (slice.Length != 0 && slice2.Length != 0)
		{
			m_CurrentEntry = new Entry
			{
				vertices = m_VertsPool.Alloc(slice.Length),
				indices = m_IndicesPool.Alloc(slice2.Length),
				material = material,
				uvIsDisplacement = ((flags & MeshGenerationContext.MeshFlags.UVisDisplacement) == MeshGenerationContext.MeshFlags.UVisDisplacement),
				clipRectID = m_ClipRectID,
				stencilRef = m_StencilRef,
				maskDepth = m_MaskDepth,
				addFlags = VertexFlags.IsSolid
			};
			if (textureId.index >= 0)
			{
				m_CurrentEntry.addFlags = addFlags;
				m_CurrentEntry.texture = textureId;
				m_Owner.InsertTexture(currentElement, texture, textureId, isAtlas);
			}
			Debug.Assert(m_CurrentEntry.vertices.Length == slice.Length);
			Debug.Assert(m_CurrentEntry.indices.Length == slice2.Length);
			m_CurrentEntry.vertices.CopyFrom(slice);
			m_CurrentEntry.indices.CopyFrom(slice2);
			m_Entries.Add(m_CurrentEntry);
			totalVertices += m_CurrentEntry.vertices.Length;
			totalIndices += m_CurrentEntry.indices.Length;
			m_CurrentEntry = default(Entry);
		}
	}

	internal unsafe void BuildGradientEntryFromNativeMesh(MeshWriteDataInterface meshData, TextureId svgTextureId)
	{
		if (meshData.vertexCount != 0 && meshData.indexCount != 0)
		{
			NativeSlice<Vertex> slice = UIRenderDevice.PtrToSlice<Vertex>((void*)meshData.vertices, meshData.vertexCount);
			NativeSlice<ushort> slice2 = UIRenderDevice.PtrToSlice<ushort>((void*)meshData.indices, meshData.indexCount);
			if (slice.Length != 0 && slice2.Length != 0)
			{
				m_CurrentEntry = new Entry
				{
					vertices = m_VertsPool.Alloc(slice.Length),
					indices = m_IndicesPool.Alloc(slice2.Length),
					texture = svgTextureId,
					clipRectID = m_ClipRectID,
					stencilRef = m_StencilRef,
					maskDepth = m_MaskDepth,
					addFlags = VertexFlags.IsSvgGradients
				};
				Debug.Assert(m_CurrentEntry.vertices.Length == slice.Length);
				Debug.Assert(m_CurrentEntry.indices.Length == slice2.Length);
				m_CurrentEntry.vertices.CopyFrom(slice);
				m_CurrentEntry.indices.CopyFrom(slice2);
				m_Entries.Add(m_CurrentEntry);
				totalVertices += m_CurrentEntry.vertices.Length;
				totalIndices += m_CurrentEntry.indices.Length;
				m_CurrentEntry = default(Entry);
			}
		}
	}

	public unsafe void BuildRawEntryFromNativeMesh(MeshWriteDataInterface meshData)
	{
		if (meshData.vertexCount != 0 && meshData.indexCount != 0)
		{
			NativeSlice<Vertex> slice = UIRenderDevice.PtrToSlice<Vertex>((void*)meshData.vertices, meshData.vertexCount);
			NativeSlice<ushort> slice2 = UIRenderDevice.PtrToSlice<ushort>((void*)meshData.indices, meshData.indexCount);
			if (slice.Length != 0 && slice2.Length != 0)
			{
				m_CurrentEntry.vertices = m_VertsPool.Alloc(meshData.vertexCount);
				m_CurrentEntry.indices = m_IndicesPool.Alloc(meshData.indexCount);
				m_CurrentEntry.vertices.CopyFrom(slice);
				m_CurrentEntry.indices.CopyFrom(slice2);
			}
		}
	}

	public void DrawText(TextElement te)
	{
		if (TextUtilities.IsFontAssigned(te))
		{
			TextInfo textInfo = te.uitkTextHandle.Update();
			bool hasMultipleColors = textInfo.hasMultipleColors;
			if (hasMultipleColors)
			{
				te.renderChainData.flags |= RenderDataFlags.IsIgnoringDynamicColorHint;
			}
			else
			{
				te.renderChainData.flags &= ~RenderDataFlags.IsIgnoringDynamicColorHint;
			}
			DrawTextInfo(textInfo, te.contentRect.min, !hasMultipleColors);
		}
	}

	public void DrawText(string text, Vector2 pos, float fontSize, Color color, FontAsset font)
	{
		PanelTextSettings textSettingsFrom = TextUtilities.GetTextSettingsFrom(currentElement);
		m_TextInfo.Clear();
		UnityEngine.TextCore.Text.TextGenerationSettings settings = new UnityEngine.TextCore.Text.TextGenerationSettings
		{
			text = text,
			screenRect = Rect.zero,
			fontAsset = font,
			textSettings = textSettingsFrom,
			fontSize = fontSize,
			color = color,
			material = font.material,
			inverseYAxis = true
		};
		UnityEngine.TextCore.Text.TextGenerator.GenerateText(settings, m_TextInfo);
		DrawTextInfo(m_TextInfo, pos, useHints: false);
	}

	private void DrawTextInfo(TextInfo textInfo, Vector2 offset, bool useHints)
	{
		for (int i = 0; i < textInfo.materialCount; i++)
		{
			if (textInfo.meshInfo[i].vertexCount == 0)
			{
				continue;
			}
			m_CurrentEntry.clipRectID = m_ClipRectID;
			m_CurrentEntry.stencilRef = m_StencilRef;
			m_CurrentEntry.maskDepth = m_MaskDepth;
			if (((Texture2D)textInfo.meshInfo[i].material.mainTexture).format != TextureFormat.Alpha8)
			{
				Texture mainTexture = textInfo.meshInfo[i].material.mainTexture;
				TextureId textureId = TextureRegistry.instance.Acquire(mainTexture);
				m_CurrentEntry.texture = textureId;
				m_Owner.InsertTexture(currentElement, mainTexture, textureId, isAtlas: false);
				MeshBuilder.MakeText(textInfo.meshInfo[i], offset, new MeshBuilder.AllocMeshData
				{
					alloc = m_AllocRawVertsIndicesDelegate
				}, VertexFlags.IsTextured);
			}
			else
			{
				Texture mainTexture2 = textInfo.meshInfo[i].material.mainTexture;
				float fontTexSDFScale = 0f;
				if (!TextGeneratorUtilities.IsBitmapRendering(textInfo.meshInfo[i].glyphRenderMode))
				{
					fontTexSDFScale = textInfo.meshInfo[i].material.GetFloat(TextShaderUtilities.ID_GradientScale);
				}
				m_CurrentEntry.isTextEntry = true;
				m_CurrentEntry.fontTexSDFScale = fontTexSDFScale;
				m_CurrentEntry.texture = TextureRegistry.instance.Acquire(mainTexture2);
				m_Owner.InsertTexture(currentElement, mainTexture2, m_CurrentEntry.texture, isAtlas: false);
				bool isDynamicColor = useHints && RenderEvents.NeedsColorID(currentElement);
				MeshBuilder.MakeText(textInfo.meshInfo[i], offset, new MeshBuilder.AllocMeshData
				{
					alloc = m_AllocRawVertsIndicesDelegate
				}, VertexFlags.IsText, isDynamicColor);
			}
			m_Entries.Add(m_CurrentEntry);
			totalVertices += m_CurrentEntry.vertices.Length;
			totalIndices += m_CurrentEntry.indices.Length;
			m_CurrentEntry = default(Entry);
		}
	}

	public void DrawRectangle(MeshGenerationContextUtils.RectangleParams rectParams)
	{
		if (!(rectParams.rect.width < 1E-30f) && !(rectParams.rect.height < 1E-30f))
		{
			if (rectParams.vectorImage != null)
			{
				DrawVectorImage(rectParams);
				return;
			}
			if (rectParams.sprite != null)
			{
				DrawSprite(rectParams);
				return;
			}
			TryAtlasTexture(rectParams.texture, rectParams.meshFlags, out var outUVRegion, out var outIsAtlas, out var outTextureId, out var outAddFlags);
			MeshBuilderNative.NativeRectParams rectParams2 = rectParams.ToNativeParams(outUVRegion);
			MeshWriteDataInterface meshData = ((!(rectParams.texture != null)) ? MeshBuilderNative.MakeSolidRect(rectParams2, 0f) : MeshBuilderNative.MakeTexturedRect(rectParams2, 0f));
			BuildEntryFromNativeMesh(meshData, rectParams.texture, outTextureId, outIsAtlas, rectParams.material, rectParams.meshFlags, outUVRegion, outAddFlags);
		}
	}

	public void DrawBorder(MeshGenerationContextUtils.BorderParams borderParams)
	{
		MeshWriteDataInterface meshData = MeshBuilderNative.MakeBorder(borderParams.ToNativeParams(), 0f);
		BuildEntryFromNativeMesh(meshData, null, default(TextureId), isAtlas: false, null, MeshGenerationContext.MeshFlags.None, new Rect(0f, 0f, 1f, 1f), VertexFlags.IsSolid);
	}

	public void DrawImmediate(Action callback, bool cullingEnabled)
	{
		RenderChainCommand renderChainCommand = m_Owner.AllocCommand();
		renderChainCommand.type = (cullingEnabled ? CommandType.ImmediateCull : CommandType.Immediate);
		renderChainCommand.owner = currentElement;
		renderChainCommand.callback = callback;
		m_Entries.Add(new Entry
		{
			customCommand = renderChainCommand
		});
	}

	public void DrawVectorImage(VectorImage vectorImage, Vector2 offset, Angle rotationAngle, Vector2 scale)
	{
		if (vectorImage == null)
		{
			return;
		}
		int settingIndexOffset = 0;
		TextureId texture = default(TextureId);
		MeshWriteData meshWriteData;
		if (vectorImage.atlas != null)
		{
			RegisterVectorImageGradient(vectorImage, out settingIndexOffset, out texture);
			meshWriteData = AddGradientsEntry(vectorImage.vertices.Length, vectorImage.indices.Length, texture, null, MeshGenerationContext.MeshFlags.None);
		}
		else
		{
			meshWriteData = DrawMesh(vectorImage.vertices.Length, vectorImage.indices.Length, null, null, MeshGenerationContext.MeshFlags.None);
		}
		Matrix4x4 matrix4x = Matrix4x4.TRS(offset, Quaternion.AngleAxis(rotationAngle.ToDegrees(), Vector3.forward), new Vector3(scale.x, scale.y, 1f));
		bool flag = (scale.x < 0f) ^ (scale.y < 0f);
		int num = vectorImage.vertices.Length;
		for (int i = 0; i < num; i++)
		{
			VectorImageVertex vectorImageVertex = vectorImage.vertices[i];
			Vector3 position = matrix4x.MultiplyPoint3x4(vectorImageVertex.position);
			position.z = Vertex.nearZ;
			uint num2 = (uint)(vectorImageVertex.settingIndex + settingIndexOffset);
			Color32 settingIndex = new Color32((byte)(num2 >> 8), (byte)num2, 0, 0);
			meshWriteData.SetNextVertex(new Vertex
			{
				position = position,
				tint = vectorImageVertex.tint,
				uv = vectorImageVertex.uv,
				settingIndex = settingIndex,
				flags = vectorImageVertex.flags,
				circle = vectorImageVertex.circle
			});
		}
		if (!flag)
		{
			meshWriteData.SetAllIndices(vectorImage.indices);
			return;
		}
		ushort[] indices = vectorImage.indices;
		for (int j = 0; j < indices.Length; j += 3)
		{
			meshWriteData.SetNextIndex(indices[j]);
			meshWriteData.SetNextIndex(indices[j + 2]);
			meshWriteData.SetNextIndex(indices[j + 1]);
		}
	}

	public void DrawVisualElementBackground()
	{
		if (currentElement.layout.width <= 1E-30f || currentElement.layout.height <= 1E-30f)
		{
			return;
		}
		ComputedStyle computedStyle = currentElement.computedStyle;
		if (computedStyle.backgroundColor.a > 1E-30f)
		{
			MeshGenerationContextUtils.RectangleParams rectParams = new MeshGenerationContextUtils.RectangleParams
			{
				rect = currentElement.rect,
				color = computedStyle.backgroundColor,
				colorPage = ColorPage.Init(m_Owner, currentElement.renderChainData.backgroundColorID),
				playmodeTintColor = ((currentElement.panel.contextType == ContextType.Editor) ? UIElementsUtility.editorPlayModeTintColor : Color.white)
			};
			MeshGenerationContextUtils.GetVisualElementRadii(currentElement, out rectParams.topLeftRadius, out rectParams.bottomLeftRadius, out rectParams.topRightRadius, out rectParams.bottomRightRadius);
			MeshGenerationContextUtils.AdjustBackgroundSizeForBorders(currentElement, ref rectParams);
			DrawRectangle(rectParams);
		}
		Vector4 slices = new Vector4(computedStyle.unitySliceLeft, computedStyle.unitySliceTop, computedStyle.unitySliceRight, computedStyle.unitySliceBottom);
		MeshGenerationContextUtils.RectangleParams rectangleParams = default(MeshGenerationContextUtils.RectangleParams);
		MeshGenerationContextUtils.GetVisualElementRadii(currentElement, out rectangleParams.topLeftRadius, out rectangleParams.bottomLeftRadius, out rectangleParams.topRightRadius, out rectangleParams.bottomRightRadius);
		Background backgroundImage = computedStyle.backgroundImage;
		if (!(backgroundImage.texture != null) && !(backgroundImage.sprite != null) && !(backgroundImage.vectorImage != null) && !(backgroundImage.renderTexture != null))
		{
			return;
		}
		MeshGenerationContextUtils.RectangleParams rectParams2 = default(MeshGenerationContextUtils.RectangleParams);
		float num = visualElement.resolvedStyle.unitySliceScale;
		bool valid;
		ScaleMode scaleMode = BackgroundPropertyHelper.ResolveUnityBackgroundScaleMode(computedStyle.backgroundPositionX, computedStyle.backgroundPositionY, computedStyle.backgroundRepeat, computedStyle.backgroundSize, out valid);
		if (backgroundImage.texture != null)
		{
			bool flag = Mathf.RoundToInt(slices.x) != 0 || Mathf.RoundToInt(slices.y) != 0 || Mathf.RoundToInt(slices.z) != 0 || Mathf.RoundToInt(slices.w) != 0;
			rectParams2 = MeshGenerationContextUtils.RectangleParams.MakeTextured(currentElement.rect, new Rect(0f, 0f, 1f, 1f), backgroundImage.texture, (!flag) ? ScaleMode.ScaleToFit : (valid ? scaleMode : ScaleMode.StretchToFill), currentElement.panel.contextType);
			rectParams2.rect = new Rect(0f, 0f, rectParams2.texture.width, rectParams2.texture.height);
		}
		else if (backgroundImage.sprite != null)
		{
			bool flag2 = !valid || scaleMode == ScaleMode.ScaleAndCrop;
			rectParams2 = MeshGenerationContextUtils.RectangleParams.MakeSprite(currentElement.rect, new Rect(0f, 0f, 1f, 1f), backgroundImage.sprite, (!flag2) ? scaleMode : ScaleMode.StretchToFill, currentElement.panel.contextType, rectangleParams.HasRadius(0.001f), ref slices, flag2);
			if (rectParams2.texture != null)
			{
				rectParams2.rect = new Rect(0f, 0f, backgroundImage.sprite.rect.width, backgroundImage.sprite.rect.height);
			}
			num *= UIElementsUtility.PixelsPerUnitScaleForElement(visualElement, backgroundImage.sprite);
		}
		else if (backgroundImage.renderTexture != null)
		{
			rectParams2 = MeshGenerationContextUtils.RectangleParams.MakeTextured(currentElement.rect, new Rect(0f, 0f, 1f, 1f), backgroundImage.renderTexture, ScaleMode.ScaleToFit, currentElement.panel.contextType);
			rectParams2.rect = new Rect(0f, 0f, rectParams2.texture.width, rectParams2.texture.height);
		}
		else if (backgroundImage.vectorImage != null)
		{
			bool flag3 = !valid || scaleMode == ScaleMode.ScaleAndCrop;
			rectParams2 = MeshGenerationContextUtils.RectangleParams.MakeVectorTextured(currentElement.rect, new Rect(0f, 0f, 1f, 1f), backgroundImage.vectorImage, (!flag3) ? scaleMode : ScaleMode.StretchToFill, currentElement.panel.contextType);
			rectParams2.rect = new Rect(0f, 0f, rectParams2.vectorImage.size.x, rectParams2.vectorImage.size.y);
		}
		rectParams2.topLeftRadius = rectangleParams.topLeftRadius;
		rectParams2.topRightRadius = rectangleParams.topRightRadius;
		rectParams2.bottomRightRadius = rectangleParams.bottomRightRadius;
		rectParams2.bottomLeftRadius = rectangleParams.bottomLeftRadius;
		if (slices != Vector4.zero)
		{
			rectParams2.leftSlice = Mathf.RoundToInt(slices.x);
			rectParams2.topSlice = Mathf.RoundToInt(slices.y);
			rectParams2.rightSlice = Mathf.RoundToInt(slices.z);
			rectParams2.bottomSlice = Mathf.RoundToInt(slices.w);
			rectParams2.sliceScale = num;
			if (!valid)
			{
				rectParams2.backgroundPositionX = BackgroundPropertyHelper.ConvertScaleModeToBackgroundPosition();
				rectParams2.backgroundPositionY = BackgroundPropertyHelper.ConvertScaleModeToBackgroundPosition();
				rectParams2.backgroundRepeat = BackgroundPropertyHelper.ConvertScaleModeToBackgroundRepeat();
				rectParams2.backgroundSize = BackgroundPropertyHelper.ConvertScaleModeToBackgroundSize();
			}
			else
			{
				rectParams2.backgroundPositionX = computedStyle.backgroundPositionX;
				rectParams2.backgroundPositionY = computedStyle.backgroundPositionY;
				rectParams2.backgroundRepeat = computedStyle.backgroundRepeat;
				rectParams2.backgroundSize = computedStyle.backgroundSize;
			}
		}
		else
		{
			rectParams2.backgroundPositionX = computedStyle.backgroundPositionX;
			rectParams2.backgroundPositionY = computedStyle.backgroundPositionY;
			rectParams2.backgroundRepeat = computedStyle.backgroundRepeat;
			rectParams2.backgroundSize = computedStyle.backgroundSize;
		}
		rectParams2.color = computedStyle.unityBackgroundImageTintColor;
		rectParams2.colorPage = ColorPage.Init(m_Owner, currentElement.renderChainData.tintColorID);
		MeshGenerationContextUtils.AdjustBackgroundSizeForBorders(currentElement, ref rectParams2);
		if (rectParams2.texture != null || rectParams2.vectorImage != null)
		{
			DrawRectangleRepeat(rectParams2, currentElement.rect, currentElement.scaledPixelsPerPoint);
		}
		else
		{
			DrawRectangle(rectParams2);
		}
	}

	private void DrawRectangleRepeat(MeshGenerationContextUtils.RectangleParams rectParams, Rect totalRect, float scaledPixelsPerPoint)
	{
		Rect rect = new Rect(0f, 0f, 1f, 1f);
		if (m_RepeatRectUVList == null)
		{
			m_RepeatRectUVList = new List<RepeatRectUV>[2];
			m_RepeatRectUVList[0] = new List<RepeatRectUV>();
			m_RepeatRectUVList[1] = new List<RepeatRectUV>();
		}
		else
		{
			m_RepeatRectUVList[0].Clear();
			m_RepeatRectUVList[1].Clear();
		}
		Rect rect2 = rectParams.rect;
		if (rectParams.backgroundSize.sizeType != BackgroundSizeType.Length)
		{
			if (rectParams.backgroundSize.sizeType == BackgroundSizeType.Contain)
			{
				float num = totalRect.width / rect2.width;
				float num2 = totalRect.height / rect2.height;
				Rect rect3 = rect2;
				if (num < num2)
				{
					rect3.width = totalRect.width;
					rect3.height = rect2.height * totalRect.width / rect2.width;
				}
				else
				{
					rect3.width = rect2.width * totalRect.height / rect2.height;
					rect3.height = totalRect.height;
				}
				rect2 = rect3;
			}
			else if (rectParams.backgroundSize.sizeType == BackgroundSizeType.Cover)
			{
				float num3 = totalRect.width / rect2.width;
				float num4 = totalRect.height / rect2.height;
				Rect rect4 = rect2;
				if (num3 > num4)
				{
					rect4.width = totalRect.width;
					rect4.height = rect2.height * totalRect.width / rect2.width;
				}
				else
				{
					rect4.width = rect2.width * totalRect.height / rect2.height;
					rect4.height = totalRect.height;
				}
				rect2 = rect4;
			}
		}
		else if (!rectParams.backgroundSize.x.IsNone() || !rectParams.backgroundSize.y.IsNone())
		{
			if (!rectParams.backgroundSize.x.IsNone() && rectParams.backgroundSize.y.IsAuto())
			{
				Rect rect5 = rect2;
				if (rectParams.backgroundSize.x.unit == LengthUnit.Percent)
				{
					rect5.width = totalRect.width * rectParams.backgroundSize.x.value / 100f;
					rect5.height = rect5.width * rect2.height / rect2.width;
				}
				else if (rectParams.backgroundSize.x.unit == LengthUnit.Pixel)
				{
					rect5.width = rectParams.backgroundSize.x.value;
					rect5.height = rect5.width * rect2.height / rect2.width;
				}
				rect2 = rect5;
			}
			else if (!rectParams.backgroundSize.x.IsNone() && !rectParams.backgroundSize.y.IsNone())
			{
				Rect rect6 = rect2;
				if (!rectParams.backgroundSize.x.IsAuto())
				{
					if (rectParams.backgroundSize.x.unit == LengthUnit.Percent)
					{
						rect6.width = totalRect.width * rectParams.backgroundSize.x.value / 100f;
					}
					else if (rectParams.backgroundSize.x.unit == LengthUnit.Pixel)
					{
						rect6.width = rectParams.backgroundSize.x.value;
					}
				}
				if (!rectParams.backgroundSize.y.IsAuto())
				{
					if (rectParams.backgroundSize.y.unit == LengthUnit.Percent)
					{
						rect6.height = totalRect.height * rectParams.backgroundSize.y.value / 100f;
					}
					else if (rectParams.backgroundSize.y.unit == LengthUnit.Pixel)
					{
						rect6.height = rectParams.backgroundSize.y.value;
					}
					if (rectParams.backgroundSize.x.IsAuto())
					{
						rect6.width = rect6.height * rect2.width / rect2.height;
					}
				}
				rect2 = rect6;
			}
		}
		if (rect2.size.x <= 1E-30f || rect2.size.y <= 1E-30f || totalRect.size.x <= 1E-30f || totalRect.size.y <= 1E-30f)
		{
			return;
		}
		if (rectParams.backgroundSize.x.IsAuto() && rectParams.backgroundRepeat.y == Repeat.Round)
		{
			float num5 = 1f / rect2.height;
			int val = (int)(totalRect.height * num5 + 0.5f);
			val = Math.Max(val, 1);
			Rect rect7 = default(Rect);
			rect7.height = totalRect.height / (float)val;
			rect7.width = rect7.height * rect2.width * num5;
			rect2 = rect7;
		}
		else if (rectParams.backgroundSize.y.IsAuto() && rectParams.backgroundRepeat.x == Repeat.Round)
		{
			float num6 = 1f / rect2.width;
			int val2 = (int)(totalRect.width * num6 + 0.5f);
			val2 = Math.Max(val2, 1);
			Rect rect8 = default(Rect);
			rect8.width = totalRect.width / (float)val2;
			rect8.height = rect8.width * rect2.height * num6;
			rect2 = rect8;
		}
		RepeatRectUV item2 = default(RepeatRectUV);
		RepeatRectUV item6 = default(RepeatRectUV);
		RepeatRectUV item3 = default(RepeatRectUV);
		RepeatRectUV item4 = default(RepeatRectUV);
		RepeatRectUV item5 = default(RepeatRectUV);
		RepeatRectUV item = default(RepeatRectUV);
		for (int i = 0; i < 2; i++)
		{
			Repeat repeat = ((i == 0) ? rectParams.backgroundRepeat.x : rectParams.backgroundRepeat.y);
			BackgroundPosition backgroundPosition = ((i == 0) ? rectParams.backgroundPositionX : rectParams.backgroundPositionY);
			float num7 = 0f;
			switch (repeat)
			{
			case Repeat.NoRepeat:
			{
				Rect rect10 = rect2;
				item2.uv = rect;
				item2.rect = rect10;
				num7 = rect10.size[i];
				m_RepeatRectUVList[i].Add(item2);
				break;
			}
			case Repeat.Repeat:
			{
				Rect rect12 = rect2;
				int num11 = (int)((totalRect.size[i] + 1f / scaledPixelsPerPoint) / rect2.size[i]);
				num11 = ((backgroundPosition.keyword != BackgroundPositionKeyword.Center) ? (num11 + 2) : (((num11 & 1) != 1) ? (num11 + 1) : (num11 + 2)));
				for (int l = 0; l < num11; l++)
				{
					Vector2 position4 = rect12.position;
					position4[i] = (float)l * rect2.size[i];
					rect12.position = position4;
					item6.rect = rect12;
					item6.uv = rect;
					num7 += item6.rect.size[i];
					m_RepeatRectUVList[i].Add(item6);
				}
				break;
			}
			case Repeat.Space:
			{
				Rect rect11 = rect2;
				int num9 = (int)(totalRect.size[i] / rect2.size[i]);
				if (num9 >= 0)
				{
					item3.rect = rect11;
					item3.uv = rect;
					m_RepeatRectUVList[i].Add(item3);
					num7 = rect2.size[i];
				}
				if (num9 >= 2)
				{
					Vector2 position2 = rect11.position;
					position2[i] = totalRect.size[i] - rect2.size[i];
					rect11.position = position2;
					item4.rect = rect11;
					item4.uv = rect;
					m_RepeatRectUVList[i].Add(item4);
					num7 = totalRect.size[i];
				}
				if (num9 > 2)
				{
					float num10 = (totalRect.size[i] - rect2.size[i] * (float)num9) / (float)(num9 - 1);
					for (int k = 0; k < num9 - 2; k++)
					{
						Vector2 position3 = rect11.position;
						position3[i] = (rect2.size[i] + num10) * (float)(1 + k);
						rect11.position = position3;
						item5.rect = rect11;
						item5.uv = rect;
						m_RepeatRectUVList[i].Add(item5);
					}
				}
				break;
			}
			case Repeat.Round:
			{
				int val3 = (int)((totalRect.size[i] + rect2.size[i] * 0.5f) / rect2.size[i]);
				val3 = Math.Max(val3, 1);
				float num8 = totalRect.size[i] / (float)val3;
				val3 = ((backgroundPosition.keyword != BackgroundPositionKeyword.Center) ? (val3 + 1) : (((val3 & 1) != 1) ? (val3 + 1) : (val3 + 2)));
				Rect rect9 = rect2;
				Vector2 size = rect9.size;
				size[i] = num8;
				rect9.size = size;
				rect2 = rect9;
				for (int j = 0; j < val3; j++)
				{
					Vector2 position = rect9.position;
					position[i] = num8 * (float)j;
					rect9.position = position;
					item.rect = rect9;
					item.uv = rect;
					m_RepeatRectUVList[i].Add(item);
					num7 += item.rect.size[i];
				}
				break;
			}
			}
			float num12 = 0f;
			bool flag = false;
			if (backgroundPosition.keyword == BackgroundPositionKeyword.Center)
			{
				num12 = (totalRect.size[i] - num7) * 0.5f;
				flag = true;
			}
			else if (repeat != Repeat.Space)
			{
				if (backgroundPosition.offset.unit == LengthUnit.Percent)
				{
					num12 = (totalRect.size[i] - rect2.size[i]) * backgroundPosition.offset.value / 100f;
					flag = true;
				}
				else if (backgroundPosition.offset.unit == LengthUnit.Pixel)
				{
					num12 = backgroundPosition.offset.value;
				}
				if (backgroundPosition.keyword == BackgroundPositionKeyword.Right || backgroundPosition.keyword == BackgroundPositionKeyword.Bottom)
				{
					num12 = totalRect.size[i] - num7 - num12;
				}
			}
			if (flag && rectParams.sprite == null && rectParams.vectorImage == null)
			{
				float num13 = rect2.size[i] * scaledPixelsPerPoint;
				if (Mathf.Abs(Mathf.Round(num13) - num13) < 0.001f)
				{
					num12 = AlignmentUtils.CeilToPixelGrid(num12, scaledPixelsPerPoint);
				}
			}
			if (repeat == Repeat.Repeat || repeat == Repeat.Round)
			{
				float num14 = rect2.size[i];
				if (num14 > 1E-30f)
				{
					if (num12 < 0f - num14)
					{
						int num15 = (int)((0f - num12) / num14);
						num12 += (float)num15 * num14;
					}
					if (num12 > 0f)
					{
						int num16 = (int)(num12 / num14);
						num12 -= (float)(1 + num16) * num14;
					}
				}
			}
			for (int m = 0; m < m_RepeatRectUVList[i].Count; m++)
			{
				RepeatRectUV value = m_RepeatRectUVList[i][m];
				Vector2 position5 = value.rect.position;
				position5[i] += num12;
				value.rect.position = position5;
				m_RepeatRectUVList[i][m] = value;
			}
		}
		Rect rect13 = new Rect(rect);
		foreach (RepeatRectUV item7 in m_RepeatRectUVList[1])
		{
			Rect rect14 = item7.rect;
			rect2.y = rect14.y;
			rect14 = item7.rect;
			rect2.height = rect14.height;
			rect14 = item7.uv;
			rect.y = rect14.y;
			rect14 = item7.uv;
			rect.height = rect14.height;
			if (rect2.y < totalRect.y)
			{
				float num17 = totalRect.y - rect2.y;
				float num18 = rect2.height - num17;
				float num19 = num17 + num18;
				float height = rect13.height * num18 / num19;
				float num20 = rect13.height * num17 / num19;
				rect.y = num20 + rect13.y;
				rect.height = height;
				rect2.y = totalRect.y;
				rect2.height = num18;
			}
			if (rect2.yMax > totalRect.yMax)
			{
				float num21 = rect2.yMax - totalRect.yMax;
				float num22 = rect2.height - num21;
				float num23 = num22 + num21;
				float num24 = (rect.height = rect.height * num22 / num23);
				rect.y = rect.yMax - num24;
				rect2.height = num22;
			}
			if (rectParams.vectorImage == null)
			{
				float num26 = rect.y - rect13.y;
				float num27 = rect13.yMax - rect.yMax;
				rect.y += num27 - num26;
			}
			foreach (RepeatRectUV item8 in m_RepeatRectUVList[0])
			{
				rect14 = item8.rect;
				rect2.x = rect14.x;
				rect14 = item8.rect;
				rect2.width = rect14.width;
				rect14 = item8.uv;
				rect.x = rect14.x;
				rect14 = item8.uv;
				rect.width = rect14.width;
				if (rect2.x < totalRect.x)
				{
					float num28 = totalRect.x - rect2.x;
					float num29 = rect2.width - num28;
					float num30 = num28 + num29;
					float width = rect.width * num29 / num30;
					float x = rect13.x + rect13.width * num28 / num30;
					rect.x = x;
					rect.width = width;
					rect2.x = totalRect.x;
					rect2.width = num29;
				}
				if (rect2.xMax > totalRect.xMax)
				{
					float num31 = rect2.xMax - totalRect.xMax;
					float num32 = rect2.width - num31;
					float num33 = num32 + num31;
					float width2 = rect.width * num32 / num33;
					rect.width = width2;
					rect2.width = num32;
				}
				StampRectangleWithSubRect(rectParams, rect2, totalRect, rect);
			}
		}
	}

	private void StampRectangleWithSubRect(MeshGenerationContextUtils.RectangleParams rectParams, Rect targetRect, Rect totalRect, Rect targetUV)
	{
		if (targetRect.width < 0.001f || targetRect.height < 0.001f)
		{
			return;
		}
		Rect rect = targetRect;
		rect.size /= targetUV.size;
		rect.position -= new Vector2(targetUV.position.x, 1f - targetUV.position.y - targetUV.size.y) * rect.size;
		Rect subRect = rectParams.subRect;
		subRect.position *= rect.size;
		subRect.position += rect.position;
		subRect.size *= rect.size;
		if (rectParams.HasSlices(0.001f))
		{
			rectParams.backgroundRepeatRect = Rect.zero;
			rectParams.rect = targetRect;
		}
		else
		{
			Rect rect2 = MeshGenerationContextUtils.RectangleParams.RectIntersection(subRect, targetRect);
			if (rect2.size.x < 0.001f || rect2.size.y < 0.001f)
			{
				return;
			}
			if (rect2.size != subRect.size)
			{
				Vector2 vector = rect2.size / subRect.size;
				Vector2 vector2 = rectParams.uv.size * vector;
				Vector2 vector3 = rectParams.uv.size - vector2;
				if (rect2.x > subRect.x)
				{
					float num = (subRect.xMax - rect2.xMax) / subRect.width * rectParams.uv.size.x;
					rectParams.uv.x += vector3.x - num;
				}
				if (rect2.yMax < subRect.yMax)
				{
					float num2 = (rect2.y - subRect.y) / subRect.height * rectParams.uv.size.y;
					rectParams.uv.y += vector3.y - num2;
				}
				rectParams.uv.size = vector2;
			}
			if (rectParams.vectorImage != null)
			{
				rectParams.backgroundRepeatRect = Rect.zero;
				rectParams.rect = rect2;
			}
			else
			{
				if (totalRect == rect2)
				{
					rectParams.backgroundRepeatRect = Rect.zero;
				}
				else
				{
					rectParams.backgroundRepeatRect = rect2;
				}
				rectParams.rect = totalRect;
			}
		}
		DrawRectangle(rectParams);
	}

	public void DrawVisualElementBorder()
	{
		if (currentElement.layout.width >= 1E-30f && currentElement.layout.height >= 1E-30f)
		{
			IResolvedStyle resolvedStyle = currentElement.resolvedStyle;
			if ((resolvedStyle.borderLeftColor != Color.clear && resolvedStyle.borderLeftWidth > 0f) || (resolvedStyle.borderTopColor != Color.clear && resolvedStyle.borderTopWidth > 0f) || (resolvedStyle.borderRightColor != Color.clear && resolvedStyle.borderRightWidth > 0f) || (resolvedStyle.borderBottomColor != Color.clear && resolvedStyle.borderBottomWidth > 0f))
			{
				MeshGenerationContextUtils.BorderParams borderParams = new MeshGenerationContextUtils.BorderParams
				{
					rect = currentElement.rect,
					leftColor = resolvedStyle.borderLeftColor,
					topColor = resolvedStyle.borderTopColor,
					rightColor = resolvedStyle.borderRightColor,
					bottomColor = resolvedStyle.borderBottomColor,
					leftWidth = resolvedStyle.borderLeftWidth,
					topWidth = resolvedStyle.borderTopWidth,
					rightWidth = resolvedStyle.borderRightWidth,
					bottomWidth = resolvedStyle.borderBottomWidth,
					leftColorPage = ColorPage.Init(m_Owner, currentElement.renderChainData.borderLeftColorID),
					topColorPage = ColorPage.Init(m_Owner, currentElement.renderChainData.borderTopColorID),
					rightColorPage = ColorPage.Init(m_Owner, currentElement.renderChainData.borderRightColorID),
					bottomColorPage = ColorPage.Init(m_Owner, currentElement.renderChainData.borderBottomColorID),
					playmodeTintColor = ((currentElement.panel.contextType == ContextType.Editor) ? UIElementsUtility.editorPlayModeTintColor : Color.white)
				};
				MeshGenerationContextUtils.GetVisualElementRadii(currentElement, out borderParams.topLeftRadius, out borderParams.bottomLeftRadius, out borderParams.topRightRadius, out borderParams.bottomRightRadius);
				DrawBorder(borderParams);
			}
		}
	}

	public void ApplyVisualElementClipping()
	{
		if (currentElement.renderChainData.clipMethod == ClipMethod.Scissor)
		{
			RenderChainCommand renderChainCommand = m_Owner.AllocCommand();
			renderChainCommand.type = CommandType.PushScissor;
			renderChainCommand.owner = currentElement;
			m_Entries.Add(new Entry
			{
				customCommand = renderChainCommand
			});
			m_ClosingInfo.needsClosing = (m_ClosingInfo.popScissorClip = true);
		}
		else if (currentElement.renderChainData.clipMethod == ClipMethod.Stencil)
		{
			if (m_MaskDepth > m_StencilRef)
			{
				m_StencilRef++;
				Debug.Assert(m_MaskDepth == m_StencilRef);
			}
			m_ClosingInfo.maskStencilRef = m_StencilRef;
			if (UIRUtility.IsVectorImageBackground(currentElement))
			{
				GenerateStencilClipEntryForSVGBackground();
			}
			else
			{
				GenerateStencilClipEntryForRoundedRectBackground();
			}
			m_MaskDepth++;
		}
		m_ClipRectID = currentElement.renderChainData.clipRectID;
	}

	private ushort[] AdjustSpriteWinding(Vector2[] vertices, ushort[] indices)
	{
		ushort[] array = new ushort[indices.Length];
		for (int i = 0; i < indices.Length; i += 3)
		{
			Vector3 vector = vertices[indices[i]];
			Vector3 vector2 = vertices[indices[i + 1]];
			Vector3 vector3 = vertices[indices[i + 2]];
			Vector3 normalized = (vector2 - vector).normalized;
			Vector3 normalized2 = (vector3 - vector).normalized;
			if (Vector3.Cross(normalized, normalized2).z >= 0f)
			{
				array[i] = indices[i + 1];
				array[i + 1] = indices[i];
				array[i + 2] = indices[i + 2];
			}
			else
			{
				array[i] = indices[i];
				array[i + 1] = indices[i + 1];
				array[i + 2] = indices[i + 2];
			}
		}
		return array;
	}

	public void DrawSprite(MeshGenerationContextUtils.RectangleParams rectParams)
	{
		Sprite sprite = rectParams.sprite;
		if (!(sprite.texture == null) && sprite.triangles.Length != 0)
		{
			MeshBuilder.AllocMeshData allocMeshData = new MeshBuilder.AllocMeshData
			{
				alloc = m_AllocThroughDrawMeshDelegate,
				texture = sprite.texture,
				flags = rectParams.meshFlags
			};
			Vector2[] vertices = sprite.vertices;
			ushort[] triangles = sprite.triangles;
			Vector2[] uv = sprite.uv;
			int num = sprite.vertices.Length;
			Vertex[] array = new Vertex[num];
			ushort[] array2 = AdjustSpriteWinding(vertices, triangles);
			MeshWriteData meshWriteData = allocMeshData.Allocate((uint)array.Length, (uint)array2.Length);
			Rect uvRegion = meshWriteData.uvRegion;
			ColorPage colorPage = rectParams.colorPage;
			Color32 pageAndID = colorPage.pageAndID;
			Color32 flags = new Color32(0, 0, 0, (byte)(colorPage.isValid ? 1 : 0));
			Color32 opacityColorPages = new Color32(0, 0, colorPage.pageAndID.r, colorPage.pageAndID.g);
			Color32 ids = new Color32(0, 0, 0, colorPage.pageAndID.b);
			for (int i = 0; i < num; i++)
			{
				Vector2 vector = vertices[i];
				vector -= rectParams.spriteGeomRect.position;
				vector /= rectParams.spriteGeomRect.size;
				vector.y = 1f - vector.y;
				vector *= rectParams.rect.size;
				vector += rectParams.rect.position;
				Vector2 uv2 = uv[i];
				uv2 *= uvRegion.size;
				uv2 += uvRegion.position;
				array[i] = new Vertex
				{
					position = new Vector3(vector.x, vector.y, Vertex.nearZ),
					tint = rectParams.color,
					uv = uv2,
					flags = flags,
					opacityColorPages = opacityColorPages,
					ids = ids
				};
			}
			meshWriteData.SetAllVertices(array);
			meshWriteData.SetAllIndices(array2);
		}
	}

	public void RegisterVectorImageGradient(VectorImage vi, out int settingIndexOffset, out TextureId texture)
	{
		texture = default(TextureId);
		GradientRemap gradientRemap = m_VectorImageManager.AddUser(vi, currentElement);
		settingIndexOffset = gradientRemap.destIndex;
		if (gradientRemap.atlas != TextureId.invalid)
		{
			texture = gradientRemap.atlas;
			return;
		}
		texture = TextureRegistry.instance.Acquire(vi.atlas);
		m_Owner.InsertTexture(currentElement, vi.atlas, texture, isAtlas: false);
	}

	public void DrawVectorImage(MeshGenerationContextUtils.RectangleParams rectParams)
	{
		VectorImage vectorImage = rectParams.vectorImage;
		Debug.Assert(vectorImage != null);
		int settingIndexOffset = 0;
		TextureId textureId = default(TextureId);
		bool flag = vectorImage.atlas != null && m_VectorImageManager != null;
		if (flag)
		{
			GradientRemap gradientRemap = m_VectorImageManager.AddUser(vectorImage, currentElement);
			settingIndexOffset = gradientRemap.destIndex;
			if (gradientRemap.atlas != TextureId.invalid)
			{
				textureId = gradientRemap.atlas;
			}
			else
			{
				textureId = TextureRegistry.instance.Acquire(vectorImage.atlas);
				m_Owner.InsertTexture(currentElement, vectorImage.atlas, textureId, isAtlas: false);
			}
		}
		int count = m_Entries.Count;
		MakeVectorGraphics(rectParams, flag, textureId, settingIndexOffset, out var finalVertexCount, out var finalIndexCount);
		Debug.Assert(count <= m_Entries.Count + 1);
		if (count != m_Entries.Count)
		{
			m_SVGBackgroundEntryIndex = m_Entries.Count - 1;
			if (finalVertexCount != 0 && finalIndexCount != 0)
			{
				Entry value = m_Entries[m_SVGBackgroundEntryIndex];
				value.vertices = value.vertices.Slice(0, finalVertexCount);
				value.indices = value.indices.Slice(0, finalIndexCount);
				m_Entries[m_SVGBackgroundEntryIndex] = value;
			}
		}
	}

	private void MakeVectorGraphics(MeshGenerationContextUtils.RectangleParams rectParams, bool isUsingGradients, TextureId svgTexture, int settingIndexOffset, out int finalVertexCount, out int finalIndexCount)
	{
		VectorImage vectorImage = rectParams.vectorImage;
		Debug.Assert(vectorImage != null);
		finalVertexCount = 0;
		finalIndexCount = 0;
		int num = vectorImage.vertices.Length;
		Vertex[] array = new Vertex[num];
		for (int i = 0; i < num; i++)
		{
			VectorImageVertex vectorImageVertex = vectorImage.vertices[i];
			array[i] = new Vertex
			{
				position = vectorImageVertex.position,
				tint = vectorImageVertex.tint,
				uv = vectorImageVertex.uv,
				settingIndex = new Color32((byte)(vectorImageVertex.settingIndex >> 8), (byte)vectorImageVertex.settingIndex, 0, 0),
				flags = vectorImageVertex.flags,
				circle = vectorImageVertex.circle
			};
		}
		MeshWriteDataInterface meshData = ((!((float)rectParams.leftSlice <= 1E-30f) || !((float)rectParams.topSlice <= 1E-30f) || !((float)rectParams.rightSlice <= 1E-30f) || !((float)rectParams.bottomSlice <= 1E-30f)) ? MeshBuilderNative.MakeVectorGraphics9SliceBackground(sliceLTRB: new Vector4(rectParams.leftSlice, rectParams.topSlice, rectParams.rightSlice, rectParams.bottomSlice), svgVertices: array, svgIndices: vectorImage.indices, svgWidth: vectorImage.size.x, svgHeight: vectorImage.size.y, targetRect: rectParams.rect, tint: rectParams.color, colorPage: rectParams.colorPage.ToNativeColorPage(), settingIndexOffset: settingIndexOffset) : MeshBuilderNative.MakeVectorGraphicsStretchBackground(array, vectorImage.indices, vectorImage.size.x, vectorImage.size.y, rectParams.rect, rectParams.uv, rectParams.scaleMode, rectParams.color, rectParams.colorPage.ToNativeColorPage(), settingIndexOffset, ref finalVertexCount, ref finalIndexCount));
		if (isUsingGradients)
		{
			BuildGradientEntryFromNativeMesh(meshData, svgTexture);
		}
		else
		{
			BuildEntryFromNativeMesh(meshData, null, default(TextureId), isAtlas: false, null, MeshGenerationContext.MeshFlags.None, new Rect(0f, 0f, 1f, 1f), VertexFlags.IsSolid);
		}
	}

	internal void Reset()
	{
		ValidateMeshWriteData();
		m_Entries.Clear();
		m_ClosingInfo = default(ClosingInfo);
		m_NextMeshWriteDataPoolItem = 0;
		currentElement = null;
		int num = (totalIndices = 0);
		totalVertices = num;
	}

	private void ValidateMeshWriteData()
	{
		for (int i = 0; i < m_NextMeshWriteDataPoolItem; i++)
		{
			MeshWriteData meshWriteData = m_MeshWriteDataPool[i];
			if (meshWriteData.vertexCount > 0 && meshWriteData.currentVertex < meshWriteData.vertexCount)
			{
				Debug.LogError("Not enough vertices written in generateVisualContent callback (asked for " + meshWriteData.vertexCount + " but only wrote " + meshWriteData.currentVertex + ")");
				Vertex nextVertex = meshWriteData.m_Vertices[0];
				while (meshWriteData.currentVertex < meshWriteData.vertexCount)
				{
					meshWriteData.SetNextVertex(nextVertex);
				}
			}
			if (meshWriteData.indexCount > 0 && meshWriteData.currentIndex < meshWriteData.indexCount)
			{
				Debug.LogError("Not enough indices written in generateVisualContent callback (asked for " + meshWriteData.indexCount + " but only wrote " + meshWriteData.currentIndex + ")");
				while (meshWriteData.currentIndex < meshWriteData.indexCount)
				{
					meshWriteData.SetNextIndex(0);
				}
			}
		}
	}

	private void GenerateStencilClipEntryForRoundedRectBackground()
	{
		if (!(currentElement.layout.width <= 1E-30f) && !(currentElement.layout.height <= 1E-30f))
		{
			IResolvedStyle resolvedStyle = currentElement.resolvedStyle;
			MeshGenerationContextUtils.GetVisualElementRadii(currentElement, out var topLeft, out var bottomLeft, out var topRight, out var bottomRight);
			float borderTopWidth = resolvedStyle.borderTopWidth;
			float borderLeftWidth = resolvedStyle.borderLeftWidth;
			float borderBottomWidth = resolvedStyle.borderBottomWidth;
			float borderRightWidth = resolvedStyle.borderRightWidth;
			MeshGenerationContextUtils.RectangleParams rectangleParams = new MeshGenerationContextUtils.RectangleParams
			{
				rect = currentElement.rect,
				color = Color.white,
				topLeftRadius = Vector2.Max(Vector2.zero, topLeft - new Vector2(borderLeftWidth, borderTopWidth)),
				topRightRadius = Vector2.Max(Vector2.zero, topRight - new Vector2(borderRightWidth, borderTopWidth)),
				bottomLeftRadius = Vector2.Max(Vector2.zero, bottomLeft - new Vector2(borderLeftWidth, borderBottomWidth)),
				bottomRightRadius = Vector2.Max(Vector2.zero, bottomRight - new Vector2(borderRightWidth, borderBottomWidth)),
				playmodeTintColor = ((currentElement.panel.contextType == ContextType.Editor) ? UIElementsUtility.editorPlayModeTintColor : Color.white)
			};
			rectangleParams.rect.x += borderLeftWidth;
			rectangleParams.rect.y += borderTopWidth;
			rectangleParams.rect.width -= borderLeftWidth + borderRightWidth;
			rectangleParams.rect.height -= borderTopWidth + borderBottomWidth;
			if (currentElement.computedStyle.unityOverflowClipBox == OverflowClipBox.ContentBox)
			{
				rectangleParams.rect.x += resolvedStyle.paddingLeft;
				rectangleParams.rect.y += resolvedStyle.paddingTop;
				rectangleParams.rect.width -= resolvedStyle.paddingLeft + resolvedStyle.paddingRight;
				rectangleParams.rect.height -= resolvedStyle.paddingTop + resolvedStyle.paddingBottom;
			}
			m_CurrentEntry.clipRectID = m_ClipRectID;
			m_CurrentEntry.stencilRef = m_StencilRef;
			m_CurrentEntry.maskDepth = m_MaskDepth;
			m_CurrentEntry.isClipRegisterEntry = true;
			MeshBuilderNative.NativeRectParams rectParams = rectangleParams.ToNativeParams(new Rect(0f, 0f, 1f, 1f));
			MeshWriteDataInterface meshData = MeshBuilderNative.MakeSolidRect(rectParams, 1f);
			if (meshData.vertexCount > 0 && meshData.indexCount > 0)
			{
				BuildRawEntryFromNativeMesh(meshData);
				m_Entries.Add(m_CurrentEntry);
				totalVertices += m_CurrentEntry.vertices.Length;
				totalIndices += m_CurrentEntry.indices.Length;
				m_ClosingInfo.needsClosing = true;
			}
			m_CurrentEntry = default(Entry);
		}
	}

	private void GenerateStencilClipEntryForSVGBackground()
	{
		if (m_SVGBackgroundEntryIndex != -1)
		{
			Entry entry = m_Entries[m_SVGBackgroundEntryIndex];
			Debug.Assert(entry.vertices.Length > 0);
			Debug.Assert(entry.indices.Length > 0);
			m_CurrentEntry.vertices = entry.vertices;
			m_CurrentEntry.indices = entry.indices;
			m_CurrentEntry.uvIsDisplacement = entry.uvIsDisplacement;
			m_CurrentEntry.clipRectID = m_ClipRectID;
			m_CurrentEntry.stencilRef = m_StencilRef;
			m_CurrentEntry.maskDepth = m_MaskDepth;
			m_CurrentEntry.isClipRegisterEntry = true;
			m_ClosingInfo.needsClosing = true;
			int length = m_CurrentEntry.vertices.Length;
			NativeSlice<Vertex> vertices = m_VertsPool.Alloc(length);
			for (int i = 0; i < length; i++)
			{
				Vertex value = m_CurrentEntry.vertices[i];
				value.position.z = 1f;
				vertices[i] = value;
			}
			m_CurrentEntry.vertices = vertices;
			totalVertices += m_CurrentEntry.vertices.Length;
			totalIndices += m_CurrentEntry.indices.Length;
			m_Entries.Add(m_CurrentEntry);
			m_CurrentEntry = default(Entry);
		}
	}
}
