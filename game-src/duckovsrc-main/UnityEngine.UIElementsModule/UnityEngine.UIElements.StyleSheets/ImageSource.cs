namespace UnityEngine.UIElements.StyleSheets;

internal struct ImageSource
{
	public Texture2D texture;

	public Sprite sprite;

	public VectorImage vectorImage;

	public RenderTexture renderTexture;

	public bool IsNull()
	{
		return texture == null && sprite == null && vectorImage == null && renderTexture == null;
	}
}
