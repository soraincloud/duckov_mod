namespace UnityEngine.TextCore.Text;

internal struct CharacterSubstitution
{
	public int index;

	public uint unicode;

	public CharacterSubstitution(int index, uint unicode)
	{
		this.index = index;
		this.unicode = unicode;
	}
}
