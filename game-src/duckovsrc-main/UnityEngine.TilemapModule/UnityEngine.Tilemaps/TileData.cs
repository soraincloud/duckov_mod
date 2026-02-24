using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.Tilemaps;

[RequiredByNativeCode]
[NativeType(Header = "Modules/Tilemap/TilemapScripting.h")]
public struct TileData
{
	private int m_Sprite;

	private Color m_Color;

	private Matrix4x4 m_Transform;

	private int m_GameObject;

	private TileFlags m_Flags;

	private Tile.ColliderType m_ColliderType;

	internal static readonly TileData Default = CreateDefault();

	public Sprite sprite
	{
		get
		{
			return Object.ForceLoadFromInstanceID(m_Sprite) as Sprite;
		}
		set
		{
			m_Sprite = ((value != null) ? value.GetInstanceID() : 0);
		}
	}

	public Color color
	{
		get
		{
			return m_Color;
		}
		set
		{
			m_Color = value;
		}
	}

	public Matrix4x4 transform
	{
		get
		{
			return m_Transform;
		}
		set
		{
			m_Transform = value;
		}
	}

	public GameObject gameObject
	{
		get
		{
			return Object.ForceLoadFromInstanceID(m_GameObject) as GameObject;
		}
		set
		{
			m_GameObject = ((value != null) ? value.GetInstanceID() : 0);
		}
	}

	public TileFlags flags
	{
		get
		{
			return m_Flags;
		}
		set
		{
			m_Flags = value;
		}
	}

	public Tile.ColliderType colliderType
	{
		get
		{
			return m_ColliderType;
		}
		set
		{
			m_ColliderType = value;
		}
	}

	private static TileData CreateDefault()
	{
		return new TileData
		{
			color = Color.white,
			transform = Matrix4x4.identity,
			flags = TileFlags.None,
			colliderType = Tile.ColliderType.None
		};
	}
}
