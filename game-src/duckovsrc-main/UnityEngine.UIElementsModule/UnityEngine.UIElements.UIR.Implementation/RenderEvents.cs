#define UNITY_ASSERTIONS
using System;

namespace UnityEngine.UIElements.UIR.Implementation;

internal static class RenderEvents
{
	private static readonly float VisibilityTreshold = 1E-30f;

	internal static void ProcessOnClippingChanged(RenderChain renderChain, VisualElement ve, uint dirtyID, ref ChainBuilderStats stats)
	{
		bool flag = (ve.renderChainData.dirtiedValues & RenderDataDirtyTypes.ClippingHierarchy) != 0;
		if (flag)
		{
			stats.recursiveClipUpdates++;
		}
		else
		{
			stats.nonRecursiveClipUpdates++;
		}
		DepthFirstOnClippingChanged(renderChain, ve.hierarchy.parent, ve, dirtyID, flag, isRootOfChange: true, isPendingHierarchicalRepaint: false, inheritedClipRectIDChanged: false, inheritedMaskingChanged: false, renderChain.device, ref stats);
	}

	internal static void ProcessOnOpacityChanged(RenderChain renderChain, VisualElement ve, uint dirtyID, ref ChainBuilderStats stats)
	{
		bool hierarchical = (ve.renderChainData.dirtiedValues & RenderDataDirtyTypes.OpacityHierarchy) != 0;
		stats.recursiveOpacityUpdates++;
		DepthFirstOnOpacityChanged(renderChain, (ve.hierarchy.parent != null) ? ve.hierarchy.parent.renderChainData.compositeOpacity : 1f, ve, dirtyID, hierarchical, ref stats);
	}

	internal static void ProcessOnColorChanged(RenderChain renderChain, VisualElement ve, uint dirtyID, ref ChainBuilderStats stats)
	{
		stats.colorUpdates++;
		OnColorChanged(renderChain, ve, dirtyID, ref stats);
	}

	internal static void ProcessOnTransformOrSizeChanged(RenderChain renderChain, VisualElement ve, uint dirtyID, ref ChainBuilderStats stats)
	{
		stats.recursiveTransformUpdates++;
		DepthFirstOnTransformOrSizeChanged(renderChain, ve.hierarchy.parent, ve, dirtyID, renderChain.device, isAncestorOfChangeSkinned: false, transformChanged: false, ref stats);
	}

	internal static void ProcessOnVisualsChanged(RenderChain renderChain, VisualElement ve, uint dirtyID, ref ChainBuilderStats stats)
	{
		bool flag = (ve.renderChainData.dirtiedValues & RenderDataDirtyTypes.VisualsHierarchy) != 0;
		if (flag)
		{
			stats.recursiveVisualUpdates++;
		}
		else
		{
			stats.nonRecursiveVisualUpdates++;
		}
		VisualElement parent = ve.hierarchy.parent;
		bool parentHierarchyHidden = parent != null && (parent.renderChainData.isHierarchyHidden || IsElementHierarchyHidden(parent));
		DepthFirstOnVisualsChanged(renderChain, ve, dirtyID, parentHierarchyHidden, flag, ref stats);
	}

	private static Matrix4x4 GetTransformIDTransformInfo(VisualElement ve)
	{
		Debug.Assert(RenderChainVEData.AllocatesID(ve.renderChainData.transformID) || (ve.renderHints & RenderHints.GroupTransform) != 0);
		Matrix4x4 res;
		if (ve.renderChainData.groupTransformAncestor != null)
		{
			VisualElement.MultiplyMatrix34(ref ve.renderChainData.groupTransformAncestor.worldTransformInverse, ref ve.worldTransformRef, out res);
		}
		else
		{
			res = ve.worldTransform;
		}
		res.m22 = 1f;
		return res;
	}

	private static Vector4 GetClipRectIDClipInfo(VisualElement ve)
	{
		Debug.Assert(RenderChainVEData.AllocatesID(ve.renderChainData.clipRectID));
		Rect rect;
		if (ve.renderChainData.groupTransformAncestor == null)
		{
			rect = ve.worldClip;
		}
		else
		{
			rect = ve.worldClipMinusGroup;
			VisualElement.TransformAlignedRect(ref ve.renderChainData.groupTransformAncestor.worldTransformInverse, ref rect);
		}
		Vector2 min = rect.min;
		Vector2 max = rect.max;
		Vector2 vector = max - min;
		Vector2 vector2 = new Vector2(1f / (vector.x + 0.0001f), 1f / (vector.y + 0.0001f));
		Vector2 vector3 = 2f * vector2;
		Vector2 vector4 = -(min + max) * vector2;
		return new Vector4(vector3.x, vector3.y, vector4.x, vector4.y);
	}

