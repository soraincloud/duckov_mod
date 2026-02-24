using System;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[VolumeComponentMenuForRenderPipeline("Post-processing/Color Curves", new Type[] { typeof(UniversalRenderPipeline) })]
public sealed class ColorCurves : VolumeComponent, IPostProcessComponent
{
	[Tooltip("Affects the luminance across the whole image.")]
	public TextureCurveParameter master = new TextureCurveParameter(new TextureCurve(new Keyframe[2]
	{
		new Keyframe(0f, 0f, 1f, 1f),
		new Keyframe(1f, 1f, 1f, 1f)
	}, 0f, loop: false, new Vector2(0f, 1f)));

	[Tooltip("Affects the red channel intensity across the whole image.")]
	public TextureCurveParameter red = new TextureCurveParameter(new TextureCurve(new Keyframe[2]
	{
		new Keyframe(0f, 0f, 1f, 1f),
		new Keyframe(1f, 1f, 1f, 1f)
	}, 0f, loop: false, new Vector2(0f, 1f)));

	[Tooltip("Affects the green channel intensity across the whole image.")]
	public TextureCurveParameter green = new TextureCurveParameter(new TextureCurve(new Keyframe[2]
	{
		new Keyframe(0f, 0f, 1f, 1f),
		new Keyframe(1f, 1f, 1f, 1f)
	}, 0f, loop: false, new Vector2(0f, 1f)));

	[Tooltip("Affects the blue channel intensity across the whole image.")]
	public TextureCurveParameter blue = new TextureCurveParameter(new TextureCurve(new Keyframe[2]
	{
		new Keyframe(0f, 0f, 1f, 1f),
		new Keyframe(1f, 1f, 1f, 1f)
	}, 0f, loop: false, new Vector2(0f, 1f)));

	[Tooltip("Shifts the input hue (x-axis) according to the output hue (y-axis).")]
	public TextureCurveParameter hueVsHue = new TextureCurveParameter(new TextureCurve(new Keyframe[0], 0.5f, loop: true, new Vector2(0f, 1f)));

	[Tooltip("Adjusts saturation (y-axis) according to the input hue (x-axis).")]
	public TextureCurveParameter hueVsSat = new TextureCurveParameter(new TextureCurve(new Keyframe[0], 0.5f, loop: true, new Vector2(0f, 1f)));

	[Tooltip("Adjusts saturation (y-axis) according to the input saturation (x-axis).")]
	public TextureCurveParameter satVsSat = new TextureCurveParameter(new TextureCurve(new Keyframe[0], 0.5f, loop: false, new Vector2(0f, 1f)));

	[Tooltip("Adjusts saturation (y-axis) according to the input luminance (x-axis).")]
	public TextureCurveParameter lumVsSat = new TextureCurveParameter(new TextureCurve(new Keyframe[0], 0.5f, loop: false, new Vector2(0f, 1f)));

	public bool IsActive()
	{
		return true;
	}

	public bool IsTileCompatible()
	{
		return true;
	}
}
