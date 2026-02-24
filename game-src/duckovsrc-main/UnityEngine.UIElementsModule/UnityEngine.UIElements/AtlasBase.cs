namespace UnityEngine.UIElements;

internal abstract class AtlasBase
{
	internal TextureRegistry textureRegistry = TextureRegistry.instance;

	public virtual bool TryGetAtlas(VisualElement ctx, Texture2D src, out TextureId atlas, out RectInt atlasRect)
	{
		atlas = TextureId.invalid;
		atlasRect = default(RectInt);
		return false;
	}

	public virtual void ReturnAtlas(VisualElement ctx, Texture2D src, TextureId atlas)
	{
	}

	public virtual void Reset()
	{
	}

	protected virtual void OnAssignedToPanel(IPanel panel)
	{
	}

	protected virtual void OnRemovedFromPanel(IPanel panel)
	{
	}

	protected virtual void OnUpdateDynamicTextures(IPanel panel)
	{
	}

	internal void InvokeAssignedToPanel(IPanel panel)
	{
		OnAssignedToPanel(panel);
	}

	internal void InvokeRemovedFromPanel(IPanel panel)
	{
		OnRemovedFromPanel(panel);
	}

	internal void InvokeUpdateDynamicTextures(IPanel panel)
	{
		OnUpdateDynamicTextures(panel);
	}

	protected static void RepaintTexturedElements(IPanel panel)
	{
		if (((panel is Panel panel2) ? panel2.GetUpdater(VisualTreeUpdatePhase.Repaint) : null) is UIRRepaintUpdater uIRRepaintUpdater)
		{
			uIRRepaintUpdater.renderChain?.RepaintTexturedElements();
		}
	}

	protected TextureId AllocateDynamicTexture()
	{
		return textureRegistry.AllocAndAcquireDynamic();
	}

	protected void FreeDynamicTexture(TextureId id)
	{
		textureRegistry.Release(id);
	}

	protected void SetDynamicTexture(TextureId id, Texture texture)
	{
		textureRegistry.UpdateDynamic(id, texture);
	}
}
