namespace UnityEngine.TextCore.Text;

internal struct LinkInfo
{
	public int hashCode;

	public int linkIdFirstCharacterIndex;

	public int linkIdLength;

	public int linkTextfirstCharacterIndex;

	public int linkTextLength;

	internal char[] linkId;

	private string m_LinkIdString;

	private string m_LinkTextString;

	internal void SetLinkId(char[] text, int startIndex, int length)
	{
		if (linkId == null || linkId.Length < length)
		{
			linkId = new char[length];
		}
		for (int i = 0; i < length; i++)
		{
			linkId[i] = text[startIndex + i];
		}
		linkIdLength = length;
		m_LinkIdString = null;
		m_LinkTextString = null;
	}

	public string GetLinkText(TextInfo textInfo)
	{
		if (string.IsNullOrEmpty(m_LinkTextString))
		{
			for (int i = linkTextfirstCharacterIndex; i < linkTextfirstCharacterIndex + linkTextLength; i++)
			{
				m_LinkTextString += textInfo.textElementInfo[i].character;
			}
		}
		return m_LinkTextString;
	}

	public string GetLinkId()
	{
		if (string.IsNullOrEmpty(m_LinkIdString))
		{
			m_LinkIdString = new string(linkId, 0, linkIdLength);
		}
		return m_LinkIdString;
	}
}
