namespace UnityEngine;

public struct QueryParameters
{
	public int layerMask;

	public bool hitMultipleFaces;

	public QueryTriggerInteraction hitTriggers;

	public bool hitBackfaces;

	public static QueryParameters Default => new QueryParameters(-5, hitMultipleFaces: false, QueryTriggerInteraction.UseGlobal, hitBackfaces: false);

	public QueryParameters(int layerMask = -5, bool hitMultipleFaces = false, QueryTriggerInteraction hitTriggers = QueryTriggerInteraction.UseGlobal, bool hitBackfaces = false)
	{
		this.layerMask = layerMask;
		this.hitMultipleFaces = hitMultipleFaces;
		this.hitTriggers = hitTriggers;
		this.hitBackfaces = hitBackfaces;
	}
}
