namespace UnityEngine.TextCore.Text;

internal struct CharacterElement
{
	private uint m_Unicode;

	private TextElement m_TextElement;

	public uint Unicode
	{
		get
		{
			return m_Unicode;
		}
		set
		{
			m_Unicode = value;
		}
	}

	public CharacterElement(TextElement textElement)
	{
		m_Unicode = textElement.unicode;
		m_TextElement = textElement;
	}
}
