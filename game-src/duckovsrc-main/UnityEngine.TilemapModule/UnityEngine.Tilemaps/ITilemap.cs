using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Scripting;

namespace UnityEngine.Tilemaps;

[RequiredByNativeCode]
public class ITilemap
{
	internal static ITilemap s_Instance;

	internal Tilemap m_Tilemap;

	internal bool m_AddToList;

	internal int m_RefreshCount;

	internal NativeArray<Vector3Int> m_RefreshPos;

	public Vector3Int origin => m_Tilemap.origin;

	public Vector3Int size => m_Tilemap.size;

	public Bounds localBounds => m_Tilemap.localBounds;

	public BoundsInt cellBounds => m_Tilemap.cellBounds;

	internal ITilemap()
	{
	}

	public ITilemap(Tilemap tilemap)
	{
		if (tilemap == null)
		{
			throw new ArgumentNullException("Argument tilemap cannot be null");
		}
		m_Tilemap = tilemap;
	}

	public static implicit operator ITilemap(Tilemap tilemap)
	{
		return new ITilemap(tilemap);
	}

	internal void SetTilemapInstance(Tilemap tilemap)
	{
		m_Tilemap = tilemap;
	}

	public virtual Sprite GetSprite(Vector3Int position)
	{
		return m_Tilemap.GetSprite(position);
	}

	public virtual Color GetColor(Vector3Int position)
	{
		return m_Tilemap.GetColor(position);
	}

	public virtual Matrix4x4 GetTransformMatrix(Vector3Int position)
	{
		return m_Tilemap.GetTransformMatrix(position);
	}

	public virtual TileFlags GetTileFlags(Vector3Int position)
	{
		return m_Tilemap.GetTileFlags(position);
	}

	public virtual TileBase GetTile(Vector3Int position)
	{
		return m_Tilemap.GetTile(position);
	}

	public virtual T GetTile<T>(Vector3Int position) where T : TileBase
	{
		return m_Tilemap.GetTile<T>(position);
	}

	public void RefreshTile(Vector3Int position)
	{
		if (m_AddToList)
		{
			if (m_RefreshCount >= m_RefreshPos.Length)
			{
				NativeArray<Vector3Int> nativeArray = new NativeArray<Vector3Int>(Math.Max(1, m_RefreshCount * 2), Allocator.Temp);
				NativeArray<Vector3Int>.Copy(m_RefreshPos, nativeArray, m_RefreshPos.Length);
				m_RefreshPos.Dispose();
				m_RefreshPos = nativeArray;
			}
			m_RefreshPos[m_RefreshCount++] = position;
		}
		else
		{
			m_Tilemap.RefreshTile(position);
		}
	}

	public T GetComponent<T>()
	{
		if (typeof(T) == typeof(Tilemap))
		{
			return (T)(object)m_Tilemap;
		}
		return m_Tilemap.GetComponent<T>();
	}

	[RequiredByNativeCode]
	private static ITilemap CreateInstance()
	{
		s_Instance = new ITilemap();
		return s_Instance;
	}

	[RequiredByNativeCode]
	private unsafe static void FindAllRefreshPositions(ITilemap tilemap, int count, IntPtr oldTilesIntPtr, IntPtr newTilesIntPtr, IntPtr positionsIntPtr)
	{
		tilemap.m_AddToList = true;
		_ = tilemap.m_RefreshPos;
		if (!tilemap.m_RefreshPos.IsCreated || tilemap.m_RefreshPos.Length < count)
		{
			tilemap.m_RefreshPos = new NativeArray<Vector3Int>(Math.Max(16, count), Allocator.Temp);
		}
		tilemap.m_RefreshCount = 0;
		void* dataPointer = oldTilesIntPtr.ToPointer();
		void* dataPointer2 = newTilesIntPtr.ToPointer();
		void* dataPointer3 = positionsIntPtr.ToPointer();
		NativeArray<int> nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(dataPointer, count, Allocator.Invalid);
		NativeArray<int> nativeArray2 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(dataPointer2, count, Allocator.Invalid);
		NativeArray<Vector3Int> nativeArray3 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Vector3Int>(dataPointer3, count, Allocator.Invalid);
		for (int i = 0; i < count; i++)
		{
			int num = nativeArray[i];
			int num2 = nativeArray2[i];
			Vector3Int position = nativeArray3[i];
			if (num != 0)
			{
				TileBase tileBase = (TileBase)Object.ForceLoadFromInstanceID(num);
				tileBase.RefreshTile(position, tilemap);
			}
			if (num2 != 0)
			{
				TileBase tileBase2 = (TileBase)Object.ForceLoadFromInstanceID(num2);
				tileBase2.RefreshTile(position, tilemap);
			}
		}
		tilemap.m_Tilemap.RefreshTilesNative(tilemap.m_RefreshPos.m_Buffer, tilemap.m_RefreshCount);
		tilemap.m_RefreshPos.Dispose();
		tilemap.m_AddToList = false;
	}

	[RequiredByNativeCode]
	private unsafe static void GetAllTileData(ITilemap tilemap, int count, IntPtr tilesIntPtr, IntPtr positionsIntPtr, IntPtr outTileDataIntPtr)
	{
		void* dataPointer = tilesIntPtr.ToPointer();
		void* dataPointer2 = positionsIntPtr.ToPointer();
		void* dataPointer3 = outTileDataIntPtr.ToPointer();
		NativeArray<int> nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(dataPointer, count, Allocator.Invalid);
		NativeArray<Vector3Int> nativeArray2 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Vector3Int>(dataPointer2, count, Allocator.Invalid);
		NativeArray<TileData> nativeArray3 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<TileData>(dataPointer3, count, Allocator.Invalid);
		for (int i = 0; i < count; i++)
		{
			TileData tileData = TileData.Default;
			int num = nativeArray[i];
			if (num != 0)
			{
				TileBase tileBase = (TileBase)Object.ForceLoadFromInstanceID(num);
				tileBase.GetTileData(nativeArray2[i], tilemap, ref tileData);
			}
			nativeArray3[i] = tileData;
		}
	}
}
