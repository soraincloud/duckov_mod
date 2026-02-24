namespace UnityEngine.VFX;

public struct VFXOutputEventArgs
{
	public int nameId { get; }

	public VFXEventAttribute eventAttribute { get; }

	public VFXOutputEventArgs(int nameId, VFXEventAttribute eventAttribute)
	{
		this.nameId = nameId;
		this.eventAttribute = eventAttribute;
	}
}
