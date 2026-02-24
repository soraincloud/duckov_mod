namespace UnityEngine.UIElements;

public static class RuntimePanelUtils
{
	public static Vector2 ScreenToPanel(IPanel panel, Vector2 screenPosition)
	{
		return ((BaseRuntimePanel)panel).ScreenToPanel(screenPosition);
	}

	public static Vector2 CameraTransformWorldToPanel(IPanel panel, Vector3 worldPosition, Camera camera)
	{
		Vector2 screen = camera.WorldToScreenPoint(worldPosition);
		screen.y = (float)Screen.height - screen.y;
		return ((BaseRuntimePanel)panel).ScreenToPanel(screen);
	}

	public static Rect CameraTransformWorldToPanelRect(IPanel panel, Vector3 worldPosition, Vector2 worldSize, Camera camera)
	{
		worldSize.y = 0f - worldSize.y;
		Vector2 vector = CameraTransformWorldToPanel(panel, worldPosition, camera);
		Vector3 worldPosition2 = worldPosition + camera.worldToCameraMatrix.MultiplyVector(worldSize);
		Vector2 vector2 = CameraTransformWorldToPanel(panel, worldPosition2, camera);
		return new Rect(vector, vector2 - vector);
	}

	public static void ResetDynamicAtlas(this IPanel panel)
	{
		if (panel is BaseVisualElementPanel { atlas: DynamicAtlas atlas })
		{
			atlas.Reset();
		}
	}

	public static void SetTextureDirty(this IPanel panel, Texture2D texture)
	{
		if (panel is BaseVisualElementPanel { atlas: DynamicAtlas atlas })
		{
			atlas.SetDirty(texture);
		}
	}
}
