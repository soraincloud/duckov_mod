using System;
using System.Text;

namespace UnityEngine.TextCore.Text;

internal class TextHandle
{
	private Vector2 m_PreferredSize;

	private TextInfo m_TextInfo;

	private static TextInfo m_LayoutTextInfo;

	private int m_PreviousGenerationSettingsHash;

	protected TextGenerationSettings textGenerationSettings;

	protected static TextGenerationSettings s_LayoutSettings = new TextGenerationSettings();

	private bool isDirty;

	internal TextInfo textInfo
	{
		get
		{
			if (m_TextInfo == null)
			{
				m_TextInfo = new TextInfo();
			}
			return m_TextInfo;
		}
	}

	internal static TextInfo layoutTextInfo
	{
		get
		{
			if (m_LayoutTextInfo == null)
			{
				m_LayoutTextInfo = new TextInfo();
			}
			return m_LayoutTextInfo;
		}
	}

	public TextHandle()
	{
		textGenerationSettings = new TextGenerationSettings();
	}

	internal bool IsTextInfoAllocated()
	{
		return m_TextInfo != null;
	}

	public void SetDirty()
	{
		isDirty = true;
	}

	public bool IsDirty()
	{
		int hashCode = textGenerationSettings.GetHashCode();
		if (m_PreviousGenerationSettingsHash == hashCode && !isDirty)
		{
			return false;
		}
		m_PreviousGenerationSettingsHash = hashCode;
		isDirty = false;
		return true;
	}

	public Vector2 GetCursorPositionFromStringIndexUsingCharacterHeight(int index, bool inverseYAxis = true)
	{
		if (textGenerationSettings == null)
		{
			return Vector2.zero;
		}
		Rect screenRect = textGenerationSettings.screenRect;
		Vector2 position = screenRect.position;
		if (textInfo.characterCount == 0)
		{
			return position;
		}
		int num = ((index >= textInfo.characterCount) ? (textInfo.characterCount - 1) : index);
		TextElementInfo textElementInfo = textInfo.textElementInfo[num];
		float descender = textElementInfo.descender;
		float x = ((index >= textInfo.characterCount) ? textElementInfo.xAdvance : textElementInfo.origin);
		return position + (inverseYAxis ? new Vector2(x, screenRect.height - descender) : new Vector2(x, descender));
	}

	public Vector2 GetCursorPositionFromStringIndexUsingLineHeight(int index, bool useXAdvance = false, bool inverseYAxis = true)
	{
		if (textGenerationSettings == null)
		{
			return Vector2.zero;
		}
		Rect screenRect = textGenerationSettings.screenRect;
		Vector2 position = screenRect.position;
		if (textInfo.characterCount == 0)
		{
			return position;
		}
		if (index >= textInfo.characterCount)
		{
			index = textInfo.characterCount - 1;
		}
		TextElementInfo textElementInfo = textInfo.textElementInfo[index];
		LineInfo lineInfo = textInfo.lineInfo[textElementInfo.lineNumber];
		if (index >= textInfo.characterCount - 1 || useXAdvance)
		{
			return position + (inverseYAxis ? new Vector2(textElementInfo.xAdvance, screenRect.height - lineInfo.descender) : new Vector2(textElementInfo.xAdvance, lineInfo.descender));
		}
		return position + (inverseYAxis ? new Vector2(textElementInfo.origin, screenRect.height - lineInfo.descender) : new Vector2(textElementInfo.origin, lineInfo.descender));
	}

	public int GetCursorIndexFromPosition(Vector2 position, bool inverseYAxis = true)
	{
		if (textGenerationSettings == null)
		{
			return 0;
		}
		if (inverseYAxis)
		{
			position.y = textGenerationSettings.screenRect.height - position.y;
		}
		int line = 0;
		if (textInfo.lineCount > 1)
		{
			line = FindNearestLine(position);
		}
		int num = FindNearestCharacterOnLine(position, line, visibleOnly: false);
		TextElementInfo textElementInfo = textInfo.textElementInfo[num];
		Vector3 bottomLeft = textElementInfo.bottomLeft;
		Vector3 topRight = textElementInfo.topRight;
		float num2 = (position.x - bottomLeft.x) / (topRight.x - bottomLeft.x);
		return (num2 < 0.5f || textElementInfo.character == '\n') ? num : (num + 1);
	}