	internal static uint DepthFirstOnChildAdded(RenderChain renderChain, VisualElement parent, VisualElement ve, int index, bool resetState)
	{
		Debug.Assert(ve.panel != null);
		if (ve.renderChainData.isInChain)
		{
			return 0u;
		}
		if (resetState)
		{
			ve.renderChainData = default(RenderChainVEData);
		}
		ve.renderChainData.isInChain = true;
		ve.renderChainData.verticesSpace = Matrix4x4.identity;
		ve.renderChainData.transformID = UIRVEShaderInfoAllocator.identityTransform;
		ve.renderChainData.clipRectID = UIRVEShaderInfoAllocator.infiniteClipRect;
		ve.renderChainData.opacityID = UIRVEShaderInfoAllocator.fullOpacity;
		ve.renderChainData.colorID = BMPAlloc.Invalid;
		ve.renderChainData.backgroundColorID = BMPAlloc.Invalid;
		ve.renderChainData.borderLeftColorID = BMPAlloc.Invalid;
		ve.renderChainData.borderTopColorID = BMPAlloc.Invalid;
		ve.renderChainData.borderRightColorID = BMPAlloc.Invalid;
		ve.renderChainData.borderBottomColorID = BMPAlloc.Invalid;
		ve.renderChainData.tintColorID = BMPAlloc.Invalid;
		ve.renderChainData.textCoreSettingsID = UIRVEShaderInfoAllocator.defaultTextCoreSettings;
		ve.renderChainData.compositeOpacity = float.MaxValue;
		UpdateLocalFlipsWinding(ve);
		if (parent != null)
		{
			if ((parent.renderHints & RenderHints.GroupTransform) != RenderHints.None)
			{
				ve.renderChainData.groupTransformAncestor = parent;
			}
			else
			{
				ve.renderChainData.groupTransformAncestor = parent.renderChainData.groupTransformAncestor;
			}
			ve.renderChainData.hierarchyDepth = parent.renderChainData.hierarchyDepth + 1;
		}
		else
		{
			ve.renderChainData.groupTransformAncestor = null;
			ve.renderChainData.hierarchyDepth = 0;
		}
		renderChain.EnsureFitsDepth(ve.renderChainData.hierarchyDepth);
		if (index > 0)
		{
			Debug.Assert(parent != null);
			ve.renderChainData.prev = GetLastDeepestChild(parent.hierarchy[index - 1]);
		}
		else
		{
			ve.renderChainData.prev = parent;
		}
		ve.renderChainData.next = ((ve.renderChainData.prev != null) ? ve.renderChainData.prev.renderChainData.next : null);
		if (ve.renderChainData.prev != null)
		{
			ve.renderChainData.prev.renderChainData.next = ve;
		}
		if (ve.renderChainData.next != null)
		{
			ve.renderChainData.next.renderChainData.prev = ve;
		}
		Debug.Assert(!RenderChainVEData.AllocatesID(ve.renderChainData.transformID));
		if (NeedsTransformID(ve))
		{
			ve.renderChainData.transformID = renderChain.shaderInfoAllocator.AllocTransform();
		}
		else
		{
			ve.renderChainData.transformID = BMPAlloc.Invalid;
		}
		ve.renderChainData.boneTransformAncestor = null;
		if (NeedsColorID(ve))
		{
			InitColorIDs(renderChain, ve);
			SetColorValues(renderChain, ve);
		}
		if (!RenderChainVEData.AllocatesID(ve.renderChainData.transformID))
		{
			if (parent != null && (ve.renderHints & RenderHints.GroupTransform) == 0)
			{
				if (RenderChainVEData.AllocatesID(parent.renderChainData.transformID))
				{
					ve.renderChainData.boneTransformAncestor = parent;
				}
				else
				{
					ve.renderChainData.boneTransformAncestor = parent.renderChainData.boneTransformAncestor;
				}
				ve.renderChainData.transformID = parent.renderChainData.transformID;
				ve.renderChainData.transformID.ownedState = OwnedState.Inherited;
			}
			else
			{
				ve.renderChainData.transformID = UIRVEShaderInfoAllocator.identityTransform;
			}
		}
		else
		{
			renderChain.shaderInfoAllocator.SetTransformValue(ve.renderChainData.transformID, GetTransformIDTransformInfo(ve));
		}
		int childCount = ve.hierarchy.childCount;
		uint num = 0u;
		for (int i = 0; i < childCount; i++)
		{
			num += DepthFirstOnChildAdded(renderChain, ve, ve.hierarchy[i], i, resetState);
		}
		return 1 + num;
	}

