namespace UnityEngine.Polybrush;

internal struct ColorMask
{
	internal bool r;

	internal bool g;

	internal bool b;

	internal bool a;

	internal ColorMask(bool r, bool g, bool b, bool a)
	{
		this.r = r;
		this.b = b;
		this.g = g;
		this.a = a;
	}
}
