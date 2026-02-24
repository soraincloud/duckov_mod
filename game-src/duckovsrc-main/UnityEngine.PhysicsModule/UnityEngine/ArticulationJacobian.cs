using System;
using System.Collections.Generic;
using UnityEngine.Bindings;

namespace UnityEngine;

[NativeHeader("Modules/Physics/ArticulationBody.h")]
public struct ArticulationJacobian
{
	private int rowsCount;

	private int colsCount;

	private List<float> matrixData;

	public float this[int row, int col]
	{
		get
		{
			if (row < 0 || row >= rowsCount)
			{
				throw new IndexOutOfRangeException();
			}
			if (col < 0 || col >= colsCount)
			{
				throw new IndexOutOfRangeException();
			}
			return matrixData[row * colsCount + col];
		}
		set
		{
			if (row < 0 || row >= rowsCount)
			{
				throw new IndexOutOfRangeException();
			}
			if (col < 0 || col >= colsCount)
			{
				throw new IndexOutOfRangeException();
			}
			matrixData[row * colsCount + col] = value;
		}
	}

	public int rows
	{
		get
		{
			return rowsCount;
		}
		set
		{
			rowsCount = value;
		}
	}

	public int columns
	{
		get
		{
			return colsCount;
		}
		set
		{
			colsCount = value;
		}
	}

	public List<float> elements
	{
		get
		{
			return matrixData;
		}
		set
		{
			matrixData = value;
		}
	}

	public ArticulationJacobian(int rows, int cols)
	{
		rowsCount = rows;
		colsCount = cols;
		matrixData = new List<float>(rows * cols);
		for (int i = 0; i < rows * cols; i++)
		{
			matrixData.Add(0f);
		}
	}
}
