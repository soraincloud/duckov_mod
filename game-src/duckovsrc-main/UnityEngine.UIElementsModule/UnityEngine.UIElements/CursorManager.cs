namespace UnityEngine.UIElements;

internal class CursorManager : ICursorManager
{
	public bool isCursorOverriden { get; private set; }

	public void SetCursor(Cursor cursor)
	{
		if (cursor.texture != null)
		{
			UnityEngine.Cursor.SetCursor(cursor.texture, cursor.hotspot, CursorMode.Auto);
			isCursorOverriden = true;
			return;
		}
		if (cursor.defaultCursorId != 0)
		{
			Debug.LogWarning("Runtime cursors other than the default cursor need to be defined using a texture.");
		}
		ResetCursor();
	}

	public void ResetCursor()
	{
		if (isCursorOverriden)
		{
			UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		}
		isCursorOverriden = false;
	}
}
