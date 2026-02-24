using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

namespace UnityEngine.UIElements;

internal class ColumnLayout
{
	private List<Column> m_StretchableColumns = new List<Column>();

	private List<Column> m_FixedColumns = new List<Column>();

	private List<Column> m_RelativeWidthColumns = new List<Column>();

	private List<Column> m_MixedWidthColumns = new List<Column>();

	private Columns m_Columns;

	private float m_ColumnsWidth = 0f;

	private bool m_ColumnsWidthDirty = true;

	private float m_MaxColumnsWidth = 0f;

	private float m_MinColumnsWidth = 0f;

	private bool m_IsDirty = false;

	private float m_PreviousWidth = float.NaN;

	private float m_LayoutWidth = float.NaN;

	private bool m_DragResizeInPreviewMode;

	private bool m_DragResizing = false;

	private float m_DragStartPos;

	private float m_DragLastPos;

	private float m_DragInitialColumnWidth;

	private List<Column> m_DragStretchableColumns = new List<Column>();

	private List<Column> m_DragRelativeColumns = new List<Column>();

	private List<Column> m_DragFixedColumns = new List<Column>();

	private Dictionary<Column, float> m_PreviewDesiredWidths;

	public Columns columns => m_Columns;

	public bool isDirty => m_IsDirty;

	public float columnsWidth
	{
		get
		{
			if (m_ColumnsWidthDirty)
			{
				m_ColumnsWidth = 0f;
				foreach (Column visible in m_Columns.visibleList)
				{
					m_ColumnsWidth += visible.desiredWidth;
				}
				m_ColumnsWidthDirty = false;
			}
			return m_ColumnsWidth;
		}
	}

	public float layoutWidth => m_LayoutWidth;

	public float minColumnsWidth => m_MinColumnsWidth;

	public float maxColumnsWidth => m_MaxColumnsWidth;

	public bool hasStretchableColumns => m_StretchableColumns.Count > 0;

	public bool hasRelativeWidthColumns => m_RelativeWidthColumns.Count > 0 || m_MixedWidthColumns.Count > 0;

	public event Action layoutRequested;

	public ColumnLayout(Columns columns)
	{
		m_Columns = columns;
		for (int i = 0; i < columns.Count; i++)
		{
			OnColumnAdded(columns[i], i);
		}
		columns.columnAdded += OnColumnAdded;
		columns.columnRemoved += OnColumnRemoved;
		columns.columnReordered += OnColumnReordered;
	}

	public void Dirty()
	{
		if (!m_IsDirty)
		{
			m_IsDirty = true;
			ClearCache();
			this.layoutRequested?.Invoke();
		}
	}

	private void OnColumnAdded(Column column, int index)
	{
		column.changed += OnColumnChanged;
		column.resized += OnColumnResized;
		Dirty();
	}

	private void OnColumnRemoved(Column column)
	{
		column.changed -= OnColumnChanged;
		column.resized -= OnColumnResized;
		Dirty();
	}

	private void OnColumnReordered(Column column, int from, int to)
	{
		Dirty();
	}

	private bool RequiresLayoutUpdate(ColumnDataType type)
	{
		if ((uint)(type - 3) <= 4u || (uint)(type - 11) <= 1u)
		{
			return true;
		}
		return false;
	}

	private void OnColumnChanged(Column column, ColumnDataType type)
	{
		if (!m_DragResizing && RequiresLayoutUpdate(type))
		{
			Dirty();
		}
	}

	private void OnColumnResized(Column column)
	{
		m_ColumnsWidthDirty = true;
	}

	private static bool IsClamped(float value, float min, float max)
	{
		return value >= min && value <= max;
	}

