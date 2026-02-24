using System;

namespace UnityEngine.Polybrush;

[Serializable]
internal struct MirrorSettings
{
	public BrushMirror Axes;

	public MirrorCoordinateSpace Space;
}