	internal static uint DepthFirstOnChildRemoving(RenderChain renderChain, VisualElement ve)
	{
		int num = ve.hierarchy.childCount - 1;
		uint num2 = 0u;
		while (num >= 0)
		{
			num2 += DepthFirstOnChildRemoving(renderChain, ve.hierarchy[num--]);
		}
		if (ve.renderChainData.isInChain)
		{
			renderChain.ChildWillBeRemoved(ve);
			CommandGenerator.ResetCommands(renderChain, ve);
			renderChain.ResetTextures(ve);
			ve.renderChainData.isInChain = false;
			ve.renderChainData.clipMethod = ClipMethod.Undetermined;
			if (ve.renderChainData.next != null)
			{
				ve.renderChainData.next.renderChainData.prev = ve.renderChainData.prev;
			}
			if (ve.renderChainData.prev != null)
			{
				ve.renderChainData.prev.renderChainData.next = ve.renderChainData.next;
			}
			if (RenderChainVEData.AllocatesID(ve.renderChainData.textCoreSettingsID))
			{
				renderChain.shaderInfoAllocator.FreeTextCoreSettings(ve.renderChainData.textCoreSettingsID);
				ve.renderChainData.textCoreSettingsID = UIRVEShaderInfoAllocator.defaultTextCoreSettings;
			}
			if (RenderChainVEData.AllocatesID(ve.renderChainData.opacityID))
			{
				renderChain.shaderInfoAllocator.FreeOpacity(ve.renderChainData.opacityID);
				ve.renderChainData.opacityID = UIRVEShaderInfoAllocator.fullOpacity;
			}
			if (RenderChainVEData.AllocatesID(ve.renderChainData.colorID))
			{
				renderChain.shaderInfoAllocator.FreeColor(ve.renderChainData.colorID);
				ve.renderChainData.colorID = BMPAlloc.Invalid;
			}
			if (RenderChainVEData.AllocatesID(ve.renderChainData.backgroundColorID))
			{
				renderChain.shaderInfoAllocator.FreeColor(ve.renderChainData.backgroundColorID);
				ve.renderChainData.backgroundColorID = BMPAlloc.Invalid;
			}
			if (RenderChainVEData.AllocatesID(ve.renderChainData.borderLeftColorID))
			{
				renderChain.shaderInfoAllocator.FreeColor(ve.renderChainData.borderLeftColorID);
				ve.renderChainData.borderLeftColorID = BMPAlloc.Invalid;
			}
			if (RenderChainVEData.AllocatesID(ve.renderChainData.borderTopColorID))
			{
				renderChain.shaderInfoAllocator.FreeColor(ve.renderChainData.borderTopColorID);
				ve.renderChainData.borderTopColorID = BMPAlloc.Invalid;
			}
			if (RenderChainVEData.AllocatesID(ve.renderChainData.borderRightColorID))
			{
				renderChain.shaderInfoAllocator.FreeColor(ve.renderChainData.borderRightColorID);
				ve.renderChainData.borderRightColorID = BMPAlloc.Invalid;
			}
			if (RenderChainVEData.AllocatesID(ve.renderChainData.borderBottomColorID))
			{
				renderChain.shaderInfoAllocator.FreeColor(ve.renderChainData.borderBottomColorID);
				ve.renderChainData.borderBottomColorID = BMPAlloc.Invalid;
			}
			if (RenderChainVEData.AllocatesID(ve.renderChainData.tintColorID))
			{
				renderChain.shaderInfoAllocator.FreeColor(ve.renderChainData.tintColorID);
				ve.renderChainData.tintColorID = BMPAlloc.Invalid;
			}
			if (RenderChainVEData.AllocatesID(ve.renderChainData.clipRectID))
			{
				renderChain.shaderInfoAllocator.FreeClipRect(ve.renderChainData.clipRectID);
				ve.renderChainData.clipRectID = UIRVEShaderInfoAllocator.infiniteClipRect;
			}
			if (RenderChainVEData.AllocatesID(ve.renderChainData.transformID))
			{
				renderChain.shaderInfoAllocator.FreeTransform(ve.renderChainData.transformID);
				ve.renderChainData.transformID = UIRVEShaderInfoAllocator.identityTransform;
			}
			ve.renderChainData.boneTransformAncestor = (ve.renderChainData.groupTransformAncestor = null);
			if (ve.renderChainData.closingData != null)
			{
				renderChain.device.Free(ve.renderChainData.closingData);
				ve.renderChainData.closingData = null;
			}
			if (ve.renderChainData.data != null)
			{
				renderChain.device.Free(ve.renderChainData.data);
				ve.renderChainData.data = null;
			}
		}
		ve.renderChainData.prev = null;
		ve.renderChainData.next = null;
		return num2 + 1;
	}

