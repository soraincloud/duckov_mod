using System;

namespace UnityEngine.UIElements;

[Serializable]
public class VectorImage : ScriptableObject
{
	[SerializeField]
	internal int version = 0;

	[SerializeField]
	internal Texture2D atlas = null;

	[SerializeField]
	internal VectorImageVertex[] vertices = null;

	[SerializeField]
	internal ushort[] indices = null;

	[SerializeField]
	internal GradientSettings[] settings = null;

	[SerializeField]
	internal Vector2 size = Vector2.zero;

	public float width => size.x;

	public float height => size.y;
}
