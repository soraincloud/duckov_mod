using UnityEngine;

namespace Umbra;

public enum BlurType
{
	[InspectorName("Gaussian (15 taps, 2 passes)")]
	Gaussian15 = 10,
	[InspectorName("Gaussian (5 taps, 2 passes)")]
	Gaussian5 = 20,
	[InspectorName("Box Blur (9 taps, 1 pass)")]
	Box = 30
}
