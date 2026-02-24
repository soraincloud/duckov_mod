namespace UnityEngine.Polybrush;

internal static class DirectionUtil
{
	internal static Vector3 ToVector3(this PolyDirection dir)
	{
		return dir switch
		{
			PolyDirection.Up => Vector3.up, 
			PolyDirection.Right => Vector3.right, 
			PolyDirection.Forward => Vector3.forward, 
			_ => Vector3.zero, 
		};
	}
}
