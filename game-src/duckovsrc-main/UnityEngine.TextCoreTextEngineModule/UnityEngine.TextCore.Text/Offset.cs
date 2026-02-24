namespace UnityEngine.TextCore.Text;

internal struct Offset
{
	private float m_Left;

	private float m_Right;

	private float m_Top;

	private float m_Bottom;

	private static readonly Offset k_ZeroOffset = new Offset(0f, 0f, 0f, 0f);

	public float left
	{
		get
		{
			return m_Left;
		}
		set
		{
			m_Left = value;
		}
	}

	public float right
	{
		get
		{
			return m_Right;
		}
		set
		{
			m_Right = value;
		}
	}

	public float top
	{
		get
		{
			return m_Top;
		}
		set
		{
			m_Top = value;
		}
	}

	public float bottom
	{
		get
		{
			return m_Bottom;
		}
		set
		{
			m_Bottom = value;
		}
	}

	public float horizontal
	{
		get
		{
			return m_Left;
		}
		set
		{
			m_Left = value;
			m_Right = value;
		}
	}

	public float vertical
	{
		get
		{
			return m_Top;
		}
		set
		{
			m_Top = value;
			m_Bottom = value;
		}
	}

	public static Offset zero => k_ZeroOffset;

	public Offset(float left, float right, float top, float bottom)
	{
		m_Left = left;
		m_Right = right;
		m_Top = top;
		m_Bottom = bottom;
	}

	public Offset(float horizontal, float vertical)
	{
		m_Left = horizontal;
		m_Right = horizontal;
		m_Top = vertical;
		m_Bottom = vertical;
	}

	public static bool operator ==(Offset lhs, Offset rhs)
	{
		return lhs.m_Left == rhs.m_Left && lhs.m_Right == rhs.m_Right && lhs.m_Top == rhs.m_Top && lhs.m_Bottom == rhs.m_Bottom;
	}

	public static bool operator !=(Offset lhs, Offset rhs)
	{
		return !(lhs == rhs);
	}

	public static Offset operator *(Offset a, float b)
	{
		return new Offset(a.m_Left * b, a.m_Right * b, a.m_Top * b, a.m_Bottom * b);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public bool Equals(Offset other)
	{
		return base.Equals((object)other);
	}
}
