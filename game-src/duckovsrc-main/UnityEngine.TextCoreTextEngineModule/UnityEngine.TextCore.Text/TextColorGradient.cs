using System;

namespace UnityEngine.TextCore.Text;

[Serializable]
[ExcludeFromPreset]
[ExcludeFromObjectFactory]
public class TextColorGradient : ScriptableObject
{
	public ColorGradientMode colorMode = ColorGradientMode.FourCornersGradient;

	public Color topLeft;

	public Color topRight;

	public Color bottomLeft;

	public Color bottomRight;

	private const ColorGradientMode k_DefaultColorMode = ColorGradientMode.FourCornersGradient;

	private static readonly Color k_DefaultColor = Color.white;

	public TextColorGradient()
	{
		colorMode = ColorGradientMode.FourCornersGradient;
		topLeft = k_DefaultColor;
		topRight = k_DefaultColor;
		bottomLeft = k_DefaultColor;
		bottomRight = k_DefaultColor;
	}

	public TextColorGradient(Color color)
	{
		colorMode = ColorGradientMode.FourCornersGradient;
		topLeft = color;
		topRight = color;
		bottomLeft = color;
		bottomRight = color;
	}

	public TextColorGradient(Color color0, Color color1, Color color2, Color color3)
	{
		colorMode = ColorGradientMode.FourCornersGradient;
		topLeft = color0;
		topRight = color1;
		bottomLeft = color2;
		bottomRight = color3;
	}
}
