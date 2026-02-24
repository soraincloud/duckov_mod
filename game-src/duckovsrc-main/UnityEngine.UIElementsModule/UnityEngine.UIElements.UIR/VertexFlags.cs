using System;

namespace UnityEngine.UIElements.UIR;

internal enum VertexFlags
{
	IsSolid = 0,
	IsText = 1,
	IsTextured = 2,
	IsDynamic = 3,
	IsSvgGradients = 4,
	[Obsolete("Enum member VertexFlags.LastType has been deprecated. Use VertexFlags.IsGraphViewEdge instead.")]
	LastType = 10,
	IsGraphViewEdge = 10
}