	public void DoLayout(float width)
	{
		m_LayoutWidth = width;
		if (m_IsDirty)
		{
			UpdateCache();
		}
		if (hasRelativeWidthColumns)
		{
			UpdateMinAndMaxColumnsWidth();
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		List<Column> list = new List<Column>();
		List<Column> list2 = new List<Column>();
		foreach (Column column in m_Columns)
		{
			if (!column.visible)
			{
				continue;
			}
			float minWidth = column.GetMinWidth(m_LayoutWidth);
			float maxWidth = column.GetMaxWidth(m_LayoutWidth);
			float width2 = column.GetWidth(m_LayoutWidth);
			if (float.IsNaN(column.desiredWidth))
			{
				if (m_Columns.stretchMode == Columns.StretchMode.GrowAndFill && column.stretchable)
				{
					list.Add(column);
					continue;
				}
				column.desiredWidth = Mathf.Clamp(width2, minWidth, maxWidth);
			}
			else
			{
				if (m_Columns.stretchMode == Columns.StretchMode.GrowAndFill && column.stretchable)
				{
					list2.Add(column);
					num3 += GetDesiredWidth(column);
				}
				if (!IsClamped(column.desiredWidth, minWidth, maxWidth))
				{
					column.desiredWidth = Mathf.Clamp(width2, minWidth, maxWidth);
				}
				if (columns.stretchMode == Columns.StretchMode.Grow && column.width.unit == LengthUnit.Percent)
				{
					column.desiredWidth = Mathf.Clamp(width2, minWidth, maxWidth);
				}
			}
			if (!column.stretchable)
			{
				num2 += column.desiredWidth;
			}
			num += column.desiredWidth;
		}
		if (list.Count > 0)
		{
			float num4 = Math.Max(0f, width - num2);
			int num5 = m_StretchableColumns.Count;
			list.Sort((Column c1, Column c2) => c1.GetMaxWidth(m_LayoutWidth).CompareTo(c2.GetMaxWidth(m_LayoutWidth)));
			foreach (Column item in list)
			{
				float value = num4 / (float)num5;
				item.desiredWidth = Mathf.Clamp(value, item.GetMinWidth(m_LayoutWidth), item.GetMaxWidth(m_LayoutWidth));
				num4 = Math.Max(0f, num4 - item.desiredWidth);
				num5--;
			}
			list2.Sort((Column c1, Column c2) => c1.GetMaxWidth(m_LayoutWidth).CompareTo(c2.GetMaxWidth(m_LayoutWidth)));
			foreach (Column item2 in list2)
			{
				float desiredWidth = GetDesiredWidth(item2);
				float num6 = desiredWidth / num3;
				float value2 = num4 * num6;
				item2.desiredWidth = Mathf.Clamp(value2, item2.GetMinWidth(m_LayoutWidth), item2.GetMaxWidth(m_LayoutWidth));
				num4 = Math.Max(0f, num4 - item2.desiredWidth);
				num3 -= desiredWidth;
				num5--;
			}
		}
		if (hasStretchableColumns || (hasRelativeWidthColumns && m_Columns.stretchMode == Columns.StretchMode.GrowAndFill))
		{
			float delta = 0f;
			if (m_Columns.stretchMode == Columns.StretchMode.Grow)
			{
				if (!float.IsNaN(m_PreviousWidth))
				{
					delta = m_PreviousWidth - width;
				}
			}
			else
			{
				delta = columnsWidth - Mathf.Clamp(width, minColumnsWidth, maxColumnsWidth);
			}
			if (delta != 0f)
			{
				List<Column> value3;
				using (CollectionPool<List<Column>, Column>.Get(out value3))
				{
					List<Column> value4;
					using (CollectionPool<List<Column>, Column>.Get(out value4))
					{
						List<Column> value5;
						using (CollectionPool<List<Column>, Column>.Get(out value5))
						{
							value3.AddRange(m_StretchableColumns);
							value4.AddRange(m_FixedColumns);
							value5.AddRange(m_RelativeWidthColumns);
							StretchResizeColumns(value3, value4, value5, ref delta, resizeToFit: false, dragResize: false);
						}
					}
				}
			}
		}
		m_PreviousWidth = width;
		m_IsDirty = false;
	}

	public void StretchResizeColumns(List<Column> stretchableColumns, List<Column> fixedColumns, List<Column> relativeWidthColumns, ref float delta, bool resizeToFit, bool dragResize)
	{
		if (stretchableColumns.Count != 0 || relativeWidthColumns.Count != 0 || fixedColumns.Count != 0)
		{
			if (delta > 0f)
			{
				DistributeOverflow(stretchableColumns, fixedColumns, relativeWidthColumns, ref delta, resizeToFit, dragResize);
			}
			else
			{
				DistributeExcess(stretchableColumns, fixedColumns, relativeWidthColumns, ref delta, resizeToFit, dragResize);
			}
		}
	}

	private void DistributeOverflow(List<Column> stretchableColumns, List<Column> fixedColumns, List<Column> relativeWidthColumns, ref float delta, bool resizeToFit, bool dragResize)
	{
		float distributedDelta = Math.Abs(delta);
		if (!resizeToFit && !dragResize)
		{
			distributedDelta = RecomputeToDesiredWidth(fixedColumns, distributedDelta, setDesiredWidthOnly: true, distributeOverflow: true);
			distributedDelta = RecomputeToDesiredWidth(relativeWidthColumns, distributedDelta, setDesiredWidthOnly: true, distributeOverflow: true);
		}
		distributedDelta = RecomputeToMinWidthProportionally(stretchableColumns, distributedDelta, !(resizeToFit || dragResize));
		if (resizeToFit)
		{
			distributedDelta = RecomputeToMinWidthProportionally(relativeWidthColumns, distributedDelta);
			distributedDelta = RecomputeToMinWidthProportionally(fixedColumns, distributedDelta);
			distributedDelta = RecomputeToMinWidth(relativeWidthColumns, distributedDelta);
			distributedDelta = RecomputeToMinWidth(fixedColumns, distributedDelta);
		}
		else if (dragResize)
		{
			distributedDelta = RecomputeToMinWidth(relativeWidthColumns, distributedDelta, setDesiredWidthOnly: true);
			distributedDelta = RecomputeToMinWidth(fixedColumns, distributedDelta, setDesiredWidthOnly: true);
		}
		else if (distributedDelta > 0f)
		{
			distributedDelta = RecomputeToMinWidth(relativeWidthColumns, distributedDelta, setDesiredWidthOnly: true);
			distributedDelta = RecomputeToMinWidth(fixedColumns, distributedDelta, setDesiredWidthOnly: true);
		}
		delta = Math.Max(0f, delta - distributedDelta);
	}

	private void DistributeExcess(List<Column> stretchableColumns, List<Column> fixedColumns, List<Column> relativeWidthColumns, ref float delta, bool resizeToFit, bool dragResize)
	{
		float distributedDelta = Math.Abs(delta);
		if (!resizeToFit && !dragResize)
		{
			distributedDelta = RecomputeToDesiredWidth(fixedColumns, distributedDelta, setDesiredWidthOnly: true, distributeOverflow: false);
			distributedDelta = RecomputeToDesiredWidth(relativeWidthColumns, distributedDelta, setDesiredWidthOnly: true, distributeOverflow: false);
		}
		if (dragResize)
		{
			distributedDelta = RecomputeToDesiredWidth(fixedColumns, distributedDelta, setDesiredWidthOnly: true, distributeOverflow: false);
			distributedDelta = RecomputeToDesiredWidth(relativeWidthColumns, distributedDelta, setDesiredWidthOnly: true, distributeOverflow: false);
		}
		distributedDelta = RecomputeToMaxWidthProportionally(stretchableColumns, distributedDelta, !(resizeToFit || dragResize));
		if (resizeToFit)
		{
			distributedDelta = RecomputeToMaxWidthProportionally(relativeWidthColumns, distributedDelta);
			distributedDelta = RecomputeToMaxWidthProportionally(fixedColumns, distributedDelta);
			distributedDelta = RecomputeToMaxWidth(relativeWidthColumns, distributedDelta);
			distributedDelta = RecomputeToMaxWidth(fixedColumns, distributedDelta);
		}
		delta += distributedDelta;
	}

	private float RecomputeToMaxWidthProportionally(List<Column> columns, float distributedDelta, bool setDesiredWidthOnly = false)
	{
		if (distributedDelta > 0f)
		{
			columns.Sort((Column c1, Column c2) => c1.GetMaxWidth(m_LayoutWidth).CompareTo(c2.GetMaxWidth(m_LayoutWidth)));
			float totalColumnWidth = 0f;
			columns.ForEach(delegate(Column c)
			{
				totalColumnWidth += GetDesiredWidth(c);
			});
			for (int num = 0; num < columns.Count; num++)
			{
				Column column = columns[num];
				float desiredWidth = GetDesiredWidth(column);
				float num2 = GetDesiredWidth(column) / totalColumnWidth;
				float val = distributedDelta * num2;
				float num3 = 0f;
				float maxWidth = column.GetMaxWidth(m_LayoutWidth);
				if (GetDesiredWidth(column) < maxWidth)
				{
					num3 = Math.Min(val, maxWidth - GetDesiredWidth(column));
				}
				if (num3 > 0f)
				{
					ResizeColumn(column, GetDesiredWidth(column) + num3, setDesiredWidthOnly);
				}
				totalColumnWidth -= desiredWidth;
				distributedDelta -= num3;
				if (distributedDelta <= 0f)
				{
					break;
				}
			}
		}
		return distributedDelta;
	}

	private float RecomputeToMinWidthProportionally(List<Column> columns, float distributedDelta, bool setDesiredWidthOnly = false)
	{
		if (distributedDelta > 0f)
		{
			columns.Sort((Column c1, Column c2) => c2.GetMinWidth(m_LayoutWidth).CompareTo(c1.GetMinWidth(m_LayoutWidth)));
			float totalColumnsWidth = 0f;
			columns.ForEach(delegate(Column c)
			{
				totalColumnsWidth += GetDesiredWidth(c);
			});
			for (int num = 0; num < columns.Count; num++)
			{
				Column column = columns[num];
				float desiredWidth = GetDesiredWidth(column);
				float num2 = GetDesiredWidth(column) / totalColumnsWidth;
				float val = distributedDelta * num2;
				float num3 = 0f;
				if (GetDesiredWidth(column) > column.GetMinWidth(m_LayoutWidth))
				{
					num3 = Math.Min(val, GetDesiredWidth(column) - column.GetMinWidth(m_LayoutWidth));
				}
				if (num3 > 0f)
				{
					ResizeColumn(column, GetDesiredWidth(column) - num3, setDesiredWidthOnly);
				}
				totalColumnsWidth -= desiredWidth;
				distributedDelta -= num3;
				if (distributedDelta <= 0f)
				{
					break;
				}
			}
		}
		return distributedDelta;
	}

	private float RecomputeToDesiredWidth(List<Column> columns, float distributedDelta, bool setDesiredWidthOnly, bool distributeOverflow)
	{
		if (distributeOverflow)
		{
			for (int num = columns.Count - 1; num >= 0; num--)
			{
				distributedDelta = RecomputeToDesiredWidth(columns[num], distributedDelta, setDesiredWidthOnly, distributeOverflow: true);
				if (distributedDelta <= 0f)
				{
					break;
				}
			}
		}
		else
		{
			for (int i = 0; i < columns.Count; i++)
			{
				distributedDelta = RecomputeToDesiredWidth(columns[i], distributedDelta, setDesiredWidthOnly, distributeOverflow: false);
				if (distributedDelta <= 0f)
				{
					break;
				}
			}
		}
		return distributedDelta;
	}

	private float RecomputeToDesiredWidth(Column column, float distributedDelta, bool setDesiredWidthOnly, bool distributeOverflow)
	{
		float num = 0f;
		float num2 = Mathf.Clamp(column.GetWidth(m_LayoutWidth), column.GetMinWidth(m_LayoutWidth), column.GetMaxWidth(m_LayoutWidth));
		if (GetDesiredWidth(column) > num2 && distributeOverflow)
		{
			num = Math.Min(distributedDelta, Math.Abs(GetDesiredWidth(column) - num2));
		}
		if (GetDesiredWidth(column) < num2 && !distributeOverflow)
		{
			num = Math.Min(distributedDelta, Math.Abs(num2 - GetDesiredWidth(column)));
		}
		float width = (distributeOverflow ? (GetDesiredWidth(column) - num) : (GetDesiredWidth(column) + num));
		if (num > 0f)
		{
			ResizeColumn(column, width, setDesiredWidthOnly);
		}
		distributedDelta -= num;
		return distributedDelta;
	}

	private float RecomputeToMinWidth(List<Column> columns, float distributedDelta, bool setDesiredWidthOnly = false)
	{
		if (distributedDelta > 0f)
		{
			for (int num = columns.Count - 1; num >= 0; num--)
			{
				Column column = columns[num];
				float num2 = 0f;
				if (GetDesiredWidth(column) > column.GetMinWidth(m_LayoutWidth))
				{
					num2 = Math.Min(distributedDelta, GetDesiredWidth(column) - column.GetMinWidth(m_LayoutWidth));
				}
				if (num2 > 0f)
				{
					ResizeColumn(column, GetDesiredWidth(column) - num2, setDesiredWidthOnly);
				}
				distributedDelta -= num2;
				if (distributedDelta <= 0f)
				{
					break;
				}
			}
		}
		return distributedDelta;
	}

	private float RecomputeToMaxWidth(List<Column> columns, float distributedDelta, bool setDesiredWidthOnly = false)
	{
		if (distributedDelta > 0f)
		{
			for (int i = 0; i < columns.Count; i++)
			{
				Column column = columns[i];
				float num = 0f;
				if (GetDesiredWidth(column) < column.GetMaxWidth(m_LayoutWidth))
				{
					num = Math.Min(distributedDelta, Math.Abs(column.GetMaxWidth(m_LayoutWidth) - GetDesiredWidth(column)));
				}
				if (num > 0f)
				{
					ResizeColumn(column, GetDesiredWidth(column) + num, setDesiredWidthOnly);
				}
				distributedDelta -= num;
				if (distributedDelta <= 0f)
				{
					break;
				}
			}
		}
		return distributedDelta;
	}

	public void ResizeToFit(float width)
	{
		float delta = columnsWidth - Mathf.Clamp(width, minColumnsWidth, maxColumnsWidth);
		List<Column> value;
		using (CollectionPool<List<Column>, Column>.Get(out value))
		{
			List<Column> value2;
			using (CollectionPool<List<Column>, Column>.Get(out value2))
			{
				List<Column> value3;
				using (CollectionPool<List<Column>, Column>.Get(out value3))
				{
					value.AddRange(m_StretchableColumns);
					value2.AddRange(m_FixedColumns);
					value3.AddRange(m_RelativeWidthColumns);
					StretchResizeColumns(value, value2, value3, ref delta, resizeToFit: true, dragResize: false);
					if (m_IsDirty)
					{
						UpdateCache();
					}
				}
			}
		}
	}

	private void ResizeColumn(Column column, float width, bool setDesiredWidthOnly = false)
	{
		Length length = new Length(width / layoutWidth * 100f, LengthUnit.Percent);
		if (m_DragResizeInPreviewMode)
		{
			m_PreviewDesiredWidths[column] = width;
			return;
		}
		if (!setDesiredWidthOnly)
		{
			column.width = ((column.width.unit == LengthUnit.Percent) ? length : ((Length)width));
		}
		column.desiredWidth = width;
	}

	internal void BeginDragResize(Column column, float pos, bool previewMode)
	{
		if (m_IsDirty)
		{
			throw new Exception("Cannot begin resizing columns because the layout needs to be updated");
		}
		m_DragResizeInPreviewMode = previewMode;
		m_DragResizing = true;
		int visibleIndex = column.visibleIndex;
		m_DragStartPos = pos;
		m_DragLastPos = pos;
		m_DragInitialColumnWidth = column.desiredWidth;
		m_DragStretchableColumns.Clear();
		m_DragFixedColumns.Clear();
		m_DragRelativeColumns.Clear();
		if (m_DragResizeInPreviewMode)
		{
			if (m_PreviewDesiredWidths == null)
			{
				m_PreviewDesiredWidths = new Dictionary<Column, float>();
			}
			m_PreviewDesiredWidths[column] = column.desiredWidth;
		}
		for (int i = visibleIndex + 1; i < m_Columns.visibleList.Count(); i++)
		{
			Column column2 = m_Columns.visibleList.ElementAt(i);
			if (column2.visible)
			{
				if (column2.stretchable)
				{
					m_DragStretchableColumns.Add(column2);
				}
				else if (column2.width.unit == LengthUnit.Percent)
				{
					m_DragRelativeColumns.Add(column2);
				}
				else
				{
					m_DragFixedColumns.Add(column2);
				}
				if (m_DragResizeInPreviewMode)
				{
					m_PreviewDesiredWidths[column2] = column2.desiredWidth;
				}
			}
		}
	}

	public float GetDesiredPosition(Column column)
	{
		if (!column.visible)
		{
			return float.NaN;
		}
		float num = 0f;
		for (int i = 0; i < column.visibleIndex; i++)
		{
			Column c = m_Columns.visibleList.ElementAt(i);
			float desiredWidth = GetDesiredWidth(c);
			if (!float.IsNaN(desiredWidth))
			{
				num += desiredWidth;
			}
		}
		return num;
	}

	public float GetDesiredWidth(Column c)
	{
		if (m_DragResizeInPreviewMode && m_PreviewDesiredWidths.ContainsKey(c))
		{
			return m_PreviewDesiredWidths[c];
		}
		return c.desiredWidth;
	}

	public void DragResize(Column column, float pos)
	{
		float minWidth = column.GetMinWidth(m_LayoutWidth);
		float maxWidth = column.GetMaxWidth(m_LayoutWidth);
		if (m_Columns.stretchMode == Columns.StretchMode.GrowAndFill)
		{
			float num = pos - m_DragLastPos;
			float num2 = Mathf.Clamp(GetDesiredWidth(column) + num, minWidth, maxWidth);
			num = num2 - GetDesiredWidth(column);
			if (m_DragStretchableColumns.Count == 0 && num < 0f)
			{
				StretchResizeColumns(m_DragStretchableColumns, m_DragFixedColumns, m_DragRelativeColumns, ref num, resizeToFit: false, dragResize: true);
				num2 = Mathf.Clamp(GetDesiredWidth(column) + num2 - GetDesiredWidth(column), minWidth, maxWidth);
			}
			else if (num > 0f && columnsWidth + num < m_LayoutWidth)
			{
				float delta = ((num < m_LayoutWidth - columnsWidth) ? 0f : (num - (m_LayoutWidth - columnsWidth)));
				StretchResizeColumns(m_DragStretchableColumns, m_DragFixedColumns, m_DragRelativeColumns, ref delta, resizeToFit: false, dragResize: true);
				num2 = Mathf.Clamp(GetDesiredWidth(column) + num - delta, minWidth, maxWidth);
			}
			else
			{
				StretchResizeColumns(m_DragStretchableColumns, m_DragFixedColumns, m_DragRelativeColumns, ref num, resizeToFit: false, dragResize: true);
				num2 = Mathf.Clamp(GetDesiredWidth(column) + num, minWidth, maxWidth);
			}
			ResizeColumn(column, num2);
		}
		else
		{
			float num3 = pos - m_DragStartPos;
			float width = Math.Max(minWidth, Math.Min(maxWidth, m_DragInitialColumnWidth + num3));
			ResizeColumn(column, width);
		}
		m_DragLastPos = pos;
	}

	internal void EndDragResize(Column column, bool cancelled)
	{
		if (m_DragResizeInPreviewMode)
		{
			m_DragResizeInPreviewMode = false;
			if (!cancelled)
			{
				foreach (KeyValuePair<Column, float> previewDesiredWidth in m_PreviewDesiredWidths)
				{
					ResizeColumn(previewDesiredWidth.Key, previewDesiredWidth.Value, previewDesiredWidth.Key != column);
				}
			}
			m_PreviewDesiredWidths.Clear();
		}
		m_DragResizing = false;
		m_DragStretchableColumns.Clear();
		m_DragFixedColumns.Clear();
		m_DragRelativeColumns.Clear();
	}

	private void UpdateCache()
	{
		ClearCache();
		foreach (Column visible in m_Columns.visibleList)
		{
			if (visible.stretchable && columns.stretchMode == Columns.StretchMode.GrowAndFill)
			{
				m_StretchableColumns.Add(visible);
			}
			else if (visible.width.unit == LengthUnit.Pixel)
			{
				m_FixedColumns.Add(visible);
			}
			if (visible.width.unit == LengthUnit.Percent)
			{
				m_RelativeWidthColumns.Add(visible);
			}
			if (visible.width.unit == LengthUnit.Pixel && (visible.minWidth.unit == LengthUnit.Percent || visible.maxWidth.unit == LengthUnit.Percent))
			{
				m_MixedWidthColumns.Add(visible);
			}
			m_MaxColumnsWidth += visible.GetMaxWidth(m_LayoutWidth);
			m_MinColumnsWidth += visible.GetMinWidth(m_LayoutWidth);
		}
	}

	private void UpdateMinAndMaxColumnsWidth()
	{
		m_MaxColumnsWidth = 0f;
		m_MinColumnsWidth = 0f;
		foreach (Column visible in m_Columns.visibleList)
		{
			m_MaxColumnsWidth += visible.GetMaxWidth(m_LayoutWidth);
			m_MinColumnsWidth += visible.GetMinWidth(m_LayoutWidth);
		}
	}

	private void ClearCache()
	{
		m_StretchableColumns.Clear();
		m_RelativeWidthColumns.Clear();
		m_FixedColumns.Clear();
		m_MaxColumnsWidth = 0f;
		m_MinColumnsWidth = 0f;
		m_ColumnsWidthDirty = true;
	}
}
