namespace UnityEngine.NVIDIA;

public struct DLSSTextureTable
{
	public Texture colorInput { get; set; }

	public Texture colorOutput { get; set; }

	public Texture depth { get; set; }

	public Texture motionVectors { get; set; }

	public Texture transparencyMask { get; set; }

	public Texture exposureTexture { get; set; }

	public Texture biasColorMask { get; set; }
}
