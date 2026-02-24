using System;
using UnityEngine.Bindings;

namespace UnityEngine;

[NativeHeader("Modules/Physics/ArticulationBody.h")]
public struct ArticulationReducedSpace
{
	private unsafe fixed float x[3];

	public int dofCount;

	public unsafe float this[int i]
	{
		get
		{
			if (i < 0 || i >= dofCount)
			{
				throw new IndexOutOfRangeException();
			}
			return x[i];
		}
		set
		{
			if (i < 0 || i >= dofCount)
			{
				throw new IndexOutOfRangeException();
			}
			x[i] = value;
		}
	}

	public unsafe ArticulationReducedSpace(float a)
	{
		x[0] = a;
		dofCount = 1;
	}

	public unsafe ArticulationReducedSpace(float a, float b)
	{
		x[0] = a;
		x[1] = b;
		dofCount = 2;
	}

	public unsafe ArticulationReducedSpace(float a, float b, float c)
	{
		x[0] = a;
		x[1] = b;
		x[2] = c;
		dofCount = 3;
	}
}