	private static void DepthFirstOnClippingChanged(RenderChain renderChain, VisualElement parent, VisualElement ve, uint dirtyID, bool hierarchical, bool isRootOfChange, bool isPendingHierarchicalRepaint, bool inheritedClipRectIDChanged, bool inheritedMaskingChanged, UIRenderDevice device, ref ChainBuilderStats stats)
	{
		if (dirtyID == ve.renderChainData.dirtyID && !inheritedClipRectIDChanged && !inheritedMaskingChanged)
		{
			return;
		}
		ve.renderChainData.dirtyID = dirtyID;
		if (!isRootOfChange)
		{
			stats.recursiveClipUpdatesExpanded++;
		}
		isPendingHierarchicalRepaint |= (ve.renderChainData.dirtiedValues & RenderDataDirtyTypes.VisualsHierarchy) != 0;
		bool flag = hierarchical || isRootOfChange || inheritedClipRectIDChanged;
		bool flag2 = hierarchical || isRootOfChange;
		bool flag3 = hierarchical || isRootOfChange || inheritedMaskingChanged;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = hierarchical;
		ClipMethod clipMethod = ve.renderChainData.clipMethod;
		ClipMethod clipMethod2 = (flag2 ? DetermineSelfClipMethod(renderChain, ve) : clipMethod);
		bool flag8 = false;
		if (flag)
		{
			BMPAlloc bMPAlloc = ve.renderChainData.clipRectID;
			if (clipMethod2 == ClipMethod.ShaderDiscard)
			{
				if (!RenderChainVEData.AllocatesID(ve.renderChainData.clipRectID))
				{
					bMPAlloc = renderChain.shaderInfoAllocator.AllocClipRect();
					if (!bMPAlloc.IsValid())
					{
						clipMethod2 = ClipMethod.Scissor;
						bMPAlloc = UIRVEShaderInfoAllocator.infiniteClipRect;
					}
				}
			}
			else
			{
				if (RenderChainVEData.AllocatesID(ve.renderChainData.clipRectID))
				{
					renderChain.shaderInfoAllocator.FreeClipRect(ve.renderChainData.clipRectID);
				}
				if ((ve.renderHints & RenderHints.GroupTransform) == 0)
				{
					bMPAlloc = ((clipMethod2 != ClipMethod.Scissor && parent != null) ? parent.renderChainData.clipRectID : UIRVEShaderInfoAllocator.infiniteClipRect);
					bMPAlloc.ownedState = OwnedState.Inherited;
				}
			}
			flag8 = !ve.renderChainData.clipRectID.Equals(bMPAlloc);
			Debug.Assert((ve.renderHints & RenderHints.GroupTransform) == 0 || !flag8);
			ve.renderChainData.clipRectID = bMPAlloc;
		}
		bool flag9 = false;
		if (clipMethod != clipMethod2)
		{
			ve.renderChainData.clipMethod = clipMethod2;
			if (clipMethod == ClipMethod.Stencil || clipMethod2 == ClipMethod.Stencil)
			{
				flag9 = true;
				flag3 = true;
			}
			if (clipMethod == ClipMethod.Scissor || clipMethod2 == ClipMethod.Scissor)
			{
				flag4 = true;
			}
			if (clipMethod2 == ClipMethod.ShaderDiscard || (clipMethod == ClipMethod.ShaderDiscard && RenderChainVEData.AllocatesID(ve.renderChainData.clipRectID)))
			{
				flag6 = true;
			}
		}
		if (flag8)
		{
			flag7 = true;
			flag5 = true;
		}
		if (flag3)
		{
			int num = 0;
			int num2 = 0;
			if (parent != null)
			{
				num = parent.renderChainData.childrenMaskDepth;
				num2 = parent.renderChainData.childrenStencilRef;
				if (clipMethod2 == ClipMethod.Stencil)
				{
					if (num > num2)
					{
						num2++;
					}
					num++;
				}
				if ((ve.renderHints & RenderHints.MaskContainer) == RenderHints.MaskContainer && num < 7)
				{
					num2 = num;
				}
			}
			if (ve.renderChainData.childrenMaskDepth != num || ve.renderChainData.childrenStencilRef != num2)
			{
				flag9 = true;
			}
			ve.renderChainData.childrenMaskDepth = num;
			ve.renderChainData.childrenStencilRef = num2;
		}
		if (flag9)
		{
			flag7 = true;
			flag5 = true;
		}
		if ((flag4 || flag5) && !isPendingHierarchicalRepaint)
		{
			renderChain.UIEOnVisualsChanged(ve, flag5);
			isPendingHierarchicalRepaint = true;
		}
		if (flag6)
		{
			renderChain.UIEOnTransformOrSizeChanged(ve, transformChanged: false, clipRectSizeChanged: true);
		}
		if (flag7)
		{
			int childCount = ve.hierarchy.childCount;
			for (int i = 0; i < childCount; i++)
			{
				DepthFirstOnClippingChanged(renderChain, ve, ve.hierarchy[i], dirtyID, hierarchical, isRootOfChange: false, isPendingHierarchicalRepaint, flag8, flag9, device, ref stats);
			}
		}
	}

