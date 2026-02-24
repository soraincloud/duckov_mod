using System.Runtime.CompilerServices;

namespace UnityEngine.UIElements;

internal struct TextureId
{
	private readonly int m_Index;

	public static readonly TextureId invalid = new TextureId(-1);

	public int index => m_Index - 1;

	public TextureId(int index)
	{
		m_Index = index + 1;
	}

	public bool IsValid()
	{
		return m_Index > 0;
	}

	public float ConvertToGpu()
	{
		return index;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is TextureId))
		{
			return false;
		}
		return (TextureId)obj == this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(TextureId other)
	{
		return m_Index == other.m_Index;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		return m_Index.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(TextureId left, TextureId right)
	{
		return left.m_Index == right.m_Index;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(TextureId left, TextureId right)
	{
		return !(left == right);
	}
}
