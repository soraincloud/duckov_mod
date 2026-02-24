namespace UnityEngine.Polybrush;

internal static class BrushMirrorUtility
{
	internal static Vector3 ToVector3(this BrushMirror mirror)
	{
		bool flag = (mirror & BrushMirror.X) != 0;
		bool flag2 = (mirror & BrushMirror.Y) != 0;
		bool flag3 = (mirror & BrushMirror.Z) != 0;
		if ((mirror < BrushMirror.None) || mirror > (BrushMirror.X | BrushMirror.Y | BrushMirror.Z))
		{
			return Vector3.one;
		}
		Vector3 one = Vector3.one;
		if (flag)
		{
			one.x = -1f;
		}
		if (flag2)
		{
			one.y = -1f;
		}
		if (flag3)
		{
			one.z = -1f;
		}
		return one;
	}
}