	private static void DepthFirstOnOpacityChanged(RenderChain renderChain, float parentCompositeOpacity, VisualElement ve, uint dirtyID, bool hierarchical, ref ChainBuilderStats stats, bool isDoingFullVertexRegeneration = false)
	{
		if (dirtyID == ve.renderChainData.dirtyID)
		{
			return;
		}
		ve.renderChainData.dirtyID = dirtyID;
		stats.recursiveOpacityUpdatesExpanded++;
		float compositeOpacity = ve.renderChainData.compositeOpacity;
		float num = ve.resolvedStyle.opacity * parentCompositeOpacity;
		bool flag = (compositeOpacity < VisibilityTreshold) ^ (num < VisibilityTreshold);
		bool flag2 = Mathf.Abs(compositeOpacity - num) > 0.0001f || flag;
		if (flag2)
		{
			ve.renderChainData.compositeOpacity = num;
		}
		bool flag3 = false;
		if (num < parentCompositeOpacity - 0.0001f)
		{
			if (ve.renderChainData.opacityID.ownedState == OwnedState.Inherited)
			{
				flag3 = true;
				ve.renderChainData.opacityID = renderChain.shaderInfoAllocator.AllocOpacity();
			}
			if ((flag3 || flag2) && ve.renderChainData.opacityID.IsValid())
			{
				renderChain.shaderInfoAllocator.SetOpacityValue(ve.renderChainData.opacityID, num);
			}
		}
		else if (ve.renderChainData.opacityID.ownedState == OwnedState.Inherited)
		{
			if (ve.hierarchy.parent != null && !ve.renderChainData.opacityID.Equals(ve.hierarchy.parent.renderChainData.opacityID))
			{
				flag3 = true;
				ve.renderChainData.opacityID = ve.hierarchy.parent.renderChainData.opacityID;
				ve.renderChainData.opacityID.ownedState = OwnedState.Inherited;
			}
		}
		else if (flag2 && ve.renderChainData.opacityID.IsValid())
		{
			renderChain.shaderInfoAllocator.SetOpacityValue(ve.renderChainData.opacityID, num);
		}
		if (!isDoingFullVertexRegeneration && flag3 && (ve.renderChainData.dirtiedValues & RenderDataDirtyTypes.Visuals) == 0 && (ve.renderChainData.data != null || ve.renderChainData.closingData != null))
		{
			renderChain.UIEOnOpacityIdChanged(ve);
		}
		if (flag2 || flag3 || hierarchical)
		{
			int childCount = ve.hierarchy.childCount;
			for (int i = 0; i < childCount; i++)
			{
				DepthFirstOnOpacityChanged(renderChain, num, ve.hierarchy[i], dirtyID, hierarchical, ref stats, isDoingFullVertexRegeneration);
			}
		}
	}

	private static void OnColorChanged(RenderChain renderChain, VisualElement ve, uint dirtyID, ref ChainBuilderStats stats)
	{
		if (dirtyID == ve.renderChainData.dirtyID)
		{
			return;
		}
		ve.renderChainData.dirtyID = dirtyID;
		stats.colorUpdatesExpanded++;
		Color backgroundColor = ve.resolvedStyle.backgroundColor;
		if (ve.renderChainData.backgroundColor.a == 0f && backgroundColor.a > 0f)
		{
			renderChain.UIEOnVisualsChanged(ve, hierarchical: false);
		}
		ve.renderChainData.backgroundColor = backgroundColor;
		bool flag = false;
		if ((ve.renderHints & RenderHints.DynamicColor) == RenderHints.DynamicColor && !ve.renderChainData.isIgnoringDynamicColorHint)
		{
			if (InitColorIDs(renderChain, ve))
			{
				flag = true;
			}
			SetColorValues(renderChain, ve);
			if (ve is TextElement && !UpdateTextCoreSettings(renderChain, ve))
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			renderChain.UIEOnVisualsChanged(ve, hierarchical: false);
		}
	}

