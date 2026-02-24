namespace UnityEngine.UIElements;

public static class BackgroundPropertyHelper
{
	public static BackgroundPosition ConvertScaleModeToBackgroundPosition(ScaleMode scaleMode = ScaleMode.StretchToFill)
	{
		return new BackgroundPosition(BackgroundPositionKeyword.Center);
	}

	public static BackgroundRepeat ConvertScaleModeToBackgroundRepeat(ScaleMode scaleMode = ScaleMode.StretchToFill)
	{
		return new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
	}

	public static BackgroundSize ConvertScaleModeToBackgroundSize(ScaleMode scaleMode = ScaleMode.StretchToFill)
	{
		return scaleMode switch
		{
			ScaleMode.ScaleAndCrop => new BackgroundSize(BackgroundSizeType.Cover), 
			ScaleMode.ScaleToFit => new BackgroundSize(BackgroundSizeType.Contain), 
			_ => new BackgroundSize(Length.Percent(100f), Length.Percent(100f)), 
		};
	}

	public static ScaleMode ResolveUnityBackgroundScaleMode(BackgroundPosition backgroundPositionX, BackgroundPosition backgroundPositionY, BackgroundRepeat backgroundRepeat, BackgroundSize backgroundSize, out bool valid)
	{
		if (backgroundPositionX == ConvertScaleModeToBackgroundPosition(ScaleMode.ScaleAndCrop) && backgroundPositionY == ConvertScaleModeToBackgroundPosition(ScaleMode.ScaleAndCrop) && backgroundRepeat == ConvertScaleModeToBackgroundRepeat(ScaleMode.ScaleAndCrop) && backgroundSize == ConvertScaleModeToBackgroundSize(ScaleMode.ScaleAndCrop))
		{
			valid = true;
			return ScaleMode.ScaleAndCrop;
		}
		if (backgroundPositionX == ConvertScaleModeToBackgroundPosition(ScaleMode.ScaleToFit) && backgroundPositionY == ConvertScaleModeToBackgroundPosition(ScaleMode.ScaleToFit) && backgroundRepeat == ConvertScaleModeToBackgroundRepeat(ScaleMode.ScaleToFit) && backgroundSize == ConvertScaleModeToBackgroundSize(ScaleMode.ScaleToFit))
		{
			valid = true;
			return ScaleMode.ScaleToFit;
		}
		if (backgroundPositionX == ConvertScaleModeToBackgroundPosition() && backgroundPositionY == ConvertScaleModeToBackgroundPosition() && backgroundRepeat == ConvertScaleModeToBackgroundRepeat() && backgroundSize == ConvertScaleModeToBackgroundSize())
		{
			valid = true;
			return ScaleMode.StretchToFill;
		}
		valid = false;
		return ScaleMode.StretchToFill;
	}
}
