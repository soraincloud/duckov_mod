#define UNITY_ASSERTIONS
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityEngine.UIElements.UIR;

internal class Allocator2D
{
	public class Area
	{
		public RectInt rect;

		public BestFitAllocator allocator;

		public Area(RectInt rect)
		{
			this.rect = rect;
			allocator = new BestFitAllocator((uint)rect.height);
		}
	}

	public class Row : LinkedPoolItem<Row>
	{
		public RectInt rect;

		public Area area;

		public BestFitAllocator allocator;

		public Alloc alloc;

		public Row next;

		public static readonly LinkedPool<Row> pool = new LinkedPool<Row>(Create, Reset, 256);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Row Create()
		{
			return new Row();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Reset(Row row)
		{
			row.rect = default(RectInt);
			row.area = null;
			row.allocator = null;
			row.alloc = default(Alloc);
			row.next = null;
		}
	}

	public struct Alloc2D
	{
		public RectInt rect;

		public Row row;

		public Alloc alloc;

		public Alloc2D(Row row, Alloc alloc, int width, int height)
		{
			this.alloc = alloc;
			this.row = row;
			rect = new RectInt(row.rect.xMin + (int)alloc.start, row.rect.yMin, width, height);
		}
	}

	private readonly Vector2Int m_MinSize;

	private readonly Vector2Int m_MaxSize;

	private readonly Vector2Int m_MaxAllocSize;

	private readonly int m_RowHeightBias;

	private readonly Row[] m_Rows;

	private readonly List<Area> m_Areas = new List<Area>();

	public Vector2Int minSize => m_MinSize;

	public Vector2Int maxSize => m_MaxSize;

	public Vector2Int maxAllocSize => m_MaxAllocSize;

	public Allocator2D(int minSize, int maxSize, int rowHeightBias)
		: this(new Vector2Int(minSize, minSize), new Vector2Int(maxSize, maxSize), rowHeightBias)
	{
	}

	public Allocator2D(Vector2Int minSize, Vector2Int maxSize, int rowHeightBias)
	{
		Debug.Assert(minSize.x > 0 && minSize.x <= maxSize.x && minSize.y > 0 && minSize.y <= maxSize.y);
		Debug.Assert(minSize.x == UIRUtility.GetNextPow2(minSize.x) && minSize.y == UIRUtility.GetNextPow2(minSize.y) && maxSize.x == UIRUtility.GetNextPow2(maxSize.x) && maxSize.y == UIRUtility.GetNextPow2(maxSize.y));
		Debug.Assert(rowHeightBias >= 0);
		m_MinSize = minSize;
		m_MaxSize = maxSize;
		m_RowHeightBias = rowHeightBias;
		BuildAreas(m_Areas, minSize, maxSize);
		m_MaxAllocSize = ComputeMaxAllocSize(m_Areas, rowHeightBias);
		m_Rows = BuildRowArray(m_MaxAllocSize.y, rowHeightBias);
	}

	public bool TryAllocate(int width, int height, out Alloc2D alloc2D)
	{
		if (width < 1 || width > m_MaxAllocSize.x || height < 1 || height > m_MaxAllocSize.y)
		{
			alloc2D = default(Alloc2D);
			return false;
		}
		int nextPow2Exp = UIRUtility.GetNextPow2Exp(Mathf.Max(height - m_RowHeightBias, 1));
		for (Row row = m_Rows[nextPow2Exp]; row != null; row = row.next)
		{
			if (row.rect.width >= width)
			{
				Alloc alloc = row.allocator.Allocate((uint)width);
				if (alloc.size != 0)
				{
					alloc2D = new Alloc2D(row, alloc, width, height);
					return true;
				}
			}
		}
		int num = (1 << nextPow2Exp) + m_RowHeightBias;
		Debug.Assert(num >= height);
		for (int i = 0; i < m_Areas.Count; i++)
		{
			Area area = m_Areas[i];
			if (area.rect.height >= num && area.rect.width >= width)
			{
				Alloc alloc2 = area.allocator.Allocate((uint)num);
				if (alloc2.size != 0)
				{
					Row row = Row.pool.Get();
					row.alloc = alloc2;
					row.allocator = new BestFitAllocator((uint)area.rect.width);
					row.area = area;
					row.next = m_Rows[nextPow2Exp];
					row.rect = new RectInt(area.rect.xMin, area.rect.yMin + (int)alloc2.start, area.rect.width, num);
					m_Rows[nextPow2Exp] = row;
					Alloc alloc3 = row.allocator.Allocate((uint)width);
					Debug.Assert(alloc3.size != 0);
					alloc2D = new Alloc2D(row, alloc3, width, height);
					return true;
				}
			}
		}
		alloc2D = default(Alloc2D);
		return false;
	}

	public void Free(Alloc2D alloc2D)
	{
		if (alloc2D.alloc.size == 0)
		{
			return;
		}
		Row row = alloc2D.row;
		row.allocator.Free(alloc2D.alloc);
		if (row.allocator.highWatermark != 0)
		{
			return;
		}
		row.area.allocator.Free(row.alloc);
		int nextPow2Exp = UIRUtility.GetNextPow2Exp(row.rect.height - m_RowHeightBias);
		Row row2 = m_Rows[nextPow2Exp];
		if (row2 == row)
		{
			m_Rows[nextPow2Exp] = row.next;
		}
		else
		{
			Row row3 = row2;
			while (row3.next != row)
			{
				row3 = row3.next;
			}
			row3.next = row.next;
		}
		Row.pool.Return(row);
	}

	private static void BuildAreas(List<Area> areas, Vector2Int minSize, Vector2Int maxSize)
	{
		int num = Mathf.Min(minSize.x, minSize.y);
		int num2 = num;
		areas.Add(new Area(new RectInt(0, 0, num, num2)));
		while (num < maxSize.x || num2 < maxSize.y)
		{
			if (num < maxSize.x)
			{
				areas.Add(new Area(new RectInt(num, 0, num, num2)));
				num *= 2;
			}
			if (num2 < maxSize.y)
			{
				areas.Add(new Area(new RectInt(0, num2, num, num2)));
				num2 *= 2;
			}
		}
	}

	private static Vector2Int ComputeMaxAllocSize(List<Area> areas, int rowHeightBias)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < areas.Count; i++)
		{
			Area area = areas[i];
			num = Mathf.Max(area.rect.width, num);
			num2 = Mathf.Max(area.rect.height, num2);
		}
		return new Vector2Int(num, UIRUtility.GetPrevPow2(num2 - rowHeightBias) + rowHeightBias);
	}

	private static Row[] BuildRowArray(int maxRowHeight, int rowHeightBias)
	{
		int num = UIRUtility.GetNextPow2Exp(maxRowHeight - rowHeightBias) + 1;
		return new Row[num];
	}
}