	private static void DepthFirstOnTransformOrSizeChanged(RenderChain renderChain, VisualElement parent, VisualElement ve, uint dirtyID, UIRenderDevice device, bool isAncestorOfChangeSkinned, bool transformChanged, ref ChainBuilderStats stats)
	{
		if (dirtyID == ve.renderChainData.dirtyID)
		{
			return;
		}
		stats.recursiveTransformUpdatesExpanded++;
		transformChanged |= (ve.renderChainData.dirtiedValues & RenderDataDirtyTypes.Transform) != 0;
		if (RenderChainVEData.AllocatesID(ve.renderChainData.clipRectID))
		{
			renderChain.shaderInfoAllocator.SetClipRectValue(ve.renderChainData.clipRectID, GetClipRectIDClipInfo(ve));
		}
		if (transformChanged && UpdateLocalFlipsWinding(ve))
		{
			renderChain.UIEOnVisualsChanged(ve, hierarchical: true);
		}
		if (transformChanged)
		{
			UpdateZeroScaling(ve);
		}
		bool flag = true;
		if (RenderChainVEData.AllocatesID(ve.renderChainData.transformID))
		{
			renderChain.shaderInfoAllocator.SetTransformValue(ve.renderChainData.transformID, GetTransformIDTransformInfo(ve));
			isAncestorOfChangeSkinned = true;
			stats.boneTransformed++;
		}
		else if (transformChanged)
		{
			if ((ve.renderHints & RenderHints.GroupTransform) != RenderHints.None)
			{
				stats.groupTransformElementsChanged++;
			}
			else if (isAncestorOfChangeSkinned)
			{
				Debug.Assert(RenderChainVEData.InheritsID(ve.renderChainData.transformID));
				flag = false;
				stats.skipTransformed++;
			}
			else if ((ve.renderChainData.dirtiedValues & (RenderDataDirtyTypes.Visuals | RenderDataDirtyTypes.VisualsHierarchy)) == 0 && ve.renderChainData.data != null)
			{
				if (!ve.renderChainData.disableNudging && CommandGenerator.NudgeVerticesToNewSpace(ve, renderChain, device))
				{
					stats.nudgeTransformed++;
				}
				else
				{
					renderChain.UIEOnVisualsChanged(ve, hierarchical: false);
					stats.visualUpdateTransformed++;
				}
			}
		}
		if (flag)
		{
			ve.renderChainData.dirtyID = dirtyID;
		}
		if (renderChain.drawInCameras)
		{
			ve.EnsureWorldTransformAndClipUpToDate();
		}
		if ((ve.renderHints & RenderHints.GroupTransform) == 0)
		{
			int childCount = ve.hierarchy.childCount;
			for (int i = 0; i < childCount; i++)
			{
				DepthFirstOnTransformOrSizeChanged(renderChain, ve, ve.hierarchy[i], dirtyID, device, isAncestorOfChangeSkinned, transformChanged, ref stats);
			}
		}
	}

	private static void DepthFirstOnVisualsChanged(RenderChain renderChain, VisualElement ve, uint dirtyID, bool parentHierarchyHidden, bool hierarchical, ref ChainBuilderStats stats)
	{
		if (dirtyID == ve.renderChainData.dirtyID)
		{
			return;
		}
		ve.renderChainData.dirtyID = dirtyID;
		if (hierarchical)
		{
			stats.recursiveVisualUpdatesExpanded++;
		}
		bool isHierarchyHidden = ve.renderChainData.isHierarchyHidden;
		ve.renderChainData.isHierarchyHidden = parentHierarchyHidden || IsElementHierarchyHidden(ve);
		if (isHierarchyHidden != ve.renderChainData.isHierarchyHidden)
		{
			hierarchical = true;
		}
		if (!hierarchical && (ve.renderChainData.dirtiedValues & RenderDataDirtyTypes.AllVisuals) == RenderDataDirtyTypes.VisualsOpacityId)
		{
			stats.opacityIdUpdates++;
			CommandGenerator.UpdateOpacityId(ve, renderChain);
			return;
		}
		UpdateWorldFlipsWinding(ve);
		Debug.Assert(ve.renderChainData.clipMethod != ClipMethod.Undetermined);
		Debug.Assert(RenderChainVEData.AllocatesID(ve.renderChainData.transformID) || ve.hierarchy.parent == null || ve.renderChainData.transformID.Equals(ve.hierarchy.parent.renderChainData.transformID) || (ve.renderHints & RenderHints.GroupTransform) != 0);
		if (ve is TextElement)
		{
			UpdateTextCoreSettings(renderChain, ve);
		}
		if ((ve.renderHints & RenderHints.DynamicColor) == RenderHints.DynamicColor)
		{
			SetColorValues(renderChain, ve);
		}
		UIRStylePainter.ClosingInfo closingInfo = CommandGenerator.PaintElement(renderChain, ve, ref stats);
		if (hierarchical)
		{
			int childCount = ve.hierarchy.childCount;
			for (int i = 0; i < childCount; i++)
			{
				DepthFirstOnVisualsChanged(renderChain, ve.hierarchy[i], dirtyID, ve.renderChainData.isHierarchyHidden, hierarchical: true, ref stats);
			}
		}
		if (closingInfo.needsClosing)
		{
			CommandGenerator.ClosePaintElement(ve, closingInfo, renderChain, ref stats);
		}
	}