	public int LineDownCharacterPosition(int originalPos)
	{
		if (originalPos >= textInfo.characterCount)
		{
			return textInfo.characterCount - 1;
		}
		TextElementInfo textElementInfo = textInfo.textElementInfo[originalPos];
		int lineNumber = textElementInfo.lineNumber;
		if (lineNumber + 1 >= textInfo.lineCount)
		{
			return textInfo.characterCount - 1;
		}
		int lastCharacterIndex = textInfo.lineInfo[lineNumber + 1].lastCharacterIndex;
		int num = -1;
		float num2 = float.PositiveInfinity;
		float num3 = 0f;
		for (int i = textInfo.lineInfo[lineNumber + 1].firstCharacterIndex; i < lastCharacterIndex; i++)
		{
			TextElementInfo textElementInfo2 = textInfo.textElementInfo[i];
			float num4 = textElementInfo.origin - textElementInfo2.origin;
			float num5 = num4 / (textElementInfo2.xAdvance - textElementInfo2.origin);
			if (num5 >= 0f && num5 <= 1f)
			{
				if (num5 < 0.5f)
				{
					return i;
				}
				return i + 1;
			}
			num4 = Mathf.Abs(num4);
			if (num4 < num2)
			{
				num = i;
				num2 = num4;
				num3 = num5;
			}
		}
		if (num == -1)
		{
			return lastCharacterIndex;
		}
		if (num3 < 0.5f)
		{
			return num;
		}
		return num + 1;
	}

	public int LineUpCharacterPosition(int originalPos)
	{
		if (originalPos >= textInfo.characterCount)
		{
			originalPos--;
		}
		TextElementInfo textElementInfo = textInfo.textElementInfo[originalPos];
		int lineNumber = textElementInfo.lineNumber;
		if (lineNumber - 1 < 0)
		{
			return 0;
		}
		int num = textInfo.lineInfo[lineNumber].firstCharacterIndex - 1;
		int num2 = -1;
		float num3 = float.PositiveInfinity;
		float num4 = 0f;
		for (int i = textInfo.lineInfo[lineNumber - 1].firstCharacterIndex; i < num; i++)
		{
			TextElementInfo textElementInfo2 = textInfo.textElementInfo[i];
			float num5 = textElementInfo.origin - textElementInfo2.origin;
			float num6 = num5 / (textElementInfo2.xAdvance - textElementInfo2.origin);
			if (num6 >= 0f && num6 <= 1f)
			{
				if (num6 < 0.5f)
				{
					return i;
				}
				return i + 1;
			}
			num5 = Mathf.Abs(num5);
			if (num5 < num3)
			{
				num2 = i;
				num3 = num5;
				num4 = num6;
			}
		}
		if (num2 == -1)
		{
			return num;
		}
		if (num4 < 0.5f)
		{
			return num2;
		}
		return num2 + 1;
	}

	public int FindWordIndex(int cursorIndex)
	{
		for (int i = 0; i < textInfo.wordCount; i++)
		{
			WordInfo wordInfo = textInfo.wordInfo[i];
			if (wordInfo.firstCharacterIndex <= cursorIndex && wordInfo.lastCharacterIndex >= cursorIndex)
			{
				return i;
			}
		}
		return -1;
	}

