namespace UnityEngine.Polybrush;

internal static class BoundsUtility
{
	internal struct SphereBounds
	{
		internal Vector3 position;

		internal float radius;

		internal SphereBounds(Vector3 p, float r)
		{
			position = p;
			radius = r;
		}

		internal bool Intersects(SphereBounds other)
		{
			return Vector3.Distance(position, other.position) < radius + other.radius;
		}
	}

	internal static bool GetSphereBounds(GameObject go, out SphereBounds bounds)
	{
		Bounds hierarchyBounds = GetHierarchyBounds(go);
		bounds = default(SphereBounds);
		if (hierarchyBounds.size == Vector3.zero)
		{
			return false;
		}
		bounds.position = hierarchyBounds.center;
		bounds.radius = Mathf.Max(hierarchyBounds.extents.x, hierarchyBounds.extents.z);
		return true;
	}

	internal static Bounds GetHierarchyBounds(GameObject parent)
	{
		Renderer[] componentsInChildren = parent.GetComponentsInChildren<Renderer>();
		Bounds result = default(Bounds);
		if (componentsInChildren.Length == 0)
		{
			return result;
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Bounds bounds = componentsInChildren[i].bounds;
			if (i == 0)
			{
				result.center = bounds.center;
			}
			result.Encapsulate(bounds.max);
			result.Encapsulate(bounds.min);
		}
		return result;
	}
}
