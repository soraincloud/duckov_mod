using System;

namespace UnityEngine;

public class IsometricRuleTile<T> : IsometricRuleTile
{
	public sealed override Type m_NeighborType => typeof(T);
}
[Serializable]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.2d.tilemap.extras@latest/index.html?subfolder=/manual/RuleTile.html")]
public class IsometricRuleTile : RuleTile
{
}
