using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.Tilemaps;

[RequiredByNativeCode]
[NativeType(Header = "Modules/Tilemap/TilemapScripting.h")]
internal struct TileDataNative
{
	private int m_Sprite;

	private Color m_Color;

	private Matrix4x4 m_Transform;

	private int m_GameObject;

	private TileFlags m_Flags;

	private Tile.ColliderType m_ColliderType;

	public int sprite
	{
		get
		{
			return m_Sprite;
		}
		set
		{
			m_Sprite = value;
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

	public int gameObject
	{
		get
		{
			return m_GameObject;
		}
		set
		{
			m_GameObject = value;
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

	public static implicit operator TileDataNative(TileData td)
	{
		return new TileDataNative
		{
			sprite = ((td.sprite != null) ? td.sprite.GetInstanceID() : 0),
			color = td.color,
			transform = td.transform,
			gameObject = ((td.gameObject != null) ? td.gameObject.GetInstanceID() : 0),
			flags = td.flags,
			colliderType = td.colliderType
		};
	}
}
