using System;
using System.Collections.Generic;

namespace UnityEngine.Polybrush;

public struct PolyEdge : IEquatable<PolyEdge>
{
	internal int x;

	internal int y;

	internal PolyEdge(int _x, int _y)
	{
		x = _x;
		y = _y;
	}

	public bool Equals(PolyEdge p)
	{
		if (p.x != x || p.y != y)
		{
			if (p.x == y)
			{
				return p.y == x;
			}
			return false;
		}
		return true;
	}

	public override bool Equals(object b)
	{
		if (b is PolyEdge)
		{
			return Equals((PolyEdge)b);
		}
		return false;
	}

	public static bool operator ==(PolyEdge a, PolyEdge b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(PolyEdge a, PolyEdge b)
	{
		return !a.Equals(b);
	}

	public override int GetHashCode()
	{
		int hashCode = ((x < y) ? x : y).GetHashCode();
		int hashCode2 = ((x < y) ? y : x).GetHashCode();
		return (17 * 29 + hashCode.GetHashCode()) * 29 + hashCode2.GetHashCode();
	}

	public override string ToString()
	{
		return $"{{{{{x},{y}}}}}";
	}

	internal static List<int> ToList(IEnumerable<PolyEdge> edges)
	{
		List<int> list = new List<int>();
		foreach (PolyEdge edge in edges)
		{
			list.Add(edge.x);
			list.Add(edge.y);
		}
		return list;
	}

	internal static HashSet<int> ToHashSet(IEnumerable<PolyEdge> edges)
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (PolyEdge edge in edges)
		{
			hashSet.Add(edge.x);
			hashSet.Add(edge.y);
		}
		return hashSet;
	}
}
