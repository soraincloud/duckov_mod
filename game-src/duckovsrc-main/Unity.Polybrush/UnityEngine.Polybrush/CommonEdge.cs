using System;
using System.Collections.Generic;

namespace UnityEngine.Polybrush;

public struct CommonEdge : IEquatable<CommonEdge>
{
	internal PolyEdge edge;

	internal PolyEdge common;

	internal int x => edge.x;

	internal int y => edge.y;

	internal int cx => common.x;

	internal int cy => common.y;

	internal CommonEdge(int _x, int _y, int _cx, int _cy)
	{
		edge = new PolyEdge(_x, _y);
		common = new PolyEdge(_cx, _cy);
	}

	public bool Equals(CommonEdge b)
	{
		return common.Equals(b.common);
	}

	public override bool Equals(object b)
	{
		if (b is CommonEdge)
		{
			return common.Equals(((CommonEdge)b).common);
		}
		return false;
	}

	public static bool operator ==(CommonEdge a, CommonEdge b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(CommonEdge a, CommonEdge b)
	{
		return !a.Equals(b);
	}

	public override int GetHashCode()
	{
		return common.GetHashCode();
	}

	public override string ToString()
	{
		return $"{{ {{{edge.x}:{common.x}}}, {{{edge.y}:{common.y}}} }}";
	}

	internal static List<int> ToList(IEnumerable<CommonEdge> edges)
	{
		List<int> list = new List<int>();
		foreach (CommonEdge edge in edges)
		{
			list.Add(edge.edge.x);
			list.Add(edge.edge.y);
		}
		return list;
	}

	internal static HashSet<int> ToHashSet(IEnumerable<CommonEdge> edges)
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (CommonEdge edge in edges)
		{
			hashSet.Add(edge.edge.x);
			hashSet.Add(edge.edge.y);
		}
		return hashSet;
	}
}