	public int FindNearestLine(Vector2 position)
	{
		float num = float.PositiveInfinity;
		int result = -1;
		for (int i = 0; i < textInfo.lineCount; i++)
		{
			LineInfo lineInfo = textInfo.lineInfo[i];
			float ascender = lineInfo.ascender;
			float descender = lineInfo.descender;
			if (ascender > position.y && descender < position.y)
			{
				return i;
			}
			float a = Mathf.Abs(ascender - position.y);
			float b = Mathf.Abs(descender - position.y);
			float num2 = Mathf.Min(a, b);
			if (num2 < num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public int FindNearestCharacterOnLine(Vector2 position, int line, bool visibleOnly)
	{
		int firstCharacterIndex = textInfo.lineInfo[line].firstCharacterIndex;
		int lastCharacterIndex = textInfo.lineInfo[line].lastCharacterIndex;
		float num = float.PositiveInfinity;
		int result = lastCharacterIndex;
		for (int i = firstCharacterIndex; i <= lastCharacterIndex; i++)
		{
			TextElementInfo textElementInfo = textInfo.textElementInfo[i];
			if ((!visibleOnly || textElementInfo.isVisible) && textElementInfo.character != '\r' && textElementInfo.character != '\n')
			{
				Vector3 bottomLeft = textElementInfo.bottomLeft;
				Vector3 vector = new Vector3(textElementInfo.bottomLeft.x, textElementInfo.topRight.y, 0f);
				Vector3 topRight = textElementInfo.topRight;
				Vector3 vector2 = new Vector3(textElementInfo.topRight.x, textElementInfo.bottomLeft.y, 0f);
				if (PointIntersectRectangle(position, bottomLeft, vector, topRight, vector2))
				{
					result = i;
					break;
				}
				float num2 = DistanceToLine(bottomLeft, vector, position);
				float num3 = DistanceToLine(vector, topRight, position);
				float num4 = DistanceToLine(topRight, vector2, position);
				float num5 = DistanceToLine(vector2, bottomLeft, position);
				float num6 = ((num2 < num3) ? num2 : num3);
				num6 = ((num6 < num4) ? num6 : num4);
				num6 = ((num6 < num5) ? num6 : num5);
				if (num > num6)
				{
					num = num6;
					result = i;
				}
			}
		}
		return result;
	}

	public int FindIntersectingLink(Vector3 position, bool inverseYAxis = true)
	{
		if (inverseYAxis)
		{
			position.y = textGenerationSettings.screenRect.height - position.y;
		}
		for (int i = 0; i < textInfo.linkCount; i++)
		{
			LinkInfo linkInfo = textInfo.linkInfo[i];
			bool flag = false;
			Vector3 a = Vector3.zero;
			Vector3 b = Vector3.zero;
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			for (int j = 0; j < linkInfo.linkTextLength; j++)
			{
				int num = linkInfo.linkTextfirstCharacterIndex + j;
				TextElementInfo textElementInfo = textInfo.textElementInfo[num];
				int lineNumber = textElementInfo.lineNumber;
				if (!flag)
				{
					flag = true;
					a = new Vector3(textElementInfo.bottomLeft.x, textElementInfo.descender, 0f);
					b = new Vector3(textElementInfo.bottomLeft.x, textElementInfo.ascender, 0f);
					if (linkInfo.linkTextLength == 1)
					{
						flag = false;
						if (PointIntersectRectangle(d: new Vector3(textElementInfo.topRight.x, textElementInfo.descender, 0f), c: new Vector3(textElementInfo.topRight.x, textElementInfo.ascender, 0f), m: position, a: a, b: b))
						{
							return i;
						}
					}
				}
				if (flag && j == linkInfo.linkTextLength - 1)
				{
					flag = false;
					if (PointIntersectRectangle(d: new Vector3(textElementInfo.topRight.x, textElementInfo.descender, 0f), c: new Vector3(textElementInfo.topRight.x, textElementInfo.ascender, 0f), m: position, a: a, b: b))
					{
						return i;
					}
				}
				else if (flag && lineNumber != textInfo.textElementInfo[num + 1].lineNumber)
				{
					flag = false;
					if (PointIntersectRectangle(d: new Vector3(textElementInfo.topRight.x, textElementInfo.descender, 0f), c: new Vector3(textElementInfo.topRight.x, textElementInfo.ascender, 0f), m: position, a: a, b: b))
					{
						return i;
					}
				}
			}
		}
		return -1;
	}

	private static bool PointIntersectRectangle(Vector3 m, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
	{
		Vector3 vector = b - a;
		Vector3 rhs = m - a;
		Vector3 vector2 = c - b;
		Vector3 rhs2 = m - b;
		float num = Vector3.Dot(vector, rhs);
		float num2 = Vector3.Dot(vector2, rhs2);
		return 0f <= num && num <= Vector3.Dot(vector, vector) && 0f <= num2 && num2 <= Vector3.Dot(vector2, vector2);
	}

	private static float DistanceToLine(Vector3 a, Vector3 b, Vector3 point)
	{
		Vector3 vector = b - a;
		Vector3 vector2 = a - point;
		float num = Vector3.Dot(vector, vector2);
		if (num > 0f)
		{
			return Vector3.Dot(vector2, vector2);
		}
		Vector3 vector3 = point - b;
		if (Vector3.Dot(vector, vector3) > 0f)
		{
			return Vector3.Dot(vector3, vector3);
		}
		Vector3 vector4 = vector2 - vector * (num / Vector3.Dot(vector, vector));
		return Vector3.Dot(vector4, vector4);
	}

	public int GetLineNumber(int index)
	{
		if (index <= 0)
		{
			index = 0;
		}
		else if (index >= textInfo.characterCount)
		{
			index = Mathf.Max(0, textInfo.characterCount - 1);
		}
		return textInfo.textElementInfo[index].lineNumber;
	}

	public float GetLineHeight(int lineNumber)
	{
		if (lineNumber <= 0)
		{
			lineNumber = 0;
		}
		else if (lineNumber >= textInfo.lineCount)
		{
			lineNumber = Mathf.Max(0, textInfo.lineCount - 1);
		}
		return textInfo.lineInfo[lineNumber].lineHeight;
	}

	public float GetLineHeightFromCharacterIndex(int index)
	{
		if (index <= 0)
		{
			index = 0;
		}
		else if (index >= textInfo.characterCount)
		{
			index = Mathf.Max(0, textInfo.characterCount - 1);
		}
		return GetLineHeight(textInfo.textElementInfo[index].lineNumber);
	}

	public float GetCharacterHeightFromIndex(int index)
	{
		if (index <= 0)
		{
			index = 0;
		}
		else if (index >= textInfo.characterCount)
		{
			index = Mathf.Max(0, textInfo.characterCount - 1);
		}
		TextElementInfo textElementInfo = textInfo.textElementInfo[index];
		return textElementInfo.ascender - textElementInfo.descender;
	}

	public bool IsElided()
	{
		if (textInfo == null)
		{
			return false;
		}
		if (textInfo.characterCount == 0)
		{
			return true;
		}
		return TextGenerator.isTextTruncated;
	}

	public string Substring(int startIndex, int length)
	{
		if (startIndex < 0 || startIndex + length > textInfo.characterCount)
		{
			throw new ArgumentOutOfRangeException();
		}
		StringBuilder stringBuilder = new StringBuilder(length);
		for (int i = startIndex; i < startIndex + length; i++)
		{
			stringBuilder.Append(textInfo.textElementInfo[i].character);
		}
		return stringBuilder.ToString();
	}

	public int IndexOf(char value, int startIndex)
	{
		if (startIndex < 0 || startIndex >= textInfo.characterCount)
		{
			throw new ArgumentOutOfRangeException();
		}
		for (int i = startIndex; i < textInfo.characterCount; i++)
		{
			if (textInfo.textElementInfo[i].character == value)
			{
				return i;
			}
		}
		return -1;
	}

	public int LastIndexOf(char value, int startIndex)
	{
		if (startIndex < 0 || startIndex >= textInfo.characterCount)
		{
			throw new ArgumentOutOfRangeException();
		}
		for (int num = startIndex; num >= 0; num--)
		{
			if (textInfo.textElementInfo[num].character == value)
			{
				return num;
			}
		}
		return -1;
	}

	protected float ComputeTextWidth(TextGenerationSettings tgs)
	{
		UpdatePreferredValues(tgs);
		return m_PreferredSize.x;
	}

	protected float ComputeTextHeight(TextGenerationSettings tgs)
	{
		UpdatePreferredValues(tgs);
		return m_PreferredSize.y;
	}

	protected void UpdatePreferredValues(TextGenerationSettings tgs)
	{
		m_PreferredSize = TextGenerator.GetPreferredValues(tgs, layoutTextInfo);
	}

	internal TextInfo Update(string newText)
	{
		textGenerationSettings.text = newText;
		return Update(textGenerationSettings);
	}

	protected TextInfo Update(TextGenerationSettings tgs)
	{
		if (!IsDirty())
		{
			return textInfo;
		}
		textInfo.isDirty = true;
		TextGenerator.GenerateText(tgs, textInfo);
		textGenerationSettings = tgs;
		return textInfo;
	}
}