	private static bool UpdateTextCoreSettings(RenderChain renderChain, VisualElement ve)
	{
		if (ve == null || !TextUtilities.IsFontAssigned(ve))
		{
			return false;
		}
		bool flag = RenderChainVEData.AllocatesID(ve.renderChainData.textCoreSettingsID);
		TextCoreSettings textCoreSettingsForElement = TextUtilities.GetTextCoreSettingsForElement(ve);
		if (!NeedsColorID(ve) && !NeedsTextCoreSettings(ve) && !flag)
		{
			ve.renderChainData.textCoreSettingsID = UIRVEShaderInfoAllocator.defaultTextCoreSettings;
			return true;
		}
		if (!flag)
		{
			ve.renderChainData.textCoreSettingsID = renderChain.shaderInfoAllocator.AllocTextCoreSettings(textCoreSettingsForElement);
		}
		if (RenderChainVEData.AllocatesID(ve.renderChainData.textCoreSettingsID))
		{
			if (ve.panel.contextType == ContextType.Editor)
			{
				textCoreSettingsForElement.faceColor *= UIElementsUtility.editorPlayModeTintColor;
				textCoreSettingsForElement.outlineColor *= UIElementsUtility.editorPlayModeTintColor;
				textCoreSettingsForElement.underlayColor *= UIElementsUtility.editorPlayModeTintColor;
			}
			renderChain.shaderInfoAllocator.SetTextCoreSettingValue(ve.renderChainData.textCoreSettingsID, textCoreSettingsForElement, ve.panel.contextType == ContextType.Editor);
		}
		return true;
	}

	private static bool IsElementHierarchyHidden(VisualElement ve)
	{
		return ve.resolvedStyle.display == DisplayStyle.None;
	}

	private static VisualElement GetLastDeepestChild(VisualElement ve)
	{
		for (int childCount = ve.hierarchy.childCount; childCount > 0; childCount = ve.hierarchy.childCount)
		{
			ve = ve.hierarchy[childCount - 1];
		}
		return ve;
	}

	private static VisualElement GetNextDepthFirst(VisualElement ve)
	{
		for (VisualElement parent = ve.hierarchy.parent; parent != null; parent = parent.hierarchy.parent)
		{
			int num = parent.hierarchy.IndexOf(ve);
			int childCount = parent.hierarchy.childCount;
			if (num < childCount - 1)
			{
				return parent.hierarchy[num + 1];
			}
			ve = parent;
		}
		return null;
	}

	private static ClipMethod DetermineSelfClipMethod(RenderChain renderChain, VisualElement ve)
	{
		if (!ve.ShouldClip())
		{
			return ClipMethod.NotClipped;
		}
		ClipMethod clipMethod = (((ve.renderHints & RenderHints.DirtyOffset) != RenderHints.None) ? ClipMethod.Scissor : ClipMethod.ShaderDiscard);
		if (!UIRUtility.IsRoundRect(ve) && !UIRUtility.IsVectorImageBackground(ve))
		{
			return clipMethod;
		}
		int num = 0;
		VisualElement parent = ve.hierarchy.parent;
		if (parent != null)
		{
			num = parent.renderChainData.childrenMaskDepth;
		}
		if (num == 7)
		{
			return clipMethod;
		}
		return renderChain.drawInCameras ? clipMethod : ClipMethod.Stencil;
	}

	private static bool UpdateLocalFlipsWinding(VisualElement ve)
	{
		bool localFlipsWinding = ve.renderChainData.localFlipsWinding;
		Vector3 scale = ve.transform.scale;
		float num = scale.x * scale.y;
		if (Math.Abs(num) < 0.001f)
		{
			return false;
		}
		bool flag = num < 0f;
		if (localFlipsWinding != flag)
		{
			ve.renderChainData.localFlipsWinding = flag;
			return true;
		}
		return false;
	}

	private static void UpdateWorldFlipsWinding(VisualElement ve)
	{
		bool localFlipsWinding = ve.renderChainData.localFlipsWinding;
		bool flag = false;
		VisualElement parent = ve.hierarchy.parent;
		if (parent != null)
		{
			flag = parent.renderChainData.worldFlipsWinding;
		}
		ve.renderChainData.worldFlipsWinding = flag ^ localFlipsWinding;
	}

	private static void UpdateZeroScaling(VisualElement ve)
	{
		bool flag = Math.Abs(ve.transform.scale.x * ve.transform.scale.y) < 0.001f;
		ve.renderChainData.localTransformScaleZero = flag;
		bool flag2 = false;
		VisualElement parent = ve.hierarchy.parent;
		if (parent != null)
		{
			flag2 = parent.renderChainData.worldTransformScaleZero;
		}
		ve.renderChainData.worldTransformScaleZero = flag2 || flag;
	}

	private static bool NeedsTransformID(VisualElement ve)
	{
		return (ve.renderHints & RenderHints.GroupTransform) == 0 && (ve.renderHints & RenderHints.BoneTransform) == RenderHints.BoneTransform;
	}

	private static bool TransformIDHasChanged(Alloc before, Alloc after)
	{
		if (before.size == 0 && after.size == 0)
		{
			return false;
		}
		if (before.size != after.size || before.start != after.start)
		{
			return true;
		}
		return false;
	}

	internal static bool NeedsColorID(VisualElement ve)
	{
		return (ve.renderHints & RenderHints.DynamicColor) == RenderHints.DynamicColor;
	}

