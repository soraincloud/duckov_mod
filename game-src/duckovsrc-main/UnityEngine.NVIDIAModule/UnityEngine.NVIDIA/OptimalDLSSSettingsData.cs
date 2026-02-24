namespace UnityEngine.NVIDIA;

public readonly struct OptimalDLSSSettingsData
{
	private readonly uint m_OutRenderWidth;

	private readonly uint m_OutRenderHeight;

	private readonly float m_Sharpness;

	private readonly uint m_MaxWidth;

	private readonly uint m_MaxHeight;

	private readonly uint m_MinWidth;

	private readonly uint m_MinHeight;

	public uint outRenderWidth => m_OutRenderWidth;

	public uint outRenderHeight => m_OutRenderHeight;

	public float sharpness => m_Sharpness;

	public uint maxWidth => m_MaxWidth;

	public uint maxHeight => m_MaxHeight;

	public uint minWidth => m_MinWidth;

	public uint minHeight => m_MinHeight;
}
