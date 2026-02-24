using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.Tilemaps;

[RequiredByNativeCode]
[NativeType(Header = "Modules/Tilemap/TilemapScripting.h")]
public struct TileChangeData
{
	private Vector3Int m_Position;

	private Object m_TileAsset;

	private Color m_Color;

	private Matrix4x4 m_Transform;

	public Vector3Int position
	{
		get
		{
			return m_Position;
		}
		set
		{
			m_Position = value;
		}
	}

	public TileBase tile
	{
		get
		{
			return (TileBase)m_TileAsset;
		}
		set
		{
			m_TileAsset = value;
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

	public TileChangeData(Vector3Int position, TileBase tile, Color color, Matrix4x4 transform)
	{
		m_Position = position;
		m_TileAsset = tile;
		m_Color = color;
		m_Transform = transform;
	}
}