	internal static bool NeedsTextCoreSettings(VisualElement ve)
	{
		TextCoreSettings textCoreSettingsForElement = TextUtilities.GetTextCoreSettingsForElement(ve);
		if (textCoreSettingsForElement.outlineWidth != 0f || textCoreSettingsForElement.underlayOffset != Vector2.zero || textCoreSettingsForElement.underlaySoftness != 0f)
		{
			return true;
		}
		return false;
	}

	private static bool InitColorIDs(RenderChain renderChain, VisualElement ve)
	{
		IResolvedStyle resolvedStyle = ve.resolvedStyle;
		bool result = false;
		if (!ve.renderChainData.colorID.IsValid() && resolvedStyle.color != Color.white)
		{
			ve.renderChainData.colorID = renderChain.shaderInfoAllocator.AllocColor();
			result = true;
		}
		if (!ve.renderChainData.backgroundColorID.IsValid() && resolvedStyle.backgroundColor != Color.clear)
		{
			ve.renderChainData.backgroundColorID = renderChain.shaderInfoAllocator.AllocColor();
			result = true;
		}
		if (!ve.renderChainData.borderLeftColorID.IsValid() && resolvedStyle.borderLeftWidth > 0f)
		{
			ve.renderChainData.borderLeftColorID = renderChain.shaderInfoAllocator.AllocColor();
			result = true;
		}
		if (!ve.renderChainData.borderTopColorID.IsValid() && resolvedStyle.borderTopWidth > 0f)
		{
			ve.renderChainData.borderTopColorID = renderChain.shaderInfoAllocator.AllocColor();
			result = true;
		}
		if (!ve.renderChainData.borderRightColorID.IsValid() && resolvedStyle.borderRightWidth > 0f)
		{
			ve.renderChainData.borderRightColorID = renderChain.shaderInfoAllocator.AllocColor();
			result = true;
		}
		if (!ve.renderChainData.borderBottomColorID.IsValid() && resolvedStyle.borderBottomWidth > 0f)
		{
			ve.renderChainData.borderBottomColorID = renderChain.shaderInfoAllocator.AllocColor();
			result = true;
		}
		if (!ve.renderChainData.tintColorID.IsValid() && resolvedStyle.unityBackgroundImageTintColor != Color.white)
		{
			ve.renderChainData.tintColorID = renderChain.shaderInfoAllocator.AllocColor();
			result = true;
		}
		return result;
	}

	private static void ResetColorIDs(VisualElement ve)
	{
		ve.renderChainData.colorID = BMPAlloc.Invalid;
		ve.renderChainData.backgroundColorID = BMPAlloc.Invalid;
		ve.renderChainData.borderLeftColorID = BMPAlloc.Invalid;
		ve.renderChainData.borderTopColorID = BMPAlloc.Invalid;
		ve.renderChainData.borderRightColorID = BMPAlloc.Invalid;
		ve.renderChainData.borderBottomColorID = BMPAlloc.Invalid;
		ve.renderChainData.tintColorID = BMPAlloc.Invalid;
	}

	private static void SetColorValues(RenderChain renderChain, VisualElement ve)
	{
		bool isEditorContext = ve.panel.contextType == ContextType.Editor;
		IResolvedStyle resolvedStyle = ve.resolvedStyle;
		if (ve.renderChainData.colorID.IsValid())
		{
			renderChain.shaderInfoAllocator.SetColorValue(ve.renderChainData.colorID, resolvedStyle.color, isEditorContext);
		}
		if (ve.renderChainData.backgroundColorID.IsValid())
		{
			renderChain.shaderInfoAllocator.SetColorValue(ve.renderChainData.backgroundColorID, resolvedStyle.backgroundColor, isEditorContext);
		}
		if (ve.renderChainData.borderLeftColorID.IsValid())
		{
			renderChain.shaderInfoAllocator.SetColorValue(ve.renderChainData.borderLeftColorID, resolvedStyle.borderLeftColor, isEditorContext);
		}
		if (ve.renderChainData.borderTopColorID.IsValid())
		{
			renderChain.shaderInfoAllocator.SetColorValue(ve.renderChainData.borderTopColorID, resolvedStyle.borderTopColor, isEditorContext);
		}
		if (ve.renderChainData.borderRightColorID.IsValid())
		{
			renderChain.shaderInfoAllocator.SetColorValue(ve.renderChainData.borderRightColorID, resolvedStyle.borderRightColor, isEditorContext);
		}
		if (ve.renderChainData.borderBottomColorID.IsValid())
		{
			renderChain.shaderInfoAllocator.SetColorValue(ve.renderChainData.borderBottomColorID, resolvedStyle.borderBottomColor, isEditorContext);
		}
		if (ve.renderChainData.tintColorID.IsValid())
		{
			renderChain.shaderInfoAllocator.SetColorValue(ve.renderChainData.tintColorID, resolvedStyle.unityBackgroundImageTintColor, isEditorContext);
		}
	}
}
